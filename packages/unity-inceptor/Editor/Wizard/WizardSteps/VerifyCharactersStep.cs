using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using InceptorEngine.Clips;
using InceptorEngine.Clips.Interfaces;


namespace InceptorEngine.Editor.Wizards
{
/// <summary>
    /// Handles the second step: assigning GameObjects to the characters defined in the script.
    /// </summary>
    public class VerifyCharactersStep : IWizardStep
    {
        public string Instructions => "Assign a character prefab to each character from the script.";
        public string NextButtonText => "Confirm Characters";

        /// <summary>
        /// Draws a foldout for each character, allowing the user to drag-and-drop a prefab.
        /// </summary>
        public void OnGUI(WizardContext context, InceptorWizard wizard)
        {

            // Defensive checks to prevent errors if data is missing.
            if (context.cinematicScript?.characters == null || wizard.characterFoldouts == null || context.characterPrefabs == null)
            {
                EditorGUILayout.HelpBox("Character data is not initialized. Please go back and re-import the script.", MessageType.Error);
                return;
            }

            wizard.scrollPos = EditorGUILayout.BeginScrollView(wizard.scrollPos);

            EditorGUILayout.LabelField("Characters Data:", EditorStyles.boldLabel);
            for (int i = 0; i < context.cinematicScript.characters.Count; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                CinematicScriptCharacterInfo currentCharacter = context.cinematicScript.characters[i];
                wizard.characterFoldouts[i] = EditorGUILayout.Foldout(wizard.characterFoldouts[i], currentCharacter.name, true, EditorStyles.foldoutHeader);

                if (wizard.characterFoldouts[i])
                {
                    EditorGUI.indentLevel++;
                    context.characterPrefabs[i] = (GameObject)EditorGUILayout.ObjectField("Character Prefab", context.characterPrefabs[i], typeof(GameObject), false);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Scans the cinematic script to find all unique animation names that will need to be assigned.
        /// </summary>
        public bool OnNext(WizardContext context)
        {
            // Initialize the dictionaries to hold the found animations.
            context.bodyAnimations = new Dictionary<string, AnimationClip>();
            context.moodAnimations = new Dictionary<string, AnimationClip>();
            context.reactionAnimations = new Dictionary<string, AnimationClip>();

            foreach (Clip clip in context.cinematicScript.clipList)
            {
                foreach (ClipCharacterData clipChar in clip.Characters)
                {
                    // Find and pre-load body animations
                    if (!string.IsNullOrEmpty(clipChar.bodyBehavior) && !context.bodyAnimations.ContainsKey(clipChar.bodyBehavior))
                    {
                        context.bodyAnimations.Add(clipChar.bodyBehavior, AssetFinder.FindBodyAnimationClip(clipChar.bodyBehavior));
                    }

                    // Find and pre-load mood animations
                    if (!string.IsNullOrEmpty(clipChar.mood) && !context.moodAnimations.ContainsKey(clipChar.mood))
                    {
                        context.moodAnimations.Add(clipChar.mood, AssetFinder.FindFaceAnimationClip(clipChar.mood));
                    }
                    
                    // Find and pre-load reaction animations
                    if (!string.IsNullOrEmpty(clipChar.reaction) && !context.reactionAnimations.ContainsKey(clipChar.reaction))
                    {
                        context.reactionAnimations.Add(clipChar.reaction, AssetFinder.FindFaceAnimationClip(clipChar.reaction));
                    }
                }
            }
            return true;
        }
    }
}