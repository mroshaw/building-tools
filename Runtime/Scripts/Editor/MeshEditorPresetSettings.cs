using UnityEditor;
using UnityEngine;
using RenderingLayerMask = UnityEngine.RenderingLayerMask;

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
    [CreateAssetMenu(fileName = "MeshEditorPresets", menuName = "Daft Apple Games/Building Tools/Mesh Editor Presets", order = 1)]
    public class MeshEditorPresetSettings : ScriptableObject
    {
        [BoxGroup("Game Object")] public string layerName;
        [BoxGroup("Game Object")] public StaticEditorFlags staticEditorFlags;

        [BoxGroup("Lighting")] public ShadowCastingMode shadowCastingMode;
        [BoxGroup("Lighting")] public bool staticShadowCaster;
        [BoxGroup("Lighting")] public ReceiveGI receiveGI;
        [BoxGroup("Lighting")] public bool contributeGI;
        [BoxGroup("Lighting")] public LightProbeUsage lightProbeUsage;

        [BoxGroup("Light Mapping")] public float scaleInLightmap;

        [BoxGroup("Additional")] public MotionVectorGenerationMode motionVectors;
        [BoxGroup("Additional")] public bool dynamicOcclusion;
        [BoxGroup("Additional")] public RenderingLayerMask renderingLayerMask;
        [BoxGroup("Additional")] public int priority;

        /// <summary>
        /// Given a Mesh Renderer object, applies the presets
        /// </summary>
        private void ApplyPreset(MeshRenderer meshRenderer)
        {
            meshRenderer.shadowCastingMode = shadowCastingMode;
            meshRenderer.staticShadowCaster = staticShadowCaster;
            meshRenderer.receiveGI = receiveGI;
            meshRenderer.renderingLayerMask = renderingLayerMask;
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
                log.Log(LogLevel.Debug, $"Configuring mesh on: {renderer.gameObject.name}.");
                ApplyPreset(renderer);
            }

            // Set the layer
            parentGameObject.layer = LayerMask.NameToLayer(layerName);

            // Update the static flags, based on whether ConfigureGI is true or false
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(parentGameObject);
            GameObjectUtility.SetStaticEditorFlags(parentGameObject,
                contributeGI ? flags | StaticEditorFlags.ContributeGI : flags & ~StaticEditorFlags.ContributeGI);
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