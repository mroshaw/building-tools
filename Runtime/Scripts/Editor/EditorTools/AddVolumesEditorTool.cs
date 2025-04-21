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
        [SerializeField] [BoxGroup("Settings")] internal string[] meshSizeIgnoreNames;
        [SerializeField] [BoxGroup("Settings")] internal LayerMask meshSizeIncludeLayers;
        [SerializeField] [BoxGroup("Settings")] internal string interiorVolumeGameObjectName;
#if DAG_HDRP || DAG_UURP
        [SerializeField] [BoxGroup("Settings")] internal VolumeProfile interiorVolumeProfile;
#endif
        [SerializeField] [BoxGroup("Settings")] internal AudioMixerSnapshot indoorSnapshot;
        [SerializeField] [BoxGroup("Settings")] internal AudioMixerSnapshot outdoorSnapshot;
        [SerializeField] [BoxGroup("Settings")] internal string[] volumeTriggerTags;
        [SerializeField] [BoxGroup("Settings")] internal LayerMask volumeTriggerLayerMask;

        protected override string GetToolName()
        {
            return "Add Volumes";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, out List<string> cannotRunReasons)
        {
            bool canRun = true;

            cannotRunReasons = new List<string>();
            if (!RequireGameObjectValidation(out string requireGameObjectReason))
            {
                cannotRunReasons.Add(requireGameObjectReason);
                canRun = false;
            }

            if (!RequiredBuildingValidation(out string requiredBuildingReason))
            {
                cannotRunReasons.Add(requiredBuildingReason);
                canRun = false;
            }

            return canRun;
        }

        protected override void RunTool(GameObject selectedGameObject, string undoGroupName)
        {
#if DAG_HDRP || DAG_UURP
            log.AddToLog(LogLevel.Info, "Configuring Lighting Volume...");
            AddInteriorLightingVolume(selectedGameObject);
            log.AddToLog(LogLevel.Info, "Configuring Lighting Volume... DONE!");
#endif
            log.AddToLog(LogLevel.Info, "Configuring Audio Volume...");
            AddInteriorAudioVolume(selectedGameObject);
            log.AddToLog(LogLevel.Info, "Configuring Audio Volume... DONE!");
        }

#if DAG_HDRP || DAG_UURP
        private void AddInteriorLightingVolume(GameObject parentGameObject)
        {
            log.AddToLog(LogLevel.Debug, $"Adding Interior Lighting Volume to {parentGameObject.name}...");
            GameObject volumeGameObject = ConfigureVolumeGameObject(parentGameObject);
            Volume interiorVolume = volumeGameObject.EnsureComponent<Volume>();
            interiorVolume.sharedProfile = interiorVolumeProfile;
            interiorVolume.isGlobal = false;
        }
#endif
        private void AddInteriorAudioVolume(GameObject parentGameObject)
        {
            log.AddToLog(LogLevel.Debug, $"Adding Interior Audio Volume to {parentGameObject.name}...");
            GameObject volumeGameObject = ConfigureVolumeGameObject(parentGameObject);
            InteriorAudioFilter audioFilter = volumeGameObject.EnsureComponent<InteriorAudioFilter>();
            audioFilter.ConfigureInEditor(volumeTriggerLayerMask, volumeTriggerTags,
                indoorSnapshot, outdoorSnapshot);
            log.AddToLog(LogLevel.Debug, $"Adding Interior Audio Volume to {parentGameObject.name}... DONE!");
        }

        private GameObject ConfigureVolumeGameObject(GameObject parentGameObject)
        {
            parentGameObject.GetMeshSize(meshSizeIncludeLayers, meshSizeIgnoreNames, out Vector3 buildingSize,
                out Vector3 buildingCenter);

            GameObject volumeGameObject = parentGameObject.FindChildGameObject(interiorVolumeGameObjectName);
            if (!volumeGameObject)
            {
                volumeGameObject = new GameObject(interiorVolumeGameObjectName)
                {
                    transform =
                    {
                        parent = parentGameObject.transform,
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