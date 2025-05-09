using System.Collections;
using System.Collections.Generic;
using DaftAppleGames.Extensions;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace DaftAppleGames.Buildings
{
    /// <summary>
    /// This is important to know when rotating the door open
    /// </summary>
    public enum DoorPivotLocation
    {
        Unknown,
        BottomLeft,
        BottomRight,
        Center
    }

    /// <summary>
    /// Depending on where the open is triggered, the door needs to move in a particular direction
    /// so as not to hit the triggering collider
    /// </summary>
    public enum DoorOpenDirection
    {
        Inwards,
        Outwards
    }

    /// <summary>
    /// The door has a "state machine" as it can only be in one of these states
    /// </summary>
    internal enum DoorState
    {
        Open,
        Opening,
        Closing,
        Closed
    }

    public class Door : MonoBehaviour
    {
        [BoxGroup("Settings")] [SerializeField] private DoorPivotLocation doorPivotLocation;
        [BoxGroup("Settings")] [SerializeField] private float openAngle = 110.0f;
        [BoxGroup("Settings")] [SerializeField] private float openingDuration = 2.0f;
        [BoxGroup("Settings")] [SerializeField] private float stayOpenDuration = 5.0f;
        [BoxGroup("Settings")] [SerializeField] private float closingDuration = 2.0f;

        [BoxGroup("Audio")] [SerializeField] private AudioClip[] openingClips;
        [BoxGroup("Audio")] [SerializeField] private AudioClip[] closingClips;
        [BoxGroup("Audio")] [SerializeField] private AudioClip[] closedClips;

        [FoldoutGroup("Events")] public UnityEvent onStartOpeningInwards;
        [FoldoutGroup("Events")] public UnityEvent onStartOpeningOutwards;
        [FoldoutGroup("Events")] public UnityEvent openingStartEvent;
        [FoldoutGroup("Events")] public UnityEvent openingEndEvent;
        [FoldoutGroup("Events")] public UnityEvent closingStartEvent;
        [FoldoutGroup("Events")] public UnityEvent closingEndEvent;

        public DoorPivotLocation DoorPivotLocation => doorPivotLocation;

        private bool IsOpen => _doorState == DoorState.Open;
        private bool IsMoving => _doorState == DoorState.Opening || _doorState == DoorState.Closing;

        private List<Transform> _blockers;
        private AudioSource _audioSource;

        private DoorState _doorState = DoorState.Closed;
        private Quaternion _doorClosedRotation;

        private void OnEnable()
        {
            _audioSource = this.EnsureComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.spatialBlend = 1.0f;

            SetDoorPivotLocation();
            _blockers = new List<Transform>();
        }

        private void Start()
        {
            // Assume door is initially closed
            _doorClosedRotation = transform.localRotation;
            _doorState = DoorState.Closed;

            _audioSource = GetComponent<AudioSource>();
            StopAllCoroutines();
        }

        /// <summary>
        /// Can be used to set the door audio configuration external to the Unity inspector
        /// </summary>
        public void SetDoorAudio(AudioMixerGroup audioMixerGroup, AudioClip[] newOpeningClips, AudioClip[] newOpenClips, AudioClip[] newClosingClips,
            AudioClip[] newClosedClips)
        {
            _audioSource.outputAudioMixerGroup = audioMixerGroup;

            openingClips = newOpeningClips;
            closingClips = newClosingClips;
            closedClips = newClosedClips;
        }

        /// <summary>
        /// Adds a 'blocking' transform that prevents the door from closing
        /// </summary>
        public void AddBlocker(Transform blocker)
        {
            _blockers.Add(blocker);
        }

        /// <summary>
        /// Removes a 'blocking' transform. If all blockers are removed, the door can close
        /// </summary>
        public void RemoveBlocker(Transform blocker)
        {
            _blockers.Remove(blocker);
        }

        /// <summary>
        /// Determines whether there are blockers preventing the door from closing
        /// </summary>
        private bool CanClose()
        {
            return _blockers == null || _blockers.Count == 0;
        }

        public void OpenAndCloseDoor(DoorTriggerLocation doorTriggerLocation)
        {
            if (IsMoving || IsOpen)
            {
                return;
            }

            DoorOpenDirection openDirection = GetDoorOpenDirection(doorTriggerLocation);
            StartCoroutine(OpenAndCloseDoorAsync(openDirection));
        }

        public void OpenDoor(DoorTriggerLocation doorTriggerLocation, bool immediate)
        {
            if (IsMoving || IsOpen)
            {
                return;
            }

            DoorOpenDirection openDirection = GetDoorOpenDirection(doorTriggerLocation);

            Debug.Log($"Trigger Location is: {doorTriggerLocation}, Pivot is: {doorPivotLocation}, Door opening: {openDirection}");

            float duration = immediate ? 0.0f : openingDuration;

            StartCoroutine(OpenDoorAsync(openDirection, duration));
        }

        private IEnumerator OpenDoorAsync(DoorOpenDirection direction, float moveDuration)
        {
            _doorState = DoorState.Opening;
            PlayRandomClip(openingClips);
            openingStartEvent.Invoke();
            if (direction == DoorOpenDirection.Inwards)
            {
                onStartOpeningInwards?.Invoke();
            }
            else
            {
                onStartOpeningOutwards?.Invoke();
            }

            float timer = 0;
            Quaternion startValue = transform.localRotation;

            Quaternion doorOpenRotation = gameObject.transform.localRotation *
                                          Quaternion.Euler(gameObject.transform.up * (direction == DoorOpenDirection.Inwards ? -openAngle : openAngle));

            while (timer < moveDuration)
            {
                transform.localRotation = Quaternion.Lerp(startValue, doorOpenRotation, timer / moveDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            _doorState = DoorState.Open;
            openingEndEvent.Invoke();
        }

        [Button("Open Door (From Outside)")]
        public void OpenDoorFromOutsideImmediate()
        {
            OpenDoor(DoorTriggerLocation.Outside, false);
        }

        [Button("Open Door (From Inside)")]
        public void OpenDoorFromInside()
        {
            OpenDoor(DoorTriggerLocation.Inside, false);
        }

        /// <summary>
        /// Determines which direction to open the door, based on where trigger is located and the pivot position of the door
        /// </summary>
        private DoorOpenDirection GetDoorOpenDirection(DoorTriggerLocation doorTriggerLocation)
        {
            return (doorTriggerLocation == DoorTriggerLocation.Inside && doorPivotLocation == DoorPivotLocation.BottomLeft) ||
                   (doorTriggerLocation == DoorTriggerLocation.Outside && doorPivotLocation == DoorPivotLocation.BottomRight)
                ? DoorOpenDirection.Inwards
                : DoorOpenDirection.Outwards;
        }

        [Button("Close Door")]
        public void CloseDoor()
        {
            CloseDoor(false);
        }

        public void CloseDoor(bool immediate)
        {
            // If already closed or closing, don't need to do anything
            if (_doorState == DoorState.Closed || _doorState == DoorState.Closing)
            {
                return;
            }

            float duration = immediate ? 0.0f : closingDuration;

            StartCoroutine(CloseDoorAsync(duration));
        }

        private IEnumerator CloseDoorAsync(float closeDuration)
        {
            // If the door is opening, wait for it to finish, then close
            while (_doorState == DoorState.Opening)
            {
                yield return null;
            }

            // Door closes
            _doorState = DoorState.Closing;
            PlayRandomClip(closingClips);
            closingStartEvent.Invoke();
            float timer = 0;
            Quaternion startValue = transform.localRotation;
            while (timer < closeDuration)
            {
                transform.localRotation = Quaternion.Lerp(startValue, _doorClosedRotation, timer / closeDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            transform.localRotation = _doorClosedRotation;

            _doorState = DoorState.Closed;
            PlayRandomClip(closedClips);
            closingEndEvent.Invoke();
        }

        private IEnumerator OpenAndCloseDoorAsync(DoorOpenDirection doorOpenDirection)
        {
            // Door opening
            yield return OpenDoorAsync(doorOpenDirection, openingDuration);

            // Door stays open
            yield return new WaitForSeconds(stayOpenDuration);

            // Door closes
            yield return CloseDoorAsync(closingDuration);
        }

        private void PlayRandomClip(AudioClip[] clips)
        {
            if (!_audioSource || clips.Length == 0)
            {
                return;
            }

            _audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }


        /// <summary>
        /// Sets the pivot location on the door
        /// </summary>
        private void SetDoorPivotLocation()
        {
            doorPivotLocation = CalculatePivotLocation();
        }

        /// <summary>
        /// Gets the location of the pivot on the door, so that we can accurately position the colliders
        /// This is set on the basis of transform.forward - in the case of 3D Forge assets, some doors are facing 'outwards'
        /// and so the pivot location may look wrong when looking from outside a building
        /// </summary>
        private DoorPivotLocation CalculatePivotLocation()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogWarning("No MeshFilter or mesh found.");
                return DoorPivotLocation.Unknown;
            }

            Mesh mesh = meshFilter.sharedMesh;
            Bounds bounds = mesh.bounds;

            // Step 1: Determine inferred facing direction based on largest face
            Vector3 size = bounds.size;
            Vector3 facingAxis;

            float xy = size.x * size.y;
            float yz = size.y * size.z;
            float xz = size.x * size.z;

            if (xy >= yz && xy >= xz)
            {
                facingAxis = Vector3.forward; // Z
            }
            else if (yz >= xz)
            {
                facingAxis = Vector3.right; // X
            }
            else
            {
                facingAxis = Vector3.up; // Y
            }

            // Step 2: Decide what axis is considered "horizontal" (left vs right)
            Vector3 rightAxis;
            if (facingAxis == Vector3.forward || facingAxis == Vector3.back)
            {
                rightAxis = Vector3.right;
            }
            else if (facingAxis == Vector3.right || facingAxis == Vector3.left)
            {
                rightAxis = Vector3.forward;
            }
            else
            {
                rightAxis = Vector3.right;
            }

            // Step 3: Get mesh pivot offset in local space
            Vector3 meshCenter = bounds.center;
            float offset = Vector3.Dot(meshCenter, rightAxis);

            float threshold = bounds.extents.magnitude * 0.05f; // 5% of size

            if (Mathf.Abs(offset) < threshold)
            {
                return DoorPivotLocation.Center;
            }

            return offset > 0f ? DoorPivotLocation.BottomRight : DoorPivotLocation.BottomLeft;
        }
    }
}