using System.Collections.Generic;
using UnityEngine;

namespace InceptorEngine
{

        /// <summary>
    /// An enum to define the available animator generation strategies.
    /// </summary>
    public enum AnimatorGenerationStrategy
    {
        HubAndSpoke,
        Mesh
    }

    /// <summary>
    /// A data container that holds all the state and user-provided information
    /// for the Inceptor import wizard process.
    /// </summary>
    public class WizardContext
    {
        public CinematicScript cinematicScript;
        public string scriptName = "";
        public InceptorSettings settings;

        // --- User-provided assets ---
        public GameObject[] characterPrefabs;

        // --- Discovered animations ---
        public Dictionary<string, AnimationClip> bodyAnimations;
        public Dictionary<string, AnimationClip> moodAnimations;
        public Dictionary<string, AnimationClip> reactionAnimations;

        // --- Meta settings ---
        public float animationTransitionSpeed = 0.6f;
        public float questionStartDelay = 0.5f;
        public bool overwriteAudio = false;
        public AnimatorGenerationStrategy selectedStrategy = AnimatorGenerationStrategy.Mesh;

    }
}