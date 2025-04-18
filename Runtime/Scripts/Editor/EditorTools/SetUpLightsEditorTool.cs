using System;
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
    [Serializable]
    internal struct BuildingLightSettings
    {
        [SerializeField] internal BuildingLightTypeSettings indoorCandleTypeSettings;
        [SerializeField] internal BuildingLightTypeSettings indoorFireTypeSettings;
        [SerializeField] internal BuildingLightTypeSettings outdoorBuildingLightTypeSettings;
    }

    /// <summary>
    /// This struct gives us an easy way of passing Lighting configuration around
    /// our various classes and methods
    /// </summary>
    [Serializable]
    public struct BuildingLightTypeSettings
    {
        public BuildingLightType buildingLightType;
        public string[] meshNames;
        public string[] flameNames;
        public bool useLensFlare;
#if DAG_HDRP || DAG_URP
        public float shadowRefreshRate;
        public float lensFlareIntensity;
        public LensFlareDataSRP lensFlareData;
#endif
        public LightEditorPresetSettings presetSettings;
    }

    [CreateAssetMenu(fileName = "ConfigureLightingEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Lighting Tool")]
    internal class SetUpLightsEditorTool : BuildingEditorTool
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
                ConfigureLighting(selectedGameObject, buildingEditorSettings.buildingLightSettings);
            }
        }

        #region Static Lighting methods

        private static void ConfigureLighting(GameObject parentGameObject, BuildingLightSettings buildingLightSettings)
        {
            LightingController lightingController = parentGameObject.EnsureComponent<LightingController>();
            log.Log(LogLevel.Info, $"Added Lighting Controller component to {parentGameObject.name}.");

            Transform[] allChildren = parentGameObject.GetComponentsInChildren<Transform>(true);

            // Configure interior candles
            foreach (Transform childTransform in allChildren)
            {
                if (buildingLightSettings.indoorCandleTypeSettings.meshNames.ItemInString(childTransform.gameObject.name))
                {
                    ConfigureBuildingLight(childTransform.gameObject, buildingLightSettings.indoorCandleTypeSettings);
                }
            }

            // Configure interior fires
            foreach (Transform childTransform in allChildren)
            {
                if (buildingLightSettings.indoorFireTypeSettings.meshNames.ItemInString(childTransform.gameObject.name))
                {
                    ConfigureBuildingLight(childTransform.gameObject, buildingLightSettings.indoorFireTypeSettings);
                }
            }

            // Configure exterior lights
            foreach (Transform childTransform in allChildren)
            {
                if (buildingLightSettings.outdoorBuildingLightTypeSettings.meshNames.ItemInString(childTransform.gameObject.name))
                {
                    ConfigureBuildingLight(childTransform.gameObject, buildingLightSettings.outdoorBuildingLightTypeSettings);
                }
            }

            lightingController.ConfigureInEditor();
        }

        private static void ConfigureBuildingLight(GameObject lightGameObject, BuildingLightTypeSettings lightingTypeSettings)
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
            buildingLight.ConfigureInEditor(lightingTypeSettings.buildingLightType, light, flameParticleSystem);

            log.Log(LogLevel.Debug, $"Configuring light on : {lightGameObject.name}...");
            ConfigureLight(lightGameObject, light, lightingTypeSettings);
            log.Log(LogLevel.Debug, $"Configuring light on : {lightGameObject.name}... DONE!");
        }

        private static void ConfigureLight(GameObject lightGameObject, Light light, BuildingLightTypeSettings lightingTypeSettings)
        {
            log.Log(LogLevel.Debug, $"Configuring light on : {lightGameObject.name}...");

            if (!lightingTypeSettings.presetSettings)
            {
                log.Log(LogLevel.Error, $"No presets found on {lightingTypeSettings.buildingLightType}.");
            }

            // Look for old Lens Flare and destroy it - not supported in URP or HDRP
#if DAG_HDRP || DAG_URP
            if (lightGameObject.TryGetComponentInChildren(out LensFlare oldLensFlare, true))
            {
                DestroyImmediate(oldLensFlare);

                log.Log(LogLevel.Debug, $"Destroyed old Lens Flare component on: {lightGameObject.name}.");
            }

            // Set up Lens Flare, if it's selected
            if (lightingTypeSettings.useLensFlare)
            {
                LensFlareComponentSRP newLensFlare = light.gameObject.EnsureComponent<LensFlareComponentSRP>();
                newLensFlare.lensFlareData = lightingTypeSettings.lensFlareData;
                newLensFlare.intensity = lightingTypeSettings.lensFlareIntensity;
                newLensFlare.environmentOcclusion = true;
                newLensFlare.useOcclusion = true;
                newLensFlare.allowOffScreen = false;

                log.Log(LogLevel.Debug, $"Configured an SRP Lens Flare component to: {lightGameObject.name}.");
            }
#endif

            log.Log(LogLevel.Debug, $"Setting light properties on: {lightGameObject.name}.");
            lightingTypeSettings.presetSettings.ApplyPreset(light);
        }

        private static void ConfigureOnDemandShadowMap(Light light, BuildingLightTypeSettings lightingSettings)
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
    }
}