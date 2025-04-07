using DaftAppleGames.Gameplay;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;

namespace DaftAppleGames.Darskerry.Core.Buildings
{
    [RequireComponent(typeof(BoxCollider))]
    public class DoorTrigger : ActionTrigger
    {
        [SerializeField] private Door door;
        [SerializeField] private DoorOpenDirection doorOpenDirection;

        public DoorOpenDirection DoorOpenDirection => doorOpenDirection;

        private void Start()
        {
            // Move from parent so not animated with the door
            GameObject doorParent = door.transform.parent.gameObject;
            gameObject.transform.SetParent(doorParent.transform);
        }

        protected override void TriggerEnter(Collider other)
        {
            OpenDoor();
        }

        protected override void TriggerExit(Collider other)
        {
            CloseDoor();
        }

        [Button("Open and Close Door")]
        private void OpenAndCloseDoor()
        {
            door.OpenAndCloseDoor(doorOpenDirection);
        }


        [Button("Open Door")]
        private void OpenDoor()
        {
            door.OpenDoor(doorOpenDirection);
        }

        [Button("Close Door")]
        private void CloseDoor()
        {
            door.CloseDoor();
        }

        #region Unity Editor methods

#if UNITY_EDITOR
        public void ConfigureInEditor(Door newDoor, LayerMask newTriggerLayerMask, string[] newTriggerTags, DoorOpenDirection newDoorOpenDirection)
        {
            door = newDoor;
            doorOpenDirection = newDoorOpenDirection;
            base.ConfigureInEditor(newTriggerLayerMask, newTriggerTags);
        }
#endif

        #endregion
    }
}