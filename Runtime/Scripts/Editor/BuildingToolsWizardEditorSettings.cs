#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using DaftAppleGames.Editor;
using UnityEngine;

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "BuildingWizardEditorSettings", menuName = "Daft Apple Games/Building Tools/Building Wizard Editor Settings")]
    public class BuildingToolsWizardEditorSettings : ButtonWizardEditorSettings
    {
    }
}