using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace InceptorEngine.Editor.Wizards
{
    /// <summary>
    /// Generates an Animator Controller using a "mesh" or "fully-connected" approach,
    /// where every animation state has a direct transition to every other state.
    /// This is the original, fallback implementation.
    /// </summary>
    public class MeshAnimatorGenerator : IAnimatorGenerator
    {
        /// <summary>
        /// Creates and configures an Animator Controller asset for a specific character.
        /// This is the main entry point for the generator.
        /// </summary>
        /// <param name="context">The shared data context of the wizard process.</param>
        /// <param name="character">The specific character for whom to create the animator.</param>
        /// <returns>A fully configured AnimatorController asset.</returns>
        public AnimatorController CreateAnimatorController(WizardContext context, CinematicScriptCharacterInfo character)
        {
            string path = $"Assets/Resources/Animators/{context.scriptName}_{character.name}_Mesh.controller";
            
            if (!Directory.Exists("Assets/Resources/Animators"))
            {
                Directory.CreateDirectory("Assets/Resources/Animators");
            }

            if (File.Exists(path))
            {
                File.Delete(path);
                AssetDatabase.Refresh();
            }

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            CreateBodyLayer(controller, context, character);
            CreateFacialLayer(controller, context, character, "MoodLayer", context.moodAnimations, AnimatorLayerBlendingMode.Additive, 1.0f);
            CreateFacialLayer(controller, context, character, "ReactionLayer", context.reactionAnimations, AnimatorLayerBlendingMode.Additive, 0.5f, true);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return controller;
        }

        /// <summary>
        /// Creates the base layer for body animations.
        /// </summary>
        private void CreateBodyLayer(AnimatorController controller, WizardContext context, CinematicScriptCharacterInfo character)
        {
            var rootStateMachine = controller.layers[0].stateMachine;
            rootStateMachine.name = "Base"; // Give the base state machine a name for clarity
            
            var idleState = rootStateMachine.AddState("Base_Idle");
            
            var states = new List<AnimatorState> { idleState };
        var uniqueBodyBehaviors = context.cinematicScript.clipList
                .SelectMany(clip => clip.Characters) 
                .Where(data => data.name == character.name && !string.IsNullOrEmpty(data.bodyBehavior))
                .Select(data => data.bodyBehavior)
                .Distinct()
                .ToList();

            foreach (string behaviorName in uniqueBodyBehaviors)
            {
                if (behaviorName.Equals("Idle", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var newState = rootStateMachine.AddState(behaviorName);
                if (context.bodyAnimations.TryGetValue(behaviorName, out var animClip))
                {
                    newState.motion = animClip;
                }
                states.Add(newState);
            }

            foreach (var fromState in states)
            {
                foreach (var toState in states)
                {
                    var transition = fromState.AddTransition(toState);
                    transition.hasExitTime = true;

                    if (fromState != toState)
                    {
                        string fromKey = fromState == idleState ? "Idle" : fromState.name;
                        string toKey = toState == idleState ? "Idle" : toState.name;
                        string triggerName = $"{fromKey}To{toKey}";

                        controller.AddParameter(triggerName, AnimatorControllerParameterType.Trigger);
                        transition.AddCondition(AnimatorConditionMode.If, 0, triggerName);
                        transition.hasExitTime = false;
                        transition.duration = context.animationTransitionSpeed;
                    }
                    else
                    {
                        transition.duration = 0f;
                        transition.exitTime = 1f;
                    }
                }
            }
        }

        /// <summary>
        /// Creates an additive facial animation layer (for moods or reactions).
        /// </summary>
        private void CreateFacialLayer(AnimatorController controller, WizardContext context, CinematicScriptCharacterInfo character, string layerName, Dictionary<string, AnimationClip> animationDict, AnimatorLayerBlendingMode blendMode, float weight, bool isReaction = false)
        {
            // --- FIX: Create all objects in memory before saving them to the asset database. ---

            // 1. Create all objects in memory first.
            var stateMachine = new AnimatorStateMachine();
            stateMachine.name = $"{layerName}_StateMachine"; // Give the state machine a unique name

            var newLayer = new AnimatorControllerLayer
            {
                name = layerName,
                stateMachine = stateMachine,
                avatarMask = Resources.Load<AvatarMask>("Prefabs/FaceMask"),
                blendingMode = blendMode,
                defaultWeight = weight
            };

            var idleState = stateMachine.AddState($"{layerName}_Idle");
            var states = new List<AnimatorState> { idleState };
            
            if (animationDict != null)
            {
                foreach (var animKvp in animationDict)
                {
                    if (animKvp.Key.Equals("Idle", System.StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var newState = stateMachine.AddState($"{layerName}_{animKvp.Key}");
                    newState.motion = animKvp.Value;
                    states.Add(newState);
                }
            }
            
            stateMachine.AddEntryTransition(idleState);

            // 2. Now that all objects are created, save them to the asset file.
            controller.AddLayer(newLayer); // This saves the layer object itself.
            AssetDatabase.AddObjectToAsset(stateMachine, controller);
            foreach (var state in states)
            {
                AssetDatabase.AddObjectToAsset(state, controller);
            }

            // 3. Finally, create the transitions between the now-saved states.
            if (isReaction)
            {
                foreach (var animState in states)
                {
                    if (animState == idleState) continue;
                    
                    string triggerName = animState.name.Substring(layerName.Length + 1);
                    controller.AddParameter(triggerName, AnimatorControllerParameterType.Trigger);

                    var toReaction = idleState.AddTransition(animState);
                    toReaction.AddCondition(AnimatorConditionMode.If, 0, triggerName);
                    toReaction.hasExitTime = false;
                    toReaction.duration = context.animationTransitionSpeed;

                    var fromReaction = animState.AddTransition(idleState);
                    fromReaction.hasExitTime = true;
                    fromReaction.exitTime = 1f;
                    fromReaction.duration = context.animationTransitionSpeed;
                }
            }
            else
            {
                foreach (var fromState in states)
                {
                    foreach (var toState in states)
                    {
                         var transition = fromState.AddTransition(toState);
                         transition.hasExitTime = true;
                         if (fromState != toState)
                         {
                            string fromKey = fromState == idleState ? "Idle" : fromState.name.Substring(layerName.Length + 1);
                            string toKey = toState == idleState ? "Idle" : toState.name.Substring(layerName.Length + 1);
                            string triggerName = $"{layerName}_{fromKey}To{toKey}";

                            controller.AddParameter(triggerName, AnimatorControllerParameterType.Trigger);
                            transition.AddCondition(AnimatorConditionMode.If, 0, triggerName);
                            transition.hasExitTime = false;
                            transition.duration = context.animationTransitionSpeed;
                         }
                    }
                }
            }
        }
    }
}
