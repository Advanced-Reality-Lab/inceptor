using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using InceptorEngine.Utils;

namespace InceptorEngine.Editor.Wizards
{
    /// <summary>
    /// A build task responsible for instantiating characters, creating their animators,
    /// and adding necessary components.
    /// </summary>
    public class BuildCharactersTask : ISceneBuildTask
    {
        public void Execute(WizardContext context, IAnimatorGenerator animatorGenerator, Inceptor inceptorInstance)
        {
            for (int i = 0; i < context.cinematicScript.characters.Count; i++)
            {
                CinematicScriptCharacterInfo characterInfo = context.cinematicScript.characters[i];
                GameObject characterPrefab = context.characterPrefabs[i];

                if (characterPrefab == null)
                {
                    Debug.LogWarning($"No prefab assigned for character '{characterInfo.name}'. Skipping.");
                    continue;
                }

                // 1. Instantiate the character from its prefab.
                GameObject characterGo = (GameObject)PrefabUtility.InstantiatePrefab(characterPrefab);
                characterGo.name = characterInfo.name;
                // Optional: Parent the character to the Inceptor object for organization.
                // characterGo.transform.SetParent(inceptorInstance.transform);

                // 2. Add the core controller script.
                characterGo.AddComponent<InceptorCharacterController>();

                // 3. Create and assign the Animator Controller using the selected generator.
                var ac = animatorGenerator.CreateAnimatorController(context, characterInfo);
                var animator = characterGo.GetComponent<Animator>() ?? characterGo.AddComponent<Animator>();
                animator.runtimeAnimatorController = ac;
                
                // 4. Add other necessary components.
                if (characterGo.GetComponent<AudioSource>() == null)
                {
                    characterGo.AddComponent<AudioSource>();
                }
                
                // 5. Register the new character with the main Inceptor instance.
                inceptorInstance.RegisterCharacter(characterGo.GetComponent<InceptorCharacterController>());
            }
        }
    }
}
