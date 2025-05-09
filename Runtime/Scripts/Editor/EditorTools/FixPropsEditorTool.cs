using System.Collections.Generic;
using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "ConfigurePropsEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Props Tool")]
    internal class FixPropsEditorTool : BuildingEditorTool
    {
        [SerializeField] [BoxGroup("Collider Settings")] internal string[] boxColliderNames;
        [SerializeField] [BoxGroup("Collider Settings")] internal string[] sphereColliderNames;
        [SerializeField] [BoxGroup("Collider Settings")] internal string[] capsuleColliderNames;
        [SerializeField] [BoxGroup("Collider Settings")] internal string[] meshColliderNames;
        [SerializeField] [BoxGroup("Alignment Settings")] internal bool terrainAlignPosition;
        [SerializeField] [BoxGroup("Alignment Settings")] internal bool terrainAlignRotation;
        [SerializeField] [BoxGroup("Alignment Settings")] internal bool terrainAlignX;
        [SerializeField] [BoxGroup("Alignment Settings")] internal bool terrainAlignY;
        [SerializeField] [BoxGroup("Alignment Settings")] internal bool terrainAlignZ;

        protected override string GetToolName()
        {
            return "Fix Props";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(out List<string> cannotRunReasons)
        {
            bool canRun = true;

            cannotRunReasons = new List<string>();
            if (!RequireGameObjectValidation(out string requireGameObjectReason))
            {
                cannotRunReasons.Add(requireGameObjectReason);
                return false;
            }

            if (!HasRequiredBuildingComponent(out string requiredBuildingReason))
            {
                cannotRunReasons.Add(requiredBuildingReason);
                canRun = false;
            }

            if (!HasPropsConfigured(out string hasPropsConfiguredReason))
            {
                cannotRunReasons.Add(hasPropsConfiguredReason);
                canRun = false;
            }

            return canRun;
        }

        protected override void RunTool(string undoGroupName)
        {
            log.AddToLog(LogLevel.Info, "Adding missing colliders to props...");
            ConfigureColliders();
            log.AddToLog(LogLevel.Info, "Done");

            log.AddToLog(LogLevel.Info, "Aligning exterior props to terrain...");
            AlignExteriorPropsToTerrain();
            log.AddToLog(LogLevel.Info, "Done");
        }

        /// <summary>
        /// Look for GameObjects with the names given and add appropriate colliders
        /// </summary>
        private void ConfigureColliders()
        {
            Renderer[] allRenderers = SelectedGameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in allRenderers)
            {
                if (boxColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<BoxCollider>(renderer.gameObject);
                }

                if (sphereColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<SphereCollider>(renderer.gameObject);
                }

                if (capsuleColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<CapsuleCollider>(renderer.gameObject);
                }

                if (meshColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<MeshCollider>(renderer.gameObject);
                }
            }
        }

        private void ConfigureCollider<T>(GameObject colliderGameObject) where T : Component
        {
            T component = colliderGameObject.GetComponent<T>();
            if (component == null)
            {
                colliderGameObject.AddComponent<T>();
                log.AddToLog(LogLevel.Debug, $"Added {typeof(T)} to {colliderGameObject.name}.");
            }
            else
            {
                log.AddToLog(LogLevel.Warning, $"{colliderGameObject.name} already has a {typeof(T)} component.");
            }
        }

        /// <summary>
        /// Aligns each External Prop mesh renderer to the terrain, if there is one
        /// </summary>
        private void AlignExteriorPropsToTerrain()
        {
            Building building = SelectedGameObject.GetComponent<Building>();

            if (Terrain.activeTerrain == null)
            {
                log.AddToLog(LogLevel.Warning, $"There is no active terrain in the scene, so exterior props will not be aligned.");
                return;
            }

            foreach (GameObject externalPropParent in building.exteriorProps)
            {
                log.AddToLog(LogLevel.Debug, $"Processing prop container: {externalPropParent.gameObject.name}...");

                foreach (Transform propGameObjectTransform in externalPropParent.transform)
                {
                    GameObject propGameObject = propGameObjectTransform.gameObject;
                    log.AddToLog(LogLevel.Debug, $"Processing prop {propGameObject.name}...");
                    // Check to see if the gameobject is already on top of another mesh renderer
                    if (IsGameObjectOnMeshRenderer(propGameObject))
                    {
                        log.AddToLog(LogLevel.Debug, $"Prop {propGameObject.name} is already on top of an existing mesh so won't be aligned.");
                        continue;
                    }

                    // If not, align to terrain
                    log.AddToLog(LogLevel.Debug, $"Aligning prop to terrain: {propGameObject.name}.");
                    Terrain.activeTerrain.AlignObject(propGameObject, terrainAlignPosition, terrainAlignRotation,
                        terrainAlignX, terrainAlignY, terrainAlignZ);
                }
            }
        }

        private bool IsGameObjectOnMeshRenderer(GameObject gameObject)
        {
            // Disable any Colliders already on the GameObject.
            SetColliderState(gameObject, false);

            // Raycast down from the GameObject, see what's there
            LayerMask rayLayerMask = ~0;
            Vector3 up = gameObject.transform.up;
            bool isHit = Physics.Raycast(gameObject.transform.position + up * 0.1f, up * -1, out RaycastHit raycastHit, 0.5f,
                rayLayerMask,
                QueryTriggerInteraction.UseGlobal);

            // Re-enable Colliders
            SetColliderState(gameObject, true);

            if (!isHit)
            {
                return false;
            }

            return raycastHit.collider.gameObject.GetComponent<Terrain>() == null;
        }

        /// <summary>
        /// Enables or disables all colliders in a GameObject
        /// </summary>
        private void SetColliderState(GameObject parentGameObject, bool state)
        {
            foreach (Collider collider in parentGameObject.GetComponentsInChildren<Collider>())
            {
                collider.enabled = state;
            }
        }
    }
}