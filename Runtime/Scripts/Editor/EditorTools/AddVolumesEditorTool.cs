using System.Collections.Generic;
using DaftAppleGames.Editor;
using DaftAppleGames.Editor.Extensions;
using DaftAppleGames.Extensions;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "ConfigureVolumesEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Volumes Tool")]
    internal class AddVolumesEditorTool : BuildingEditorTool
    {
        [SerializeField] [BoxGroup("Settings")]
        [Tooltip(
            "When determining the \"bounds\" of a building, any mesh renderer Game Objects containing these names are ignored by the calculation, as are any of their children. Can be used to remove \"invisible\" meshes, such as those under the ground or that \"overhang\" the interior space, from the calculation.")]
        internal string[] meshSizeIgnoreNames;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("LayerMask containing layers that will be considered by the mesh size calculation. Only mesh renderers within these layers will be considered.")]
        internal LayerMask meshSizeIncludeLayers;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Name of the Game Object created, if not there already, that contains the various volume components.")] internal string interiorVolumeGameObjectName;
#if DAG_HDRP || DAG_URP
        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("Selected Volume Profile that will be applied when the player/camera enters the building.")] internal VolumeProfile interiorVolumeProfile;
#endif
        [SerializeField] [BoxGroup("Settings")]
        [Tooltip(
            "AudioMixer Snapshot corresponding to the AudioMixer profile that is applied when the player/camera enters the building. Typically contains \"Lowbypass/Highbypass\" filters to \"muffle\" the audio.")]
        internal AudioMixerSnapshot indoorSnapshot;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("AudioMixer Snapshot corresponding to the AudioMixer profile that is applied when the player/camera exists the building. Typically a \"clean\" snapshot profile.")]
        internal AudioMixerSnapshot outdoorSnapshot;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("When a collider enters the volume trigger collider, only colliders on Game Objects with one of these tags will trigger the volume effect.")]
        internal string[] volumeTriggerTags;

        [SerializeField] [BoxGroup("Settings")]
        [Tooltip("When a collider enters the volume trigger collider, only colliders on Game Objects on one of these layerswill trigger the volume effect.")]
        internal LayerMask volumeTriggerLayerMask;

        protected override string GetToolName()
        {
            return "Add Volumes";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(out List<string> cannotRunReasons)
        {
            bool canRun = true;

            cannotRunReasons = new List<string>();
            if (!RequireGameObjectValidation(out string requireGameObjectReason))
            {
                cannotRunReasons.Add(requireGameObjectReason);
                return false;
            }

            if (!HasRequiredBuildingComponent(out string requiredBuildingReason))
            {
                cannotRunReasons.Add(requiredBuildingReason);
                canRun = false;
            }

            if (!HasMeshExteriorLayerConfigured(out string layerFailedReason))
            {
                cannotRunReasons.Add(layerFailedReason);
                canRun = false;
            }

            return canRun;
        }

        protected override void RunTool(string undoGroupName)
        {
#if DAG_HDRP || DAG_UURP
            log.AddToLog(LogLevel.Info, "Configuring Lighting Volume...");
            AddInteriorLightingVolume();
            log.AddToLog(LogLevel.Info, "Configuring Lighting Volume... DONE!");
#endif
            log.AddToLog(LogLevel.Info, "Configuring Audio Volume...");
            AddInteriorAudioVolume();
            log.AddToLog(LogLevel.Info, "Configuring Audio Volume... DONE!");
        }

#if DAG_HDRP || DAG_UURP
        private void AddInteriorLightingVolume()
        {
            log.AddToLog(LogLevel.Debug, $"Adding Interior Lighting Volume to {SelectedGameObject.name}...");
            GameObject volumeGameObject = ConfigureVolumeGameObject();
            Volume interiorVolume = volumeGameObject.EnsureComponent<Volume>();
            interiorVolume.sharedProfile = interiorVolumeProfile;
            interiorVolume.isGlobal = false;
        }
#endif
        private void AddInteriorAudioVolume()
        {
            log.AddToLog(LogLevel.Debug, $"Adding Interior Audio Volume to {SelectedGameObject.name}...");
            GameObject volumeGameObject = ConfigureVolumeGameObject();
            InteriorAudioFilter audioFilter = volumeGameObject.EnsureComponent<InteriorAudioFilter>();
            audioFilter.TriggerLayerMask = volumeTriggerLayerMask;
            audioFilter.TriggerTags = volumeTriggerTags;
            audioFilter.SetSnapShots(indoorSnapshot, outdoorSnapshot);
            log.AddToLog(LogLevel.Debug, $"Adding Interior Audio Volume to {SelectedGameObject.name}... DONE!");
        }

        private GameObject ConfigureVolumeGameObject()
        {
            SelectedGameObject.GetLocalMeshDimensions(meshSizeIncludeLayers, meshSizeIgnoreNames, out Vector3 buildingCenter, out Vector3 buildingSize);
            log.AddToLog(LogLevel.Debug, $"Building size calculated as: {buildingSize}");
            log.AddToLog(LogLevel.Debug, $"Center point is: {buildingCenter}");

            GameObject volumeGameObject = SelectedGameObject.FindChildGameObject(interiorVolumeGameObjectName);
            if (!volumeGameObject)
            {
                volumeGameObject = new GameObject(interiorVolumeGameObjectName)
                {
                    transform =
                    {
                        parent = SelectedGameObject.transform,
                        localPosition = Vector3.zero,
                        localScale = Vector3.one,
                        localRotation = Quaternion.identity
                    }
                };
            }

            BoxCollider volumeCollider = volumeGameObject.gameObject.EnsureComponent<BoxCollider>();
            volumeCollider.isTrigger = true;
            volumeCollider.center = buildingCenter;
            volumeCollider.size = buildingSize;

            return volumeGameObject;
        }
    }
}