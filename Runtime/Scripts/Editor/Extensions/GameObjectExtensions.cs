using UnityEngine;

namespace DaftAppleGames.Editor.Extensions
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Returns true if the GameObject has any parent that contains any of the given strings
        /// </summary>
        public static bool HasParentWithName(this GameObject gameObject, string[] nameStrings)
        {
            Transform currentGameObject = gameObject.transform;

            while (currentGameObject != null)
            {
                foreach (string nameToTest in nameStrings)
                {
                    if (currentGameObject.name.Contains(nameToTest))
                    {
                        return true;
                    }
                }

                currentGameObject = currentGameObject.parent;
            }

            return false;
        }

        /// <summary>
        /// Return the local center and dimensions of box enclosing meshes of the Game Object
        /// </summary>
        public static void GetLocalMeshDimensions(this GameObject gameObject, LayerMask includeLayerMask, string[] ignoreNames, out Vector3 localCenter, out Vector3 localSize)
        {
            // Get all MeshRenderers
            MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);

            // Compute combined bounds in world space
            Bounds combinedBounds = new(meshRenderers[0].bounds.center, meshRenderers[0].bounds.size);

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                if ((includeLayerMask & (1 << meshRenderer.gameObject.layer)) == 0 ||
                    (ignoreNames.Length != 0 && HasParentWithName(meshRenderer.gameObject, ignoreNames)))
                {
                    continue;
                }

                combinedBounds.Encapsulate(meshRenderer.bounds);
            }

            // Inverse-transform the center from world space to local space
            localCenter = gameObject.transform.InverseTransformPoint(combinedBounds.center);
            localSize = combinedBounds.size;
        }
    }
}