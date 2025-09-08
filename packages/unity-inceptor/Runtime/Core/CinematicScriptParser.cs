   using System;
using System.Collections.Generic;
using System.IO;
using InceptorEngine.Clips.Interfaces;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace InceptorEngine
{
   
   /// <summary>
    /// [Editor-Only] A service class responsible for parsing a JSON string into an
    /// in-memory representation of a CinematicScript without touching the AssetDatabase.
    /// </summary>
    public static class CinematicScriptParser
    {
        /// <summary>
        /// Parses a JSON string to create and populate a CinematicScript object in memory.
        /// </summary>
        /// <param name="jsonString">The raw JSON content to parse.</param>
        /// <returns>A populated CinematicScript object, or null if a critical parsing error occurs.</returns>
        public static CinematicScript Parse(string jsonString)
        {
            var asset = ScriptableObject.CreateInstance<CinematicScript>();
            JObject cinematicScriptJson = JObject.Parse(jsonString);

            // --- Build Clips ---
            if (cinematicScriptJson["Clips"] != null)
            {
                foreach (JObject clipJson in cinematicScriptJson["Clips"].Children<JObject>())
                {
                    string clipType = clipJson["metadata"]?["type"]?.Value<string>();
                    if (string.IsNullOrEmpty(clipType))
                    {
                        Debug.LogWarning("Found a clip in JSON with a missing 'type' in its metadata. Skipping.");
                        continue;
                    }

                    Type type = Type.GetType($"InceptorEngine.Clips.{clipType}");
                    if (type == null)
                    {
                        Debug.LogError($"Fatal Error: Clip Type '{clipType}' not found. Please ensure a corresponding script exists in the InceptorEngine.Clips namespace. Aborting build.");
                        return null; // Critical error, abort the process.
                    }
                    
                    var newClip = (Clip)ScriptableObject.CreateInstance(type);
                    newClip.InitializeFromJSON(clipJson);
                    asset.clipList.Add(newClip);
                }
            }

            // --- Parse Characters ---
            if (cinematicScriptJson["Characters"] != null)
            {
                foreach (JObject characterData in cinematicScriptJson["Characters"].Children<JObject>())
                {
                    CinematicScriptCharacterInfo newChar = new CinematicScriptCharacterInfo
                    {
                        name = characterData["name"]?.Value<string>(),
                        description = characterData["description"]?.Value<string>() ?? "",
                        modelName = characterData["modelName"]?.Value<string>() ?? ""
                    };
                    asset.characters.Add(newChar);
                }
            }

            return asset;
        }
    }
}