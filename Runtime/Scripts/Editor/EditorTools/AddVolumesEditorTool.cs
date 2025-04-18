using System;
using DaftAppleGames.Editor;
using DaftAppleGames.Editor.Extensions;
using DaftAppleGames.Extensions;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace DaftAppleGames.BuildingTools.Editor
{
    [Serializable]
    internal struct BuildingVolumeSettings
    {
        [SerializeField] internal string[] meshSizeIgnoreNames;
        [SerializeField] internal LayerMask meshSizeIncludeLayers;
        [SerializeField] internal string interiorVolumeGameObjectName;
#if DAG_HDRP || DAG_UURP
        [SerializeField] internal VolumeProfile interiorVolumeProfile;
#endif
        [SerializeField] internal AudioMixerSnapshot indoorSnapshot;
        [SerializeField] internal AudioMixerSnapshot outdoorSnapshot;
        [SerializeField] internal string[] volumeTriggerTags;
        [SerializeField] internal LayerMask volumeTriggerLayerMask;
    }

    [CreateAssetMenu(fileName = "ConfigureVolumesEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Volumes Tool")]
    internal class AddVolumesEditorTool : BuildingEditorTool
    {
        private bool _configureLightingVolumeOption;
        private bool _configureAudioVolumeOption;

        protected override string GetToolName()
        {
            return "Add Volumes";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, out string cannotRunReason)
        {
            if (RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation())
            {
                cannotRunReason = string.Empty;
                return true;
            }

            cannotRunReason = $"{selectEditorSettingsAndGameObjectError}\n{buildingComponentRequiredError}";
            return false;
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, string undoGroupName)
        {
            if (editorSettings is BuildingWizardEditorSettings buildingEditorSettings)
            {
#if DAG_HDRP || DAG_UURP
                if (_configureLightingVolumeOption)
                {
                    log.Log(LogLevel.Info, "Configuring Lighting Volume...");
                    AddInteriorLightingVolume(selectedGameObject, buildingEditorSettings.buildingVolumeSettings);
                    log.Log(LogLevel.Info, "Configuring Lighting Volume... DONE!");
                }
#endif
                if (_configureAudioVolumeOption)
                {
                    log.Log(LogLevel.Info, "Configuring Audio Volume...");
                    AddInteriorAudioVolume(selectedGameObject, buildingEditorSettings.buildingVolumeSettings);
                    log.Log(LogLevel.Info, "Configuring Audio Volume... DONE!");
                }
            }
        }

        /// <summary>
        /// Add bindings for custom tool options
        /// </summary>
        protected override void AddCustomBindings()
        {
            _configureLightingVolumeOption = BindToToggleOption("ConfigureLightingVolumeToggle", SetConfigureLightingVolumeOption);
            _configureAudioVolumeOption = BindToToggleOption("ConfigureAudioVolumeToggle", SetConfigureAudioVolumeOption);
        }

        private void SetConfigureLightingVolumeOption(ChangeEvent<bool> changeEvent)
        {
            _configureLightingVolumeOption = changeEvent.newValue;
        }

        private void SetConfigureAudioVolumeOption(ChangeEvent<bool> changeEvent)
        {
            _configureAudioVolumeOption = changeEvent.newValue;
        }

        #region Static tool methods

#if DAG_HDRP || DAG_UURP
        private static void AddInteriorLightingVolume(GameObject parentGameObject, BuildingVolumeSettings buildingVolumeSettings)
        {
            log.Log(LogLevel.Debug, $"Adding Interior Lighting Volume to {parentGameObject.name}...");
            GameObject volumeGameObject = ConfigureVolumeGameObject(parentGameObject, buildingVolumeSettings);
            Volume interiorVolume = volumeGameObject.EnsureComponent<Volume>();
            interiorVolume.sharedProfile = buildingVolumeSettings.interiorVolumeProfile;
            interiorVolume.isGlobal = false;
        }
#endif
        private static void AddInteriorAudioVolume(GameObject parentGameObject, BuildingVolumeSettings buildingVolumeSettings)
        {
            log.Log(LogLevel.Debug, $"Adding Interior Audio Volume to {parentGameObject.name}...");
            GameObject volumeGameObject = ConfigureVolumeGameObject(parentGameObject, buildingVolumeSettings);
            InteriorAudioFilter audioFilter = volumeGameObject.EnsureComponent<InteriorAudioFilter>();
            audioFilter.ConfigureInEditor(buildingVolumeSettings.volumeTriggerLayerMask, buildingVolumeSettings.volumeTriggerTags,
                buildingVolumeSettings.indoorSnapshot, buildingVolumeSettings.outdoorSnapshot);
            log.Log(LogLevel.Debug, $"Adding Interior Audio Volume to {parentGameObject.name}... DONE!");
        }

        private static GameObject ConfigureVolumeGameObject(GameObject parentGameObject, BuildingVolumeSettings buildingVolumeSettings)
        {
            parentGameObject.GetMeshSize(buildingVolumeSettings.meshSizeIncludeLayers, buildingVolumeSettings.meshSizeIgnoreNames, out Vector3 buildingSize,
                out Vector3 buildingCenter);

            GameObject volumeGameObject = parentGameObject.FindChildGameObject(buildingVolumeSettings.interiorVolumeGameObjectName);
            if (!volumeGameObject)
            {
                volumeGameObject = new GameObject(buildingVolumeSettings.interiorVolumeGameObjectName)
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

        #endregion
    }
}