using UnityEngine.Events;

namespace InceptorEngine.Input.Interfaces
{
    /// <summary>
    /// The contract for any system that provides user input to the Inceptor framework.
    /// </summary>
    public interface IInceptorInput
    {
        /// <summary>
        /// Begins an asynchronous request for user input.
        /// </summary>
        /// <param name="onResultReceived">The callback to invoke with the string result when input is received.</param>
        void RequestInput(UnityAction<string> onResultReceived);

        /// <summary>
        /// Cancels any pending input request.
        /// </summary>
        void CancelInputRequest();

        /// <summary>
        /// Checks if the provider is currently waiting for user input.
        /// </summary>
        /// <returns>True if the provider is active, otherwise false.</returns>
        bool IsListening();
    }
}
