#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;

namespace DaftAppleGames.Darskerry.Core.Buildings
{
    public class Building : MonoBehaviour
    {
        [BoxGroup("Meshes")] [SerializeField] public GameObject[] interiors;

        [BoxGroup("Meshes")] [SerializeField] public GameObject[] exteriors;

        [BoxGroup("Meshes")] [SerializeField] public GameObject[] interiorProps;

        [BoxGroup("Meshes")] [SerializeField] public GameObject[] exteriorProps;
    }
}