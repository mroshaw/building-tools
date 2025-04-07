using DaftAppleGames.BuildingTools.Editor;
using UnityEngine;

namespace DaftAppleGames.Editor
{
    [CreateAssetMenu(fileName = "AddBuildingComponentEditorTool", menuName = "Daft Apple Games/Editor Tools/Add Building Component Tool")]
    internal class AddBuildingComponentEditorTool : EditorTool
    {
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