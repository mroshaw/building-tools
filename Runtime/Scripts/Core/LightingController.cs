using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;

namespace DaftAppleGames.Buildings
{
    public class LightingController : MonoBehaviour
    {
        [BoxGroup("Settings")] [SerializeField] private bool findLightsOnAwake;
        [BoxGroup("Building Lights")] [SerializeField] private List<BuildingLight> candleLights;
        [BoxGroup("Building Lights")] [SerializeField] private List<BuildingLight> cookingLights;
        [BoxGroup("Building Lights")] [SerializeField] private List<BuildingLight> outdoorLights;

        #region Startup

        private void Awake()
        {
            if (findLightsOnAwake)
            {
                UpdateLights();
            }
        }

        #endregion

        #region Class Methods

        [Button("Update Lights")]
        public void UpdateLights()
        {
            candleLights = new List<BuildingLight>();
            cookingLights = new List<BuildingLight>();
            outdoorLights = new List<BuildingLight>();
            BuildingLight[] allLights = gameObject.GetComponentsInChildren<BuildingLight>(true);
            foreach (BuildingLight currLight in allLights)
            {
                currLight.UpdateLights();

                switch (currLight.BuildingLightType)
                {
                    case BuildingLightType.IndoorCandle:
                        candleLights.Add(currLight);
                        break;
                    case BuildingLightType.IndoorFire:
                        cookingLights.Add(currLight);
                        break;
                    case BuildingLightType.OutdoorCandle:
                        outdoorLights.Add(currLight);
                        break;
                }
            }
        }

        [Button("Turn On Candles")]
        public void TurnOnCandleLights()
        {
            SetLightsState(candleLights, true);
        }

        [Button("Turn Off Candles")]
        public void TurnOffCandleLights()
        {
            SetLightsState(candleLights, false);
        }

        [Button("Turn On Outdoor Lights")]
        public void TurnOnOutdoorLights()
        {
            SetLightsState(outdoorLights, true);
        }

        [Button("Turn Off Outdoor Lights")]
        public void TurnOffOutdoorLights()
        {
            SetLightsState(outdoorLights, false);
        }

        [Button("Turn On Cooking")]
        public void TurnOnCookingLights()
        {
            SetLightsState(cookingLights, true);
        }

        [Button("Turn Off Cooking")]
        public void TurnOffCookingLights()
        {
            SetLightsState(cookingLights, false);
        }


        private static void SetLightsState(List<BuildingLight> lights, bool state)
        {
            foreach (BuildingLight currLight in lights)
            {
                currLight.SetLightState(state);
            }
        }

        #endregion
    }
}