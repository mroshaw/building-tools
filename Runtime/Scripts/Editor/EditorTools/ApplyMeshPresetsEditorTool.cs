using System;
using DaftAppleGames.Buildings;
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
    [Serializable]
    public struct BuildingMeshSettings
    {
        [SerializeField] internal MeshEditorPresetSettings interiorMeshSettings;
        [SerializeField] internal MeshEditorPresetSettings exteriorMeshSettings;
        [SerializeField] internal MeshEditorPresetSettings interiorPropMeshSettings;
        [SerializeField] internal MeshEditorPresetSettings exteriorPropMeshSettings;
    }

    [CreateAssetMenu(fileName = "ConfigureMeshesEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Meshes Tool")]
    internal class ApplyMeshPresetsEditorTool : BuildingEditorTool
    {
        protected override string GetToolName()
        {
            return "Apply Mesh Presets";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation() && RequiredBuildingMeshValidation() &&
                   ValidateLayerSetup(selectedGameObject);
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, string undoGroupName)
        {
            if (editorSettings is BuildingWizardEditorSettings buildingEditorSettings)
            {
                Building building = selectedGameObject.GetComponent<Building>();
                buildingEditorSettings.buildingMeshSettings.interiorMeshSettings.ConfigureMeshOnAllGameObjects(building.interiors, log);
                buildingEditorSettings.buildingMeshSettings.exteriorMeshSettings.ConfigureMeshOnAllGameObjects(building.exteriors, log);
                buildingEditorSettings.buildingMeshSettings.interiorPropMeshSettings.ConfigureMeshOnAllGameObjects(building.interiorProps, log);
                buildingEditorSettings.buildingMeshSettings.exteriorPropMeshSettings.ConfigureMeshOnAllGameObjects(building.exteriorProps, log);
            }
        }

        /// <summary>
        /// Checks to see if it's possible to configure the layers as we want them
        /// </summary>
        private bool ValidateLayerSetup(GameObject selectedGameObject)
        {
            // If not a prefab or prefab instance, then we're good
            if (!PrefabUtility.IsPartOfAnyPrefab(selectedGameObject))
            {
                return true;
            }

            if (!ArePropsInMainBuildingStructure(selectedGameObject))
            {
                return true;
            }

            log.Log(LogLevel.Error,
                "The selected GameObject is a prefab or prefab instance, and it's props GameObjects are children of the main building structure. Please amend the prefab and re-parent the props outside of the building structure.");
            return false;
        }

        /// <summary>
        /// Checks to see if the Props are inside main structure GameObjects
        /// </summary>
        private static bool ArePropsInMainBuildingStructure(GameObject buildingGameObject)
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