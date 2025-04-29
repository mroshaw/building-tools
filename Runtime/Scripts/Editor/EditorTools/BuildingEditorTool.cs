using DaftAppleGames.Buildings;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEngine;

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// Sits on top of EditorTool to provide some building specific validation methods
    /// </summary>
    internal abstract class BuildingEditorTool : EditorTool
    {
        protected bool HasBuildingComponent(out string failedReason)
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

        /// <summary>
        /// Many tools need at least the `Exterior` mesh renderers to be configured. This checks to see if that's the case
        /// </summary>
        protected bool HasMeshExteriorLayerConfigured(out string failedReason)
        {
            // Tool must check for required components first
            if (SelectedGameObject && SelectedGameObject.TryGetComponent(out Building building) && building.exteriors != null && building.exteriors.Length != 0)
            {
                // Check to see if we have any "unconfigured" exterior meshes, by looking for 'Default' layers
                bool isValid = true;
                foreach (MeshRenderer meshRenderer in SelectedGameObject.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (meshRenderer.gameObject.layer == LayerMask.NameToLayer("Default"))
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                {
                    failedReason = string.Empty;
                    return true;
                }

                failedReason = "You must configure run the 'Apply Mesh Presets' tool first. Exterior MeshRenderers found still in the 'Default' layer!";
                return false;
            }

            failedReason = "You must configure the Exterior meshes on this Game Object, via the Building component, and run the 'Apply Mesh Presets' tool!";
            return false;
        }

        /// <summary>
        /// Checks if Props have been allocated to the Building arrays
        /// </summary>
        protected bool HasPropsConfigured(out string failedReason)
        {
            if (SelectedGameObject && SelectedGameObject.TryGetComponent(out Building building) &&
                building.interiorProps != null && building.exteriors.Length != 0 &&
                building.exteriorProps != null && building.exteriorProps.Length != 0)
            {
                failedReason = string.Empty;
                return true;
            }

            failedReason = "No 'Props' have been configured on the 'Building' component for the selected GameObject!";
            return false;
        }

        protected bool HasExteriorConfigured(out string failedReason)
        {
            if (SelectedGameObject && SelectedGameObject.TryGetComponent(out Building building) &&
                building.exteriors != null && building.exteriors.Length != 0)
            {
                failedReason = string.Empty;
                return true;
            }

            failedReason = "You must set the 'Exterior' Mesh properties on the Building Component, and 'Interior' and 'Prop' properties if appropriate!";
            return false;
        }
    }
}