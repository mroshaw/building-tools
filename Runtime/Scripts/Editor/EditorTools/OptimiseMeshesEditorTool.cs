using DaftAppleGames.Editor;
using UnityEngine;

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "OptimiseMeshesEditorTool", menuName = "Daft Apple Games/Building Tools/Optimise Meshes Tool")]
    internal class OptimiseMeshesEditorTool : BuildingEditorTool
    {
        protected override string GetToolName()
        {
            return "Optimise Meshes";
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
                BuildingConfigTools.OptimiseMeshes(selectedGameObject, buildingEditorSettings, Log);
            }
        }
    }
}