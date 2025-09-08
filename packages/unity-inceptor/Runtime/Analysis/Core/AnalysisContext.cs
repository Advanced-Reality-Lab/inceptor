using System.Collections.Generic;

namespace InceptorEngine.Analysis
{
    /// <summary>
    /// A data container that provides all the necessary information from a Clip
    /// to an IInceptorNextClipAnalyzer for it to perform its analysis.
    /// </summary>
    public class AnalysisContext
    {
        /// <summary>
        /// The text input provided by the user.
        /// </summary>
        public string UserInput { get; set; }

        /// <summary>
        /// The list of possible valid answers or choices for the current clip.
        /// </summary>
        public IReadOnlyList<string> PossibleAnswers { get; set; }
    }
}
