using System;
using System.Collections;
using System.Collections.Generic;
using InceptorEngine.Input;
using InceptorEngine.UI.Core;
using InceptorEngine.UI.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InceptorEngine.UI.Components
{
    /// <summary>
    /// A simple, non-animated UI controller for quiz clips. It uses colored outlines
    /// on buttons to provide feedback to the user.
    /// </summary>
    public class BasicQuestionClipUIController : BaseInceptorUI, IInceptorQuizUI
    {
        #region Serialized Fields

        [Header("References")]
        [Tooltip("A list of the GameObjects representing the answer boxes. Each should have a Button, Outline, and TextMeshProUGUI component.")]
        [SerializeField] private List<GameObject> _answerBoxes;
        [Tooltip("The UI component used to display feedback text after an answer.")]
        [SerializeField] private FeedbackUI _feedbackUI;

        [Header("Feedback Colors")]
        [SerializeField] private Color _outlineSelectedColor = Color.yellow;
        [SerializeField] private Color _outlineCorrectColor = Color.green;
        [SerializeField] private Color _outlineWrongColor = Color.red;

        [Header("Timing")] 
        [SerializeField] private float _delayAfterAnswer = 1.5f;
        
        #endregion

        #region Private State

        private List<Button> _buttons = new List<Button>();
        private List<Outline> _outlines = new List<Outline>();
        private List<TextMeshProUGUI> _textFields = new List<TextMeshProUGUI>();
        private bool _isAnimatingFeedback;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache all necessary components from the answer boxes.
            for (int i = 0; i < _answerBoxes.Count; i++)
            {
                var box = _answerBoxes[i];
                _buttons.Add(box.GetComponent<Button>());
                _outlines.Add(box.GetComponent<Outline>());
                _textFields.Add(box.GetComponentInChildren<TextMeshProUGUI>());
                
                // Hook up the button's onClick event to fire our global input event.
                int buttonIndex = i; // Capture index for the closure
                _buttons[i].onClick.AddListener(() => OnChoiceButtonClicked(buttonIndex));
            }

            if (_feedbackUI != null)
            {
                _feedbackUI.OnHidden += OnFeedbackHidden;
            }
        }

        #endregion

        #region Interface Implementations

        public override bool IsUIAnimating() => _isAnimatingFeedback;

        public void SetChoices(IReadOnlyList<string> choices)
        {
            for (int i = 0; i < _answerBoxes.Count; i++)
            {
                if (i < choices.Count)
                {
                    _answerBoxes[i].SetActive(true);
                    _textFields[i].text = choices[i];
                }
                else
                {
                    _answerBoxes[i].SetActive(false);
                }
            }
            ResetOutlines();
        }

        public void ShowError(InceptorErrorType errorType)
        {
            // Show a generic error message using the feedback panel.
            if (_feedbackUI != null)
            {
                _feedbackUI.ShowFeedback("Sorry, I couldn't understand that. Please try again.");
            }
        }

        public void ShowCorrectFeedback(int answerIndex, string feedbackText)
        {
            StartCoroutine(ShowFeedbackRoutine(true, answerIndex, -1, feedbackText));
        }

        public void ShowIncorrectFeedback(int userAnswerIndex, int correctAnswerIndex, string feedbackText)
        {
            StartCoroutine(ShowFeedbackRoutine(false, userAnswerIndex, correctAnswerIndex, feedbackText));
        }

        #endregion

        #region Private Logic

        private void OnChoiceButtonClicked(int buttonIndex)
        {
            // Fire the global event that the PointAndClickProvider is listening for.
            InputEvents.OnChoiceButtonClicked?.Invoke(_textFields[buttonIndex].text);
        }

        private IEnumerator ShowFeedbackRoutine(bool isCorrect, int userAnswerIndex, int correctAnswerIndex, string feedbackText)
        {
            _isAnimatingFeedback = true;
            
            // Show the user's selection first.
            _outlines[userAnswerIndex].effectColor = _outlineSelectedColor;
            _outlines[userAnswerIndex].enabled = true;
            
            yield return new WaitForSeconds(_delayAfterAnswer);

            if (isCorrect)
            {
                _outlines[userAnswerIndex].effectColor = _outlineCorrectColor;
            }
            else
            {
                _outlines[userAnswerIndex].effectColor = _outlineWrongColor;
                if (correctAnswerIndex >= 0 && correctAnswerIndex < _outlines.Count)
                {
                    _outlines[correctAnswerIndex].effectColor = _outlineCorrectColor;
                    _outlines[correctAnswerIndex].enabled = true;
                }
            }
            
            yield return new WaitForSeconds(_delayAfterAnswer);

            if (_feedbackUI != null && !string.IsNullOrEmpty(feedbackText))
            {
                _feedbackUI.ShowFeedback(feedbackText);
            }
            else
            {
                // If there's no feedback, we're done animating.
                _isAnimatingFeedback = false;
            }
        }

        private void OnFeedbackHidden()
        {
            // This is called by the FeedbackUI when its timer runs out.
            _isAnimatingFeedback = false;
        }

        private void ResetOutlines()
        {
            foreach (var outline in _outlines)
            {
                outline.enabled = false;
            }
        }

        #endregion
    }
}
