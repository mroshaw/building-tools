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
        [SerializeField] private bool configureMeshLayersOption;
        [SerializeField] private bool setStaticFlagsOption;

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation() &&
                   ValidateLayerSetup(selectedGameObject) && ValidateBuildingSetup(selectedGameObject);
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            Log.Log(LogLevel.Info, $"Running ConfigureMeshesEditorTool. Configure Layers is {configureMeshLayersOption}, Set Static Flags is {setStaticFlagsOption}");
            if (editorSettings is BuildingEditorSettings buildingEditorSettings)
            {
                if (configureMeshLayersOption)
                {
                    Log.Log(LogLevel.Info, "Configuring layers...");
                    BuildingConfigTools.ConfigureLayers(selectedGameObject, buildingEditorSettings, Log);
                    Log.Log(LogLevel.Info, "Done!");
                }

                if (setStaticFlagsOption)
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

        protected override void AddCustomBindings()
        {
            configureMeshLayersOption = BindToToggleOption("ConfigureMeshLayersToggle", SetConfigureMeshLayersOption);
            setStaticFlagsOption = BindToToggleOption("SetStaticFlagsToggle", SetStaticFlagOption);
        }

        private void SetConfigureMeshLayersOption(ChangeEvent<bool> changeEvent)
        {
            configureMeshLayersOption = changeEvent.newValue;
        }

        private void SetStaticFlagOption(ChangeEvent<bool> changeEvent)
        {
            setStaticFlagsOption = changeEvent.newValue;
        }
    }
}