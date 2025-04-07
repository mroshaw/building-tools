using DaftAppleGames.BuildingTools.Editor;
using DaftAppleGames.Darskerry.Core.Buildings;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.Editor
{
    [CreateAssetMenu(fileName = "ConfigureMeshesEditorTool", menuName = "Daft Apple Games/Editor Tools/Configure Meshes Tool")]
    internal class ConfigureMeshesEditorTool : EditorTool
    {
        private bool _configureMeshLayersOption;
        private bool _setStaticFlagsOption;

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation() &&
                   ValidateLayerSetup(selectedGameObject) && ValidateBuildingSetup(selectedGameObject);
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            Log.Log(LogLevel.Info, $"Running ConfigureMeshesEditorTool. Configure Layers is {_configureMeshLayersOption}, Set Static Flags is {_setStaticFlagsOption}");
            if (editorSettings is BuildingEditorSettings buildingEditorSettings)
            {
                if (_configureMeshLayersOption)
                {
                    Log.Log(LogLevel.Info, "Configuring layers...");
                    BuildingConfigTools.ConfigureLayers(selectedGameObject, buildingEditorSettings, Log);
                    Log.Log(LogLevel.Info, "Done!");
                }

                if (_setStaticFlagsOption)
                {
                    Log.Log(LogLevel.Info, "Configuring static flags...");
                    BuildingConfigTools.ConfigureStaticFlags(selectedGameObject, buildingEditorSettings, Log);
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

            if (!BuildingConfigTools.ArePropsInMainBuildingStructure(selectedGameObject))
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
    }
}