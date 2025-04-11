using DaftAppleGames.Editor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// This Scriptable Object allows different configurations for the tools for different assets and use cases
    /// At some point, I may consider splitting this into Tool specific configuration. For now, it's nice kept in one place.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingWizardEditorSettings", menuName = "Daft Apple Games/Building Tools/Building Wizard Editor Settings")]
    public class BuildingWizardEditorSettings : ButtonWizardEditorSettings
    {
        [BoxGroup("Initialization")] [SerializeField] internal BuildingInitSettings buildingInitSetting;
        [BoxGroup("Meshes")] [SerializeField] internal BuildingMeshSettings buildingMeshSettings;
        [BoxGroup("Props")] [SerializeField] internal BuildingPropsSettings buildingPropsSettings;
        [BoxGroup("Doors")] [SerializeField] internal BuildingDoorSettings buildingDoorSettings;
        [BoxGroup("Lights")] [SerializeField] internal BuildingLightSettings buildingLightSettings;
        [BoxGroup("Volumes")] [SerializeField] internal BuildingVolumeSettings buildingVolumeSettings;
        [BoxGroup("Combine Meshes")] [SerializeField] internal string meshAssetOutputPath;
    }
}