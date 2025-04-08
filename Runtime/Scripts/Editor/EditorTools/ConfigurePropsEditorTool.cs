using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "ConfigurePropsEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Props Tool")]
    internal class ConfigurePropsEditorTool : BuildingEditorTool
    {
        private bool _addMissingCollidersOption;
        private bool _alignExteriorPropsToTerrainOption;

        protected override string GetToolName()
        {
            return "Configure Props";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation() && RequiredBuildingMeshValidation();
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
                ConfigureColliders(selectedGameObject, buildingEditorSettings);
                log.Log(LogLevel.Info, "Done");
            }

            if (_alignExteriorPropsToTerrainOption)
            {
                log.Log(LogLevel.Info, "Aligning exterior props to terrain...");
                AlignExteriorPropsToTerrain(selectedGameObject, buildingEditorSettings);
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


        #region Static Props methods

        /// <summary>
        /// Look for GameObjects with the names given and add appropriate colliders
        /// </summary>
        private static void ConfigureColliders(GameObject parentGameObject, BuildingWizardEditorSettings buildingWizardSettings)
        {
            Renderer[] allRenderers = parentGameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in allRenderers)
            {
                if (buildingWizardSettings.boxColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<BoxCollider>(renderer.gameObject);
                }

                if (buildingWizardSettings.sphereColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<SphereCollider>(renderer.gameObject);
                }

                if (buildingWizardSettings.capsuleColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<CapsuleCollider>(renderer.gameObject);
                }

                if (buildingWizardSettings.meshColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<MeshCollider>(renderer.gameObject);
                }
            }
        }

        private static void ConfigureCollider<T>(GameObject colliderGameObject)
        {
            T component = colliderGameObject.GetComponent<T>();
            if (component == null)
            {
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
        private static void AlignExteriorPropsToTerrain(GameObject parentGameObject, BuildingWizardEditorSettings buildingWizardSettings)
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
                        continue;
                    }

                    // If not, align to terrain

                    log.Log(LogLevel.Debug, $"Aligning prop to terrain: {propRenderer.gameObject.name}.");
                    Terrain.activeTerrain.AlignObject(propRenderer.gameObject, buildingWizardSettings.terrainAlignPosition, buildingWizardSettings.terrainAlignRotation,
                        buildingWizardSettings.terrainAlignX, buildingWizardSettings.terrainAlignY, buildingWizardSettings.terrainAlignZ);
                }
            }
        }

        private static bool IsGameObjectOnMeshRenderer(GameObject gameObject)
        {
            // Raycast down from the GameObject, see what's there
            LayerMask rayLayerMask = ~0;
            Vector3 up = gameObject.transform.up;
            bool isHit = Physics.Raycast(gameObject.transform.position + up * 0.1f, up * -1, out RaycastHit raycastHit, 0.5f,
                rayLayerMask,
                QueryTriggerInteraction.UseGlobal);
            if (!isHit)
            {
                return false;
            }

            return raycastHit.collider.gameObject.GetComponent<Terrain>() == null;
        }

        #endregion
    }
}