using System;
using System.Collections.Generic;
using UnityEngine;

namespace DaftAppleGames.Buildings
{
    [Serializable]
    internal class LightList
    {
        [SerializeField] private BuildingLightType lightType;
        [SerializeField] private List<BuildingLight> lightList;
        [SerializeField] private bool defaultState;
        [SerializeField] private bool currentState;

        internal LightList(BuildingLightType lightType)
        {
            this.lightType = lightType;
        }

        internal void SetDefaultState()
        {
            SetLightState(defaultState);
        }

        internal void SetLightState(bool state)
        {
            foreach (BuildingLight light in lightList)
            {
                light.SetLightState(state);
                currentState = state;
            }
        }

        internal void ToggleLightState()
        {
            foreach (BuildingLight light in lightList)
            {
                light.SetLightState(!light.GetLightState());
            }
        }

        internal void RefreshLightList(GameObject parentGameObject)
        {
            BuildingLight[] allLights = parentGameObject.GetComponentsInChildren<BuildingLight>(true);
            lightList = new List<BuildingLight>();

            foreach (BuildingLight currLight in allLights)
            {
                currLight.UpdateLights();

                if (currLight.BuildingLightType == lightType)
                {
                    lightList.Add(currLight);
                }
            }
        }
    }
}