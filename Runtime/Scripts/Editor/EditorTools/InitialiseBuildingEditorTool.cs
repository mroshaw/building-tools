using System.Collections.Generic;
using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// This tool sets up the building, adding the Building component and re-jigging the pivot so that the building can be placed easier
    /// </summary>
    [CreateAssetMenu(fileName = "InitialiseBuildingEditorTool", menuName = "Daft Apple Games/Building Tools/Initialise Building Tool")]
    internal class InitialiseBuildingEditorTool : BuildingEditorTool
    {
        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Drops the pivot by this amount, effectively raising the building. This makes it easier to place on uneven terrain.")] private float adjustPivotHeight;

        protected override string GetToolName()
        {
            return "Initialise Building";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(out List<string> cannotRunReasons)
        {
            bool canRun = true;

            cannotRunReasons = new List<string>();
            if (!RequireGameObjectValidation(out string requireGameObjectReason))
            {
                cannotRunReasons.Add(requireGameObjectReason);
                return false;
            }

            if (AlreadyHasBuildingComponent(out string alreadyConfiguredReason))
            {
                cannotRunReasons.Add(alreadyConfiguredReason);
                canRun = false;
            }

            return canRun;
        }

        private bool AlreadyHasBuildingComponent(out string failedReason)
        {
            if (SelectedGameObject && SelectedGameObject.HasComponent<Building>())
            {
                failedReason = "This game object has already been configured as a building!";
                return true;
            }

            failedReason = string.Empty;
            return false;
        }

        protected override void RunTool(string undoGroupName)
        {
            log.AddToLog(LogLevel.Info, "Adding Building component (if not already there)...");
            if (!AddBuildingComponent())
            {
                ShowPopupWindow("Daft Apple Building Tools", "IMPORTANT! ACTION REQUIRED!",
                    "You must now expand the 'Meshes' foldout in the new 'Building' component, and drag 'Interior', 'Exterior', and 'Prop' parent game objects into the appropriate array.");
            }

            log.AddToLog(LogLevel.Info, "Adding Building component (if not already there). DONE!");

            log.AddToLog(LogLevel.Info, "Setting Building anchor...");
            SetBuildingPivot();
            log.AddToLog(LogLevel.Info, "Setting Building anchor... DONE!");
        }

        /// <summary>
        /// Adds the Building component
        /// </summary>
        private bool AddBuildingComponent()
        {
            Building building = SelectedGameObject.EnsureComponent<Building>();
            log.AddToLog(LogLevel.Info, $"Added Building component to {SelectedGameObject.name}.");

            return building && building.RequiredPropertiesSet();
        }

        /// <summary>
        /// Adjusts the building anchor, by parenting the building, so that it is raised up over the anchor point
        /// This makes it easier to place on a non-flat surface or terrain
        /// </summary>
        private void SetBuildingPivot()
        {
            // Move each direct child game object vertically by the settings amount
            foreach (Transform child in SelectedGameObject.transform)
            {
                // Vector3 newPosition = new(child.localPosition.x, adjustPivotHeight, child.localPosition.z);
                Vector3 newPosition = child.transform.position + child.transform.up * adjustPivotHeight;
                child.localPosition = newPosition;
                log.AddToLog(LogLevel.Debug, $"Moved child {child.name} to position {newPosition}.");
            }
        }
    }
}