import React, { useState, useEffect } from 'react';
import { Routes, Route } from 'react-router-dom';
import ClipMap from './ClipMap'; // Import the new component
import ClipEditorPanel from './ClipEditorPanel';
import Toolbar from './Toolbar';
import TextToJSONModal from './TextToJSONModal';


// This is the example JSON data we created.
const initialCinematicScript = {
  "Characters": [
    { "name": "Nirit", "description": "The main character, a friendly guide.", "modelName": "Nirit_Model" },
    { "name": "Assistant", "description": "A helpful AI assistant.", "modelName": "Assistant_Model" }
  ],
  "Clips": [
    { "metadata": { "name": "Clip_0_Intro", "type": "LinearClip", "nextClip": 1 }, "characters": [{ "name": "Nirit", "text": "Welcome! I'm glad you're here.", "isTalking": true }] },
    { "metadata": { "name": "Clip_1_SimpleChoice", "type": "ChoiceClip", "questionText": "Which topic interests you more?", "answers": ["History", "Technology"], "nextClips": [2, 3] }, "characters": [{ "name": "Assistant", "text": "Please make your selection.", "isTalking": true }] },
    { "metadata": { "name": "Clip_2_HistoryPath", "type": "LinearClip", "nextClip": 4 }, "characters": [{ "name": "Nirit", "text": "An excellent choice! This place was founded...", "isTalking": true }] },
    { "metadata": { "name": "Clip_3_TechPath", "type": "LinearClip", "nextClip": 4 }, "characters": [{ "name": "Nirit", "text": "Fascinating! The core technology is based on...", "isTalking": true }] },
    { "metadata": { "name": "Clip_4_Quiz", "type": "QuizClip", "questionText": "What is the primary power source?", "answers": ["Solar", "Geothermal", "Quantum Core"], "nextClips": [5, 5, 5], "correctAnswerIndex": 2 }, "characters": [{ "name": "Assistant", "text": "Time for a quick quiz.", "isTalking": true }] },
    { "metadata": { "name": "Clip_5_Outro", "type": "LinearClip", "nextClip": -1 }, "characters": [{ "name": "Nirit", "text": "Thank you for participating.", "isTalking": true }] }
  ]
};



// The main App component that handles routing
// The main App component that handles routing and state
export default function App() {
  const [cinematicScript, setCinematicScript] = useState(initialCinematicScript);
  const [selectedClipIndex, setSelectedClipIndex] = useState(null);
  const [config, setConfig] = useState(null);
    const [isTextModalOpen, setIsTextModalOpen] = useState(false);



  useEffect(() => {
    fetch('/config.json')
      .then(res => res.json())
      .then(data => setConfig(data))
      .catch(err => console.error("Failed to load config.json", err));
  }, []);

    const handleTextToJSON = (newScript) => {
    setCinematicScript(newScript);
    setSelectedClipIndex(null);
  };
  
  const handleNodeClick = (event, node) => {
    const clipIndex = parseInt(node.id, 10);
    setSelectedClipIndex(clipIndex);
  };

  const handleClosePanel = () => {
    setSelectedClipIndex(null);
  };

  const handleLayoutChange = (nodeId, position) => {
    const clipIndex = parseInt(nodeId, 10);
    const newScript = {
      ...cinematicScript,
      Clips: cinematicScript.Clips.map((clip, i) => {
        if (i === clipIndex) {
          return {
            ...clip,
            metadata: {
              ...clip.metadata,
              layout: { x: position.x, y: position.y }
            }
          };
        }
        return clip;
      }),
    };
    setCinematicScript(newScript);
  };

const blankScript = { Characters: [], Clips: [] }; // A template for a new script

const handleNewScript = () => {
  if (window.confirm("Are you sure you want to start a new script? Any unsaved changes will be lost.")) {
    setCinematicScript(blankScript);
    setSelectedClipIndex(null);
  }
};

const handleSaveScript = () => {
  const jsonString = JSON.stringify(cinematicScript, null, 2);
  const blob = new Blob([jsonString], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = 'cinematicScript.json';
  a.click();
  URL.revokeObjectURL(url);
};

const handleOpenScript = (file) => {
  const reader = new FileReader();
  reader.onload = (e) => {
    try {
      const script = JSON.parse(e.target.result);
      setCinematicScript(script);
      setSelectedClipIndex(null);
    } catch (error) {
      alert("Error: Could not parse the JSON file. Please ensure it is a valid cinematic script.");
    }
  };
  reader.readAsText(file);
};

  const handleClipUpdate = (index, updatedClip) => {
    const newScript = {
      ...cinematicScript,
      Clips: cinematicScript.Clips.map((clip, i) => 
        i === index ? updatedClip : clip
      ),
    };
    setCinematicScript(newScript);
  };

    const handleAddClip = () => {
    const newClip = {
      metadata: {
        name: `Clip_${cinematicScript.Clips.length}`,
        type: 'LinearClip', // Default to the simplest type
        nextClip: -1,
      },
      characters: [],
    };
    const newScript = {
      ...cinematicScript,
      Clips: [...cinematicScript.Clips, newClip],
    };
    setCinematicScript(newScript);
  };


  const selectedClip = selectedClipIndex !== null ? cinematicScript.Clips[selectedClipIndex] : null;

return (
  <div style={{ display: 'flex', flexDirection: 'column', width: '100vw', height: '100vh' }}>
      <Toolbar 
        onNew={handleNewScript}
        onOpen={handleOpenScript}
        onSave={handleSaveScript}
        onAddClip={handleAddClip}
        onTextToJSON={() => setIsTextModalOpen(true)}

      />
    <div style={{ display: 'flex', flexGrow: 1, overflow: 'hidden' }}>
      <div style={{ flexGrow: 1 }}>
        <Routes>
            <Route path="/" element={<ClipMap cinematicScript={cinematicScript} onNodeClick={handleNodeClick} onLayoutChange={handleLayoutChange} />} />
        </Routes>
      </div>
        <ClipEditorPanel 
          clip={selectedClip} 
          clipIndex={selectedClipIndex}
          onUpdate={handleClipUpdate}
          onClose={handleClosePanel} 
          config={config}
        />
    </div>
          {isTextModalOpen && (
        <TextToJSONModal 
          onConvert={handleTextToJSON}
          onClose={() => setIsTextModalOpen(false)}
        />
      )}
  </div>
);
}
