using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// Sits on top of EditorTool to provide some building specific validation methods
    /// </summary>
    public abstract class BuildingEditorTool : EditorTool
    {
        // Standard return strings for validation functions
        protected static string buildingComponentRequiredError = "The selected game object must contain a Building component to run this tool!";
        protected static string buildingMeshNotSetError = "You must configure the Mesh properties on the Building Component before running this tool!";

        protected bool RequiredBuildingValidation()
        {
            if (ParentToolsList.SelectedGameObject && ParentToolsList.SelectedGameObject.HasComponent<Building>())
            {
                return true;
            }

            return false;
        }

        protected bool RequiredBuildingMeshValidation()
        {
            if (!ParentToolsList.SelectedGameObject.TryGetComponent(out Building building))
            {
                return true;
            }

            if (building.interiors != null && building.interiors.Length != 0 &&
                building.exteriors != null && building.exteriors.Length != 0)
            {
                return true;
            }

            return false;
        }
    }
}