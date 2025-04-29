#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;

namespace DaftAppleGames.Buildings
{
    /// <summary>
    /// Simple Windmill blade spinner Monobehaviour
    /// </summary>
    public class Windmill : MonoBehaviour
    {
        [BoxGroup("Settings")] [SerializeField] private GameObject windMillBlades;
        [BoxGroup("Settings")] [SerializeField] private float rotateSpeed = 5.0f;

        private void Start()
        {
            if (!windMillBlades)
            {
                windMillBlades = gameObject;
            }
        }

        /// <summary>
        /// Rotate the windmill
        /// </summary>
        private void Update()
        {
            windMillBlades.transform.Rotate(rotateSpeed * Time.deltaTime, 0.0f, 0.0f, Space.Self);
        }
    }
}