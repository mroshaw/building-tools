using System;
using DaftAppleGames.Buildings;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using RenderingLayerMask = UnityEngine.RenderingLayerMask;
using HDRenderingLayerMask = UnityEngine.Rendering.HighDefinition.RenderingLayerMask;

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// Static methods for working with Lighting
    /// </summary>
    internal static class LightTools
    {
        /// <summary>
        /// This struct gives us an easy way of passing Lighting configuration around
        /// our various classes and methods
        /// </summary>
        [Serializable]
        public struct LightingSettings
        {
            public BuildingLightType buildingLightType;
            public string[] meshNames;
            public string[] flameNames;
            public float range;
            public float intensity;
            public float radius;
            public bool useLensFlare;
#if DAG_HDRP
            public HDRenderingLayerMask renderingLayerMask;
#endif
#if DAG_HDRP || DAG_URP
            public float lensFlareIntensity;
            public LensFlareDataSRP lensFlareData;
#endif
            public Color filterColor;
            public float temperature;
            public LightmapBakeType lightmapBakeType;
            public float shadowRefreshRate;
        }

        internal static void ConfigureLight(Light light, LightingSettings lightingSettings)
        {
#if DAG_HDRP
            HDAdditionalLightData hdLightData = light.GetComponent<HDAdditionalLightData>();
            hdLightData.shapeRadius = lightingSettings.radius;
            hdLightData.range = lightingSettings.range;
            hdLightData.lightlayersMask = lightingSettings.renderingLayerMask;
            float convertedIntensity = LightUnitUtils.ConvertIntensity(light, lightingSettings.intensity, LightUnit.Lumen, light.lightUnit);
            light.intensity = convertedIntensity;
#else
            light.range = lightingSettings.range;
#endif
            light.color = lightingSettings.filterColor;
            light.intensity = lightingSettings.intensity;
            light.useColorTemperature = true;
            light.colorTemperature = lightingSettings.temperature;
            light.lightmapBakeType = lightingSettings.lightmapBakeType;
        }
    }
}