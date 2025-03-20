/*
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
#else
using DaftAppleGames.Attributes;
#endif

using System.Collections.Generic;
using System.IO;
using DaftAppleGames.Darskerry.Core.Buildings;
using DaftAppleGames.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DaftAppleGames.Editor.Mesh
{
    public class MeshCombinerEditorWindow : OdinEditorWindow
    {
        [MenuItem("Daft Apple Games/Meshes/Mesh Combine Tool")]
        public static void ShowWindow()
        {
            GetWindow(typeof(MeshCombinerEditorWindow));
        }

        [BoxGroup("Selected Objects")]
        [SerializeField]
        private GameObject[] selectedObjects;

        [BoxGroup("Tool Settings")] [SerializeField] private bool showDebug = true;

        private void WriteLog(string logMessage)
        {
            if (showDebug)
            {
                Debug.Log(logMessage);
            }
        }

        /// <summary>
        /// Refresh the list of GameObjects selected
        /// </summary>
        private void OnSelectionChange()
        {
            selectedObjects = Selection.gameObjects;
        }


        [BoxGroup("Mesh Settings")] [SerializeField] private string outputPath = "Assets/_Project/Meshes/";
        [BoxGroup("Mesh Settings")] [SerializeField] private string namePrefix = "PlayerHouse";
        [BoxGroup("Mesh Settings")] [SerializeField] private bool is32bit = true;
        [BoxGroup("Mesh Settings")] [SerializeField] private bool generateSecondaryUVs = false;

        [BoxGroup("Lighting")] [SerializeField] private bool interiorLayer = false;
        [BoxGroup("Lighting")] [SerializeField] private bool exteriorLayer = true;

        [BoxGroup("Lighting")] [SerializeField] private ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
        [BoxGroup("Lighting")] [SerializeField] private bool staticShadowCaster = true;
        [BoxGroup("Lighting")] [SerializeField] private bool contributeGI = true;
        [BoxGroup("Lighting")] [SerializeField] private ReceiveGI receiveGI = ReceiveGI.LightProbes;

        [Button("Combine Meshes")]
        public void CombineMeshes()
        {
            foreach (GameObject currGameObject in selectedObjects)
            {
                CombineGameObjectMeshes(currGameObject, outputPath, namePrefix);
            }
        }


    }
}
*/