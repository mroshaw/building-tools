using System.Collections.Generic;
using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Editor.Extensions;
using DaftAppleGames.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "ConfigureDoorsEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Doors Tool")]
    internal class AddDoorsEditorTool : BuildingEditorTool
    {
        [SerializeField] [BoxGroup("Settings")] internal string[] doorNames;
        [SerializeField] [BoxGroup("Settings")] internal AudioClip[] doorOpeningClips;
        [SerializeField] [BoxGroup("Settings")] internal AudioClip[] doorOpenClips;
        [SerializeField] [BoxGroup("Settings")] internal AudioClip[] doorClosingClips;
        [SerializeField] [BoxGroup("Settings")] internal AudioClip[] doorClosedClips;
        [SerializeField] [BoxGroup("Settings")] internal AudioMixerGroup doorSfxGroup;
        [SerializeField] [BoxGroup("Settings")] internal LayerMask doorTriggerLayerMask;
        [SerializeField] [BoxGroup("Settings")] internal string[] doorTriggerTags;
        [SerializeField] [BoxGroup("Settings")] internal float doorColliderHeight = 2.0f;
        [SerializeField] [BoxGroup("Settings")] internal float doorColliderDepth = 1.0f;
        [SerializeField] [BoxGroup("Settings")] internal bool overrideColliderWidth = false;
        [SerializeField] [BoxGroup("Settings")] internal float doorColliderWidth = 1.0f;
        [SerializeField] [BoxGroup("Settings")] internal StaticEditorFlags doorRendererStaticFlags;

        protected override string GetToolName()
        {
            return "Add Doors";
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

            if (!HasDoors(out string doorFailedReason))
            {
                cannotRunReasons.Add(doorFailedReason);
                canRun = false;
            }

            if (!RequireGameObjectValidation(out string requireGameObjectReason))
            {
                cannotRunReasons.Add(requireGameObjectReason);
                canRun = false;
            }

            if (!HasBuildingComponent(out string requiredBuildingReason))
            {
                cannotRunReasons.Add(requiredBuildingReason);
                canRun = false;
            }

            return canRun;
        }

        protected override void RunTool(string undoGroupName)
        {
            ConfigureDoors();
        }

        /// <summary>
        /// Check to see if there are any doors to configure
        /// </summary>
        private bool HasDoors(out string validationReason)
        {
            MeshRenderer[] allMeshRenderers = SelectedGameObject.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer renderer in allMeshRenderers)
            {
                if (doorNames.ItemInString(renderer.gameObject.name))
                {
                    validationReason = string.Empty;
                    return true;
                }
            }

            validationReason = "No GameObjects were founding matching any of the 'doorNames' listed in the tool config. That is, no doors were found!";
            return false;
        }

        private void ConfigureDoors()
        {
            DoorController doorController = SelectedGameObject.EnsureComponent<DoorController>();
            log.AddToLog(LogLevel.Debug, $"Added Door Controller component to {SelectedGameObject.name}.");

            MeshRenderer[] allMeshRenderers = SelectedGameObject.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer renderer in allMeshRenderers)
            {
                if (!doorNames.ItemInString(renderer.gameObject.name))
                {
                    continue;
                }

                Door newDoor = ConfigureDoor(renderer.gameObject);
                doorController.AddDoor(newDoor);
            }
        }

        private Door ConfigureDoor(GameObject doorGameObject)
        {
            log.AddToLog(LogLevel.Info, $"Configuring door on {doorGameObject.name}.");
            Door door = doorGameObject.EnsureComponent<Door>();
            // We don't want to combine this mesh, as it needs to move
            doorGameObject.EnsureComponent<DynamicMeshRenderer>();
            // Set the static flags, as the door will move
            GameObjectUtility.SetStaticEditorFlags(door.gameObject, doorRendererStaticFlags);
            door.ConfigureInEditor(doorSfxGroup, doorOpeningClips, doorOpenClips,
                doorClosingClips,
                doorClosedClips);
            CreateOrUpdateDoorTriggers(door);
            return door;
        }

        private void CreateOrUpdateDoorTriggers(Door door)
        {
            DoorTrigger[] doorTriggers = door.GetComponentsInChildren<DoorTrigger>(true);

            if (doorTriggers.Length == 0)
            {
                // We need to create new door triggers
                CreateDoorTrigger(door, DoorOpenDirection.Inwards);
                CreateDoorTrigger(door, DoorOpenDirection.Outwards);
            }
            else
            {
                // We want to reconfigure existing triggers
                foreach (DoorTrigger existingDoorTrigger in doorTriggers)
                {
                    ConfigureDoorTrigger(door, existingDoorTrigger, existingDoorTrigger.DoorOpenDirection);
                }
            }
        }

        private void CreateDoorTrigger(Door door, DoorOpenDirection openDirection)
        {
            string gameObjectName = openDirection == DoorOpenDirection.Outwards ? "Inside Trigger" : "Outside Trigger";
            GameObject triggerGameObject = new(gameObjectName);
            triggerGameObject.transform.SetParent(door.gameObject.transform);
            triggerGameObject.transform.localPosition = Vector3.zero;
            triggerGameObject.transform.localRotation = Quaternion.identity;
            triggerGameObject.EnsureComponent<BoxCollider>();
            DoorTrigger trigger = triggerGameObject.EnsureComponent<DoorTrigger>();
            ConfigureDoorTrigger(door, trigger, openDirection);
        }

        private void ConfigureDoorTrigger(Door door, DoorTrigger doorTrigger, DoorOpenDirection openDirection)
        {
            doorTrigger.ConfigureInEditor(door, doorTriggerLayerMask, doorTriggerTags, openDirection);
            doorTrigger.transform.parent.gameObject.GetMeshSize(~0, new string[] { }, out Vector3 meshSize, out Vector3 _);
            float distanceFromDoor = openDirection == DoorOpenDirection.Inwards ? 0.3f : -(0.3f + meshSize.x);
            float triggerWidth = meshSize.z;
            float triggerLocalCenter = meshSize.z / 2;

            BoxCollider boxCollider = doorTrigger.GetComponent<BoxCollider>();

            boxCollider.size = overrideColliderWidth
                ? new Vector3(doorColliderDepth, doorColliderHeight, doorColliderWidth)
                : new Vector3(doorColliderDepth, doorColliderHeight, triggerWidth);

            boxCollider.center = new Vector3(distanceFromDoor, doorColliderHeight / 2, triggerLocalCenter);
            boxCollider.isTrigger = true;
        }
    }
}