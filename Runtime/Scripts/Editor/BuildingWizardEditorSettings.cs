using UnityEditor;
using System;
using DaftAppleGames.Buildings;
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
    /// This struct gives us an easy way of passing Lighting configuration around
    /// our various classes and methods
    /// </summary>
    [Serializable]
    public struct LightingSettings
    {
        public BuildingLightType buildingLightType;
        public string[] meshNames;
        public string[] flameNames;
        public float range;
        public float intensity;
        public float radius;
        public LightLayerMode layerMode;
        public bool useLensFlare;
#if DAG_HDRP || DAG_UURP
        public float lensFlareIntensity;
        public LensFlareDataSRP lensFlareData;
#endif
        public Color filterColor;
        public float temperature;
        public LightmapBakeType lightmapBakeType;
        public float shadowRefreshRate;
    }

    /// <summary>
    /// This Scriptable Object allows different configurations for the tools for different assets and use cases
    /// At some point, I may consider splitting this into Tool specific configuration. For now, it's nice kept in one place.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingEditorSettings", menuName = "Daft Apple Games/Building Tools/BuildingEditorSettings")]
    public class BuildingWizardEditorSettings : ButtonWizardEditorSettings
    {
        [BoxGroup("Meshes")] [SerializeField] internal string buildingExteriorLayer = "BuildingExterior";
        [BoxGroup("Meshes")] [SerializeField] internal LightLayerMode buildingExteriorLightLayerMode = LightLayerMode.Exterior;
        [BoxGroup("Meshes")] [SerializeField] internal string buildingInteriorLayer = "BuildingInterior";
        [BoxGroup("Meshes")] [SerializeField] internal LightLayerMode buildingInteriorLightLayerMode = LightLayerMode.Interior;
        [BoxGroup("Meshes")] [SerializeField] internal string exteriorPropsLayer = "ExteriorProps";
        [BoxGroup("Meshes")] [SerializeField] internal LightLayerMode exteriorPropsLightLayerMode = LightLayerMode.Exterior;
        [BoxGroup("Meshes")] [SerializeField] internal string interiorPropsLayer = "InteriorProps";
        [BoxGroup("Meshes")] [SerializeField] internal LightLayerMode interiorPropsLightLayerMode = LightLayerMode.Interior;
        [BoxGroup("Meshes")] [SerializeField] internal StaticEditorFlags staticMeshFlags;
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

        [BoxGroup("Lighting")] [SerializeField] internal LightingSettings indoorCandleSettings;
        [BoxGroup("Lighting")] [SerializeField] internal LightingSettings indoorFireSettings;
        [BoxGroup("Lighting")] [SerializeField] internal LightingSettings outdoorLightSettings;

        [BoxGroup("Volumes")] [SerializeField] internal string[] meshSizeIgnoreNames;
        [BoxGroup("Volumes")] [SerializeField] internal LayerMask meshSizeIncludeLayers;
#if DAG_HDRP || DAG_UURP

        [BoxGroup("Volumes")] [SerializeField] internal string interiorVolumeGameObjectName;
        [BoxGroup("Volumes")] [SerializeField] internal VolumeProfile interiorVolumeProfile;

#endif
        [BoxGroup("Volumes")] [SerializeField] internal AudioMixerSnapshot indoorSnapshot;
        [BoxGroup("Volumes")] [SerializeField] internal AudioMixerSnapshot outdoorSnapshot;
        [BoxGroup("Volumes")] [SerializeField] internal string[] volumeTriggerTags;
        [BoxGroup("Volumes")] [SerializeField] internal LayerMask volumeTriggerLayerMask;

        [BoxGroup("CombineMeshes")] [SerializeField] internal string meshAssetOutputPath;
    }
}