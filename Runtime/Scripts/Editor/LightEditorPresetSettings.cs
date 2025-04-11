using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
#if DAG_HDRP
using HDRenderingLayerMask = UnityEngine.Rendering.HighDefinition.RenderingLayerMask;
using UnityEngine.Rendering.HighDefinition;
#else
using RenderingLayerMask = UnityEngine.RenderingLayerMask;
#endif

#if DAG_HDRP || DAG_URP
using UnityEngine.Rendering;
#endif

namespace DaftAppleGames.Editor
{
    /// <summary>
    /// Lighting presets to apply consistent settings to lights, independent of render pipeline
    /// </summary>
    [CreateAssetMenu(fileName = "LightEditorPresets", menuName = "Daft Apple Games/Building Tools/Light Editor Presets", order = 1)]
    public class LightEditorPresetSettings : ScriptableObject
    {
        public float range;
        public float intensity;
        public float radius;

#if DAG_HDRP
        public HDRenderingLayerMask renderingLayerMask;
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
#else
            light.intensity = intensity;
            light.range = lightingSettings.range;
#endif
            light.color = filterColor;
            light.useColorTemperature = true;
            light.colorTemperature = temperature;
            light.lightmapBakeType = lightmapBakeType;
        }
    }
}