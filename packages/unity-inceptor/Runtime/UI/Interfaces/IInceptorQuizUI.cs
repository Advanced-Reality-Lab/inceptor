using System.Collections.Generic;

namespace InceptorEngine.UI.Interfaces
{
    /// <summary>
    /// A specialized contract for UIs that can handle quiz-like interactions,
    /// providing specific feedback for correct and incorrect answers.
    /// </summary>
    public interface IInceptorQuizUI : IInceptorInteractiveUI
    {
        /// <summary>
        /// Displays feedback for a correctly selected answer.
        /// </summary>
        /// <param name="answerIndex">The index of the correct answer that was selected.</param>
        /// <param name="feedbackText">The feedback text to display.</param>
        void ShowCorrectFeedback(int answerIndex, string feedbackText);

        /// <summary>
        /// Displays feedback for an incorrectly selected answer.
        /// </summary>
        /// <param name="userAnswerIndex">The index of the incorrect answer the user selected.</param>
        /// <param name="correctAnswerIndex">The index of the actual correct answer.</param>
        /// <param name="feedbackText">The feedback text to display for the user's incorrect choice.</param>
        void ShowIncorrectFeedback(int userAnswerIndex, int correctAnswerIndex, string feedbackText);
    }
}
