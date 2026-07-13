"""Markdown→DOCX 转换引擎。

流程：
1. 用 Pandoc 将 Markdown 转为临时 DOCX（保留基本结构）
2. 用 python-docx 创建最终文档，按格式规范逐段构建
3. 合并模板封面/封底
4. 添加页眉页脚、分节符、脚注、批注
"""

from __future__ import annotations

import re
import subprocess
import tempfile
from pathlib import Path
from typing import List, Optional

from docx import Document
from docx.shared import Pt, Cm, Emu, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.section import WD_ORIENT
from docx.oxml.ns import qn, nsdecls
from docx.oxml import parse_xml
from lxml import etree

from .parser import ParsedDocument, ParsedSection, TableData, parse_markdown
from .styles import (
    ThesisConfig, PageConfig, FontConfig, SpacingConfig,
    FONT_SIMSUN, FONT_SIMHEI, FONT_FANGSONG, FONT_TIMES,
)
from .template_merge import merge_template
from .postprocess import (
    setup_headers_footers, add_section_break, add_footnotes,
    add_comments, add_toc_field,
)


# ═══════════════════════════════════════════
# Pandoc 调用
# ═══════════════════════════════════════════

def _find_pandoc() -> str:
    """查找 Pandoc 可执行文件路径"""
    # 优先使用项目指定路径
    candidates = [
        Path.home() / ".local" / "bin" / "pandoc",
        Path("/usr/local/bin/pandoc"),
        Path("/usr/bin/pandoc"),
    ]
    for p in candidates:
        if p.exists():
            return str(p)
    # 尝试 PATH
    try:
        result = subprocess.run(
            ["pandoc", "--version"], capture_output=True, text=True, timeout=5
        )
        if result.returncode == 0:
            return "pandoc"
    except (FileNotFoundError, subprocess.TimeoutExpired):
        pass
    raise FileNotFoundError(
        "Pandoc 未安装。请安装 Pandoc: https://pandoc.org/installing.html"
    )


def _pandoc_convert(md_path: Path, output_docx: Path) -> None:
    """调用 Pandoc 将 Markdown 转为 DOCX"""
    pandoc = _find_pandoc()
    cmd = [
        pandoc, str(md_path),
        "-o", str(output_docx),
        "--from", "markdown",
        "--to", "docx",
    ]
    result = subprocess.run(cmd, capture_output=True, text=True, timeout=30)
    if result.returncode != 0:
        raise RuntimeError(f"Pandoc 转换失败: {result.stderr}")


# ═══════════════════════════════════════════
# 段落构建辅助函数
# ═══════════════════════════════════════════

def _set_run_font(run, east_asian: str, latin: str, size_pt: float,
                   bold: bool = False, italic: bool = False) -> None:
    """设置 run 的字体属性"""
    run.font.size = Pt(size_pt)
    run.font.bold = bold
    run.font.italic = italic
    # 设置西文字体
    run.font.name = latin
    # 设置东亚字体
    rpr = run._element.get_or_add_rPr()
    rFonts = rpr.find(qn('w:rFonts'))
    if rFonts is None:
        rFonts = parse_xml(f'<w:rFonts {nsdecls("w")}/>')
        rpr.insert(0, rFonts)
    rFonts.set(qn('w:eastAsia'), east_asian)
    rFonts.set(qn('w:ascii'), latin)
    rFonts.set(qn('w:hAnsi'), latin)


def _set_paragraph_spacing(paragraph, before_pt: float = 0, after_pt: float = 0,
                            line_spacing: float = 1.25, first_line_chars: int = 0,
                            hanging_chars: int = 0) -> None:
    """设置段落间距和缩进"""
    pf = paragraph.paragraph_format
    pf.space_before = Pt(before_pt)
    pf.space_after = Pt(after_pt)
    pf.line_spacing = line_spacing

    if first_line_chars > 0:
        # 首行缩进：按字符数 × 字号
        font_size = paragraph.runs[0].font.size if paragraph.runs else Pt(12)
        pf.first_line_indent = int(font_size * first_line_chars)
    elif hanging_chars > 0:
        font_size = paragraph.runs[0].font.size if paragraph.runs else Pt(12)
        pf.first_line_indent = -int(font_size * hanging_chars)


def _add_paragraph(doc: Document, text: str, font_cfg: FontConfig,
                    spacing_cfg: SpacingConfig, alignment=None,
                    east_asian: str = None, latin: str = None,
                    size_pt: float = None, bold: bool = False,
                    italic: bool = False, first_line_chars: int = 0,
                    hanging_chars: int = 0,
                    before_pt: float = 0, after_pt: float = 0) -> 'Paragraph':
    """添加一个格式化段落"""
    para = doc.add_paragraph()
    if alignment is not None:
        para.alignment = alignment
    run = para.add_run(text)
    _set_run_font(
        run,
        east_asian or font_cfg.body_east_asian,
        latin or font_cfg.body_latin,
        size_pt or font_cfg.body_size_pt,
        bold=bold, italic=italic,
    )
    _set_paragraph_spacing(
        para,
        before_pt=before_pt, after_pt=after_pt,
        line_spacing=spacing_cfg.line_spacing,
        first_line_chars=first_line_chars,
        hanging_chars=hanging_chars,
    )
    return para


def _add_body_paragraph(doc: Document, text: str, cfg: ThesisConfig) -> 'Paragraph':
    """添加正文段落（宋体小四，首行缩进2字符）"""
    return _add_paragraph(
        doc, text, cfg.fonts, cfg.spacing,
        first_line_chars=cfg.spacing.first_line_indent_chars,
    )


def _add_heading1(doc: Document, title: str, cfg: ThesisConfig) -> 'Paragraph':
    """添加一级标题（黑体三号，居中，段前段后各1行）"""
    return _add_paragraph(
        doc, title, cfg.fonts, cfg.spacing,
        alignment=WD_ALIGN_PARAGRAPH.CENTER,
        east_asian=cfg.fonts.heading1_font,
        size_pt=cfg.fonts.heading1_size_pt,
        bold=True,
        before_pt=cfg.spacing.heading1_before_pt,
        after_pt=cfg.spacing.heading1_after_pt,
    )


def _add_heading2(doc: Document, title: str, cfg: ThesisConfig) -> 'Paragraph':
    """添加二级标题（宋体四号加粗，左对齐）"""
    return _add_paragraph(
        doc, title, cfg.fonts, cfg.spacing,
        east_asian=cfg.fonts.heading2_font,
        size_pt=cfg.fonts.heading2_size_pt,
        bold=cfg.fonts.heading2_bold,
        before_pt=cfg.spacing.heading2_before_pt,
        after_pt=cfg.spacing.heading2_after_pt,
    )


def _add_heading3(doc: Document, title: str, cfg: ThesisConfig) -> 'Paragraph':
    """添加三级标题（宋体小四号加粗，左对齐）"""
    return _add_paragraph(
        doc, title, cfg.fonts, cfg.spacing,
        east_asian=cfg.fonts.heading3_font,
        size_pt=cfg.fonts.heading3_size_pt,
        bold=cfg.fonts.heading3_bold,
        before_pt=cfg.spacing.heading3_before_pt,
        after_pt=cfg.spacing.heading3_after_pt,
    )


# ═══════════════════════════════════════════
# 特殊章节构建
# ═══════════════════════════════════════════

def _build_abstract_cn(doc: Document, parsed: ParsedDocument, cfg: ThesisConfig) -> None:
    """构建中文摘要"""
    # 标题：摘  要（中间空2格）
    para = _add_paragraph(
        doc, "摘  要", cfg.fonts, cfg.spacing,
        alignment=WD_ALIGN_PARAGRAPH.CENTER,
        east_asian=cfg.fonts.abstract_title_font,
        size_pt=cfg.fonts.abstract_title_size_pt,
        bold=True,
        before_pt=cfg.spacing.abstract_before_pt,
        after_pt=cfg.spacing.abstract_after_pt,
    )
    # 正文
    content = parsed.get_abstract_content()
    if content:
        _add_body_paragraph(doc, content, cfg)
    # 关键词
    keywords = parsed.get_abstract_keywords()
    if keywords:
        kw_para = doc.add_paragraph()
        kw_para.paragraph_format.space_before = Pt(cfg.spacing.keywords_before_pt)
        kw_para.paragraph_format.line_spacing = cfg.spacing.line_spacing
        # "关键词"标签（黑体四号）
        label_run = kw_para.add_run("关键词：")
        _set_run_font(label_run, cfg.fonts.keywords_label_font,
                      FONT_TIMES, cfg.fonts.keywords_label_size_pt, bold=True)
        # 关键词内容（宋体小四号）
        kw_run = kw_para.add_run(keywords)
        _set_run_font(kw_run, cfg.fonts.body_east_asian,
                      cfg.fonts.body_latin, cfg.fonts.body_size_pt)


def _build_abstract_en(doc: Document, parsed: ParsedDocument, cfg: ThesisConfig) -> None:
    """构建英文摘要"""
    # 标题：Abstract（Times New Roman 三号加粗）
    para = _add_paragraph(
        doc, "Abstract", cfg.fonts, cfg.spacing,
        alignment=WD_ALIGN_PARAGRAPH.CENTER,
        east_asian=cfg.fonts.eng_abstract_title_font,
        latin=cfg.fonts.eng_abstract_title_font,
        size_pt=cfg.fonts.eng_abstract_title_size_pt,
        bold=True,
        before_pt=cfg.spacing.abstract_before_pt,
        after_pt=cfg.spacing.abstract_after_pt,
    )
    # 正文
    content = parsed.get_eng_abstract_content()
    if content:
        para = doc.add_paragraph()
        para.paragraph_format.first_line_indent = Pt(cfg.fonts.body_size_pt * 2)
        para.paragraph_format.line_spacing = cfg.spacing.line_spacing
        run = para.add_run(content)
        _set_run_font(run, FONT_SIMSUN, FONT_TIMES, cfg.fonts.body_size_pt)
    # 英文关键词
    keywords = parsed.get_eng_keywords()
    if keywords:
        kw_para = doc.add_paragraph()
        kw_para.paragraph_format.space_before = Pt(cfg.spacing.keywords_before_pt)
        kw_para.paragraph_format.line_spacing = cfg.spacing.line_spacing
        # "Keywords:" 标签（Times New Roman 四号加粗）
        label_run = kw_para.add_run("Keywords: ")
        _set_run_font(label_run, FONT_TIMES, FONT_TIMES,
                      cfg.fonts.keywords_label_size_pt, bold=True)
        # 关键词内容（小四号）
        kw_run = kw_para.add_run(keywords)
        _set_run_font(kw_run, FONT_SIMSUN, FONT_TIMES, cfg.fonts.body_size_pt)


def _build_toc(doc: Document, cfg: ThesisConfig) -> None:
    """构建目录页"""
    # 标题：目  录
    _add_paragraph(
        doc, "目  录", cfg.fonts, cfg.spacing,
        alignment=WD_ALIGN_PARAGRAPH.CENTER,
        east_asian=FONT_SIMSUN,
        size_pt=cfg.fonts.abstract_title_size_pt,
        bold=True,
        before_pt=cfg.spacing.abstract_before_pt,
        after_pt=cfg.spacing.abstract_after_pt,
    )
    # 添加 TOC 域代码
    add_toc_field(doc)


def _build_references(doc: Document, refs: List[str], cfg: ThesisConfig) -> None:
    """构建参考文献"""
    if not refs:
        return
    for ref_text in refs:
        # 去除原始编号，由 Word 自动编号
        content = re.sub(r'^\[\d+\]\s*', '', ref_text)
        para = doc.add_paragraph()
        para.paragraph_format.line_spacing = cfg.spacing.line_spacing
        # 悬挂缩进
        para.paragraph_format.first_line_indent = Pt(
            -(cfg.fonts.ref_size_pt * cfg.spacing.ref_hanging_indent_chars)
        )
        para.paragraph_format.left_indent = Pt(
            cfg.fonts.ref_size_pt * cfg.spacing.ref_hanging_indent_chars
        )

        # 判断是否为外文文献
        if _is_foreign_ref(ref_text):
            m = re.match(r'^([A-Za-z][^.]*\.\s*)(.*?)(\[)', content)
            if m:
                run1 = para.add_run(m.group(1))
                _set_run_font(run1, FONT_SIMSUN, FONT_TIMES, cfg.fonts.ref_size_pt)
                run2 = para.add_run(m.group(2))
                _set_run_font(run2, FONT_TIMES, FONT_TIMES, cfg.fonts.ref_size_pt, italic=True)
                run3 = para.add_run(content[m.start(3):])
                _set_run_font(run3, FONT_SIMSUN, FONT_TIMES, cfg.fonts.ref_size_pt)
            else:
                run = para.add_run(content)
                _set_run_font(run, FONT_SIMSUN, FONT_TIMES, cfg.fonts.ref_size_pt)
        else:
            run = para.add_run(content)
            _set_run_font(run, FONT_SIMSUN, FONT_TIMES, cfg.fonts.ref_size_pt)


def _is_foreign_ref(text: str) -> bool:
    """判断是否为外文参考文献"""
    content = re.sub(r'^\[\d+\]\s*', '', text)
    m = re.match(r'^([A-Za-z][^.]*\.\s*)', content)
    if not m:
        return False
    author = m.group(1)
    latin_count = sum(1 for c in author if c.isalpha())
    return latin_count > len(author) * 0.6


def _build_body_section(doc: Document, sec: ParsedSection, cfg: ThesisConfig,
                         input_dir: Path, img_counter: dict) -> None:
    """构建正文章节"""
    # 标题
    if sec.level == 1:
        _add_heading1(doc, sec.title, cfg)
        # 新章节：递增章号，重置图片计数
        img_counter['chapter'] += 1
        img_counter['img'] = 0
        img_counter['table'] = 0
    elif sec.level == 2:
        _add_heading2(doc, sec.title, cfg)
    elif sec.level == 3:
        _add_heading3(doc, sec.title, cfg)

    # 正文内容
    for line in sec.content.split('\n'):
        trimmed = line.strip()
        if not trimmed or trimmed.startswith('#'):
            continue

        # 清理 HTML 标签
        trimmed = re.sub(r'<br\s*/?>', '', trimmed, flags=re.IGNORECASE).strip()
        if not trimmed:
            continue

        # 处理图片引用 ![caption](path)
        img_match = re.match(r'^!\[(.*?)\]\((.*?)\)', trimmed)
        if img_match:
            caption = img_match.group(1)
            img_path = img_match.group(2)
            if not Path(img_path).is_absolute():
                img_path = input_dir / img_path
            img_counter['img'] += 1
            auto_cap = f"图{img_counter['chapter']}-{img_counter['img']}"
            if caption:
                auto_cap += f" {caption}"
            _add_image(doc, Path(img_path), auto_cap, cfg)
            continue

        # 处理中文图片引用 "图 X-X 描述"
        if re.match(r'^图\s*\d', trimmed):
            img_dir = input_dir / "photo"
            if img_dir.exists():
                imgs = sorted(img_dir.iterdir(), key=lambda f: _extract_num(f.name))
                if img_counter['photo_idx'] < len(imgs):
                    img_counter['img'] += 1
                    auto_cap = f"图{img_counter['chapter']}-{img_counter['img']}"
                    # 从原文提取描述（去掉已有的"图X-X"前缀）
                    desc = re.sub(r'^图\s*[\d\-]+\s*', '', trimmed).strip()
                    if desc:
                        auto_cap += f" {desc}"
                    _add_image(doc, imgs[img_counter['photo_idx']], auto_cap, cfg)
                    img_counter['photo_idx'] += 1
                else:
                    _add_body_paragraph(doc, trimmed, cfg)
            else:
                _add_body_paragraph(doc, trimmed, cfg)
            continue

        # 处理脚注标记 [^N]
        if re.search(r'\[\^\d+\]', trimmed):
            _add_paragraph_with_footnotes(doc, trimmed, cfg)

        # 处理表格占位符 <!--TABLE:N-->
        elif re.match(r'^<!--TABLE:(\d+)-->$', trimmed):
            table_idx = int(re.match(r'^<!--TABLE:(\d+)-->$', trimmed).group(1))
            if table_idx < len(sec.tables):
                td = sec.tables[table_idx]
                img_counter['table'] += 1
                # 如果没有表标题，自动生成 "表X-Y"
                if not td.caption:
                    td.caption = f"表{img_counter['chapter']}-{img_counter['table']}"
                _build_table(doc, td, cfg)
            else:
                _add_body_paragraph(doc, f"[表格数据缺失: index={table_idx}]", cfg)

        else:
            _add_body_paragraph(doc, trimmed, cfg)


def _extract_num(filename: str) -> int:
    """从文件名中提取数字用于排序"""
    m = re.search(r'\d+', filename)
    return int(m.group()) if m else 0


def _add_image(doc: Document, img_path: Path, caption: str, cfg: ThesisConfig,
                max_width_inches: float = 4) -> None:
    """插入图片和图标题"""
    if not img_path.exists():
        # 图片不存在时添加占位文本
        _add_body_paragraph(doc, f"[图片未找到: {img_path}]", cfg)
        return

    # 插入图片（居中）
    para = doc.add_paragraph()
    para.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run = para.add_run()
    run.add_picture(str(img_path), width=Inches(max_width_inches))

    # 图标题（仿宋五号，居中）
    cap_para = doc.add_paragraph()
    cap_para.alignment = WD_ALIGN_PARAGRAPH.CENTER
    cap_para.paragraph_format.space_after = Pt(cfg.spacing.caption_after_pt)
    cap_run = cap_para.add_run(caption)
    _set_run_font(cap_run, cfg.fonts.caption_font, FONT_TIMES, cfg.fonts.caption_size_pt)


def _add_paragraph_with_footnotes(doc: Document, text: str, cfg: ThesisConfig) -> None:
    """添加带脚注标记的段落"""
    # 清除脚注标记后添加正文
    clean = re.sub(r'\[\^\d+\]', '', text).strip()
    if clean:
        _add_body_paragraph(doc, clean, cfg)
    # 脚注将在 postprocess 中统一处理


def _set_cell_border(cell, **kwargs) -> None:
    """设置单元格边框。

    kwargs: top, bottom, left, right — 每个值为 dict，包含:
        sz (int): 线宽（1/8 磅），如 12 = 1.5pt
        val (str): 线型，默认 'single'
        color (str): 颜色，默认 '000000'
    """
    tc = cell._tc
    tcPr = tc.get_or_add_tcPr()
    tcBorders = tcPr.find(qn('w:tcBorders'))
    if tcBorders is None:
        tcBorders = parse_xml(f'<w:tcBorders {nsdecls("w")}/>')
        tcPr.append(tcBorders)
    for edge, attrs in kwargs.items():
        el = tcBorders.find(qn(f'w:{edge}'))
        if el is None:
            el = parse_xml(f'<w:{edge} {nsdecls("w")}/>')
            tcBorders.append(el)
        el.set(qn('w:val'), attrs.get('val', 'single'))
        el.set(qn('w:sz'), str(attrs.get('sz', 4)))
        el.set(qn('w:color'), attrs.get('color', '000000'))
        el.set(qn('w:space'), '0')


def _build_table(doc: Document, table_data: TableData, cfg: ThesisConfig) -> None:
    """构建三线表格。

    三线表格规则：
    - 顶线：粗线（表头上方）
    - 栏目线：粗线（表头下方）
    - 底线：粗线（表格最下方）
    - 无竖线，无其他横线
    - 表标题在表格上方：仿宋 五号(10.5pt) 居中
    """
    headers = table_data.headers
    rows = table_data.rows
    if not headers:
        return

    num_cols = len(headers)

    # ① 表标题（在表格上方）
    if table_data.caption:
        cap_para = doc.add_paragraph()
        cap_para.alignment = WD_ALIGN_PARAGRAPH.CENTER
        cap_para.paragraph_format.space_before = Pt(6)
        cap_para.paragraph_format.space_after = Pt(3)
        cap_run = cap_para.add_run(table_data.caption)
        _set_run_font(cap_run, FONT_FANGSONG, FONT_TIMES, 10.5)  # 五号

    # ② 创建表格
    num_row_data = len(rows) if rows else 0
    table = doc.add_table(rows=1 + num_row_data, cols=num_cols)

    # ③ 表格宽度设为页面可用宽度的 100%
    tbl = table._tbl
    tblPr = tbl.tblPr if tbl.tblPr is not None else parse_xml(f'<w:tblPr {nsdecls("w")}/>')
    # 移除已有 tblW
    for old in tblPr.findall(qn('w:tblW')):
        tblPr.remove(old)
    tblW = parse_xml(f'<w:tblW {nsdecls("w")} w:type="pct" w:w="5000"/>')
    tblPr.append(tblW)
    # 自动适应窗口
    for old in tblPr.findall(qn('w:tblLayout')):
        tblPr.remove(old)
    tblLayout = parse_xml(f'<w:tblLayout {nsdecls("w")} w:type="autofit"/>')
    tblPr.append(tblLayout)

    # ④ 清除表格默认边框（设为无边框）
    for old in tblPr.findall(qn('w:tblBorders')):
        tblPr.remove(old)
    no_border_xml = (
        f'<w:tblBorders {nsdecls("w")}>'
        '<w:top w:val="none" w:sz="0" w:color="auto" w:space="0"/>'
        '<w:left w:val="none" w:sz="0" w:color="auto" w:space="0"/>'
        '<w:bottom w:val="none" w:sz="0" w:color="auto" w:space="0"/>'
        '<w:right w:val="none" w:sz="0" w:color="auto" w:space="0"/>'
        '<w:insideH w:val="none" w:sz="0" w:color="auto" w:space="0"/>'
        '<w:insideV w:val="none" w:sz="0" w:color="auto" w:space="0"/>'
        '</w:tblBorders>'
    )
    tblPr.append(parse_xml(no_border_xml))

    # ⑤ 填写表头行
    header_row = table.rows[0]
    thick = {'sz': 12, 'val': 'single', 'color': '000000'}  # 12 × 1/8pt = 1.5pt
    none_b = {'sz': 0, 'val': 'none', 'color': 'auto'}
    for j, htext in enumerate(headers):
        cell = header_row.cells[j]
        # 清空默认段落
        cell.text = ""
        para = cell.paragraphs[0]
        para.alignment = WD_ALIGN_PARAGRAPH.CENTER
        run = para.add_run(htext)
        _set_run_font(run, FONT_SIMSUN, FONT_TIMES, 10.5)  # 五号
        # 表头：顶线(粗) + 底线(粗)，左右无
        _set_cell_border(cell,
                         top=thick, bottom=thick,
                         left=none_b, right=none_b)

    # ⑥ 填写数据行
    for r_idx, row_data in enumerate(rows):
        row = table.rows[1 + r_idx]
        is_last = (r_idx == len(rows) - 1)
        for j in range(num_cols):
            cell_text = row_data[j] if j < len(row_data) else ""
            cell = row.cells[j]
            cell.text = ""
            para = cell.paragraphs[0]
            para.alignment = WD_ALIGN_PARAGRAPH.CENTER
            run = para.add_run(cell_text)
            _set_run_font(run, FONT_SIMSUN, FONT_TIMES, 10.5)  # 五号
            # 数据行：默认无边框
            border_kwargs = {'top': none_b, 'bottom': none_b,
                             'left': none_b, 'right': none_b}
            # 最后一行加底线(粗)
            if is_last:
                border_kwargs['bottom'] = thick
            _set_cell_border(cell, **border_kwargs)

    # ⑦ 表格后添加空段落（间距）
    after_para = doc.add_paragraph()
    after_para.paragraph_format.space_before = Pt(3)
    after_para.paragraph_format.space_after = Pt(6)


# 需要导入 Inches
from docx.shared import Inches


# ═══════════════════════════════════════════
# 主转换函数
# ═══════════════════════════════════════════

def convert(
    input_md: str | Path,
    template_path: str | Path,
    config: Optional[dict] = None,
    output_path: Optional[str | Path] = None,
) -> dict:
    """将 Markdown 文件转换为符合山西财经大学格式规范的 DOCX。

    Args:
        input_md: 输入 Markdown 文件路径
        template_path: 学校模板 DOCX 路径
        config: 可选的格式配置覆盖
        output_path: 输出 DOCX 路径（默认自动生成）

    Returns:
        包含 success, output_path, pages, warnings 的字典
    """
    input_md = Path(input_md)
    template_path = Path(template_path)
    warnings: List[str] = []

    # 解析配置
    cfg = ThesisConfig.from_dict(config) if config else ThesisConfig()

    # 解析 Markdown
    parsed = parse_markdown(input_md)
    input_dir = input_md.parent

    # 确定输出路径
    if output_path is None:
        safe_title = re.sub(r'[<>:"/\\|?*]', '_', parsed.title)[:80]
        output_path = Path("output") / f"{safe_title}.docx"
    output_path = Path(output_path)
    output_path.parent.mkdir(parents=True, exist_ok=True)

    # 创建文档
    doc = Document()

    # 设置页面
    _setup_page(doc, cfg)

    # 设置默认字体
    _setup_default_font(doc, cfg)

    # ── 构建各章节，同时跟踪每个分节的类型 ──
    # section_types 与 doc.sections 一一对应（合并模板后封面分节会前插）
    section_types: List[str] = []

    # 如果有封面模板，在最前面预留一个空分节给封面内容
    has_cover = cfg.sections.cover and template_path.exists()
    if has_cover:
        add_section_break(doc, "odd_page")   # section 0 → 留给封面；确保中文摘要从奇数页开始

    # ── 前置部分：中文摘要 / 英文摘要 / 目录 ──
    if cfg.sections.abstract_cn:
        section_types.append("front_matter")
        _build_abstract_cn(doc, parsed, cfg)
        add_section_break(doc, "odd_page")   # 确保英文摘要从奇数页开始

    if cfg.sections.abstract_en:
        section_types.append("front_matter")
        _build_abstract_en(doc, parsed, cfg)
        add_section_break(doc, "odd_page")   # 确保目录从奇数页开始

    if cfg.sections.toc:
        section_types.append("front_matter")
        _build_toc(doc, cfg)
        add_section_break(doc, "odd_page")   # 确保正文从奇数页开始

    # ── 正文 ──
    if cfg.sections.body:
        section_types.append("body")
        img_counter = {'chapter': 0, 'img': 0, 'photo_idx': 0, 'table': 0}
        for sec in parsed.get_body_sections():
            _build_body_section(doc, sec, cfg, input_dir, img_counter)

    # ── 参考文献（独立分节）──
    if cfg.sections.references:
        refs = parsed.get_references()
        if refs:
            add_section_break(doc, "odd_page")   # 确保参考文献从奇数页开始
            section_types.append("body")
            _add_heading1(doc, "参考文献", cfg)
            _build_references(doc, refs, cfg)
        else:
            warnings.append("未找到参考文献")

    # ── 附录（独立分节）──
    if cfg.sections.appendix and parsed.has_appendix():
        add_section_break(doc, "odd_page")   # 确保附录从奇数页开始
        section_types.append("body")
        sec = parsed.find_section("附录")
        if sec:
            _add_paragraph(
                doc, "附  录", cfg.fonts, cfg.spacing,
                alignment=WD_ALIGN_PARAGRAPH.CENTER,
                east_asian=FONT_SIMSUN,
                size_pt=16, bold=True,
                before_pt=cfg.spacing.heading1_before_pt,
                after_pt=cfg.spacing.heading1_after_pt,
            )
            for line in sec.content.split('\n'):
                if line.strip():
                    _add_body_paragraph(doc, line.strip(), cfg)

    # ── 致谢（独立分节）──
    if cfg.sections.acknowledgment:
        add_section_break(doc, "odd_page")   # 确保致谢从奇数页开始
        section_types.append("body")
        ack = parsed.get_acknowledgment_content()
        _add_paragraph(
            doc, "致  谢", cfg.fonts, cfg.spacing,
            alignment=WD_ALIGN_PARAGRAPH.CENTER,
            east_asian=FONT_SIMSUN,
            size_pt=16, bold=True,
            before_pt=cfg.spacing.heading1_before_pt,
            after_pt=cfg.spacing.heading1_after_pt,
        )
        if ack:
            _add_body_paragraph(doc, ack, cfg)
        else:
            _add_body_paragraph(
                doc,
                "本论文的写作过程是一次系统学习学术研究方法的宝贵经历。"
                "在导师的悉心指导下，我对相关领域有了更深入的认识，"
                "也初步掌握了文献研究、案例比较等基本方法。"
                "感谢导师在选题确定、资料搜集、论文修改各环节给予的耐心指导，"
                "感谢同学们在讨论交流中提供的启发与帮助。"
                "今后将继续努力，不断提升自身的学术素养与研究能力。",
                cfg,
            )

    # 如果有封面模板，在末尾预留一个空分节给封底
    if has_cover:
        add_section_break(doc)
        section_types.append("cover")    # 封底

    # ── 合并模板封面/封底 ──
    if has_cover:
        try:
            merge_template(doc, template_path, parsed.metadata)
            # 模板封面被插入到最前面的空分节（section 0），类型为 cover
            section_types = ["cover"] + section_types
        except Exception as e:
            warnings.append(f"模板合并失败: {e}")

    # ── 设置页眉页脚（传入分节类型列表）──
    grade = parsed.get_meta("studentId", "202300000000")[:4]
    setup_headers_footers(doc, cfg, grade, section_types)

    # 批注已禁用（预览和打印时不需要批注）

    # 保存
    doc.save(str(output_path))

    return {
        "success": True,
        "output_path": str(output_path),
        "preview_url": f"/api/preview/{output_path.stem}",
        "pages": _estimate_pages(doc),
        "warnings": warnings,
    }


def _setup_page(doc: Document, cfg: ThesisConfig) -> None:
    """设置页面尺寸、页边距和文档网格"""
    section = doc.sections[0]
    section.page_width = Cm(cfg.page.width_cm)
    section.page_height = Cm(cfg.page.height_cm)
    section.top_margin = Cm(cfg.page.margin_top_cm)
    section.bottom_margin = Cm(cfg.page.margin_bottom_cm)
    section.left_margin = Cm(cfg.page.margin_left_cm)
    section.right_margin = Cm(cfg.page.margin_right_cm)
    section.header_distance = Cm(1.27)  # 页眉距边界
    section.footer_distance = Cm(1.27)  # 页脚距边界

    # 设置文档网格（指定行和字符网格）
    # 每行44字符，间距10.5磅；每页43行，间距15.6磅
    sectPr = section._sectPr
    # 移除已有的 docGrid（如果有）
    for old in sectPr.findall(qn('w:docGrid')):
        sectPr.remove(old)
    docGrid = etree.SubElement(sectPr, qn('w:docGrid'))
    docGrid.set(qn('w:type'), 'linesAndChars')
    docGrid.set(qn('w:linePitch'), '312')    # 15.6pt × 20 = 312 twips
    docGrid.set(qn('w:charSpace'), '210')    # 10.5pt × 20 = 210 half-points
    docGrid.set(qn('w:charsPerLine'), '44')
    docGrid.set(qn('w:linesPerPage'), '43')


def _setup_default_font(doc: Document, cfg: ThesisConfig) -> None:
    """设置文档默认字体"""
    style = doc.styles['Normal']
    font = style.font
    font.name = cfg.fonts.body_latin
    font.size = Pt(cfg.fonts.body_size_pt)
    # 设置东亚字体
    rpr = style.element.get_or_add_rPr()
    rFonts = rpr.find(qn('w:rFonts'))
    if rFonts is None:
        rFonts = parse_xml(f'<w:rFonts {nsdecls("w")}/>')
        rpr.insert(0, rFonts)
    rFonts.set(qn('w:eastAsia'), cfg.fonts.body_east_asian)


def _estimate_pages(doc: Document) -> int:
    """粗略估算页数（基于段落数量）"""
    para_count = len(doc.paragraphs)
    # A4 约 30 行/页，每行约 35 字
    # 粗略按每页 25 个段落估算
    return max(1, para_count // 25)
