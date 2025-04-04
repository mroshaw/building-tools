using System.Collections.Generic;
using System.IO;
using DaftAppleGames.Darskerry.Core.Buildings;
using DaftAppleGames.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// Used by static methods to configure specific attributes of a Mesh, while being able to ignore others
    /// </summary>
    internal enum MeshProperties
    {
        CastShadows,
        StaticShadowCaster,
        ContributeGI,
        ReceiveGI,
        MotionVectors,
        DynamicOcclusion,
        RenderLayerMask,
        Priority
    }


    /// <summary>
    /// Static methods for working with Meshes
    /// </summary>
    internal static class MeshTools
    {
        #region Static properties

        static MeshTools()
        {
        }

        #endregion

        #region Tool prarameter structs

        /// <summary>
        /// Struct to consolidate parameters for use with the 'CombineMesh' tool
        /// </summary>
        internal struct CombineMeshParameters
        {
            internal string BaseAssetOutputPath;
            internal string AssetOutputFolder;
            internal string AssetFileNamePrefix;
            internal bool CreateOutputFolder;
            internal bool Is32BIT;
            internal bool GenerateSecondaryUVs;
        }

        internal struct ConfigureMeshParameters
        {
            internal ShadowCastingMode ShadowCastingMode;
            internal bool StaticShadowCaster;
            internal bool ContributeGI;
            internal ReceiveGI ReceiveGlobalGI;
            internal LightLayerMode LightLayerMode;
            internal string LayerName;
        }

        #endregion

        #region Configure Mesh methoods

        /// <summary>
        /// Validate the CombineMeshParameters
        /// </summary>
        internal static bool ValidateCombineMeshParameters(CombineMeshParameters combineMeshParameters)
        {
            // Check the base asset folder exists
            if (!Directory.Exists(combineMeshParameters.BaseAssetOutputPath))
            {
                Debug.LogError($"Combined Mesh base output folder does not exist: {combineMeshParameters.BaseAssetOutputPath}. Aborting!");
                return false;
            }

            string fullOutputPath = Path.Combine(combineMeshParameters.BaseAssetOutputPath, combineMeshParameters.AssetOutputFolder);

            if (!combineMeshParameters.CreateOutputFolder && !Directory.Exists(fullOutputPath))
            {
                Debug.LogError($"Combined Mesh output folder does not exist: {fullOutputPath}. Aborting!");
                return false;
            }

            return true;
        }

        internal static void ConfigureMeshOnGameObject(GameObject parentGameObject, ConfigureMeshParameters configureMeshParameters)
        {
            if (!parentGameObject.TryGetComponent(out MeshRenderer renderer))
            {
                Debug.LogWarning($"No MeshRenderer found on {parentGameObject.name}");
                return;
            }

            renderer.shadowCastingMode = configureMeshParameters.ShadowCastingMode;
            renderer.staticShadowCaster = configureMeshParameters.StaticShadowCaster;

            // Apply ContributeGI only if renderer is a MeshRenderer
            if (renderer is MeshRenderer meshRenderer)
            {
                meshRenderer.receiveGI = configureMeshParameters.ReceiveGlobalGI;
            }

            // Apply the Light Layer Mask using the static dictionary
            renderer.renderingLayerMask = LightTools.GetMaskByMode(configureMeshParameters.LightLayerMode);

            // Set the layer
            renderer.gameObject.layer = LayerMask.NameToLayer(configureMeshParameters.LayerName);

            // Update the static flags, based on whether ConfigureGI is true or false
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
            GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,
                configureMeshParameters.ContributeGI ? flags | StaticEditorFlags.ContributeGI : flags & ~StaticEditorFlags.ContributeGI);
        }

        #endregion

        #region Combine Mesh methods

        /// <summary>
        /// Combines all meshes in the given GameObject, writing the resulting Mesh as an asset to the given path.
        /// Any components with the 'MeshCombineExcluder' component will be ignored by the process
        /// </summary>
        internal static void CombineGameObjectMeshes(GameObject parentGameObject, CombineMeshParameters combineMeshParameters, ConfigureMeshParameters newMeshParameters)
        {
            if (!ValidateCombineMeshParameters(combineMeshParameters))
            {
                return;
            }

            string fullOutputPath = Path.Combine(combineMeshParameters.BaseAssetOutputPath, combineMeshParameters.AssetOutputFolder);

            if (combineMeshParameters.CreateOutputFolder && !Directory.Exists(fullOutputPath))
            {
                Directory.CreateDirectory(fullOutputPath);
                Debug.Log($"Created new folder for Mesh asset: {fullOutputPath}");
            }

            MeshFilter[] meshFilters = parentGameObject.GetComponentsInChildren<MeshFilter>(true);
            Vector3 originalPosition = parentGameObject.transform.position;
            Quaternion originalRotation = parentGameObject.transform.rotation;

            parentGameObject.transform.position = Vector3.zero;
            parentGameObject.transform.rotation = Quaternion.identity;

            Dictionary<Material, List<MeshFilter>> materialToMeshFilterList = new();
            List<GameObject> combinedObjects = new();

            foreach (MeshFilter meshFilter in meshFilters)
            {
                MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    Debug.LogWarning("The Mesh Filter on object " + meshFilter.name + " has no Mesh Renderer component attached. Skipping.");
                    continue;
                }

                Material[] materials = meshRenderer.sharedMaterials;
                if (materials == null)
                {
                    Debug.LogWarning("The Mesh Renderer on object " + meshFilter.name + " has no material assigned. Skipping.");
                    continue;
                }

                if (meshFilter.gameObject.HasComponent<MeshCombineExcluder>())
                {
                    Debug.LogWarning("The object " + meshFilter.name + " has a MeshCombineExcluder. Skipping.");
                    continue;
                }

                if (materials.Length > 1)
                {
                    // Rollback: return the object to original position
                    parentGameObject.transform.position = originalPosition;
                    parentGameObject.transform.rotation = originalRotation;
                    Debug.LogError(
                        "Objects with multiple materials on the same mesh are not supported. Create multiple meshes from this object's sub-meshes in an external 3D tool and assign separate materials to each. Aborted!");
                    return;
                }

                Material material = materials[0];

                // Add material to mesh filter mapping to dictionary
                if (materialToMeshFilterList.ContainsKey(material))
                {
                    materialToMeshFilterList[material].Add(meshFilter);
                }
                else
                {
                    materialToMeshFilterList.Add(material, new List<MeshFilter>() { meshFilter });
                }

                // Disable the MeshRenderer
                if (meshFilter.TryGetComponent<Renderer>(out Renderer renderer))
                {
                    renderer.enabled = false;
                }
            }

            // For each material, create a new merged object, in the scene and in the assets.
            foreach (KeyValuePair<Material, List<MeshFilter>> entry in materialToMeshFilterList)
            {
                List<MeshFilter> meshesWithSameMaterial = entry.Value;
                // Create a convenient material name
                string materialName = entry.Key.ToString().Split(' ')[0];

                CombineInstance[] combine = new CombineInstance[meshesWithSameMaterial.Count];
                for (int i = 0; i < meshesWithSameMaterial.Count; i++)
                {
                    combine[i].mesh = meshesWithSameMaterial[i].sharedMesh;
                    combine[i].transform = meshesWithSameMaterial[i].transform.localToWorldMatrix;
                }

                // Create a new mesh using the combined properties
                IndexFormat format = combineMeshParameters.Is32BIT ? IndexFormat.UInt32 : IndexFormat.UInt16;
                Mesh combinedMesh = new() { indexFormat = format };
                combinedMesh.CombineMeshes(combine);

                if (combineMeshParameters.GenerateSecondaryUVs)
                {
                    bool secondaryUVsResult = Unwrapping.GenerateSecondaryUVSet(combinedMesh);
                    if (!secondaryUVsResult)
                    {
                        Debug.LogWarning(
                            "Could not generate secondary UVs.");
                    }
                }

                // Create asset
                materialName += "_" + combinedMesh.GetInstanceID();
                AssetDatabase.CreateAsset(combinedMesh, Path.Combine(fullOutputPath, $"{combineMeshParameters.AssetFileNamePrefix}CombinedMeshes_{materialName}.asset"));

                // Create game object
                string combinedMeshGoName = materialToMeshFilterList.Count > 1 ? "CombinedMeshes_" + materialName : "CombinedMeshes_" + parentGameObject.name;
                GameObject combinedMeshGameObject = new(combinedMeshGoName);
                MeshFilter filter = combinedMeshGameObject.AddComponent<MeshFilter>();
                filter.sharedMesh = combinedMesh;
                MeshRenderer renderer = combinedMeshGameObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = entry.Key;
                combinedObjects.Add(combinedMeshGameObject);

                // Configure the new Mesh Renderer
                ConfigureMeshOnGameObject(combinedMeshGameObject, newMeshParameters);
            }

            // If there was more than one material, and thus multiple GOs created, parent them and work with result
            GameObject resultGO = null;
            if (combinedObjects.Count > 1)
            {
                resultGO = new GameObject("CombinedMeshes_" + parentGameObject.name);
                foreach (GameObject combinedObject in combinedObjects) combinedObject.transform.parent = resultGO.transform;
            }
            else
            {
                resultGO = combinedObjects[0];
            }

            // Create prefab
            string prefabPath = Path.Combine(fullOutputPath, resultGO.name + ".prefab");
            PrefabUtility.SaveAsPrefabAssetAndConnect(resultGO, prefabPath, InteractionMode.UserAction);

            // Return both to original positions
            parentGameObject.transform.position = originalPosition;
            parentGameObject.transform.rotation = originalRotation;
            if (parentGameObject.transform.parent != null)
            {
                resultGO.transform.SetParent(parentGameObject.transform.parent, false);
                resultGO.transform.position = originalPosition;
                resultGO.transform.rotation = originalRotation;
            }
        }

        #endregion

        #region Volume helper methods

        /// <summary>
        /// Returns the size of a box that encloses the meshes in the Game Object
        /// </summary>
        internal static void GetMeshSize(GameObject parentGameObject, LayerMask includeLayerMask, string[] ignoreNames, out Vector3 meshSize, out Vector3 meshCenter)
        {
            Bounds meshBounds = GetMeshBounds(parentGameObject, includeLayerMask, ignoreNames);
            meshSize = meshBounds.size;
            meshCenter = meshBounds.center;
        }

        /// <summary>
        /// Return the bounds of the enclosing meshes in the Game Object
        /// </summary>
        internal static Bounds GetMeshBounds(GameObject parentGameObject, LayerMask includeLayerMask, string[] ignoreNames)
        {
            Bounds combinedBounds = new(Vector3.zero, Vector3.zero);
            bool hasValidRenderer = false;

            foreach (MeshRenderer childRenderer in parentGameObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                if ((includeLayerMask & (1 << childRenderer.gameObject.layer)) == 0 ||
                    (ignoreNames.Length != 0 && ignoreNames.ItemInString(childRenderer.gameObject.name)))
                {
                    continue;
                }

                Bounds meshBounds = childRenderer.localBounds;

                // Initialize or expand the combined bounds
                if (!hasValidRenderer)
                {
                    combinedBounds = meshBounds;
                    hasValidRenderer = true;
                }
                else
                {
                    combinedBounds.Encapsulate(meshBounds);
                }
            }

            return combinedBounds;
        }

        #endregion
    }
}