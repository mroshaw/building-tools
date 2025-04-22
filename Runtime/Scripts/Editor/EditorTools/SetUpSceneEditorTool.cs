using System.Collections.Generic;
using DaftAppleGames.Editor;
using UnityEngine;
#if DAG_HDRP
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
#if DAG_HDRP || DAG_URP
        [SerializeField] [BoxGroup("Settings")] internal RenderingLayerMask directionLightRenderingLayerMask;
#endif

#if DAG_BIRP
        [SerializeField] [BoxGroup("Settings")] internal LayerMask directionLightCullingLayerMask;
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

        protected override bool CanRunTool(GameObject selectedGameObject, out List<string> cannotRunReasons)
        {
            cannotRunReasons = new List<string>();
            return true;
        }

        /// <summary>
        /// Implementation of the "Set Up" tool
        /// </summary>
        protected override void RunTool(GameObject selectedGameObject, string undoGroupName)
        {
            log.AddToLog(LogLevel.Debug, "Configuring Directional Light in scene, if there is one...");
            if (GetDirectionalLight(out Light directionalLight))
            {
                log.AddToLog(LogLevel.Debug, $"Found {directionalLight.name}...");
                ConfigureDirectionalLight(directionalLight);
            }

            log.AddToLog(LogLevel.Debug, "Configuring Directional Light... DONE.");
        }

        private void ConfigureDirectionalLight(Light directionalLight)
        {
#if DAG_HDRP
            log.AddToLog(LogLevel.Debug, "Configuring HDRP light...");
            HDAdditionalLightData hdLightData = directionalLight.GetComponent<HDAdditionalLightData>();
            hdLightData.lightlayersMask = directionLightRenderingLayerMask;
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
        private bool GetDirectionalLight(out Light directionalLight)
        {
            Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);

            foreach (Light light in allLights)
            {
                if (light.type == LightType.Directional)
                {
                    log.AddToLog(LogLevel.Debug, $"Directional Light found: {light.name} at position {light.transform.position}");
                    directionalLight = light;
                    return true;
                }
            }

            directionalLight = null;
            return false;
        }
    }
}