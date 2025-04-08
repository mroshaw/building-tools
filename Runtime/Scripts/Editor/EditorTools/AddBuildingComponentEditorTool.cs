using DaftAppleGames.Editor;
using UnityEngine;

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "AddBuildingComponentEditorTool", menuName = "Daft Apple Games/Building Tools/Add Building Component Tool")]
    internal class AddBuildingComponentEditorTool : BuildingEditorTool
    {
        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation();
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            if (editorSettings is BuildingEditorSettings buildingEditorSettings)
            {
                BuildingConfigTools.AddBuildingComponent(selectedGameObject, Log);
            }
        }
    }
}