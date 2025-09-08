using System.Collections.Generic;
using System.Linq;
using InceptorEngine.Analysis;
using InceptorEngine.Analysis.Interfaces;
using InceptorEngine.Clips;
using InceptorEngine.Clips.Interfaces;
using UnityEngine;

namespace InceptorEngine.Editor.Validation
{
    /// <summary>
    /// A static utility class that provides methods for validating an Inceptor CinematicScript
    /// to find common configuration errors before runtime.
    /// </summary>
    public static class CinematicScriptValidator
    {
        /// <summary>
        /// The main entry point for the validation process. It runs a comprehensive suite of checks.
        /// </summary>
        /// <param name="script">The CinematicScript asset to validate.</param>
        /// <param name="inceptorInstance">The Inceptor instance that will run this script.</param>
        /// <returns>A list of all found validation errors and warnings.</returns>
        public static List<ValidationResult> Validate(CinematicScript script, Inceptor inceptorInstance)
        {
            var results = new List<ValidationResult>();

            if (script == null || inceptorInstance == null)
            {
                results.Add(new ValidationResult(ValidationSeverity.Error, "Script or Inceptor instance is null."));
                return results;
            }

            // --- Run all individual validation checks ---
            results.AddRange(ValidateClipList(script));
            results.AddRange(ValidateBranching(script));
            results.AddRange(ValidateCharacterAssignments(script, inceptorInstance));
            results.AddRange(ValidateAnalyzerCompatibility(script, inceptorInstance));
            // Add calls to new validation methods here in the future.

            return results;
        }

        /// <summary>
        /// Checks for null entries in the main clip list.
        /// </summary>
        private static IEnumerable<ValidationResult> ValidateClipList(CinematicScript script)
        {
            for (int i = 0; i < script.clipList.Count; i++)
            {
                if (script.clipList[i] == null)
                {
                    yield return new ValidationResult(
                        ValidationSeverity.Error,
                        $"Clip at index {i} is null or missing.",
                        script);
                }
            }
        }

        /// <summary>
        /// Checks for invalid nextClip indices and finds unreachable "orphan" clips.
        /// </summary>
        private static IEnumerable<ValidationResult> ValidateBranching(CinematicScript script)
        {
            var reachableClips = new HashSet<int>();
            var clipsToVisit = new Queue<int>();
            
            // Start the search from the first clip.
            if (script.clipList.Count > 0)
            {
                clipsToVisit.Enqueue(0);
                reachableClips.Add(0);
            }

            while (clipsToVisit.Count > 0)
            {
                int currentIndex = clipsToVisit.Dequeue();
                var currentClip = script.clipList[currentIndex];
                if (currentClip == null) continue;

                // Check all possible next clips from the current clip.
                var nextClipIndices = new List<int>();
                if (currentClip is LinearClip linearClip)
                {
                    nextClipIndices.Add(linearClip.NextClipIndex);
                }
                else if (currentClip is ChoiceClip choiceClip)
                {
                    nextClipIndices.AddRange(choiceClip.NextClips);
                }

                foreach (var nextIndex in nextClipIndices)
                {
                    if (nextIndex >= script.clipList.Count)
                    {
                        yield return new ValidationResult(
                            ValidationSeverity.Error,
                            $"Clip '{currentClip.name}' (Index {currentIndex}) has an out-of-bounds nextClip index: {nextIndex}.",
                            currentClip);
                    }
                    else if (nextIndex >= 0 && !reachableClips.Contains(nextIndex))
                    {
                        reachableClips.Add(nextIndex);
                        clipsToVisit.Enqueue(nextIndex);
                    }
                }
            }

            // Check for any clips that were not reached.
            for (int i = 0; i < script.clipList.Count; i++)
            {
                if (!reachableClips.Contains(i))
                {
                    yield return new ValidationResult(
                        ValidationSeverity.Warning,
                        $"Orphan Clip: Clip '{script.clipList[i].name}' (Index {i}) is unreachable from the start.",
                        script.clipList[i]);
                }
            }
        }

        /// <summary>
        /// Checks that every character mentioned in the script has a corresponding prefab assigned in the Inceptor.
        /// </summary>
        private static IEnumerable<ValidationResult> ValidateCharacterAssignments(CinematicScript script, Inceptor inceptorInstance)
        {
            var assignedCharacters = inceptorInstance.CharacterList;
            var assignedNames = new HashSet<string>(assignedCharacters.Select(c => c.name));

            foreach (var characterInfo in script.characters)
            {
                if (!assignedNames.Contains(characterInfo.name))
                {
                    yield return new ValidationResult(
                        ValidationSeverity.Error,
                        $"Character '{characterInfo.name}' is defined in the script but has no prefab assigned in the Inceptor's Character List.",
                        inceptorInstance);
                }
            }
        }
        
        /// <summary>
        /// The core compatibility check. Verifies that the analyzer chosen for each clip
        /// can handle the type of AnalysisContext that the clip provides.
        /// </summary>
        private static IEnumerable<ValidationResult> ValidateAnalyzerCompatibility(CinematicScript script, Inceptor inceptorInstance)
        {
            var defaultAnalyzer = inceptorInstance.Settings.NextClipAnalyzer;

            for (int i = 0; i < script.clipList.Count; i++)
            {
                var clip = script.clipList[i];
                if (clip == null) continue;

                // Determine which analyzer will be used for this clip.
                var analyzerToUse = defaultAnalyzer;
                if (clip.OverrideAnalyzer != null)
                {
                    analyzerToUse = clip.OverrideAnalyzer;
                }

                if (analyzerToUse == null)
                {
                    yield return new ValidationResult(
                        ValidationSeverity.Error,
                        $"Clip '{clip.name}' (Index {i}) requires an analyzer, but none is assigned (neither an override nor a default in Inceptor Settings).",
                        clip);
                    continue;
                }

                // --- This is the key logic ---
                // We need to create a dummy context to check compatibility.
                // A more advanced system might use reflection to check types without instantiation.
                var context = clip.CreateAnalysisContextForValidation();

                if (!analyzerToUse.CanHandleContext(context))
                {
                    var analyzerObject = analyzerToUse as UnityEngine.Object;
                    string analyzerName = "Unknown (Non-Asset Analyzer)";
                    if (analyzerObject != null)
                        {
                            // This will work for ANY analyzer asset, because they are all ScriptableObjects.
                            analyzerName = analyzerObject.name;
                            // ... use the name in the error message ...
                        }

                    
                    yield return new ValidationResult(
                        ValidationSeverity.Error,
                        $"Compatibility Error on Clip '{clip.name}' (Index {i}): The assigned analyzer '{analyzerName}' cannot handle the context type '{context.GetType().Name}' provided by this clip.",
                        clip);
                }
            }
        }
    }
}
