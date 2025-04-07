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
        [FoldoutGroup("Meshes")] [SerializeField] public GameObject[] interiors;

        [FoldoutGroup("Meshes")] [SerializeField] public GameObject[] exteriors;

        [FoldoutGroup("Meshes")] [SerializeField] public GameObject[] interiorProps;

        [FoldoutGroup("Meshes")] [SerializeField] public GameObject[] exteriorProps;

        public bool RequiredPropertiesSet()
        {
            return interiors is { Length: > 0 } && exteriors is { Length: > 0 };
        }
    }
}