using UnityEditor.Animations;

namespace InceptorEngine.Editor.Wizards
{
    /// <summary>
    /// Defines a contract for a class that can generate and configure
    /// an Animator Controller for an Inceptor character.
    /// </summary>
    public interface IAnimatorGenerator
    {
        /// <summary>
        /// Creates and configures an Animator Controller asset for a specific character
        /// based on the data collected in the wizard.
        /// </summary>
        /// <param name="context">The shared data context of the wizard process.</param>
        /// <param name="character">The specific character for whom to create the animator.</param>
        /// <returns>A fully configured AnimatorController asset.</returns>
        AnimatorController CreateAnimatorController(WizardContext context, CinematicScriptCharacterInfo character);
    }
}
