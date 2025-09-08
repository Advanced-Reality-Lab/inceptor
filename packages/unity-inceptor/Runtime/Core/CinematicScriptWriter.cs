   using System;
using System.Collections.Generic;
using System.IO;
using InceptorEngine.Clips;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace InceptorEngine
{
   
/// <summary>
    /// [Editor-Only] A service class responsible for taking a populated CinematicScript object
    /// and writing it and its associated clips to disk as .asset files.
    /// </summary>
    public static class CinematicScriptWriter
    {
        /// <summary>
        /// Saves a CinematicScript and its child clips to the AssetDatabase.
        /// </summary>
        /// <param name="scriptData">The populated CinematicScript object to save.</param>
        /// <param name="scriptName">The base name for the script and its containing folder.</param>
        public static void WriteToDisk(CinematicScript scriptData, string scriptName)
        {
            string scriptFolderPath = $"Assets/Inceptor/{scriptName}";
            string clipsFolderPath = $"{scriptFolderPath}/Clips";
            if (!Directory.Exists(scriptFolderPath)) Directory.CreateDirectory(scriptFolderPath);
            if (!Directory.Exists(clipsFolderPath)) Directory.CreateDirectory(clipsFolderPath);

            // Save the main script asset
            string mainAssetPath = $"{scriptFolderPath}/{scriptName}.asset";
            AssetDatabase.CreateAsset(scriptData, mainAssetPath);

            // Save each individual clip asset
            foreach (var clip in scriptData.clipList)
            {
                // Use the clip's name property for the filename.
                string clipName = clip.name; 
                if (string.IsNullOrEmpty(clipName))
                {
                    // Fallback for safety, though the JSON should always have a name.
                    clipName = $"Clip_{Guid.NewGuid()}"; 
                }
                AssetDatabase.CreateAsset(clip, $"{clipsFolderPath}/{clipName}.asset");
            }
            
            EditorUtility.SetDirty(scriptData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }


}