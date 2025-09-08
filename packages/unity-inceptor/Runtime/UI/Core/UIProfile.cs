using System;
using System.Collections.Generic;
using System.Linq;
using InceptorEngine.Clips.Interfaces;
using UnityEngine;

namespace InceptorEngine.UI.Core // Moved to a 'Core' sub-namespace for organization
{
    /// <summary>
    /// A serializable data structure that maps a Clip's type name to a specific UI prefab.
    /// </summary>
    [Serializable]
    public class UITypeMapping
    {
        [Tooltip("The class name of the Clip type (e.g., 'QuizClip').")]
        public string ClipTypeName;
        [Tooltip("The UI prefab that should be used by default for this clip type.")]
        public GameObject UIPrefab;
    }

    /// <summary>
    /// A ScriptableObject that acts as a central registry for all UI prefabs used by the Inceptor system.
    /// It maps clip types to default UI prefabs and holds a reference to a global overlay UI.
    /// </summary>
    [CreateAssetMenu(fileName = "UIProfile", menuName = "Inceptor/UI Profile")]
    public class UIProfile : ScriptableObject
    {
        [Header("Global UI")]
        [Tooltip("A persistent UI prefab that will be instantiated once and shown across all clips (e.g., for subtitles or a pause menu).")]
        [SerializeField]
        private GameObject _globalOverlayPrefab;

        [Header("Clip-Specific UI Mappings")]
        [Tooltip("A list that defines the default UI prefab to use for specific types of clips.")]
        [SerializeField]
        private List<UITypeMapping> _uiTypeMappings;

        // A private dictionary for fast, cached lookups at runtime.
        private Dictionary<string, GameObject> _uiPrefabCache;

        /// <summary>
        /// Gets the global overlay UI prefab.
        /// </summary>
        public GameObject GlobalOverlayPrefab => _globalOverlayPrefab;

        /// <summary>
        /// Called by Unity when the asset is loaded into memory.
        /// </summary>
        private void OnEnable()
        {
            // Build the cache for fast runtime lookups.
            _uiPrefabCache = _uiTypeMappings.ToDictionary(mapping => mapping.ClipTypeName, mapping => mapping.UIPrefab);
        }

        /// <summary>
        /// Gets the default UI prefab associated with a given clip type.
        /// </summary>
        /// <param name="clipType">The Type of the clip (e.g., typeof(QuizClip)).</param>
        /// <returns>The associated UI prefab, or null if no mapping is found.</returns>
        public GameObject GetUIPrefabForClip(Type clipType)
        {
            if (_uiPrefabCache == null)
            {
                // Ensure the cache is built, especially important for editor scripts.
                OnEnable();
            }

            _uiPrefabCache.TryGetValue(clipType.Name, out var uiPrefab);
            return uiPrefab;
        }

        /// <summary>
        /// Gets a UI prefab by its direct string name. This is used for overrides.
        /// </summary>
        /// <param name="uiName">The string name of the UI prefab to find.</param>
        /// <returns>The associated UI prefab, or null if no mapping is found.</returns>
        public GameObject GetUIPrefabByName(string uiName)
        {
            if (_uiPrefabCache == null)
            {
                OnEnable();
            }
            
            _uiPrefabCache.TryGetValue(uiName, out var uiPrefab);
            return uiPrefab;
        }
    }
}
