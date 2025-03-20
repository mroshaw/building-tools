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
        [BoxGroup("Lights")] [SerializeField] private Light[] lights;
        [BoxGroup("Lights")] [SerializeField] private ParticleSystem[] particles;
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
            lights = gameObject.GetComponentsInChildren<Light>(true);
            particles = gameObject.GetComponentsInChildren<ParticleSystem>(true);
        }

        public Light[] GetLights()
        {
            return lights;
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
            foreach (Light currLight in lights)
            {
                currLight.gameObject.SetActive(state);
            }

            foreach (ParticleSystem currParticle in particles)
            {
                currParticle.gameObject.SetActive(state);
            }
        }

        #endregion
    }
}