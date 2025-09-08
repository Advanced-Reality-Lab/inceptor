using UnityEngine.Events;

namespace InceptorEngine.Analysis.Interfaces
{
    /// <summary>
    /// The contract for any class that can analyze user input to determine the next clip.
    /// </summary>
    public interface IInceptorNextClipAnalyzer
    {
        /// <summary>
        /// Performs an asynchronous analysis of the user's input.
        /// </summary>
        /// <param name="context">The context object containing the user's input and possible answers.</param>
        /// <param name="onResultReceived">The callback to invoke with the result (the index of the chosen answer, or -1 for failure).</param>
        void AnalyzeNextClip(AnalysisContext context, UnityAction<int> onResultReceived);

        bool CanHandleContext(AnalysisContext context);
    }
}
