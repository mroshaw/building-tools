using DaftAppleGames.Editor;
using UnityEditor;
using UnityEngine;

namespace DaftAppleGames.BuildingTools.Editor
{
    public class BuildingEditorWindow : ButtonWizardEditorWindow
    {
        /// <summary>
        /// Custom editor overrides
        /// </summary>
        protected override string ToolTitle => "Building Editor";

        protected override string IntroText => "Welcome to Building Tools by Daft Apple Games!";

        protected override string WelcomeLogText =>
            "Welcome to Building Tools! Select some building preset settings, then select a Game Object in the hierarchy or a prefab asset, then click the buttons to configure your building.";

        protected override void CreateCustomGUI()
        {
        }

        [MenuItem("Tools/Daft Apple Games/Building Tools/Building Editor")]
        public static void ShowWindow()
        {
            BuildingEditorWindow packageInitWindow = GetWindow<BuildingEditorWindow>();
            packageInitWindow.titleContent = new GUIContent("Building Editor");
        }
    }
}