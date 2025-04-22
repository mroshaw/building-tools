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
        protected bool RequiredBuildingValidation(out string failedReason)
        {
            if (!SelectedGameObject)
            {
                failedReason = string.Empty;
                return true;
            }

            if (SelectedGameObject.HasComponent<Building>())
            {
                failedReason = string.Empty;
                return true;
            }

            failedReason = "The selected game object must contain a Building component. Please run the 'Initialize Building' tool first!";
            return false;
        }

        protected bool RequiredBuildingMeshValidation(out string failedReason)
        {
            if (SelectedGameObject && SelectedGameObject.TryGetComponent(out Building building) && building.interiors != null && building.interiors.Length != 0 &&
                building.exteriors != null && building.exteriors.Length != 0)
            {
                failedReason = string.Empty;
                return true;
            }

            failedReason = "You must configure the Mesh properties on the Building Component!";
            return false;
        }
    }
}