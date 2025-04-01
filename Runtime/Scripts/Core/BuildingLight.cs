#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;

namespace DaftAppleGames.Darskerry.Core.Buildings
{
    public enum BuildingLightType
    {
        IndoorCandle,
        IndoorFire,
        IndoorCooking,
        OutdoorCandle,
        OutdoorFire
    }

    public class BuildingLight : MonoBehaviour
    {
        #region Class properties

        [BoxGroup("Settings")] [SerializeField] private bool findLightsOnAwake;
        [BoxGroup("Settings")] [SerializeField] public BuildingLightType buildingLightType;
        [BoxGroup("Lights")] [SerializeField] private Light light;
        [BoxGroup("Lights")] [SerializeField] private ParticleSystem particles;
        public BuildingLightType BuildingLightType => buildingLightType;

        #endregion

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
            light = gameObject.GetComponentInChildren<Light>(true);
            particles = gameObject.GetComponentInChildren<ParticleSystem>(true);
        }

        public Light GetLights()
        {
            return light;
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
            light.gameObject.SetActive(state);
            particles.gameObject.SetActive(state);
        }

        #endregion


        #region Unity Editor methods
        #if UNITY_EDITOR
        public void ConfigureInEditor(BuildingLightType newLightType, Light newLight, ParticleSystem newParticles)
        {
            buildingLightType = newLightType;
            light = newLight;
            particles = newParticles;
        }
        #endif
        #endregion

    }
}