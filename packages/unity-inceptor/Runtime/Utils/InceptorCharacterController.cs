using InceptorEngine.Clips;
using UnityEngine;

namespace InceptorEngine.Utils
{
    /// <summary>
    /// Manages the runtime behavior of a single character in an Inceptor scene.
    /// This component is responsible for playing audio and triggering animations
    /// based on the data provided by the currently executing Clip.
    /// </summary>
    [RequireComponent(typeof(Animator), typeof(AudioSource))]
    public class InceptorCharacterController : MonoBehaviour
    {
        #region Private Fields

        // --- Component References ---
        private AudioSource _audioSource;
        private Animator _animator;

        // --- State Tracking ---
        private string _lastBodyBehavior = "Idle";
        private string _lastMood = "Idle";

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Get references to required components on initialization.
            _audioSource = GetComponent<AudioSource>();
            _animator = GetComponent<Animator>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts playing the behaviors (audio and animation) for this character
        /// as defined in the provided ClipCharacterData.
        /// </summary>
        /// <param name="clipData">The data defining what the character should do in this clip.</param>
        /// <returns>The duration of the audio clip, or 0 if no audio is played.</returns>
        public float StartBehavior(ClipCharacterData clipData)
        {
            float audioDuration = 0f;

            // --- 1. Handle Audio ---
            if (clipData.isTalking)
            {
                // Always prefer the direct audio clip reference if it exists.
                if (clipData.audioClip != null)
                {
                    _audioSource.clip = clipData.audioClip;
                }
                else
                {
                    // Fallback to loading from Resources if no direct reference is provided.
                    string audioFileName = $"{Inceptor.Instance.CinematicScript.name}/{Inceptor.Instance.CurrentClipIndex}_{clipData.name}";
                    _audioSource.clip = Resources.Load<AudioClip>($"Audio/{audioFileName}");
                    if (_audioSource.clip != null)
                    {
                        Debug.LogWarning($"Loaded audio clip '{audioFileName}' from Resources. A direct reference in the ClipCharacterData is recommended.", this);
                    }
                }

                if (_audioSource.clip != null)
                {
                    _audioSource.Play();
                    audioDuration = _audioSource.clip.length;
                }
                else
                {
                     Debug.LogWarning($"Character '{name}' is set to talk, but no audio clip was found.", this);
                }
            }

            // --- 2. Handle Animations (using triggers) ---
            // This logic assumes the Animator Controller is set up to respond to these triggers.
            
            // Body animations (Mesh: "IdleToWalk", Hub-and-Spoke: "Walk")
            if (!string.IsNullOrEmpty(clipData.bodyBehavior))
            {
                string trigger = $"{_lastBodyBehavior}To{clipData.bodyBehavior}";
                _animator.SetTrigger(trigger.Replace('.', '_'));
                _lastBodyBehavior = clipData.bodyBehavior;
            }

            // Mood animations (Mesh: "MoodLayer_IdleToHappy", Hub-and-Spoke: "Happy")
            if (!string.IsNullOrEmpty(clipData.mood))
            {
                string trigger = $"MoodLayer_{_lastMood}To{clipData.mood}";
                _animator.SetTrigger(trigger.Replace('.', '_'));
                _lastMood = clipData.mood;
            }

            // Reaction animations (Both: "Gasp")
            if (!string.IsNullOrEmpty(clipData.reaction))
            {
                _animator.SetTrigger(clipData.reaction.Replace('.', '_'));
            }
            
            return audioDuration;
        }
        
        /// <summary>
        /// Immediately stops any audio that the character is currently playing.
        /// </summary>
        public void ForceStopAudio()
        {
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }
        }

        #endregion
    }
}
