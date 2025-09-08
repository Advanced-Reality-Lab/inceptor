using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InceptorEngine.Analysis.Interfaces;
using InceptorEngine.Clips;
using InceptorEngine.Clips.Interfaces;
using InceptorEngine.Input.Interfaces;
using InceptorEngine.UI.Interfaces;
using InceptorEngine.Utils;
using InceptorEngine.UI.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


namespace InceptorEngine
{
    /// <summary>
    /// The central conductor for the Inceptor cinematic system.
    /// This MonoBehaviour orchestrates the entire scene, manages services,
    /// and executes the sequence of clips defined in the CinematicScript.
    /// </summary>
    public class Inceptor : MonoBehaviour
    {
        #region Public Events

        /// <summary>
        /// Fired when a new clip begins playing. The integer payload is the index of the clip.
        /// </summary>
        public event UnityAction<int> ClipStarted;

        /// <summary>
        /// Fired when the entire sequence of clips has finished playing.
        /// </summary>
        public event UnityAction ClipsFinished;

        #endregion

        #region Serialized Fields

        [Header("Core Assets")]
        [Tooltip("The settings asset containing API keys and default service implementations.")]
        [SerializeField] private InceptorSettings _settings;
        [Tooltip("The cinematic script asset that defines the narrative flow.")]
        [SerializeField] private CinematicScript _cinematicScript;
        [Tooltip("The UI Profile asset that defines the UI prefabs available to the clips.")]
        [SerializeField] private UIProfile _uiProfile;
        
        [Header("Configuration")]
        [Tooltip("If true, the cinematic will begin playing automatically when the scene loads.")]
        [SerializeField] private bool _startInceptorOnSceneLoad = true;

        [Header("Scene References")]
        [Tooltip("A list of all InceptorCharacterController instances present in the scene.")]
        [SerializeField] private List<InceptorCharacterController> _characterList;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the singleton instance of the Inceptor in the scene.
        /// </summary>
        public static Inceptor Instance { get; private set; }

        /// <summary>
        /// Gets the settings asset currently being used by the Inceptor.
        /// </summary>
        public InceptorSettings Settings => _settings;

        /// <summary>
        /// Gets the cinematic script asset currently loaded.
        /// </summary>
        public CinematicScript CinematicScript => _cinematicScript;

        /// <summary>
        /// Gets the character list connected to the ciematicScript.
        /// </summary>
        public List<InceptorCharacterController> CharacterList => _characterList;
        
        /// <summary>
        /// Gets the index of the clip that is currently playing.
        /// </summary>
        public int CurrentClipIndex { get; private set; }

        #endregion

        #region Public API

        /// <summary>
        /// [Editor-Only] Initializes the Inceptor with its core asset dependencies.
        /// This should be called by the SceneBuilder after instantiation.
        /// </summary>
        public void Initialize(InceptorSettings settings, CinematicScript script)
        {
            _settings = settings;
            _cinematicScript = script;
        }

        /// <summary>
        /// Registers a character controller with the Inceptor's internal list.
        /// </summary>
        public void RegisterCharacter(InceptorCharacterController character)
        {
            if (_characterList == null) _characterList = new List<InceptorCharacterController>();
            if (character != null && !_characterList.Contains(character))
            {
                _characterList.Add(character);
            }
        }

        // ... (rest of the Public API methods like StartCinematic, CreateUI, etc.) ...

        #endregion

        #region Private State

        private IInceptorNextClipAnalyzer _defaultAnalyzer;
        private IInceptorInput _defaultInputProvider;
        private Dictionary<string, InceptorCharacterController> _characterControllers;
        private Coroutine _cinematicCoroutine;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // --- Singleton Initialization ---
            if (Instance != null && Instance != this)
            {
                Debug.LogError("There is more than one Inceptor instance in the scene. Destroying this one.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // --- Service and Data Initialization ---
            if (_settings == null)
            {
                Debug.LogError("Inceptor Settings asset is not assigned.", this);
                return;
            }
            
            // Load default services from the settings asset.
            _defaultAnalyzer = _settings.NextClipAnalyzer;
            _defaultInputProvider = _settings.InputProvider;

            // Build a dictionary of characters for fast lookups.
            _characterControllers = _characterList.ToDictionary(c => c.name, c => c);
        }

        private void OnEnable()
        {
            if (_startInceptorOnSceneLoad)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        private void OnDisable()
        {
            if (_startInceptorOnSceneLoad)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCinematic();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Starts the cinematic playback from the beginning.
        /// </summary>
        public void StartCinematic()
        {
            if (_cinematicCoroutine != null)
            {
                StopCoroutine(_cinematicCoroutine);
            }
            _cinematicCoroutine = StartCoroutine(RunCinematic());
        }

        /// <summary>
        /// Stops the cinematic playback immediately.
        /// </summary>
        public void StopCinematic()
        {
            if (_cinematicCoroutine != null)
            {
                StopCoroutine(_cinematicCoroutine);
                _cinematicCoroutine = null;
            }
        }
        
        /// <summary>
        /// Instantiates a UI prefab from the UI Profile.
        /// </summary>
        /// <param name="uiName">The name of the UI prefab to create.</param>
        /// <returns>The instantiated GameObject.</returns>
        public GameObject CreateUI(string uiName)
        {
            GameObject uiPrefab = _uiProfile.GetUIPrefabByName(uiName);
            if (uiPrefab == null)
            {
                Debug.LogError($"UI prefab '{uiName}' not found in the UI Profile.");
                return null;
            }
            return Instantiate(uiPrefab);
        }

        /// <summary>
        /// Hides and destroys a UI element.
        /// </summary>
        /// <param name="uiElement">The UI GameObject to remove.</param>
        public IEnumerator RemoveUI(GameObject uiElement)
        {
            if (uiElement != null && uiElement.TryGetComponent<IInceptorUI>(out var uiController))
            {
                yield return uiController.Hide();
                Destroy(uiElement);
            }
        }

        #endregion

        #region Clip Execution

        /// <summary>
        /// The main coroutine that drives the entire cinematic. It runs in a loop,
        /// executing one clip at a time until the script is finished.
        /// </summary>
        private IEnumerator RunCinematic()
        {
            CurrentClipIndex = 0;

            while (CurrentClipIndex != -1 && CurrentClipIndex < _cinematicScript.clipList.Count)
            {
                Clip currentClip = _cinematicScript.clipList[CurrentClipIndex];
                
                // --- Prepare the Context (Dependency Injection) ---
                var runtimeContext = CreateRuntimeContextForClip(currentClip);
                
                // --- Execute the Clip ---
                ClipStarted?.Invoke(CurrentClipIndex);
                yield return currentClip.RunClip(runtimeContext, OnClipExecutionComplete);
            }

            ClipsFinished?.Invoke();
        }

        /// <summary>
        /// A callback method that is passed to each clip. The clip invokes this
        /// method when it has finished its execution.
        /// </summary>
        /// <param name="nextClipIndex">The index of the next clip to play.</param>
        private void OnClipExecutionComplete(int nextClipIndex)
        {
            CurrentClipIndex = nextClipIndex;
        }

        /// <summary>
        /// Creates the context object for the current clip, deciding which services to provide.
        /// </summary>
        /// <param name="clip">The clip that is about to be executed.</param>
        /// <returns>A fully populated ClipRuntimeContext.</returns>
        private ClipRuntimeContext CreateRuntimeContextForClip(Clip clip)
        {
            return new ClipRuntimeContext(_characterControllers, _defaultAnalyzer, _defaultInputProvider);
        }


        [System.Obsolete("This method is deprecated. Please use Inceptor.Instance instead.")]
        public static Inceptor GetAvailableInceptors()
        {
            return Instance;
        }

        #endregion
    }
}
