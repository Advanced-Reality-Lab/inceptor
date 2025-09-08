using UnityEngine;

namespace InceptorEngine.Editor.Wizards
{
    /// <summary>
    /// A static utility class for finding project assets required by the Inceptor wizard.
    /// </summary>
    public static class AssetFinder
    {
        /// <summary>
        /// Finds a body animation clip from the project's Resources folder.
        /// </summary>
        /// <param name="animationName">The name of the animation file (e.g., "Walking.anim").</param>
        /// <returns>The found AnimationClip, or null if not found.</returns>
        public static AnimationClip FindBodyAnimationClip(string animationName)
        {
            if (string.IsNullOrEmpty(animationName))
            {
                Debug.LogWarning("Attempted to find an animation with an empty name.");
                return null;
            }

            // Assumes animations are in a "Resources/Animations" folder.
            // Strips the file extension for the Resources.Load call.
            string path = "Animations/" + System.IO.Path.GetFileNameWithoutExtension(animationName);
            AnimationClip clip = Resources.Load<AnimationClip>(path);

            if (clip == null)
            {
                Debug.LogWarning($"Could not find body animation clip at 'Resources/{path}.anim'.");
            }
            return clip;
        }

        /// <summary>
        /// Finds a facial animation clip from the project's Resources folder.
        /// </summary>
        /// <param name="animationName">The name of the animation file (e.g., "Smile.anim").</param>
        /// <returns>The found AnimationClip, or null if not found.</returns>
        public static AnimationClip FindFaceAnimationClip(string animationName)
        {
            if (string.IsNullOrEmpty(animationName))
            {
                Debug.LogWarning("Attempted to find a face animation with an empty name.");
                return null;
            }

            // Assumes face animations are in a "Resources/Face Animations" folder.
            string path = "Face Animations/" + System.IO.Path.GetFileNameWithoutExtension(animationName);
            AnimationClip clip = Resources.Load<AnimationClip>(path);

            if (clip == null)
            {
                Debug.LogWarning($"Could not find face animation clip at 'Resources/{path}.anim'.");
            }
            return clip;
        }
    }
}
