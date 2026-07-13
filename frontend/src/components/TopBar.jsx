import { memo } from 'react'
import InputSelector from './InputSelector'
import './TopBar.css'

const TopBar = memo(({
  theme,
  toggleTheme,
  inputs,
  templates,
  selectedInput,
  selectedTemplate,
  onInputChange,
  onTemplateChange,
  onConvert,
  onPreview,
  isConverting,
  isPreviewing,
  onUploadSuccess
}) => {
  return (
    <header className="topbar">
      <div className="topbar-left">
        <h1 className="topbar-title">ThesisBuilder</h1>
      </div>

      <div className="topbar-center">
        <InputSelector
          inputs={inputs}
          selectedInput={selectedInput}
          onSelect={onInputChange}
          onUploadSuccess={onUploadSuccess}
          disabled={isConverting || isPreviewing}
        />

        <div className="topbar-select-group">
          <label className="topbar-label">Template:</label>
          <select
            className="topbar-select"
            value={selectedTemplate}
            onChange={(e) => onTemplateChange(e.target.value)}
            disabled={isConverting || isPreviewing}
          >
            <option value="">Select...</option>
            {templates.map((template, index) => (
              <option key={index} value={template.path || template}>
                {template.name || (typeof template === 'string' ? template.split('/').pop() : '')}
              </option>
            ))}
          </select>
        </div>
      </div>

      <div className="topbar-right">
        <button
          className="topbar-btn topbar-btn-primary"
          onClick={onConvert}
          disabled={isConverting || isPreviewing || !selectedInput || !selectedTemplate}
        >
          {isConverting ? 'Converting...' : 'Convert'}
        </button>

        <button
          className="topbar-btn topbar-btn-secondary"
          onClick={onPreview}
          disabled={isConverting || isPreviewing || !selectedInput || !selectedTemplate}
        >
          {isPreviewing ? 'Previewing...' : 'Preview'}
        </button>

        <button
          className="topbar-btn topbar-btn-icon"
          onClick={toggleTheme}
          title="Toggle theme"
        >
          {theme === 'light' ? '🌙' : '☀️'}
        </button>
      </div>
    </header>
  )
})

TopBar.displayName = 'TopBar'

export default TopBar
