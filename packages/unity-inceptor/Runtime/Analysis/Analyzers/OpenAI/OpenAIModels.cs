using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InceptorEngine.Analysis.Analyzers.OpenAI
{
    // --- Data structures for the OpenAI Chat Completions API ---

    [Serializable]
    public class OpenAIRequest
    {
        [JsonProperty("model")]
        public string Model;

        [JsonProperty("messages")]
        public List<OpenAIMessage> Messages;

        [JsonProperty("temperature")]
        public float Temperature = 0.5f;

        [JsonProperty("max_tokens")]
        public int MaxTokens = 10;
    }

    [Serializable]
    public class OpenAIMessage
    {
        [JsonProperty("role")]
        public string Role;

        [JsonProperty("content")]
        public string Content;
    }

    [Serializable]
    public class OpenAIResponse
    {
        [JsonProperty("choices")]
        public List<OpenAIChoice> Choices;
    }

    [Serializable]
    public class OpenAIChoice
    {
        [JsonProperty("message")]
        public OpenAIMessage Message;
    }
}
