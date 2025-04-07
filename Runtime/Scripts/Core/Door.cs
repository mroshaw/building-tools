using System.Collections;
using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using DaftAppleGames.Extensions;

namespace DaftAppleGames.Darskerry.Core.Buildings
{
    public enum DoorOpenDirection
    {
        Inwards,
        Outwards
    }

    internal enum DoorState
    {
        Open,
        Opening,
        Closing,
        Closed
    }

    public class Door : MonoBehaviour
    {
        [BoxGroup("Settings")] [SerializeField] private float openAngle = 110.0f;
        [BoxGroup("Settings")] [SerializeField] private float openingTime = 2.0f;
        [BoxGroup("Settings")] [SerializeField] private float stayOpenTime = 5.0f;
        [BoxGroup("Settings")] [SerializeField] private float closingTime = 2.0f;

        [BoxGroup("Audio")] [SerializeField] private AudioClip[] openingClips;
        [BoxGroup("Audio")] [SerializeField] private AudioClip[] closingClips;
        [BoxGroup("Audio")] [SerializeField] private AudioClip[] closedClips;

        [FoldoutGroup("Events")] public UnityEvent onStartOpeningInwards;
        [FoldoutGroup("Events")] public UnityEvent onStartOpeningOutwards;
        [FoldoutGroup("Events")] public UnityEvent openingStartEvent;
        [FoldoutGroup("Events")] public UnityEvent openingEndEvent;
        [FoldoutGroup("Events")] public UnityEvent closingStartEvent;
        [FoldoutGroup("Events")] public UnityEvent closingEndEvent;

        private List<Transform> _blockers;

        private AudioSource _audioSource;

        private bool IsOpen => _doorState == DoorState.Open;
        private bool IsMoving => _doorState == DoorState.Opening || _doorState == DoorState.Closing;

        private DoorState _doorState = DoorState.Closed;
        private Quaternion _doorClosedRotation;

        private void Start()
        {
            _doorClosedRotation = transform.localRotation;
            _audioSource = GetComponent<AudioSource>();

            _doorState = DoorState.Closed;
            StopAllCoroutines();
        }

        public void AddBlocker(Transform blocker)
        {
            _blockers.Add(blocker);
        }

        public void RemoveBlocker(Transform blocker)
        {
            _blockers.Remove(blocker);
        }

        private bool CanClose()
        {
            return _blockers.Count == 0;
        }

        public void OpenAndCloseDoor(DoorOpenDirection doorOpenDirection)
        {
            if (IsMoving || IsOpen)
            {
                return;
            }

            StartCoroutine(OpenAndCloseDoorAsync(doorOpenDirection));
        }

        public void OpenDoor(DoorOpenDirection direction, bool immediate = false)
        {
            if (IsMoving || IsOpen)
            {
                return;
            }

            if (!immediate)
            {
                StartCoroutine(OpenDoorAsync(direction));
            }

            transform.localRotation = gameObject.transform.localRotation *
                                      Quaternion.Euler(gameObject.transform.up * (direction == DoorOpenDirection.Inwards ? -openAngle : openAngle));
            _doorState = DoorState.Open;
        }

        private IEnumerator OpenDoorAsync(DoorOpenDirection direction)
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

            while (timer < openingTime)
            {
                transform.localRotation = Quaternion.Lerp(startValue, doorOpenRotation, timer / openingTime);
                timer += Time.deltaTime;
                yield return null;
            }

            _doorState = DoorState.Open;
            openingEndEvent.Invoke();
        }


        [Button("Close Door")]
        public void CloseDoor(bool immediate = false)
        {
            if (IsMoving || !IsOpen)
            {
                return;
            }

            if (!immediate)
            {
                StartCoroutine(CloseDoorAsync());
            }

            transform.localRotation = _doorClosedRotation;
            _doorState = DoorState.Closed;
        }

        private IEnumerator CloseDoorAsync()
        {
            // Door closes
            _doorState = DoorState.Closing;
            PlayRandomClip(closingClips);
            closingStartEvent.Invoke();
            float timer = 0;
            Quaternion startValue = transform.localRotation;
            while (timer < closingTime)
            {
                transform.localRotation = Quaternion.Lerp(startValue, _doorClosedRotation, timer / closingTime);
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
            yield return OpenDoorAsync(doorOpenDirection);

            // Door stays open
            yield return new WaitForSeconds(stayOpenTime);

            // Door closes
            yield return CloseDoorAsync();
        }

        private void PlayRandomClip(AudioClip[] clips)
        {
            if (!_audioSource || clips.Length == 0)
            {
                return;
            }

            _audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }

        #region Unity Editor methods

#if UNITY_EDITOR
        public void ConfigureInEditor(AudioMixerGroup audioMixerGroup, AudioClip[] newOpeningClips, AudioClip[] newOpenClips, AudioClip[] newClosingClips,
            AudioClip[] newClosedClips)
        {
            if (!_audioSource)
            {
                _audioSource = this.EnsureComponent<AudioSource>();
            }

            _audioSource.outputAudioMixerGroup = audioMixerGroup;
            openingClips = newOpeningClips;
            openingClips = newOpenClips;
            closingClips = newClosingClips;
            closedClips = newClosedClips;
        }

        [Button("Open Door (Inward)")]
        internal void OpenDoorInwardEditor()
        {
            OpenDoor(DoorOpenDirection.Inwards, true);
        }

        [Button("Open Door (Outward)")]
        internal void OpenDoorOutwardEditor()
        {
            OpenDoor(DoorOpenDirection.Outwards, true);
        }

        [Button("Close Door")]
        internal void CloseDoorEditor()
        {
            CloseDoor(true);
        }
#endif

        #endregion
    }
}