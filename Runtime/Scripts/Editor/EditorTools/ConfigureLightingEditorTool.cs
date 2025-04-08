using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEngine;

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "ConfigureLightingEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Lighting Tool")]
    internal class ConfigureLightingEditorTool : BuildingEditorTool
    {
        protected override string GetToolName()
        {
            return "Configure Lighting";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
#if !DAG_HDRP
            notSupportedReason = "Only currently supported on HDRP - URP and BIRP support is on it's way!";
            return false;
#endif
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
                ConfigureLighting(selectedGameObject, buildingEditorSettings, Log);
            }
        }

        #region Static Lighting methods

        private static void ConfigureLighting(GameObject parentGameObject, BuildingWizardEditorSettings buildingWizardSettings, EditorLog log)
        {
            LightingController lightingController = parentGameObject.EnsureComponent<LightingController>();
            log.Log(LogLevel.Info, $"Added Lighting Controller component to {parentGameObject.name}.");

            Transform[] allChildren = parentGameObject.GetComponentsInChildren<Transform>(true);

            // Configure interior candles
            foreach (Transform childTransform in allChildren)
            {
                if (buildingWizardSettings.indoorCandleSettings.meshNames.ItemInString(childTransform.gameObject.name))
                {
                    ConfigureBuildingLight(childTransform.gameObject, buildingWizardSettings.indoorCandleSettings, buildingWizardSettings, log);
                }
            }

            // Configure interior fires
            foreach (Transform childTransform in allChildren)
            {
                if (buildingWizardSettings.indoorFireSettings.meshNames.ItemInString(childTransform.gameObject.name))
                {
                    ConfigureBuildingLight(childTransform.gameObject, buildingWizardSettings.indoorFireSettings, buildingWizardSettings, log);
                }
            }

            // Configure exterior lights
            foreach (Transform childTransform in allChildren)
            {
                if (buildingWizardSettings.outdoorLightSettings.meshNames.ItemInString(childTransform.gameObject.name))
                {
                    ConfigureBuildingLight(childTransform.gameObject, buildingWizardSettings.outdoorLightSettings, buildingWizardSettings, log);
                }
            }

            lightingController.UpdateLights();
            ConfigureLightLayers(parentGameObject, buildingWizardSettings, log);
        }

        private static void ConfigureBuildingLight(GameObject lightGameObject, LightingSettings lightingSettings, BuildingWizardEditorSettings buildingWizardSettings,
            EditorLog log)
        {
            if (!lightGameObject.TryGetComponentInChildren(out Light light, true))
            {
                log.Log(LogLevel.Warning, $"No light found on parent mesh {lightGameObject.name}.");
                return;
            }

            if (!lightGameObject.TryGetComponentInChildren(out ParticleSystem flameParticleSystem, true))
            {
                log.Log(LogLevel.Debug, $"No flame particle system found on parent mesh {lightGameObject.name}.");
            }

            BuildingLight buildingLight = lightGameObject.EnsureComponent<BuildingLight>();
            buildingLight.ConfigureInEditor(lightingSettings.buildingLightType, light, flameParticleSystem);

            log.Log(LogLevel.Debug, $"Configuring light on : {lightGameObject.name}...");
            LightTools.ConfigureLight(lightGameObject, light, lightingSettings, log);
            log.Log(LogLevel.Debug, $"Configuring light on : {lightGameObject.name}... DONE!");
        }

        /// <summary>
        /// Set the LightLayer/RenderLayer of meshes in the building
        /// </summary>
        private static void ConfigureLightLayers(GameObject parentGameObject, BuildingWizardEditorSettings buildingWizardSettings, EditorLog log)
        {
            Building building = parentGameObject.GetComponent<Building>();

            LightTools.SetLightLayers(building.exteriors, buildingWizardSettings.buildingExteriorLightLayerMode, log);
            LightTools.SetLightLayers(building.interiors, buildingWizardSettings.buildingInteriorLightLayerMode, log);
            LightTools.SetLightLayers(building.interiorProps, buildingWizardSettings.interiorPropsLightLayerMode, log);
            LightTools.SetLightLayers(building.exteriorProps, buildingWizardSettings.exteriorPropsLightLayerMode, log);
        }

        #endregion
    }
}