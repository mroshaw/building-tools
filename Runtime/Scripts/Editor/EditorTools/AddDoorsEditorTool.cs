using System;
using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Editor.Extensions;
using DaftAppleGames.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace DaftAppleGames.BuildingTools.Editor
{
    [Serializable] internal struct BuildingDoorSettings
    {
        [SerializeField] internal string[] doorNames;
        [SerializeField] internal AudioClip[] doorOpeningClips;
        [SerializeField] internal AudioClip[] doorOpenClips;
        [SerializeField] internal AudioClip[] doorClosingClips;
        [SerializeField] internal AudioClip[] doorClosedClips;
        [SerializeField] internal AudioMixerGroup doorSfxGroup;
        [SerializeField] internal LayerMask doorTriggerLayerMask;
        [SerializeField] internal string[] doorTriggerTags;
        [SerializeField] internal StaticEditorFlags moveableMeshStaticFlags;
    }

    [CreateAssetMenu(fileName = "ConfigureDoorsEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Doors Tool")]
    internal class AddDoorsEditorTool : BuildingEditorTool
    {
        protected override string GetToolName()
        {
            return "Add Doors";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation();
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, string undoGroupName)
        {
            if (editorSettings is BuildingWizardEditorSettings buildingEditorSettings)
            {
                ConfigureDoors(selectedGameObject, buildingEditorSettings.buildingDoorSettings);
            }
        }

        #region Static Door methods

        private static void ConfigureDoors(GameObject parentGameObject, BuildingDoorSettings buildingDoorSettings)
        {
            DoorController doorController = parentGameObject.EnsureComponent<DoorController>();
            log.Log(LogLevel.Debug, $"Added Door Controller component to {parentGameObject.name}.");

            MeshRenderer[] allMeshRenderers = parentGameObject.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer renderer in allMeshRenderers)
            {
                if (!buildingDoorSettings.doorNames.ItemInString(renderer.gameObject.name))
                {
                    continue;
                }

                Door newDoor = ConfigureDoor(renderer.gameObject, buildingDoorSettings);
                doorController.AddDoor(newDoor);
            }
        }

        private static Door ConfigureDoor(GameObject doorGameObject, BuildingDoorSettings buildingDoorSettings)
        {
            log.Log(LogLevel.Info, $"Configuring door on {doorGameObject.name}.");
            Door door = doorGameObject.EnsureComponent<Door>();
            // We don't want to combine this mesh, as it needs to move
            doorGameObject.EnsureComponent<MeshCombineExcluder>();
            // Set the static flags, as the door will move
            GameObjectUtility.SetStaticEditorFlags(door.gameObject, buildingDoorSettings.moveableMeshStaticFlags);
            door.ConfigureInEditor(buildingDoorSettings.doorSfxGroup, buildingDoorSettings.doorOpeningClips, buildingDoorSettings.doorOpenClips,
                buildingDoorSettings.doorClosingClips,
                buildingDoorSettings.doorClosedClips);
            CreateOrUpdateDoorTriggers(door, buildingDoorSettings);
            return door;
        }


        private static void CreateOrUpdateDoorTriggers(Door door, BuildingDoorSettings buildingDoorSettings)
        {
            DoorTrigger[] doorTriggers = door.GetComponentsInChildren<DoorTrigger>(true);

            if (doorTriggers.Length == 0)
            {
                // We need to create new door triggers
                CreateDoorTrigger(door, buildingDoorSettings, DoorOpenDirection.Inwards);
                CreateDoorTrigger(door, buildingDoorSettings, DoorOpenDirection.Outwards);
            }
            else
            {
                // We want to reconfigure existing triggers
                foreach (DoorTrigger existingDoorTrigger in doorTriggers)
                {
                    ConfigureDoorTrigger(door, existingDoorTrigger, buildingDoorSettings, existingDoorTrigger.DoorOpenDirection);
                }
            }
        }

        private static void CreateDoorTrigger(Door door, BuildingDoorSettings buildingDoorSettings, DoorOpenDirection openDirection)
        {
            string gameObjectName = openDirection == DoorOpenDirection.Outwards ? "Inside Trigger" : "Outside Trigger";
            GameObject triggerGameObject = new(gameObjectName);
            triggerGameObject.transform.SetParent(door.gameObject.transform);
            triggerGameObject.transform.localPosition = Vector3.zero;
            triggerGameObject.transform.localRotation = Quaternion.identity;
            triggerGameObject.EnsureComponent<BoxCollider>();
            DoorTrigger trigger = triggerGameObject.EnsureComponent<DoorTrigger>();
            ConfigureDoorTrigger(door, trigger, buildingDoorSettings, openDirection);
        }

        private static void ConfigureDoorTrigger(Door door, DoorTrigger doorTrigger, BuildingDoorSettings buildingDoorSettings, DoorOpenDirection openDirection)
        {
            doorTrigger.ConfigureInEditor(door, buildingDoorSettings.doorTriggerLayerMask, buildingDoorSettings.doorTriggerTags, openDirection);
            doorTrigger.transform.parent.gameObject.GetMeshSize(~0, new string[] { }, out Vector3 meshSize, out Vector3 _);
            float distanceFromDoor = openDirection == DoorOpenDirection.Inwards ? 0.3f : -(0.3f + meshSize.x);
            float triggerWidth = meshSize.z;
            float triggerLocalCenter = meshSize.z / 2;

            BoxCollider boxCollider = doorTrigger.GetComponent<BoxCollider>();

            boxCollider.size = new Vector3(1.0f, 1.0f, triggerWidth);
            boxCollider.center = new Vector3(distanceFromDoor, 0, triggerLocalCenter);
            boxCollider.isTrigger = true;
        }

        #endregion
    }
}