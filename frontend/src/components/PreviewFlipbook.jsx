import { memo, useRef, useCallback, useEffect, forwardRef } from 'react'
import HTMLFlipBook from 'react-pageflip'
import './PreviewFlipbook.css'

// 单页组件 (必须用 forwardRef)
const Page = forwardRef(({ page }, ref) => {
  return (
    <div className="flipbook-page" ref={ref}>
      <div className="page-content">
        <img
          src={page.image_url}
          alt={`Page ${page.page}`}
          className="page-image"
          loading="lazy"
        />
      </div>
      <div className="page-number">{page.page}</div>
    </div>
  )
})

Page.displayName = 'Page'

// 预览翻书组件
const PreviewFlipbook = memo(({
  pages,
  currentPage,
  totalPages,
  previewTitle,
  isLoading,
  error,
  onPageChange,
  onNextPage,
  onPrevPage
}) => {
  const flipBookRef = useRef(null)

  // 翻页事件回调（flipbook 内部翻页 → 同步到父组件）
  const onFlip = useCallback((e) => {
    onPageChange(e.data)
  }, [onPageChange])

  // 程序化翻页（按钮点击 → 通知 flipbook）
  const handlePrev = useCallback(() => {
    onPrevPage()
    try {
      flipBookRef.current?.pageFlip()?.flipPrev()
    } catch (e) { /* flipbook 未就绪时忽略 */ }
  }, [onPrevPage])

  const handleNext = useCallback(() => {
    onNextPage()
    try {
      flipBookRef.current?.pageFlip()?.flipNext()
    } catch (e) { /* flipbook 未就绪时忽略 */ }
  }, [onNextPage])

  // 渲染加载状态
  if (isLoading) {
    return (
      <div className="preview-container">
        <div className="preview-loading">
          <div className="loading-spinner large"></div>
          <p className="loading-text">Generating preview...</p>
        </div>
      </div>
    )
  }

  // 渲染错误状态
  if (error) {
    return (
      <div className="preview-container">
        <div className="preview-error">
          <div className="error-icon">⚠️</div>
          <p className="error-text">{error}</p>
        </div>
      </div>
    )
  }

  // 渲染空状态
  if (!pages || pages.length === 0) {
    return (
      <div className="preview-container">
        <div className="preview-empty">
          <div className="empty-icon">📄</div>
          <p className="empty-text">No preview</p>
          <p className="empty-hint">Select input and template, then click Preview</p>
        </div>
      </div>
    )
  }

  // 渲染翻书预览
  return (
    <div className="preview-container">
      <div className="preview-header">
        <h2 className="preview-title">{previewTitle || 'Document Preview'}</h2>
        <div className="preview-controls">
          <button
            className="preview-btn"
            onClick={handlePrev}
            disabled={currentPage <= 0}
            title="Previous (←)"
          >
            ◀
          </button>
          <span className="page-info">
            {currentPage + 1} / {totalPages}
          </span>
          <button
            className="preview-btn"
            onClick={handleNext}
            disabled={currentPage >= totalPages - 1}
            title="Next (→)"
          >
            ▶
          </button>
        </div>
      </div>

      <div className="flipbook-wrapper">
        <HTMLFlipBook
          ref={flipBookRef}
          width={595}
          height={842}
          size="stretch"
          minWidth={300}
          maxWidth={1200}
          minHeight={400}
          maxHeight={1700}
          maxShadowOpacity={0.5}
          showCover={true}
          mobileScrollSupport={true}
          onFlip={onFlip}
          className="flipbook"
          startPage={currentPage}
          drawShadow={true}
          flippingTime={600}
          usePortrait={true}
          startZIndex={0}
          autoSize={true}
          clickEventForward={true}
          useMouseEvents={true}
          swipeDistance={30}
          renderOnlyPageLengthChange={false}
          disableFlipByClick={false}
        >
          {pages.map((page, index) => (
            <Page key={page.page || index} page={page} />
          ))}
        </HTMLFlipBook>
      </div>

      <div className="page-indicators">
        {pages.map((page, index) => (
          <button
            key={page.page || index}
            className={`page-indicator ${currentPage === index ? 'active' : ''}`}
            onClick={() => onPageChange(index)}
            title={`Page ${page.page}`}
          />
        ))}
      </div>

      <div className="keyboard-hint">
        <span>← → keys to flip</span>
      </div>
    </div>
  )
})

PreviewFlipbook.displayName = 'PreviewFlipbook'

export default PreviewFlipbook
