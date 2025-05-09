using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.Buildings
{
    /// <summary>
    /// Allows a Game Object that's had it's meshes combined to be rolled back to how it was
    /// </summary>
    public class MeshCombineRollBack : MonoBehaviour
    {
        [BoxGroup("Status")] [SerializeField] [ReadOnly] public bool isOptimised;
        [BoxGroup("Settings")] [SerializeField] private bool deleteAssets = true;
        [BoxGroup("Settings")] [SerializeField] private bool deleteAssetFolder = true;
        [BoxGroup("Settings")] [SerializeField] private bool deleteSelf;
        [FoldoutGroup("Audit")] [SerializeField] private List<Renderer> deactivatedRenderers;
        [FoldoutGroup("Audit")] [SerializeField] private List<GameObject> resultGameObjects;
        [FoldoutGroup("Audit")] [SerializeField] private string assetAbsoluteFolderPath;
        [FoldoutGroup("Audit")] [SerializeField] private string assetRelativeFolderPath;

        public void ClearAudit()
        {
            isOptimised = false;
            deactivatedRenderers?.Clear();
            resultGameObjects?.Clear();
            assetAbsoluteFolderPath = string.Empty;
            assetRelativeFolderPath = string.Empty;
        }

        public void AddResultGameObject(GameObject newGameObject)
        {
            resultGameObjects ??= new List<GameObject>();
            resultGameObjects.Add(newGameObject);
        }

        public void SetPaths(string absolutePath, string relativePath)
        {
            assetAbsoluteFolderPath = absolutePath;
            assetRelativeFolderPath = relativePath;
        }

        public void AddRenderer(Renderer newRenderer)
        {
            deactivatedRenderers ??= new List<Renderer>();
            deactivatedRenderers.Add(newRenderer);
        }

        [Button("Roll Back Optimisation Changes")]
        public void RollBack()
        {
            if (!isOptimised)
            {
                return;
            }

            // Re-enable all activated Mesh GameObjects
            foreach (Renderer currRenderer in deactivatedRenderers)
            {
                currRenderer.enabled = true;
            }

            // Destroy the resultGameObject Game Object
            foreach (GameObject currGameObject in resultGameObjects)
            {
                DestroyImmediate(currGameObject);
            }

            resultGameObjects.Clear();

            string[] assetFolders = { assetRelativeFolderPath };

            // Delete the Prefab Assets
            if (deleteAssets)
            {
                foreach (string asset in AssetDatabase.FindAssets("", assetFolders))
                {
                    string path = AssetDatabase.GUIDToAssetPath(asset);
                    AssetDatabase.DeleteAsset(path);
                }
            }

            // Delete the asset folder
            if (deleteAssetFolder)
            {
                if (Directory.Exists(assetAbsoluteFolderPath))
                {
                    string metaFilePath = $"{assetAbsoluteFolderPath}.meta";

                    Directory.Delete(assetAbsoluteFolderPath, true);
                    Debug.Log($"Deleting folder: {assetAbsoluteFolderPath}");

                    Debug.Log($"Deleting file: {metaFilePath}");
                    File.Delete(metaFilePath);
                }
            }

            // Clear and optionally delete the component
            ClearAudit();
            if (deleteSelf)
            {
                StartCoroutine(DestroySelf());
            }
        }

        private IEnumerator DestroySelf()
        {
            // Hide from inspector to stop it trying to redraw after it's been destroyed
            hideFlags |= HideFlags.HideInInspector;

            // Wait a a frame so Unity doesn't redraw the component
            yield return null;

            // Now we can destroy it
            Debug.Log($"Deleting MeshCombineRollBack component on : {gameObject.name}");
            DestroyImmediate(this);
        }
    }
}