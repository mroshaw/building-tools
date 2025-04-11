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
        [BoxGroup("Settings")] [SerializeField] private bool defaultState;
        [BoxGroup("Indoor Candle Lights")] [SerializeField] private List<BuildingLight> candleLights;
        [BoxGroup("Indoor Candle Lights")] [SerializeField] private bool candleDefaultState;
        [BoxGroup("Indoor Candle Lights")] [SerializeField] private bool candleCurrentState;
        [BoxGroup("Indoor Fire Lights")] [SerializeField] private List<BuildingLight> indoorFireLights;
        [BoxGroup("Indoor Fire Lights")] [SerializeField] private bool indoorFireDefaultState = true;
        [BoxGroup("Indoor Fire Lights")] [SerializeField] private bool indoorFireCurrentState;
        [BoxGroup("Outdoor Lights")] [SerializeField] private List<BuildingLight> outdoorLights;
        [BoxGroup("Outdoor Lights")] [SerializeField] private bool outdoorLightDefaultState;
        [BoxGroup("Outdoor Lights")] [SerializeField] private bool outdoorLightCurrentState;

        #region Startup

        private void Awake()
        {
            if (findLightsOnAwake)
            {
                UpdateLightLists();
            }

            SetDefaultState();
        }

        private void SetDefaultState()
        {
            SetCandleLightsState(candleDefaultState);
            SetFireLightsState(indoorFireDefaultState);
            SetOutdoorLightsState(outdoorLightDefaultState);
        }

        private void SetCandleLightsState(bool state)
        {
            candleCurrentState = state;
            SetLightsState(candleLights, state);
        }

        private void SetFireLightsState(bool state)
        {
            indoorFireCurrentState = state;
            SetLightsState(indoorFireLights, state);
        }

        private void SetOutdoorLightsState(bool state)
        {
            outdoorLightCurrentState = state;
            SetLightsState(outdoorLights, state);
        }

        #endregion

        #region Class Methods

        [Button("Update Light Lists")]
        private void UpdateLightLists()
        {
            candleLights = new List<BuildingLight>();
            indoorFireLights = new List<BuildingLight>();
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
                        indoorFireLights.Add(currLight);
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
            SetCandleLightsState(true);
        }

        [Button("Turn Off Candles")]
        public void TurnOffCandleLights()
        {
            SetCandleLightsState(false);
        }

        [Button("Turn On Outdoor Lights")]
        public void TurnOnOutdoorLights()
        {
            SetOutdoorLightsState(true);
        }

        [Button("Turn Off Outdoor Lights")]
        public void TurnOffOutdoorLights()
        {
            SetOutdoorLightsState(false);
        }

        [Button("Turn On Fire Lights")]
        public void TurnOnFireLights()
        {
            SetFireLightsState(true);
        }

        [Button("Turn Off Fire Lights")]
        public void TurnOffCookingLights()
        {
            SetFireLightsState(false);
        }


        private static void SetLightsState(List<BuildingLight> lights, bool state)
        {
            foreach (BuildingLight currLight in lights)
            {
                currLight.SetLightState(state);
            }
        }

        #endregion

        #region Unity Editor Methods

#if UNITY_EDITOR
        public void ConfigureInEditor()
        {
            UpdateLightLists();
        }
#endif

        #endregion
    }
}