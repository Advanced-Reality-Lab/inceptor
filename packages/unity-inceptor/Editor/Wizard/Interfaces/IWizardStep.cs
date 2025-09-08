namespace InceptorEngine.Editor.Wizards
{
    /// <summary>
    /// Defines the contract for a single, self-contained step in the Inceptor Wizard.
    /// </summary>
    public interface IWizardStep
    {
        /// <summary>
        /// Gets the instructional text to display at the top of the wizard for this step.
        /// </summary>
        string Instructions { get; }

        /// <summary>
        /// Gets the text to display on the primary action button (e.g., "Next" or "Finish").
        /// </summary>
        string NextButtonText { get; }

        /// <summary>
        /// Draws the specific GUI controls for this wizard step.
        /// </summary>
        /// <param name="context">The shared data context for the entire wizard process.</param>
        /// <param name="wizard">A reference to the main wizard window, for accessing GUI state like scroll position.</param>
        void OnGUI(WizardContext context, InceptorWizard wizard);

        /// <summary>
        /// Executes the primary logic for this step when the user clicks the next button.
        /// This typically involves processing data and preparing the context for the next step.
        /// </summary>
        /// <param name="context">The shared data context for the entire wizard process.</param>
        bool OnNext(WizardContext context);
    }
}
