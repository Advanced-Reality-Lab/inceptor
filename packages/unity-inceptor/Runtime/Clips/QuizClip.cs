using System;
using System.Collections;
using InceptorEngine.Clips.Interfaces;
using InceptorEngine.UI.Interfaces;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using InceptorEngine.Analysis;


namespace InceptorEngine.Clips
{
    /// <summary>
    /// A specialized clip that presents a question with a single correct answer.
    /// It provides feedback to the user based on whether their choice was correct.
    /// </summary>
    [CreateAssetMenu(menuName = "Inceptor/Clip/Quiz Clip", fileName = "QuizClip", order = 4)]
    [Serializable]
    public class QuizClip : ChoiceClip
    {
        #region Static Events
        
        /// <summary>
        /// A global event fired after a user's response has been analyzed.
        /// The int is the selected answer index, and the bool is whether it was correct.
        /// </summary>
        public static UnityAction<int, bool> ResponseCorrectness;

        #endregion

        #region Serialized Data

        [Header("Quiz-Specific Data")]
        [Tooltip("The feedback text to show for each corresponding answer.")]
        [SerializeField]
        private string[] _answersFeedbacks;
        
        [Tooltip("The index of the correct answer within the answers array.")]
        [SerializeField]
        private int _correctAnswerIndex;
        
        #endregion

        #region Public Properties

        public IReadOnlyList<string> AnswerFeedbacks => _answersFeedbacks;
        public int CorrectAnswerIndex => _correctAnswerIndex;

        #endregion

        #region Core Methods

        /// <summary>
        /// [Editor-Only] Parses the data specific to the QuizClip from the JSON object,
        /// after the base ChoiceClip data has been parsed.
        /// </summary>
        protected override void ParseCustomData(JObject clipData)
        {
            // First, let the base class parse its data.
            base.ParseCustomData(clipData);

            // Now, parse the data unique to this quiz clip.
            _correctAnswerIndex = clipData["metadata"]?["correctAnswerIndex"]?.Value<int>() ?? -1;
            _answersFeedbacks = clipData["metadata"]?["feedback"]?.ToObject<string[]>() ?? Array.Empty<string>();
        }
        
        /// <summary>
        /// Overrides the base ShowFeedback method to provide correctness-based UI feedback.
        /// </summary>
        protected IEnumerator ShowFeedback(IInceptorUI uiController, IInceptorQuizUI uiInteractive)
        {
            // First, ensure we have a valid result from the analyzer.
            if (_analyzerResult == null) yield break;

            if (_analyzerResult.Value == _correctAnswerIndex)
            {
                uiInteractive.ShowCorrectFeedback(_analyzerResult.Value, _answersFeedbacks[_correctAnswerIndex]);
                ResponseCorrectness?.Invoke(_analyzerResult.Value, true);
            }
            else
            {
                uiInteractive.ShowIncorrectFeedback(_analyzerResult.Value, _correctAnswerIndex, _answersFeedbacks[_analyzerResult.Value]);
                ResponseCorrectness?.Invoke(_analyzerResult.Value, false);
            }

            // Wait for any feedback animations (like a checkmark or X) to finish.
            yield return new WaitUntil(() => !uiController.IsUIAnimating());
        }

         public override AnalysisContext CreateAnalysisContextForValidation()
        {
            // This clip provides the specialized QuizAnalysisContext.
            return new QuizAnalysisContext();
        }

        #endregion

        #region Static Initialization

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeQuizEvents()
        {
            ResponseCorrectness = null;
        }

        #endregion
    }
}
