using System.Collections.Generic;
using System.IO;
using System.Linq;
using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Editor.Extensions;
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
        [BoxGroup("Settings")] [SerializeField]
        [Tooltip(
            "If checked, meshes will be combined separately for each of the BuildngExterior, BuildingInterior, ExteriorProps and InteriorProps layers. If unchecked, the whole building will be merged as one.")]
        internal bool combineMeshesByLayer = true;

        [BoxGroup("Settings")] [SerializeField] [Tooltip("Folder in the \"Assets\" folder where combined meshes and prefabs will be saved.")] internal string assetOutputFolder;
        [BoxGroup("Settings")] [SerializeField] [Tooltip("Prefix that will be added to each generated combined mesh and prefab.")] internal string assetFileNamePrefix;

        [BoxGroup("Settings")] [SerializeField]
        [Tooltip("If the Asset Output Folder does not exist, checking this will cause the tool to create it.")] internal bool createOutputFolder;

        [BoxGroup("Settings")] [SerializeField]
        [Tooltip(
            "If checked, forces the created mesh to use a 32-bit index format. Otherwise, it will use a 16-bit index format. See Mesh.indexFormat for more details.")]
        internal bool is32BIT;

        [BoxGroup("Settings")] [SerializeField] [Tooltip("Forces the tool to generate the UVs for the new combined meshes.")] internal bool generateSecondaryUVs;

        [InlineEditor] [BoxGroup("Settings")]
        [Tooltip(
            "This is an instance of the Apply Mesh Presets Tool, and is used to apply pre-set parameters to the newly created merged meshes. Note that in the case of a \"whole building\" merge (if \"Combine Meshes By Layer\" is unchecked), then the resulting meshing will have the \"Building Exterior\" mesh pre-sets applied.")]
        [SerializeField] internal ApplyMeshPresetsEditorTool combinedMeshPresets;

        // Pre-calculate some folder paths (absolute and relative to /Asset) to make it easier to save assets
        private string OutputAbsolutePath => Path.Combine(Application.dataPath, assetOutputFolder);
        private string OutputRelativePath => Path.Combine("Assets", assetOutputFolder);
        private string GameObjectAbsolutePath => SelectedGameObject == null ? string.Empty : Path.Combine(OutputAbsolutePath, $"{SelectedGameObject.name}");
        private string GameObjectRelativePath => SelectedGameObject == null ? string.Empty : Path.Combine(OutputRelativePath, $"{SelectedGameObject.name}");

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        private MeshCombineRollBack _meshCombineRollBack;

        protected override string GetToolName()
        {
            return "Optimise Meshes";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(out List<string> cannotRunReasons)
        {
            bool canRun = true;
            cannotRunReasons = new List<string>();

            if (!DoOutputPathsExist(out string toolValidationReason))
            {
                cannotRunReasons.Add(toolValidationReason);
                canRun = false;
            }

            if (!RequireGameObjectValidation(out string requireGameObjectReason))
            {
                cannotRunReasons.Add(requireGameObjectReason);
                return false;
            }

            if (IsAlreadyOptimised(out string alreadyOptimisedReason))
            {
                cannotRunReasons.Add(alreadyOptimisedReason);
                return false;
            }

            if (!HasRequiredBuildingComponent(out string requiredBuildingReason))
            {
                cannotRunReasons.Add(requiredBuildingReason);
                return false;
            }

            if (!HasMeshExteriorLayerConfigured(out string layerFailedReason))
            {
                cannotRunReasons.Add(layerFailedReason);
                canRun = false;
            }

            return canRun;
        }

        private bool DoOutputPathsExist(out string validationReason)
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

        /// <summary>
        /// Check to see if already optimised
        /// </summary>
        private bool IsAlreadyOptimised(out string validationReason)
        {
            if (SelectedGameObject && SelectedGameObject.TryGetComponent(out MeshCombineRollBack meshCombineRollBack) && meshCombineRollBack.isOptimised)
            {
                validationReason = "Optimisation has already been run against this Game Object. Please rollback the changes, if you want to re-run this tool!";
                return true;
            }

            validationReason = string.Empty;
            return false;
        }

        protected override void RunTool(string undoGroupName)
        {
            // Add the MeshCombineRollBack component, if it doesn't exist
            _meshCombineRollBack = SelectedGameObject.EnsureComponent<MeshCombineRollBack>();
            _meshCombineRollBack.ClearAudit();

            // Check the base asset folder exists
            if (!Directory.Exists(OutputAbsolutePath) && createOutputFolder)
            {
                Directory.CreateDirectory(OutputAbsolutePath);
            }

            OptimiseMeshes();
        }

        private void OptimiseMeshes()
        {
            // Create the GameObject folder, if it doesn't exist
            DirectoryExtensions.CreateFolderIfNotExists(GameObjectAbsolutePath);

            // Zero the Game Object
            SetPositionToZero();

            // Combine the Meshes
            if (combineMeshesByLayer)
            {
                CombineMeshesByLayer();
            }
            else
            {
                CombineAllMeshes();
            }

            // Reset the Game Object position
            ResetPosition();

            // Update the RollBack component
            _meshCombineRollBack.SetPaths(GameObjectAbsolutePath, GameObjectRelativePath);
            _meshCombineRollBack.isOptimised = true;
        }

        /// <summary>
        /// Sets the GameObject position to ZERO, to prevent weirdness when meshes are merged
        /// </summary>
        private void SetPositionToZero()
        {
            _originalPosition = SelectedGameObject.transform.position;
            _originalRotation = SelectedGameObject.transform.rotation;

            SelectedGameObject.transform.position = Vector3.zero;
            SelectedGameObject.transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Restores the GameObject position to it's original
        /// </summary>
        private void ResetPosition()
        {
            SelectedGameObject.transform.position = _originalPosition;
            SelectedGameObject.transform.rotation = _originalRotation;
        }

        private void CombineMeshesByLayer()
        {
            // Merge building meshes
            CombineMeshesInLayer(combinedMeshPresets.exteriorMeshSettings);
            CombineMeshesInLayer(combinedMeshPresets.interiorMeshSettings);

            // Merge prop meshes
            CombineMeshesInLayer(combinedMeshPresets.exteriorPropMeshSettings);
            CombineMeshesInLayer(combinedMeshPresets.interiorPropMeshSettings);
        }

        private void CombineAllMeshes()
        {
            // Combine everything in one go
            CombineAllMeshesInGameObject();
        }

        /// <summary>
        /// Returns all MeshFilters on the Game Object
        /// </summary>
        private List<MeshFilter> GetAllMeshFiltersInGameObject()
        {
            return SelectedGameObject.GetComponentsInChildren<MeshFilter>(true).ToList();
        }

        /// <summary>
        /// Returns all MeshFilters in the given layer
        /// </summary>
        private List<MeshFilter> GetMeshFiltersInLayer(string layerName)
        {
            List<MeshFilter> meshFiltersInLayer = new();
            MeshFilter[] allMeshFilters = SelectedGameObject.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter currentMeshFilter in allMeshFilters)
            {
                if (LayerMask.LayerToName(currentMeshFilter.gameObject.layer) == layerName)
                {
                    meshFiltersInLayer.Add(currentMeshFilter);
                }
            }

            return meshFiltersInLayer;
        }

        /// <summary>
        /// Combines all meshes in the given GameObject, writing the resulting Mesh as an asset to the given path.
        /// Any components with the 'MeshCombineExcluder' component will be ignored by the process
        /// </summary>
        private void CombineMeshesInLayer(MeshEditorPresetSettings meshPresets)
        {
            List<MeshFilter> allMeshFiltersInLayer = GetMeshFiltersInLayer(meshPresets.layerName);
            if (allMeshFiltersInLayer.Count == 0)
            {
                log.AddToLog(LogLevel.Debug, $"No MeshFilters found in layer: {meshPresets.layerName}, so skipping...");
                return;
            }

            // Create a child folder for this containers meshes, if it does not exist
            string folderName = $"{SelectedGameObject.name}_{meshPresets.layerName}";
            DirectoryExtensions.CreateFolderIfNotExists(folderName);

            // Derive a unique name for our combined assets
            string newMeshName = $"{assetFileNamePrefix}_{SelectedGameObject.name}_{meshPresets.layerName}";

            // Combine the Meshes
            CombineMeshes(allMeshFiltersInLayer, newMeshName, folderName, meshPresets);
        }

        /// <summary>
        /// Does the same as above but combines ALL meshes in the GameObject into as small a number of meshes as possible
        /// </summary>
        private void CombineAllMeshesInGameObject()
        {
            List<MeshFilter> allMeshFilters = GetAllMeshFiltersInGameObject();
            if (allMeshFilters.Count == 0)
            {
                log.AddToLog(LogLevel.Debug, $"No MeshFilters found in GameObject: {SelectedGameObject.name}, so skipping...");
                return;
            }

            string newMeshName = $"{assetFileNamePrefix}_{SelectedGameObject.name}_Mesh";
            string folderName = $"{SelectedGameObject.name}";

            // Combine all meshes, then apply the 'Exterior' properties
            CombineMeshes(allMeshFilters, newMeshName, folderName, combinedMeshPresets.exteriorMeshSettings);
        }

        /// <summary>
        /// Combines the provided Meshes into as few meshes as possible, determined by the number of materials used
        /// </summary>
        private void CombineMeshes(List<MeshFilter> meshesToCombine, string newMeshName, string folderName, MeshEditorPresetSettings meshPresets)
        {
            string instanceAbsolutePath = Path.Combine(GameObjectAbsolutePath, folderName);
            string instanceRelativePath = Path.Combine(GameObjectRelativePath, folderName);

            if (!Directory.Exists(instanceAbsolutePath))
            {
                log.AddToLog(LogLevel.Debug, $"Creating folder: {instanceAbsolutePath}");
                Directory.CreateDirectory(instanceAbsolutePath);
            }

            log.AddToLog(LogLevel.Debug, $"Absolute destination folder: {instanceAbsolutePath}");
            log.AddToLog(LogLevel.Debug, $"Relative destination folder: {instanceRelativePath}");

            Dictionary<Material, List<MeshFilter>> materialToMeshFilterList = new();
            List<GameObject> combinedObjects = new();

            foreach (MeshFilter meshFilter in meshesToCombine)
            {
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

                if (meshFilter.gameObject.HasComponent<DynamicMeshRenderer>())
                {
                    Debug.LogWarning("The object " + meshFilter.name + " has a MeshCombineExcluder. Skipping.");
                    continue;
                }

                if (materials.Length > 1)
                {
                    log.AddToLog(LogLevel.Error,
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
                    _meshCombineRollBack.AddRenderer(renderer);
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

                // Create asset, using the Material Name to generate a unique name
                materialName += "_" + combinedMesh.GetInstanceID();
                string meshAssetPath = Path.Combine(instanceRelativePath, $"{newMeshName}_{materialName}.asset");
                log.AddToLog(LogLevel.Debug, $"Saving mesh asset to: {meshAssetPath}");
                AssetDatabase.CreateAsset(combinedMesh, meshAssetPath);

                // Create a new game object
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
                resultGameObject = new GameObject(newMeshName);
                foreach (GameObject combinedObject in combinedObjects)
                {
                    combinedObject.transform.parent = resultGameObject.transform;
                }
            }
            else
            {
                resultGameObject = combinedObjects[0];
            }

            // Parent the result
            resultGameObject.transform.SetParent(SelectedGameObject.transform);

            // Update the RollBack component
            _meshCombineRollBack.AddResultGameObject(resultGameObject);

            // Apply Mesh Presets
            meshPresets.ConfigureMeshOnGameObject(resultGameObject, log);

            // Create prefab
            string prefabPath = Path.Combine(instanceRelativePath, resultGameObject.name + ".prefab");
            log.AddToLog(LogLevel.Debug, $"Saving prefab asset to: {prefabPath}");
            PrefabUtility.SaveAsPrefabAssetAndConnect(resultGameObject, prefabPath, InteractionMode.UserAction);
        }
    }
}