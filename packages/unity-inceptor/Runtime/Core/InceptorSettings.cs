using InceptorEngine.Analysis.Interfaces;
using InceptorEngine.Input.Interfaces; // Assuming this is the correct namespace for IInceptorInput
using UnityEngine;

/// <summary>
/// A ScriptableObject that holds central configuration settings for the Inceptor system.
/// This includes API keys and references to core service implementations.
/// </summary>
[CreateAssetMenu(fileName = "InceptorSettings", menuName = "Inceptor/Inceptor Settings", order = 1)]
public class InceptorSettings : ScriptableObject
{
    [Header("API Keys")]
    [Tooltip("The API key for Google Cloud services (e.g., Speech-to-Text).")]
    public string GoogleAPIKey;

    [Tooltip("The API key for OpenAI services (e.g., GPT for analysis).")]
    public string OpenAIKey;

    [Header("Core Service Implementations")]
    [Tooltip("The analysis module (as a ScriptableObject) responsible for deciding the next clip.")]
    [SerializeField]
    private ScriptableObject _nextClipAnalyzer;

    /// <summary>
    /// The analysis service that determines the next clip to play based on user input.
    /// </summary>
    public IInceptorNextClipAnalyzer NextClipAnalyzer => _nextClipAnalyzer as IInceptorNextClipAnalyzer;

    [Tooltip("The input provider (as a ScriptableObject) responsible for capturing user input.")]
    [SerializeField]
    private ScriptableObject _inputProvider;

    /// <summary>
    /// The input service that provides user input to the system.
    /// </summary>
    public IInceptorInput InputProvider => _inputProvider as IInceptorInput;
}
