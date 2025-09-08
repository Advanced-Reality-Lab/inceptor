using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using InceptorEngine.Analysis.Interfaces;
using InceptorEngine.Analysis.Analyzers.OpenAI;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace InceptorEngine.Analysis.Analyzers
{
    /// <summary>
    /// An analyzer that uses the OpenAI Chat Completions API to determine the best-matching answer.
    /// This is a flexible, powerful analyzer that requires an internet connection and a valid API key.
    /// </summary>
    [CreateAssetMenu(fileName = "OpenAIAnalyzer", menuName = "Inceptor/Analyzers/OpenAI Analyzer")]
    public class OpenAIAnalyzer : BaseNextClipAnalyzer
    {
        #region Configuration

        [Header("API Configuration")]
        [Tooltip("The OpenAI model to use for the analysis (e.g., 'gpt-4', 'gpt-3.5-turbo').")]
        [SerializeField] private string _model = "gpt-3.5-turbo";

        [Header("Prompt Engineering")]
        [Tooltip("The system prompt that instructs the AI on how to behave. Use {answers} as a placeholder for the list of choices.")]
        [TextArea(5, 15)]
        [SerializeField]
        private string _systemPrompt = "You are an AI assistant for a narrative game. The user will provide a statement. You must choose which of the following numbered options is the most semantically similar to the user's statement. The options are:\n{answers}\n\nYou must respond with only a single integer representing the index of the best-matching option. If none of the options are a good match, respond with -1.";

        #endregion

        #region IInceptorNextClipAnalyzer Implementation

        /// <summary>
        /// Initiates the analysis by preparing a web request and asking the Inceptor to run it.
        /// </summary>
        public override void AnalyzeNextClip(AnalysisContext context, UnityAction<int> onResultReceived)
        {
            // Since ScriptableObjects cannot start coroutines, we delegate the execution to the Inceptor singleton.
            Inceptor.Instance.StartCoroutine(GetOpenAIResponse(context, onResultReceived));
        }

        #endregion

        #region Private Coroutine Logic

        /// <summary>
        /// The main coroutine that builds the request, sends it to OpenAI, and processes the response.
        /// </summary>
        private IEnumerator GetOpenAIResponse(AnalysisContext context, UnityAction<int> onResultReceived)
        {
            string apiKey = Inceptor.Instance.Settings.OpenAIKey;
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("OpenAI API Key is not set in InceptorSettings. Aborting analysis.");
                onResultReceived?.Invoke(-1);
                yield break;
            }

            // 1. Build the request payload.
            string requestBody = BuildRequestPayload(context);
            
            // 2. Create and configure the web request.
            using (var www = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST"))
            {
                byte[] bodyRaw = new UTF8Encoding().GetBytes(requestBody);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", $"Bearer {apiKey}");

                // 3. Send the request and wait for a response.
                yield return www.SendWebRequest();

                // 4. Process the response.
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"OpenAI API Error: {www.error}\nResponse: {www.downloadHandler.text}");
                    onResultReceived?.Invoke(-1);
                }
                else
                {
                    int resultIndex = ParseResponse(www.downloadHandler.text);
                    onResultReceived?.Invoke(resultIndex);
                }
            }
        }

        /// <summary>
        /// Builds the JSON payload for the OpenAI API request.
        /// </summary>
        private string BuildRequestPayload(AnalysisContext context)
        {
            // Format the list of answers for inclusion in the prompt.
            var formattedAnswers = new StringBuilder();
            for (int i = 0; i < context.PossibleAnswers.Count; i++)
            {
                formattedAnswers.AppendLine($"{i}: {context.PossibleAnswers[i]}");
            }

            var request = new OpenAIRequest
            {
                Model = _model,
                Messages = new List<OpenAIMessage>
                {
                    new OpenAIMessage
                    {
                        Role = "system",
                        Content = _systemPrompt.Replace("{answers}", formattedAnswers.ToString())
                    },
                    new OpenAIMessage
                    {
                        Role = "user",
                        Content = context.UserInput
                    }
                }
            };

            return JsonConvert.SerializeObject(request);
        }

        /// <summary>
        /// Safely parses the JSON response from OpenAI to extract the integer result.
        /// </summary>
        private int ParseResponse(string jsonResponse)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<OpenAIResponse>(jsonResponse);
                string content = response?.Choices[0]?.Message?.Content;

                if (int.TryParse(content, out int resultIndex))
                {
                    return resultIndex;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse OpenAI response: {e.Message}\nJSON: {jsonResponse}");
            }

            return -1; // Return -1 on any parsing failure.
        }

                public override bool CanHandleContext(AnalysisContext context)
        {
            // This analyzer is simple and only needs the base information.
            // It can handle the base AnalysisContext and any class that inherits from it (like QuizAnalysisContext).
            return context is AnalysisContext;
        }

        #endregion
    }
}
