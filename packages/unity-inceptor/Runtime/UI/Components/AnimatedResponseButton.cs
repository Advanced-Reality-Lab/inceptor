using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InceptorEngine.UI.Components // Moved to a new 'Components' sub-namespace
{
    /// <summary>
    /// A UI component that represents a single, animatable response button in a choice or quiz clip.
    /// It uses an Animator to transition between states like 'Selected', 'Correct', and 'Incorrect'.
    /// </summary>
    [RequireComponent(typeof(Animator), typeof(Button))]
    public class AnimatedResponseButton : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Animation Triggers")] 
        [SerializeField] private string _onSelectedTrigger = "Select";
        [SerializeField] private string _onCorrectAnswerTrigger = "Correct";
        [SerializeField] private string _onNotChosenCorrectAnswerTrigger = "NotChosenCorrect";
        [SerializeField] private string _onIncorrectAnswerTrigger = "Incorrect";
        [SerializeField] private string _onResetTrigger = "Reset";
        
        [Header("References")] 
        [Tooltip("The TextMeshPro component used to display the answer text.")]
        [SerializeField] private TextMeshProUGUI _textComponent;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the core UI Button component attached to this GameObject.
        /// </summary>
        public Button Button { get; private set; }

        #endregion

        #region Private State

        private Animator _animator;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Get references to the required components on this GameObject.
            _animator = GetComponent<Animator>();
            Button = GetComponent<Button>();

            if (_textComponent == null)
            {
                // Try to find it in children if not assigned, which is a common setup.
                _textComponent = GetComponentInChildren<TextMeshProUGUI>();
                if (_textComponent == null)
                {
                    Debug.LogWarning("AnimatedResponseButton: TextMeshProUGUI component is not assigned and could not be found in children.", this);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the display text of the button.
        /// </summary>
        public void SetText(string text)
        {
            if (_textComponent != null)
            {
                _textComponent.text = text;
            }
        }

        /// <summary>
        /// Gets the display text of the button.
        /// </summary>
        public string GetText()
        {
            return _textComponent != null ? _textComponent.text : string.Empty;
        }

        /// <summary>
        /// Triggers the 'Selected' animation state.
        /// </summary>
        public void Select()
        {
            _animator.SetTrigger(_onSelectedTrigger);
        }
        
        /// <summary>
        /// Triggers the 'CorrectAnswer' animation state.
        /// </summary>
        public void CorrectAnswer()
        {
            _animator.SetTrigger(_onCorrectAnswerTrigger);
        }
        
        /// <summary>
        /// Triggers the 'NotChosenCorrectAnswer' animation state. Used to highlight the correct
        /// answer when the user has chosen an incorrect one.
        /// </summary>
        public void NotChosenCorrectAnswer()
        {
            _animator.SetTrigger(_onNotChosenCorrectAnswerTrigger);
        }
        
        /// <summary>
        /// Triggers the 'IncorrectAnswer' animation state.
        /// </summary>
        public void IncorrectAnswer()
        {
            _animator.SetTrigger(_onIncorrectAnswerTrigger);
        }
        
        /// <summary>
        /// Resets the button to its default, idle state.
        /// </summary>
        public void Reset()
        {
            _animator.SetTrigger(_onResetTrigger);
        }

        #endregion
    }
}
