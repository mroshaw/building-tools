using DaftAppleGames.Editor;
using UnityEngine;

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "ConfigureLightingEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Lighting Tool")]
    internal class ConfigureLightingEditorTool : BuildingEditorTool
    {
        protected override bool IsSupported(out string notSupportedReason)
        {
#if !DAG_HDRP
            notSupportedReason = "Only currently supported on HDRP - URP and BIRP support is on it's way!";
            return false;
#endif
            notSupportedReason = string.Empty;
            return true;
        }

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