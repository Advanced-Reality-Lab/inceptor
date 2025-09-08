namespace InceptorEngine.Editor.Wizards
{
    /// <summary>
    /// Defines a contract for a single, atomic task within the scene construction process.
    /// </summary>
    public interface ISceneBuildTask
    {
        /// <summary>
        /// Executes this specific task of the scene building process.
        /// </summary>
        /// <param name="context">The shared data context of the wizard process.</param>
        /// <param name="animatorGenerator">The selected animator generator to use.</param>
        /// <param name="inceptorInstance">The root Inceptor GameObject instance being configured.</param>
        void Execute(WizardContext context, IAnimatorGenerator animatorGenerator, Inceptor inceptorInstance);
    }
}
