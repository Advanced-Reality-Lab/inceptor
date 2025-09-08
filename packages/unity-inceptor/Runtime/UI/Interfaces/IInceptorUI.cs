using System.Collections;
using UnityEngine;

namespace InceptorEngine.UI.Interfaces
{
    /// <summary>
    /// The core contract for any UI component that can be managed by the Inceptor system.
    /// </summary>
    public interface IInceptorUI
    {
        /// <summary>
        /// Gets the GameObject that this UI component is attached to.
        /// This is necessary for the Inceptor to manage the UI's lifecycle (e.g., destroying it).
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// An asynchronous operation to show the UI. Can be used for fade-in animations.
        /// </summary>
        /// <returns>An IEnumerator for use in a coroutine.</returns>
        IEnumerator Show();

        /// <summary>
        /// An asynchronous operation to hide the UI. Can be used for fade-out animations.
        /// </summary>
        /// <returns>An IEnumerator for use in a coroutine.</returns>
        IEnumerator Hide();

        /// <summary>
        /// Checks if the UI has finished its initial loading and is ready to be shown.
        /// </summary>
        /// <returns>True if the UI is ready, otherwise false.</returns>
        bool IsLoaded();

        /// <summary>
        /// Checks if the UI is currently playing a Show or Hide animation.
        /// </summary>
        /// <returns>True if an animation is active, otherwise false.</returns>
        bool IsUIAnimating();
    }
}
