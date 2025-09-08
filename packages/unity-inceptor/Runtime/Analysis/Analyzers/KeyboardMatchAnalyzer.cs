using System;
using InceptorEngine.Analysis.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace InceptorEngine.Analysis.Analyzers
{
    /// <summary>
    /// An analyzer that checks if any of the possible answer strings appear as a keyword
    /// within the user's input string, ignoring case.
    /// </summary>
    [CreateAssetMenu(fileName = "KeywordMatchAnalyzer", menuName = "Inceptor/Analyzers/Keyword Match Analyzer")]
    public class KeywordMatchAnalyzer : BaseNextClipAnalyzer
    {
        /// <summary>
        /// Analyzes the user input for a keyword match.
        /// </summary>
        /// <param name="context">The context containing the user's input and possible answers.</param>
        /// <param name="onResultReceived">The callback to invoke with the index of the first matched keyword, or -1 if no match is found.</param>
        public override void AnalyzeNextClip(AnalysisContext context, UnityAction<int> onResultReceived)
        {
            for (int i = 0; i < context.PossibleAnswers.Count; i++)
            {
                // Check if the user's input contains the keyword, ignoring case.
                if (context.UserInput.IndexOf(context.PossibleAnswers[i], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Found a keyword match.
                    onResultReceived?.Invoke(i);
                    return;
                }
            }
            
            // No keyword match was found.
            onResultReceived?.Invoke(-1);
        }

        /// <summary>
        /// Checks if this analyzer can process the given context.
        /// This simple analyzer can handle any context that is at least a base AnalysisContext.
        /// </summary>
        /// <param name="context">The context object provided by a Clip.</param>
        /// <returns>True if the context is a valid type, otherwise false.</returns>
        public override bool CanHandleContext(AnalysisContext context)
        {
            // This analyzer is also simple and only needs the base information.
            return context is AnalysisContext;
        }
    }
}
