using InceptorEngine.Input.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace InceptorEngine.Input.Providers
{
    /// <summary>
    /// An input provider that listens for clicks on UI choice buttons.
    /// It works by subscribing to the global InputEvents.OnChoiceButtonClicked event.
    /// </summary>
    [CreateAssetMenu(fileName = "PointAndClickProvider", menuName = "Inceptor/Input/Point and Click Provider")]
    public class PointAndClickProvider : ScriptableObject, IInceptorInput
    {
        private UnityAction<string> _onResultCallback;
        private bool _isListening;

        public void RequestInput(UnityAction<string> onResultReceived)
        {
            if (_isListening) return;

            _onResultCallback = onResultReceived;
            InputEvents.OnChoiceButtonClicked += HandleButtonClick;
            _isListening = true;
        }

        public void CancelInputRequest()
        {
            if (!_isListening) return;
            
            InputEvents.OnChoiceButtonClicked -= HandleButtonClick;
            _onResultCallback = null;
            _isListening = false;
        }

        public bool IsListening() => _isListening;

        private void HandleButtonClick(string result)
        {
            _onResultCallback?.Invoke(result);
            // Once a button is clicked, this provider's job is done.
            CancelInputRequest();
        }
    }
}
