using System;
using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEngine;
using UnityEngine.UIElements;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.BuildingTools.Editor
{
    [Serializable]
    public struct BuildingPropsSettings
    {
        [SerializeField] internal string[] boxColliderNames;
        [SerializeField] internal string[] sphereColliderNames;
        [SerializeField] internal string[] capsuleColliderNames;
        [SerializeField] internal string[] meshColliderNames;
        [SerializeField] internal bool terrainAlignPosition;
        [SerializeField] internal bool terrainAlignRotation;
        [SerializeField] internal bool terrainAlignX;
        [SerializeField] internal bool terrainAlignY;
        [SerializeField] internal bool terrainAlignZ;
    }

    [CreateAssetMenu(fileName = "ConfigurePropsEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Props Tool")]
    internal class FixPropsEditorTool : BuildingEditorTool
    {
        private bool _addMissingCollidersOption;
        private bool _alignExteriorPropsToTerrainOption;

        protected override string GetToolName()
        {
            return "Fix Props";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, out string cannotRunReason)
        {
            if (RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation() && RequiredBuildingMeshValidation())
            {
                cannotRunReason = string.Empty;
                return true;
            }

            cannotRunReason = $"{selectEditorSettingsAndGameObjectError}\n{buildingComponentRequiredError}\n{buildingMeshNotSetError}";
            return false;
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, string undoGroupName)
        {
            log.Log(LogLevel.Info, $"Running ConfigurePropsEditorTool. Add Colliders is {_addMissingCollidersOption}, Align To Terrain is {_alignExteriorPropsToTerrainOption}");

            if (editorSettings is not BuildingWizardEditorSettings buildingEditorSettings)
            {
                return;
            }

            if (_addMissingCollidersOption)
            {
                log.Log(LogLevel.Info, "Adding missing colliders to props...");
                ConfigureColliders(selectedGameObject, buildingEditorSettings.buildingPropsSettings);
                log.Log(LogLevel.Info, "Done");
            }

            if (_alignExteriorPropsToTerrainOption)
            {
                log.Log(LogLevel.Info, "Aligning exterior props to terrain...");
                AlignExteriorPropsToTerrain(selectedGameObject, buildingEditorSettings.buildingPropsSettings);
                log.Log(LogLevel.Info, "Done");
            }
        }

        /// <summary>
        /// Add bindings for custom tool options
        /// </summary>
        protected override void AddCustomBindings()
        {
            _addMissingCollidersOption = BindToToggleOption("CreateMissingCollidersToggle", SetConfigureCollidersOption);
            _alignExteriorPropsToTerrainOption = BindToToggleOption("AlignExteriorPropsToTerrainToggle", SetAlignToTerrainOption);
        }

        private void SetConfigureCollidersOption(ChangeEvent<bool> changeEvent)
        {
            _addMissingCollidersOption = changeEvent.newValue;
        }

        private void SetAlignToTerrainOption(ChangeEvent<bool> changeEvent)
        {
            _alignExteriorPropsToTerrainOption = changeEvent.newValue;
        }

        /// <summary>
        /// Look for GameObjects with the names given and add appropriate colliders
        /// </summary>
        private static void ConfigureColliders(GameObject parentGameObject, BuildingPropsSettings buildingPropsSettings)
        {
            Renderer[] allRenderers = parentGameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in allRenderers)
            {
                if (buildingPropsSettings.boxColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<BoxCollider>(renderer.gameObject);
                }

                if (buildingPropsSettings.sphereColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<SphereCollider>(renderer.gameObject);
                }

                if (buildingPropsSettings.capsuleColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<CapsuleCollider>(renderer.gameObject);
                }

                if (buildingPropsSettings.meshColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<MeshCollider>(renderer.gameObject);
                }
            }
        }

        private static void ConfigureCollider<T>(GameObject colliderGameObject) where T : Component
        {
            T component = colliderGameObject.GetComponent<T>();
            if (component == null)
            {
                colliderGameObject.AddComponent<T>();
                log.Log(LogLevel.Debug, $"Added {typeof(T)} to {colliderGameObject.name}.");
            }
            else
            {
                log.Log(LogLevel.Warning, $"{colliderGameObject.name} already has a {typeof(T)} component.");
            }
        }

        /// <summary>
        /// Aligns each External Prop mesh renderer to the terrain, if there is one
        /// </summary>
        private static void AlignExteriorPropsToTerrain(GameObject parentGameObject, BuildingPropsSettings buildingPropsSettings)
        {
            Building building = parentGameObject.GetComponent<Building>();

            if (Terrain.activeTerrain == null)
            {
                log.Log(LogLevel.Warning, $"There is no active terrain in the scene, so exterior props will not be aligned.");
                return;
            }

            foreach (GameObject externalPropParent in building.exteriorProps)
            {
                foreach (MeshRenderer propRenderer in externalPropParent.GetComponentsInChildren<MeshRenderer>(true))
                {
                    // Check to see if the renderer is already on top of another mesh renderer
                    if (IsGameObjectOnMeshRenderer(propRenderer.gameObject))
                    {
                        log.Log(LogLevel.Debug, $"Prop {propRenderer.gameObject.name} is already on top of an existing mesh so won't be aligned.");
                        continue;
                    }

                    // If not, align to terrain
                    log.Log(LogLevel.Debug, $"Aligning prop to terrain: {propRenderer.gameObject.name}.");
                    Terrain.activeTerrain.AlignObject(propRenderer.gameObject, buildingPropsSettings.terrainAlignPosition, buildingPropsSettings.terrainAlignRotation,
                        buildingPropsSettings.terrainAlignX, buildingPropsSettings.terrainAlignY, buildingPropsSettings.terrainAlignZ);
                }
            }
        }

        private static bool IsGameObjectOnMeshRenderer(GameObject gameObject)
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
        private static void SetColliderState(GameObject parentGameObject, bool state)
        {
            foreach (Collider collider in parentGameObject.GetComponentsInChildren<Collider>())
            {
                collider.enabled = state;
            }
        }
    }
}