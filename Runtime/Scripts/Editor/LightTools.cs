using System.Collections.Generic;
using DaftAppleGames.Darskerry.Core.Buildings;
using UnityEngine;
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
    public enum LightLayerMode { Interior, Exterior, Both }

    /// <summary>
    /// Static methods for working with Lighting
    /// </summary>
    internal static class LightTools
    {
        internal static Dictionary<LightLayerMode, uint> lightLayerMasks;

        static LightTools()
        {
            lightLayerMasks = new Dictionary<LightLayerMode, uint>
            {
                { LightLayerMode.Interior, RenderingLayerMask.GetMask("Exterior") },
                { LightLayerMode.Exterior, RenderingLayerMask.GetMask("Exterior") },
                { LightLayerMode.Both, RenderingLayerMask.GetMask("Interior", "Exterior") }
            };
        }

        /// <summary>
        /// Gets the LayerMask defined by the mode
        /// </summary>
        internal static RenderingLayerMask GetMaskByMode(LightLayerMode layerMode)
        {
            return lightLayerMasks[layerMode];
        }

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
        /// Validate the CombineMeshParameters
        /// </summary>
        internal static bool ValidateConfigureLightParameters(LightConfigParameters lightingParameters)
        {
            return true;
        }

        internal static void ConfigureLight(Light light, LightConfigParameters lightingParameters)
        {
#if DAG_HDRP
            HDAdditionalLightData hdLightData = light.GetComponent<HDAdditionalLightData>();

            hdLightData.range = lightingParameters.Range;
            light.intensity = lightingParameters.Intensity;
            // hdLightData.lightlayersMask = GetMaskByMode(lightingParameters.LayerMode);
#endif

#if DAG_URP

#endif

#if DAG_BIRP

#endif
        }

    #endregion

    }
}