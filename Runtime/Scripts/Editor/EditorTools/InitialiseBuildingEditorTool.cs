using System.Collections.Generic;
using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEngine;
using UnityEngine.UIElements;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// This tool sets up the building, adding the Building component and re-jigging the anchor so that the building can be placed easier
    /// </summary>
    [CreateAssetMenu(fileName = "InitialiseBuildingEditorTool", menuName = "Daft Apple Games/Building Tools/Initialise Building Tool")]
    internal class InitialiseBuildingEditorTool : BuildingEditorTool
    {
        [SerializeField] [BoxGroup("Settings")] internal float adjustAnchorHeight;

        protected override string GetToolName()
        {
            return "Initialise Building";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, out List<string> cannotRunReasons)
        {
            bool canRun = true;

            cannotRunReasons = new List<string>();
            if (!RequireGameObjectValidation(out string requireGameObjectReason))
            {
                cannotRunReasons.Add(requireGameObjectReason);
                canRun = false;
            }

            return canRun;
        }

        protected override void RunTool(GameObject selectedGameObject, string undoGroupName)
        {
            log.AddToLog(LogLevel.Info, "Adding Building component (if not already there)...");
            if (!AddBuildingComponent(selectedGameObject))
            {
                ShowPopupWindow("Daft Apple Building Tools", "IMPORTANT! ACTION REQUIRED!",
                    "You must now expand the 'Meshes' foldout in the new 'Building' component, and drag 'Interior', 'Exterior', and 'Prop' parent game objects into the appropriate array. If prop Game Objects are children of building mesh game objects, please 'un-parent' them now.");
            }

            log.AddToLog(LogLevel.Info, "Adding Building component (if not already there). DONE!");

            log.AddToLog(LogLevel.Info, "Setting Building anchor...");
            SetBuildingAnchor(selectedGameObject);
            log.AddToLog(LogLevel.Info, "Setting Building anchor... DONE!");
        }

        /// <summary>
        /// Adds the Building component
        /// </summary>
        /// <param name="parentGameObject"></param>
        private static bool AddBuildingComponent(GameObject parentGameObject)
        {
            Building building = parentGameObject.EnsureComponent<Building>();
            log.AddToLog(LogLevel.Info, $"Added Building component to {parentGameObject.name}.");

            return building && building.RequiredPropertiesSet();
        }

        /// <summary>
        /// Adjusts the building anchor, by parenting the building, so that it is raised up over the anchor point
        /// This makes it easier to place on a non-flat surface or terrain
        /// </summary>
        private void SetBuildingAnchor(GameObject parentGameObject)
        {
            // Move each child game object vertically by the settings amount
            foreach (Transform child in parentGameObject.transform)
            {
                Vector3 newPosition = new(0, adjustAnchorHeight, 0);
                // child.position += Vector3.up * buildingEditorSettings.adjustAnchorHeight;
                child.localPosition = newPosition;
                log.AddToLog(LogLevel.Debug, $"Moved child {child.name} to position {newPosition}.");
            }
        }
    }
}