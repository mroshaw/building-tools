using DaftAppleGames.Darskerry.Core.Buildings;
using DaftAppleGames.Editor;
using UnityEngine;

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// Static methods for working with Buildings
    /// </summary>
    internal static class BuildingTools
    {
        #region Static properties

        static BuildingTools()
        {

        }
        #endregion

        #region Tool prarameter structs
        /// <summary>
        /// Struct to consolidate parameters for use with the static methods
        /// </summary>
        internal struct ConfigureBuildingParameters
        {
        }
        #endregion

        #region Base methods
        internal static void AddBuildingComponent(GameObject parentGameObject, EditorLog log)
        {
            Building building = parentGameObject.GetComponent<Building>();
            if (building)
            {
                log.Log(LogLevel.Warning, $"{parentGameObject.name} already has a Building component.");
                return;
            }
            building = parentGameObject.AddComponent<Building>();
            log.Log(LogLevel.Info, $"Added Building component to {parentGameObject.name}.");
        }
        #endregion
    }
}