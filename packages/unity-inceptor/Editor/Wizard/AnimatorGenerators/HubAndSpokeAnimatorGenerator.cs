using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace InceptorEngine.Editor.Wizards
{
    /// <summary>
    /// Generates an Animator Controller using a more advanced "hub-and-spoke" model.
    /// This approach is designed to be both easier to control via script and more visually fluid.
    /// It uses integer parameters to control states and "Any State" transitions for immediate reactions.
    /// </summary>
    public class HubAndSpokeAnimatorGenerator : IAnimatorGenerator
    {
        // Constants for the animator parameters. Using constants avoids "magic strings".
        private const string BODY_STATE_PARAM = "BodyState";
        private const string MOOD_STATE_PARAM = "MoodState";
        
        /// <summary>
        /// Creates and configures an Animator Controller asset for a specific character.
        /// This is the main entry point for the generator.
        /// </summary>
        /// <param name="context">The shared data context of the wizard process.</param>
        /// <param name="character">The specific character for whom to create the animator.</param>
        /// <returns>A fully configured AnimatorController asset.</returns>
        public AnimatorController CreateAnimatorController(WizardContext context, CinematicScriptCharacterInfo character)
        {
            string path = $"Assets/Resources/Animators/{context.scriptName}_{character.name}_HubAndSpoke.controller";

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

            // Create each layer using the robust, multi-pass approach.
            CreateBaseLayer(controller, context, character);
            CreateMoodLayer(controller, context, character);
            CreateReactionLayer(controller, context, character);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return controller;
        }

        /// <summary>
        /// Creates the base layer for body animations using a hub-and-spoke model.
        /// </summary>
        private void CreateBaseLayer(AnimatorController controller, WizardContext context, CinematicScriptCharacterInfo character)
        {
            controller.AddParameter(BODY_STATE_PARAM, AnimatorControllerParameterType.Int);
            var rootStateMachine = controller.layers[0].stateMachine;
            rootStateMachine.name = "Base_StateMachine";
            
            var idleState = rootStateMachine.AddState("Base_Idle");
            
            var uniqueBodyBehaviors = context.bodyAnimations.Keys.ToList();
            
            for (int i = 0; i < uniqueBodyBehaviors.Count; i++)
            {
                string behaviorName = uniqueBodyBehaviors[i];
                if (behaviorName.Equals("Idle", System.StringComparison.OrdinalIgnoreCase)) continue;

                // Prefix the state name to guarantee uniqueness.
                var animState = rootStateMachine.AddState($"Base_{behaviorName.Replace('.', '_')}");
                animState.motion = context.bodyAnimations[behaviorName];
                int stateIndex = i + 1; // 0 is reserved for Idle

                // Transition FROM Idle TO the new state
                var toStateTransition = idleState.AddTransition(animState);
                toStateTransition.AddCondition(AnimatorConditionMode.Equals, stateIndex, BODY_STATE_PARAM);
                toStateTransition.hasExitTime = false;
                toStateTransition.duration = context.animationTransitionSpeed;

                // Transition FROM the new state back TO Idle
                var toIdleTransition = animState.AddTransition(idleState);
                toIdleTransition.AddCondition(AnimatorConditionMode.Equals, 0, BODY_STATE_PARAM);
                toIdleTransition.hasExitTime = false;
                toIdleTransition.duration = context.animationTransitionSpeed;
                
                // Optional: Add a transition back to idle based on exit time.
                var exitTimeTransition = animState.AddTransition(idleState);
                exitTimeTransition.hasExitTime = true;
                exitTimeTransition.exitTime = 0.9f;
                exitTimeTransition.duration = context.animationTransitionSpeed;
            }
        }

        /// <summary>
        /// Creates an additive layer for sustained moods (e.g., smiling, frowning).
        /// </summary>
        private void CreateMoodLayer(AnimatorController controller, WizardContext context, CinematicScriptCharacterInfo character)
        {
            controller.AddParameter(MOOD_STATE_PARAM, AnimatorControllerParameterType.Int);

            // 1. Create all objects in memory.
            var stateMachine = new AnimatorStateMachine { name = "Mood_StateMachine" };
            var moodLayer = new AnimatorControllerLayer
            {
                name = "MoodLayer",
                stateMachine = stateMachine,
                avatarMask = Resources.Load<AvatarMask>("Prefabs/FaceMask"),
                blendingMode = AnimatorLayerBlendingMode.Additive,
                defaultWeight = 1.0f
            };
            var idleState = stateMachine.AddState("Mood_Idle");
            var moodStates = new List<AnimatorState> { idleState };
            var moodNames = context.moodAnimations?.Keys.ToList() ?? new List<string>();

            // DEBUG: Log the initial list of mood names from the context.
            Debug.Log($"--- Creating Mood Layer --- Found {moodNames.Count} mood names.");

            foreach (string moodName in moodNames)
            {
                if (moodName.Equals("Idle", System.StringComparison.OrdinalIgnoreCase)) continue;
                
                // Sanitize the name to remove characters Unity doesn't allow in state names.
                string sanitizedName = moodName.Replace('.', '_');
                string prefixedName = $"Mood_{sanitizedName}";
                
                // DEBUG: Log the names we are working with.
                Debug.Log($"Processing mood: '{moodName}'. Creating state with intended name: '{prefixedName}'");

                var moodState = stateMachine.AddState(prefixedName);
                moodState.motion = context.moodAnimations[moodName];
                moodStates.Add(moodState);

                // DEBUG: Log the actual name Unity assigned to the state.
                Debug.Log($"State created. Actual name assigned by Unity: '{moodState.name}'");
            }
            stateMachine.AddEntryTransition(idleState);

            // 2. Save all created objects to the asset database.
            controller.AddLayer(moodLayer);
            AssetDatabase.AddObjectToAsset(stateMachine, controller);
            foreach (var state in moodStates) AssetDatabase.AddObjectToAsset(state, controller);

            // DEBUG: Log the names of all states we have stored before creating transitions.
            Debug.Log("--- Stored Mood States for Transitioning ---");
            foreach(var state in moodStates) Debug.Log(state.name);
            Debug.Log("------------------------------------------");


            // 3. Create transitions between the now-persistent states.
            for (int i = 0; i < moodNames.Count; i++)
            {
                string moodName = moodNames[i];
                if (moodName.Equals("Idle", System.StringComparison.OrdinalIgnoreCase)) continue;
                
                string sanitizedName = moodName.Replace('.', '_');
                string searchName = $"Mood_{sanitizedName}";

                // DEBUG: Log the name we are now searching for.
                Debug.Log($"Creating transition for: '{moodName}'. Searching for state: '{searchName}'");

                var moodState = moodStates.First(s => s.name == searchName);
                int stateIndex = i + 1;

                var toMood = idleState.AddTransition(moodState);
                toMood.AddCondition(AnimatorConditionMode.Equals, stateIndex, MOOD_STATE_PARAM);
                toMood.hasExitTime = false;
                toMood.duration = context.animationTransitionSpeed;

                var toIdle = moodState.AddTransition(idleState);
                toIdle.AddCondition(AnimatorConditionMode.Equals, 0, MOOD_STATE_PARAM);
                toIdle.hasExitTime = false;
                toIdle.duration = context.animationTransitionSpeed;
            }
        }

        /// <summary>
        /// Creates an additive layer for one-shot reactions (e.g., wink, gasp).
        /// </summary>
        private void CreateReactionLayer(AnimatorController controller, WizardContext context, CinematicScriptCharacterInfo character)
        {
            // 1. Create all objects in memory.
            var stateMachine = new AnimatorStateMachine { name = "Reaction_StateMachine" };
            var reactionLayer = new AnimatorControllerLayer
            {
                name = "ReactionLayer",
                stateMachine = stateMachine,
                avatarMask = Resources.Load<AvatarMask>("Prefabs/FaceMask"),
                blendingMode = AnimatorLayerBlendingMode.Additive,
                defaultWeight = 1.0f
            };
            var idleState = stateMachine.AddState("Reaction_Idle");
            var reactionStates = new List<AnimatorState> { idleState };
            var reactionAnimations = context.reactionAnimations ?? new Dictionary<string, AnimationClip>();

            foreach (var reactionKvp in reactionAnimations)
            {
                if (reactionKvp.Key.Equals("Idle", System.StringComparison.OrdinalIgnoreCase)) continue;

                // Sanitize the name to remove characters Unity doesn't allow in state names.
                string sanitizedName = reactionKvp.Key.Replace('.', '_');
                var reactionState = stateMachine.AddState($"Reaction_{sanitizedName}");
                reactionState.motion = reactionKvp.Value;
                reactionStates.Add(reactionState);
            }
            stateMachine.AddEntryTransition(idleState);
            
            // 2. Save all created objects to the asset database.
            controller.AddLayer(reactionLayer);
            AssetDatabase.AddObjectToAsset(stateMachine, controller);
            foreach (var state in reactionStates) AssetDatabase.AddObjectToAsset(state, controller);

            // 3. Create transitions using the "Any State" node for interrupts.
            foreach (var reactionState in reactionStates)
            {
                if (reactionState == idleState) continue;

                // Derive the trigger name from the state's prefixed name.
                string triggerName = $"Play{reactionState.name.Substring("Reaction_".Length)}";
                controller.AddParameter(triggerName, AnimatorControllerParameterType.Trigger);

                var toReaction = stateMachine.AddAnyStateTransition(reactionState);
                toReaction.AddCondition(AnimatorConditionMode.If, 0, triggerName);
                toReaction.duration = 0.1f;
                toReaction.canTransitionToSelf = false;

                var fromReaction = reactionState.AddTransition(idleState);
                fromReaction.hasExitTime = true;
                fromReaction.exitTime = 1f;
                fromReaction.duration = 0.1f;
            }
        }
    }
}
