using System;
using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.BuildingTools.Editor
{
    [Serializable]
    internal struct BuildingInitSettings
    {
        [SerializeField] internal float adjustAnchorHeight;
    }

    /// <summary>
    /// This tool sets up the building, adding the Building component and re-jigging the anchor so that the building can be placed easier
    /// </summary>
    [CreateAssetMenu(fileName = "InitialiseBuildingEditorTool", menuName = "Daft Apple Games/Building Tools/Initialise Building Tool")]
    internal class InitialiseBuildingEditorTool : BuildingEditorTool
    {
        private bool _addBuildingComponentOption;
        private bool _setBuildingAnchorOption;

        protected override string GetToolName()
        {
            return "Initialise Building";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation();
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, string undoGroupName)
        {
            if (editorSettings is BuildingWizardEditorSettings buildingEditorSettings)
            {
                if (_addBuildingComponentOption)
                {
                    log.Log(LogLevel.Info, "Adding Building component (if not already there)...");
                    AddBuildingComponent(selectedGameObject);
                    log.Log(LogLevel.Info, "Adding Building component (if not already there). DONE!");
                }

                if (_setBuildingAnchorOption)
                {
                    log.Log(LogLevel.Info, "Setting Building anchor...");
                    SetBuildingAnchor(selectedGameObject, buildingEditorSettings.buildingInitSetting);
                    log.Log(LogLevel.Info, "Setting Building anchor... DONE!");
                }
            }
        }

        /// <summary>
        /// Adds the Building component
        /// </summary>
        /// <param name="parentGameObject"></param>
        private static void AddBuildingComponent(GameObject parentGameObject)
        {
            _ = parentGameObject.EnsureComponent<Building>();
            log.Log(LogLevel.Info, $"Added Building component to {parentGameObject.name}.");
        }

        /// <summary>
        /// Adjusts the building anchor, by parenting the building, so that it is raised up over the anchor point
        /// This makes it easier to place on a non-flat surface or terrain
        /// </summary>
        private static void SetBuildingAnchor(GameObject parentGameObject, BuildingInitSettings buildingInitSettings)
        {
            // Move each child game object vertically by the settings amount
            foreach (Transform child in parentGameObject.transform)
            {
                Vector3 newPosition = new(0, buildingInitSettings.adjustAnchorHeight, 0);
                // child.position += Vector3.up * buildingEditorSettings.adjustAnchorHeight;
                child.localPosition = newPosition;
                log.Log(LogLevel.Debug, $"Moved child {child.name} to position {newPosition}.");
            }
        }

        /// <summary>
        /// Add bindings for custom tool options
        /// </summary>
        protected override void AddCustomBindings()
        {
            _addBuildingComponentOption = BindToToggleOption("AddBuildingComponentToggle", SetAddBuildingComponentOption);
            _setBuildingAnchorOption = BindToToggleOption("SetBuildingAnchorToggle", SetBuildingAnchorOption);
        }

        private void SetAddBuildingComponentOption(ChangeEvent<bool> changeEvent)
        {
            _addBuildingComponentOption = changeEvent.newValue;
        }

        private void SetBuildingAnchorOption(ChangeEvent<bool> changeEvent)
        {
            _setBuildingAnchorOption = changeEvent.newValue;
        }
    }
}