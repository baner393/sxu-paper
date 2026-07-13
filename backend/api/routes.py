"""API 路由定义。

对应 API_CONTRACT.md 中的所有端点：
- POST /api/convert
- POST /api/preview
- GET  /api/pages/{title}/{page_num}
- GET  /api/templates
- GET  /api/inputs
- GET  /api/config/default
- POST /api/config/save
"""

from __future__ import annotations

import json
import re
import subprocess
import tempfile
from pathlib import Path
from typing import Any, Dict, List, Optional

from fastapi import APIRouter, HTTPException, UploadFile, File
from fastapi.responses import FileResponse
from pydantic import BaseModel

from ..thesis_builder.converter import convert
from ..thesis_builder.styles import ThesisConfig


# ═══════════════════════════════════════════
# 路由器
# ═══════════════════════════════════════════

router = APIRouter()

# 项目根目录
PROJECT_ROOT = Path(__file__).resolve().parent.parent.parent
TEMPLATE_DIR = PROJECT_ROOT / "template"
INPUT_DIR = PROJECT_ROOT / "input"
OUTPUT_DIR = PROJECT_ROOT / "output"
CONFIG_DIR = PROJECT_ROOT / "config"

# 确保目录存在
OUTPUT_DIR.mkdir(exist_ok=True)
CONFIG_DIR.mkdir(exist_ok=True)


# ═══════════════════════════════════════════
# 请求/响应模型
# ═══════════════════════════════════════════

class ConvertRequest(BaseModel):
    """转换请求"""
    input_md: str
    template_path: str = "template/山财学年论文模板.docx"
    config: Optional[Dict[str, Any]] = None


class ConvertResponse(BaseModel):
    """转换响应"""
    success: bool
    output_path: str
    preview_url: str
    pages: int
    warnings: List[str]


class PreviewRequest(BaseModel):
    """预览请求（与转换请求相同）"""
    input_md: str
    template_path: str = "template/山财学年论文模板.docx"
    config: Optional[Dict[str, Any]] = None


class PageInfo(BaseModel):
    """页面信息"""
    page: int
    image_url: str
    width: int
    height: int


class PreviewResponse(BaseModel):
    """预览响应"""
    success: bool
    pdf_url: str
    total_pages: int


class ConfigSaveRequest(BaseModel):
    """配置保存请求"""
    name: str
    config: Dict[str, Any]


# ═══════════════════════════════════════════
# 转换端点
# ═══════════════════════════════════════════

@router.post("/convert", response_model=ConvertResponse)
async def api_convert(req: ConvertRequest):
    """将 Markdown 转换为 DOCX"""
    input_path = _resolve_input(req.input_md)
    template_path = _resolve_template(req.template_path)

    if not input_path.exists():
        raise HTTPException(status_code=404, detail=f"输入文件不存在: {req.input_md}")
    if not template_path.exists():
        raise HTTPException(status_code=404, detail=f"模板文件不存在: {req.template_path}")

    try:
        result = convert(
            input_md=str(input_path),
            template_path=str(template_path),
            config=req.config,
        )
        return ConvertResponse(**result)
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"转换失败: {str(e)}")


# ═══════════════════════════════════════════
# 预览端点
# ═══════════════════════════════════════════

@router.post("/preview", response_model=PreviewResponse)
async def api_preview(req: PreviewRequest):
    """生成 PDF 预览"""
    input_path = _resolve_input(req.input_md)
    template_path = _resolve_template(req.template_path)

    if not input_path.exists():
        raise HTTPException(status_code=404, detail=f"输入文件不存在: {req.input_md}")

    try:
        result = convert(
            input_md=str(input_path),
            template_path=str(template_path),
            config=req.config,
        )
        docx_path = Path(result["output_path"])
        title = docx_path.stem

        # 转换为 PDF
        pdf_path = _convert_to_pdf(docx_path, title)
        if pdf_path is None:
            raise HTTPException(status_code=500, detail="PDF 转换失败，请确保 Microsoft Word 或 LibreOffice 已安装")

        # 计算页数
        total_pages = _count_pdf_pages(pdf_path)
        safe_title = _url_safe_title(title)

        return PreviewResponse(
            success=True,
            pdf_url=f"/api/pdf/{safe_title}",
            total_pages=total_pages,
        )
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"预览生成失败: {str(e)}")


@router.get("/pages/{title}/{page_num}")
async def api_get_page(title: str, page_num: int):
    """获取单页预览图片（兼容旧接口）"""
    page_dir = OUTPUT_DIR / title / "pages"
    page_file = page_dir / f"{page_num}.png"

    if not page_file.exists():
        raise HTTPException(status_code=404, detail=f"页面不存在: {title}/{page_num}")

    return FileResponse(
        str(page_file),
        media_type="image/png",
        filename=f"{title}_{page_num}.png",
    )


@router.get("/pdf/{title}")
async def api_get_pdf(title: str):
    """获取预览 PDF 文件"""
    # FastAPI 自动 URL 解码 title 参数
    for pdf_file in OUTPUT_DIR.glob("*.pdf"):
        if pdf_file.stem == title:
            return FileResponse(
                str(pdf_file),
                media_type="application/pdf",
                filename=f"{title}.pdf",
            )

    raise HTTPException(status_code=404, detail=f"PDF 不存在: {title}")


# ═══════════════════════════════════════════
# 模板和输入列表
# ═══════════════════════════════════════════

@router.get("/templates")
async def api_list_templates():
    """列出可用的 DOCX 模板"""
    templates = []
    if TEMPLATE_DIR.exists():
        for f in sorted(TEMPLATE_DIR.glob("*.docx")):
            templates.append({
                "name": f.stem,
                "path": f"template/{f.name}",
                "size": f.stat().st_size,
            })
    return {"templates": templates}


@router.get("/inputs")
async def api_list_inputs():
    """列出可用的输入论文"""
    inputs = []
    if INPUT_DIR.exists():
        for md_file in sorted(INPUT_DIR.rglob("*.md")):
            rel_path = md_file.relative_to(PROJECT_ROOT)
            inputs.append({
                "name": md_file.stem,
                "path": str(rel_path).replace("\\", "/"),
                "directory": str(md_file.parent.relative_to(PROJECT_ROOT)).replace("\\", "/"),
            })
    return {"inputs": inputs}


@router.post("/upload")
async def api_upload_file(file: UploadFile = File(...)):
    """上传 .md 文件到 input 目录"""
    # 验证文件类型
    if not file.filename.endswith('.md'):
        raise HTTPException(status_code=400, detail="仅支持 .md 文件")

    # 限制上传大小（10MB）
    MAX_UPLOAD_SIZE = 10 * 1024 * 1024

    # 确定保存路径（自动编号子目录）
    existing_dirs = [d.name for d in INPUT_DIR.iterdir() if d.is_dir() and d.name.isdigit()]
    next_num = max((int(d) for d in existing_dirs), default=0) + 1
    upload_dir = INPUT_DIR / str(next_num)
    upload_dir.mkdir(parents=True, exist_ok=True)

    # 清理文件名（防止路径遍历）
    safe_filename = Path(file.filename).name
    if not safe_filename or safe_filename.startswith('.'):
        raise HTTPException(status_code=400, detail="无效的文件名")

    # 读取并检查大小
    content = await file.read()
    if len(content) > MAX_UPLOAD_SIZE:
        raise HTTPException(status_code=413, detail="文件过大，最大 10MB")

    # 保存文件
    file_path = upload_dir / safe_filename
    file_path.write_bytes(content)

    # 返回文件信息
    rel_path = file_path.relative_to(PROJECT_ROOT)
    return {
        "success": True,
        "name": safe_filename.replace('.md', ''),
        "path": str(rel_path).replace("\\", "/"),
        "directory": str(upload_dir.relative_to(PROJECT_ROOT)).replace("\\", "/"),
    }


# ═══════════════════════════════════════════
# 配置管理
# ═══════════════════════════════════════════

@router.get("/config/default")
async def api_get_default_config():
    """获取默认配置"""
    cfg = ThesisConfig()
    return cfg.to_dict()


@router.post("/config/save")
async def api_save_config(req: ConfigSaveRequest):
    """保存命名配置预设"""
    # 验证名称
    safe_name = re.sub(r'[^\w\-]', '_', req.name)
    if not safe_name:
        raise HTTPException(status_code=400, detail="无效的配置名称")

    config_file = CONFIG_DIR / f"{safe_name}.json"
    config_file.write_text(
        json.dumps(req.config, ensure_ascii=False, indent=2),
        encoding="utf-8",
    )
    return {"success": True, "path": f"config/{safe_name}.json"}


@router.get("/config/list")
async def api_list_configs():
    """列出已保存的配置预设"""
    configs = []
    if CONFIG_DIR.exists():
        for f in sorted(CONFIG_DIR.glob("*.json")):
            configs.append({
                "name": f.stem,
                "path": f"config/{f.name}",
            })
    return {"configs": configs}


@router.get("/config/{name}")
async def api_load_config(name: str):
    """加载命名配置预设"""
    safe_name = re.sub(r'[^\w\-]', '_', name)
    config_file = CONFIG_DIR / f"{safe_name}.json"

    if not config_file.exists():
        raise HTTPException(status_code=404, detail=f"配置不存在: {name}")

    config = json.loads(config_file.read_text(encoding="utf-8"))
    return {"name": safe_name, "config": config}


# ═══════════════════════════════════════════
# 辅助函数
# ═══════════════════════════════════════════

def _validate_path(path: Path) -> Path:
    """验证路径在项目目录内（防止路径遍历攻击）"""
    try:
        resolved = path.resolve()
        project_resolved = PROJECT_ROOT.resolve()
        if not resolved.is_relative_to(project_resolved):
            raise HTTPException(status_code=403, detail="访问被拒绝")
        return resolved
    except (ValueError, OSError):
        raise HTTPException(status_code=400, detail="无效的路径")


def _resolve_input(input_md: str) -> Path:
    """解析输入文件路径"""
    path = Path(input_md)
    if not path.is_absolute():
        path = PROJECT_ROOT / path
    return _validate_path(path)


def _resolve_template(template_path: str) -> Path:
    """解析模板文件路径"""
    path = Path(template_path)
    if not path.is_absolute():
        path = PROJECT_ROOT / path
    return _validate_path(path)


def _convert_to_images(docx_path: Path, title: str) -> List[PageInfo]:
    """将 DOCX 转换为 PNG 图片（优先 LibreOffice，后备 Pillow 渲染）"""
    page_dir = OUTPUT_DIR / title / "pages"
    page_dir.mkdir(parents=True, exist_ok=True)

    # 方法 1：LibreOffice（高质量）
    pages = _convert_via_libreoffice(docx_path, page_dir, title)
    if pages:
        return pages

    # 方法 2：Pillow 文本渲染（后备）
    return _convert_via_pillow(docx_path, page_dir, title)


def _convert_via_libreoffice(docx_path: Path, page_dir: Path, title: str) -> List[PageInfo]:
    """使用 LibreOffice 将 DOCX 转换为 PNG"""
    lo_cmd = _find_libreoffice()
    if lo_cmd is None:
        return []

    try:
        subprocess.run(
            [lo_cmd, "--headless", "--convert-to", "png", "--outdir", str(page_dir), str(docx_path)],
            capture_output=True, text=True, timeout=60,
        )
    except (subprocess.TimeoutExpired, FileNotFoundError):
        return []

    pages = []
    for img_file in sorted(page_dir.glob("*.png")):
        match = re.search(r'(\d+)', img_file.stem)
        page_num = int(match.group(1)) if match else len(pages) + 1
        pages.append(PageInfo(page=page_num, image_url=f"/api/pages/{title}/{page_num}", width=1240, height=1754))
    return pages


def _convert_via_pillow(docx_path: Path, page_dir: Path, title: str) -> List[PageInfo]:
    """使用 Pillow 渲染 DOCX 内容为接近真实排版的页面图片"""
    try:
        from docx import Document as DocxDocument
        from docx.shared import Pt as DocxPt
        from docx.enum.text import WD_ALIGN_PARAGRAPH
        from PIL import Image, ImageDraw, ImageFont
    except ImportError:
        return []

    try:
        doc = DocxDocument(str(docx_path))
    except Exception:
        return []

    # A4 @ 150 DPI
    page_w, page_h = 1240, 1754
    margin_left = 113   # 2.5cm
    margin_right = 90   # 2cm
    margin_top = 136    # 3cm
    margin_bottom = 113 # 2.5cm
    header_y = 50       # 页眉位置
    footer_y = page_h - 60  # 页脚位置

    text_area_w = page_w - margin_left - margin_right

    # 加载字体
    def load_font(size, bold=False):
        for fp in [
            "C:/Windows/Fonts/simhei.ttf" if bold else "C:/Windows/Fonts/simsun.ttc",
            "C:/Windows/Fonts/msyh.ttc",
            "/usr/share/fonts/truetype/noto/NotoSansCJK-Bold.ttc" if bold
            else "/usr/share/fonts/truetype/noto/NotoSansCJK-Regular.ttc",
            "/usr/share/fonts/opentype/noto/NotoSansCJK-Bold.ttc" if bold
            else "/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc",
        ]:
            try:
                return ImageFont.truetype(fp, size)
            except (OSError, IOError):
                continue
        return ImageFont.load_default()

    font_body = load_font(24)        # 小四号 ≈ 24pt
    font_h1 = load_font(32, True)    # 三号黑体
    font_h2 = load_font(28, True)    # 四号黑体
    font_h3 = load_font(24, True)    # 小四号宋体加粗
    font_header = load_font(18)      # 五号页眉
    font_key = load_font(24, True)   # 关键词标签
    line_h_body = 36                 # 1.25倍行距
    line_h_h1 = 48

    # 提取页眉文本
    header_text = ""
    if doc.sections and doc.sections[0].header:
        for p in doc.sections[0].header.paragraphs:
            if p.text.strip():
                header_text = p.text.strip()
                break

    # 收集渲染元素
    elements = []
    for p in doc.paragraphs:
        text = p.text.strip()
        alignment = p.alignment
        is_bold = any(r.bold for r in p.runs if r.bold is True)
        is_heading = p.style and "Heading" in p.style.name if p.style else False

        # 判断标题级别
        level = 0
        if is_heading:
            if "1" in p.style.name:
                level = 1
            elif "2" in p.style.name:
                level = 2
            elif "3" in p.style.name:
                level = 3
        elif is_bold and text and len(text) < 50:
            # 短粗体段落可能是标题
            if any(c.isdigit() for c in text[:3]):
                level = 2
            elif text in ("摘  要", "摘要", "Abstract", "目  录", "目录",
                          "致  谢", "致谢", "附  录", "附录", "参考文献"):
                level = 1

        # 关键词行
        is_keyword = text.startswith("关键词") or text.startswith("Keywords") or text.startswith("Key words")

        # 首行缩进
        indent = 0
        if level == 0 and not is_keyword and text and p.paragraph_format.first_line_indent:
            indent_val = p.paragraph_format.first_line_indent
            if indent_val:
                indent = int(indent_val.pt * 1.5) if hasattr(indent_val, 'pt') else 48

        if not text:
            elements.append({"type": "empty", "height": line_h_body // 2})
        elif level == 1:
            elements.append({"type": "h1", "text": text, "height": line_h_h1})
        elif level == 2:
            elements.append({"type": "h2", "text": text, "height": line_h_body + 8})
        elif level == 3:
            elements.append({"type": "h3", "text": text, "height": line_h_body + 4})
        elif is_keyword:
            elements.append({"type": "keyword", "text": text, "height": line_h_body + 8})
        else:
            # 正文段落 - 自动换行
            font = font_body
            cpl = text_area_w // (24 + 2)
            lines = []
            remaining = text
            while len(remaining) > cpl:
                lines.append(remaining[:cpl])
                remaining = remaining[cpl:]
            if remaining:
                lines.append(remaining)
            elements.append({
                "type": "body", "lines": lines, "indent": indent,
                "height": len(lines) * line_h_body,
                "alignment": alignment,
            })

    # 预处理：拆分超长 body 段落，每段不超过一页可用行数
    usable_h = page_h - margin_top - 30 - margin_bottom  # 页眉下方到页脚上方
    max_lines_per_page = usable_h // line_h_body
    split_elements = []
    for elem in elements:
        if elem["type"] == "body" and len(elem["lines"]) > max_lines_per_page:
            lines = elem["lines"]
            indent = elem.get("indent", 0)
            align = elem.get("alignment")
            while lines:
                chunk = lines[:max_lines_per_page]
                lines = lines[max_lines_per_page:]
                split_elements.append({
                    "type": "body", "lines": chunk, "indent": indent,
                    "height": len(chunk) * line_h_body, "alignment": align,
                })
                indent = 0  # 只有第一段有首行缩进
        else:
            split_elements.append(elem)
    elements = split_elements

    # 渲染页面
    pages = []
    page_num = 1
    elem_idx = 0
    page_start_y = margin_top + 30  # 页眉下方留空

    while elem_idx < len(elements):
        img = Image.new("RGB", (page_w, page_h), "white")
        draw = ImageDraw.Draw(img)

        # 页眉
        if header_text:
            draw.text((margin_left, header_y), header_text, fill="black", font=font_header)
            draw.line([(margin_left, header_y + 24), (page_w - margin_right, header_y + 24)],
                      fill="black", width=1)

        # 页脚页码
        draw.text((page_w // 2 - 10, footer_y), str(page_num), fill="black", font=font_header)

        y = page_start_y
        while elem_idx < len(elements):
            elem = elements[elem_idx]
            needed = elem["height"] + (10 if elem["type"] in ("h1",) else 0)

            if y + needed > page_h - margin_bottom:
                break  # 换页

            if elem["type"] == "empty":
                y += elem["height"]
            elif elem["type"] == "h1":
                y += 10  # 标题前间距
                text_w = draw.textlength(elem["text"], font=font_h1)
                x = (page_w - text_w) / 2
                draw.text((x, y), elem["text"], fill="black", font=font_h1)
                y += elem["height"]
            elif elem["type"] == "h2":
                draw.text((margin_left, y), elem["text"], fill="black", font=font_h2)
                y += elem["height"]
            elif elem["type"] == "h3":
                draw.text((margin_left, y), elem["text"], fill="black", font=font_h3)
                y += elem["height"]
            elif elem["type"] == "keyword":
                draw.text((margin_left, y), elem["text"], fill="black", font=font_key)
                y += elem["height"]
            elif elem["type"] == "body":
                align = elem.get("alignment")
                first_line = True
                for line in elem["lines"]:
                    x = margin_left + (elem["indent"] if first_line else 0)
                    if align == WD_ALIGN_PARAGRAPH.CENTER:
                        lw = draw.textlength(line, font=font_body)
                        x = (page_w - lw) / 2
                    elif align == WD_ALIGN_PARAGRAPH.RIGHT:
                        lw = draw.textlength(line, font=font_body)
                        x = page_w - margin_right - lw
                    draw.text((x, y), line, fill="black", font=font_body)
                    y += line_h_body
                    first_line = False

            elem_idx += 1

        img.save(str(page_dir / f"{page_num}.png"), "PNG")
        pages.append(PageInfo(page=page_num, image_url=f"/api/pages/{title}/{page_num}",
                              width=page_w, height=page_h))
        page_num += 1

    return pages


def _find_libreoffice() -> Optional[str]:
    """查找 LibreOffice 可执行文件"""
    import shutil

    # Windows 路径
    candidates = [
        r"C:\Program Files\LibreOffice\program\soffice.exe",
        r"C:\Program Files (x86)\LibreOffice\program\soffice.exe",
    ]
    for p in candidates:
        if Path(p).exists():
            return p

    # Linux/macOS
    for cmd in ("libreoffice", "soffice"):
        if shutil.which(cmd):
            return cmd

    return None


def _url_safe_title(title: str) -> str:
    """将标题转换为 URL 安全的字符串"""
    import urllib.parse
    return urllib.parse.quote(title, safe="")


def _convert_to_pdf(docx_path: Path, title: str) -> Optional[Path]:
    """将 DOCX 转换为 PDF（优先 Word，后备 LibreOffice）"""
    pdf_path = OUTPUT_DIR / f"{title}.pdf"

    # 方法 1：Microsoft Word（Windows）
    if _convert_via_word(docx_path, pdf_path):
        return pdf_path

    # 方法 2：LibreOffice
    lo_cmd = _find_libreoffice()
    if lo_cmd:
        try:
            subprocess.run(
                [lo_cmd, "--headless", "--convert-to", "pdf", "--outdir", str(OUTPUT_DIR), str(docx_path)],
                capture_output=True, text=True, timeout=120,
            )
            if pdf_path.exists():
                return pdf_path
        except (subprocess.TimeoutExpired, FileNotFoundError):
            pass

    return None


def _convert_via_word(docx_path: Path, pdf_path: Path) -> bool:
    """使用 Microsoft Word 将 DOCX 转换为 PDF"""
    ps_script = Path(__file__).parent.parent / "scripts" / "docx2pdf.ps1"
    if not ps_script.exists():
        return False

    try:
        # 将 Linux 路径转换为 Windows 路径
        def _to_win(p: Path) -> str:
            try:
                result = subprocess.run(
                    ["wslpath", "-w", str(p)], capture_output=True, timeout=5
                )
                if result.returncode == 0:
                    return result.stdout.decode().strip()
            except (subprocess.TimeoutExpired, FileNotFoundError):
                pass
            return str(p).replace("/mnt/", "").replace("/", "\\")

        win_script = _to_win(ps_script)
        win_docx = _to_win(docx_path)
        win_pdf = _to_win(pdf_path)

        result = subprocess.run(
            ["powershell.exe", "-ExecutionPolicy", "Bypass", "-File", win_script,
             "-InputDocx", win_docx, "-OutputPdf", win_pdf],
            capture_output=True, timeout=120,
        )
        return result.returncode == 0 and pdf_path.exists()
    except (subprocess.TimeoutExpired, FileNotFoundError):
        return False


def _count_pdf_pages(pdf_path: Path) -> int:
    """计算 PDF 页数"""
    try:
        content = pdf_path.read_bytes()
        import re
        # 匹配 /Type /Page（但不匹配 /Type /Pages）
        pages = len(re.findall(rb'/Type\s*/Page[^s]', content))
        return max(1, pages)
    except Exception as e:
        print(f"PDF page count error: {e}")
        return 1
