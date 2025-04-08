using DaftAppleGames.Gameplay;
using UnityEngine;
using UnityEngine.Audio;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.BuildingTools
{
    /// <summary>
    /// Provides a simple means to apply a "muffled" effect to ambient / outdoor audio
    /// while inside a building or interior.
    /// </summary>
    public class InteriorAudioFilter : ActionTrigger
    {
        #region Class Variables

        [BoxGroup("Snapshots")] [SerializeField] private AudioMixerSnapshot indoorSnapshot;
        [BoxGroup("Snapshots")] [SerializeField] private AudioMixerSnapshot outdoorSnapshot;
        [BoxGroup("Settings")] public float transitionTime = 0.1f;

        #endregion

        #region Class methods

        protected override void TriggerEnter(Collider other)
        {
            FadeInFilters();
        }

        protected override void TriggerExit(Collider other)
        {
            FadeOutFilters();
        }

        /// <summary>
        /// Fade in the filter effects
        /// </summary>
        [Button("Fade In")]
        private void FadeInFilters()
        {
            indoorSnapshot.TransitionTo(transitionTime);
        }

        /// <summary>
        /// Fade out the filter effects
        /// </summary>
        [Button("Fade Out")]
        private void FadeOutFilters()
        {
            outdoorSnapshot.TransitionTo(transitionTime);
        }

        #endregion
    }
}