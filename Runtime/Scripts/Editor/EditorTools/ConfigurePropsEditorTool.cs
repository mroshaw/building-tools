using DaftAppleGames.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "ConfigurePropsEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Props Tool")]
    internal class ConfigurePropsEditorTool : BuildingEditorTool
    {
        private bool _addMissingCollidersOption;
        private bool _alignExteriorPropsToTerrainOption;

        protected override string GetToolName()
        {
            return "Configure Props";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation() && RequiredBuildingMeshValidation();
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, string undoGroupName)
        {
            Log.Log(LogLevel.Info, $"Running ConfigurePropsEditorTool. Add Colliders is {_addMissingCollidersOption}, Align To Terrain is {_alignExteriorPropsToTerrainOption}");

            if (editorSettings is not BuildingWizardEditorSettings buildingEditorSettings)
            {
                return;
            }

            if (_addMissingCollidersOption)
            {
                Log.Log(LogLevel.Info, "Adding missing colliders to props...");
                BuildingConfigTools.ConfigureColliders(selectedGameObject, buildingEditorSettings, Log);
                Log.Log(LogLevel.Info, "Done");
            }

            if (_alignExteriorPropsToTerrainOption)
            {
                Log.Log(LogLevel.Info, "Aligning exterior props to terrain...");
                BuildingConfigTools.AlignExteriorPropsToTerrain(selectedGameObject, buildingEditorSettings, Log);
                Log.Log(LogLevel.Info, "Done");
            }
        }

        /// <summary>
        /// Add bindings for custom tool options
        /// </summary>
        protected override void AddCustomBindings()
        {
            _addMissingCollidersOption = BindToToggleOption("CreateMissingCollidersToggle", SetConfigureCollidersOption);
            _alignExteriorPropsToTerrainOption = BindToToggleOption("AlignExteriorPropsToTerrainToggle", SetAlignToTerrainOption);
        }

        private void SetConfigureCollidersOption(ChangeEvent<bool> changeEvent)
        {
            _addMissingCollidersOption = changeEvent.newValue;
        }

        private void SetAlignToTerrainOption(ChangeEvent<bool> changeEvent)
        {
            _alignExteriorPropsToTerrainOption = changeEvent.newValue;
        }
    }
}