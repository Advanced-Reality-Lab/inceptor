using System;
using UnityEngine;

namespace InceptorEngine.Clips
{
    /// <summary>
    /// A data structure that holds all the specific information for a single character's
    /// performance within a single clip.
    /// </summary>
    [Serializable]
    public struct ClipCharacterData
    {
        [Tooltip("The name of the character performing the actions.")]
        public string name;

        [Tooltip("The line of dialogue the character speaks in this clip.")]
        public string text;

        [Tooltip("A direct reference to the audio clip for the character's dialogue.")]
        public AudioClip audioClip;

        [Tooltip("Is this character the primary speaker in this clip?")]
        public bool isTalking;

        [Tooltip("The name of the sustained facial expression animation (e.g., 'Happy', 'Sad').")]
        public string mood;

        [Tooltip("The name of a one-shot facial animation to play (e.g., 'Wink', 'Gasp').")]
        public string reaction;

        [Tooltip("The name of the full-body animation to play (e.g., 'Walking', 'Waving').")]
        public string bodyBehavior;
    }
}
