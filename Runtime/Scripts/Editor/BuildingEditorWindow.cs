using DaftAppleGames.Editor;
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
        protected override string WindowTitle => "Building Editor";
        protected override string ToolTitle => "Building Editor";
        protected override string IntroText => "Welcome to Building Tools by Daft Apple Games!";
        protected override string WelcomeLogText =>
            "Welcome to Building Tools! Select some building preset settings, then select a Game Object in the hierarchy or a prefab asset, then click the buttons to configure your building.";

        [SerializeField] private GameObject selectedGameObject;

        [MenuItem("Daft Apple Games/Building Tools/Building Editor")]
        public static void ShowWindow()
        {
            BuildingEditorWindow packageInitWindow = GetWindow<BuildingEditorWindow>();
            packageInitWindow.titleContent = new GUIContent("Building Editor");
        }

        /// <summary>
        /// Override the base CreateGUI to add in the specific editor content
        /// </summary>
        public override void CreateGUI()
        {
            base.CreateGUI();

            // Register buttons
            Button addBuildingComponentButton = rootVisualElement.Q<Button>("AddBuildingComponentButton");
            if (addBuildingComponentButton != null)
            {
                addBuildingComponentButton.clicked -= AddBuildingComponent;
                addBuildingComponentButton.clicked += AddBuildingComponent;
            }
        }

        /// <summary>
        /// Update the GUI when selection changes
        /// </summary>
        private void OnSelectionChange()
        {
            selectedGameObject = Selection.activeGameObject;
        }

        /// <summary>
        /// Handle the button click
        /// </summary>
        private void AddBuildingComponent()
        {
            BuildingTools.AddBuildingComponent(selectedGameObject, log);
        }
    }
}