using DaftAppleGames.Editor;
using DaftAppleGames.Editor.Package;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

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

            ConfigureInstalledItems(packageContents, editorLog);
        }

        /// <summary>
        /// Reconfigure the deployed Scriptable Object instance
        /// </summary>
        private void ConfigureInstalledItems(PackageContents packageContents, EditorLog editorLog)
        {
            editorLog.Log(LogLevel.Info, "Configuring installed items...");

            if (!packageContents.GetPackageObjectByName("BuildingSettings", out BuildingEditorSettings buildingEditorSettings))
            {
                editorLog.Log(LogLevel.Error, "Cannot find BuildingSettings installed by package!");
                return;
            }

            packageContents.GetPackageObjectByName("AudioMixer", out AudioMixer audioMixer);
            buildingEditorSettings.doorSfxGroup = audioMixer.FindMatchingGroups("Master/SoundFX")[0];

            packageContents.GetPackageObjectByName("DoorOpenAudio", out AudioClip doorOpenClip);
            AudioClip[] doorOpenClips = { doorOpenClip };
            buildingEditorSettings.doorOpenClips = doorOpenClips;

            packageContents.GetPackageObjectByName("DoorCloseAudio", out AudioClip doorCloseClip);
            buildingEditorSettings.doorClosingClips = doorOpenClips;
            AudioClip[] doorClosedClips = { doorCloseClip };
            buildingEditorSettings.doorClosedClips = doorClosedClips;

            packageContents.GetPackageObjectByName("InteriorVolume", out VolumeProfile volumeProfile);
            buildingEditorSettings.interiorVolumeProfile = volumeProfile;

            // Commit changes to the Scriptable Object instance asset
            editorLog.Log(LogLevel.Info, "Committing all asset changes...");
            CustomEditorTools.SaveChangesToAsset(buildingEditorSettings);
        }

        protected override void PostUnInstallation(PackageContents packageContents, EditorLog editorLog)
        {
        }
    }
}