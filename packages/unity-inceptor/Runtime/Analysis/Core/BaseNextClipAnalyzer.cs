using InceptorEngine.Analysis.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace InceptorEngine.Analysis
{
    /// <summary>
    /// The abstract base class for all analyzer ScriptableObjects.
    /// It provides the common foundation for creating different analysis strategies.
    /// </summary>
    public abstract class BaseNextClipAnalyzer : ScriptableObject, IInceptorNextClipAnalyzer
    {
        /// <summary>
        /// When implemented in a derived class, performs an asynchronous analysis of the user's input.
        /// </summary>
        /// <param name="context">The context object containing the user's input and possible answers.</param>
        /// <param name="onResultReceived">The callback to invoke with the result (the index of the chosen answer, or -1 for failure).</param>
        public abstract void AnalyzeNextClip(AnalysisContext context, UnityAction<int> onResultReceived);

        public abstract bool CanHandleContext(AnalysisContext context);
    }
}
