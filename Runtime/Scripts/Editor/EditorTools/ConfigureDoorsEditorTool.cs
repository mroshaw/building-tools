using DaftAppleGames.BuildingTools.Editor;
using UnityEngine;

namespace DaftAppleGames.Editor
{
    [CreateAssetMenu(fileName = "ConfigureDoorsEditorTool", menuName = "Daft Apple Games/Editor Tools/Configure Doors Tool")]
    internal class ConfigureDoorsEditorTool : EditorTool
    {
        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation();
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            if (editorSettings is BuildingEditorSettings buildingEditorSettings)
            {
                BuildingConfigTools.ConfigureDoors(selectedGameObject, buildingEditorSettings, Log);
            }
        }
    }
}