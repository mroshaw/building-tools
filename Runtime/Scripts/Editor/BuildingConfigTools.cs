using DaftAppleGames.Darskerry.Core.Buildings;
using DaftAppleGames.Editor;
using UnityEngine;
using DaftAppleGames.Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// Static methods for working with Buildings
    /// </summary>
    internal static class BuildingConfigTools
    {
        #region Tool prarameter structs

        /// <summary>
        /// Struct to consolidate parameters for use with the static methods
        /// </summary>
        internal struct ConfigureBuildingParameters
        {
        }

        #endregion

        #region Base methods

        internal static void AddBuildingComponent(GameObject parentGameObject, EditorLog log)
        {
            _ = parentGameObject.EnsureComponent<Building>();
            log.Log(LogLevel.Info, $"Added Building component to {parentGameObject.name}.");
        }

        #endregion

        #region Mesh config methods

        /// <summary>
        /// Applies layers, static and lighting properties to meshes
        /// </summary>
        internal static void ConfigureMeshes(GameObject parentGameObject, BuildingEditorSettings buildingSettings, EditorLog log)
        {
            log.Log(LogLevel.Info, "Configuring layers...");
            ConfigureLayers(parentGameObject, buildingSettings, log);
            log.Log(LogLevel.Info, "Configuring static flags...");
            ConfigureStaticFlags(parentGameObject, buildingSettings, log);
        }

        /// <summary>
        /// Sets the static flags on all child mesh renderers
        /// </summary>
        internal static void ConfigureStaticFlags(GameObject parentGameObject, BuildingEditorSettings buildingSettings, EditorLog log)
        {
            foreach (MeshRenderer meshRenderer in parentGameObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                log.Log(LogLevel.Debug, $"Setting static flags on {meshRenderer.gameObject.name} to {buildingSettings.staticMeshFlags}");
                GameObjectUtility.SetStaticEditorFlags(meshRenderer.gameObject, buildingSettings.staticMeshFlags);
            }
        }

        /// <summary>
        /// Configure the Building layers
        /// </summary>
        internal static void ConfigureLayers(GameObject parentGameObject, BuildingEditorSettings buildingSettings, EditorLog log)
        {
            Building building = parentGameObject.GetComponent<Building>();

            ConfigureLayersOnGameObjects(building.exteriors, buildingSettings.buildingExteriorLayer, log);
            ConfigureLayersOnGameObjects(building.interiors, buildingSettings.buildingInteriorLayer, log);
            ConfigureLayersOnGameObjects(building.interiorProps, buildingSettings.interiorPropsLayer, log);
            ConfigureLayersOnGameObjects(building.exteriorProps, buildingSettings.exteriorPropsLayer, log);

            // If props are within the building interior/exterior, move them up a level
            MovePropsToParent(parentGameObject, log);
        }

        private static void ConfigureLayersOnGameObjects(GameObject[] gameObjects, string layerName, EditorLog log)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                SetLayerInChildren(gameObject, layerName, log);
            }
        }

        private static void MovePropsToParent(GameObject buildingGameObject, EditorLog log)
        {
            Building building = buildingGameObject.GetComponent<Building>();
            MoveAndRenameProps(building.interiorProps, building.interiors, "InteriorProps", log);
            MoveAndRenameProps(building.exteriorProps, building.exteriors, "ExteriorProps", log);
        }

        private static void MoveAndRenameProps(GameObject[] props, GameObject[] buildingMeshes, string newName, EditorLog log)
        {
            foreach (GameObject prop in props)
            {
                if (!prop)
                {
                    continue;
                }

                if (!prop.IsParentedByAny(buildingMeshes, out GameObject parentGameObject))
                {
                    continue;
                }

                log.Log(LogLevel.Debug, $"Moving Props GameObject {prop.name} out of {parentGameObject.name} into {parentGameObject.transform.parent.gameObject.name}...");
                prop.name = newName;
                prop.transform.SetParent(parentGameObject.transform.parent);
            }
        }

        /// <summary>
        /// Checks to see if the Props are inside main structure GameObjects
        /// </summary>
        internal static bool ArePropsInMainBuildingStructure(GameObject buildingGameObject)
        {
            Building building = buildingGameObject.GetComponent<Building>();

            if (building.interiorProps != null && building.interiorProps.Length > 0)
            {
                foreach (GameObject prop in building.interiorProps)
                {
                    if (prop.IsParentedByAny(building.interiors, out _))
                    {
                        return true;
                    }
                }
            }

            if (building.exteriorProps == null || building.exteriorProps.Length <= 0)
            {
                return false;
            }

            {
                foreach (GameObject prop in building.exteriorProps)
                {
                    if (prop.IsParentedByAny(building.exteriors, out _))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the Layer of all children of the given Game Object
        /// </summary>
        private static void SetLayerInChildren(GameObject parentGameObject, string layerName, EditorLog log, bool includeParent = true)
        {
            foreach (MeshRenderer child in parentGameObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                child.gameObject.layer = LayerMask.NameToLayer(layerName);
                log.Log(LogLevel.Debug, $"Layer set to {layerName} in {child.gameObject}.");
            }

            if (!includeParent)
            {
                return;
            }

            parentGameObject.layer = LayerMask.NameToLayer(layerName);
            log.Log(LogLevel.Debug, $"Layer set to {layerName} in {parentGameObject.gameObject}.");
        }

        #endregion

        #region Props methods

        internal static void ConfigureProps(GameObject parentGameObject, BuildingEditorSettings buildingSettings, EditorLog log)
        {
            log.Log(LogLevel.Info, "Configuring colliders...");
            ConfigureColliders(parentGameObject, buildingSettings, log);
            log.Log(LogLevel.Info, "Aligning props to terrain...");
            AlignExteriorPropsToTerrain(parentGameObject, buildingSettings, log);
        }

        /// <summary>
        /// Look for GameObjects with the names given and add appropriate colliders
        /// </summary>
        private static void ConfigureColliders(GameObject parentGameObject, BuildingEditorSettings buildingSettings, EditorLog log)
        {
            Renderer[] allRenderers = parentGameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in allRenderers)
            {
                if (buildingSettings.boxColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<BoxCollider>(renderer.gameObject, log);
                }

                if (buildingSettings.sphereColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<SphereCollider>(renderer.gameObject, log);
                }

                if (buildingSettings.capsuleColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<CapsuleCollider>(renderer.gameObject, log);
                }

                if (buildingSettings.meshColliderNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureCollider<MeshCollider>(renderer.gameObject, log);
                }
            }
        }

        private static void ConfigureCollider<T>(GameObject colliderGameObject, EditorLog log)
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
        private static void AlignExteriorPropsToTerrain(GameObject parentGameObject, BuildingEditorSettings buildingSettings, EditorLog log)
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
                    Terrain.activeTerrain.AlignObject(propRenderer.gameObject, buildingSettings.terrainAlignPosition, buildingSettings.terrainAlignRotation,
                        buildingSettings.terrainAlignX, buildingSettings.terrainAlignY, buildingSettings.terrainAlignZ);
                }
            }
        }

        private static bool IsGameObjectOnMeshRenderer(GameObject gameObject)
        {
            // Raycast down from the GameObject, see what's there
            LayerMask rayLayerMask = ~0;
            bool isHit = Physics.Raycast(gameObject.transform.position, gameObject.transform.up * -1, out RaycastHit raycastHit, 0.5f, rayLayerMask,
                QueryTriggerInteraction.UseGlobal);
            if (!isHit)
            {
                return false;
            }

            return raycastHit.collider.gameObject.GetComponent<Terrain>() == null;
        }

        #endregion

        #region Volume methods

        internal static void ConfigureVolumes(GameObject parentGameObject, BuildingEditorSettings buildingSettings, EditorLog log)
        {
            MeshTools.GetMeshSize(parentGameObject, buildingSettings.meshSizeIncludeLayers, buildingSettings.meshSizeIgnoreNames, out Vector3 buildingSize,
                out Vector3 buildingCenter);
            log.Log(LogLevel.Info, $"Building size is: {buildingSize}, local center is at: {buildingCenter}");
        }

        #endregion

        #region Lighting methods

        internal static void ConfigureLighting(GameObject parentGameObject, BuildingEditorSettings buildingSettings, EditorLog log)
        {
            LightingController lightingController = parentGameObject.EnsureComponent<LightingController>();
            log.Log(LogLevel.Info, $"Added Lighting Controller component to {parentGameObject.name}.");

            MeshRenderer[] allMeshRenderers = parentGameObject.GetComponentsInChildren<MeshRenderer>(true);

            // Configure interior candles
            foreach (MeshRenderer renderer in allMeshRenderers)
            {
                if (buildingSettings.indoorCandleSettings.meshNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureBuildingLight(renderer.gameObject, buildingSettings.indoorCandleSettings, buildingSettings, log);
                }
            }
            // Configure interior fires

            // Configure exterior lights

            lightingController.UpdateLights();
        }

        private static void ConfigureBuildingLight(GameObject lightGameObject, LightingSettings lightingSettings, BuildingEditorSettings buildingSettings, EditorLog log)
        {
            if (!lightGameObject.TryGetComponentInChildren(out Light light, true))
            {
                log.Log(LogLevel.Warning, $"No light found on parent mesh {lightGameObject.name}.");
                return;
            }

            if (!lightGameObject.TryGetComponentInChildren(out ParticleSystem flameParticleSystem, true))
            {
                log.Log(LogLevel.Warning, $"No flame particle system found on parent mesh {lightGameObject.name}.");
            }

            BuildingLight buildingLight = lightGameObject.EnsureComponent<BuildingLight>();
            buildingLight.ConfigureInEditor(lightingSettings.buildingLightType, light, flameParticleSystem);
        }

        #endregion

        #region Door methods

        internal static void ConfigureDoors(GameObject parentGameObject, BuildingEditorSettings buildingSettings, EditorLog log)
        {
            DoorController doorController = parentGameObject.EnsureComponent<DoorController>();
            log.Log(LogLevel.Info, $"Added Door Controller component to {parentGameObject.name}.");

            MeshRenderer[] allMeshRenderers = parentGameObject.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer renderer in allMeshRenderers)
            {
                if (!buildingSettings.doorNames.ItemInString(renderer.gameObject.name))
                {
                    continue;
                }

                Door newDoor = ConfigureDoor(renderer.gameObject, buildingSettings, log);
                doorController.AddDoor(newDoor);
            }
        }

        private static Door ConfigureDoor(GameObject doorGameObject, BuildingEditorSettings buildingSettings, EditorLog log)
        {
            log.Log(LogLevel.Info, $"Configuring door on {doorGameObject.name}.");
            Door door = doorGameObject.EnsureComponent<Door>();
            // We don't want to combine this mesh, as it needs to move
            doorGameObject.EnsureComponent<MeshCombineExcluder>();
            // Set the static flags, as the door will move
            GameObjectUtility.SetStaticEditorFlags(door.gameObject, buildingSettings.moveableMeshFlags);
            door.ConfigureInEditor(buildingSettings.doorSfxGroup, buildingSettings.doorOpeningClips, buildingSettings.doorOpenClips, buildingSettings.doorClosingClips,
                buildingSettings.doorClosedClips);
            CreateOrUpdateDoorTriggers(door, buildingSettings, log);
            return door;
        }


        private static void CreateOrUpdateDoorTriggers(Door door, BuildingEditorSettings buildingSettings, EditorLog log)
        {
            DoorTrigger[] doorTriggers = door.GetComponentsInChildren<DoorTrigger>(true);

            if (doorTriggers.Length == 0)
            {
                // We need to create new door triggers
                CreateDoorTrigger(door, buildingSettings, DoorOpenDirection.Inwards, log);
                CreateDoorTrigger(door, buildingSettings, DoorOpenDirection.Outwards, log);
            }
            else
            {
                // We want to reconfigure existing triggers
                foreach (DoorTrigger existingDoorTrigger in doorTriggers)
                {
                    ConfigureDoorTrigger(door, existingDoorTrigger, buildingSettings, existingDoorTrigger.DoorOpenDirection, log);
                }
            }
        }

        private static void CreateDoorTrigger(Door door, BuildingEditorSettings buildingSettings, DoorOpenDirection openDirection, EditorLog log)
        {
            string gameObjectName = openDirection == DoorOpenDirection.Outwards ? "Inside Trigger" : "Outside Trigger";
            GameObject triggerGameObject = new(gameObjectName);
            triggerGameObject.transform.SetParent(door.gameObject.transform);
            triggerGameObject.transform.localPosition = Vector3.zero;
            triggerGameObject.transform.localRotation = Quaternion.identity;
            triggerGameObject.EnsureComponent<BoxCollider>();
            DoorTrigger trigger = triggerGameObject.EnsureComponent<DoorTrigger>();
            ConfigureDoorTrigger(door, trigger, buildingSettings, openDirection, log);
        }

        private static void ConfigureDoorTrigger(Door door, DoorTrigger doorTrigger, BuildingEditorSettings buildingSettings, DoorOpenDirection openDirection, EditorLog log)
        {
            doorTrigger.ConfigureInEditor(door, buildingSettings.doorTriggerLayerMask, buildingSettings.doorTriggerTags, openDirection);
            MeshTools.GetMeshSize(doorTrigger.transform.parent.gameObject, ~0, new string[] { }, out Vector3 meshSize, out Vector3 center);
            float distanceFromDoor = openDirection == DoorOpenDirection.Inwards ? 0.3f : -(0.3f + meshSize.x);
            float triggerWidth = meshSize.z;
            float triggerLocalCenter = meshSize.z / 2;

            BoxCollider boxCollider = doorTrigger.GetComponent<BoxCollider>();

            boxCollider.size = new Vector3(1.0f, 1.0f, triggerWidth);
            boxCollider.center = new Vector3(distanceFromDoor, 0, triggerLocalCenter);
            boxCollider.isTrigger = true;
        }

        #endregion

        #region Optimisation methods

        internal static void OptimiseMeshes(GameObject parentGameObject, BuildingEditorSettings buildingSettings, EditorLog log)
        {
            Building building = parentGameObject.GetComponent<Building>();

            // Combine the exterior Prop meshes
            MeshTools.CombineMeshParameters combineMeshParameters = new()
            {
                BaseAssetOutputPath = buildingSettings.meshAssetOutputPath,
                AssetOutputFolder = parentGameObject.name,
                CreateOutputFolder = true
            };

            MeshTools.ConfigureMeshParameters newMeshParameters = new()
            {
                // Set properties and merge exterior meshes
                LightLayerMode = buildingSettings.buildingExteriorLightLayerMode,
                LayerName = buildingSettings.buildingExteriorLayer
            };

            OptimiseMeshGroup(building.exteriorProps, "exteriorProps", combineMeshParameters, newMeshParameters, log);
            OptimiseMeshGroup(building.exteriors, "exteriors", combineMeshParameters, newMeshParameters, log);

            // Set properties and merge interior meshes
            newMeshParameters.LightLayerMode = buildingSettings.buildingInteriorLightLayerMode;
            newMeshParameters.LayerName = buildingSettings.buildingInteriorLayer;
            OptimiseMeshGroup(building.interiors, "interiors", combineMeshParameters, newMeshParameters, log);
            OptimiseMeshGroup(building.interiorProps, "interiorProps", combineMeshParameters, newMeshParameters, log);
        }

        private static void OptimiseMeshGroup(GameObject[] allGameObjects, string namePrefix, MeshTools.CombineMeshParameters combineMeshParameters,
            MeshTools.ConfigureMeshParameters newMeshParameters, EditorLog log)
        {
            foreach (GameObject gameObjectParent in allGameObjects)
            {
                combineMeshParameters.AssetFileNamePrefix = namePrefix;
                MeshTools.CombineGameObjectMeshes(gameObjectParent, combineMeshParameters, newMeshParameters);
            }
        }

        #endregion
    }
}