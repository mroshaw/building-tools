using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "ConfigureVolumesEditorTool", menuName = "Daft Apple Games/Building Tools/Configure Volumes Tool")]
    internal class ConfigureVolumesEditorTool : BuildingEditorTool
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

        protected override bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings)
        {
            return RequireSettingsAndGameObjectValidation() && RequiredBuildingValidation();
        }

        protected override void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, string undoGroupName)
        {
            if (editorSettings is BuildingWizardEditorSettings buildingEditorSettings)
            {
#if DAG_HDRP || DAG_UURP
                if (_configureLightingVolumeOption)
                {
                    log.Log(LogLevel.Info, "Configuring Lighting Volume...");
                    AddInteriorLightingVolume(selectedGameObject, buildingEditorSettings);
                    log.Log(LogLevel.Info, "Configuring Lighting Volume... DONE!");
                }
#endif
                if (_configureAudioVolumeOption)
                {
                    log.Log(LogLevel.Info, "Configuring Audio Volume...");
                    AddInteriorAudioVolume(selectedGameObject, buildingEditorSettings);
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
        private static void AddInteriorLightingVolume(GameObject parentGameObject, BuildingWizardEditorSettings buildingWizardSettings)
        {
            log.Log(LogLevel.Debug, $"Adding Interior Lighting Volume to {parentGameObject.name}...");
            GameObject volumeGameObject = ConfigureVolumeGameObject(parentGameObject, buildingWizardSettings);
            Volume interiorVolume = volumeGameObject.EnsureComponent<Volume>();
            interiorVolume.sharedProfile = buildingWizardSettings.interiorVolumeProfile;
            interiorVolume.isGlobal = false;
        }
#endif
        private static void AddInteriorAudioVolume(GameObject parentGameObject, BuildingWizardEditorSettings buildingWizardSettings)
        {
            log.Log(LogLevel.Debug, $"Adding Interior Audio Volume to {parentGameObject.name}...");
            GameObject volumeGameObject = ConfigureVolumeGameObject(parentGameObject, buildingWizardSettings);
            InteriorAudioFilter audioFilter = volumeGameObject.EnsureComponent<InteriorAudioFilter>();
            audioFilter.ConfigureInEditor(buildingWizardSettings.volumeTriggerLayerMask, buildingWizardSettings.volumeTriggerTags,
                buildingWizardSettings.indoorSnapshot, buildingWizardSettings.outdoorSnapshot);
            log.Log(LogLevel.Debug, $"Adding Interior Audio Volume to {parentGameObject.name}... DONE!");
        }

        private static GameObject ConfigureVolumeGameObject(GameObject parentGameObject, BuildingWizardEditorSettings buildingWizardSettings)
        {
            MeshTools.GetMeshSize(parentGameObject, buildingWizardSettings.meshSizeIncludeLayers, buildingWizardSettings.meshSizeIgnoreNames, out Vector3 buildingSize,
                out Vector3 buildingCenter);

            GameObject volumeGameObject = parentGameObject.FindChildGameObject(buildingWizardSettings.interiorVolumeGameObjectName);
            if (!volumeGameObject)
            {
                volumeGameObject = new GameObject(buildingWizardSettings.interiorVolumeGameObjectName)
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