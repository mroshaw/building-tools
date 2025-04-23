using System.Collections.Generic;
using System.IO;
using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "OptimiseMeshesEditorTool", menuName = "Daft Apple Games/Building Tools/Optimise Meshes Tool")]
    internal class OptimiseMeshesEditorTool : BuildingEditorTool
    {
        [BoxGroup("Settings")] [SerializeField] internal string assetOutputFolder;
        [BoxGroup("Settings")] [SerializeField] internal string assetFileNamePrefix;
        [BoxGroup("Settings")] [SerializeField] internal bool createOutputFolder;
        [BoxGroup("Settings")] [SerializeField] internal bool is32BIT;
        [BoxGroup("Settings")] [SerializeField] internal bool generateSecondaryUVs;
        [BoxGroup("Settings")] [SerializeField] internal ApplyMeshPresetsEditorTool combinedMeshPresets;

        private string OutputAbsolutePath => Path.Combine(Application.dataPath, assetOutputFolder);
        private string OutputRelativePath => Path.Combine("Assets", assetOutputFolder);

        protected override string GetToolName()
        {
            return "Optimise Meshes";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, out List<string> cannotRunReasons)
        {
            bool canRun = true;

            cannotRunReasons = new List<string>();
            if (!RequireGameObjectValidation(out string requireGameObjectReason))
            {
                cannotRunReasons.Add(requireGameObjectReason);
                canRun = false;
            }

            if (!RequiredBuildingValidation(out string requiredBuildingReason))
            {
                cannotRunReasons.Add(requiredBuildingReason);
                canRun = false;
            }

            if (!ValidateToolSettings(out string toolValidationReason))
            {
                cannotRunReasons.Add(toolValidationReason);
                canRun = false;
            }

            return canRun;
        }

        private bool ValidateToolSettings(out string validationReason)
        {
            // Check the base asset folder exists
            if (!Directory.Exists(OutputAbsolutePath) && !createOutputFolder)
            {
                validationReason = $"Combined Mesh base output folder does not exist and 'createOutputFolder' is not ticked: {OutputAbsolutePath}!";
                return false;
            }

            validationReason = string.Empty;
            return true;
        }

        protected override void RunTool(GameObject selectedGameObject, string undoGroupName)
        {
            // Check the base asset folder exists
            if (!Directory.Exists(OutputAbsolutePath) && createOutputFolder)
            {
                Directory.CreateDirectory(OutputAbsolutePath);
            }

            OptimiseMeshes(selectedGameObject);
        }

        private void OptimiseMeshes(GameObject parentGameObject)
        {
            Building building = parentGameObject.GetComponent<Building>();

            // Create a folder for this GameObjects meshes
            string gameObjectAbsolutePath = Path.Combine(OutputAbsolutePath, $"{parentGameObject.name}");
            string gameObjectRelativePath = Path.Combine(OutputRelativePath, $"{parentGameObject.name}");
            if (!Directory.Exists(gameObjectAbsolutePath))
            {
                log.AddToLog(LogLevel.Debug, $"Creating folder: {gameObjectAbsolutePath}");
                Directory.CreateDirectory(gameObjectAbsolutePath);
            }

            // Merge building meshes
            CombineMeshLayer(parentGameObject, combinedMeshPresets.exteriorMeshSettings.layerName, gameObjectAbsolutePath, gameObjectRelativePath);
            CombineMeshLayer(parentGameObject, combinedMeshPresets.interiorMeshSettings.layerName, gameObjectAbsolutePath, gameObjectRelativePath);

            // Merge prop meshes
            CombineMeshLayer(parentGameObject, combinedMeshPresets.exteriorPropMeshSettings.layerName, gameObjectAbsolutePath, gameObjectRelativePath);
            CombineMeshLayer(parentGameObject, combinedMeshPresets.interiorPropMeshSettings.layerName, gameObjectAbsolutePath, gameObjectRelativePath);
        }

        /// <summary>
        /// Combines all meshes in the given GameObject, writing the resulting Mesh as an asset to the given path.
        /// Any components with the 'MeshCombineExcluder' component will be ignored by the process
        /// </summary>
        private void CombineMeshLayer(GameObject parentGameObject, string layerName, string gameObjectAbsolutePath, string gameObjectRelativePath)
        {
            // Derive a unique name for our combined assets
            string newMeshName = $"{assetFileNamePrefix}_{parentGameObject.name}_{layerName}_Mesh";
            string newPrefabAssetName = $"{assetFileNamePrefix}_{parentGameObject.name}_{layerName}_Prefab";

            // Create a child folder for this containers meshes
            string instanceAbsolutePath = Path.Combine(gameObjectAbsolutePath, $"{parentGameObject.name}_{layerName}");
            string instanceRelativePath = Path.Combine(gameObjectRelativePath, $"{parentGameObject.name}_{layerName}");
            if (!Directory.Exists(instanceAbsolutePath))
            {
                log.AddToLog(LogLevel.Debug, $"Creating folder: {instanceAbsolutePath}");
                Directory.CreateDirectory(instanceAbsolutePath);
            }

            log.AddToLog(LogLevel.Debug, $"Absolute destination folder: {instanceAbsolutePath}");
            log.AddToLog(LogLevel.Debug, $"Relative destination folder: {instanceRelativePath}");

            MeshFilter[] meshFilters = parentGameObject.GetComponentsInChildren<MeshFilter>(true);
            Vector3 originalPosition = parentGameObject.transform.position;
            Quaternion originalRotation = parentGameObject.transform.rotation;

            // Move the GameObject to zero, otherwise mesh positions go wonky
            parentGameObject.transform.position = Vector3.zero;
            parentGameObject.transform.rotation = Quaternion.identity;

            Dictionary<Material, List<MeshFilter>> materialToMeshFilterList = new();
            List<GameObject> combinedObjects = new();

            foreach (MeshFilter meshFilter in meshFilters)
            {
                // If the MeshFilter isn't on our layer, skip it
                if (LayerMask.LayerToName(meshFilter.gameObject.layer) != layerName)
                {
                    continue;
                }

                MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    log.AddToLog(LogLevel.Debug, "The Mesh Filter on object " + meshFilter.name + " has no Mesh Renderer component attached. Skipping.");
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
                IndexFormat format = is32BIT ? IndexFormat.UInt32 : IndexFormat.UInt16;
                Mesh combinedMesh = new() { indexFormat = format };
                combinedMesh.CombineMeshes(combine);

                if (generateSecondaryUVs)
                {
                    bool secondaryUVsResult = Unwrapping.GenerateSecondaryUVSet(combinedMesh);
                    if (!secondaryUVsResult)
                    {
                        log.AddToLog(LogLevel.Warning,
                            "Could not generate secondary UVs.");
                    }
                }

                // Create asset
                materialName += "_" + combinedMesh.GetInstanceID();
                string meshAssetPath = Path.Combine(instanceRelativePath, $"{newMeshName}_{materialName}.asset");
                log.AddToLog(LogLevel.Debug, $"Saving mesh asset to: {meshAssetPath}");
                AssetDatabase.CreateAsset(combinedMesh, meshAssetPath);

                // Create game object
                string combinedMeshGoName = materialToMeshFilterList.Count > 1 ? $"{newMeshName}_{materialName}" : $"{newMeshName}";
                GameObject combinedMeshGameObject = new(combinedMeshGoName);
                MeshFilter filter = combinedMeshGameObject.AddComponent<MeshFilter>();
                filter.sharedMesh = combinedMesh;
                log.AddToLog(LogLevel.Debug, $"Set mesh on {filter.name} to {combinedMesh.name}");
                MeshRenderer renderer = combinedMeshGameObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = entry.Key;
                combinedObjects.Add(combinedMeshGameObject);
            }

            // If there was more than one material parent them and work with result
            GameObject resultGameObject;
            if (combinedObjects.Count > 1)
            {
                resultGameObject = new GameObject(newPrefabAssetName);
                foreach (GameObject combinedObject in combinedObjects) combinedObject.transform.parent = resultGameObject.transform;
            }
            else
            {
                resultGameObject = combinedObjects[0];
            }

            // Create prefab
            string prefabPath = Path.Combine(instanceRelativePath, resultGameObject.name + ".prefab");
            log.AddToLog(LogLevel.Debug, $"Saving prefab asset to: {prefabPath}");
            PrefabUtility.SaveAsPrefabAssetAndConnect(resultGameObject, prefabPath, InteractionMode.UserAction);

            // Return both to original positions
            parentGameObject.transform.position = originalPosition;
            parentGameObject.transform.rotation = originalRotation;
            log.AddToLog(LogLevel.Debug, $"Setting parent of {resultGameObject.name} to {parentGameObject.name}");
            resultGameObject.transform.SetParent(parentGameObject.transform, false);
            resultGameObject.transform.position = originalPosition;
            resultGameObject.transform.rotation = originalRotation;
        }
    }
}