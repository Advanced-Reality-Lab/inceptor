using System.Collections;
using InceptorEngine.UI.Interfaces;
using UnityEngine;

namespace InceptorEngine.UI.Core
{
    /// <summary>
    /// A helpful abstract base class for creating simple Inceptor UI components.
    /// It provides default, non-animated implementations for the IInceptorUI interface,
    /// making it easy for new UIs to be created without complex coroutine logic.
    /// </summary>
    public abstract class BaseInceptorUI : MonoBehaviour, IInceptorUI
    {
        /// <summary>
        /// The default implementation for showing the UI. It simply activates the GameObject.
        /// This can be overridden in child classes to add custom animations.
        /// </summary>
        public virtual IEnumerator Show()
        {
            gameObject.SetActive(true);
            yield break; // Coroutine finishes in the same frame.
        }

        /// <summary>
        /// The default implementation for hiding the UI. It simply deactivates the GameObject.
        /// This can be overridden in child classes to add custom animations.
        /// </summary>
        public virtual IEnumerator Hide()
        {
            gameObject.SetActive(false);
            yield break; // Coroutine finishes in the same frame.
        }

        /// <summary>
        /// The default implementation for the loaded check. It always returns true.
        /// This can be overridden if a UI needs to perform asynchronous loading (e.g., downloading images).
        /// </summary>
        public virtual bool IsLoaded()
        {
            return true;
        }

        /// <summary>
        /// The default implementation for the animating check. It always returns false.
        /// This should be overridden in child classes that implement custom Show/Hide animations.
        /// </summary>
        public virtual bool IsUIAnimating()
        {
            return false;
        }
    }
}
