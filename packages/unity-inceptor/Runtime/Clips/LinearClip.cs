using System;
using System.Collections;
using InceptorEngine.Clips.Interfaces;
using InceptorEngine.Analysis;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace InceptorEngine.Clips
{
    /// <summary>
    /// A simple, linear clip that plays its associated behaviors and then proceeds
    /// to a predefined next clip index.
    /// </summary>
    [CreateAssetMenu(menuName = "Inceptor/Clip/Linear Clip", fileName = "LinearClip", order = 1)]
    [Serializable]
    public class LinearClip : Clip
    {
        [Tooltip("The index of the clip to play immediately after this one finishes.")]
        [SerializeField]
        private int _nextClip;

        /// <summary>
        /// Gets the index of the next clip to play.
        /// </summary>
        public int NextClipIndex => _nextClip;
        
        /// <summary>
        /// [Editor-Only] Parses the data specific to this clip type from the JSON object.
        /// In this case, it only needs to parse the next clip index.
        /// </summary>
        /// <param name="clipData">The JObject representing this clip from the cinematic script file.</param>
        protected override void ParseCustomData(JObject clipData)
        {
            // We only need to parse the data unique to this clip type.
            _nextClip = clipData["metadata"]?["nextClip"]?.Value<int>() ?? -1; // Use -1 as a sentinel for "end".
        }
        
        /// <summary>
        /// The core coroutine logic for this clip. It plays character behaviors, waits for them
        /// to finish, and then invokes the onClipEnd callback with the next clip's index.
        /// </summary>
        protected override IEnumerator Run(ClipRuntimeContext runtimeContext, UnityAction<int> onClipEnd)
        {
            // Start all character behaviors and get the calculated duration.
            float clipLength = PlayCharactersBehavior(runtimeContext);
            
            // Wait for the duration of the clip.
            // A more complex clip might break this wait into smaller chunks to check the IsStopForced flag more often.
            yield return new WaitForSeconds(clipLength);

            // If an external system forced a stop during the wait, exit the coroutine immediately.
            if (IsStopForced)
            {
                yield break;
            }
            
            // Invoke the callback to tell the Inceptor which clip to play next.
            onClipEnd?.Invoke(_nextClip);
        }

        public override IEnumerable<int> GetAllPossibleNextClips()
        {
            // Create a new list of integers.
            var nextClips = new List<int>();
            // Add the single next clip index to the list.
            nextClips.Add(NextClipIndex);
            // Return the list.
            return nextClips;
        }

        public override AnalysisContext CreateAnalysisContextForValidation()
        {
            // This clip provides the base AnalysisContext.
            return new AnalysisContext();
        }
    }
}
