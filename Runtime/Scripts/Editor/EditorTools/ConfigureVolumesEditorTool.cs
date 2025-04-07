using DaftAppleGames.BuildingTools.Editor;
using UnityEngine;

namespace DaftAppleGames.Editor
{
    [CreateAssetMenu(fileName = "ConfigureVolumesEditorTool", menuName = "Daft Apple Games/Editor Tools/Configure Volumes Tool")]
    internal class ConfigureVolumesEditorTool : EditorTool
    {
        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation();
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            if (editorSettings is BuildingEditorSettings buildingEditorSettings)
            {
                BuildingConfigTools.ConfigureVolumes(selectedGameObject, buildingEditorSettings, Log);
            }
        }
    }
}