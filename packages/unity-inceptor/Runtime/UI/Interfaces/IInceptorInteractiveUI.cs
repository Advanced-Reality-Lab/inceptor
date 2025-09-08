using System.Collections.Generic;
using InceptorEngine;

namespace InceptorEngine.UI.Interfaces
{
    /// <summary>
    /// The base contract for any UI that can present choices to the user.
    /// It inherits from IInceptorUI, ensuring it can be managed by the core system.
    /// </summary>
    public interface IInceptorInteractiveUI : IInceptorUI
    {
        /// <summary>
        /// Populates the UI with the possible choices for the user.
        /// </summary>
        /// <param name="choices">A read-only list of strings representing the choices.</param>
        void SetChoices(IReadOnlyList<string> choices);

        /// <summary>
        /// Displays a generic error message to the user, for example, if their input could not be understood.
        /// </summary>
        void ShowError(InceptorErrorType error);
    }
}
