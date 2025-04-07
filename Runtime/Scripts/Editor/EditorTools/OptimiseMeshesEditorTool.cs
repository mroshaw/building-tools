using DaftAppleGames.BuildingTools.Editor;
using UnityEngine;

namespace DaftAppleGames.Editor
{
    [CreateAssetMenu(fileName = "OptimiseMeshesEditorTool", menuName = "Daft Apple Games/Editor Tools/Optimise Meshes Tool")]
    internal class OptimiseMeshesEditorTool : EditorTool
    {
        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation();
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            if (editorSettings is BuildingEditorSettings buildingEditorSettings)
            {
                BuildingConfigTools.OptimiseMeshes(selectedGameObject, buildingEditorSettings, Log);
            }
        }
    }
}