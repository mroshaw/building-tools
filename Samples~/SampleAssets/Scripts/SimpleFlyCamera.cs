using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.Samples.BuildingTools
{
    public class SimpleFlyCamera : MonoBehaviour
    {
        [BoxGroup("Settings")] [SerializeField] private float acceleration = 50;
        [BoxGroup("Settings")] [SerializeField] private float accSprintMultiplier = 4;
        [BoxGroup("Settings")] [SerializeField] private float lookSensitivity = 1;
        [BoxGroup("Settings")] [SerializeField] private float dampingCoefficient = 5;
        [BoxGroup("Settings")] [SerializeField] private bool focusOnEnable = true;

        private Vector3 _velocity;

        private static bool Focused
        {
            get => Cursor.lockState == CursorLockMode.Locked;
            set
            {
                Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = value == false;
            }
        }

        private void OnEnable()
        {
            if (focusOnEnable) Focused = true;
        }

        private void OnDisable()
        {
            Focused = false;
        }

        private void Update()
        {
            if (Focused)
            {
                UpdateInput();
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Focused = true;
            }

            _velocity = Vector3.Lerp(_velocity, Vector3.zero, dampingCoefficient * Time.deltaTime);
            transform.position += _velocity * Time.deltaTime;
        }

        private void UpdateInput()
        {
            // Position
            _velocity += GetAccelerationVector() * Time.deltaTime;

            // Rotation
            Vector2 mouseDelta = lookSensitivity * new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
            Quaternion rotation = transform.rotation;
            Quaternion horiz = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);
            Quaternion vert = Quaternion.AngleAxis(mouseDelta.y, Vector3.right);
            transform.rotation = horiz * rotation * vert;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Focused = false;
            }
        }

        private Vector3 GetAccelerationVector()
        {
            Vector3 moveInput = default;

            void AddMovement(KeyCode key, Vector3 dir)
            {
                if (Input.GetKey(key))
                {
                    moveInput += dir;
                }
            }

            AddMovement(KeyCode.W, Vector3.forward);
            AddMovement(KeyCode.S, Vector3.back);
            AddMovement(KeyCode.D, Vector3.right);
            AddMovement(KeyCode.A, Vector3.left);
            AddMovement(KeyCode.Space, Vector3.up);
            AddMovement(KeyCode.LeftControl, Vector3.down);
            Vector3 direction = transform.TransformVector(moveInput.normalized);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                return direction * (acceleration * accSprintMultiplier);
            }
            return direction * acceleration;
        }
    }
}