using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace InceptorEngine.Editor.Wizards
{

   /// <summary>
    /// Handles the first step of the wizard: selecting the cinematic JSON file.
    /// </summary>
    public class ImportFileStep : IWizardStep
    {
        public string Instructions => "Start by importing the cinematic JSON script.";
        public string NextButtonText => "Import Script...";

        /// <summary>
        /// This step has no unique GUI elements; the action is in the OnNext method.
        /// </summary>
        public void OnGUI(WizardContext context, InceptorWizard wizard) { }

        /// <summary>
        /// Opens a file panel for the user to select the JSON file, then parses it
        /// and populates the initial data in the context.
        /// </summary>
        /// <returns>True if a file was selected and loaded successfully, otherwise false.</returns>
        public bool OnNext(WizardContext context)
        {
            string jsonPath = EditorUtility.OpenFilePanel("Select Scene Script", "Assets/Resources/", "json");
            if (string.IsNullOrEmpty(jsonPath))
            {
                // User cancelled the file dialog. Return false to prevent advancing.
                return false;
            }

            context.scriptName = Path.GetFileNameWithoutExtension(jsonPath);
            context.cinematicScript = CinematicScript.BuildScript(jsonPath);

            if (context.cinematicScript != null && context.cinematicScript.characters != null)
            {
                int characterCount = context.cinematicScript.characters.Count;
                context.characterPrefabs = new GameObject[characterCount];
            }
            
            // Return true to signal that the wizard can proceed to the next step.
            return true;
        }
    }
}