using DaftAppleGames.Editor;
using UnityEditor;
using UnityEngine;

namespace DaftAppleGames.BuildingTools.Editor
{
    public class BuildingToolsPackageInitializerEditorWindow : PackageInitializerEditorWindow
    {
        protected override string ToolTitle => "Building Tools Installer";

        protected override string WelcomeLogText =>
            "Welcome to the Building Tools installer!";

        [MenuItem("Daft Apple Games/Packages/Building Tools")]
        public static void ShowWindow()
        {
            BuildingToolsPackageInitializerEditorWindow packageInitWindow = GetWindow<BuildingToolsPackageInitializerEditorWindow>();
            packageInitWindow.titleContent = new GUIContent("Building Tools Installer");
        }
    }
}