using UnityEditor;
using DaftAppleGames.Editor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
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
        [BoxGroup("Building")] [SerializeField] internal float adjustAnchorHeight;

        [BoxGroup("Meshes")] [SerializeField] internal MeshTools.MeshSettings interiorMeshSettings;
        [BoxGroup("Meshes")] [SerializeField] internal MeshTools.MeshSettings exteriorMeshSettings;
        [BoxGroup("Meshes")] [SerializeField] internal MeshTools.MeshSettings interiorPropMeshSettings;
        [BoxGroup("Meshes")] [SerializeField] internal MeshTools.MeshSettings exteriorPropMeshSettings;
        [BoxGroup("Meshes")] [SerializeField] internal StaticEditorFlags moveableMeshFlags;

        [BoxGroup("Props")] [SerializeField] internal string[] boxColliderNames;
        [BoxGroup("Props")] [SerializeField] internal string[] sphereColliderNames;
        [BoxGroup("Props")] [SerializeField] internal string[] capsuleColliderNames;
        [BoxGroup("Props")] [SerializeField] internal string[] meshColliderNames;
        [BoxGroup("Props")] [SerializeField] internal bool terrainAlignPosition;
        [BoxGroup("Props")] [SerializeField] internal bool terrainAlignRotation;
        [BoxGroup("Props")] [SerializeField] internal bool terrainAlignX;
        [BoxGroup("Props")] [SerializeField] internal bool terrainAlignY;
        [BoxGroup("Props")] [SerializeField] internal bool terrainAlignZ;

        [BoxGroup("Doors")] [SerializeField] internal string[] doorNames;
        [BoxGroup("Doors")] [SerializeField] internal AudioClip[] doorOpeningClips;
        [BoxGroup("Doors")] [SerializeField] internal AudioClip[] doorOpenClips;
        [BoxGroup("Doors")] [SerializeField] internal AudioClip[] doorClosingClips;
        [BoxGroup("Doors")] [SerializeField] internal AudioClip[] doorClosedClips;
        [BoxGroup("Doors")] [SerializeField] internal AudioMixerGroup doorSfxGroup;
        [BoxGroup("Doors")] [SerializeField] internal LayerMask doorTriggerLayerMask;
        [BoxGroup("Doors")] [SerializeField] internal string[] doorTriggerTags;

        [BoxGroup("Lighting")] [SerializeField] internal LightTools.LightingSettings indoorCandleSettings;
        [BoxGroup("Lighting")] [SerializeField] internal LightTools.LightingSettings indoorFireSettings;
        [BoxGroup("Lighting")] [SerializeField] internal LightTools.LightingSettings outdoorLightSettings;

        [BoxGroup("Volumes")] [SerializeField] internal string[] meshSizeIgnoreNames;
        [BoxGroup("Volumes")] [SerializeField] internal LayerMask meshSizeIncludeLayers;
        [BoxGroup("Volumes")] [SerializeField] internal string interiorVolumeGameObjectName;
#if DAG_HDRP || DAG_UURP
        [BoxGroup("Volumes")] [SerializeField] internal VolumeProfile interiorVolumeProfile;
#endif
        [BoxGroup("Volumes")] [SerializeField] internal AudioMixerSnapshot indoorSnapshot;
        [BoxGroup("Volumes")] [SerializeField] internal AudioMixerSnapshot outdoorSnapshot;
        [BoxGroup("Volumes")] [SerializeField] internal string[] volumeTriggerTags;
        [BoxGroup("Volumes")] [SerializeField] internal LayerMask volumeTriggerLayerMask;

        [BoxGroup("Combine Meshes")] [SerializeField] internal string meshAssetOutputPath;
    }
}