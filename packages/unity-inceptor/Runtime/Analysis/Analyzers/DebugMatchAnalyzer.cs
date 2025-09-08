using System;
using InceptorEngine.Analysis.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace InceptorEngine.Analysis.Analyzers
{
    /// <summary>
    /// A simple analyzer that checks for an exact, case-insensitive match between the user's input
    /// and the list of possible answers. This is primarily intended for debugging and testing purposes.
    /// </summary>
    [CreateAssetMenu(fileName = "DebugMatchAnalyzer", menuName = "Inceptor/Analyzers/Debug Match Analyzer")]
    public class DebugMatchAnalyzer : BaseNextClipAnalyzer
    {
        /// <summary>
        /// Analyzes the user input for an exact match.
        /// </summary>
        /// <param name="context">The context containing the user's input and possible answers.</param>
        /// <param name="onResultReceived">The callback to invoke with the index of the matched answer, or -1 if no match is found.</param>
        public override void AnalyzeNextClip(AnalysisContext context, UnityAction<int> onResultReceived)
        {
            for (int i = 0; i < context.PossibleAnswers.Count; i++)
            {
                if (string.Equals(context.UserInput, context.PossibleAnswers[i], StringComparison.OrdinalIgnoreCase))
                {
                    // Found an exact match.
                    onResultReceived?.Invoke(i);
                    return;
                }
            }
            
            // No exact match was found.
            onResultReceived?.Invoke(-1);
        }

        public override bool CanHandleContext(AnalysisContext context)
        {
            // This analyzer is simple and only needs the base information.
            // It can handle the base AnalysisContext and any class that inherits from it (like QuizAnalysisContext).
            return context is AnalysisContext;
        }

    }
}
