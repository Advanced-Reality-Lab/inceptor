using System.Collections.Generic;

namespace InceptorEngine.Analysis
{
    /// <summary>
    /// A specialized context object for clips that have a concept of a "correct" answer.
    /// It inherits from the base AnalysisContext and adds quiz-specific data.
    /// </summary>
    public class QuizAnalysisContext : AnalysisContext
    {
        /// <summary>
        /// The index of the correct answer within the PossibleAnswers list.
        /// </summary>
        public int CorrectAnswerIndex { get; set; }

        /// <summary>
        // The list of feedback strings corresponding to each possible answer.
        /// </summary>
        public IReadOnlyList<string> AnswerFeedbacks { get; set; }
    }
}
