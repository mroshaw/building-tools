#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;

namespace DaftAppleGames.Buildings
{
    public enum BuildingLightType
    {
        IndoorCandle,
        IndoorFire,
        OutdoorCandle,
        OutdoorFire
    }

    public class BuildingLight : MonoBehaviour
    {
        [BoxGroup("Settings")] [SerializeField] private bool findLightsOnAwake;
        [BoxGroup("Settings")] [SerializeField] public BuildingLightType buildingLightType;
        [BoxGroup("Lights")] [SerializeField] private Light lightComponent;
        [BoxGroup("Lights")] [SerializeField] private ParticleSystem particles;
        public BuildingLightType BuildingLightType => buildingLightType;
        private bool _lightState;

        private void Awake()
        {
            if (findLightsOnAwake)
            {
                UpdateLights();
            }
        }

        /// <summary>
        /// Allow light to be configured externally, outside of Unity inspector
        /// </summary>
        public void ConfigureLight(BuildingLightType newLightType, Light newLight, ParticleSystem newParticles)
        {
            buildingLightType = newLightType;
            lightComponent = newLight;
            particles = newParticles;
        }

        [Button("Update Lights")]
        public void UpdateLights()
        {
            lightComponent = gameObject.GetComponentInChildren<Light>(true);
            particles = gameObject.GetComponentInChildren<ParticleSystem>(true);
        }

        public Light GetLights()
        {
            return lightComponent;
        }

        [Button("Turn On")]
        public void TurnOnLights()
        {
            SetLightState(true);
        }

        [Button("Turn Off")]
        public void TurnOffLights()
        {
            SetLightState(false);
        }

        public void SetLightState(bool state)
        {
            lightComponent.gameObject.SetActive(state);
            particles.gameObject.SetActive(state);
            _lightState = state;
        }

        public bool GetLightState()
        {
            return _lightState;
        }
    }
}