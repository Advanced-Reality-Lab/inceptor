---
sidebar_position: 3
---

# Getting Started: Your First Scene
Welcome to the Inceptor Framework! This guide will walk you through the entire process of creating your first simple, interactive scene. By the end of this tutorial, you will have a working scene that you can build upon.

We'll cover the three main stages:

1. *Installation:* Adding the Inceptor package to your Unity project.

2. *Authoring:* Using the Clip Explorer to write a script and generate a .json file.

3. *Generation:* Using the Inceptor Wizard in Unity to turn your script into a 3D scene.

## Step 1: Installing the Unity Package
The Inceptor Framework is a standard Unity package. The easiest way to install it is directly from its GitHub repository using the Unity Package Manager.

1. Open your Unity project.

2. Go to Window > Package Manager.

3. In the Package Manager window, click the "+" icon in the top-left corner and select "Add package from git URL...".

4. Paste the following URL into the text box and click "Add":

```https://github.com/Advanced-Reality-Lab/inceptor.git```

Unity will now download and install the package. You'll see a new "Inceptor" menu item appear at the top of your screen when it's finished.

## Step 2: Writing Your First Script
Now, let's create the narrative. We'll use the Clip Explorer, our web-based authoring tool.

1. Open the Clip Explorer: [Clip Explorer](https://explorer.useinceptor.com)

2. Open the Text-to-JSON Tool: Click the "Text-to-JSON" button in the toolbar.

3. Write Your Script: A modal will appear with a placeholder script. For this tutorial, let's use a very simple script. Copy and paste the following text into the text area:

```
Nirit: Hello and welcome to your first Inceptor scene!

[CHOICE]
This is your first choice. Which button will you press?
- The left button.
Assistant: You pressed the left button!
- The right button.
Assistant: You pressed the right button!

Nirit: Great job! That's the end of this simple demonstration.
```

4. Convert and Load: Click the "Convert and Load Script" button. You will now see your simple, branching narrative visualized in the Clip Map!

## Step 3: Saving Your Script
With your script loaded in the Clip Explorer, you just need to save it to your computer.

1. Click the "Save Script" button in the toolbar.

2. This will download a file named cinematicScript.json. Save this file somewhere you can easily find it,Preferably the Unity project's Assets folder.

## Step 4: Using the Inceptor Wizard in Unity
Now for the final step: bringing your script to life in Unity.

1. Open the Wizard: Back in your Unity project, go to Inceptor > Import Cinematic Script. This will open the Inceptor Wizard.

2. Import Your Script: The wizard's first step will prompt you to import a file. Click the "Import Script" button and select the cinematicScript.json file you just downloaded.

3. Assign Characters: The wizard will now move to the "Verify Characters" step. It has correctly identified that your script has two characters: "Nirit" and "Assistant." For this test, let's assign simple placeholders:

4. For "Nirit," click the object picker next to "Game Model" and assign a simple Cube primitive.

5. For "Assistant," assign a Sphere primitive.

6. Confirm and Build: Click "Confirm Characters," then "Confirm Animations." Finally, on the "Meta Settings" tab, click the "Complete Wizard" button.

7. Step 5: Press Play!
The wizard will close, and you will now see an Inceptor GameObject and your two character objects (the cube and the sphere) in your scene hierarchy.

8. Press the Play button in Unity. You will see your interactive scene come to life, driven by the script you wrote! Congratulations, you've just created your first Inceptor scene.