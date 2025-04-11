using System.Collections.Generic;
using System.IO;
using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "OptimiseMeshesEditorTool", menuName = "Daft Apple Games/Building Tools/Optimise Meshes Tool")]
    internal class OptimiseMeshesEditorTool : BuildingEditorTool
    {
        protected override string GetToolName()
        {
            return "Optimise Meshes";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation();
        }

        private static bool ValidateCombineMeshParameters(CombineMeshParameters combineMeshParameters)
        {
            // Check the base asset folder exists
            if (!Directory.Exists(combineMeshParameters.BaseAssetOutputPath))
            {
                Debug.LogError($"Combined Mesh base output folder does not exist: {combineMeshParameters.BaseAssetOutputPath}. Aborting!");
                return false;
            }

            string fullOutputPath = Path.Combine(combineMeshParameters.BaseAssetOutputPath, combineMeshParameters.AssetOutputFolder);

            if (combineMeshParameters.CreateOutputFolder || Directory.Exists(fullOutputPath))
            {
                return true;
            }

            Debug.LogError($"Combined Mesh output folder does not exist: {fullOutputPath}. Aborting!");
            return false;
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, string undoGroupName)
        {
            if (editorSettings is BuildingWizardEditorSettings buildingEditorSettings)
            {
                OptimiseMeshes(selectedGameObject, buildingEditorSettings);
            }
        }

        #region Static Mesh Optimisation methods

        /// <summary>
        /// Struct to consolidate parameters for use with the 'CombineMesh' tool
        /// </summary>
        private struct CombineMeshParameters
        {
            internal string BaseAssetOutputPath;
            internal string AssetOutputFolder;
            internal string AssetFileNamePrefix;
            internal bool CreateOutputFolder;
            internal bool Is32BIT;
            internal bool GenerateSecondaryUVs;
        }

        private static void OptimiseMeshes(GameObject parentGameObject, BuildingWizardEditorSettings buildingWizardSettings)
        {
            Building building = parentGameObject.GetComponent<Building>();

            // Combine the exterior Prop meshes
            CombineMeshParameters combineMeshParameters = new()
            {
                BaseAssetOutputPath = buildingWizardSettings.meshAssetOutputPath,
                AssetOutputFolder = parentGameObject.name,
                CreateOutputFolder = true
            };

            // Merge exterior meshes
            OptimiseMeshGroup(building.exteriorProps, "exteriorProps", combineMeshParameters, buildingWizardSettings.buildingMeshSettings.exteriorPropMeshSettings);
            OptimiseMeshGroup(building.exteriors, "exteriors", combineMeshParameters, buildingWizardSettings.buildingMeshSettings.exteriorMeshSettings);

            // Merge interior meshes
            OptimiseMeshGroup(building.interiorProps, "interiorProps", combineMeshParameters, buildingWizardSettings.buildingMeshSettings.interiorPropMeshSettings);
            OptimiseMeshGroup(building.interiors, "interiors", combineMeshParameters, buildingWizardSettings.buildingMeshSettings.interiorMeshSettings);
        }

        private static void OptimiseMeshGroup(GameObject[] allGameObjects, string namePrefix, CombineMeshParameters combineMeshParameters,
            MeshEditorPresetSettings newMeshParameters)
        {
            foreach (GameObject gameObjectParent in allGameObjects)
            {
                combineMeshParameters.AssetFileNamePrefix = namePrefix;
                CombineGameObjectMeshes(gameObjectParent, combineMeshParameters, newMeshParameters);
            }
        }

        /// <summary>
        /// Combines all meshes in the given GameObject, writing the resulting Mesh as an asset to the given path.
        /// Any components with the 'MeshCombineExcluder' component will be ignored by the process
        /// </summary>
        private static void CombineGameObjectMeshes(GameObject parentGameObject, CombineMeshParameters combineMeshParameters, MeshEditorPresetSettings newMeshParameters)
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
                    materialToMeshFilterList.Add(material, new List<MeshFilter> { meshFilter });
                }

                // Disable the MeshRenderer
                if (meshFilter.TryGetComponent(out Renderer renderer))
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
                newMeshParameters.ConfigureMeshOnGameObject(combinedMeshGameObject, log);
            }

            // If there was more than one material, and thus multiple GOs created, parent them and work with result
            GameObject resultGameObject;
            if (combinedObjects.Count > 1)
            {
                resultGameObject = new GameObject("CombinedMeshes_" + parentGameObject.name);
                foreach (GameObject combinedObject in combinedObjects) combinedObject.transform.parent = resultGameObject.transform;
            }
            else
            {
                resultGameObject = combinedObjects[0];
            }

            // Create prefab
            string prefabPath = Path.Combine(fullOutputPath, resultGameObject.name + ".prefab");
            PrefabUtility.SaveAsPrefabAssetAndConnect(resultGameObject, prefabPath, InteractionMode.UserAction);

            // Return both to original positions
            parentGameObject.transform.position = originalPosition;
            parentGameObject.transform.rotation = originalRotation;
            if (parentGameObject.transform.parent == null)
            {
                return;
            }

            resultGameObject.transform.SetParent(parentGameObject.transform.parent, false);
            resultGameObject.transform.position = originalPosition;
            resultGameObject.transform.rotation = originalRotation;
        }

        #endregion
    }
}