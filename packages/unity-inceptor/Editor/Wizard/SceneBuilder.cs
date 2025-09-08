using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

namespace InceptorEngine.Editor.Wizards
{
    /// <summary>
    /// A service class responsible for constructing the final Inceptor scene
    /// by executing a pipeline of build tasks.
    /// </summary>
    public static class SceneBuilder
    {
        /// <summary>
        /// The main entry point for building the scene.
        /// </summary>
        /// <param name="context">The data collected from the wizard.</param>
        /// <param name="animatorGenerator">The chosen animator generation strategy.</param>
        public static void Build(WizardContext context, IAnimatorGenerator animatorGenerator)
        {
            // --- Phase 1: Pre-build validation and setup ---
            if (!ValidateScene()) return;
            
            // FIX: The SaveCinematicScriptAsAsset method has been removed, as the BuildScript
            // function now correctly handles creating the asset on disk.
            
            // Create the main Inceptor instance that all tasks will modify.
            GameObject inceptorGo = CreateInceptorObject(context);
            Inceptor inceptorInstance = inceptorGo.GetComponent<Inceptor>();
            
            // --- Phase 2: Execute the build pipeline ---
            var buildPipeline = new List<ISceneBuildTask>
            {
                new BuildCharactersTask(),
            };

            foreach (var task in buildPipeline)
            {
                task.Execute(context, animatorGenerator, inceptorInstance);
            }
            
            // --- Phase 3: Finalize and save ---
            EditorUtility.SetDirty(inceptorInstance);
            PrefabUtility.ApplyPrefabInstance(inceptorGo, InteractionMode.UserAction);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Inceptor scene built successfully!");
        }

        // FIX: This entire method is no longer needed and has been removed.
        // private static void SaveCinematicScriptAsAsset(WizardContext context) { ... }

        /// <summary>
        /// Validates that the current scene is saved and can be used.
        /// </summary>
        /// <returns>True if the scene is valid, otherwise false.</returns>
        private static bool ValidateScene()
        {
            if (string.IsNullOrEmpty(SceneManager.GetActiveScene().name))
            {
                EditorUtility.DisplayDialog("Scene Not Saved", "Please save the current scene before running the wizard.", "OK");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Creates the root Inceptor GameObject and its prefab variant.
        /// </summary>
        private static GameObject CreateInceptorObject(WizardContext context)
        {
            // Clean up any old instance.
            var existingInceptor = Object.FindFirstObjectByType<Inceptor>();
            if(existingInceptor != null)
            {
                Object.DestroyImmediate(existingInceptor.gameObject);
            }

            // Create a new instance from scratch.
            GameObject go = new GameObject(context.scriptName);
            var inceptorComponent = go.AddComponent<Inceptor>();
            inceptorComponent.Initialize(context.settings, context.cinematicScript);

            
            string resourcesPath = "Assets/Prefabs/Inceptor";
            if (!Directory.Exists(resourcesPath)) Directory.CreateDirectory(resourcesPath);
            string variantPath = $"{resourcesPath}/{SceneManager.GetActiveScene().name}_{context.scriptName}.prefab";
            
            GameObject prefabVariant = PrefabUtility.SaveAsPrefabAsset(go, variantPath);
            Object.DestroyImmediate(go); // Destroy the temporary object

            return (GameObject)PrefabUtility.InstantiatePrefab(prefabVariant);
        }
    }
}
