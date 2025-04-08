using System.Collections.Generic;
using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using DaftAppleGames.Lighting;
using UnityEngine;
using UnityEngine.Rendering;
using RenderingLayerMask = UnityEngine.RenderingLayerMask;

#if DAG_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

#if DAG_URP
#endif

#if DAG_BIRP
#endif


namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// Used to assign specific Light Layers to lights and meshes
    /// </summary>
    public enum LightLayerMode
    {
        Interior,
        Exterior,
        Both
    }

    /// <summary>
    /// Static methods for working with Lighting
    /// </summary>
    internal static class LightTools
    {
        private static readonly Dictionary<LightLayerMode, uint> MeshLightLayerMasks;
#if DAG_HDRP
        private static readonly Dictionary<LightLayerMode, UnityEngine.Rendering.HighDefinition.RenderingLayerMask> HDRPLightLayerMasks;
#endif

        static LightTools()
        {
            MeshLightLayerMasks = new Dictionary<LightLayerMode, uint>
            {
                { LightLayerMode.Interior, RenderingLayerMask.GetMask("Interior") },
                { LightLayerMode.Exterior, RenderingLayerMask.GetMask("Exterior") },
                { LightLayerMode.Both, RenderingLayerMask.GetMask("Interior", "Exterior") }
            };

#if DAG_HDRP
            HDRPLightLayerMasks = new Dictionary<LightLayerMode, UnityEngine.Rendering.HighDefinition.RenderingLayerMask>
            {
                { LightLayerMode.Interior, UnityEngine.Rendering.HighDefinition.RenderingLayerMask.RenderingLayer3 },
                { LightLayerMode.Exterior, UnityEngine.Rendering.HighDefinition.RenderingLayerMask.RenderingLayer4 },
                {
                    LightLayerMode.Both,
                    UnityEngine.Rendering.HighDefinition.RenderingLayerMask.RenderingLayer3 | UnityEngine.Rendering.HighDefinition.RenderingLayerMask.RenderingLayer4
                }
            };


#endif
        }

        /// <summary>
        /// Gets the LayerMask defined by the mode
        /// </summary>
        internal static RenderingLayerMask GetMaskByMode(LightLayerMode layerMode)
        {
            return MeshLightLayerMasks[layerMode];
        }

#if DAG_HDRP

        private static UnityEngine.Rendering.HighDefinition.RenderingLayerMask GetHDRPMaskByMode(LightLayerMode layerMode)
        {
            return HDRPLightLayerMasks[layerMode];
        }

#endif

        #region Tool prarameter structs

        /// <summary>
        /// Struct to consolidate parameters for Lighting config
        /// </summary>
        internal struct LightConfigParameters
        {
            internal BuildingLightType BuildingLightType;
            internal string[] MeshNames;
            internal string[] FlameNames;
            internal float Range;
            internal float Intensity;
            internal float Radius;
            internal LightLayerMode LayerMode;
        }

        #endregion

        #region Configure Lights methoods

        /// <summary>
        /// Sets the LightLayer on all child meshes
        /// </summary>
        public static void SetLightLayers(GameObject[] parentGameObjects, LightLayerMode lightLayerMode, EditorLog log)
        {
            foreach (GameObject parentGameObject in parentGameObjects)
            {
                foreach (MeshRenderer renderer in parentGameObject.GetComponentsInChildren<MeshRenderer>())
                {
                    renderer.renderingLayerMask = GetMaskByMode(lightLayerMode);
                }
            }
        }

        public static void ConfigureLight(GameObject lightGameObject, Light light, LightingSettings lightingSettings, EditorLog log)
        {
            // Look for old Lens Flare and destroy it - not supported in URP or HDRP
#if DAG_HDRP || DAG_URP
            if (lightGameObject.TryGetComponentInChildren(out LensFlare oldLensFlare, true))
            {
                Object.DestroyImmediate(oldLensFlare);

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
            UpdateLightProperties(light, lightingSettings, log);
        }

        private static void UpdateLightProperties(Light light, LightingSettings lightingSettings, EditorLog log)
        {
#if DAG_HDRP
            HDAdditionalLightData hdLightData = light.GetComponent<HDAdditionalLightData>();
            hdLightData.shapeRadius = lightingSettings.radius;
            hdLightData.range = lightingSettings.range;
#else
            light.range = lightingSettings.range;
#endif
            light.color = lightingSettings.filterColor;
            light.intensity = lightingSettings.intensity;
            light.useColorTemperature = true;
            light.colorTemperature = lightingSettings.temperature;
            hdLightData.lightlayersMask = GetHDRPMaskByMode(lightingSettings.layerMode);

            float convertedIntensity = LightUnitUtils.ConvertIntensity(light, lightingSettings.intensity, LightUnit.Lumen, light.lightUnit);
            light.intensity = convertedIntensity;

            light.lightmapBakeType = lightingSettings.lightmapBakeType;

            // Add a ShadowMap manager
            hdLightData.shadowUpdateMode = ShadowUpdateMode.OnDemand;
            OnDemandShadowMapUpdate shadowMapUpdate = light.EnsureComponent<OnDemandShadowMapUpdate>();
            shadowMapUpdate.counterMode = CounterMode.Frames;
            shadowMapUpdate.shadowMapToRefresh = ShadowMapToRefresh.EntireShadowMap;
            shadowMapUpdate.fullShadowMapRefreshWaitSeconds = lightingSettings.shadowRefreshRate;
        }

        #endregion
    }
}