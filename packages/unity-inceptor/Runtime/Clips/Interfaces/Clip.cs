using System;
using System.Collections;
using System.Collections.Generic;
using InceptorEngine.Analysis.Interfaces;
using InceptorEngine.Analysis;
using InceptorEngine.Input.Interfaces;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace InceptorEngine.Clips.Interfaces
{
    /// <summary>
    /// The abstract base class for all cinematic actions (Clips) in the Inceptor system.
    /// Each clip is a self-contained, reusable ScriptableObject.
    /// </summary>
    [Serializable]
    public abstract class Clip : ScriptableObject
    {
        #region Serialized Data
        
        [Header("Base Clip Settings")]
        [Tooltip("The list of characters involved in this clip and their specific data.")]
        [SerializeField]
        private List<ClipCharacterData> _characters;
        
        [Tooltip("The delay in seconds after this clip finishes before the next one starts.")]
        [SerializeField]
        private float _endDelay;
        
        [Tooltip("The default runtime duration of the clip in seconds. Can be overridden by audio length.")]
        [SerializeField]
        private float _clipRuntime;
        
        [Tooltip("The index of the camera to use for this clip.")]
        [SerializeField]
        private int _camera;

        public virtual IInceptorNextClipAnalyzer OverrideAnalyzer { get; }


        /// <summary>
        /// The UI element associated with this clip, which will be activated when the clip runs.
        /// </summary>
        protected GameObject _activeUI;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the name of this clip, as defined in the JSON metadata.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Gets a read-only list of the character data for this clip.
        /// </summary>
        public IReadOnlyList<ClipCharacterData> Characters => _characters;
        
        /// <summary>
        /// Gets the camera index assigned to this clip.
        /// </summary>
        public int CameraIndex => _camera;

        /// <summary>
        /// Gets a value indicating whether an external system has requested this clip to stop immediately.
        /// </summary>
        public bool IsStopForced { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// The main entry point called by the Inceptor to execute the clip's logic.
        /// This is a template method that sets up and then calls the abstract Run().
        /// </summary>
        /// <param name="runtimeContext">Contextual data required for the clip to run, like character controllers.</param>
        /// <param name="onClipEnd">A callback action to invoke when the clip finishes, passing the index of the next clip.</param>
        public IEnumerator RunClip(ClipRuntimeContext runtimeContext, UnityAction<int> onClipEnd)
        {
            IsStopForced = false;
            yield return Run(runtimeContext, onClipEnd);
        }

        /// <summary>
        /// [Editor-Only] The final, non-overridable method for populating this clip's data from a JSON object.
        /// It handles base initialization and then calls the abstract ParseCustomData method for child-specific logic.
        /// </summary>
        /// <param name="clipData">The JObject representing this clip from the cinematic script file.</param>
        public void InitializeFromJSON(JObject clipData)
        {
            // The 'name' property of a ScriptableObject is special. We set it here.
            name = clipData["metadata"]?["name"]?.Value<string>();
            
            // Guaranteed base initialization
            _characters = clipData["characters"]?.ToObject<List<ClipCharacterData>>() ?? new List<ClipCharacterData>();
            _camera = clipData["metadata"]?["camera"]?.Value<int>() ?? -1;

            // Call the abstract method for child classes to implement.
            ParseCustomData(clipData);
        }
        
        /// <summary>
        /// Forces the clip to stop its execution and cleans up any running behaviors.
        /// </summary>
        /// <param name="runtimeContext">The runtime context containing the character controllers to stop.</param>
        public virtual void ForceStop(ClipRuntimeContext runtimeContext)
        {
            IsStopForced = true;
            
            // Stop all the characters' behaviors
            foreach (var character in _characters)
            {
                if (runtimeContext.CharacterControllers.TryGetValue(character.name, out var characterController))
                {
                    characterController.ForceStopAudio();
                }
            }
        }

        #endregion

        #region Protected & Abstract Methods


        public abstract AnalysisContext CreateAnalysisContextForValidation();
        
        /// <summary>
        /// When implemented in a derived class, contains the core coroutine logic for the clip's execution.
        /// </summary>
        /// <param name="runtimeContext">Contextual data required for the clip to run.</param>
        /// <param name="onClipEnd">The callback to invoke with the index of the next clip when finished.</param>
        protected abstract IEnumerator Run(ClipRuntimeContext runtimeContext, UnityAction<int> onClipEnd);
        
        /// <summary>
        /// When implemented in a derived class, parses data specific to that clip type from the JSON object.
        /// This method is called automatically by InitializeFromJSON.
        /// </summary>
        /// <param name="clipData">The JObject for this clip.</param>
        protected abstract void ParseCustomData(JObject clipData);

        /// <summary>
        /// A helper method that iterates through the clip's characters and tells their controllers to start their assigned behavior.
        /// </summary>
        /// <param name="runtimeContext">The runtime context containing the character controllers.</param>
        /// <returns>The calculated maximum duration of all triggered behaviors.</returns>
        protected float PlayCharactersBehavior(ClipRuntimeContext runtimeContext)
        {
            float maxBehaviorTime = _clipRuntime;
            
            foreach (var character in _characters)
            {
                if (runtimeContext.CharacterControllers.TryGetValue(character.name, out var characterController))
                {
                    float behaviorTime = characterController.StartBehavior(character);
                    if (maxBehaviorTime < behaviorTime)
                    {
                        maxBehaviorTime = behaviorTime;
                    }
                }
            }
            return maxBehaviorTime;
        }

        /// <summary>
        /// Gets a collection of all possible next clip indices that this clip can branch to.
        /// This is used by validation tools to traverse the cinematic graph.
        /// </summary>
        public virtual IEnumerable<int> GetAllPossibleNextClips()
        {
            // By default, a clip has no exit points.
            // This can be overridden by child classes that have branching logic.
            yield break; 
        }

        #endregion
    }
}
