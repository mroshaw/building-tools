using DaftAppleGames.Editor;
using UnityEngine;

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// Implementation of the initial setup tool, to create and amend layers, tags and render layer config
    /// </summary>
    [CreateAssetMenu(fileName = "SetUpEditorTool", menuName = "Daft Apple Games/Building Tools/Set Up Tool")]
    internal class SetUpEditorTool : BuildingEditorTool
    {
        protected override string GetToolName()
        {
            return "Set Up Building Tools";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, out string cannotRunReason)
        {
            // Check to see if setup has already been run or settings added manually
            if (!CustomEditorTools.DoLayersExist(new[] { "BuildingExterior", "BuildingInterior", "InteriorProps", "ExteriorProps" }))
            {
                cannotRunReason = string.Empty;
                return true;
            }

            cannotRunReason = "Setup has already been run, or the required layers have been manually added";
            return false;
        }

        /// <summary>
        /// Implementation of the "Set Up" tool
        /// </summary>
        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, string undoGroupName)
        {
            log.Log(LogLevel.Info, "Running setup...");
            log.Log(LogLevel.Info, "Creating tags...");
            CustomEditorTools.AddTag("Player");
            log.Log(LogLevel.Info, "Creating layers...");
            log.Log(LogLevel.Debug, "Adding BuildingExterior.");
            CustomEditorTools.AddLayer("BuildingExterior");
            log.Log(LogLevel.Debug, "Adding BuildingInterior.");
            CustomEditorTools.AddLayer("BuildingInterior");
            log.Log(LogLevel.Debug, "Adding InteriorProps.");
            CustomEditorTools.AddLayer("InteriorProps");
            log.Log(LogLevel.Debug, "Adding ExteriorProps.");
            CustomEditorTools.AddLayer("ExteriorProps");
            log.Log(LogLevel.Info, "Renaming Rendering Layers...");
            log.Log(LogLevel.Debug, "Renaming index 1 to Exterior.");
            CustomEditorTools.RenameRenderingLayer(1, "Exterior");
            log.Log(LogLevel.Debug, "Renaming index 2 to Interior.");
            CustomEditorTools.RenameRenderingLayer(2, "Interior");
            log.Log(LogLevel.Info, "Done!");
        }
    }
}