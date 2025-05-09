using DaftAppleGames.Gameplay;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;

namespace DaftAppleGames.Buildings
{
    public enum DoorTriggerLocation
    {
        Inside,
        Outside
    }

    [RequireComponent(typeof(BoxCollider))]
    public class DoorTrigger : ActionTrigger
    {
        [SerializeField] private Door door;
        [SerializeField] private DoorTriggerLocation doorTriggerLocation;

        public DoorTriggerLocation DoorTriggerLocation
        {
            set => doorTriggerLocation = value;
        }

        public Door Door
        {
            set => door = value;
        }

        private void Start()
        {
            // Move from parent so not animated with the door
            if (!door.transform.parent)
            {
                return;
            }

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
            door.OpenAndCloseDoor(doorTriggerLocation);
        }


        [Button("Open Door")]
        private void OpenDoor()
        {
            door.OpenDoor(doorTriggerLocation, false);
        }

        [Button("Close Door")]
        private void CloseDoor()
        {
            door.CloseDoor();
        }
    }
}