using UnityEditor;
using System;
using DaftAppleGames.Darskerry.Core.Buildings;
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
    }

    [CreateAssetMenu(fileName = "BuildingEditorSettings", menuName = "Daft Apple Games/Building Tools/BuildingEditorSettings")]
    public class BuildingEditorSettings : ScriptableObject
    {
        [BoxGroup("Layers")] [SerializeField] internal string buildingExteriorLayer = "BuildingExterior";
        [BoxGroup("Layers")] [SerializeField] internal LightLayerMode buildingExteriorLightLayerMode = LightLayerMode.Exterior;
        [BoxGroup("Layers")] [SerializeField] internal string buildingInteriorLayer = "BuildingInterior";
        [BoxGroup("Layers")] [SerializeField] internal LightLayerMode buildingInteriorLightLayerMode = LightLayerMode.Interior;
        [BoxGroup("Layers")] [SerializeField] internal string exteriorPropsLayer = "ExteriorProps";
        [BoxGroup("Layers")] [SerializeField] internal LightLayerMode exteriorPropsLightLayerMode = LightLayerMode.Exterior;
        [BoxGroup("Layers")] [SerializeField] internal string interiorPropsLayer = "InteriorProps";
        [BoxGroup("Layers")] [SerializeField] internal LightLayerMode interiorPropsLightLayerMode = LightLayerMode.Interior;

        [BoxGroup("Colliders")] [SerializeField] internal string[] boxColliderNames;
        [BoxGroup("Colliders")] [SerializeField] internal string[] sphereColliderNames;
        [BoxGroup("Colliders")] [SerializeField] internal string[] capsuleColliderNames;
        [BoxGroup("Colliders")] [SerializeField] internal string[] meshColliderNames;

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
        [BoxGroup("Volumes")] [SerializeField] internal string interiorVolumeGameObjectName;
        [BoxGroup("Volumes")] [SerializeField] internal VolumeProfile interiorVolumeProfile;
        [BoxGroup("CombineMeshes")] [SerializeField] internal string meshAssetOutputPath;

        [Button("Save A Copy")]
        internal BuildingEditorSettings SaveALocalCopy()
        {
            string pathToSave = EditorUtility.SaveFilePanel(
                "Save a local copy of settings",
                "",
                "myBuildingEditorSettings.asset",
                "asset");

            if (string.IsNullOrEmpty(pathToSave))
            {
                return this;
            }

            string relativePath = "Assets" + pathToSave.Substring(Application.dataPath.Length);

            BuildingEditorSettings newEditorSettings = Instantiate(this);
            AssetDatabase.CreateAsset(newEditorSettings, relativePath);
            return newEditorSettings;
        }
    }
}