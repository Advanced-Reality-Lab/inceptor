import React, { useRef } from 'react';
import { theme } from './theme';

export default function Toolbar({ onNew, onOpen, onSave, onAddClip, onTextToJSON  }) {
  // A ref is used to programmatically click the hidden file input.
  const fileInputRef = useRef(null);

  const handleOpenClick = () => {
    fileInputRef.current.click();
  };

  const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (file) {
      onOpen(file);
    }
    // Reset the input value to allow opening the same file again.
    event.target.value = null; 
  };

  const buttonStyle = {
    padding: '8px 12px',
    border: `1px solid ${theme.colors.border}`,
    backgroundColor: theme.colors.panel,
    color: theme.colors.text,
    cursor: 'pointer',
    borderRadius: theme.borderRadius,
  };

  return (
    <div style={{
      padding: theme.spacing.medium,
      borderBottom: `1px solid ${theme.colors.border}`,
      backgroundColor: theme.colors.background,
      display: 'flex',
      gap: theme.spacing.medium
    }}>
      <button style={buttonStyle} onClick={onNew}>New Script</button>
      <button style={buttonStyle} onClick={handleOpenClick}>Open Script</button>
      <button style={{...buttonStyle, backgroundColor: theme.colors.success, color: theme.colors.primaryText}} onClick={onSave}>Save Script</button>
      <button style={buttonStyle} onClick={onTextToJSON}>Text-to-JSON</button>
      <button style={{...buttonStyle, marginLeft: 'auto'}} onClick={onAddClip}>+ Add Clip</button>
      <input
        type="file"
        ref={fileInputRef}
        onChange={handleFileChange}
        style={{ display: 'none' }}
        accept=".json"
      />
    </div>
  );
}
