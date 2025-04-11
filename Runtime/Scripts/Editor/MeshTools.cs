using System;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// Static methods for working with Meshes
    /// </summary>
    internal static class MeshTools
    {
        #region Mesh Settings

        /// <summary>
        /// Struct to give us flexibility it configuring Meshes for different use cases
        /// </summary>
        [Serializable]
        public struct MeshSettings
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
        }

        #endregion

        #region Configure Mesh methoods

        /// <summary>
        /// Applies the Mesh Settings to the given Mesh Renderer
        /// </summary>
        private static void ConfigureMeshRenderer(MeshRenderer meshRenderer, MeshSettings meshSettings)
        {
            meshRenderer.shadowCastingMode = meshSettings.shadowCastingMode;
            meshRenderer.staticShadowCaster = meshSettings.staticShadowCaster;
            meshRenderer.receiveGI = meshSettings.receiveGI;
            meshRenderer.renderingLayerMask = meshSettings.renderingLayerMask;
            meshRenderer.lightProbeUsage = meshSettings.lightProbeUsage;
            meshRenderer.scaleInLightmap = meshSettings.scaleInLightmap;
            meshRenderer.motionVectorGenerationMode = meshSettings.motionVectors;
            meshRenderer.allowOcclusionWhenDynamic = meshSettings.dynamicOcclusion;
            meshRenderer.rendererPriority = meshSettings.priority;
        }

        /// <summary>
        /// Applies the Mesh settings to all Meshes on the given Game Object
        /// </summary>
        internal static void ConfigureMeshOnGameObject(GameObject parentGameObject, MeshSettings meshSettings, EditorLog log)
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
                ConfigureMeshRenderer(renderer, meshSettings);
            }

            // Set the layer
            parentGameObject.layer = LayerMask.NameToLayer(meshSettings.layerName);

            // Update the static flags, based on whether ConfigureGI is true or false
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(parentGameObject);
            GameObjectUtility.SetStaticEditorFlags(parentGameObject,
                meshSettings.contributeGI ? flags | StaticEditorFlags.ContributeGI : flags & ~StaticEditorFlags.ContributeGI);
        }

        /// <summary>
        /// Apply Mesh Settings to all GameObjects in parent
        /// </summary>
        internal static void ConfigureMeshOnAllGameObjects(GameObject[] parentGameObjects, MeshSettings meshSettings, EditorLog log)
        {
            foreach (GameObject gameObject in parentGameObjects)
            {
                foreach (Transform transform in gameObject.GetComponentsInChildren<Transform>(true))
                {
                    ConfigureMeshOnGameObject(transform.gameObject, meshSettings, log);
                }
            }
        }

        #endregion

        #region Volume helper methods

        /// <summary>
        /// Returns the size of a box that encloses the meshes in the Game Object
        /// </summary>
        internal static void GetMeshSize(GameObject parentGameObject, LayerMask includeLayerMask, string[] ignoreNames, out Vector3 meshSize, out Vector3 meshCenter)
        {
            Bounds meshBounds = GetMeshBounds(parentGameObject, includeLayerMask, ignoreNames);
            meshSize = meshBounds.size;
            meshCenter = meshBounds.center;
        }

        /// <summary>
        /// Return the bounds of the enclosing meshes in the Game Object
        /// </summary>
        private static Bounds GetMeshBounds(GameObject parentGameObject, LayerMask includeLayerMask, string[] ignoreNames)
        {
            Bounds combinedBounds = new(Vector3.zero, Vector3.zero);
            bool hasValidRenderer = false;

            foreach (MeshRenderer childRenderer in parentGameObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                if ((includeLayerMask & (1 << childRenderer.gameObject.layer)) == 0 ||
                    (ignoreNames.Length != 0 && ignoreNames.ItemInString(childRenderer.gameObject.name)))
                {
                    continue;
                }

                Bounds meshBounds = childRenderer.localBounds;

                // Initialize or expand the combined bounds
                if (!hasValidRenderer)
                {
                    combinedBounds = meshBounds;
                    hasValidRenderer = true;
                }
                else
                {
                    combinedBounds.Encapsulate(meshBounds);
                }
            }

            return combinedBounds;
        }

        #endregion
    }
}