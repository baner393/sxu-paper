import { useState, memo, useCallback } from 'react'
import './ConfigPanel.css'

// 配置分组组件
const ConfigSection = memo(({ title, children, defaultExpanded = true }) => {
  const [expanded, setExpanded] = useState(defaultExpanded)

  return (
    <div className="config-section">
      <div
        className="config-section-header"
        onClick={() => setExpanded(!expanded)}
      >
        <span className="config-section-title">{title}</span>
        <span className={`config-section-arrow ${expanded ? 'expanded' : ''}`}>
          ▼
        </span>
      </div>
      {expanded && (
        <div className="config-section-content">
          {children}
        </div>
      )}
    </div>
  )
})

ConfigSection.displayName = 'ConfigSection'

// 输入框组件
const ConfigInput = memo(({ label, value, onChange, type = 'text', min, max, step, unit, tooltip }) => {
  return (
    <div className="config-item" title={tooltip}>
      <label className="config-label">{label}</label>
      <div className="config-input-wrapper">
        <input
          type={type}
          value={value}
          onChange={(e) => onChange(type === 'number' ? (parseFloat(e.target.value) || 0) : e.target.value)}
          min={min}
          max={max}
          step={step}
          className="config-input"
        />
        {unit && <span className="config-unit">{unit}</span>}
      </div>
    </div>
  )
})

ConfigInput.displayName = 'ConfigInput'

// 选择框组件
const ConfigSelect = memo(({ label, value, onChange, options, tooltip }) => {
  return (
    <div className="config-item" title={tooltip}>
      <label className="config-label">{label}</label>
      <select
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="config-select"
      >
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
    </div>
  )
})

ConfigSelect.displayName = 'ConfigSelect'

// 开关组件
const ConfigSwitch = memo(({ label, checked, onChange, tooltip }) => {
  return (
    <div className="config-item config-switch-item" title={tooltip}>
      <label className="config-label">{label}</label>
      <label className="config-switch">
        <input
          type="checkbox"
          checked={checked}
          onChange={(e) => onChange(e.target.checked)}
        />
        <span className="config-switch-slider"></span>
      </label>
    </div>
  )
})

ConfigSwitch.displayName = 'ConfigSwitch'

// 对齐选项
const alignOptions = [
  { value: 'left', label: '左对齐' },
  { value: 'center', label: '居中' },
  { value: 'right', label: '右对齐' }
]

// 页码格式选项
const pageFormatOptions = [
  { value: 'decimal', label: '阿拉伯数字 (1, 2, 3)' },
  { value: 'roman_upper', label: '大写罗马 (I, II, III)' },
  { value: 'roman_lower', label: '小写罗马 (i, ii, iii)' }
]

const ConfigPanel = memo(({ config, updateConfig, collapsed, onToggle }) => {
  // 更新页面设置
  const updatePage = useCallback((key, value) => {
    updateConfig('page', key, value)
  }, [updateConfig])

  // 更新字体设置
  const updateFonts = useCallback((key, value) => {
    updateConfig('fonts', key, value)
  }, [updateConfig])

  // 更新间距设置
  const updateSpacing = useCallback((key, value) => {
    updateConfig('spacing', key, value)
  }, [updateConfig])

  // 更新页眉设置
  const updateHeader = useCallback((key, value) => {
    updateConfig('header', key, value)
  }, [updateConfig])

  // 更新页脚设置
  const updateFooter = useCallback((key, value) => {
    updateConfig('footer', key, value)
  }, [updateConfig])

  // 更新章节开关
  const updateSection = useCallback((key, value) => {
    updateConfig('sections', key, value)
  }, [updateConfig])

  return (
    <aside className={`config-panel ${collapsed ? 'collapsed' : ''}`}>
      <div className="config-panel-header">
        <span className="config-panel-title">格式配置</span>
        <button
          className="config-panel-toggle"
          onClick={onToggle}
          title={collapsed ? '展开面板' : '折叠面板'}
        >
          {collapsed ? '▶' : '◀'}
        </button>
      </div>

      {!collapsed && (
        <div className="config-panel-content">
          {/* 页面设置 */}
          <ConfigSection title="页面设置">
            <ConfigInput
              label="纸张宽度"
              value={config.page.width_cm}
              onChange={(v) => updatePage('width_cm', v)}
              type="number"
              min={10}
              max={30}
              step={0.1}
              unit="cm"
            />
            <ConfigInput
              label="纸张高度"
              value={config.page.height_cm}
              onChange={(v) => updatePage('height_cm', v)}
              type="number"
              min={15}
              max={40}
              step={0.1}
              unit="cm"
            />
            <ConfigInput
              label="上边距"
              value={config.page.margin_top_cm}
              onChange={(v) => updatePage('margin_top_cm', v)}
              type="number"
              min={1}
              max={5}
              step={0.1}
              unit="cm"
            />
            <ConfigInput
              label="下边距"
              value={config.page.margin_bottom_cm}
              onChange={(v) => updatePage('margin_bottom_cm', v)}
              type="number"
              min={1}
              max={5}
              step={0.1}
              unit="cm"
            />
            <ConfigInput
              label="左边距"
              value={config.page.margin_left_cm}
              onChange={(v) => updatePage('margin_left_cm', v)}
              type="number"
              min={1}
              max={5}
              step={0.1}
              unit="cm"
            />
            <ConfigInput
              label="右边距"
              value={config.page.margin_right_cm}
              onChange={(v) => updatePage('margin_right_cm', v)}
              type="number"
              min={1}
              max={5}
              step={0.1}
              unit="cm"
            />
          </ConfigSection>

          {/* 字体设置 */}
          <ConfigSection title="字体设置" defaultExpanded={false}>
            <ConfigInput
              label="正文中文字体"
              value={config.fonts.body_east_asian}
              onChange={(v) => updateFonts('body_east_asian', v)}
              tooltip="如：SimSun, SimHei, KaiTi"
            />
            <ConfigInput
              label="正文西文字体"
              value={config.fonts.body_latin}
              onChange={(v) => updateFonts('body_latin', v)}
              tooltip="如：Times New Roman, Arial"
            />
            <ConfigInput
              label="正文字号"
              value={config.fonts.body_size_pt}
              onChange={(v) => updateFonts('body_size_pt', v)}
              type="number"
              min={8}
              max={24}
              step={0.5}
              unit="pt"
            />
            <ConfigInput
              label="一级标题字体"
              value={config.fonts.heading1_font}
              onChange={(v) => updateFonts('heading1_font', v)}
            />
            <ConfigInput
              label="一级标题字号"
              value={config.fonts.heading1_size_pt}
              onChange={(v) => updateFonts('heading1_size_pt', v)}
              type="number"
              min={12}
              max={24}
              step={0.5}
              unit="pt"
            />
            <ConfigInput
              label="二级标题字体"
              value={config.fonts.heading2_font}
              onChange={(v) => updateFonts('heading2_font', v)}
            />
            <ConfigInput
              label="二级标题字号"
              value={config.fonts.heading2_size_pt}
              onChange={(v) => updateFonts('heading2_size_pt', v)}
              type="number"
              min={10}
              max={20}
              step={0.5}
              unit="pt"
            />
            <ConfigSwitch
              label="二级标题加粗"
              checked={config.fonts.heading2_bold}
              onChange={(v) => updateFonts('heading2_bold', v)}
            />
            <ConfigInput
              label="三级标题字体"
              value={config.fonts.heading3_font}
              onChange={(v) => updateFonts('heading3_font', v)}
            />
            <ConfigInput
              label="三级标题字号"
              value={config.fonts.heading3_size_pt}
              onChange={(v) => updateFonts('heading3_size_pt', v)}
              type="number"
              min={10}
              max={18}
              step={0.5}
              unit="pt"
            />
            <ConfigSwitch
              label="三级标题加粗"
              checked={config.fonts.heading3_bold}
              onChange={(v) => updateFonts('heading3_bold', v)}
            />
            <ConfigInput
              label="摘要标题字体"
              value={config.fonts.abstract_title_font}
              onChange={(v) => updateFonts('abstract_title_font', v)}
            />
            <ConfigInput
              label="摘要标题字号"
              value={config.fonts.abstract_title_size_pt}
              onChange={(v) => updateFonts('abstract_title_size_pt', v)}
              type="number"
              min={14}
              max={22}
              step={0.5}
              unit="pt"
            />
            <ConfigInput
              label="英文摘要标题字体"
              value={config.fonts.eng_abstract_title_font}
              onChange={(v) => updateFonts('eng_abstract_title_font', v)}
              tooltip="如：Times New Roman, Arial"
            />
            <ConfigInput
              label="英文摘要标题字号"
              value={config.fonts.eng_abstract_title_size_pt}
              onChange={(v) => updateFonts('eng_abstract_title_size_pt', v)}
              type="number"
              min={14}
              max={22}
              step={0.5}
              unit="pt"
            />
            <ConfigInput
              label="关键词标签字体"
              value={config.fonts.keywords_label_font}
              onChange={(v) => updateFonts('keywords_label_font', v)}
            />
            <ConfigInput
              label="关键词标签字号"
              value={config.fonts.keywords_label_size_pt}
              onChange={(v) => updateFonts('keywords_label_size_pt', v)}
              type="number"
              min={10}
              max={18}
              step={0.5}
              unit="pt"
            />
            <ConfigInput
              label="参考文献字体"
              value={config.fonts.ref_font}
              onChange={(v) => updateFonts('ref_font', v)}
            />
            <ConfigInput
              label="参考文献字号"
              value={config.fonts.ref_size_pt}
              onChange={(v) => updateFonts('ref_size_pt', v)}
              type="number"
              min={8}
              max={16}
              step={0.5}
              unit="pt"
            />
            <ConfigInput
              label="图片题注字体"
              value={config.fonts.caption_font}
              onChange={(v) => updateFonts('caption_font', v)}
            />
            <ConfigInput
              label="图片题注字号"
              value={config.fonts.caption_size_pt}
              onChange={(v) => updateFonts('caption_size_pt', v)}
              type="number"
              min={8}
              max={14}
              step={0.5}
              unit="pt"
            />
            <ConfigInput
              label="页眉中文字体"
              value={config.fonts.header_font_east_asian}
              onChange={(v) => updateFonts('header_font_east_asian', v)}
            />
            <ConfigInput
              label="页眉西文字体"
              value={config.fonts.header_font_latin}
              onChange={(v) => updateFonts('header_font_latin', v)}
            />
            <ConfigInput
              label="页眉字号"
              value={config.fonts.header_size_pt}
              onChange={(v) => updateFonts('header_size_pt', v)}
              type="number"
              min={6}
              max={14}
              step={0.5}
              unit="pt"
            />
            <ConfigInput
              label="页脚字号"
              value={config.fonts.footer_size_pt}
              onChange={(v) => updateFonts('footer_size_pt', v)}
              type="number"
              min={6}
              max={14}
              step={0.5}
              unit="pt"
            />
            <ConfigInput
              label="页脚东亚字体"
              value={config.fonts.footer_font_east_asian}
              onChange={(v) => updateFonts('footer_font_east_asian', v)}
            />
            <ConfigInput
              label="页脚西文字体"
              value={config.fonts.footer_font_latin}
              onChange={(v) => updateFonts('footer_font_latin', v)}
            />
          </ConfigSection>

          {/* 间距设置 */}
          <ConfigSection title="间距设置" defaultExpanded={false}>
            <ConfigInput
              label="行距倍数"
              value={config.spacing.line_spacing}
              onChange={(v) => updateSpacing('line_spacing', v)}
              type="number"
              min={1.0}
              max={2.0}
              step={0.05}
              tooltip="如：1.25, 1.5, 2.0"
            />
            <ConfigInput
              label="一级标题段前"
              value={config.spacing.heading1_before_pt}
              onChange={(v) => updateSpacing('heading1_before_pt', v)}
              type="number"
              min={0}
              max={36}
              step={1}
              unit="pt"
            />
            <ConfigInput
              label="一级标题段后"
              value={config.spacing.heading1_after_pt}
              onChange={(v) => updateSpacing('heading1_after_pt', v)}
              type="number"
              min={0}
              max={36}
              step={1}
              unit="pt"
            />
            <ConfigInput
              label="二级标题段前"
              value={config.spacing.heading2_before_pt}
              onChange={(v) => updateSpacing('heading2_before_pt', v)}
              type="number"
              min={0}
              max={24}
              step={1}
              unit="pt"
            />
            <ConfigInput
              label="二级标题段后"
              value={config.spacing.heading2_after_pt}
              onChange={(v) => updateSpacing('heading2_after_pt', v)}
              type="number"
              min={0}
              max={24}
              step={1}
              unit="pt"
            />
            <ConfigInput
              label="三级标题段前"
              value={config.spacing.heading3_before_pt}
              onChange={(v) => updateSpacing('heading3_before_pt', v)}
              type="number"
              min={0}
              max={18}
              step={1}
              unit="pt"
            />
            <ConfigInput
              label="三级标题段后"
              value={config.spacing.heading3_after_pt}
              onChange={(v) => updateSpacing('heading3_after_pt', v)}
              type="number"
              min={0}
              max={18}
              step={1}
              unit="pt"
            />
            <ConfigInput
              label="摘要段前"
              value={config.spacing.abstract_before_pt}
              onChange={(v) => updateSpacing('abstract_before_pt', v)}
              type="number"
              min={0}
              max={36}
              step={1}
              unit="pt"
            />
            <ConfigInput
              label="摘要段后"
              value={config.spacing.abstract_after_pt}
              onChange={(v) => updateSpacing('abstract_after_pt', v)}
              type="number"
              min={0}
              max={36}
              step={1}
              unit="pt"
            />
            <ConfigInput
              label="首行缩进"
              value={config.spacing.first_line_indent_chars}
              onChange={(v) => updateSpacing('first_line_indent_chars', v)}
              type="number"
              min={0}
              max={4}
              step={0.5}
              unit="字符"
            />
            <ConfigInput
              label="关键词段前"
              value={config.spacing.keywords_before_pt}
              onChange={(v) => updateSpacing('keywords_before_pt', v)}
              type="number"
              min={0}
              max={36}
              step={1}
              unit="pt"
            />
            <ConfigInput
              label="参考文献悬挂缩进"
              value={config.spacing.ref_hanging_indent_chars}
              onChange={(v) => updateSpacing('ref_hanging_indent_chars', v)}
              type="number"
              min={0}
              max={4}
              step={0.5}
              unit="字符"
            />
            <ConfigInput
              label="图表标题段后"
              value={config.spacing.caption_after_pt}
              onChange={(v) => updateSpacing('caption_after_pt', v)}
              type="number"
              min={0}
              max={18}
              step={1}
              unit="pt"
            />
          </ConfigSection>

          {/* 页眉页脚 */}
          <ConfigSection title="页眉页脚" defaultExpanded={false}>
            <ConfigSwitch
              label="显示页眉"
              checked={config.header.show}
              onChange={(v) => updateHeader('show', v)}
            />
            <ConfigInput
              label="页眉文本"
              value={config.header.text_template}
              onChange={(v) => updateHeader('text_template', v)}
              tooltip="支持 {grade} 变量替换为年级"
            />
            <ConfigSelect
              label="奇数页页眉对齐"
              value={config.header.odd_align}
              onChange={(v) => updateHeader('odd_align', v)}
              options={alignOptions}
            />
            <ConfigSelect
              label="偶数页页眉对齐"
              value={config.header.even_align}
              onChange={(v) => updateHeader('even_align', v)}
              options={alignOptions}
            />
            <ConfigSwitch
              label="显示页码"
              checked={config.footer.show_page_number}
              onChange={(v) => updateFooter('show_page_number', v)}
            />
            <ConfigSelect
              label="摘要页码格式"
              value={config.footer.abstract_format}
              onChange={(v) => updateFooter('abstract_format', v)}
              options={pageFormatOptions}
            />
            <ConfigSelect
              label="正文页码格式"
              value={config.footer.body_format}
              onChange={(v) => updateFooter('body_format', v)}
              options={pageFormatOptions}
            />
            <ConfigSelect
              label="摘要页码对齐"
              value={config.footer.abstract_align}
              onChange={(v) => updateFooter('abstract_align', v)}
              options={alignOptions}
            />
            <ConfigSelect
              label="奇数页页脚对齐"
              value={config.footer.odd_align}
              onChange={(v) => updateFooter('odd_align', v)}
              options={alignOptions}
            />
            <ConfigSelect
              label="偶数页页脚对齐"
              value={config.footer.even_align}
              onChange={(v) => updateFooter('even_align', v)}
              options={alignOptions}
            />
          </ConfigSection>

          {/* 章节开关 */}
          <ConfigSection title="章节开关">
            <ConfigSwitch
              label="封面"
              checked={config.sections.cover}
              onChange={(v) => updateSection('cover', v)}
            />
            <ConfigSwitch
              label="中文摘要"
              checked={config.sections.abstract_cn}
              onChange={(v) => updateSection('abstract_cn', v)}
            />
            <ConfigSwitch
              label="英文摘要"
              checked={config.sections.abstract_en}
              onChange={(v) => updateSection('abstract_en', v)}
            />
            <ConfigSwitch
              label="目录"
              checked={config.sections.toc}
              onChange={(v) => updateSection('toc', v)}
            />
            <ConfigSwitch
              label="正文"
              checked={config.sections.body}
              onChange={(v) => updateSection('body', v)}
            />
            <ConfigSwitch
              label="参考文献"
              checked={config.sections.references}
              onChange={(v) => updateSection('references', v)}
            />
            <ConfigSwitch
              label="附录"
              checked={config.sections.appendix}
              onChange={(v) => updateSection('appendix', v)}
            />
            <ConfigSwitch
              label="致谢"
              checked={config.sections.acknowledgment}
              onChange={(v) => updateSection('acknowledgment', v)}
            />
            <ConfigSwitch
              label="封底"
              checked={config.sections.back_cover}
              onChange={(v) => updateSection('back_cover', v)}
            />
          </ConfigSection>
        </div>
      )}
    </aside>
  )
})

ConfigPanel.displayName = 'ConfigPanel'

export default ConfigPanel
