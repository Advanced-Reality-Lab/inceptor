using System;
using System.Collections;
using System.Collections.Generic;
using InceptorEngine.Analysis.Interfaces;
using InceptorEngine.Input.Interfaces;
using InceptorEngine.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace InceptorEngine.Clips.Interfaces
{
    /// <summary>
    /// A data container passed from the Inceptor to a Clip at runtime. It provides all necessary
    /// scene and service dependencies, allowing the Clip to be a self-contained unit of logic.
    /// </summary>
    public class ClipRuntimeContext
    {
        /// <summary>
        /// A dictionary mapping character names to their active controller instances in the scene.
        /// </summary>
        public IReadOnlyDictionary<string, InceptorCharacterController> CharacterControllers { get; }

        /// <summary>
        /// The analysis service chosen by the Inceptor for this specific clip.
        /// </summary>
        public IInceptorNextClipAnalyzer ActiveAnalyzer { get; }

        /// <summary>
        /// The input service that will provide user input to the clip.
        /// </summary>
        public IInceptorInput InputProvider { get; }

        public ClipRuntimeContext(
            IReadOnlyDictionary<string, InceptorCharacterController> characterControllers,
            IInceptorNextClipAnalyzer activeAnalyzer,
            IInceptorInput inputProvider)
        {
            CharacterControllers = characterControllers;
            ActiveAnalyzer = activeAnalyzer;
            InputProvider = inputProvider;
        }
    }
}
