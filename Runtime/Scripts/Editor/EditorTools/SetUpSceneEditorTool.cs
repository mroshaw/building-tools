using System.Collections.Generic;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEngine;
#if DAG_HDRP
using DaftAppleGames.Lighting;
using UnityEngine.Rendering.HighDefinition;
using RenderingLayerMask = UnityEngine.Rendering.HighDefinition.RenderingLayerMask;
#endif

#if DAG_URP
using UnityEngine.Rendering.Universal;
#endif

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// Configures scene - for example, configures the Directional Light
    /// </summary>
    [CreateAssetMenu(fileName = "SetUpSceneEditorTool", menuName = "Daft Apple Games/Building Tools/Set Up Scene Tool")]
    internal class SetUpSceneEditorTool : BuildingEditorTool
    {
#if DAG_HDRP
        [SerializeField] [BoxGroup("Settings")]
        [Tooltip(
            "If true, an `OnDemandShadowMapUpdate` component will be added to your main Directional Light. This can be configured to update the shadow on the light every n frames or m seconds. This can be a significant performance improvement if you are currently updating shadows every frame.")]
        internal bool addOnDemandShadowMapComponent;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Sets the number of frames to wait before refreshing the shadows of a light, driven by the new `OnDemandShadowUpdate` component.")] internal int shadowRefreshRate;
#endif
#if DAG_HDRP || DAG_URP
        [SerializeField] [BoxGroup("Settings")]
        [Tooltip(
            "Determines which meshes are influence by the main directional light, driven by \"Rendering Layers\", which is a concept only supported in the \"Scriptable\" rendering pipelines.")]
        internal RenderingLayerMask directionLightRenderingLayerMask;
#endif

#if DAG_BIRP
        [SerializeField] [BoxGroup("Settings")] [Tooltip("Determines which meshes are influence by the main directional light, driven by the mesh renderer Game Object layers.")] internal LayerMask directionLightCullingLayerMask;
#endif
        protected override string GetToolName()
        {
            return "Set Up Scene";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(out List<string> cannotRunReasons)
        {
            cannotRunReasons = new List<string>();
            return true;
        }

        /// <summary>
        /// Implementation of the "Set Up" tool
        /// </summary>
        protected override void RunTool(string undoGroupName)
        {
            log.AddToLog(LogLevel.Debug, "Configuring Directional Light in scene, if there is one...");
            if (GetDirectionalLights(out List<Light> directionalLights))
            {
                foreach (Light directionalLight in directionalLights)
                {
                    log.AddToLog(LogLevel.Debug, $"Configuring {directionalLight.name}...");
                    ConfigureDirectionalLight(directionalLight);
                }
            }

            log.AddToLog(LogLevel.Debug, "Configuring Directional Light... DONE.");
        }

        private void ConfigureDirectionalLight(Light directionalLight)
        {
#if DAG_HDRP
            log.AddToLog(LogLevel.Debug, "Configuring HDRP light...");
            HDAdditionalLightData hdLightData = directionalLight.GetComponent<HDAdditionalLightData>();
            hdLightData.lightlayersMask = directionLightRenderingLayerMask;

            if (addOnDemandShadowMapComponent)
            {
                log.AddToLog(LogLevel.Debug, "Adding OnDemandShadowMapUpdate component...");
                hdLightData.shadowUpdateMode = ShadowUpdateMode.OnDemand;
                OnDemandShadowMapUpdate shadowMapUpdate = directionalLight.EnsureComponent<OnDemandShadowMapUpdate>();
                shadowMapUpdate.fullShadowMapRefreshWaitFrames = shadowRefreshRate;
                shadowMapUpdate.counterMode = CounterMode.Frames;
                shadowMapUpdate.shadowMapToRefresh = ShadowMapToRefresh.EntireShadowMap;
            }
#endif

#if DAG_URP
            log.AddToLog(LogLevel.Debug, "Configuring URP light...");
            UniversalAdditionalLightData urpLightData = directionalLight.GetComponent<UniversalAdditionalLightData>();
            urpLightData.renderingLayers = directionLightRenderingLayerMask;
#endif

#if DAG_BIRP
            log.AddToLog(LogLevel.Debug, "Configuring BIRP light...");
            directionalLight.cullingMask = directionLightCullingLayerMask;
#endif
        }

        /// <summary>
        /// Searches for a Directional Light in the scene
        /// </summary>
        private bool GetDirectionalLights(out List<Light> directionalLights)
        {
            directionalLights = new List<Light>();

            Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);

            foreach (Light light in allLights)
            {
                if (light.type == LightType.Directional)
                {
                    log.AddToLog(LogLevel.Debug, $"Directional Light found: {light.name} at position {light.transform.position}");
                    directionalLights.Add(light);
                }
            }

            return directionalLights.Count > 0;
        }
    }
}