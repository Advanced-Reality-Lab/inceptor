---
sidebar_position: 2
--- 

# Core Concepts & Philosophy

The Inceptor Framework is built on a simple but powerful architectural philosophy designed to be both easy to understand and highly extensible. The core idea is to model the process of creating an interactive scene after a professional film set.

### The ```Inceptor``` Component (The Director)
At the heart of every Unity scene is the Inceptor component. This MonoBehaviour acts as the Director on your film set. The Director's job is to run the set efficiently: they call "Action!", ensure the production follows the screenplay, and manage the high-level flow of the story. The Inceptor is in charge, but it doesn't get involved in the low-level performance of any single character.

### The ```CinematicScript``` (The Screenplay)
The CinematicScript is the .json file you create in the Clip Explorer. It is the complete Screenplay for your interactive scene. It contains all the information about the characters, the sequence of events (the clips), and the branching logic.

### Clips (The Scenes)
Each individual action in your story - a line of dialogue, a user choice, a quiz - is a Clip. Think of each Clip as a single Scene on a call sheet. They are self-contained ScriptableObject assets that know how to execute one specific part of the story. The Inceptor (the Director) simply calls for the next scene, and the Clip handles its own performance from start to finish.

### Analyzers
When a scene requires a decision based on user input, it uses an Analyzer. The analyzer is a reactive interpreter of the screenplay's branching paths. Its job is to take the user's input during a scene and determine which of the pre-defined paths the story should follow, ensuring the narrative continues as written.

### Co-Directors (The AI Storyteller) - *Coming soon!*
For more advanced, generative storytelling, the framework will support Co-Directors. A Co-Director is a proactive AI service that acts as your creative partner. It can look at the story so far and the screenplay for the next scene, and it has the freedom to give the actors new "performance notes" (like overriding an animation) or even change which scene is shot next to make the story more dynamic.

## Our Design Philosophy
This entire structure is guided by two core values:

- **Simplicity for the Non-Expert:**
Our primary goal is to empower users who are not expert programmers. The systems are designed to be intuitive and to hide as much technical complexity as possible.

- **Extensibility for the Power User:**
Every core part of the system is built on a foundation of interfaces and ScriptableObjects, making it easy for experienced developers to extend the framework with their own custom functionality without having to modify the core code.