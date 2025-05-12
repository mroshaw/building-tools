using System.Collections.Generic;
using DaftAppleGames.Editor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// Implementation of the initial setup tool, to create and amend layers, tags and render layer config
    /// </summary>
    [CreateAssetMenu(fileName = "ConfigureProjectEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Project Tool")]
    internal class ConfigureProjectEditorTool : BuildingEditorTool
    {
        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Name of the layer on which building exteriors (think outer walls, etc) will sit.")] internal string exteriorLayerName = "BuildingExterior";

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Name of the layer on which building interiors (think inner walls, etc) will sit.")] internal string interiorLayerName = "BuildingInterior";

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Name of the layer on which interior props (think tables, chairs, rugs, etc) will sit.")] internal string interiorPropsLayerName = "InteriorProps";

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Name of the layer on which exterior props (think barrels, carts, stables, etc) will sit.")] internal string exteriorPropsLayerName = "ExteriorProps";

#if DAG_HDRP || DAG_URP
        [SerializeField] [BoxGroup("HDRP/URP Settings")]
        [Tooltip("Name of Rendering Layer that will be influenced by Exterior lights.")] internal string exteriorRenderLayerName = "Exterior";

        [SerializeField] [BoxGroup("HDRP/URP Settings")]
        [Tooltip("Name of Rendering Layer that will be influenced by Interior lights.")] internal string interiorRenderLayerName = "Interior";
#endif

        protected override string GetToolName()
        {
#if DAG_HDRP
            return "Configure Project (HDRP)";
#endif
#if DAG_URP
            return "Configure Project (URP)";
#endif
#if DAG_BIRP
            return "Configure Project (BIRP)";
#endif
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(out List<string> cannotRunReasons)
        {
            cannotRunReasons = new List<string>();

            // Check to see if setup has already been run or settings added manually
            if (!CustomEditorTools.DoLayersExist(new[] { exteriorLayerName, interiorLayerName, interiorPropsLayerName, exteriorPropsLayerName }))
            {
                return true;
            }

            cannotRunReasons.Add("Project setup has already been run, or the required layers/tags etc have been manually added.");
            return false;
        }

        /// <summary>
        /// Implementation of the "Set Up" tool
        /// </summary>
        protected override void RunTool(string undoGroupName)
        {
            log.AddToLog(LogLevel.Info, "Running setup...");
            log.AddToLog(LogLevel.Info, "Creating tags...");
            CustomEditorTools.AddTag("Player");
            log.AddToLog(LogLevel.Info, "Creating layers...");
            log.AddToLog(LogLevel.Debug, $"Adding layer: {exteriorLayerName}");
            CustomEditorTools.AddLayer(exteriorLayerName);
            log.AddToLog(LogLevel.Debug, $"Adding layer: {interiorLayerName}");
            CustomEditorTools.AddLayer(interiorLayerName);
            log.AddToLog(LogLevel.Debug, $"Adding layer: {interiorPropsLayerName}.");
            CustomEditorTools.AddLayer(interiorPropsLayerName);
            log.AddToLog(LogLevel.Debug, $"Adding layer: {exteriorPropsLayerName}");
            CustomEditorTools.AddLayer(exteriorPropsLayerName);

#if DAG_HDRP || DAG_URP

            log.AddToLog(LogLevel.Info, "Renaming Rendering Layers...");
            log.AddToLog(LogLevel.Debug, $"Renaming index 1 to {exteriorRenderLayerName}.");
            CustomEditorTools.RenameRenderingLayer(1, exteriorRenderLayerName);
            log.AddToLog(LogLevel.Debug, $"Renaming index 2 to {interiorRenderLayerName}.");
            CustomEditorTools.RenameRenderingLayer(2, interiorRenderLayerName);
            log.AddToLog(LogLevel.Info, "Done!");
#endif
        }
    }
}