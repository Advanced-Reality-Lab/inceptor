using System;
using System.Collections;
using System.Collections.Generic;
using InceptorEngine.Clips.Interfaces;
using InceptorEngine.UI.Interfaces;
using InceptorEngine.Analysis;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace InceptorEngine.Clips
{
        /// <summary>
    /// A temporary enum to satisfy the ShowError<T> generic constraint.
    /// This can be replaced with a project-wide error enum in the future.
    /// </summary>
    public enum ClipErrorType { AnalysisFailed }


    /// <summary>
    /// A base clip for presenting a choice to the user, analyzing their response,
    /// and branching the narrative accordingly. This clip does not have a concept of a "correct" answer.
    /// </summary>
    [CreateAssetMenu(menuName = "Inceptor/Clip/Choice Clip", fileName = "ChoiceClip", order = 3)]
    [Serializable]
    public class ChoiceClip : Clip
    {
        #region Static Events
        
        /// <summary>
        /// A global event fired when a choice/question is presented to the user.
        /// The string payload is the question text.
        /// </summary>
        public static UnityAction<string> ChoicePresented;

        #endregion

        #region Serialized Data

        [Header("Choice Configuration")]
        [Tooltip("The name of the UI prefab to instantiate for this choice.")]
        [SerializeField]
        private string _uiName = "ChoiceClipUI";

        [Header("Choice Data")]
        [Tooltip("The text of the question to be presented to the user.")]
        [SerializeField]
        private string _questionText;
        
        [Tooltip("The possible choices/answers for this clip.")]
        [SerializeField]
        private string[] _answers;
        
        [Tooltip("The index of the clip to jump to for each corresponding answer.")]
        [SerializeField]
        private int[] _nextClips;

        #endregion

        #region Private Runtime State

        private string _inputResult;
        protected int? _analyzerResult; // Protected to allow child access

        #endregion

        #region Public Properties

        public string QuestionText => _questionText;
        public IReadOnlyList<string> Answers => _answers;
        public IReadOnlyList<int> NextClips => _nextClips;

        #endregion

        #region Core Methods

        /// <summary>
        /// [Editor-Only] Parses the data specific to this clip type from the JSON object.
        /// </summary>
        protected override void ParseCustomData(JObject clipData)
        {
            _uiName = clipData["metadata"]?["uiName"]?.Value<string>() ?? "ChoiceClipUI";
            // FIX: The question text is now a dedicated field in the JSON.
            _questionText = clipData["metadata"]?["questionText"]?.Value<string>() ?? "";
            _answers = clipData["metadata"]?["answers"]?.ToObject<string[]>() ?? Array.Empty<string>();
            _nextClips = clipData["metadata"]?["nextClips"]?.ToObject<int[]>() ?? Array.Empty<int>();
        }

        /// <summary>
        /// The core coroutine logic. It orchestrates asking, listening, analyzing, and responding.
        /// </summary>
        protected override IEnumerator Run(ClipRuntimeContext runtimeContext, UnityAction<int> onClipEnd)
        {
            float leadInDuration = PlayCharactersBehavior(runtimeContext);
            yield return new WaitForSeconds(leadInDuration);
            if (IsStopForced) yield break;

            ChoicePresented?.Invoke(_questionText);
            
            // The Inceptor is still responsible for creating/destroying UI for now.
            GameObject uiInstance = Inceptor.Instance.CreateUI(_uiName);
            var uiController = uiInstance.GetComponent<IInceptorUI>();
            var uiInteractive = uiInstance.GetComponent<IInceptorInteractiveUI>();

            yield return new WaitUntil(() => uiController.IsLoaded());
            if (IsStopForced) yield break;
            
            uiInteractive.SetChoices(_answers); // No feedback in the base choice clip
            yield return uiController.Show();
            if (IsStopForced) yield break;
            
            yield return GetAndProcessAnswer(runtimeContext, uiInteractive);
            if (IsStopForced) yield break;

            // This is a virtual method that child classes can override to add feedback logic.
            yield return ShowFeedback(uiController, uiInteractive);
            if (IsStopForced) yield break;
            
            yield return Inceptor.Instance.RemoveUI(uiInstance);
            if (IsStopForced) yield break;
            
            onClipEnd?.Invoke(_nextClips[_analyzerResult.Value]);
        }
        
        #endregion

        #region Private & Protected Coroutines

        /// <summary>
        /// A sub-coroutine that handles the loop of getting user input and analyzing it.
        /// </summary>
        private IEnumerator GetAndProcessAnswer(ClipRuntimeContext runtimeContext, IInceptorInteractiveUI ui)
        {
            _analyzerResult = null;
            while (_analyzerResult is null or -1)
            {
                _inputResult = null;
                
                runtimeContext.InputProvider.RequestInput(OnInputReceived);
                yield return new WaitUntil(() => !string.IsNullOrEmpty(_inputResult));
                if (IsStopForced) yield break;

                var context = new AnalysisContext
                {
                    UserInput = _inputResult,
                    PossibleAnswers = _answers
                };

                runtimeContext.ActiveAnalyzer.AnalyzeNextClip(context, OnAnalyzerResultReceived);
                yield return new WaitUntil(() => _analyzerResult != null);
                if (IsStopForced) yield break;
                
                if (_analyzerResult == -1)
                {
                    // Assuming a non-generic ShowError() method exists for general failures.
                    ui.ShowError(InceptorErrorType.AnalysisFailed);
                }
            }
        }
        
        /// <summary>
        /// A virtual method for showing feedback. The base implementation does nothing.
        /// Child classes like QuizClip will override this to provide feedback logic.
        /// </summary>
        protected virtual IEnumerator ShowFeedback(IInceptorUI uiController, IInceptorInteractiveUI uiInteractive)
        {
            // Base implementation has no feedback.
            yield break;
        }

        #endregion

        #region Callbacks

        private void OnInputReceived(string input) => _inputResult = input;
        private void OnAnalyzerResultReceived(int result) => _analyzerResult = result;

        #endregion

        #region Static Initialization

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            ChoicePresented = null;
        }

        public override IEnumerable<int> GetAllPossibleNextClips()
        {
            return NextClips;
        }

        public override AnalysisContext CreateAnalysisContextForValidation()
        {
            // This clip provides the base AnalysisContext.
            return new AnalysisContext();
        }

        #endregion
    }
}
