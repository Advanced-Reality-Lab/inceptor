---
sidebar_position: 4
---

# Using the Clip Explorer
The Clip Explorer is the heart of the Inceptor authoring experience. It's a powerful web-based tool that allows you to create, visualize, edit, and manage your interactive narratives. This guide provides a detailed tour of its features.

### The Main Interface
The Clip Explorer is divided into three main sections:

- **The Toolbar:** Located at the top, this contains all your global and file management actions.

- **The Clip Map:** The main canvas area where you can see a visual representation of your narrative's structure.

- **The Clip Editor Panel:** A context-sensitive panel that appears on the right when you select a clip, allowing you to edit its details.

### The Toolbar: Managing Your Scripts
The toolbar provides all the high-level controls for your project.

- **New Script:** Clears the current session and starts a new, blank cinematic script.

- **Open Script:** Allows you to upload and load a previously saved .json file from your computer.

- **Save Script:** Downloads the current state of your cinematic script as a cinematicScript.json file, ready to be used in Unity.

- **Text-to-JSON:** Opens a modal where you can paste a simple text script and have it automatically converted into the JSON structure.

- **+ Add Clip:** Adds a new, default LinearClip to your script, which you can then edit.

### The Clip Map: Visualizing Your Narrative
The Clip Map is your primary workspace. It gives you a bird's-eye view of your story's flow.

- **Nodes:** Each box on the map represents a Clip in your script. The label shows the clip's index and its name (e.g., 0: Clip_0_Intro).

- **Edges:** The arrows connecting the nodes show the branching paths of your narrative. A LinearClip will have one outgoing arrow, while a ChoiceClip will have multiple.

- **Interaction:** You can click and drag the nodes to rearrange your map for better clarity. This layout is automatically saved as metadata within your .json file, so it will be preserved the next time you open the script.

### The Clip Editor Panel: Editing the Details
When you click on a node in the Clip Map, the Editor Panel appears on the right. This is where you'll do all the detailed authoring work. The panel is context-aware and will change based on the selected clip's type.

- **The Metadata Tab**
This tab contains the core configuration for the selected clip.

    - **Name:** The unique name for this clip (e.g., "Clip_0_Intro").

    - **Type:** A dropdown that allows you to change the fundamental type of the clip (e.g., from a LinearClip to a QuizClip). The form below will dynamically update to show the fields relevant to the selected type.

    - **Dynamic Fields:** The rest of the form is generated based on your config.json file. For a LinearClip, you'll see a field for the "Next Clip Index." For a ChoiceClip, you'll see the powerful "Choices Editor," allowing you to manage answers and their corresponding next clip indices side-by-side.

- **The Characters Tab**
This tab allows you to define the performance of each character within the selected clip.

    - **Is Talking?:** A checkbox to determine if the character has dialogue in this clip. If checked, a text area will appear for you to write their line.

    - **Dropdowns (Mood, Body Behavior, Reaction):** These dropdowns are populated from your config.json file and allow you to assign specific animations to the character for this clip.

    - **Suggest Animations:** - *Coming Soon!* This powerful button will use AI to analyze the character's dialogue and automatically suggest the most appropriate animations for you, turning your job from creator to curator. 