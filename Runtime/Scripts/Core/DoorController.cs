using System.Collections.Generic;
using System.Linq;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;

namespace DaftAppleGames.Buildings
{
    public class DoorController : MonoBehaviour
    {
        [BoxGroup("Settings")] [SerializeField] private bool findDoorsOnAwake;
        [BoxGroup("Building Doors")] [SerializeField] private List<Door> doors = new();

        private void Awake()
        {
            if (findDoorsOnAwake)
            {
                RefreshDoorList();
            }
        }

        [Button("Refresh Door List")]
        private void RefreshDoorList()
        {
            Door[] allDoors = GetComponentsInChildren<Door>(true);
            doors = allDoors.ToList();
        }

        public void CloseAllDoors(bool immediate = true)
        {
            foreach (Door door in doors)
            {
                door.CloseDoor(immediate);
            }
        }

        public void AddDoor(Door newDoor)
        {
            if (!doors.Contains(newDoor))
            {
                doors.Add(newDoor);
            }
        }

#if UNITY_EDITOR
        [Button("Close Doors")]
        private void CloseDoorsEditor()
        {
            CloseAllDoors(true);
        }
#endif
    }
}