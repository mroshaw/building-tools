using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEngine.UIElements;

namespace DaftAppleGames.BuildingTools.Editor
{
    public abstract class BuildingEditorTool : EditorTool
    {
        protected bool RequiredBuildingValidation()
        {
            if (ParentToolsList.SelectedGameObject && ParentToolsList.SelectedGameObject.HasComponent<Building>())
            {
                return true;
            }

            ParentToolsList.EditorLog.Log(LogLevel.Error, "The selected game object must contain a Building component to run this tool!");
            return false;
        }

        protected bool RequiredBuildingMeshValidation()
        {
            if (ParentToolsList.SelectedGameObject && ParentToolsList.SelectedGameObject.HasComponent<Building>())
            {
                return true;
            }

            ParentToolsList.EditorLog.Log(LogLevel.Error, "The selected game object must contain a Building component to run this tool!");
            return false;
        }

        /// <summary>
        /// Binds the given toggle control to an event callback. This allows tools to present their own toggle options in the UI
        /// and bind the control to a local bool
        /// </summary>
        /// <param name="toggleName"></param>
        /// <param name="toggleChangeEvent"></param>
        /// <returns></returns>
        protected bool BindToToggleOption(string toggleName, EventCallback<ChangeEvent<bool>> toggleChangeEvent)
        {
            Toggle toggle = RootVisualElement.Q<Toggle>(toggleName);
            if (toggle == null)
            {
                ParentToolsList.EditorLog.Log(LogLevel.Error, "Couldn't find toggle. Failed to bind option to toggle: " + toggleName);
                return false;
            }

            toggle.RegisterValueChangedCallback(toggleChangeEvent);
            return toggle.value;
        }
    }
}