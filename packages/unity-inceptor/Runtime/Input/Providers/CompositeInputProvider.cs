using System.Collections.Generic;
using InceptorEngine.Input.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace InceptorEngine.Input.Providers
{
    /// <summary>
    /// A special input provider that manages a list of other providers.
    /// When input is requested, it activates all child providers simultaneously.
    /// The first one to return a result "wins," and all others are cancelled.
    /// </summary>
    [CreateAssetMenu(fileName = "CompositeInputProvider", menuName = "Inceptor/Input/Composite Provider")]
    public class CompositeInputProvider : ScriptableObject, IInceptorInput
    {
        [Tooltip("A list of other IInceptorInput providers (as ScriptableObjects) to run in parallel.")]
        [SerializeField]
        private List<ScriptableObject> _providers;

        private List<IInceptorInput> _activeProviders = new List<IInceptorInput>();
        private UnityAction<string> _onResultCallback;
        private bool _isListening;

        public void RequestInput(UnityAction<string> onResultReceived)
        {
            if (_isListening) return;

            _onResultCallback = onResultReceived;
            _isListening = true;
            _activeProviders.Clear();

            foreach (var providerSO in _providers)
            {
                if (providerSO is IInceptorInput provider)
                {
                    _activeProviders.Add(provider);
                    // Each child provider gets a callback that routes through our central handler.
                    provider.RequestInput(OnChildProviderResult);
                }
            }
        }

        public void CancelInputRequest()
        {
            if (!_isListening) return;

            // Cancel all child providers that were activated.
            foreach (var provider in _activeProviders)
            {
                provider.CancelInputRequest();
            }
            
            _activeProviders.Clear();
            _onResultCallback = null;
            _isListening = false;
        }

        public bool IsListening() => _isListening;

        /// <summary>
        /// The callback that all child providers will invoke.
        /// </summary>
        /// <param name="result">The string result from the winning provider.</param>
        private void OnChildProviderResult(string result)
        {
            // If we are no longer listening, it means another provider already won. Ignore this result.
            if (!_isListening) return;
            
            // Invoke the original callback to notify the clip.
            _onResultCallback?.Invoke(result);

            // The race is over. Cancel all other providers.
            CancelInputRequest();
        }
    }
}
