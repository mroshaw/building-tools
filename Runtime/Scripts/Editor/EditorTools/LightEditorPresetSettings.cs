using DaftAppleGames.Editor;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
#if DAG_HDRP
using UnityEngine.Rendering.HighDefinition;
using HDRenderingLayerMask = UnityEngine.Rendering.HighDefinition.RenderingLayerMask;
#endif
#if DAG_URP
using UnityEngine.Rendering.Universal;
#endif
using UnityEngine.Rendering;

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// Lighting presets to apply consistent settings to lights, independent of render pipeline
    /// </summary>
    [CreateAssetMenu(fileName = "LightEditorPresets", menuName = "Daft Apple Games/Building Tools/Light Editor Presets", order = 1)]
    public class LightEditorPresetSettings : EnhancedScriptableObject
    {
        public float range;
        public float intensity;
        public float radius;

#if DAG_HDRP
        public HDRenderingLayerMask renderingLayerMask;
#endif
#if DAG_URP
        public RenderingLayerMask renderingLayerMask;
#endif
#if DAG_BIRP
        public LayerMask cullingLayerMask;
#endif
        public Color filterColor;
        public float temperature;
        public LightmapBakeType lightmapBakeType;

        /// <summary>
        /// Given a Light object, applies the presets
        /// </summary>
        public void ApplyPreset(Light light)
        {
#if DAG_HDRP
            HDAdditionalLightData hdLightData = light.GetComponent<HDAdditionalLightData>();
            hdLightData.shapeRadius = radius;
            hdLightData.range = range;
            hdLightData.lightlayersMask = renderingLayerMask;
            float convertedIntensity = LightUnitUtils.ConvertIntensity(light, intensity, LightUnit.Lumen, light.lightUnit);
            light.intensity = convertedIntensity;
#endif

#if DAG_BIRP
            light.intensity = intensity;
            light.range = range;
#endif

#if DAG_URP
            UniversalAdditionalLightData urpLightData = light.GetComponent<UniversalAdditionalLightData>();
            // urpLightData.shapeRadius = radius;
            light.range = range;
            urpLightData.renderingLayers = renderingLayerMask;
            float convertedIntensity = LightUnitUtils.ConvertIntensity(light, intensity, LightUnit.Lumen, light.lightUnit);
            light.intensity = convertedIntensity;
#endif
            light.color = filterColor;
            light.useColorTemperature = true;
            light.colorTemperature = temperature;
            light.lightmapBakeType = lightmapBakeType;
        }
    }
}