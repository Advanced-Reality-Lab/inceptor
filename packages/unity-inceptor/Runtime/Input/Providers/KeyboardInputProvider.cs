using InceptorEngine.Input.Interfaces;
using UnityEngine;
using UnityEngine.Events;
using TMPro; // Requires TextMeshPro package

namespace InceptorEngine.Input.Providers
{
    /// <summary>
    /// A simple input provider that gets text input from the user via the keyboard.
    /// Creates a temporary UI element to capture the input.
    /// </summary>
    [CreateAssetMenu(fileName = "KeyboardInputProvider", menuName = "Inceptor/Input/Keyboard Provider")]
    public class KeyboardInputProvider : ScriptableObject, IInceptorInput
    {
        [Tooltip("A reference to a simple prefab with a Canvas and an TMP_InputField.")]
        [SerializeField] private GameObject _inputFieldPrefab;
        
        private KeyboardInputUI _activeUIInstance;

        public void RequestInput(UnityAction<string> onResultReceived)
        {
            if (IsListening()) return;

            GameObject uiInstance = Instantiate(_inputFieldPrefab);
            _activeUIInstance = uiInstance.GetComponent<KeyboardInputUI>();
            _activeUIInstance.Activate(onResultReceived, OnRequestFinished);
        }

        public void CancelInputRequest()
        {
            if (IsListening())
            {
                _activeUIInstance.Cancel();
            }
        }

        public bool IsListening() => _activeUIInstance != null;

        private void OnRequestFinished()
        {
            if (_activeUIInstance != null)
            {
                Destroy(_activeUIInstance.gameObject);
                _activeUIInstance = null;
            }
        }
    }

    /// <summary>
    /// A MonoBehaviour that should be on the _inputFieldPrefab.
    /// It manages the UI and handles the input submission.
    /// </summary>
    public class KeyboardInputUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputField;
        
        private UnityAction<string> _onResultCallback;
        private UnityAction _onFinishedCallback;

        public void Activate(UnityAction<string> onResult, UnityAction onFinished)
        {
            _onResultCallback = onResult;
            _onFinishedCallback = onFinished;
            _inputField.onSubmit.AddListener(Submit);
            _inputField.Select();
            _inputField.ActivateInputField();
        }

        private void Submit(string text)
        {
            _onResultCallback?.Invoke(text);
            _onFinishedCallback?.Invoke();
        }

        public void Cancel()
        {
            _onFinishedCallback?.Invoke();
        }
    }
}
