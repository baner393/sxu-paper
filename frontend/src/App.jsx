import { useState, useEffect, useCallback } from 'react'
import TopBar from './components/TopBar'
import ConfigPanel from './components/ConfigPanel'
import PdfFlipbook from './components/PdfFlipbook'
import './App.css'

// 默认配置（山西财经大学标准格式）
const defaultConfig = {
  page: {
    width_cm: 21.0,
    height_cm: 29.7,
    margin_top_cm: 3.0,
    margin_bottom_cm: 2.5,
    margin_left_cm: 2.5,
    margin_right_cm: 2.0
  },
  fonts: {
    body_east_asian: 'SimSun',
    body_latin: 'Times New Roman',
    body_size_pt: 12,
    heading1_font: 'SimHei',
    heading1_size_pt: 16,
    heading2_font: 'SimSun',
    heading2_size_pt: 14,
    heading2_bold: true,
    heading3_font: 'SimSun',
    heading3_size_pt: 12,
    heading3_bold: true,
    abstract_title_font: 'SimSun',
    abstract_title_size_pt: 18,
    eng_abstract_title_font: 'Times New Roman',
    eng_abstract_title_size_pt: 16,
    keywords_label_font: 'SimHei',
    keywords_label_size_pt: 14,
    ref_font: 'SimSun',
    ref_size_pt: 12,
    caption_font: 'FangSong',
    caption_size_pt: 10.5,
    header_font_east_asian: 'SimSun',
    header_font_latin: 'Times New Roman',
    header_size_pt: 9,
    footer_font_east_asian: 'SimSun',
    footer_font_latin: 'Times New Roman',
    footer_size_pt: 9
  },
  spacing: {
    line_spacing: 1.25,
    heading1_before_pt: 18,
    heading1_after_pt: 18,
    heading2_before_pt: 0,
    heading2_after_pt: 0,
    heading3_before_pt: 0,
    heading3_after_pt: 0,
    abstract_before_pt: 18,
    abstract_after_pt: 18,
    keywords_before_pt: 18,
    first_line_indent_chars: 2,
    ref_hanging_indent_chars: 2,
    caption_after_pt: 6
  },
  header: {
    text_template: '山西财经大学{grade}级本科生学年论文',
    odd_align: 'right',
    even_align: 'left',
    show: true
  },
  footer: {
    show_page_number: true,
    abstract_format: 'roman_upper',
    body_format: 'decimal',
    abstract_align: 'center',
    odd_align: 'right',
    even_align: 'left'
  },
  sections: {
    cover: true,
    abstract_cn: true,
    abstract_en: true,
    toc: true,
    body: true,
    references: true,
    appendix: true,
    acknowledgment: true,
    back_cover: true
  }
}

function App() {
  // 状态管理
  const [config, setConfig] = useState(defaultConfig)
  const [theme, setTheme] = useState(() => {
    const saved = localStorage.getItem('theme')
    return saved || 'light'
  })
  const [configPanelCollapsed, setConfigPanelCollapsed] = useState(false)
  const [selectedInput, setSelectedInput] = useState('')
  const [selectedTemplate, setSelectedTemplate] = useState('')
  const [inputs, setInputs] = useState([])
  const [templates, setTemplates] = useState([])
  const [pdfUrl, setPdfUrl] = useState('')
  const [totalPages, setTotalPages] = useState(0)
  const [isConverting, setIsConverting] = useState(false)
  const [isPreviewing, setIsPreviewing] = useState(false)
  const [error, setError] = useState(null)
  const [previewTitle, setPreviewTitle] = useState('')

  // 应用主题
  useEffect(() => {
    document.documentElement.setAttribute('data-theme', theme)
    localStorage.setItem('theme', theme)
  }, [theme])

  // 获取输入文件列表（可重复调用）
  const fetchInputs = useCallback(async () => {
    try {
      const res = await fetch('/api/inputs')
      if (res.ok) {
        const data = await res.json()
        setInputs(data.inputs || [])
        return data.inputs || []
      }
    } catch (err) {
      console.error('获取输入列表失败:', err)
    }
    return []
  }, [])

  // 获取模板列表
  const fetchTemplates = useCallback(async () => {
    try {
      const res = await fetch('/api/templates')
      if (res.ok) {
        const data = await res.json()
        setTemplates(data.templates || [])
        // 只在未选择模板时自动选中第一个
        setSelectedTemplate(prev => {
          if (!prev && data.templates?.length > 0) {
            return data.templates[0].path || data.templates[0]
          }
          return prev
        })
      }
    } catch (err) {
      console.error('获取模板列表失败:', err)
    }
  }, [])

  // 初始化加载
  useEffect(() => {
    fetchInputs().then(list => {
      if (list.length > 0) {
        setSelectedInput(list[0].path || list[0])
      }
    })
    fetchTemplates()
  }, [fetchInputs, fetchTemplates])

  // 上传成功回调
  const handleUploadSuccess = useCallback((data) => {
    fetchInputs().then(() => {
      // 自动选中新上传的文件
      if (data.path) {
        setSelectedInput(data.path)
      }
    })
  }, [fetchInputs])

  // 更新配置
  const updateConfig = useCallback((section, key, value) => {
    setConfig(prev => ({
      ...prev,
      [section]: {
        ...prev[section],
        [key]: value
      }
    }))
  }, [])

  // 切换主题
  const toggleTheme = useCallback(() => {
    setTheme(prev => prev === 'light' ? 'dark' : 'light')
  }, [])

  // 转换文档
  const handleConvert = useCallback(async () => {
    if (!selectedInput || !selectedTemplate) {
      setError('请选择输入文件和模板')
      return
    }

    setIsConverting(true)
    setError(null)

    try {
      const response = await fetch('/api/convert', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          input_md: selectedInput,
          template_path: selectedTemplate,
          config: config
        })
      })

      if (!response.ok) {
        const errData = await response.json().catch(() => ({}))
        throw new Error(errData.detail || `HTTP ${response.status}`)
      }

      const data = await response.json()

      if (data.success) {
        alert(`转换成功！\n输出文件: ${data.output_path}\n页数: ${data.pages}`)
      } else {
        setError(data.error || '转换失败')
      }
    } catch (err) {
      setError('转换请求失败: ' + err.message)
    } finally {
      setIsConverting(false)
    }
  }, [selectedInput, selectedTemplate, config])

  // 预览文档
  const handlePreview = useCallback(async () => {
    if (!selectedInput || !selectedTemplate) {
      setError('请选择输入文件和模板')
      return
    }

    setIsPreviewing(true)
    setError(null)
    setPdfUrl('')
    setTotalPages(0)

    try {
      const response = await fetch('/api/preview', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          input_md: selectedInput,
          template_path: selectedTemplate,
          config: config
        })
      })

      if (!response.ok) {
        const errData = await response.json().catch(() => ({}))
        throw new Error(errData.detail || `HTTP ${response.status}`)
      }

      const data = await response.json()

      if (data.success) {
        setPdfUrl(data.pdf_url || '')
        setTotalPages(data.total_pages || 0)
        setPreviewTitle(selectedInput.split('/').pop().replace('.md', ''))
      } else {
        setError(data.error || '预览生成失败')
      }
    } catch (err) {
      setError('预览请求失败: ' + err.message)
    } finally {
      setIsPreviewing(false)
    }
  }, [selectedInput, selectedTemplate, config])

  // 关闭预览
  const closePreview = useCallback(() => {
    setPdfUrl('')
    setTotalPages(0)
  }, [])

  return (
    <div className="app-container">
      <TopBar
        theme={theme}
        toggleTheme={toggleTheme}
        inputs={inputs}
        templates={templates}
        selectedInput={selectedInput}
        selectedTemplate={selectedTemplate}
        onInputChange={setSelectedInput}
        onTemplateChange={setSelectedTemplate}
        onConvert={handleConvert}
        onPreview={handlePreview}
        isConverting={isConverting}
        isPreviewing={isPreviewing}
        onUploadSuccess={handleUploadSuccess}
      />

      <div className="main-content">
        <ConfigPanel
          config={config}
          updateConfig={updateConfig}
          collapsed={configPanelCollapsed}
          onToggle={() => setConfigPanelCollapsed(!configPanelCollapsed)}
        />

        <PdfFlipbook
          pdfUrl={pdfUrl}
          totalPages={totalPages}
          isLoading={isPreviewing}
          error={error}
          onClose={closePreview}
        />
      </div>
    </div>
  )
}

export default App
