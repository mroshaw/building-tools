using System.Collections.Generic;
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
    internal static class BuildingTools
    {
        #region Static properties

        static BuildingTools()
        {
        }

        #endregion

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
            Building building = parentGameObject.EnsureComponent<Building>();
            log.Log(LogLevel.Info, $"Added Building component to {parentGameObject.name}.");
        }

        #endregion

        #region Layers methods

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

                if (prop.IsParentedByAny(buildingMeshes, out GameObject parentGameObject))
                {
                    log.Log(LogLevel.Info, $"Moving Props GameObject {prop.name} out of {parentGameObject.name} into {parentGameObject.transform.parent.gameObject.name}...");
                    prop.name = newName;
                    prop.transform.SetParent(parentGameObject.transform.parent);
                }
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
                    if (prop.IsParentedByAny(building.interiors, out GameObject parentGameObject))
                    {
                        return true;
                    }
                }
            }

            if (building.exteriorProps != null && building.exteriorProps.Length > 0)
            {
                foreach (GameObject prop in building.exteriorProps)
                {
                    if (prop.IsParentedByAny(building.exteriors, out GameObject parentGameObject))
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
                log.Log(LogLevel.Info, $"Layer set to {layerName} in {child.gameObject}.");
            }

            if (includeParent)
            {
                parentGameObject.layer = LayerMask.NameToLayer(layerName);
                log.Log(LogLevel.Info, $"Layer set to {layerName} in {parentGameObject.gameObject}.");
            }
        }

        #endregion

        #region Collider methods

        /// <summary>
        /// Look for GameObjects with the names given and add appropriate colliders
        /// </summary>
        internal static void ConfigureColliders(GameObject parentGameObject, BuildingEditorSettings buildingSettings, EditorLog log)
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
                log.Log(LogLevel.Info, $"Added {typeof(T)} to {colliderGameObject.name}.");
            }
            else
            {
                log.Log(LogLevel.Warning, $"{colliderGameObject.name} already has a {typeof(T)} component.");
            }
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

        internal static void ConfigureBuildingLight(GameObject lightGameObject, LightingSettings lightingSettings, BuildingEditorSettings buildingSettings, EditorLog log)
        {
            if (!lightGameObject.TryGetComponentInChildren<Light>(out Light light, true))
            {
                log.Log(LogLevel.Warning, $"No light found on parent mesh {lightGameObject.name}.");
                return;
            }

            if (!lightGameObject.TryGetComponentInChildren<ParticleSystem>(out ParticleSystem flameParticleSystem, true))
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
                if (buildingSettings.doorNames.ItemInString(renderer.gameObject.name))
                {
                    ConfigureDoor(renderer.gameObject, buildingSettings, log);
                }
            }
        }

        private static void ConfigureDoor(GameObject doorGameObject, BuildingEditorSettings buildingSettings, EditorLog log)
        {
            log.Log(LogLevel.Info, $"Configuring door on {doorGameObject.name}.");
            Door door = doorGameObject.EnsureComponent<Door>();
            // We don't want to combine this mesh, as it needs to move
            doorGameObject.EnsureComponent<MeshCombineExcluder>();
            door.ConfigureInEditor(buildingSettings.doorSfxGroup, buildingSettings.doorOpeningClips, buildingSettings.doorOpenClips, buildingSettings.doorClosingClips,
                buildingSettings.doorClosedClips);
            CreateOrUpdateDoorTriggers(door, buildingSettings, log);
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

            MeshTools.ConfigureMeshParameters newMeshParameters = new();

            // Set properties and merge exterior meshes
            newMeshParameters.LightLayerMode = buildingSettings.buildingExteriorLightLayerMode;
            newMeshParameters.LayerName = buildingSettings.buildingExteriorLayer;
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