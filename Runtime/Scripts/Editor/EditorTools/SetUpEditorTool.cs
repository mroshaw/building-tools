using System.Collections.Generic;
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

        protected override bool CanRunTool(GameObject selectedGameObject, out List<string> cannotRunReasons)
        {
            cannotRunReasons = new List<string>();

            // Check to see if setup has already been run or settings added manually
            if (!CustomEditorTools.DoLayersExist(new[] { "BuildingExterior", "BuildingInterior", "InteriorProps", "ExteriorProps" }))
            {
                return true;
            }

            cannotRunReasons.Add("Setup has already been run, or the required layers have been manually added");
            return false;
        }

        /// <summary>
        /// Implementation of the "Set Up" tool
        /// </summary>
        protected override void RunTool(GameObject selectedGameObject, string undoGroupName)
        {
            log.AddToLog(LogLevel.Info, "Running setup...");
            log.AddToLog(LogLevel.Info, "Creating tags...");
            CustomEditorTools.AddTag("Player");
            log.AddToLog(LogLevel.Info, "Creating layers...");
            log.AddToLog(LogLevel.Debug, "Adding BuildingExterior.");
            CustomEditorTools.AddLayer("BuildingExterior");
            log.AddToLog(LogLevel.Debug, "Adding BuildingInterior.");
            CustomEditorTools.AddLayer("BuildingInterior");
            log.AddToLog(LogLevel.Debug, "Adding InteriorProps.");
            CustomEditorTools.AddLayer("InteriorProps");
            log.AddToLog(LogLevel.Debug, "Adding ExteriorProps.");
            CustomEditorTools.AddLayer("ExteriorProps");
            log.AddToLog(LogLevel.Info, "Renaming Rendering Layers...");
            log.AddToLog(LogLevel.Debug, "Renaming index 1 to Exterior.");
            CustomEditorTools.RenameRenderingLayer(1, "Exterior");
            log.AddToLog(LogLevel.Debug, "Renaming index 2 to Interior.");
            CustomEditorTools.RenameRenderingLayer(2, "Interior");
            log.AddToLog(LogLevel.Info, "Done!");
        }
    }
}