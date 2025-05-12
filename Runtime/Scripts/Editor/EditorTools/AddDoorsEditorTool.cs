using System.Collections.Generic;
using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
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
        [SerializeField] [BoxGroup("Settings")] [Tooltip("Game Objects that contain any of these strings will be configured as doors.")] internal string[] doorNames;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Optional audio clip(s) that play when the door starts opening. If multiple clips are added, a random clip will be played.")]
        internal AudioClip[] doorOpeningClips;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Optional audio clip(s) that play when the door completes opening. If multiple clips are added, a random clip will be played.")]
        internal AudioClip[] doorOpenClips;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Optional audio clip(s) that play when the door starts closing. If multiple clips are added, a random clip will be played.")]
        internal AudioClip[] doorClosingClips;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Optional audio clip(s) that play when the door completes closing. If multiple clips are added, a random clip will be played.")]
        internal AudioClip[] doorClosedClips;

        [SerializeField] [BoxGroup("Settings")] [Tooltip("The AudioMixerGroup used to play the door audio sounds.")] internal AudioMixerGroup doorSfxGroup;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("When a collider enters the door trigger, only colliders on any of the identified layers will be considered for triggering the door.")]
        internal LayerMask doorTriggerLayerMask;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("When a collider enters the door trigger, only colliders with any of the identified tags will be considered for triggering the door.")]
        internal string[] doorTriggerTags;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip(
            "By default, the colliders on either side of the door will match the height of the door mesh. If you want to override this, check this box and enter a value in the override.")]
        internal bool overrideColliderHeight;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("The height of the colliders created on either side of the door, if you've chosen to override the default.")] internal float doorColliderOverrideHeight = 2.0f;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("The depth (distance from the door) of the colliders created on either side of the door.")] internal float doorColliderDepth = 1.0f;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("By default, the trigger width is calculated based on the width of the door. Tick this to override that with a fixed value.")] internal bool overrideColliderWidth;

        [SerializeField] [BoxGroup("Settings")] [Tooltip("The override width of the colliders, if the override flag is set.")] internal float doorColliderOverrideWidth = 1.0f;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip(
            "| Static flag mask to be applied to the door. Generally \"Nothing\" to allow the door to be animated. |\n| ------------------------------------------------------------ |\n|                                                              |")]
        internal StaticEditorFlags doorRendererStaticFlags;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("The name of the Game Object created to hold the \"outside\" door trigger and collider.")] internal string outsideTriggerName = "Outside Trigger";

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("The name of the Game Object created to hold the \"inside\" door trigger and collider.")] internal string insideTriggerName = "Inside Trigger";

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

            if (!HasRequiredBuildingComponent(out string requiredBuildingReason))
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

        /// <summary>
        /// Adds the DoorController then loops through child game object looking for doors to configure
        /// </summary>
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

        /// <summary>
        /// Configures a Door component on the given Game Object
        /// </summary>
        private Door ConfigureDoor(GameObject doorGameObject)
        {
            log.AddToLog(LogLevel.Info, $"Configuring door on {doorGameObject.name}.");
            Door door = doorGameObject.EnsureComponent<Door>();

            // We don't want to combine this mesh, as it needs to move
            doorGameObject.EnsureComponent<DynamicMeshRenderer>();

            // Set the static flags, as the door will move
            GameObjectUtility.SetStaticEditorFlags(door.gameObject, doorRendererStaticFlags);
            door.SetDoorAudio(doorSfxGroup, doorOpeningClips, doorOpenClips, doorClosingClips, doorClosedClips);
            door.SetDoorPivotLocation();
            // Process the door triggers
            ConfigureTriggerColliders(door);
            return door;
        }

        private GameObject ConfigureDoorTrigger(Door door, string triggerName, DoorTriggerLocation doorTriggerLocation)
        {
            Transform triggerTransform = door.transform.Find(triggerName);
            GameObject triggerGameObject = triggerTransform != null ? triggerTransform.gameObject : new GameObject(triggerName);
            triggerGameObject.transform.SetParent(door.transform);
            triggerGameObject.transform.localPosition = Vector3.zero;
            triggerGameObject.transform.localScale = Vector3.one;
            triggerGameObject.transform.localRotation = Quaternion.identity;

            DoorTrigger doorTrigger = triggerGameObject.EnsureComponent<DoorTrigger>();
            doorTrigger.Door = door;
            doorTrigger.TriggerLayerMask = doorTriggerLayerMask;
            doorTrigger.TriggerTags = doorTriggerTags;
            doorTrigger.DoorTriggerLocation = doorTriggerLocation;
            return triggerGameObject;
        }

        private void ConfigureTriggerColliders(Door door)
        {
            GameObject insideTriggerGameObject = ConfigureDoorTrigger(door, insideTriggerName, DoorTriggerLocation.Inside);
            GameObject outsideTriggerGameObject = ConfigureDoorTrigger(door, outsideTriggerName, DoorTriggerLocation.Outside);

            Transform doorTransform = door.transform;

            // Configure the colliders
            // Capture current rotation, and set to zero. Makes positioning the colliders easier.

            Quaternion doorRotation = doorTransform.rotation;
            doorTransform.rotation = Quaternion.identity;

            Renderer renderer = door.GetComponent<Renderer>();

            Bounds bounds = renderer.bounds;
            Vector3 size = bounds.size;
            Vector3 center = bounds.center;

            // Determine the depth axis (smallest dimension)
            int depthAxis = GetSmallestAxis(size);
            Vector3 normal = GetAxisVector(depthAxis, door.transform);

            float height = size.y;
            float width = depthAxis == 0 ? size.z : size.x; // whichever is not depth
            float doorDepth = size[depthAxis];

            // Determine if we need to rotate the colliders
            bool rotateColliders = depthAxis == 0; // X-axis = transform.right

            CreateWorldAlignedCollider(door.gameObject, outsideTriggerGameObject, center + normal * (doorDepth / 2f + doorColliderDepth / 2f), doorTransform.rotation, width,
                height,
                doorColliderDepth, rotateColliders);
            CreateWorldAlignedCollider(door.gameObject, insideTriggerGameObject, center - normal * (doorDepth / 2f + doorColliderDepth / 2f), doorTransform.rotation, width,
                height,
                doorColliderDepth, rotateColliders);

            // Restore the door rotation
            doorTransform.rotation = doorRotation;
        }

        private void CreateWorldAlignedCollider(GameObject doorGameObject, GameObject colliderGameObject, Vector3 worldPosition, Quaternion doorRotation, float width, float height,
            float depth,
            bool rotateY)
        {
            // Compute final rotation
            Quaternion finalRotation = doorRotation;
            if (rotateY)
            {
                finalRotation *= Quaternion.Euler(0f, 90f, 0f); // Apply 90° Y rotation in world space
            }

            colliderGameObject.transform.SetPositionAndRotation(worldPosition, finalRotation);

            BoxCollider box = colliderGameObject.EnsureComponent<BoxCollider>();
            box.size = new Vector3(width, height, depth);
            box.center = Vector3.zero;
            box.isTrigger = true;
            // Parent to door while preserving world transform
            colliderGameObject.transform.SetParent(doorGameObject.transform, true);
        }


        private int GetSmallestAxis(Vector3 v)
        {
            if (v.x <= v.y && v.x <= v.z) return 0;
            if (v.y <= v.x && v.y <= v.z) return 1;
            return 2;
        }

        private Vector3 GetAxisVector(int axis, Transform t)
        {
            return axis switch
            {
                0 => t.right,
                1 => t.up,
                2 => t.forward,
                _ => Vector3.forward
            };
        }
    }
}