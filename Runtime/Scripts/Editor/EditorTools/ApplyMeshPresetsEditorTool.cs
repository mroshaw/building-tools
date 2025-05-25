using System.Collections.Generic;
using System.IO;
using DaftAppleGames.Buildings;
using DaftAppleGames.Core;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "ConfigureMeshesEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Meshes Tool")]
    internal class ApplyMeshPresetsEditorTool : BuildingEditorTool
    {
        [InlineEditor] [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Mesh presets to be applied to all interior building mesh renderers.")] internal MeshEditorPresetSettings interiorMeshSettings;

        [InlineEditor] [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Mesh presets to be applied to all exterior building mesh renderers.")] internal MeshEditorPresetSettings exteriorMeshSettings;

        [InlineEditor] [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Mesh presets to be applied to all interior prop mesh renderers.")] internal MeshEditorPresetSettings interiorPropMeshSettings;

        [InlineEditor] [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Mesh presets to be applied to all exterior prop mesh renderers.")] internal MeshEditorPresetSettings exteriorPropMeshSettings;

        protected override string GetToolName()
        {
            return "Apply Mesh Presets";
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

            if (!HasRequiredBuildingComponent(out string requiredBuildingReason))
            {
                cannotRunReasons.Add(requiredBuildingReason);
                return false;
            }

            if (!HasExteriorConfigured(out string requiredBuildingMeshReason))
            {
                cannotRunReasons.Add(requiredBuildingMeshReason);
                return false;
            }

            if (!ValidateToolSettings(out string toolSettingsReason))
            {
                cannotRunReasons.Add(toolSettingsReason);
                canRun = false;
            }

            return canRun;
        }

        private bool ValidateToolSettings(out string validationReason)
        {
            if (interiorMeshSettings && exteriorMeshSettings && interiorPropMeshSettings && exteriorPropMeshSettings)
            {
                validationReason = string.Empty;
                return true;
            }

            validationReason = "Mesh presets are missing from the selected settings!";
            return false;
        }

        /// <summary>
        /// Save the associated Mesh Settings too
        /// </summary>
        public override EnhancedScriptableObject SaveCopy(string pathToSave, string fileName, string childFolder)
        {
            ApplyMeshPresetsEditorTool newTool = base.SaveCopy(pathToSave, fileName, childFolder) as ApplyMeshPresetsEditorTool;

            if (!newTool)
            {
                return this;
            }

            string newBaseFolder = Path.Combine(pathToSave, childFolder);

            MeshEditorPresetSettings newInteriorMeshSettings =
                interiorMeshSettings.SaveCopy(newBaseFolder, string.Empty, "Mesh Settings") as MeshEditorPresetSettings;
            MeshEditorPresetSettings newExteriorMeshSettings = exteriorMeshSettings.SaveCopy(newBaseFolder, string.Empty, "Mesh Settings") as MeshEditorPresetSettings;
            MeshEditorPresetSettings newInteriorPropMeshSettings = interiorPropMeshSettings.SaveCopy(newBaseFolder, string.Empty, "Mesh Settings") as MeshEditorPresetSettings;
            MeshEditorPresetSettings newExteriorPropMeshSettings = exteriorPropMeshSettings.SaveCopy(newBaseFolder, string.Empty, "Mesh Settings") as MeshEditorPresetSettings;

            newTool.interiorMeshSettings = newInteriorMeshSettings;
            newTool.exteriorMeshSettings = newExteriorMeshSettings;
            newTool.interiorPropMeshSettings = newInteriorPropMeshSettings;
            newTool.exteriorPropMeshSettings = newExteriorPropMeshSettings;

            return newTool;
        }

        protected override void RunTool(string undoGroupName)
        {
            Building building = SelectedGameObject.GetComponent<Building>();
            interiorMeshSettings.ConfigureMeshOnAllGameObjects(building.interiors, log);
            exteriorMeshSettings.ConfigureMeshOnAllGameObjects(building.exteriors, log);
            interiorPropMeshSettings.ConfigureMeshOnAllGameObjects(building.interiorProps, log);
            exteriorPropMeshSettings.ConfigureMeshOnAllGameObjects(building.exteriorProps, log);
        }

        /// <summary>
        /// Checks to see if it's possible to configure the layers as we want them
        /// </summary>
        private bool LayerSetupValidation(out string validationReason)
        {
            // If not a prefab or prefab instance, then we're good
            if (!PrefabUtility.IsPartOfAnyPrefab(SelectedGameObject) || !ArePropsInMainBuildingStructure(SelectedGameObject))
            {
                validationReason = string.Empty;
                return true;
            }

            validationReason =
                "The selected GameObject is a prefab or prefab instance, and it's props GameObjects are children of the main building structure. Please amend the prefab and re-parent the props outside of the building structure.";
            return false;
        }

        /// <summary>
        /// Checks to see if the Props are inside main structure GameObjects
        /// </summary>
        private bool ArePropsInMainBuildingStructure(GameObject buildingGameObject)
        {
            Building building = buildingGameObject.GetComponent<Building>();

            if (building.interiorProps != null && building.interiorProps.Length > 0)
            {
                foreach (GameObject prop in building.interiorProps)
                {
                    if (prop.IsParentedByAny(building.interiors, out _))
                    {
                        return true;
                    }
                }
            }

            if (building.exteriorProps == null || building.exteriorProps.Length <= 0)
            {
                return false;
            }

            {
                foreach (GameObject prop in building.exteriorProps)
                {
                    if (prop.IsParentedByAny(building.exteriors, out _))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}