using DaftAppleGames.Darskerry.Core.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.BuildingTools.Editor
{
    public class BuildingEditorWindow : BaseEditorWindow
    {
        /// <summary>
        /// Custom editor overrides
        /// </summary>
        protected override string ToolTitle => "Building Editor";

        protected override string IntroText => "Welcome to Building Tools by Daft Apple Games!";

        protected override string WelcomeLogText =>
            "Welcome to Building Tools! Select some building preset settings, then select a Game Object in the hierarchy or a prefab asset, then click the buttons to configure your building.";

        [SerializeField] private GameObject selectedGameObject;
        [SerializeField] private BuildingEditorSettings buildingEditorSettings;

        private Button _addBuildingComponentButton;
        private Button _configureLayersButton;
        private Button _configureCollidersButton;
        private Button _configureVolumesButton;
        private Button _optimiseMeshesButton;
        private Button _configureLightingButton;
        private Button _configureDoorsButton;
        private Button _saveCopySettingsButton;

        [MenuItem("Daft Apple Games/Building Tools/Building Editor")]
        public static void ShowWindow()
        {
            BuildingEditorWindow packageInitWindow = GetWindow<BuildingEditorWindow>();
            packageInitWindow.titleContent = new GUIContent("Building Editor");
        }

        #region UI creation

        /// <summary>
        /// Override the base CreateGUI to add in the specific editor content
        /// </summary>
        public override void CreateGUI()
        {
            base.CreateGUI();

            // Register buttons
            _addBuildingComponentButton = rootVisualElement.Q<Button>("AddBuildingComponentButton");
            if (_addBuildingComponentButton != null)
            {
                _addBuildingComponentButton.clicked -= AddBuildingComponent;
                _addBuildingComponentButton.clicked += AddBuildingComponent;
            }

            _configureLayersButton = rootVisualElement.Q<Button>("ConfigureLayersButton");
            if (_configureLayersButton != null)
            {
                _configureLayersButton.clicked -= ConfigureLayers;
                _configureLayersButton.clicked += ConfigureLayers;
            }

            _configureCollidersButton = rootVisualElement.Q<Button>("ConfigureCollidersButton");
            if (_configureCollidersButton != null)
            {
                _configureCollidersButton.clicked -= ConfigureColliders;
                _configureCollidersButton.clicked += ConfigureColliders;
            }

            _configureVolumesButton = rootVisualElement.Q<Button>("ConfigureVolumesButton");
            if (_configureVolumesButton != null)
            {
                _configureVolumesButton.clicked -= ConfigureVolumes;
                _configureVolumesButton.clicked += ConfigureVolumes;
            }

            _optimiseMeshesButton = rootVisualElement.Q<Button>("OptimiseMeshesButton");
            if (_optimiseMeshesButton != null)
            {
                _optimiseMeshesButton.clicked -= OptimiseMeshes;
                _optimiseMeshesButton.clicked += OptimiseMeshes;
            }

            _configureLightingButton = rootVisualElement.Q<Button>("ConfigureLightingButton");
            if (_configureLightingButton != null)
            {
                _configureLightingButton.clicked -= ConfigureLighting;
                _configureLightingButton.clicked += ConfigureLighting;
            }

            _configureDoorsButton = rootVisualElement.Q<Button>("ConfigureDoorsButton");
            if (_configureDoorsButton != null)
            {
                _configureDoorsButton.clicked -= ConfigureDoors;
                _configureDoorsButton.clicked += ConfigureDoors;
            }


            _saveCopySettingsButton = rootVisualElement.Q<Button>("SaveLocalSettingsCopyButton");
            if (_saveCopySettingsButton != null)
            {
                _saveCopySettingsButton.clicked -= SaveCopySettings;
                _saveCopySettingsButton.clicked += SaveCopySettings;
            }

            selectedGameObject = Selection.activeGameObject;
            SetButtonToolTips();
            SetButtonStates();
        }

        /// <summary>
        /// Sets appropriate tooltips for buttons that aren't supported in the current
        /// pipeline or configuration
        /// </summary>
        private void SetButtonToolTips()
        {
#if !DAG_HDRP
            _configureLightingButton.tooltip = "Currently only supported in HDRP! URP and BIRP support coming soon!";
            _configureVolumesButton.tooltip = "Currently only supported in HDRP! URP and BIRP support coming soon!";
#endif
        }

        #endregion

        /// <summary>
        /// Update the GUI when selection changes
        /// </summary>
        private void OnSelectionChange()
        {
            selectedGameObject = Selection.activeGameObject;
            SetButtonStates();
        }

        private void SaveCopySettings()
        {
            if (buildingEditorSettings)
            {
                buildingEditorSettings = buildingEditorSettings.SaveALocalCopy();
            }
        }

        /// <summary>
        /// Check we've got everything before running the functions
        /// </summary>
        private bool Validate(bool checkForBuildingComponent = false)
        {
            bool isValid = true;

            if (selectedGameObject == null)
            {
                log.Log(LogLevel.Error, "Please select a GameObject!");
                isValid = false;
            }

            if (buildingEditorSettings == null)
            {
                log.Log(LogLevel.Error, "Please select some Building Editor Settings!");
                isValid = false;
            }

            if (checkForBuildingComponent && selectedGameObject && !selectedGameObject.HasComponent<Building>())
            {
                log.Log(LogLevel.Error, "The selected Game Object must contain a Building for this function to work!");
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Checks to see if it's possible to configure the layers as we want them
        /// </summary>
        private bool ValidateLayerSetup()
        {
            // If not a prefab or prefab instance, then we're good
            if (!PrefabUtility.IsPartOfAnyPrefab(selectedGameObject))
            {
                return true;
            }

            if (BuildingTools.ArePropsInMainBuildingStructure(selectedGameObject))
            {
                log.Log(LogLevel.Error,
                    "The selected GameObject is a prefab or prefab instance, and it's props GameObjects are children of the main building structure. Please amend the prefab and re-parent the props outside of the building structure.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handle the Building Component button click
        /// </summary>
        private void AddBuildingComponent()
        {
            if (!Validate())
            {
                return;
            }

            log.Log(LogLevel.Info, "Adding building component...", true);
            BuildingTools.AddBuildingComponent(selectedGameObject, log);
            log.Log(LogLevel.Info, "Done!", true);
            SetButtonStates();
        }

        /// <summary>
        /// Handle the Layers button click
        /// </summary>
        private void ConfigureLayers()
        {
            if (!Validate() || !ValidateLayerSetup())
            {
                return;
            }

            log.Log(LogLevel.Info, "Configuring layers...", true);
            BuildingTools.ConfigureLayers(selectedGameObject, buildingEditorSettings, log);
            log.Log(LogLevel.Info, "Done!", true);
        }

        /// <summary>
        /// Handle the Colliders button click
        /// </summary>
        private void ConfigureColliders()
        {
            if (!Validate(true))
            {
                return;
            }

            log.Log(LogLevel.Info, "Configuring colliders...", true);
            BuildingTools.ConfigureColliders(selectedGameObject, buildingEditorSettings, log);
            log.Log(LogLevel.Info, "Done!", true);
        }

        /// <summary>
        /// Handle the Volumes button click
        /// </summary>
        private void ConfigureVolumes()
        {
            if (!Validate(true))
            {
                return;
            }

            log.Log(LogLevel.Info, "Configuring colliders...", true);
            BuildingTools.ConfigureVolumes(selectedGameObject, buildingEditorSettings, log);
            log.Log(LogLevel.Info, "Done!", true);
        }

        /// <summary>
        /// Handle the Mesh button
        /// </summary>
        private void OptimiseMeshes()
        {
            BuildingTools.OptimiseMeshes(selectedGameObject, buildingEditorSettings, log);
        }

        /// <summary>
        /// Handle the lights button
        /// </summary>
        private void ConfigureLighting()
        {
            BuildingTools.ConfigureLighting(selectedGameObject, buildingEditorSettings, log);
        }

        /// <summary>
        /// Handle the doors button
        /// </summary>
        private void ConfigureDoors()
        {
            if (!Validate(true))
            {
                return;
            }

            log.Log(LogLevel.Info, "Configuring doors...", true);
            BuildingTools.ConfigureDoors(selectedGameObject, buildingEditorSettings, log);
            log.Log(LogLevel.Info, "Done!", true);
        }

        /// <summary>
        /// Enables and disables buttons, depending on whether we are ready to process
        /// </summary>
        private void SetButtonStates()
        {
            // Disable all button states if no GameObject is selected and no settings
            if (selectedGameObject == null || buildingEditorSettings == null)
            {
                _addBuildingComponentButton.SetEnabled(false);
                _configureLayersButton.SetEnabled(false);
                _configureCollidersButton.SetEnabled(false);
                _configureVolumesButton.SetEnabled(false);
                _configureDoorsButton.SetEnabled(false);
                _optimiseMeshesButton.SetEnabled(false);
                _configureVolumesButton.SetEnabled(false);
                _configureLightingButton.SetEnabled(false);
                return;
            }

            // Enable the button only if the component has not already been added
            bool hasBuildingComponent = selectedGameObject.HasComponent<Building>();
            _addBuildingComponentButton.SetEnabled(!hasBuildingComponent);

            // Enable the remaining buttons only once the building component has been added.
            _configureLayersButton.SetEnabled(hasBuildingComponent);
            _configureCollidersButton.SetEnabled(hasBuildingComponent);
#if !DAG_HDRP
            _configureVolumesButton.SetEnabled(false);
            _configureLightingButton.SetEnabled(false);
#else
            _configureVolumesButton.SetEnabled(hasBuildingComponent);
            _configureLightingButton.SetEnabled(hasBuildingComponent);
#endif
            _configureDoorsButton.SetEnabled(hasBuildingComponent);
            _optimiseMeshesButton.SetEnabled(hasBuildingComponent);
        }
    }
}