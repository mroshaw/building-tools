using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.Editor.Package
{
    [CreateAssetMenu(fileName = "BuildingToolsPackageContents", menuName = "Daft Apple Games/Package/Building Tools Package Contents", order = 1)]
    public class BuildingToolsPackageContents : PackageContents
    {
        protected override bool InstallPackage(EditorLog log)
        {
            log.Log(LogLevel.Info, "Creating tags...");
            CustomEditorTools.AddTag("Player");
            log.Log(LogLevel.Info, "Creating layers...");
            CustomEditorTools.AddLayer("BuildingExterior");
            CustomEditorTools.AddLayer("BuildingInterior");
            CustomEditorTools.AddLayer("InteriorProps");
            CustomEditorTools.AddLayer("ExteriorProps");
            log.Log(LogLevel.Info, "Renaming Rendering Layers...");
            CustomEditorTools.RenameRenderingLayer(1, "Exterior");
            CustomEditorTools.RenameRenderingLayer(2, "Interior");

            return true;
        }

        protected override bool UnInstallPackage(EditorLog log)
        {
            return true;
        }
    }
}