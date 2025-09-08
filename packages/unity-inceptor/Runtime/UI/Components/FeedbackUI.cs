using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace InceptorEngine.UI.Components // Moved to the 'Components' sub-namespace
{
    /// <summary>
    /// A UI component that displays a piece of text for a specified duration
    /// and then fires an event when it has finished.
    /// </summary>
    public class FeedbackUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Configuration")] 
        [Tooltip("The TextMeshPro component used to display the feedback text.")]
        [SerializeField] private TextMeshProUGUI _text;
        
        [Tooltip("The duration in seconds to display the feedback before automatically hiding.")]
        [SerializeField] private float _duration = 7f;

        #endregion

        #region Public Events

        /// <summary>
        /// An event fired when the feedback has finished its display cycle (either by timeout or by being forced).
        /// The QuestionClipUIController listens for this to know when to proceed.
        /// </summary>
        public event UnityAction OnHidden;

        #endregion

        #region Private State

        private Coroutine _disableCoroutine;

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows the feedback panel with the provided text and starts the auto-hide timer.
        /// </summary>
        /// <param name="text">The feedback text to display.</param>
        public void ShowFeedback(string text)
        {
            // Stop any previous auto-hide timer that might be running.
            if (_disableCoroutine != null)
            {
                StopCoroutine(_disableCoroutine);
            }

            _text.text = text;
            gameObject.SetActive(true);
            
            _disableCoroutine = StartCoroutine(DisableAfterDuration());
        }
    
        /// <summary>
        /// Immediately stops the timer and hides the feedback panel.
        /// </summary>
        public void ForceHide()
        {
            if (!gameObject.activeInHierarchy) return;
            
            if (_disableCoroutine != null)
            {
                StopCoroutine(_disableCoroutine);
                _disableCoroutine = null;
            }
            
            OnHidden?.Invoke();
        }

        #endregion

        #region Private Coroutine

        /// <summary>
        /// A coroutine that waits for the specified duration and then invokes the OnHidden event.
        /// </summary>
        private IEnumerator DisableAfterDuration()
        {
            yield return new WaitForSeconds(_duration);
            
            // The coroutine is finished, so we can clear the reference.
            _disableCoroutine = null;
            
            OnHidden?.Invoke();
        }

        #endregion
    }
}
