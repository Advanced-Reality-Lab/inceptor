import React, { useCallback, useEffect } from 'react';
import ReactFlow, {
  MiniMap,
  Controls,
  Background,
  useNodesState,
  useEdgesState,
  addEdge,
} from 'reactflow';

import 'reactflow/dist/style.css';

// A helper function to transform our JSON into the format React Flow needs.
const transformScriptToFlow = (script) => {
  if (!script || !script.Clips) {
    return { initialNodes: [], initialEdges: [] };
  }

  const nodes = [];
  const edges = [];

  script.Clips.forEach((clip, index) => {
    // FIX: Add the clip index to the node label for clarity.
    const label = `[${index}] ${clip.metadata.name} (${clip.metadata.type})`;
    
    const position = clip.metadata.layout
      ? { x: clip.metadata.layout.x, y: clip.metadata.layout.y }
      : { x: (index % 4) * 250, y: Math.floor(index / 4) * 150 };

    nodes.push({
      id: `${index}`,
      type: 'default',
      data: { label: label },
      position: position,
    });

    if (clip.metadata.type === "LinearClip" && clip.metadata.nextClip !== -1) {
      edges.push({
        id: `e${index}-${clip.metadata.nextClip}`,
        source: `${index}`,
        target: `${clip.metadata.nextClip}`,
        animated: true,
      });
    } else if (clip.metadata.type === "ChoiceClip" || clip.metadata.type === "QuizClip") {
      clip.metadata.nextClips.forEach((nextClipIndex, answerIndex) => {
        if (nextClipIndex !== -1) {
          edges.push({
            id: `e${index}-${nextClipIndex}-${answerIndex}`,
            source: `${index}`,
            target: `${nextClipIndex}`,
            label: `[${clip.metadata.answers[answerIndex]}]`,
            animated: true,
          });
        }
      });
    }
  });

  return { nodes, edges };
};

export default function ClipMap({ cinematicScript, onNodeClick, onLayoutChange }) {
  const [nodes, setNodes, onNodesChange] = useNodesState([]);
  const [edges, setEdges, onEdgesChange] = useEdgesState([]);

  // Use a useEffect hook to update the nodes and edges whenever the script changes.
  useEffect(() => {
    const { nodes: newNodes, edges: newEdges } = transformScriptToFlow(cinematicScript);
    setNodes(newNodes);
    setEdges(newEdges);
  }, [cinematicScript, setNodes, setEdges]); // This effect runs when cinematicScript changes

  const onConnect = useCallback((params) => setEdges((eds) => addEdge(params, eds)), [setEdges]);

  // This function is called when a user finishes dragging a node.
  const handleNodeDragStop = (event, node) => {
    if (onLayoutChange) {
      onLayoutChange(node.id, node.position);
    }
  };

  return (
    <div style={{ height: '100%', width: '100%' }}>
      <ReactFlow
        nodes={nodes}
        edges={edges}
        onNodesChange={onNodesChange}
        onEdgesChange={onEdgesChange}
        onConnect={onConnect}
        onNodeClick={onNodeClick}
        onNodeDragStop={handleNodeDragStop} // This line was added
        nodesConnectable={false}
        fitView
      >
        <MiniMap />
        <Controls />
        <Background />
      </ReactFlow>
    </div>
  );
}