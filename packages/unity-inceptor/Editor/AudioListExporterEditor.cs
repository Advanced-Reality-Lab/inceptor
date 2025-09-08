using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using InceptorEngine.Clips;
using InceptorEngine.Clips.Interfaces;

namespace InceptorEngine
{
    /// <summary>
    /// An editor utility to export a list of required audio files for an Inceptor cinematic.
    /// </summary>
    /// <remarks>
    /// This script creates a menu item in the Unity Editor under "Inceptor/Export Inceptor Audio List".
    /// When triggered, it opens a save dialog and then generates a CSV file detailing the expected audio clips.
    /// This is primarily used to guide audio production and ensure files are named correctly for automatic lookup.
    /// </remarks>
    public static class AudioListExporterEditor
    {
        /// <summary>
        /// Creates and exports a CSV file that lists all required audio clips for the active Inceptor instance.
        /// </summary>
        [MenuItem("Inceptor/Export Inceptor Audio List", false, 26)]
        public static void ExportAudioList()
        {
            Inceptor inceptor = Inceptor.Instance;

            // Use a save file panel to let the user choose the location.
            string path = EditorUtility.SaveFilePanel(
                "Save Inceptor Audio List",
                "", // Default directory
                $"{inceptor.name}_AudioList.csv", // Default file name
                "csv");

            // Exit if the user cancels the dialog.
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string[] headers = { "File Name", "Audio Text", "Speaking Character" };

            var csvContent = new StringBuilder();
            csvContent.AppendLine(string.Join(",", headers));

            int clipIndex = 0;
            foreach (Clip clip in inceptor.CinematicScript.clipList)
            {
                foreach (ClipCharacterData character in clip.Characters)
                {
                    if (character.isTalking)
                    {
                        // Using string interpolation for better readability.
                        string audioFileName = $"{clipIndex}_{character.name}";
                        string[] row = { audioFileName, $"\"{character.text}\"", character.name };
                        csvContent.AppendLine(string.Join(",", row));
                    }
                }
                clipIndex++;
            }
            
            File.WriteAllText(path, csvContent.ToString(), Encoding.UTF8);
            Debug.Log("Exported Audio File List to: " + path);
            
            // It's good practice to refresh if the file was saved within the Assets folder.
            if (path.StartsWith(Application.dataPath))
            {
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Validates whether the "Export Inceptor Audio List" menu item should be enabled.
        /// </summary>
        /// <returns>True if an Inceptor instance exists in the scene; otherwise, false.</returns>
        [MenuItem("Inceptor/Export Inceptor Audio List", true, 26)]
        private static bool ValidateExportAudioList()
        {
            return Inceptor.Instance != null;
        }
    }
}
