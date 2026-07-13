import { useState, useCallback, useRef, useEffect } from 'react'
import './InputSelector.css'

const InputSelector = ({ inputs, selectedInput, onSelect, onUploadSuccess, disabled }) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isDragging, setIsDragging] = useState(false)
  const [isUploading, setIsUploading] = useState(false)
  const [error, setError] = useState(null)
  const fileInputRef = useRef(null)
  const panelRef = useRef(null)

  // 获取当前选中项的显示名称
  const selectedName = inputs.find(i => i.path === selectedInput)?.name
    || selectedInput?.split('/').pop()
    || 'Select...'

  // 点击外部关闭面板
  useEffect(() => {
    if (!isOpen) return
    const handleClick = (e) => {
      if (panelRef.current && !panelRef.current.contains(e.target)) {
        setIsOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClick)
    return () => document.removeEventListener('mousedown', handleClick)
  }, [isOpen])

  // 选择文件
  const handleSelect = useCallback((path) => {
    onSelect(path)
    setIsOpen(false)
    setError(null)
  }, [onSelect])

  // 拖拽事件
  const handleDrag = useCallback((e) => {
    e.preventDefault()
    e.stopPropagation()
  }, [])

  const handleDragIn = useCallback((e) => {
    e.preventDefault()
    e.stopPropagation()
    // 使用计数器处理子元素事件冒泡
    if (e.currentTarget.contains(e.relatedTarget)) return
    setIsDragging(true)
  }, [])

  const handleDragOut = useCallback((e) => {
    e.preventDefault()
    e.stopPropagation()
    // 只有当拖出整个区域时才取消
    if (e.currentTarget.contains(e.relatedTarget)) return
    setIsDragging(false)
  }, [])

  // 上传文件
  const uploadFile = useCallback(async (file) => {
    if (!file.name.endsWith('.md')) {
      setError('仅支持 .md 文件')
      return
    }

    setIsUploading(true)
    setError(null)

    try {
      const formData = new FormData()
      formData.append('file', file)

      const res = await fetch('/api/upload', {
        method: 'POST',
        body: formData,
      })

      const data = await res.json()

      if (data.success) {
        onUploadSuccess?.(data)
        setIsOpen(false)
      } else {
        setError(data.detail || '上传失败')
      }
    } catch (err) {
      setError('上传失败: ' + err.message)
    } finally {
      setIsUploading(false)
      setIsDragging(false)
    }
  }, [onUploadSuccess])

  const handleDrop = useCallback((e) => {
    e.preventDefault()
    e.stopPropagation()
    setIsDragging(false)

    const files = e.dataTransfer?.files
    if (files?.length > 0) {
      uploadFile(files[0])
    }
  }, [uploadFile])

  const handleFileSelect = useCallback((e) => {
    const file = e.target.files?.[0]
    if (file) {
      uploadFile(file)
    }
    e.target.value = ''
  }, [uploadFile])

  return (
    <div className="input-selector" ref={panelRef}>
      <div className="topbar-select-group">
        <label className="topbar-label">Input:</label>

        {/* 触发按钮 */}
        <button
          className="input-selector-trigger"
          onClick={() => !disabled && setIsOpen(!isOpen)}
          disabled={disabled}
          type="button"
        >
          <span className="input-selector-value">{selectedName}</span>
          <span className="input-selector-arrow">{isOpen ? '▲' : '▼'}</span>
        </button>
      </div>

      {/* 下拉面板 */}
      {isOpen && (
        <div className="input-selector-panel">
          {/* 文件列表 */}
          <div className="input-selector-list">
            {inputs.length === 0 ? (
              <div className="input-selector-empty">暂无文件，请拖入 .md 文件</div>
            ) : (
              inputs.map((input, index) => (
                <div
                  key={index}
                  className={`input-selector-item ${input.path === selectedInput ? 'active' : ''}`}
                  onClick={() => handleSelect(input.path)}
                >
                  <span className="input-selector-item-icon">📄</span>
                  <span className="input-selector-item-name">{input.name}</span>
                  <span className="input-selector-item-path">{input.directory}</span>
                </div>
              ))
            )}
          </div>

          {/* 底部拖拽上传区域 */}
          <div
            className={`input-selector-dropzone ${isDragging ? 'dragging' : ''} ${isUploading ? 'uploading' : ''}`}
            onDragEnter={handleDragIn}
            onDragLeave={handleDragOut}
            onDragOver={handleDrag}
            onDrop={handleDrop}
            onClick={() => fileInputRef.current?.click()}
          >
            <input
              ref={fileInputRef}
              type="file"
              accept=".md"
              onChange={handleFileSelect}
              style={{ display: 'none' }}
            />
            {isUploading ? (
              <>
                <div className="dropzone-spinner" />
                <span>上传中...</span>
              </>
            ) : isDragging ? (
              <>
                <span>📥</span>
                <span>释放以上传</span>
              </>
            ) : (
              <>
                <span>📄</span>
                <span>拖入 .md 文件或点击选择</span>
              </>
            )}
          </div>

          {/* 错误提示 */}
          {error && <div className="input-selector-error">{error}</div>}
        </div>
      )}
    </div>
  )
}

export default InputSelector
