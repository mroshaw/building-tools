/*
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using DaftAppleGames.Darskerry.Core.Buildings;
using DaftAppleGames.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DaftAppleGames.Editor.Mesh
{
    public enum MeshProperties { CastShadows, StaticShadowCaster, ContributeGI, ReceiveGI, MotionVectors, DynamicOcclusion, RenderLayerMask, Priority }
    public enum LightLayerPresets { Interior, Exterior, Both, None }

    public class MeshToolEditorWindow : OdinEditorWindow
    {
        [MenuItem("Daft Apple Games/Meshes/Mesh Tool")]
        public static void ShowWindow()
        {
            GetWindow(typeof(MeshToolEditorWindow));
        }

        [PropertyOrder(1)]
        [BoxGroup("Selected Objects")]
        [SerializeField]
        private GameObject[] selectedObjects;

        [BoxGroup("Tool Settings")] [SerializeField] private bool showDebug = true;

        private void WriteLog(string logMessage)
        {
            if (showDebug)
            {
                Debug.Log(logMessage);
            }
        }

        /// <summary>
        /// Refresh the list of GameObjects selected
        /// </summary>
        private void OnSelectionChange()
        {
            selectedObjects = Selection.gameObjects;
        }


        [PropertyOrder(3)]
        [BoxGroup("Lighting Settings")]
        [SerializeField]
        private ShadowCastingMode shadowCastingMode = ShadowCastingMode.Off;

        [PropertyOrder(3)]
        [BoxGroup("Lighting Settings")]
        [Button("Apply Shadow Casting")]
        private void ApplyContributeGi()
        {
            ApplyIndividualSetting(MeshProperties.CastShadows);
        }

        [PropertyOrder(4)]
        [BoxGroup("Lighting Settings")]
        [SerializeField]
        private bool staticShadowCaster = true;

        [PropertyOrder(4)]
        [BoxGroup("Lighting Settings")]
        [Button("Apply Static Shadow Caster")]
        private void ApplyStaticShadowCaster()
        {
            ApplyIndividualSetting(MeshProperties.StaticShadowCaster);
        }

        [PropertyOrder(5)]
        [BoxGroup("Lighting Settings")]
        [SerializeField]
        private bool contributeGI = true;

        [PropertyOrder(5)]
        [BoxGroup("Lighting Settings")]
        [Button("Apply Contribute GI")]
        private void ApplyContributeGI()
        {
            ApplyIndividualSetting(MeshProperties.ContributeGI);
        }

        [PropertyOrder(6)]
        [BoxGroup("Lighting Settings")]
        [SerializeField]
        private ReceiveGI receiveGI = ReceiveGI.LightProbes;

        [PropertyOrder(6)]
        [BoxGroup("Lighting Settings")]
        [Button("Apply Receive GI")]
        private void ApplyReceiveGi()
        {
            ApplyIndividualSetting(MeshProperties.ReceiveGI);
        }

        [PropertyOrder(7)][BoxGroup("Lighting Settings")][SerializeField] private LightLayerPresets lightlayerPreset;
        [PropertyOrder(7)]
        [BoxGroup("Lighting Settings")]
        [Button("Apply Render Layers")]
        private void ApplyRenderLayer()
        {
            ApplyIndividualSetting(MeshProperties.RenderLayerMask);
        }


        [PropertyOrder(8)]
        [BoxGroup("Lighting Settings")]
        [Button("Set Shadows on LODS")]
        private void SetShadowsOnLods()
        {
            foreach (GameObject gameObject in Selection.gameObjects)
            {
                AdjustLODShadows(gameObject);
            }

            SavePrefabChanges();
        }


        [PropertyOrder(8)][BoxGroup("LOD Fixer")][SerializeField] private LodGroupSettingsSO lodGroupSettings;
        [PropertyOrder(8)]
        [Button("Fix LOD Groups")]
        public void FixLodGroups()
        {
            foreach (GameObject gameObject in Selection.gameObjects)
            {
                ConfigureLodGroup(gameObject);
            }
            SavePrefabChanges();
        }


        private void ConfigureLodGroup(GameObject gameObject)
        {
            LODGroup lodGroup = gameObject.GetComponent<LODGroup>();
            if (!lodGroup)
            {
                Debug.LogError($"There is no LODGroup on {gameObject.name}");
                return;
            }
            lodGroupSettings.ConfigureLodGroup(lodGroup);
            SavePrefabChanges();
        }

        private void ApplyIndividualSetting(MeshProperties meshProperties)
        {
            foreach (Renderer renderer in GetChildRenderers())
            {
                WriteLog($"Updating '{meshProperties}' on '{renderer.gameObject.name}'");
                Undo.RegisterCompleteObjectUndo(renderer.gameObject, $"Update {meshProperties} on '{renderer.gameObject.name}'");
                switch (meshProperties)
                {
                    case MeshProperties.CastShadows:
                        renderer.shadowCastingMode = shadowCastingMode;
                        break;
                    case MeshProperties.StaticShadowCaster:
                        renderer.staticShadowCaster = staticShadowCaster;
                        break;
                    case MeshProperties.ContributeGI:
                        StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
                        StaticEditorFlags newFlags;
                        if (contributeGI)
                        {
                            newFlags = flags | StaticEditorFlags.ContributeGI;
                        }
                        else
                        {
                            newFlags = flags & ~StaticEditorFlags.ContributeGI;
                        }
                        GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, newFlags);
                        break;
                    case MeshProperties.ReceiveGI:
                        if (renderer is MeshRenderer meshRenderer)
                        {
                            meshRenderer.receiveGI = receiveGI;
                        }

                        break;
                    case MeshProperties.RenderLayerMask:
                        switch (lightlayerPreset)
                        {
                            case LightLayerPresets.Interior:
                                renderer.renderingLayerMask = RenderingLayerMask.GetMask("Interior");
                                break;

                            case LightLayerPresets.Exterior:
                                renderer.renderingLayerMask = RenderingLayerMask.GetMask("Exterior");
                                break;

                            case LightLayerPresets.Both:
                                renderer.renderingLayerMask = RenderingLayerMask.GetMask("Interior", "Exterior");
                                break;
                        }

                        break;
                }
            }
            SavePrefabChanges();
        }

        /// <summary>
        /// Return all MeshRenderers in selected GameObjects
        /// </summary>
        /// <returns></returns>
        private List<Renderer> GetChildRenderers()
        {
            List<Renderer> allRenderers = new List<Renderer>();

            foreach (GameObject gameObject in Selection.gameObjects)
            {
                // Check if prefab
                allRenderers.AddRange(gameObject.GetComponentsInChildren<Renderer>(true));
            }

            return allRenderers;
        }

        /// <summary>
        /// If any of the Selection is a Prefab, mark as dirty and force a save
        /// </summary>
        private void SavePrefabChanges()
        {
            foreach (GameObject gameObject in Selection.gameObjects)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(gameObject))
                {
                    UnityEditor.EditorUtility.SetDirty(gameObject);
                    AssetDatabase.SaveAssetIfDirty(gameObject);
                }
            }
        }

        // Call this method to adjust shadows on all LODGroups within the target object
        public void AdjustLODShadows(GameObject targetGameObject)
        {
            // Get all LODGroup components in the target object and its children
            LODGroup[] lodGroups = targetGameObject.GetComponentsInChildren<LODGroup>();

            // Loop through each LODGroup
            foreach (LODGroup lodGroup in lodGroups)
            {
                // Get the LODs array from the LODGroup
                LOD[] lods = lodGroup.GetLODs();

                // Loop through each LOD in the LODGroup
                for (int i = 0; i < lods.Length; i++)
                {
                    // Get all renderers for the current LOD
                    Renderer[] renderers = lods[i].renderers;

                    // For each renderer, adjust the Cast Shadows property
                    foreach (Renderer renderer in renderers)
                    {
                        if (renderer is MeshRenderer meshRenderer)
                        {
                            if (i == 0)
                            {
                                // Set Cast Shadows to true for LOD0
                                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                            }
                            else
                            {
                                // Set Cast Shadows to false for all other LODs
                                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                            }
                        }
                    }
                }
            }
        }


        [PropertyOrder(9)] [BoxGroup("Combine Meshes")] [SerializeField] private string outputPath = "Assets/_Project/Meshes/";
        [PropertyOrder(9)] [BoxGroup("Combine Meshes")] [SerializeField] private string namePrefix = "PlayerHouse";
        [PropertyOrder(9)] [Button("Combine Meshes")]
        public void CombineMeshes()
        {
            foreach (GameObject currGameObject in selectedObjects)
            {
                CombineGameObjectMeshes(currGameObject, outputPath, namePrefix);
            }
        }

        private void CombineGameObjectMeshes(GameObject gameObject, string outputPath, string namePrefix)
        {
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            List<CombineInstance> combines = new List<CombineInstance>();

            Vector3 objectPosition = gameObject.transform.position;
            Quaternion objectRotation = gameObject.transform.rotation;

            gameObject.transform.position = Vector3.zero;
            gameObject.transform.rotation = Quaternion.identity;

            List<Material> materials = new List<Material>();

            for (int i = 0; i < meshFilters.Length; i++)
            {
                // Ignore parent and skip movable mesh renderers
                if (meshFilters[i] == gameObject.GetComponent<MeshFilter>() || meshFilters[i].gameObject.HasComponent<MeshCombineExcluder>())
                {
                    continue;
                }

                MeshRenderer meshRenderer = meshFilters[i].GetComponent<MeshRenderer>();
                for (int j = 0; j < meshRenderer.sharedMaterials.Length; j++)
                {
                    CombineInstance combine = new()
                    {
                        mesh = meshFilters[i].sharedMesh,
                        subMeshIndex = j,
                        transform = meshFilters[i].transform.localToWorldMatrix
                    };

                    // meshFilters[i].gameObject.SetActive(false);
                    meshFilters[i].gameObject.GetComponent<Renderer>().enabled = false;

                    combines.Add(combine);

                    materials.Add(meshRenderer.sharedMaterials[j]);
                }
            }

            gameObject.EnsureComponent<MeshFilter>();
            gameObject.EnsureComponent<MeshRenderer>();

            UnityEngine.Mesh newMesh = new();
            newMesh.CombineMeshes(combines.ToArray(), false);
            gameObject.transform.GetComponent<MeshFilter>().sharedMesh = newMesh;

            // Restore the position and rotation
            gameObject.transform.position = objectPosition;
            gameObject.transform.rotation = objectRotation;

            gameObject.transform.gameObject.SetActive(true);

            AssetDatabase.CreateAsset(newMesh, $"{outputPath}/{namePrefix}_{gameObject.name}_CombinedMesh.asset");
            AssetDatabase.SaveAssets();

            gameObject.GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
        }

    }
}
*/