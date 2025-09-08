using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InceptorEngine.Editor.Wizards
{
    /// <summary>
    /// A step-by-step wizard to guide users through importing a cinematic script
    /// and generating a fully configured Inceptor scene.
    /// This class orchestrates the wizard flow, while delegating the actual work
    /// to various step, generator, and builder classes.
    /// </summary>
    public class InceptorWizard : EditorWindow
    {

        // --- Wizard State ---
        private int _currentStepIndex;
        private List<IWizardStep> _steps;
        private WizardContext _context;

        // --- GUI-specific State ---
        // This state is managed by the main window because it persists across steps.
        public Vector2 scrollPos;
        public bool[] characterFoldouts;

        [MenuItem("Inceptor/Import Cinematic Script", false, 12)]
        private static void ShowWindow()
        {
            GetWindow<InceptorWizard>(false, "Inceptor Wizard", true);
        }

        /// <summary>
        /// OnEnable is called when the EditorWindow is created or reloaded.
        /// This is where we initialize the wizard's state.
        /// </summary>
        private void OnEnable()
        {
            _context = new WizardContext();
            _steps = new List<IWizardStep>
            {
                new ImportFileStep(),
                new VerifyCharactersStep(),
                new VerifyAnimationsStep(),
                new MetaSettingsStep()
            };
        }

        /// <summary>
        /// OnGUI is called for rendering and handling GUI events.
        /// It delegates the drawing of step-specific controls to the current step object.
        /// </summary>
        private void OnGUI()
        {
            if (_steps == null || _currentStepIndex >= _steps.Count)
            {
                // Safety check in case state is lost.
                OnEnable(); 
            }

            IWizardStep currentStep = _steps[_currentStepIndex];

            // --- Header ---
            EditorGUILayout.LabelField($"Step {_currentStepIndex + 1} of {_steps.Count}: {currentStep.Instructions}", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // --- Step-specific GUI ---
            // The main wizard window simply tells the current step to draw its own GUI.
            currentStep.OnGUI(_context, this);

            EditorGUILayout.Space();

            // --- Footer and Navigation ---
            DrawFooter();
        }
        
        private void DrawFooter()
        {
            
            EditorGUILayout.BeginHorizontal();
            
            // Back button logic (optional but good for usability)
            if (_currentStepIndex > 0)
            {
                if (GUILayout.Button("Back"))
                {
                    _currentStepIndex--;
                }
            }
            
            // Spacer to push the next button to the right
            GUILayout.FlexibleSpace();

            // The main action button
            IWizardStep currentStep = _steps[_currentStepIndex];
            if (GUILayout.Button(currentStep.NextButtonText, GUILayout.MinWidth(120)))
            {
                HandleNextButton();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Handles the logic when the primary action button is clicked.
        /// </summary>
        private void HandleNextButton()
        {
            IWizardStep currentStep = _steps[_currentStepIndex];

            // Execute the current step's logic. It returns true on success.
            bool success = currentStep.OnNext(_context);

            if (!success)
            {
                // If the step failed (e.g., user cancelled file dialog), we don't advance.
                return;
            }

            // --- Post-Step Initialization ---
            // If the step we just completed was the import step, we need to initialize
            // the GUI state for the next step (VerifyCharacters).
            if (currentStep is ImportFileStep)
            {
                characterFoldouts = new bool[_context.cinematicScript.characters.Count];
                for (int i = 0; i < characterFoldouts.Length; i++) characterFoldouts[i] = true;
            }

            // --- Advance or Finish ---
            if (_currentStepIndex < _steps.Count - 1)
            {
                // Move to the next step
                _currentStepIndex++;
                scrollPos = Vector2.zero; // Reset scroll position for the new step
            }
            else
            {
                // We are on the last step, so we finish and build the scene.
                FinishAndBuild();
                Close(); // Close the wizard window
            }
        }

        /// <summary>
        /// Finalizes the process by selecting the appropriate generator and
        /// telling the SceneBuilder to construct the scene.
        /// </summary>
        private void FinishAndBuild()
        {
            IAnimatorGenerator selectedGenerator;

            // Select the animator generator based on the user's choice.
            switch (_context.selectedStrategy)
            {
                case AnimatorGenerationStrategy.HubAndSpoke:
                    selectedGenerator = new HubAndSpokeAnimatorGenerator();
                    break;
                case AnimatorGenerationStrategy.Mesh:
                default:
                    selectedGenerator = new MeshAnimatorGenerator();
                    break;
            }

            // Delegate the entire scene construction process to the SceneBuilder.
            SceneBuilder.Build(_context, selectedGenerator);
        }
    }
}
