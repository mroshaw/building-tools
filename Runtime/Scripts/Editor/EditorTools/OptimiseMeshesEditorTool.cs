using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using UnityEngine;

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "OptimiseMeshesEditorTool", menuName = "Daft Apple Games/Building Tools/Optimise Meshes Tool")]
    internal class OptimiseMeshesEditorTool : BuildingEditorTool
    {
        protected override string GetToolName()
        {
            return "Optimise Meshes";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation();
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, string undoGroupName)
        {
            if (editorSettings is BuildingWizardEditorSettings buildingEditorSettings)
            {
                OptimiseMeshes(selectedGameObject, buildingEditorSettings, Log);
            }
        }

        #region Static Mesh Optimisation methods

        private static void OptimiseMeshes(GameObject parentGameObject, BuildingWizardEditorSettings buildingWizardSettings, EditorLog log)
        {
            Building building = parentGameObject.GetComponent<Building>();

            // Combine the exterior Prop meshes
            MeshTools.CombineMeshParameters combineMeshParameters = new()
            {
                BaseAssetOutputPath = buildingWizardSettings.meshAssetOutputPath,
                AssetOutputFolder = parentGameObject.name,
                CreateOutputFolder = true
            };

            MeshTools.ConfigureMeshParameters newMeshParameters = new()
            {
                // Set properties and merge exterior meshes
                LightLayerMode = buildingWizardSettings.buildingExteriorLightLayerMode,
                LayerName = buildingWizardSettings.buildingExteriorLayer
            };

            OptimiseMeshGroup(building.exteriorProps, "exteriorProps", combineMeshParameters, newMeshParameters, log);
            OptimiseMeshGroup(building.exteriors, "exteriors", combineMeshParameters, newMeshParameters, log);

            // Set properties and merge interior meshes
            newMeshParameters.LightLayerMode = buildingWizardSettings.buildingInteriorLightLayerMode;
            newMeshParameters.LayerName = buildingWizardSettings.buildingInteriorLayer;
            OptimiseMeshGroup(building.interiors, "interiors", combineMeshParameters, newMeshParameters, log);
            OptimiseMeshGroup(building.interiorProps, "interiorProps", combineMeshParameters, newMeshParameters, log);
        }

        private static void OptimiseMeshGroup(GameObject[] allGameObjects, string namePrefix, MeshTools.CombineMeshParameters combineMeshParameters,
            MeshTools.ConfigureMeshParameters newMeshParameters, EditorLog log)
        {
            foreach (GameObject gameObjectParent in allGameObjects)
            {
                combineMeshParameters.AssetFileNamePrefix = namePrefix;
                MeshTools.CombineGameObjectMeshes(gameObjectParent, combineMeshParameters, newMeshParameters);
            }
        }

        #endregion
    }
}