#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;

namespace DaftAppleGames.Buildings
{
    /// <summary>
    /// Controls all the lights in the building
    /// </summary>
    public class BuildingLightController : MonoBehaviour
    {
        [BoxGroup("Settings")] [SerializeField] private bool findLightsOnAwake;
        [BoxGroup("Settings")] [SerializeField] private bool defaultState;
        [SerializeField] private LightList candleLights = new(BuildingLightType.IndoorCandle);
        [SerializeField] private LightList indoorFireLights = new(BuildingLightType.IndoorFire);
        [SerializeField] private LightList outdoorLights = new(BuildingLightType.OutdoorCandle);

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
            candleLights.SetDefaultState();
            indoorFireLights.SetDefaultState();
            outdoorLights.SetDefaultState();
        }

        [Button("Update Light Lists")]
        public void UpdateLightLists()
        {
            candleLights.RefreshLightList(gameObject);
            indoorFireLights.RefreshLightList(gameObject);
            outdoorLights.RefreshLightList(gameObject);
        }


        [Button("Turn On Candles")]
        public void TurnOnCandleLights()
        {
            candleLights.SetLightState(true);
        }

        [Button("Turn Off Candles")]
        public void TurnOffCandleLights()
        {
            candleLights.SetLightState(false);
        }

        [Button("Toggle Candles")]
        public void ToggleCandleLights()
        {
            candleLights.ToggleLightState();
        }

        public void SetCandleLightsState(bool state)
        {
            candleLights.SetLightState(state);
        }

        [Button("Turn On Outdoor Lights")]
        public void TurnOnOutdoorLights()
        {
            outdoorLights.SetLightState(true);
        }

        [Button("Turn Off Outdoor Lights")]
        public void TurnOffOutdoorLights()
        {
            outdoorLights.SetLightState(false);
        }

        [Button("Toggle Outdoor Lights")]
        public void ToggleOutdoorLights()
        {
            outdoorLights.ToggleLightState();
        }

        public void SetOutdoorLightsState(bool state)
        {
            outdoorLights.SetLightState(state);
        }

        [Button("Turn On Interior Fire Lights")]
        public void TurnOnFireLights()
        {
            indoorFireLights.SetLightState(true);
        }

        [Button("Turn Off Interior Fire Lights")]
        public void TurnOffCookingLights()
        {
            indoorFireLights.SetLightState(true);
        }

        [Button("Toggle Interior Fire Lights")]
        public void ToggleCookingLights()
        {
            indoorFireLights.ToggleLightState();
        }

        public void SetCookingLightsState(bool state)
        {
            indoorFireLights.SetLightState(state);
        }
    }
}