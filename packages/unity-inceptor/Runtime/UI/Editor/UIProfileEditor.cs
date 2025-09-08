using System;
using System.Collections.Generic;
using System.Linq;
using InceptorEngine.Clips.Interfaces;
using UnityEditor;
using UnityEngine;

namespace InceptorEngine.UI.Editor // It's good practice to put editor scripts in an 'Editor' sub-namespace
{
    /// <summary>
    /// A custom editor for the UIProfile ScriptableObject.
    /// It enhances the inspector experience by replacing the default list view with a custom,
    /// three-column layout for managing clip-to-UI mappings.
    /// </summary>
    [CustomEditor(typeof(Core.UIProfile))]
    public class UIProfileEditor : UnityEditor.Editor
    {
        private SerializedProperty _globalOverlayPrefabProp;
        private SerializedProperty _uiTypeMappingsProp;
        
        private string[] _allClipTypeNames;

        private void OnEnable()
        {
            // Find the properties we want to customize.
            _globalOverlayPrefabProp = serializedObject.FindProperty("_globalOverlayPrefab");
            _uiTypeMappingsProp = serializedObject.FindProperty("_uiTypeMappings");

            // Use reflection to find all non-abstract classes that inherit from Clip.
            _allClipTypeNames = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(Clip)) && !type.IsAbstract)
                .Select(type => type.Name)
                .ToArray();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // --- Draw the Global UI Field ---
            EditorGUILayout.PropertyField(_globalOverlayPrefabProp);
            
            EditorGUILayout.Space(10);
            
            // --- Draw the Custom Mappings List ---
            EditorGUILayout.LabelField("Clip-Specific UI Mappings", EditorStyles.boldLabel);

            // First, get a complete set of all clip types that are currently in use.
            var allUsedTypes = new HashSet<string>();
            for (int i = 0; i < _uiTypeMappingsProp.arraySize; i++)
            {
                allUsedTypes.Add(_uiTypeMappingsProp.GetArrayElementAtIndex(i).FindPropertyRelative("ClipTypeName").stringValue);
            }

            // Manually iterate through the list to create our custom layout.
            for (int i = 0; i < _uiTypeMappingsProp.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                var mappingElement = _uiTypeMappingsProp.GetArrayElementAtIndex(i);
                var clipTypeNameProp = mappingElement.FindPropertyRelative("ClipTypeName");
                var uiPrefabProp = mappingElement.FindPropertyRelative("UIPrefab");

                // --- Column 1: Clip Type Dropdown ---
                
                string currentSelectedType = clipTypeNameProp.stringValue;

                // Build a list of available options for this specific dropdown.
                // An option is available if it's not used by ANOTHER mapping, OR if it's the one currently selected here.
                var availableOptions = _allClipTypeNames
                    .Where(name => !allUsedTypes.Contains(name) || name == currentSelectedType)
                    .ToList();
                
                int currentIndex = availableOptions.IndexOf(currentSelectedType);
                
                int newIndex = EditorGUILayout.Popup(currentIndex, availableOptions.ToArray(), GUILayout.MinWidth(100));
                
                if (newIndex >= 0 && newIndex < availableOptions.Count && newIndex != currentIndex)
                {
                    clipTypeNameProp.stringValue = availableOptions[newIndex];
                }
                
                // --- Column 2: UI Prefab Field ---
                EditorGUILayout.PropertyField(uiPrefabProp, GUIContent.none, GUILayout.MinWidth(100));

                // --- Column 3: Remove Button ---
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    // Important: When removing, we must remove the correct element regardless of display filtering.
                    // We find the element's actual index in the serialized property before deleting.
                    int propertyIndexToDelete = i;
                    _uiTypeMappingsProp.DeleteArrayElementAtIndex(propertyIndexToDelete);
                    break; 
                }

                EditorGUILayout.EndHorizontal();
            }

            // --- Add New Mapping Button ---
            EditorGUILayout.Space();
            // Only show the "Add" button if there are still unmapped clip types available.
            if (allUsedTypes.Count < _allClipTypeNames.Length)
            {
                if (GUILayout.Button("Add New UI Mapping"))
                {
                    // FIX: When adding a new element, immediately assign the first available clip type.
                    int newIndex = _uiTypeMappingsProp.arraySize;
                    _uiTypeMappingsProp.InsertArrayElementAtIndex(newIndex);
                    var newElement = _uiTypeMappingsProp.GetArrayElementAtIndex(newIndex);
                    var newClipTypeNameProp = newElement.FindPropertyRelative("ClipTypeName");
                    
                    // Find the first clip type that is not already in the 'allUsedTypes' set.
                    string firstAvailableType = _allClipTypeNames.FirstOrDefault(name => !allUsedTypes.Contains(name));
                    newClipTypeNameProp.stringValue = firstAvailableType;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("All available Clip types have been mapped.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
