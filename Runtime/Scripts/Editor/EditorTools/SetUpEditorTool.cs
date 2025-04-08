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
        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return true;
        }

        /// <summary>
        /// Implementation of the "Set Up" tool
        /// </summary>
        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            Log.Log(LogLevel.Info, "Running setup...");
            Log.Log(LogLevel.Info, "Creating tags...");
            CustomEditorTools.AddTag("Player");
            Log.Log(LogLevel.Info, "Creating layers...");
            Log.Log(LogLevel.Debug, "Adding BuildingExterior.");
            CustomEditorTools.AddLayer("BuildingExterior");
            Log.Log(LogLevel.Debug, "Adding BuildingInterior.");
            CustomEditorTools.AddLayer("BuildingInterior");
            Log.Log(LogLevel.Debug, "Adding InteriorProps.");
            CustomEditorTools.AddLayer("InteriorProps");
            Log.Log(LogLevel.Debug, "Adding ExteriorProps.");
            CustomEditorTools.AddLayer("ExteriorProps");
            Log.Log(LogLevel.Info, "Renaming Rendering Layers...");
            Log.Log(LogLevel.Debug, "Renaming index 1 to Exterior.");
            CustomEditorTools.RenameRenderingLayer(1, "Exterior");
            Log.Log(LogLevel.Debug, "Renaming index 2 to Interior.");
            CustomEditorTools.RenameRenderingLayer(2, "Interior");
            Log.Log(LogLevel.Info, "Done!");
        }
    }
}