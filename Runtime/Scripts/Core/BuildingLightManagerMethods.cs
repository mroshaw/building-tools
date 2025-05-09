using DaftAppleGames.Buildings;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.Core
{
    /// <summary>
    /// This component acts as a proxy for the singleton BuildingLightManager
    /// You can add this component as many times you want into as many scenes as you want
    /// You can then hook event calls into these public methods, effectively invoking the singleton methods
    /// </summary>
    public class BuildingLightManagerMethods : MonoBehaviour
    {
        public void TurnOnCandleLights()
        {
            BuildingLightManager.Instance.TurnOnCandleLights();
        }

        public void TurnOffCandleLights()
        {
            BuildingLightManager.Instance.TurnOffCandleLights();
        }

        public void ToggleCandleLights()
        {
            BuildingLightManager.Instance.ToggleCandleLights();
        }

        public void TurnOnCookingLights()
        {
            BuildingLightManager.Instance.TurnOnCookingLights();
        }

        public void TurnOffCookingLights()
        {
            BuildingLightManager.Instance.TurnOffCookingLights();
        }

        public void ToggleCookingLights()
        {
            BuildingLightManager.Instance.ToggleCookingLights();
        }

        public void TurnOnOutsideLights()
        {
            BuildingLightManager.Instance.TurnOnOutsideLights();
        }

        public void TurnOffOutsideLights()
        {
            BuildingLightManager.Instance.TurnOffOutsideLights();
        }

        public void ToggleOutsideLights()
        {
            BuildingLightManager.Instance.ToggleOutsideLights();
        }
    }
}