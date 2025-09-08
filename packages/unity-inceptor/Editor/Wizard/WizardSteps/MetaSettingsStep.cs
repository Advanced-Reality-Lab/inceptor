
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace InceptorEngine.Editor.Wizards
{
    /// <summary>
    /// Handles the final step: configuring meta-settings for the Inceptor instance.
    /// </summary>
    public class MetaSettingsStep : IWizardStep
    {
        // A static style for our warning label, initialized once.
        private static GUIStyle _warningLabelStyle;

        public string Instructions => "Configure the final settings for the Inceptor scene.";
        public string NextButtonText => "Finish and Build Scene";

        /// <summary>
        /// Draws fields for various meta-settings.
        /// </summary>
        public void OnGUI(WizardContext context, InceptorWizard wizard)
        {

            // Initialize the style if it hasn't been already.
            if (_warningLabelStyle == null)
            {
                _warningLabelStyle = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = new Color(0.8f, 0.2f, 0.2f) }, // A reddish color
                    fontStyle = FontStyle.Bold,
                    wordWrap = true
                };
            }

            // --- FIX: Check for existing prefab and display a warning ---
            string resourcesPath = "Assets/Prefabs/Inceptor";
            string variantPath = $"{resourcesPath}/{SceneManager.GetActiveScene().name}_{context.scriptName}.prefab";
            if (File.Exists(variantPath))
            {
                EditorGUILayout.LabelField(
                    "Warning: Proceeding will overwrite an existing Inceptor prefab with the same name.", 
                    _warningLabelStyle
                );
                EditorGUILayout.Space();
            }

            wizard.scrollPos = EditorGUILayout.BeginScrollView(wizard.scrollPos);

            context.settings = (InceptorSettings)EditorGUILayout.ObjectField("Settings File", context.settings, typeof(InceptorSettings), false);
            context.questionStartDelay = EditorGUILayout.FloatField("Delay Before Question Clip", context.questionStartDelay);
            context.overwriteAudio = EditorGUILayout.Toggle("Overwrite Existing Audio", context.overwriteAudio);
            context.animationTransitionSpeed = EditorGUILayout.FloatField("Animation Transition (s)", context.animationTransitionSpeed);

            EditorGUILayout.LabelField("Generator Options", EditorStyles.boldLabel);
            context.selectedStrategy = (AnimatorGenerationStrategy)EditorGUILayout.EnumPopup("Animator Generator", context.selectedStrategy);


            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// This is the final step. The main wizard will handle the scene generation call.
        /// </summary>
        public bool OnNext(WizardContext context) { return true; }
    }
}