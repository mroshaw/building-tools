using System.Collections.Generic;
using System.Linq;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;

namespace DaftAppleGames.Darskerry.Core.Buildings
{
    public class DoorController : MonoBehaviour
    {
        [BoxGroup("Settings")] [SerializeField] private bool findLightsOnAwake;
        [BoxGroup("Building Doors")] [SerializeField] private List<Door> doors = new();

        [Button("Refresh Door List")]
        private void RefreshDoorList()
        {
            Door[] allDoors = GetComponentsInChildren<Door>(true);
            doors = allDoors.ToList();
        }

        [Button("Close All Doors")]
        public void CloseAllDoors(bool immediate = true)
        {
            foreach (Door door in doors)
            {
                door.CloseDoor(immediate);
            }
        }

        [Button("Open All Doors (Inwards")]
        public void OpenAllDoorsInwards(bool immediate = true)
        {
            OpenAllDoors(DoorOpenDirection.Inwards, immediate);
        }

        [Button("Open All Doors (Outwards")]
        public void OpenAllDoorsOutwards(bool immediate = true)
        {
            OpenAllDoors(DoorOpenDirection.Outwards, immediate);
        }


        private void OpenAllDoors(DoorOpenDirection doorOpenDirection, bool immediate)
        {
            foreach (Door door in doors)
            {
                door.OpenDoor(doorOpenDirection, immediate);
            }
        }

        public void AddDoor(Door newDoor)
        {
            if (!doors.Contains(newDoor))
            {
                doors.Add(newDoor);
            }
        }
    }
}