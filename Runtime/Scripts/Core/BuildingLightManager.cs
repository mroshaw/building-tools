using System.Collections.Generic;
using DaftAppleGames.Utilities;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.Buildings
{
    public class BuildingLightManager : Singleton<BuildingLightManager>
    {
        [BoxGroup("Settings")] [SerializeField] private bool findLightControllersOnStart;
        [SerializeField] private List<BuildingLightController> lightControllers;

        private void Start()
        {
            if (findLightControllersOnStart)
            {
                RefreshLightControllers();
            }
        }

        [Button("Refresh Light Controllers")]
        public void RefreshLightControllers()
        {
            BuildingLightController[] allLightControllers = FindObjectsByType<BuildingLightController>(FindObjectsSortMode.None);
            lightControllers = new List<BuildingLightController>(allLightControllers);
        }

        [Button("Turn On All Candles")]
        public void TurnOnCandleLights()
        {
            ApplyLightController(BuildingLightType.IndoorCandle, true, false);
        }

        [Button("Turn Off All Candles")]
        public void TurnOffCandleLights()
        {
            ApplyLightController(BuildingLightType.IndoorCandle, false, false);
        }

        public void SetCandleLightState(bool state)
        {
            ApplyLightController(BuildingLightType.IndoorCandle, state, false);
        }

        [Button("Toggle All Candles")]
        public void ToggleCandleLights()
        {
            ApplyLightController(BuildingLightType.IndoorCandle, false, true);
        }

        [Button("Turn On All Interior Fires")]
        public void TurnOnCookingLights()
        {
            ApplyLightController(BuildingLightType.IndoorFire, true, false);
        }

        [Button("Turn Off All Interior Fires")]
        public void TurnOffCookingLights()
        {
            ApplyLightController(BuildingLightType.IndoorFire, false, false);
        }

        public void SetCookingLightState(bool state)
        {
            ApplyLightController(BuildingLightType.IndoorFire, state, false);
        }

        [Button("Toggle All Interior Fires")]
        public void ToggleCookingLights()
        {
            ApplyLightController(BuildingLightType.IndoorFire, false, true);
        }

        [Button("Turn On All Outside Lights")]
        public void TurnOnOutsideLights()
        {
            ApplyLightController(BuildingLightType.OutdoorCandle, true, false);
        }

        [Button("Turn Off All Outside Lights")]
        public void TurnOffOutsideLights()
        {
            ApplyLightController(BuildingLightType.OutdoorCandle, false, false);
        }

        public void SetOutsideLightState(bool state)
        {
            ApplyLightController(BuildingLightType.OutdoorCandle, state, false);
        }

        [Button("Toggle All Outside Lights")]
        public void ToggleOutsideLights()
        {
            ApplyLightController(BuildingLightType.OutdoorCandle, false, true);
        }

        private void ApplyLightController(BuildingLightType lightType, bool state, bool toggle)
        {
            foreach (BuildingLightController lightController in lightControllers)
            {
                switch (lightType)
                {
                    case BuildingLightType.IndoorCandle:
                    {
                        if (toggle)
                        {
                            lightController.ToggleCandleLights();
                        }
                        else
                        {
                            lightController.SetCandleLightsState(state);
                        }

                        break;
                    }
                    case BuildingLightType.IndoorFire:
                    {
                        if (toggle)
                        {
                            lightController.ToggleCookingLights();
                        }
                        else
                        {
                            lightController.SetCookingLightsState(state);
                        }

                        break;
                    }

                    case BuildingLightType.OutdoorCandle:
                    {
                        if (toggle)
                        {
                            lightController.ToggleOutdoorLights();
                        }
                        else
                        {
                            lightController.SetOutdoorLightsState(state);
                        }

                        break;
                    }
                }
            }
        }
    }
}