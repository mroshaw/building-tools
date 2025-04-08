using DaftAppleGames.Editor;
using UnityEngine;

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "ConfigureVolumesEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Volumes Tool")]
    internal class ConfigureVolumesEditorTool : BuildingEditorTool
    {
        protected override string GetToolName()
        {
            return "Configure Volumes";
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

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, string undoGroupName)
        {
            if (editorSettings is BuildingWizardEditorSettings buildingEditorSettings)
            {
                BuildingConfigTools.ConfigureVolumes(selectedGameObject, buildingEditorSettings, Log);
            }
        }
    }
}