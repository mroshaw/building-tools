using System;
using System.Collections.Generic;
using System.IO;
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
        [Tooltip("The type of light to which these settings apply. You should only have one instance of each light type in a given tool configuration.")]
        public BuildingLightType buildingLightType;

        [Tooltip("Game Objects that contain any of these strings will be configured as Lights by the tool.")] public string[] meshNames;

        [Tooltip(
            "Game Objects that contain any of these strings will be configured as flames by the tool. This is then used to toggle flames on and off along with the light itself.")]
        public string[] flameNames;

        [Tooltip("Determines whether Lens Flare components should be added and configured for this light type.")] public bool useLensFlare;
#if DAG_HDRP
        [Tooltip(
            "If true, an `OnDemandShadowMapUpdate` component will be added to the light. This can be configured to update the shadow on the light every n frames or m seconds. This can be a significant performance improvement if you are currently updating shadows every frame.")]
        public bool addOnDemandShadowMapComponent;

        [Tooltip("Sets the number of frames to wait before refreshing the shadows of a light, driven by the new `OnDemandShadowUpdate` component.")] public int shadowRefreshRate;
#endif

#if DAG_HDRP || DAG_URP

        [Tooltip("The intensity of the lens flare, if one is added.")] public float lensFlareIntensity;
        [Tooltip("SRP Lens Flare configuration for the lens flare, if one is added.")] public LensFlareDataSRP lensFlareData;
#endif
        [Tooltip("Light properties to be applied to each light.")] [InlineEditor] public LightEditorPresetSettings presetSettings;
    }

    [CreateAssetMenu(fileName = "ConfigureLightingEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Lighting Tool")]
    internal class SetUpLightsEditorTool : BuildingEditorTool
    {
        [SerializeField] [BoxGroup("Settings")] internal BuildingLightTypeSettings indoorCandleTypeSettings;
        [SerializeField] [BoxGroup("Settings")] internal BuildingLightTypeSettings indoorFireTypeSettings;
        [SerializeField] [BoxGroup("Settings")] internal BuildingLightTypeSettings outdoorBuildingLightTypeSettings;

        [SerializeField] [BoxGroup("Settings")] internal bool addLightManager = true;
        [SerializeField] [BoxGroup("Settings")] internal string lightManagerGameObjectName = "Building Light Manager";

        protected override string GetToolName()
        {
            return "Set Up Lights";
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

            if (!HasLights(out string lightsValidationReason))
            {
                cannotRunReasons.Add(lightsValidationReason);
                return false;
            }

            if (!HasRequiredBuildingComponent(out string requiredBuildingReason))
            {
                cannotRunReasons.Add(requiredBuildingReason);
                canRun = false;
            }

            return canRun;
        }

        /// <summary>
        /// Look for any lights, so we can skip this tool if there are none
        /// </summary>
        private bool HasLights(out string validationReason)
        {
            Light[] allLights = SelectedGameObject.GetComponentsInChildren<Light>(true);
            if (allLights.Length > 0)
            {
                validationReason = string.Empty;
                return true;
            }

            validationReason = "No Light components were found on the selected Game Object!";
            return false;
        }

        /// <summary>
        /// Save the associated Light Settings too
        /// </summary>
        public override EnhancedScriptableObject SaveCopy(string pathToSave, string fileName, string childFolder)
        {
            SetUpLightsEditorTool newTool = base.SaveCopy(pathToSave, fileName, childFolder) as SetUpLightsEditorTool;

            if (!newTool)
            {
                return this;
            }

            string newBaseFolder = Path.Combine(pathToSave, childFolder);

            LightEditorPresetSettings newCandleSettings =
                indoorCandleTypeSettings.presetSettings.SaveCopy(newBaseFolder, string.Empty, "Light Settings") as LightEditorPresetSettings;
            LightEditorPresetSettings newFireSettings = indoorFireTypeSettings.presetSettings.SaveCopy(newBaseFolder, string.Empty, "Light Settings") as LightEditorPresetSettings;
            LightEditorPresetSettings newOutdoorSettings =
                outdoorBuildingLightTypeSettings.presetSettings.SaveCopy(newBaseFolder, string.Empty, "Light Settings") as LightEditorPresetSettings;

            newTool.indoorFireTypeSettings.presetSettings = newCandleSettings;
            newTool.indoorFireTypeSettings.presetSettings = newFireSettings;
            newTool.outdoorBuildingLightTypeSettings.presetSettings = newOutdoorSettings;

            return newTool;
        }

        protected override void RunTool(string undoGroupName)
        {
            ConfigureLighting();
        }

        private void ConfigureLighting()
        {
            BuildingLightController buildingLightController = SelectedGameObject.EnsureComponent<BuildingLightController>();
            log.AddToLog(LogLevel.Info, $"Added Lighting Controller component to {SelectedGameObject.name}.");

            Transform[] allChildren = SelectedGameObject.GetComponentsInChildren<Transform>(true);

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

            buildingLightController.UpdateLightLists();

            if (addLightManager)
            {
                log.AddToLog(LogLevel.Debug, $"Adding light manager...");
                AddLightManager();
                log.AddToLog(LogLevel.Debug, $"Adding light manager... DONE!");
            }
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
            buildingLight.ConfigureLight(lightTypeSettings.buildingLightType, light, flameParticleSystem);

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

#if DAG_HDRP
            if (lightingTypeSettings.addOnDemandShadowMapComponent)
            {
                log.AddToLog(LogLevel.Debug, "Adding OnDemandShadowMapUpdate component...");
                ConfigureOnDemandShadowMap(light, lightingTypeSettings);
            }
#endif
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
            shadowMapUpdate.fullShadowMapRefreshWaitFrames = lightingSettings.shadowRefreshRate;

#endif
        }

        private void AddLightManager()
        {
            BuildingLightManager lightManager = FindFirstObjectByType<BuildingLightManager>();
            if (lightManager)
            {
                log.AddToLog(LogLevel.Info, $"There is already a Light Manager in the scene.");
                return;
            }

            GameObject newLightManagerGameObject = new(lightManagerGameObjectName);
            BuildingLightManager newLightManager = newLightManagerGameObject.AddComponent<BuildingLightManager>();
            newLightManager.RefreshLightControllers();
        }
    }
}