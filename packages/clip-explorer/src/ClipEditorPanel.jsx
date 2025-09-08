import React, { useState, useEffect } from 'react';
import { theme } from './theme';

// A specialized component for editing choices side-by-side.
// It is now "quiz-aware" and will show feedback fields if needed.
function ChoicesEditor({ answers, nextClips, feedback, isQuiz, onChange }) {
  const handleAnswerChange = (index, value) => {
    const newAnswers = [...answers];
    newAnswers[index] = value;
    onChange('answers', newAnswers);
  };

  const handleNextClipChange = (index, value) => {
    const newNextClips = [...nextClips];
    newNextClips[index] = parseInt(value, 10) || 0;
    onChange('nextClips', newNextClips);
  };

  const handleFeedbackChange = (index, value) => {
    const newFeedback = [...feedback];
    newFeedback[index] = value;
    onChange('feedback', newFeedback);
  };

  const addChoice = () => {
    onChange('answers', [...answers, 'New Answer']);
    onChange('nextClips', [...nextClips, -1]);
    if (isQuiz) {
      onChange('feedback', [...feedback, '']);
    }
  };

  const removeChoice = (index) => {
    onChange('answers', answers.filter((_, i) => i !== index));
    onChange('nextClips', nextClips.filter((_, i) => i !== index));
    if (isQuiz) {
      onChange('feedback', feedback.filter((_, i) => i !== index));
    }
  };

  return (
    <div style={{ marginTop: '10px' }}>
      <label style={{ fontWeight: 'bold' }}>Choices:</label>
      {answers.map((answer, index) => (
        <div key={index} style={{ border: '1px solid #eee', padding: '5px', borderRadius: '4px', marginTop: '5px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '5px' }}>
            <input
              type="text"
              value={answer}
              onChange={(e) => handleAnswerChange(index, e.target.value)}
              style={{ flexGrow: 1, padding: '5px' }}
              placeholder="Answer Text"
            />
            <input
              type="number"
              value={nextClips[index]}
              onChange={(e) => handleNextClipChange(index, e.target.value)}
              style={{ width: '60px', padding: '5px' }}
              title="Next Clip Index"
            />
            <button onClick={() => removeChoice(index)} style={{ backgroundColor: '#f44336', color: 'white', border: 'none', cursor: 'pointer' }}>-</button>
          </div>
          {isQuiz && (
            <input
              type="text"
              value={feedback[index] || ''}
              onChange={(e) => handleFeedbackChange(index, e.target.value)}
              style={{ width: '100%', padding: '5px', marginTop: '5px' }}
              placeholder="Feedback for this answer..."
            />
          )}
        </div>
      ))}
      <button onClick={addChoice} style={{ width: '100%', marginTop: '10px', padding: '5px' }}>Add Choice</button>
    </div>
  );
}

// The main FormField component, now aware of the new 'choices' type.
function FormField({ field, value, onChange, allMetadata }) {
  const { name, label, type, editorVisible } = field;

  // If the field is configured to be hidden in the editor, render nothing.
  if (editorVisible === false) {
    return null;
  }

  const handleChange = (e) => {
    const newValue = type.endsWith('[]') ? e.target.value.split(',').map(item => item.trim()) : e.target.value;
    onChange(name, newValue);
  };

  if (type === 'choices') {
    return (
      <ChoicesEditor 
        answers={allMetadata.answers || []} 
        nextClips={allMetadata.nextClips || []}
        feedback={allMetadata.feedback || []}
        isQuiz={allMetadata.type === 'QuizClip'}
        onChange={onChange}
      />
    );
  }

  if (type === 'textarea') {
    return (
      <div style={{ marginTop: '10px' }}>
        <label style={{ fontWeight: 'bold' }}>{label}:</label>
        <textarea
          name={name}
          value={value || ''}
          onChange={handleChange}
          style={{ width: '100%', padding: '5px', marginTop: '5px', minHeight: '60px' }}
        />
      </div>
    );
  }

  return (
    <div style={{ marginTop: '10px' }}>
      <label style={{ fontWeight: 'bold' }}>{label}:</label>
      <input
        type={type === 'number' ? 'number' : 'text'}
        name={name}
        value={Array.isArray(value) ? value.join(', ') : value || ''}
        onChange={handleChange}
        style={{ width: '100%', padding: '5px', marginTop: '5px' }}
      />
    </div>
  );
}


export default function ClipEditorPanel({ clip, clipIndex, onUpdate, onClose, config }) {
  const [editedClip, setEditedClip] = useState(clip);
  const [activeTab, setActiveTab] = useState('metadata');
  const [validationError, setValidationError] = useState(null);
  const [isSuggesting, setIsSuggesting] = useState(false);

  useEffect(() => {
    setEditedClip(clip);
    setActiveTab('metadata');
  }, [clip]);

  useEffect(() => {
    if (editedClip) {
      validateClip(editedClip);
    }
  }, [editedClip]);

  if (!clip || !editedClip || !config) {
    return (
      <div style={{ width: '300px', padding: '10px', borderLeft: '1px solid #ccc', backgroundColor: '#f8f8f8' }}>
        <p>{!config ? "Loading configuration..." : "Select a clip to see its details."}</p>
      </div>
    );
  }

  const validateClip = (clipToValidate) => {
    const { metadata } = clipToValidate;
    if (metadata.type === 'LinearClip') {
      if (metadata.nextClip === undefined || metadata.nextClip === null || isNaN(metadata.nextClip)) {
        setValidationError('LinearClip must have a valid Next Clip Index.');
        return false;
      }
    } else if (metadata.type === 'ChoiceClip' || metadata.type === 'QuizClip') {
      if (!metadata.answers || metadata.answers.length === 0) {
        setValidationError('Choice and Quiz clips must have at least one answer.');
        return false;
      }
      if (metadata.type === 'QuizClip' && (!metadata.feedback || metadata.feedback.length !== metadata.answers.length)) {
        setValidationError('QuizClip must have a corresponding feedback entry for every answer.');
        return false;
      }
    }
    setValidationError(null);
    return true;
  };

  const handleMetadataChange = (fieldName, value) => {
    setEditedClip(prevClip => ({
      ...prevClip,
      metadata: {
        ...prevClip.metadata,
        [fieldName]: value,
      },
    }));
  };

  const handleSuggestAnimations = async () => {
    if (!editedClip || !config?.aiServiceUrl) {
      alert("AI suggestion service is not configured in config.json.");
      return;
    }

    setIsSuggesting(true);

    // This is the data we would send to the real AI service.
    // It includes the dialogue context and the possible animation choices.
    const requestPayload = {
      characters: editedClip.characters.map(c => ({ name: c.name, text: c.text, isTalking: c.isTalking })),
      availableAnimations: {
        moods: config.moods,
        bodyBehaviors: config.bodyBehaviors,
        reactions: config.reactions,
      }
    };

    try {
      // --- This is where the real API call would go ---
      // const response = await fetch(config.aiServiceUrl, {
      //   method: 'POST',
      //   headers: { 'Content-Type': 'application/json' },
      //   body: JSON.stringify(requestPayload),
      // });
      // if (!response.ok) throw new Error(`API Error: ${response.status}`);
      // const suggestions = await response.json();
      
      // --- For now, we simulate the API call with a mock response ---
      console.log("Simulating API call with payload:", JSON.stringify(requestPayload, null, 2));
      await new Promise(resolve => setTimeout(resolve, 1000)); // Simulate network delay

      const mockSuggestions = {
        characters: editedClip.characters.map(char => ({
          name: char.name,
          // Simple mock logic: suggest "Happy" if not talking, "Thinking" if talking.
          mood: char.isTalking ? "Thinking" : "Happy",
          bodyBehavior: char.isTalking ? "Explain" : "Idle",
          reaction: "None",
        }))
      };
      
      // --- Update the local 'editedClip' state with the new suggestions ---
      setEditedClip(prevClip => {
        const newCharacters = prevClip.characters.map(originalChar => {
          const suggestion = mockSuggestions.characters.find(s => s.name === originalChar.name);
          return suggestion ? { ...originalChar, ...suggestion } : originalChar;
        });
        return { ...prevClip, characters: newCharacters };
      });

    } catch (error) {
      console.error("Failed to get AI suggestions:", error);
      alert("An error occurred while getting AI suggestions.");
    } finally {
      setIsSuggesting(false);
    }
  };
  
  const handleTypeChange = (newType) => {
    const newMetadata = {
      name: editedClip.metadata.name,
      type: newType,
    };
    const newClip = {
      ...editedClip,
      metadata: newMetadata,
    };
    setEditedClip(newClip);
  };

  const handleCharacterChange = (charIndex, fieldName, value) => {
    setEditedClip(prevClip => {
      const newCharacters = [...prevClip.characters];
      const finalValue = fieldName === 'isTalking' ? !newCharacters[charIndex].isTalking : value;
      newCharacters[charIndex] = { ...newCharacters[charIndex], [fieldName]: finalValue };
      return { ...prevClip, characters: newCharacters };
    });
  };

  const handleSave = () => {
    if (validateClip(editedClip)) {
      onUpdate(clipIndex, editedClip);
    }
  };
  
  const clipConfig = config.clipTypes[editedClip.metadata.type];

  const tabButtonStyle = (tabName) => ({
    padding: theme.spacing.medium,
    cursor: 'pointer',
    border: 'none',
    borderBottom: activeTab === tabName ? `2px solid ${theme.colors.primary}` : '2px solid transparent',
    backgroundColor: activeTab === tabName ? theme.colors.panel : 'transparent',
    fontWeight: activeTab === tabName ? 'bold' : 'normal',
    color: theme.colors.text,
  });

  return (
    <div style={{
      width: '300px',
      padding: theme.spacing.medium,
      borderLeft: `1px solid ${theme.colors.border}`,
      backgroundColor: theme.colors.panel,
      display: 'flex',
      flexDirection: 'column',
      overflowY: 'auto',
      color: theme.colors.text,
    }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: theme.spacing.medium }}>
        <h3 style={{ margin: 0 }}>Edit Clip</h3>
        <button onClick={onClose} style={{ cursor: 'pointer', border: 'none', background: 'transparent' }}>X</button>
      </div>
      
      <div style={{ display: 'flex', borderBottom: `1px solid ${theme.colors.border}` }}>
        <button style={tabButtonStyle('metadata')} onClick={() => setActiveTab('metadata')}>Metadata</button>
        <button style={tabButtonStyle('characters')} onClick={() => setActiveTab('characters')}>Characters</button>
      </div>
      
      <div style={{ flexGrow: 1, overflowY: 'auto', padding: '10px 0' }}>
        {activeTab === 'metadata' && (
          <div>
            <div style={{ marginTop: '10px' }}>
              <label style={{ fontWeight: 'bold' }}>Name:</label>
              <input 
                type="text"
                name="name"
                value={editedClip.metadata.name} 
                onChange={(e) => handleMetadataChange("name", e.target.value)}
                style={{ width: '100%', padding: '5px', marginTop: '5px' }} 
              />
            </div>
            <div style={{ marginTop: '10px' }}>
              <label style={{ fontWeight: 'bold' }}>Type:</label>
              <select 
                value={editedClip.metadata.type} 
                onChange={(e) => handleTypeChange(e.target.value)}
                style={{ width: '100%', padding: '5px', marginTop: '5px' }}
              >
                {Object.keys(config.clipTypes).map(typeName => (
                  <option key={typeName} value={typeName}>{typeName}</option>
                ))}
              </select>
            </div>
            {clipConfig && clipConfig.fields.map(field => (
              <FormField 
                key={field.name}
                field={field}
                value={editedClip.metadata[field.name]}
                onChange={handleMetadataChange}
                allMetadata={editedClip.metadata}
              />
            ))}
          </div>
        )}

        {activeTab === 'characters' && (
          <div>
            {editedClip.characters.map((character, index) => (
              <div key={index} style={{ marginBottom: '15px', border: '1px solid #ddd', padding: '10px', borderRadius: '5px' }}>
                <h5 style={{ marginTop: 0 }}>{character.name}</h5>
                <div style={{ display: 'flex', alignItems: 'center', marginBottom: '10px' }}>
                  <input 
                    type="checkbox" 
                    checked={character.isTalking || false} 
                    onChange={() => handleCharacterChange(index, 'isTalking')}
                    id={`isTalking-${index}`}
                  />
                  <label htmlFor={`isTalking-${index}`} style={{ marginLeft: '5px' }}>Is Talking?</label>
                </div>
                {character.isTalking && (
                  <div>
                    <label>Text:</label>
                    <textarea
                      value={character.text}
                      onChange={(e) => handleCharacterChange(index, 'text', e.target.value)}
                      style={{ width: '100%', minHeight: '60px' }}
                    />
                  </div>
                )}
                <label>Mood:</label>
                <select 
                  value={character.mood || ''} 
                  onChange={(e) => handleCharacterChange(index, 'mood', e.target.value)} 
                  style={{ width: '100%', padding: '5px', marginTop: '5px' }}
                >
                  <option value="">None</option>
                  {config.moods.map(mood => <option key={mood} value={mood}>{mood}</option>)}
                </select>
                <label>Body Behavior:</label>
                <select 
                  value={character.bodyBehavior || ''} 
                  onChange={(e) => handleCharacterChange(index, 'bodyBehavior', e.target.value)} 
                  style={{ width: '100%', padding: '5px', marginTop: '5px' }}
                >
                  <option value="">None</option>
                  {config.bodyBehaviors.map(behavior => <option key={behavior} value={behavior}>{behavior}</option>)}
                </select>
                <label>Reaction:</label>
                <select 
                  value={character.reaction || ''} 
                  onChange={(e) => handleCharacterChange(index, 'reaction', e.target.value)} 
                  style={{ width: '100%', padding: '5px', marginTop: '5px' }}
                >
                  {config.reactions.map(reaction => <option key={reaction} value={reaction}>{reaction}</option>)}
                </select>
              </div>
            ))}
                    <div style={{ marginTop: 'auto', paddingTop: theme.spacing.large }}>
        {validationError && <p style={{ color: theme.colors.danger, textAlign: 'center' }}>{validationError}</p>}
        <button 
          onClick={handleSuggestAnimations} 
          disabled={!!isSuggesting}
          style={{ 
            width: '100%', 
            padding: theme.spacing.medium, 
            backgroundColor: isSuggesting ? theme.colors.secondary : theme.colors.success, 
            color: theme.colors.primaryText, 
            border: 'none', 
            borderRadius: theme.borderRadius,
            cursor: validationError ? 'not-allowed' : 'pointer' 
          }}
        >
          Suggest Animation Picks
        </button>
      </div>
          </div>
        )}
      </div>
      

      <div style={{ marginTop: 'auto', paddingTop: theme.spacing.large }}>
        {validationError && <p style={{ color: theme.colors.danger, textAlign: 'center' }}>{validationError}</p>}
        <button 
          onClick={handleSave} 
          disabled={!!validationError}
          style={{ 
            width: '100%', 
            padding: theme.spacing.medium, 
            backgroundColor: validationError ? theme.colors.secondary : theme.colors.success, 
            color: theme.colors.primaryText, 
            border: 'none', 
            borderRadius: theme.borderRadius,
            cursor: validationError ? 'not-allowed' : 'pointer' 
          }}
        >
          Save Changes
        </button>
      </div>
    </div>
  );
}
