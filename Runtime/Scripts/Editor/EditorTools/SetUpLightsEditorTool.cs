using System;
using System.Collections.Generic;
using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
#if DAG_HDRP || DAG_UURP
using DaftAppleGames.Lighting;
using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine;
using UnityEngine.Rendering;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.BuildingTools.Editor
{
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
        [SerializeField] [BoxGroup("Settings")] internal BuildingLightTypeSettings indoorCandleTypeSettings;
        [SerializeField] [BoxGroup("Settings")] internal BuildingLightTypeSettings indoorFireTypeSettings;
        [SerializeField] [BoxGroup("Settings")] internal BuildingLightTypeSettings outdoorBuildingLightTypeSettings;

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

        protected override bool CanRunTool(GameObject selectedGameObject, out List<string> cannotRunReasons)
        {
            bool canRun = true;

            cannotRunReasons = new List<string>();
            if (!RequireGameObjectValidation(out string requireGameObjectReason))
            {
                cannotRunReasons.Add(requireGameObjectReason);
                canRun = false;
            }

            if (!RequiredBuildingValidation(out string requiredBuildingReason))
            {
                cannotRunReasons.Add(requiredBuildingReason);
                canRun = false;
            }

            return canRun;
        }

        protected override void RunTool(GameObject selectedGameObject, string undoGroupName)
        {
            ConfigureLighting(selectedGameObject);
        }

        private void ConfigureLighting(GameObject parentGameObject)
        {
            LightingController lightingController = parentGameObject.EnsureComponent<LightingController>();
            log.AddToLog(LogLevel.Info, $"Added Lighting Controller component to {parentGameObject.name}.");

            Transform[] allChildren = parentGameObject.GetComponentsInChildren<Transform>(true);

            // Configure interior candles
            foreach (Transform childTransform in allChildren)
            {
                if (indoorCandleTypeSettings.meshNames.ItemInString(childTransform.gameObject.name))
                {
                    ConfigureBuildingLight(childTransform.gameObject, indoorCandleTypeSettings);
                }
            }

            // Configure interior fires
            foreach (Transform childTransform in allChildren)
            {
                if (indoorFireTypeSettings.meshNames.ItemInString(childTransform.gameObject.name))
                {
                    ConfigureBuildingLight(childTransform.gameObject, indoorFireTypeSettings);
                }
            }

            // Configure exterior lights
            foreach (Transform childTransform in allChildren)
            {
                if (outdoorBuildingLightTypeSettings.meshNames.ItemInString(childTransform.gameObject.name))
                {
                    ConfigureBuildingLight(childTransform.gameObject, outdoorBuildingLightTypeSettings);
                }
            }

            lightingController.ConfigureInEditor();
        }

        private void ConfigureBuildingLight(GameObject lightGameObject, BuildingLightTypeSettings lightTypeSettings)
        {
            if (!lightGameObject.TryGetComponentInChildren(out Light light, true))
            {
                log.AddToLog(LogLevel.Warning, $"No light found on parent mesh {lightGameObject.name}.");
                return;
            }

            if (!lightGameObject.TryGetComponentInChildren(out ParticleSystem flameParticleSystem, true))
            {
                log.AddToLog(LogLevel.Debug, $"No flame particle system found on parent mesh {lightGameObject.name}.");
            }

            BuildingLight buildingLight = lightGameObject.EnsureComponent<BuildingLight>();
            buildingLight.ConfigureInEditor(lightTypeSettings.buildingLightType, light, flameParticleSystem);

            log.AddToLog(LogLevel.Debug, $"Configuring light on : {lightGameObject.name}...");
            ConfigureLight(lightGameObject, light, lightTypeSettings);
            log.AddToLog(LogLevel.Debug, $"Configuring light on : {lightGameObject.name}... DONE!");
        }

        private void ConfigureLight(GameObject lightGameObject, Light light, BuildingLightTypeSettings lightingTypeSettings)
        {
            log.AddToLog(LogLevel.Debug, $"Configuring light on : {lightGameObject.name}...");

            if (!lightingTypeSettings.presetSettings)
            {
                log.AddToLog(LogLevel.Error, $"No presets found on {lightingTypeSettings.buildingLightType}.");
            }

            // Look for old Lens Flare and destroy it - not supported in URP or HDRP
#if DAG_HDRP || DAG_URP
            if (lightGameObject.TryGetComponentInChildren(out LensFlare oldLensFlare, true))
            {
                DestroyImmediate(oldLensFlare);

                log.AddToLog(LogLevel.Debug, $"Destroyed old Lens Flare component on: {lightGameObject.name}.");
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

                log.AddToLog(LogLevel.Debug, $"Configured an SRP Lens Flare component to: {lightGameObject.name}.");
            }
#endif

            log.AddToLog(LogLevel.Debug, $"Setting light properties on: {lightGameObject.name}.");
            lightingTypeSettings.presetSettings.ApplyPreset(light);
        }

        private void ConfigureOnDemandShadowMap(Light light, BuildingLightTypeSettings lightingSettings)
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
    }
}