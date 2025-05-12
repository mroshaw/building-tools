using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEditor;
using UnityEngine;
using RenderingLayerMask = UnityEngine.RenderingLayerMask;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

using UnityEngine.Rendering;

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// Mesh presets to apply consistent settings to MehRenderers, independent of render pipeline
    /// </summary>
    [CreateAssetMenu(fileName = "MeshEditorPresets", menuName = "Daft Apple Games/Building Tools/Mesh Editor Presets", order = 1)]
    public class MeshEditorPresetSettings : EnhancedScriptableObject
    {
        [BoxGroup("Game Object")] public string layerName;
        [BoxGroup("Game Object")] public StaticEditorFlags staticEditorFlags;
        [BoxGroup("Game Object")] public StaticEditorFlags dynamicMeshStaticFlags;

        [BoxGroup("Lighting")] public ShadowCastingMode shadowCastingMode;
        [BoxGroup("Lighting")] public bool staticShadowCaster;
        [BoxGroup("Lighting")] public ReceiveGI receiveGI;
        [BoxGroup("Lighting")] public LightProbeUsage lightProbeUsage;

        [BoxGroup("Light Mapping")] public float scaleInLightmap;

        [BoxGroup("Additional")] public MotionVectorGenerationMode motionVectors;
        [BoxGroup("Additional")] public bool dynamicOcclusion;

#if DAG_HDRP || DAG_URP
        [BoxGroup("Additional")] public RenderingLayerMask renderingLayerMask;
#endif
        [BoxGroup("Additional")] public int priority;

        /// <summary>
        /// Given a Mesh Renderer object, applies the presets
        /// </summary>
        private void ApplyPreset(MeshRenderer meshRenderer)
        {
            meshRenderer.gameObject.layer = LayerMask.NameToLayer(layerName);
            meshRenderer.shadowCastingMode = shadowCastingMode;
            meshRenderer.staticShadowCaster = staticShadowCaster;
            meshRenderer.receiveGI = receiveGI;
#if DAG_HDRP || DAG_URP
            meshRenderer.renderingLayerMask = renderingLayerMask;
#endif
            meshRenderer.lightProbeUsage = lightProbeUsage;
            meshRenderer.scaleInLightmap = scaleInLightmap;
            meshRenderer.motionVectorGenerationMode = motionVectors;
            meshRenderer.allowOcclusionWhenDynamic = dynamicOcclusion;
            meshRenderer.rendererPriority = priority;
        }

        /// <summary>
        /// Applies the Mesh settings to all Meshes on the given Game Object
        /// </summary>
        internal void ConfigureMeshOnGameObject(GameObject parentGameObject, EditorLog log)
        {
            MeshRenderer[] meshRenderers = parentGameObject.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers.Length == 0)
            {
                return;
            }

            foreach (MeshRenderer renderer in meshRenderers)
            {
                // Apply Mesh Renderer settings
                log.AddToLog(LogLevel.Debug, $"Configuring mesh on: {renderer.gameObject.name}.");
                ApplyPreset(renderer);
            }

            // Set the layer
            parentGameObject.layer = LayerMask.NameToLayer(layerName);

            // Update the static flags
            GameObjectUtility.SetStaticEditorFlags(parentGameObject, parentGameObject.HasComponent<DynamicMeshRenderer>() ? dynamicMeshStaticFlags : staticEditorFlags);
        }

        /// <summary>
        /// Apply Mesh Settings to all GameObjects in parent
        /// </summary>
        internal void ConfigureMeshOnAllGameObjects(GameObject[] parentGameObjects, EditorLog log)
        {
            foreach (GameObject gameObject in parentGameObjects)
            {
                foreach (Transform transform in gameObject.GetComponentsInChildren<Transform>(true))
                {
                    ConfigureMeshOnGameObject(transform.gameObject, log);
                }
            }
        }
    }
}