using DaftAppleGames.BuildingTools.Editor;
using UnityEngine;

namespace DaftAppleGames.Editor
{
    [CreateAssetMenu(fileName = "ConfigureLightingEditorTool", menuName = "Daft Apple Games/Editor Tools/Configure Lighting Tool")]
    internal class ConfigureLightingEditorTool : EditorTool
    {
        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation();
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            if (editorSettings is BuildingEditorSettings buildingEditorSettings)
            {
                BuildingConfigTools.ConfigureLighting(selectedGameObject, buildingEditorSettings, Log);
            }
        }
    }
}