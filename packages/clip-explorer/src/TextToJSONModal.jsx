import React, { useState } from 'react';

const placeholderText = `
# Lines starting with # are comments and are ignored.
# The parser will automatically handle non-speaking characters.

Nirit: Welcome to the Inceptor test drive!

[CHOICE]
Which system do you find more interesting so far?
- The Unity refactor
Assistant: An excellent choice! The refactoring in Unity was designed to make the framework robust and extensible.
- The React Clip Explorer
Assistant: A great pick! The Clip Explorer is designed to make the authoring process intuitive and powerful.

Nirit: Now that you've seen a simple branch, let's try a quiz.

[QUIZ] (correct: 1)
What is the primary benefit of the new refactored architecture?
- It's faster
Nirit: While performance is a nice side effect, that's not the main goal.
[FEEDBACK] Not quite! The main goal was architectural clarity.
- It's more modular and decoupled
Nirit: That's exactly right!
[FEEDBACK] Correct! Decoupling the components makes the system much easier to maintain and extend.
- It uses fewer files
Nirit: Actually, the refactor resulted in more files, but they are much better organized.
[FEEDBACK] Incorrect. The new design uses more, smaller files for better organization.

Assistant: The test is now complete. Thank you for participating!
`.trim();

const parseTextToJSON = (text) => {
    const lines = text.split('\n').filter(line => line.trim() !== '' && !line.trim().startsWith('#'));
    const finalClips = [];
    let clipIndex = 0;

    // --- First Pass: Identify all unique character names ---
    const characterNames = new Set();
    lines.forEach(line => {
        const parts = line.split(':');
        if (parts.length > 1 && !line.trim().startsWith('-') && !line.trim().startsWith('[')) {
            characterNames.add(parts[0].trim());
        }
    });
    const allCharactersList = Array.from(characterNames);

    // --- Second Pass: Build the clips ---
    let i = 0;
    while (i < lines.length) {
        const line = lines[i].trim();

        if (line.toUpperCase().startsWith('[CHOICE]') || line.toUpperCase().startsWith('[QUIZ]')) {
            const isQuiz = line.toUpperCase().startsWith('[QUIZ]');
            let correctAnswerIndex = -1;
            if (isQuiz) {
                const match = line.match(/\(correct:\s*(\d+)\)/);
                if (match) { correctAnswerIndex = parseInt(match[1], 10); }
            }

            i++; // Move to question line
            const questionText = lines[i]?.trim() || 'Default Question';
            i++; // Move to first answer

            const answers = [];
            const responseClipIndices = [];
            const feedbacks = [];
            const choiceClipIndex = clipIndex;
            
            const choiceClip = {
                metadata: { name: `Clip_${clipIndex}`, type: isQuiz ? 'QuizClip' : 'ChoiceClip', questionText, answers: [], nextClips: [] },
                characters: allCharactersList.map(name => ({ name, text: '', isTalking: false, mood: 'Neutral', bodyBehavior: 'Idle', reaction: 'None' })),
            };
            if(isQuiz) {
                choiceClip.metadata.correctAnswerIndex = correctAnswerIndex;
                choiceClip.metadata.feedback = [];
            }
            finalClips.push(choiceClip);
            clipIndex++;

            while (i < lines.length && lines[i].trim().startsWith('-')) {
                answers.push(lines[i].trim().substring(1).trim());
                i++; // Move to the response dialogue line
                
                responseClipIndices.push(clipIndex);
                const dialogueLine = lines[i]?.trim() || 'Character: Default response.';
                const parts = dialogueLine.split(':');
                const speakingCharName = parts[0].trim();
                const dialogueText = parts.slice(1).join(':').trim();

                const responseCharacters = allCharactersList.map(name => ({
                    name: name,
                    text: name === speakingCharName ? dialogueText : '',
                    isTalking: name === speakingCharName,
                    mood: 'Neutral', bodyBehavior: 'Idle', reaction: 'None'
                }));

                finalClips.push({
                    metadata: { name: `Clip_${clipIndex}`, type: 'LinearClip', nextClip: -1 }, // Placeholder
                    characters: responseCharacters
                });
                clipIndex++;
                i++;

                if (isQuiz) {
                    if (i < lines.length && lines[i].trim().toUpperCase().startsWith('[FEEDBACK]')) {
                        feedbacks.push(lines[i].trim().substring(10).trim());
                        i++;
                    } else {
                        feedbacks.push(dialogueText); // Default feedback
                    }
                }
            }
            
            finalClips[choiceClipIndex].metadata.answers = answers;
            finalClips[choiceClipIndex].metadata.nextClips = responseClipIndices;
            if(isQuiz) {
                finalClips[choiceClipIndex].metadata.feedback = feedbacks;
            }

            const nextMajorClipIndex = clipIndex;
            responseClipIndices.forEach(idx => {
                finalClips[idx].metadata.nextClip = nextMajorClipIndex;
            });

        } else { // It's a linear clip
            const parts = line.split(':');
            const speakingCharName = parts[0].trim();
            const dialogueText = parts.slice(1).join(':').trim();
            
            const clipCharacters = allCharactersList.map(name => ({
                name: name,
                text: name === speakingCharName ? dialogueText : '',
                isTalking: name === speakingCharName,
                mood: 'Neutral', bodyBehavior: 'Idle', reaction: 'None'
            }));

            finalClips.push({
                metadata: { name: `Clip_${clipIndex}`, type: 'LinearClip', nextClip: clipIndex + 1 },
                characters: clipCharacters
            });
            clipIndex++;
            i++;
        }
    }

    if (finalClips.length > 0) {
        const lastClip = finalClips[finalClips.length - 1];
        if (lastClip.metadata.type === 'LinearClip') {
            lastClip.metadata.nextClip = -1;
        }
    }

    const characters = allCharactersList.map(name => ({ name, description: '', modelName: '' }));
    return { Characters: characters, Clips: finalClips };
};


export default function TextToJSONModal({ onConvert, onClose }) {
  const [text, setText] = useState(placeholderText);

  const handleConvert = () => {
    try {
      const newScript = parseTextToJSON(text);
      onConvert(newScript);
      onClose();
    } catch (error) {
      alert(`Error parsing script: ${error.message}`);
    }
  };

  return (
    <div style={{
      position: 'fixed', top: 0, left: 0, width: '100%', height: '100%',
      backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex',
      justifyContent: 'center', alignItems: 'center', zIndex: 1000
    }}>
      <div style={{ backgroundColor: 'white', padding: '20px', borderRadius: '5px', width: '600px', maxHeight: '90vh', display: 'flex', flexDirection: 'column' }}>
        <h2>Text-to-JSON Converter</h2>
        <p>Write your script below using the specified format. The system will convert it into a JSON structure.</p>
        <textarea
          value={text}
          onChange={(e) => setText(e.target.value)}
          style={{ width: '100%', flexGrow: 1, fontFamily: 'monospace', minHeight: '200px' }}
        />
        <div style={{ marginTop: '10px', display: 'flex', justifyContent: 'flex-end', gap: '10px' }}>
          <button onClick={onClose}>Cancel</button>
          <button onClick={handleConvert} style={{ backgroundColor: '#4CAF50', color: 'white' }}>Convert</button>
        </div>
      </div>
    </div>
  );
}
