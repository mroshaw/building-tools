using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "ConfigureMeshesEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Meshes Tool")]
    internal class ConfigureMeshesEditorTool : BuildingEditorTool
    {
        private bool _configureMeshLayersOption;
        private bool _setStaticFlagsOption;

        protected override string GetToolName()
        {
            return "Configure Meshes";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation() &&
                   ValidateLayerSetup(selectedGameObject) && ValidateBuildingSetup(selectedGameObject);
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, string undoGroupName)
        {
            Log.Log(LogLevel.Info, $"Running ConfigureMeshesEditorTool. Configure Layers is {_configureMeshLayersOption}, Set Static Flags is {_setStaticFlagsOption}");
            if (editorSettings is BuildingWizardEditorSettings buildingEditorSettings)
            {
                if (_configureMeshLayersOption)
                {
                    Log.Log(LogLevel.Info, "Configuring layers...");
                    ConfigureLayers(selectedGameObject, buildingEditorSettings, Log);
                    Log.Log(LogLevel.Info, "Done!");
                }

                if (_setStaticFlagsOption)
                {
                    Log.Log(LogLevel.Info, "Configuring static flags...");
                    ConfigureStaticFlags(selectedGameObject, buildingEditorSettings, Log);
                    Log.Log(LogLevel.Info, "Done!");
                }
            }

            Log.Log(LogLevel.Info, "Done");
        }

        private bool ValidateBuildingSetup(GameObject selectedGameObject)
        {
            Building building = selectedGameObject.GetComponent<Building>();
            if (building.interiors != null && building.interiors.Length != 0 &&
                building.exteriors != null && building.exteriors.Length != 0)
            {
                return true;
            }

            Log.Log(LogLevel.Error, "Please set the Interiors and Exteriors references in the Building component. These are required by the Mesh config process.");
            return false;
        }

        /// <summary>
        /// Checks to see if it's possible to configure the layers as we want them
        /// </summary>
        private bool ValidateLayerSetup(GameObject selectedGameObject)
        {
            // If not a prefab or prefab instance, then we're good
            if (!PrefabUtility.IsPartOfAnyPrefab(selectedGameObject))
            {
                return true;
            }

            if (!ArePropsInMainBuildingStructure(selectedGameObject))
            {
                return true;
            }

            Log.Log(LogLevel.Error,
                "The selected GameObject is a prefab or prefab instance, and it's props GameObjects are children of the main building structure. Please amend the prefab and re-parent the props outside of the building structure.");
            return false;
        }

        /// <summary>
        /// Add bindings for custom tool options
        /// </summary>
        protected override void AddCustomBindings()
        {
            _configureMeshLayersOption = BindToToggleOption("ConfigureMeshLayersToggle", SetConfigureMeshLayersOption);
            _setStaticFlagsOption = BindToToggleOption("SetStaticFlagsToggle", SetStaticFlagOption);
        }

        private void SetConfigureMeshLayersOption(ChangeEvent<bool> changeEvent)
        {
            _configureMeshLayersOption = changeEvent.newValue;
        }

        private void SetStaticFlagOption(ChangeEvent<bool> changeEvent)
        {
            _setStaticFlagsOption = changeEvent.newValue;
        }

        #region Static Mesh config methods

        /// <summary>
        /// Applies layers, static and lighting properties to meshes
        /// </summary>
        private static void ConfigureMeshes(GameObject parentGameObject, BuildingWizardEditorSettings buildingWizardSettings, EditorLog log)
        {
            log.Log(LogLevel.Info, "Configuring layers...");
            ConfigureLayers(parentGameObject, buildingWizardSettings, log);
            log.Log(LogLevel.Info, "Configuring static flags...");
            ConfigureStaticFlags(parentGameObject, buildingWizardSettings, log);
        }

        /// <summary>
        /// Sets the static flags on all child mesh renderers
        /// </summary>
        private static void ConfigureStaticFlags(GameObject parentGameObject, BuildingWizardEditorSettings buildingWizardSettings, EditorLog log)
        {
            foreach (MeshRenderer meshRenderer in parentGameObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                log.Log(LogLevel.Debug, $"Setting static flags on {meshRenderer.gameObject.name} to {buildingWizardSettings.staticMeshFlags}");
                GameObjectUtility.SetStaticEditorFlags(meshRenderer.gameObject, buildingWizardSettings.staticMeshFlags);
            }
        }

        /// <summary>
        /// Configure the Building layers
        /// </summary>
        private static void ConfigureLayers(GameObject parentGameObject, BuildingWizardEditorSettings buildingWizardSettings, EditorLog log)
        {
            Building building = parentGameObject.GetComponent<Building>();

            ConfigureLayersOnGameObjects(building.exteriors, buildingWizardSettings.buildingExteriorLayer, log);
            ConfigureLayersOnGameObjects(building.interiors, buildingWizardSettings.buildingInteriorLayer, log);
            ConfigureLayersOnGameObjects(building.interiorProps, buildingWizardSettings.interiorPropsLayer, log);
            ConfigureLayersOnGameObjects(building.exteriorProps, buildingWizardSettings.exteriorPropsLayer, log);

            // If props are within the building interior/exterior, move them up a level
            MovePropsToParent(parentGameObject, log);
        }


        private static void ConfigureLayersOnGameObjects(GameObject[] gameObjects, string layerName, EditorLog log)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                SetLayerInChildren(gameObject, layerName, log);
            }
        }

        private static void MovePropsToParent(GameObject buildingGameObject, EditorLog log)
        {
            Building building = buildingGameObject.GetComponent<Building>();
            MoveAndRenameProps(building.interiorProps, building.interiors, "InteriorProps", log);
            MoveAndRenameProps(building.exteriorProps, building.exteriors, "ExteriorProps", log);
        }

        private static void MoveAndRenameProps(GameObject[] props, GameObject[] buildingMeshes, string newName, EditorLog log)
        {
            foreach (GameObject prop in props)
            {
                if (!prop)
                {
                    continue;
                }

                if (!prop.IsParentedByAny(buildingMeshes, out GameObject parentGameObject))
                {
                    continue;
                }

                log.Log(LogLevel.Debug, $"Moving Props GameObject {prop.name} out of {parentGameObject.name} into {parentGameObject.transform.parent.gameObject.name}...");
                prop.name = newName;
                prop.transform.SetParent(parentGameObject.transform.parent);
            }
        }

        /// <summary>
        /// Checks to see if the Props are inside main structure GameObjects
        /// </summary>
        internal static bool ArePropsInMainBuildingStructure(GameObject buildingGameObject)
        {
            Building building = buildingGameObject.GetComponent<Building>();

            if (building.interiorProps != null && building.interiorProps.Length > 0)
            {
                foreach (GameObject prop in building.interiorProps)
                {
                    if (prop.IsParentedByAny(building.interiors, out _))
                    {
                        return true;
                    }
                }
            }

            if (building.exteriorProps == null || building.exteriorProps.Length <= 0)
            {
                return false;
            }

            {
                foreach (GameObject prop in building.exteriorProps)
                {
                    if (prop.IsParentedByAny(building.exteriors, out _))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the Layer of all children of the given Game Object
        /// </summary>
        private static void SetLayerInChildren(GameObject parentGameObject, string layerName, EditorLog log, bool includeParent = true)
        {
            foreach (MeshRenderer child in parentGameObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                child.gameObject.layer = LayerMask.NameToLayer(layerName);
                log.Log(LogLevel.Debug, $"Layer set to {layerName} in {child.gameObject}.");
            }

            if (!includeParent)
            {
                return;
            }

            parentGameObject.layer = LayerMask.NameToLayer(layerName);
            log.Log(LogLevel.Debug, $"Layer set to {layerName} in {parentGameObject.gameObject}.");
        }

        #endregion
    }
}