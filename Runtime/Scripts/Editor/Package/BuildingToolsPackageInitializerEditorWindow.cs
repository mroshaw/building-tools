using DaftAppleGames.Editor;
using DaftAppleGames.Editor.Package;
using UnityEditor;
using UnityEngine;

namespace DaftAppleGames.TimeAndWeather.Editor
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

        protected override void PostInstallation(PackageContents packageContents, EditorLog editorLog)
        {
            editorLog.Log(LogLevel.Info, "Creating tags...");
            CustomEditorTools.AddTag("Player");
            editorLog.Log(LogLevel.Info, "Creating layers...");
            CustomEditorTools.AddLayer("BuildingExterior");
            CustomEditorTools.AddLayer("BuildingInterior");
            CustomEditorTools.AddLayer("InteriorProps");
            CustomEditorTools.AddLayer("ExteriorProps");
            editorLog.Log(LogLevel.Info, "Renaming Rendering Layers...");
            CustomEditorTools.RenameRenderingLayer(1, "Exterior");
            CustomEditorTools.RenameRenderingLayer(2, "Interior");
        }

        protected override void PostUnInstallation(PackageContents packageContents, EditorLog editorLog)
        {
        }
    }
}