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

        internal static UnityEngine.Rendering.HighDefinition.RenderingLayerMask GetHDRPMaskByMode(LightLayerMode layerMode)
        {
            return HDRPLightLayerMasks[layerMode];
        }

#endif
    }
}