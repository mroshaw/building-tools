using DaftAppleGames.Extensions;
using UnityEngine;

namespace DaftAppleGames.Editor.Extensions
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Returns the size of a box that encloses the meshes in the Game Object
        /// </summary>
        public static void GetMeshSize(this GameObject parentGameObject, LayerMask includeLayerMask, string[] ignoreNames, out Vector3 meshSize, out Vector3 meshCenter)
        {
            Bounds meshBounds = GetMeshBounds(parentGameObject, includeLayerMask, ignoreNames);
            meshSize = meshBounds.size;
            meshCenter = meshBounds.center;
        }

        /// <summary>
        /// Return the bounds of the enclosing meshes in the Game Object
        /// </summary>
        private static Bounds GetMeshBounds(GameObject gameObject, LayerMask includeLayerMask, string[] ignoreNames)
        {
            Bounds combinedBounds = new(Vector3.zero, Vector3.zero);
            bool hasValidRenderer = false;

            foreach (MeshRenderer childRenderer in gameObject.GetComponentsInChildren<MeshRenderer>(true))
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
    }
}