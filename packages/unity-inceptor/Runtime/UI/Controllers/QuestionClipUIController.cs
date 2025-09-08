using System;
using System.Collections;
using System.Collections.Generic;
using InceptorEngine.Input;
using InceptorEngine.UI.Core;
using InceptorEngine.UI.Interfaces;
using InceptorEngine.UI.Components;
using TMPro;
using UnityEngine;

namespace InceptorEngine.UI.Controllers
{
    /// <summary>
    /// A comprehensive UI controller for handling interactive choice and quiz clips.
    /// It manages animations, button states, and feedback based on the data it receives.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class QuestionClipUIController : BaseInceptorUI, IInceptorQuizUI
    {
        #region Serialized Fields

        [Header("References")]
        [Tooltip("A list of the button components that will display the answers.")]
        [SerializeField] private List<AnimatedResponseButton> _answerButtons;
        [SerializeField] private FeedbackUI _feedbackPanel; // Optional

        [Header("Configuration")]
        [SerializeField, Range(0f, 5f)] private float _delayAfterAnswer = 1.5f;
        
        [Header("Animation")]
        [Tooltip("This value will be added to any animation length to give the user time to see the result.")]
        [SerializeField] private float _additionalAnimationDelay = 0.5f;
        [SerializeField] private string _showCanvasTrigger = "Show";
        [SerializeField] private string _hideCanvasTrigger = "Hide";
        [SerializeField] private string _showFeedbackTrigger = "ShowFeedback";
        [SerializeField] private string _hideFeedbackTrigger = "HideFeedback";

        [Header("Sound Effects")]
        [SerializeField] private AudioClip _selectedSound;
        [SerializeField] private AudioClip _correctSound;
        [SerializeField] private AudioClip _incorrectSound;
        
        #endregion

        #region Private State

        private Animator _animator;
        private bool _isAnimating;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            
            // Hook up the buttons to fire the global input event when clicked.
            for (int i = 0; i < _answerButtons.Count; i++)
            {
                // This is a common C# gotcha. We need to capture the index in a local variable
                // for the closure to work correctly inside a loop.
                int buttonIndex = i; 
                _answerButtons[i].Button.onClick.AddListener(() => OnChoiceButtonClicked(buttonIndex));
            }

            if (_feedbackPanel != null)
            {
                _feedbackPanel.OnHidden += OnFeedbackHidden;
            }
        }

        #endregion

        #region Interface Implementations

        public override IEnumerator Show()
        {
            gameObject.SetActive(true);
            _animator.SetTrigger(_showCanvasTrigger);
            // A simple way to wait for an animation to finish. A more robust system might use Animation Events.
            yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length + _additionalAnimationDelay);
        }

        public override IEnumerator Hide()
        {
            _answerButtons.ForEach(button => button.Reset());
            _animator.SetTrigger(_hideCanvasTrigger);
            yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length + _additionalAnimationDelay);
            gameObject.SetActive(false);
        }

        public override bool IsUIAnimating() => _isAnimating;

        public void SetChoices(IReadOnlyList<string> choices)
        {
            for (int i = 0; i < _answerButtons.Count; i++)
            {
                if (i < choices.Count)
                {
                    _answerButtons[i].gameObject.SetActive(true);
                    _answerButtons[i].SetText(choices[i]);
                }
                else
                {
                    _answerButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void ShowError(InceptorErrorType errorType)
        {
            // For now, we just show a generic error on the feedback panel.
            // This could be expanded to show different messages based on the errorType.
            if (_feedbackPanel != null)
            {
                _feedbackPanel.ShowFeedback("Sorry, I didn't understand. Please try again.");
                _animator.SetTrigger(_showFeedbackTrigger);
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
            InputEvents.OnChoiceButtonClicked?.Invoke(_answerButtons[buttonIndex].GetText());
        }

        private IEnumerator ShowFeedbackRoutine(bool isCorrect, int userAnswerIndex, int correctAnswerIndex, string feedbackText)
        {
            _isAnimating = true;
            
            PlaySound(_selectedSound);
            _answerButtons[userAnswerIndex].Select();
            yield return new WaitForSeconds(_delayAfterAnswer);

            if (isCorrect)
            {
                PlaySound(_correctSound);
                _answerButtons[userAnswerIndex].CorrectAnswer();
            }
            else
            {
                PlaySound(_incorrectSound);
                _answerButtons[userAnswerIndex].IncorrectAnswer();
                if (correctAnswerIndex >= 0)
                {
                    _answerButtons[correctAnswerIndex].NotChosenCorrectAnswer();
                }
            }
            
            yield return new WaitForSeconds(_delayAfterAnswer);

            if (_feedbackPanel != null && !string.IsNullOrEmpty(feedbackText))
            {
                _feedbackPanel.ShowFeedback(feedbackText);
            }
            else
            {
                // If there's no feedback to show, we're done animating.
                _isAnimating = false;
            }
        }

        private void OnFeedbackHidden()
        {
            _animator.SetTrigger(_hideFeedbackTrigger);
            _isAnimating = false;
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            }
        }

        #endregion
    }
}
