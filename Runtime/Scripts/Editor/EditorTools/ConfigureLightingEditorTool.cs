using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
#if DAG_HDRP || DAG_UURP
using DaftAppleGames.Lighting;
using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine;
using UnityEngine.Rendering;


namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "ConfigureLightingEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Lighting Tool")]
    internal class ConfigureLightingEditorTool : BuildingEditorTool
    {
        protected override string GetToolName()
        {
            return "Set Up Lights";
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
                ConfigureLighting(selectedGameObject, buildingEditorSettings);
            }
        }

        #region Static Lighting methods

        private static void ConfigureLighting(GameObject parentGameObject, BuildingWizardEditorSettings buildingWizardSettings)
        {
            LightingController lightingController = parentGameObject.EnsureComponent<LightingController>();
            log.Log(LogLevel.Info, $"Added Lighting Controller component to {parentGameObject.name}.");

            Transform[] allChildren = parentGameObject.GetComponentsInChildren<Transform>(true);

            // Configure interior candles
            foreach (Transform childTransform in allChildren)
            {
                if (buildingWizardSettings.indoorCandleSettings.meshNames.ItemInString(childTransform.gameObject.name))
                {
                    ConfigureBuildingLight(childTransform.gameObject, buildingWizardSettings.indoorCandleSettings);
                }
            }

            // Configure interior fires
            foreach (Transform childTransform in allChildren)
            {
                if (buildingWizardSettings.indoorFireSettings.meshNames.ItemInString(childTransform.gameObject.name))
                {
                    ConfigureBuildingLight(childTransform.gameObject, buildingWizardSettings.indoorFireSettings);
                }
            }

            // Configure exterior lights
            foreach (Transform childTransform in allChildren)
            {
                if (buildingWizardSettings.outdoorLightSettings.meshNames.ItemInString(childTransform.gameObject.name))
                {
                    ConfigureBuildingLight(childTransform.gameObject, buildingWizardSettings.outdoorLightSettings);
                }
            }

            lightingController.ConfigureInEditor();
        }

        private static void ConfigureBuildingLight(GameObject lightGameObject, LightTools.LightingSettings lightingSettings)
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
            ConfigureLight(lightGameObject, light, lightingSettings);
            log.Log(LogLevel.Debug, $"Configuring light on : {lightGameObject.name}... DONE!");
        }

        #region Configure Lights methods

        private static void ConfigureLight(GameObject lightGameObject, Light light, LightTools.LightingSettings lightingSettings)
        {
            // Look for old Lens Flare and destroy it - not supported in URP or HDRP
#if DAG_HDRP || DAG_URP
            if (lightGameObject.TryGetComponentInChildren(out LensFlare oldLensFlare, true))
            {
                DestroyImmediate(oldLensFlare);

                log.Log(LogLevel.Debug, $"Destroyed old Lens Flare component on: {lightGameObject.name}.");
            }

            // Set up Lens Flare, if it's selected
            if (lightingSettings.useLensFlare)
            {
                LensFlareComponentSRP newLensFlare = light.gameObject.EnsureComponent<LensFlareComponentSRP>();
                newLensFlare.lensFlareData = lightingSettings.lensFlareData;
                newLensFlare.intensity = lightingSettings.lensFlareIntensity;
                newLensFlare.environmentOcclusion = true;
                newLensFlare.useOcclusion = true;
                newLensFlare.allowOffScreen = false;

                log.Log(LogLevel.Debug, $"Configured an SRP Lens Flare component to: {lightGameObject.name}.");
            }
#endif

            log.Log(LogLevel.Debug, $"Setting light properties on: {lightGameObject.name}.");
            LightTools.ConfigureLight(light, lightingSettings);
        }

        private static void ConfigureOnDemandShadowMap(Light light, LightTools.LightingSettings lightingSettings)
        {
#if DAG_HDRP
            HDAdditionalLightData hdLightData = light.GetComponent<HDAdditionalLightData>();
            // Add a ShadowMap manager
            hdLightData.shadowUpdateMode = ShadowUpdateMode.OnDemand;
            OnDemandShadowMapUpdate shadowMapUpdate = light.EnsureComponent<OnDemandShadowMapUpdate>();
            shadowMapUpdate.counterMode = CounterMode.Frames;
            shadowMapUpdate.shadowMapToRefresh = ShadowMapToRefresh.EntireShadowMap;
            shadowMapUpdate.fullShadowMapRefreshWaitSeconds = lightingSettings.shadowRefreshRate;

#endif
        }

        #endregion

        #endregion
    }
}