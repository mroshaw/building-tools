using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEditor;
using UnityEngine;

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "ConfigureDoorsEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Doors Tool")]
    internal class ConfigureDoorsEditorTool : BuildingEditorTool
    {
        protected override string GetToolName()
        {
            return "Configure Doors";
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
                ConfigureDoors(selectedGameObject, buildingEditorSettings);
            }
        }

        #region Static Door methods

        private static void ConfigureDoors(GameObject parentGameObject, BuildingWizardEditorSettings buildingWizardSettings)
        {
            DoorController doorController = parentGameObject.EnsureComponent<DoorController>();
            log.Log(LogLevel.Debug, $"Added Door Controller component to {parentGameObject.name}.");

            MeshRenderer[] allMeshRenderers = parentGameObject.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer renderer in allMeshRenderers)
            {
                if (!buildingWizardSettings.doorNames.ItemInString(renderer.gameObject.name))
                {
                    continue;
                }

                Door newDoor = ConfigureDoor(renderer.gameObject, buildingWizardSettings);
                doorController.AddDoor(newDoor);
            }
        }

        private static Door ConfigureDoor(GameObject doorGameObject, BuildingWizardEditorSettings buildingWizardSettings)
        {
            log.Log(LogLevel.Info, $"Configuring door on {doorGameObject.name}.");
            Door door = doorGameObject.EnsureComponent<Door>();
            // We don't want to combine this mesh, as it needs to move
            doorGameObject.EnsureComponent<MeshCombineExcluder>();
            // Set the static flags, as the door will move
            GameObjectUtility.SetStaticEditorFlags(door.gameObject, buildingWizardSettings.moveableMeshFlags);
            door.ConfigureInEditor(buildingWizardSettings.doorSfxGroup, buildingWizardSettings.doorOpeningClips, buildingWizardSettings.doorOpenClips,
                buildingWizardSettings.doorClosingClips,
                buildingWizardSettings.doorClosedClips);
            CreateOrUpdateDoorTriggers(door, buildingWizardSettings);
            return door;
        }


        private static void CreateOrUpdateDoorTriggers(Door door, BuildingWizardEditorSettings buildingWizardSettings)
        {
            DoorTrigger[] doorTriggers = door.GetComponentsInChildren<DoorTrigger>(true);

            if (doorTriggers.Length == 0)
            {
                // We need to create new door triggers
                CreateDoorTrigger(door, buildingWizardSettings, DoorOpenDirection.Inwards);
                CreateDoorTrigger(door, buildingWizardSettings, DoorOpenDirection.Outwards);
            }
            else
            {
                // We want to reconfigure existing triggers
                foreach (DoorTrigger existingDoorTrigger in doorTriggers)
                {
                    ConfigureDoorTrigger(door, existingDoorTrigger, buildingWizardSettings, existingDoorTrigger.DoorOpenDirection);
                }
            }
        }

        private static void CreateDoorTrigger(Door door, BuildingWizardEditorSettings buildingWizardSettings, DoorOpenDirection openDirection)
        {
            string gameObjectName = openDirection == DoorOpenDirection.Outwards ? "Inside Trigger" : "Outside Trigger";
            GameObject triggerGameObject = new(gameObjectName);
            triggerGameObject.transform.SetParent(door.gameObject.transform);
            triggerGameObject.transform.localPosition = Vector3.zero;
            triggerGameObject.transform.localRotation = Quaternion.identity;
            triggerGameObject.EnsureComponent<BoxCollider>();
            DoorTrigger trigger = triggerGameObject.EnsureComponent<DoorTrigger>();
            ConfigureDoorTrigger(door, trigger, buildingWizardSettings, openDirection);
        }

        private static void ConfigureDoorTrigger(Door door, DoorTrigger doorTrigger, BuildingWizardEditorSettings buildingWizardSettings, DoorOpenDirection openDirection)
        {
            doorTrigger.ConfigureInEditor(door, buildingWizardSettings.doorTriggerLayerMask, buildingWizardSettings.doorTriggerTags, openDirection);
            MeshTools.GetMeshSize(doorTrigger.transform.parent.gameObject, ~0, new string[] { }, out Vector3 meshSize, out Vector3 _);
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