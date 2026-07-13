"""模板合并：从学校模板 DOCX 提取封面+封底，合并到输出文档。

对应 C# 中的 MergeTemplate / FillMajorCode / FillCoverTable / CopyImages 等函数。
"""

from __future__ import annotations

import re
from copy import deepcopy
from pathlib import Path
from typing import Dict, List, Optional

from docx import Document
from docx.oxml.ns import qn
from lxml import etree
from docx.opc.constants import RELATIONSHIP_TYPE as RT


# ═══════════════════════════════════════════
# 公共 API
# ═══════════════════════════════════════════

def merge_template(
    out_doc: Document,
    template_path: str | Path,
    metadata: Dict[str, str],
) -> None:
    """将学校模板的封面、说明页、学术承诺合并到输出文档。

    模板结构：
    - [0..14]: 封面（学校代码、标题、信息表）
    - [15..sect_pr_indices[0]]: 说明页
    - [sect_pr_indices[0]+1..sect_pr_indices[1]]: 学术承诺、使用授权
    """
    template_path = Path(template_path)
    if not template_path.exists():
        return

    tpl_doc = Document(str(template_path))
    tpl_body = tpl_doc.element.body

    # 找到模板中的分节符位置
    sect_pr_indices = _find_section_breaks(tpl_body)
    print(f"[template_merge] Template section breaks: {sect_pr_indices}")
    if len(sect_pr_indices) < 2:
        return

    # 模板结构分析：
    # [0..14]: 封面（学校代码、标题、信息表）
    # [15..sect_pr_indices[0]]: 说明页
    # [sect_pr_indices[0]+1..sect_pr_indices[1]]: 学术承诺、使用授权
    cover_end_idx = 14  # 封面只到索引14（表格之后）
    explanation_start_idx = 15  # 说明页从索引15开始
    explanation_end_idx = sect_pr_indices[0]  # 说明页到第一个分节符
    pledge_start_idx = sect_pr_indices[0] + 1  # 学术承诺从第一个分节符后开始
    pledge_end_idx = sect_pr_indices[1]  # 学术承诺到第二个分节符

    # 提取封面元素并插入到输出文档开头
    out_body = out_doc.element.body
    first_child = _get_first_content_child(out_body)
    rid_map = _build_rid_map(tpl_doc, out_doc)
    
    # 1. 插入封面（索引 0-14）
    cover_elements = _extract_elements(tpl_body, 0, cover_end_idx + 1, rid_map)
    for el in cover_elements:
        _fix_signature_underlines(el)
        if first_child is not None:
            out_body.insert(out_body.index(first_child), el)
        else:
            out_body.append(el)
    print(f"[template_merge] Inserted {len(cover_elements)} cover elements")

    # 2. 插入说明页（索引 15 到第一个分节符）
    explanation_elements = _extract_elements(tpl_body, explanation_start_idx, explanation_end_idx + 1, rid_map)
    # 找到摘要段落，在它之前插入说明页和学术承诺
    abstract_para = None
    for p in out_body.iter(qn('w:p')):
        text = _get_element_text(p)
        if '摘' in text and '要' in text:
            abstract_para = p
            break
    
    if abstract_para is not None:
        for el in explanation_elements:
            out_body.insert(out_body.index(abstract_para), el)
        print(f"[template_merge] Inserted {len(explanation_elements)} explanation elements")
    
    # 3. 插入学术承诺（第一个分节符后到第二个分节符）
    pledge_elements = _extract_elements(tpl_body, pledge_start_idx, pledge_end_idx + 1, rid_map)
    if abstract_para is not None:
        for el in pledge_elements:
            out_body.insert(out_body.index(abstract_para), el)
        print(f"[template_merge] Inserted {len(pledge_elements)} pledge elements")

    # 填写专业代码
    major_code = metadata.get("majorCode", "120210")
    _fill_major_code(out_body, major_code)

    # 填写封面表格
    _fill_cover_table(out_body, metadata)
    
    # 封面表格防分页处理
    _fix_cover_table_anti_page_break(out_body)


# ═══════════════════════════════════════════
# 内部实现
# ═══════════════════════════════════════════

def _find_section_breaks(body) -> List[int]:
    """查找 body 中所有包含 sectPr 的段落索引"""
    indices = []
    for i, child in enumerate(body):
        # 检查段落属性中的 sectPr
        pPr = child.find(qn('w:pPr'))
        if pPr is not None and pPr.find(qn('w:sectPr')) is not None:
            indices.append(i)
        # 检查 body 级别的 sectPr
        if child.tag == qn('w:sectPr'):
            indices.append(i)
    return indices


def _get_first_content_child(body):
    """获取 body 中第一个非 sectPr 的子元素"""
    for child in body:
        if child.tag != qn('w:sectPr'):
            return child
    return None


def _extract_elements(body, start_idx: int, end_idx: Optional[int], rid_map: Optional[Dict[str, str]] = None) -> List:
    """提取 body 中指定范围的元素（深度拷贝，去除 sectPr，保留并修正图片引用）"""
    children = list(body)
    if end_idx is None:
        end_idx = len(children)
    end_idx = min(end_idx, len(children))

    elements = []
    for i in range(start_idx, end_idx):
        child = children[i]
        if child.tag == qn('w:sectPr'):
            continue
        el = deepcopy(child)
        # 移除段落中的 sectPr
        pPr = el.find(qn('w:pPr'))
        if pPr is not None:
            sect_pr = pPr.find(qn('w:sectPr'))
            if sect_pr is not None:
                pPr.remove(sect_pr)
                run = el.makeelement(qn('w:r'), {})
                br = run.makeelement(qn('w:br'), {qn('w:type'): 'page'})
                run.append(br)
                el.append(run)
        # 更新图片引用的 rId，使其指向输出文档中的图片
        if rid_map:
            _update_image_refs(el, rid_map)
        elements.append(el)
    return elements


def _build_rid_map(src_doc: Document, dst_doc: Document) -> Dict[str, str]:
    """将模板文档中的图片关系复制到输出文档，返回 {old_rId: new_rId} 映射。"""
    rid_map: Dict[str, str] = {}
    dst_part = dst_doc.part
    for rel_id, rel in src_doc.part.rels.items():
        if rel.reltype != RT.IMAGE:
            continue
        try:
            image_part = rel.target_part
            new_rid = dst_part.relate_to(image_part, RT.IMAGE)
            rid_map[rel_id] = new_rid
        except Exception:
            pass
    return rid_map


def _update_image_refs(element, rid_map: Dict[str, str]) -> None:
    """更新 XML 元素中所有图片引用的 rId（a:blip r:embed 属性）。"""
    nsmap_r = '{http://schemas.openxmlformats.org/officeDocument/2006/relationships}'
    for blip in element.iter(qn('a:blip')):
        old_rid = blip.get(nsmap_r + 'embed')
        if old_rid and old_rid in rid_map:
            blip.set(nsmap_r + 'embed', rid_map[old_rid])


def _fix_signature_underlines(element) -> None:
    """修复签名处的下划线长度（对应 C# FixSignatureUnderlines）"""
    for p in element.iter(qn('w:p')):
        p_text = _get_element_text(p)
        if not any(kw in p_text for kw in ('签名', '期', '指导教师')):
            continue

        for r in p.findall(qn('w:r')):
            rPr = r.find(qn('w:rPr'))
            if rPr is None:
                continue
            u = rPr.find(qn('w:u'))
            if u is None:
                continue
            t = r.find(qn('w:t'))
            if t is None or t.text is None:
                continue

            text = t.text
            if text.strip() == '' and len(text) > 0:
                t.text = ' ' * max(24, len(text) * 2)
                t.set(qn('xml:space'), 'preserve')
            elif (text.strip() and ' ' in text
                  and len(text) - len(text.rstrip()) > len(text.strip())):
                trimmed = text.rstrip()
                space_count = len(text) - len(trimmed)
                t.text = trimmed + ' ' * max(24, space_count * 2)
                t.set(qn('xml:space'), 'preserve')


def _fill_major_code(body, major_code: str) -> None:
    """填写专业代码（对应 C# FillMajorCode）"""
    for p in body.iter(qn('w:p')):
        p_text = _get_element_text(p)
        if '专业代码' not in p_text:
            continue

        runs = p.findall(qn('w:r'))
        for i, r in enumerate(runs):
            t = r.find(qn('w:t'))
            if t is None or t.text is None or '专业代码' not in t.text:
                continue

            # 下一个 run 应该是下划线占位符
            if i + 1 >= len(runs):
                break
            next_run = runs[i + 1]
            next_t = next_run.find(qn('w:t'))
            if next_t is None:
                continue

            # 居中填入专业代码（总宽度15字符）
            total_width = 15
            pad_left = (total_width - len(major_code)) // 2
            pad_right = total_width - len(major_code) - pad_left
            next_t.text = ' ' * pad_left + major_code + ' ' * pad_right
            next_t.set(qn('xml:space'), 'preserve')
            break


def _fill_cover_table(body, metadata: Dict[str, str]) -> None:
    """填写封面表格字段（对应 C# FillCoverTable）"""
    tbl = body.find(qn('w:tbl'))
    if tbl is None:
        return

    field_map = {
        '中文题目': ('title', False),
        '英文题目': ('engTitle', True),
        '姓名': ('name', False),
        '学号': ('studentId', False),
        '班级': ('class', False),
        '专业': ('major', False),
        '学院': ('college', False),
        '指导教师': ('advisor', False),
        '完成时间': ('date', False),
    }

    for row in tbl.findall(qn('w:tr')):
        cells = row.findall(qn('w:tc'))
        if len(cells) < 3:
            continue

        # 在前两列中查找标签
        row_label = ''
        for c in range(min(2, len(cells))):
            cell_text = _get_element_text(cells[c]).strip()
            if cell_text:
                row_label = cell_text
                break
        if not row_label:
            continue

        # 匹配字段
        if row_label not in field_map:
            continue
        meta_key, is_english = field_map[row_label]
        value = metadata.get(meta_key, '')

        # 如果没有值，使用默认值
        defaults = {
            'title': '论文标题',
            'engTitle': 'Paper Title',
            'name': '姓名',
            'studentId': '学号',
            'class': '班级',
            'major': '专业',
            'college': '学院',
            'advisor': '指导教师',
            'date': '2026年5月',
        }
        if not value:
            value = defaults.get(meta_key, '')

        # 清空目标单元格并写入新内容
        target_cell = cells[2]
        # 移除现有段落
        for old_p in target_cell.findall(qn('w:p')):
            target_cell.remove(old_p)

        # 创建新段落
        p = target_cell.makeelement(qn('w:p'), {})
        pPr = p.makeelement(qn('w:pPr'), {})
        spacing = pPr.makeelement(qn('w:spacing'), {
            qn('w:line'): '360',
            qn('w:lineRule'): 'auto',
        })
        pPr.append(spacing)
        jc = pPr.makeelement(qn('w:jc'), {
            qn('w:val'): 'left' if is_english else 'center',
        })
        pPr.append(jc)
        p.append(pPr)

        if is_english:
            # 英文题目：Times New Roman 斜体
            run = p.makeelement(qn('w:r'), {})
            rPr = run.makeelement(qn('w:rPr'), {})
            rFonts = rPr.makeelement(qn('w:rFonts'), {
                qn('w:ascii'): 'Times New Roman',
                qn('w:hAnsi'): 'Times New Roman',
                qn('w:eastAsia'): 'Times New Roman',
            })
            rPr.append(rFonts)
            italic = rPr.makeelement(qn('w:i'), {})
            rPr.append(italic)
            sz = rPr.makeelement(qn('w:sz'), {qn('w:val'): '28'})
            rPr.append(sz)
            run.append(rPr)
            t = run.makeelement(qn('w:t'), {})
            t.text = value
            t.set(qn('xml:space'), 'preserve')
            run.append(t)
            p.append(run)
        else:
            # 中文字段：区分中英文字符，分别设置字体
            _add_mixed_run(p, value)

        target_cell.append(p)


def _add_mixed_run(parent, text: str) -> None:
    """为混合中英文的文本创建 run，中文用宋体，英文/数字用 Times New Roman"""
    # 按字符类型分组
    segments = []
    current = ''
    is_latin = None

    for ch in text:
        ch_is_latin = (
            ('0' <= ch <= '9') or ('a' <= ch <= 'z') or ('A' <= ch <= 'Z')
            or ch in '/-. '
        )
        if ch == ' ':
            ch_is_latin = is_latin  # 空格跟随前一个字符类型

        if is_latin is None:
            is_latin = ch_is_latin
            current = ch
        elif ch_is_latin == is_latin:
            current += ch
        else:
            segments.append((current, is_latin))
            current = ch
            is_latin = ch_is_latin

    if current:
        segments.append((current, is_latin))

    for seg_text, seg_is_latin in segments:
        run = parent.makeelement(qn('w:r'), {})
        rPr = run.makeelement(qn('w:rPr'), {})
        bold = rPr.makeelement(qn('w:b'), {})
        rPr.append(bold)
        sz = rPr.makeelement(qn('w:sz'), {qn('w:val'): '28'})
        rPr.append(sz)
        szCs = rPr.makeelement(qn('w:szCs'), {qn('w:val'): '28'})
        rPr.append(szCs)

        if seg_is_latin:
            rFonts = rPr.makeelement(qn('w:rFonts'), {
                qn('w:ascii'): 'Times New Roman',
                qn('w:hAnsi'): 'Times New Roman',
                qn('w:eastAsia'): 'Times New Roman',
            })
        else:
            rFonts = rPr.makeelement(qn('w:rFonts'), {
                qn('w:ascii'): 'Times New Roman',
                qn('w:hAnsi'): 'Times New Roman',
                qn('w:eastAsia'): 'SimSun',
            })
        rPr.append(rFonts)
        run.append(rPr)

        t = run.makeelement(qn('w:t'), {})
        t.text = seg_text
        t.set(qn('xml:space'), 'preserve')
        run.append(t)
        parent.append(run)


def _get_element_text(element) -> str:
    """递归获取元素的全部文本内容"""
    texts = []
    for t in element.iter(qn('w:t')):
        if t.text:
            texts.append(t.text)
    return ''.join(texts)


def _fix_cover_table_anti_page_break(body) -> None:
    """封面表格防分页处理：禁止行跨页断行、删除段落分页约束"""
    # 找到第一个表格（封面表格）
    tbl = body.find(qn('w:tbl'))
    if tbl is None:
        return
    
    # 遍历所有行
    for row in tbl.findall(qn('w:tr')):
        # 获取或创建行属性
        trPr = row.find(qn('w:trPr'))
        if trPr is None:
            trPr = row.makeelement(qn('w:trPr'), {})
            row.insert(0, trPr)
        
        # 禁止跨页断行（cantSplit）
        if trPr.find(qn('w:cantSplit')) is None:
            cantSplit = trPr.makeelement(qn('w:cantSplit'), {})
            trPr.append(cantSplit)
        
        # 行高设为最小值（删除固定行高）
        trHeight = trPr.find(qn('w:trHeight'))
        if trHeight is not None:
            hRule = trHeight.get(qn('w:hRule'))
            if hRule == 'exact':
                # 改为 atLeast
                trHeight.set(qn('w:hRule'), 'atLeast')
        
        # 遍历单元格内的段落，删除分页约束
        for cell in row.findall(qn('w:tc')):
            for para in cell.findall(qn('w:p')):
                pPr = para.find(qn('w:pPr'))
                if pPr is None:
                    continue
                # 删除段前分页、段中不分页、与下段同页
                for tag in ['w:pageBreakBefore', 'w:keepLines', 'w:keepNext']:
                    elem = pPr.find(qn(tag))
                    if elem is not None:
                        pPr.remove(elem)
    
    print("[template_merge] Applied anti-page-break settings to cover table")
