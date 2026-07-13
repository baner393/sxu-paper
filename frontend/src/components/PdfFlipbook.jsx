import { useState, useEffect, useRef, useCallback, memo } from 'react'
import * as pdfjsLib from 'pdfjs-dist'
import './PdfFlipbook.css'

pdfjsLib.GlobalWorkerOptions.workerSrc = new URL(
  'pdfjs-dist/build/pdf.worker.min.mjs',
  import.meta.url
).toString()

const PdfPage = memo(({ pdfDoc, pageNum, width, height }) => {
  const canvasRef = useRef(null)
  const renderTaskRef = useRef(null)

  useEffect(() => {
    if (!pdfDoc || !canvasRef.current || pageNum < 1 || width < 10 || height < 10) return

    let cancelled = false
    const render = async () => {
      try {
        if (renderTaskRef.current) {
          renderTaskRef.current.cancel()
          renderTaskRef.current = null
        }
        const page = await pdfDoc.getPage(pageNum)
        if (cancelled) return

        const vp = page.getViewport({ scale: 1 })
        const scale = Math.min(width / vp.width, height / vp.height)
        const scaledVp = page.getViewport({ scale })

        const canvas = canvasRef.current
        const ctx = canvas.getContext('2d')
        const dpr = window.devicePixelRatio || 1
        canvas.width = Math.floor(scaledVp.width * dpr)
        canvas.height = Math.floor(scaledVp.height * dpr)
        canvas.style.width = Math.floor(scaledVp.width) + 'px'
        canvas.style.height = Math.floor(scaledVp.height) + 'px'
        ctx.scale(dpr, dpr)

        const task = page.render({ canvasContext: ctx, viewport: scaledVp })
        renderTaskRef.current = task
        await task.promise
        renderTaskRef.current = null
      } catch (err) {
        if (err?.name !== 'RenderingCancelledException') {
          console.warn(`Page ${pageNum} render failed:`, err)
        }
      }
    }
    render()
    return () => {
      cancelled = true
      if (renderTaskRef.current) { renderTaskRef.current.cancel(); renderTaskRef.current = null }
    }
  }, [pdfDoc, pageNum, width, height])

  return <canvas ref={canvasRef} className="pdf-page-canvas" />
})
PdfPage.displayName = 'PdfPage'

const A4_RATIO = 297 / 210

const PdfFlipbook = memo(({ pdfUrl, totalPages, isLoading, error, onClose }) => {
  const [pdfDoc, setPdfDoc] = useState(null)
  const [currentPage, setCurrentPage] = useState(0)
  const [loadError, setLoadError] = useState(null)
  const [flipDir, setFlipDir] = useState(null)
  const [flipping, setFlipping] = useState(false)
  const areaRef = useRef(null)
  const [size, setSize] = useState({ w: 0, h: 0 })

  // 加载 PDF
  useEffect(() => {
    if (!pdfUrl) { setPdfDoc(null); setLoadError(null); return }
    let cancelled = false
    setLoadError(null); setPdfDoc(null)
    pdfjsLib.getDocument(pdfUrl).promise.then(doc => {
      if (!cancelled) { setPdfDoc(doc); setCurrentPage(0) }
    }).catch(err => {
      if (!cancelled) setLoadError(`PDF 加载失败: ${err.message}`)
    })
    return () => { cancelled = true }
  }, [pdfUrl])

  // 测量容器尺寸（使用 useLayoutEffect 同步测量）
  useEffect(() => {
    const el = areaRef.current
    if (!el) return
    const measure = () => {
      const rect = el.getBoundingClientRect()
      setSize({ w: Math.floor(rect.width), h: Math.floor(rect.height) })
    }
    measure()
    const observer = new ResizeObserver(() => measure())
    observer.observe(el)
    return () => observer.disconnect()
  }, [pdfDoc]) // 重新测量当 PDF 加载后（DOM 可能变化）

  // 翻页
  const flip = useCallback((dir, target) => {
    if (flipping) return
    setFlipDir(dir); setFlipping(true)
    setTimeout(() => { setCurrentPage(target); setFlipDir(null); setFlipping(false) }, 400)
  }, [flipping])

  const nextPage = useCallback(() => {
    if (!pdfDoc || flipping) return
    const step = currentPage === 0 ? 1 : 2
    if (currentPage + step < pdfDoc.numPages) flip('next', currentPage + step)
  }, [pdfDoc, currentPage, flipping, flip])

  const prevPage = useCallback(() => {
    if (!pdfDoc || flipping) return
    const step = currentPage === 1 ? 1 : 2
    if (currentPage - step >= 0) flip('prev', currentPage - step)
  }, [pdfDoc, currentPage, flipping, flip])

  const goTo = useCallback((idx) => {
    if (!pdfDoc || flipping || idx === currentPage) return
    flip(idx > currentPage ? 'next' : 'prev', Math.max(0, Math.min(idx, pdfDoc.numPages - 1)))
  }, [pdfDoc, currentPage, flipping, flip])

  useEffect(() => {
    const onKey = (e) => {
      if (e.key === 'ArrowRight' || e.key === 'ArrowDown') nextPage()
      else if (e.key === 'ArrowLeft' || e.key === 'ArrowUp') prevPage()
      else if (e.key === 'Escape') onClose?.()
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [nextPage, prevPage, onClose])

  // 状态渲染
  if (error) return <div className="pdf-flipbook-container"><div className="pdf-error"><div className="error-icon">⚠️</div><p>{error}</p></div></div>
  if (isLoading) return <div className="pdf-flipbook-container"><div className="pdf-loading"><div className="loading-spinner large" /><p>正在生成 PDF 预览...</p></div></div>
  if (loadError) return <div className="pdf-flipbook-container"><div className="pdf-error"><div className="error-icon">⚠️</div><p>{loadError}</p></div></div>
  if (!pdfUrl) return <div className="pdf-flipbook-container"><div className="pdf-empty"><div className="empty-icon">📄</div><p className="empty-text">暂无预览</p><p className="empty-hint">选择输入文件和模板，然后点击 Preview</p></div></div>
  if (!pdfDoc) return <div className="pdf-flipbook-container"><div className="pdf-loading"><div className="loading-spinner large" /><p>正在加载 PDF...</p></div></div>

  const numPages = pdfDoc.numPages
  const isCover = currentPage === 0

  let leftPage, rightPage
  if (isCover) { leftPage = 1; rightPage = null }
  else {
    leftPage = currentPage + 1
    rightPage = Math.min(currentPage + 2, numPages)
    if (rightPage === leftPage) rightPage = null
  }

  // 计算页面尺寸：使用实际测量的容器尺寸
  const padding = 40
  const spineGap = isCover ? 0 : 28 // 书脊 + 间距
  const availW = size.w - padding
  const availH = size.h - padding

  let pageW, pageH
  if (availW > 0 && availH > 0) {
    if (isCover) {
      // 封面：用满可用空间
      pageW = availW
      pageH = availW * A4_RATIO
      if (pageH > availH) { pageH = availH; pageW = availH / A4_RATIO }
    } else {
      // 双面：两页并排
      const singleW = (availW - spineGap) / 2
      pageW = singleW
      pageH = singleW * A4_RATIO
      if (pageH > availH) { pageH = availH; pageW = availH / A4_RATIO }
    }
  } else {
    pageW = 400; pageH = 566 // 回退值
  }
  pageW = Math.floor(pageW)
  pageH = Math.floor(pageH)

  // 页码指示器
  const spreads = []
  let i = 0
  while (i < numPages) {
    if (i === 0) { spreads.push({ start: 0 }); i = 1 }
    else { const e = Math.min(i + 1, numPages - 1); spreads.push({ start: i, label: `${i + 1}${e > i ? '-' + (e + 1) : ''}` }); i = e + 1 }
  }
  const curIdx = spreads.findIndex(s => s.start === currentPage)

  return (
    <div className="pdf-flipbook-container">
      <div className="pdf-toolbar">
        <h2 className="pdf-title">打印预览</h2>
        <div className="pdf-controls">
          <button className="pdf-btn" onClick={prevPage} disabled={currentPage <= 0 || flipping}>◀ 上一页</button>
          <span className="pdf-page-info">
            {isCover ? `封面 / ${numPages} 页` : `${leftPage}${rightPage ? `-${rightPage}` : ''} / ${numPages} 页`}
          </span>
          <button className="pdf-btn" onClick={nextPage} disabled={currentPage >= numPages - 1 || flipping}>下一页 ▶</button>
          {onClose && <button className="pdf-btn close" onClick={onClose}>✕</button>}
        </div>
      </div>

      <div className={`pdf-pages-area ${isCover ? 'cover-mode' : 'spread-mode'}`} ref={areaRef}>
        <div className={`pdf-spread-wrapper ${flipDir ? `flip-${flipDir}` : ''}`}>
          {isCover && (
            <div className="pdf-page-wrapper cover-page">
              <PdfPage pdfDoc={pdfDoc} pageNum={1} width={pageW} height={pageH} />
              <div className="pdf-page-number">1</div>
            </div>
          )}
          {!isCover && (
            <>
              <div className="pdf-page-wrapper left-page">
                <PdfPage pdfDoc={pdfDoc} pageNum={leftPage} width={pageW} height={pageH} />
                <div className="pdf-page-number">{leftPage}</div>
              </div>
              {rightPage && (
                <>
                  <div className="pdf-spine" />
                  <div className="pdf-page-wrapper right-page">
                    <PdfPage pdfDoc={pdfDoc} pageNum={rightPage} width={pageW} height={pageH} />
                    <div className="pdf-page-number">{rightPage}</div>
                  </div>
                </>
              )}
            </>
          )}
        </div>
      </div>

      <div className="pdf-page-indicators">
        {spreads.map((s, idx) => (
          <button key={idx} className={`pdf-indicator ${idx === curIdx ? 'active' : ''}`}
            onClick={() => goTo(s.start)} title={`第 ${s.label || '1'} 页`} />
        ))}
      </div>

      <div className="pdf-keyboard-hint">
        <span>← → 翻页</span>
        <span>ESC 关闭</span>
      </div>
    </div>
  )
})
PdfFlipbook.displayName = 'PdfFlipbook'
export default PdfFlipbook
