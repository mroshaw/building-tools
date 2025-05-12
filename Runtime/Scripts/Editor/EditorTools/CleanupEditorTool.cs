using System.Collections.Generic;
using DaftAppleGames.Editor;
using DaftAppleGames.Extensions;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.BuildingTools.Editor
{
    [CreateAssetMenu(fileName = "CleanupEditorTool", menuName = "Daft Apple Games/Building Tools/Cleanup Tool")]
    internal class CleanupEditorTool : BuildingEditorTool
    {
        [SerializeField] [BoxGroup("Material Fix Settings")]
        [Tooltip("Disable Mesh Renderers in the building that contain any materials with these names.")]
        internal string[] materialNames;

        protected override string GetToolName()
        {
            return "Cleanup";
        }

        protected override bool IsSupported(out string notSupportedReason)
        {
            notSupportedReason = string.Empty;
            return true;
        }

        protected override bool CanRunTool(out List<string> cannotRunReasons)
        {
            bool canRun = true;

            cannotRunReasons = new List<string>();
            if (!RequireGameObjectValidation(out string requireGameObjectReason))
            {
                cannotRunReasons.Add(requireGameObjectReason);
                return false;
            }

            if (!HasRequiredBuildingComponent(out string requiredBuildingReason))
            {
                cannotRunReasons.Add(requiredBuildingReason);
                canRun = false;
            }

            return canRun;
        }

        protected override void RunTool(string undoGroupName)
        {
            CleanupBuilding();
        }

        /// <summary>
        /// Find known problems with building Game Objects and fixes them
        /// </summary>
        private void CleanupBuilding()
        {
            DisableInvalidMaterialRenderers();
        }

        /// <summary>
        /// Finds renderers using the named materials and deactivates them
        /// </summary>
        private void DisableInvalidMaterialRenderers()
        {
            Renderer[] allRenderers = SelectedGameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in allRenderers)
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (!material || !materialNames.ItemInString(material.name))
                    {
                        continue;
                    }

                    log.AddToLog(LogLevel.Info, $"Found instance of invalid material. Disabling renderer on: {renderer.name}");
                    renderer.enabled = false;
                    break;
                }
            }
        }
    }
}