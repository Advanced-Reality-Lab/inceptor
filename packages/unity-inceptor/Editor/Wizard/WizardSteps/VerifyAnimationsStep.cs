using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using InceptorEngine.Clips;

namespace InceptorEngine.Editor.Wizards
{
    /// <summary>
    /// Handles the third step: verifying and allowing overrides for all found animations.
    /// </summary>
    public class VerifyAnimationsStep : IWizardStep
    {
        public string Instructions => "Verify the animations found in the project. You can assign different clips here.";
        public string NextButtonText => "Confirm Animations";

        /// <summary>
        /// Draws lists of all found animations, allowing the user to change them if needed.
        /// </summary>
        public void OnGUI(WizardContext context, InceptorWizard wizard)
        {
            wizard.scrollPos = EditorGUILayout.BeginScrollView(wizard.scrollPos);

            DrawAnimationList("Body Behaviors", context.bodyAnimations);
            DrawAnimationList("Mood Animations", context.moodAnimations);
            DrawAnimationList("Reaction Animations", context.reactionAnimations);

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// This step only involves GUI verification, so no logic is needed on "Next".
        /// </summary>
        public bool OnNext(WizardContext context) { return true; }

        /// <summary>
        /// Helper method to draw a dictionary of animations.
        /// </summary>
        private void DrawAnimationList(string title, Dictionary<string, AnimationClip> animationDict)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            if (animationDict.Count == 0)
            {
                EditorGUILayout.LabelField("No animations of this type found in the script.");
            }
            else
            {
                // Using ToList() creates a copy, allowing us to modify the dictionary while iterating.
                foreach (string animName in animationDict.Keys.ToList())
                {
                    animationDict[animName] = (AnimationClip)EditorGUILayout.ObjectField(animName, animationDict[animName], typeof(AnimationClip), false);
                    GUILayout.Space(5);
                }
            }
            EditorGUI.indentLevel--;
            GUILayout.Space(15);
        }
    }
}