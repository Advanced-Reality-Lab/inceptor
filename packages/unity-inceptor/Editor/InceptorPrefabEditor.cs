using UnityEditor;
using UnityEngine;

namespace InceptorEngine
{
    /// <summary>
    /// An editor utility for adding core Inceptor prefabs to the scene.
    /// </summary>
    public static class InceptorPrefabEditor
    {
        // A constant for the prefab path to avoid "magic strings" and make it easy to update.
        private const string CAMERA_MANAGER_PREFAB_PATH = "Prefabs/InceptorCameraManager";

        /// <summary>
        /// Adds the Inceptor Camera Manager prefab to the current scene from the package resources.
        /// </summary>
        /// <remarks>
        /// This method ensures that only one instance of the Camera Manager exists in the scene.
        /// If an instance is already present, it will be focused in the hierarchy instead of creating a new one.
        /// </remarks>
        [MenuItem("Inceptor/Add Camera Manager", false, 30)]
        private static void AddCameraManager()
        {
            // Correctly look for the runtime component "InceptorCameraManager".
            InceptorCameraManager existingManager = Object.FindFirstObjectByType<InceptorCameraManager>();

            if (existingManager != null)
            {
                Debug.Log("InceptorCameraManager already exists in the scene. Selecting it.");
                // Highlight the existing object for the user to see.
                Selection.activeObject = existingManager.gameObject;
                return;
            }

            // If no instance exists, load the prefab from the Resources folder.
            GameObject cameraManagerPrefab = Resources.Load<GameObject>(CAMERA_MANAGER_PREFAB_PATH);

            // Defensive check in case the prefab is missing or moved.
            if (cameraManagerPrefab == null)
            {
                Debug.LogError($"Failed to load Camera Manager prefab. Ensure it's located at: Resources/{CAMERA_MANAGER_PREFAB_PATH}");
                return;
            }
            
            // Instantiate the prefab into the scene.
            GameObject managerInstance = (GameObject)PrefabUtility.InstantiatePrefab(cameraManagerPrefab);
            
            // Give the new instance a clean name in the hierarchy.
            if(managerInstance != null)
            {
                managerInstance.name = "InceptorCameraManager";
                // Select the newly created object to give the user immediate feedback.
                Selection.activeObject = managerInstance;
            }
        }
    }
}