using UnityEngine.Events;

namespace InceptorEngine.Input
{
    /// <summary>
    /// A static class to hold global events related to the input system.
    /// This allows different parts of the system to communicate without direct references.
    /// </summary>
    public static class InputEvents
    {
        /// <summary>
        /// Fired when a UI choice button is clicked. The string payload is the text of the button.
        /// The PointAndClickProvider listens for this event.
        /// </summary>
        public static UnityAction<string> OnChoiceButtonClicked;
    }
}
