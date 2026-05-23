"""
DOCX → 锚点 Markdown 提取器
用法: python extract.py <docx_path> [output_dir]
输出: output_dir/论文标题/paper.md + meta.json + assets/images/

纯 zipfile + lxml 实现，无 python-docx 依赖。
"""

import zipfile
import json
import os
import sys
import re
import shutil
from pathlib import Path
from datetime import datetime
from lxml import etree

# ── OOXML 命名空间 ──
NSMAP = {
    'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main',
    'r': 'http://schemas.openxmlformats.org/officeDocument/2006/relationships',
    'wp': 'http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing',
    'a': 'http://schemas.openxmlformats.org/drawingml/2006/main',
    'pic': 'http://schemas.openxmlformats.org/drawingml/2006/picture',
    'm': 'http://schemas.openxmlformats.org/officeDocument/2006/math',
    'mc': 'http://schemas.openxmlformats.org/markup-compatibility/2006',
    'w14': 'http://schemas.microsoft.com/office/word/2010/wordml',
    'wpc': 'http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas',
    'v': 'urn:schemas-microsoft-com:vml',
}

def q(tag):
    """快捷命名空间查询, 如 q('w:p') → {ns}p"""
    if ':' in tag:
        ns, name = tag.split(':')
        return f'{{{NSMAP[ns]}}}{name}'
    return tag

class DocxExtractor:
    def __init__(self, docx_path):
        self.docx_path = Path(docx_path)
        self.zf = zipfile.ZipFile(docx_path, 'r')
        self.document_xml = etree.parse(self.zf.open('word/document.xml'))
        self.block_id = 0
        self.image_map = {}  # rId → image_path
        self.blocks = []
        self.block_map = {}  # block_id → body 元素索引
        self.title = "untitled"
        self._parse_rels()

    def _parse_rels(self):
        """解析关系文件, 建立 rId → 图片路径映射"""
        try:
            rels_xml = etree.parse(self.zf.open('word/_rels/document.xml.rels'))
            for rel in rels_xml.getroot():
                rtype = rel.get('Type', '')
                target = rel.get('Target', '')
                rid = rel.get('Id', '')
                if 'image' in rtype:
                    self.image_map[rid] = f"assets/images/{Path(target).name}"
        except:
            pass

    def _next_id(self, prefix='p', element_index=None):
        self.block_id += 1
        bid = f"{prefix}{self.block_id}"
        if element_index is not None:
            self.block_map[bid] = element_index
        return bid

    def _block_comment(self, bid):
        return f"<!-- block:{bid} -->"

    def _get_text(self, element, parent_tag=''):
        """递归提取元素文本, 处理 r/t、tab、br 等"""
        if element.tag == q('w:r'):
            texts = []
            for child in element:
                if child.tag == q('w:t'):
                    t = child.text or ''
                    if child.get('{http://www.w3.org/XML/1998/namespace}space') == 'preserve':
                        texts.append(t)
                    else:
                        texts.append(t)
                elif child.tag == q('w:tab'):
                    texts.append('\t')
                elif child.tag == q('w:br'):
                    texts.append('\n')
            return ''.join(texts)
        elif element.tag in (q('w:pPr'), q('w:rPr'), q('w:tblPr'), q('w:tcPr'),
                              q('w:trPr'), q('w:tblGrid'), q('w:sectPr')):
            return ''
        else:
            return ''.join(self._get_text(c, element.tag) for c in element)

    def _has_content(self, element):
        text = self._get_text(element).strip()
        # 排除画布、形状等非内容元素
        for bad in element.iter(q('wpc:wpc'), q('mc:AlternateContent'), q('v:shape')):
            pass
        return len(text) > 0

    def _extract_paragraph(self, para, element_index=None, in_table=False):
        """提取段落: 标题/正文/图片/公式"""
        text = self._get_text(para).strip()
        pPr = para.find(q('w:pPr'))

        # 检查是否包含图片
        drawings = para.findall('.//' + q('w:drawing'))
        if drawings:
            for drawing in drawings:
                blips = drawing.findall('.//' + q('a:blip'))
                for blip in blips:
                    embed = blip.get(f'{{{NSMAP["r"]}}}embed')
                    if embed and embed in self.image_map:
                        img_path = self.image_map[embed]
                        bid = self._next_id('img', element_index)
                        self.blocks.append(f"{self._block_comment(bid)}\n![图]({img_path})")
                        return

        # 检查是否包含公式 (OMML)
        math_paras = para.findall('.//' + q('m:oMathPara'))
        math_elements = para.findall('.//' + q('m:oMath'))
        if math_paras or math_elements:
            eq_id = self._next_id('eq', element_index)
            self.blocks.append(f"{self._block_comment(eq_id)}\n\n$$\\text{{(公式待转换)}}$$\n")
            return

        # 检查标题样式
        if pPr is not None:
            outline_level = pPr.find(q('w:outlineLvl'))
            pstyle = pPr.find(q('w:pStyle'))
            style_val = pstyle.get(q('w:val')) if pstyle is not None else ''

            # 检测是否为标题 (OutlineLevel 或 Heading 样式 或 中文标题样式)
            is_heading = False
            level = 1
            if outline_level is not None:
                is_heading = True
                level = int(outline_level.get(q('w:val'), '0')) + 1
            elif style_val.startswith('Heading') or style_val.startswith('heading'):
                is_heading = True
                level = int(style_val[-1]) if style_val[-1].isdigit() else 1
            elif style_val in ('1', '2', '3'):  # 中文模板数字样式ID
                is_heading = True
                level = int(style_val)

            if is_heading and text:
                bid = self._next_id('h', element_index)
                prefix = '#' * min(level, 3)
                self.blocks.append(f"{self._block_comment(bid)}\n{prefix} {text}")
                return

        # 检测数字编号格式标题（如 2.1.1、5.1.2，后面可跟文字）
        if re.match(r'^\d+\.\d+\.\d+', text):
            # 提取标题部分（数字编号 + 冒号/句号前的文字，最多50字符）
            title_match = re.match(r'^(\d+\.\d+\.\d+\s*[^：:。.]{1,50})', text)
            if title_match:
                title = title_match.group(1).strip()
            else:
                title = text[:50] + '...' if len(text) > 50 else text
            bid = self._next_id('h', element_index)
            self.blocks.append(f"{self._block_comment(bid)}\n### {title}")
            return

        # 普通正文段落
        if text:
            bid = self._next_id('p', element_index)
            self.blocks.append(f"{self._block_comment(bid)}\n{text}")

    def _extract_table(self, tbl, element_index=None):
        """提取表格为 Markdown table"""
        rows = tbl.findall('.//' + q('w:tr'))
        if not rows:
            return

        md_rows = []
        for row in rows:
            cells = []
            for cell in row.findall(q('w:tc')):
                # 合并单元格内所有段落
                cell_parts = []
                for para in cell.findall(q('w:p')):
                    t = self._get_text(para).strip()
                    if t:
                        cell_parts.append(t)
                cells.append(' '.join(cell_parts).replace('|', '\\|'))
            md_rows.append(cells)

        if not md_rows:
            return

        bid = self._next_id('tbl', element_index)
        lines = [self._block_comment(bid)]
        # Header row
        header = md_rows[0]
        lines.append('| ' + ' | '.join(header) + ' |')
        lines.append('| ' + ' | '.join(['---'] * len(header)) + ' |')
        # Data rows
        for row in md_rows[1:]:
            # Pad row to match header column count
            while len(row) < len(header):
                row.append('')
            lines.append('| ' + ' | '.join(row[:len(header)]) + ' |')

        self.blocks.append('\n'.join(lines))

    def _find_cover_table(self, body):
        """查找封面信息表格"""
        for tbl in body.findall(q('w:tbl')):
            rows = tbl.findall('.//' + q('w:tr'))
            for row in rows:
                cells = row.findall(q('w:tc'))
                if len(cells) >= 2:
                    label = self._get_text(cells[0]).strip().replace('\n', '')
                    value = self._get_text(cells[1]).strip().replace('\n', '') if len(cells) >= 3 else ''
                    if not value and len(cells) >= 3:
                        value = self._get_text(cells[2]).strip().replace('\n', '')
                    if label == '中文题目' and value:
                        self.title = value
                        return

    def extract(self):
        body = self.document_xml.getroot().find(q('w:body'))
        if body is None:
            print("ERROR: 未找到 body 元素")
            return None

        self._find_cover_table(body)

        # ── Phase 1: 收集所有段落文本和标签, 建立索引 ──
        elements = []
        for child in body:
            tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
            text = ''
            if tag == 'p':
                text = self._get_text(child).strip()
            elements.append({'el': child, 'tag': tag, 'text': text})

        # ── Phase 2: 查找锚点位置 ──
        anchors = {}  # name → index
        toc_start_idx = None
        for i, e in enumerate(elements):
            t = e['text']
            if t in ('摘  要',):
                anchors['abstract_zh_start'] = i
            elif t in ('Abstract',):
                anchors['abstract_en_start'] = i
            elif t in ('目  录', '目录'):
                anchors['toc_start'] = i
                toc_start_idx = i
            elif re.match(r'^[\d一二三四五六七八九十]*[.\s、章]*\s*[导绪]', t) and re.search(r'论', t):
                # 只在目录区之后查找正文起始锚点
                if toc_start_idx is None or i > toc_start_idx:
                    anchors['body_start'] = i
            elif t in ('参考文献',):
                anchors['ref_start'] = i
            elif t in ('附  录',):
                anchors['appendix_start'] = i
            elif t in ('致  谢',):
                anchors['ack_start'] = i

        # ── Phase 3: 按区域提取 ──
        def in_range(i, start_key, end_key):
            """判断元素 i 是否在 [start, end) 区域内"""
            a = anchors.get(start_key, 99999)
            b = anchors.get(end_key, 99999)
            return a <= i < b

        zh_abs_done = False
        en_abs_done = False
        ref_items = []
        toc_active = False

        for i, e in enumerate(elements):
            t = e['text']

            # 跳过封面区（第一个锚点之前）
            if 'abstract_zh_start' in anchors and i < anchors['abstract_zh_start']:
                continue

            # 跳过目录区
            if t == '目  录' or t == '目录':
                toc_active = True
                continue
            if toc_active:
                if anchors.get('body_start', 0) <= i:
                    toc_active = False
                else:
                    continue

            # ── 中文摘要区 ──
            if in_range(i, 'abstract_zh_start', 'abstract_en_start'):
                if t == '摘  要':
                    bid = self._next_id('h', i)
                    self.blocks.append(f"{self._block_comment(bid)}\n## {t}")
                    continue
                if '关键词' in t:
                    bid = self._next_id('kw', i)
                    self.blocks.append(f"{self._block_comment(bid)}\n**{t}**")
                    zh_abs_done = True
                elif not zh_abs_done and t:
                    bid = self._next_id('abs', i)
                    self.blocks.append(f"{self._block_comment(bid)}\n{t}")
                continue

            # ── 英文摘要区 ──
            if in_range(i, 'abstract_en_start', 'toc_start'):
                if t == 'Abstract':
                    bid = self._next_id('h', i)
                    self.blocks.append(f"{self._block_comment(bid)}\n## {t}")
                    continue
                if 'Keywords' in t or 'Key words' in t:
                    bid = self._next_id('kw', i)
                    self.blocks.append(f"{self._block_comment(bid)}\n**{t}**")
                    en_abs_done = True
                elif not en_abs_done and t:
                    bid = self._next_id('abs', i)
                    self.blocks.append(f"{self._block_comment(bid)}\n{t}")
                continue

            # ── 正文区 ──
            if in_range(i, 'body_start', 'ref_start'):
                if t in ('参考文献', '致  谢', '附  录'):
                    bid = self._next_id('h', i)
                    self.blocks.append(f"{self._block_comment(bid)}\n# {t}")
                    continue
                # 如果是正文起始锚点（如 1 导论），识别为一级标题
                if i == anchors.get('body_start') and t:
                    bid = self._next_id('h', i)
                    self.blocks.append(f"{self._block_comment(bid)}\n# {t}")
                    continue
                if e['tag'] == 'p' and t:
                    self._extract_paragraph(e['el'], element_index=i)
                elif e['tag'] == 'tbl':
                    self._extract_table(e['el'], element_index=i)

            # ── 参考文献区 ──
            if in_range(i, 'ref_start', 'appendix_start'):
                if t == '参考文献':
                    bid = self._next_id('h', i)
                    self.blocks.append(f"{self._block_comment(bid)}\n# {t}")
                    continue
                if t and (t[0] == '[' or t.startswith('[')):
                    bid = self._next_id('ref', i)
                    self.blocks.append(f"{self._block_comment(bid)}\n{t}")
                elif t and not t.startswith('<!--'):
                    bid = self._next_id('ref', i)
                    self.blocks.append(f"{self._block_comment(bid)}\n{t}")

            # ── 附录/致谢/封底区 ──
            if in_range(i, 'appendix_start', 'ack_start'):
                if t == '附  录':
                    bid = self._next_id('h', i)
                    self.blocks.append(f"{self._block_comment(bid)}\n# {t}")
                    continue
                if t:
                    self._extract_paragraph(e['el'], element_index=i)

            if i >= anchors.get('ack_start', 99999):
                if t == '致  谢':
                    bid = self._next_id('h', i)
                    self.blocks.append(f"{self._block_comment(bid)}\n# {t}")
                    continue
                if t:
                    self._extract_paragraph(e['el'], element_index=i)
                if e['tag'] == 'tbl':
                    self._extract_table(e['el'], element_index=i)

        # 补: 如果没找到锚点, 宽松匹配
        if not self.blocks:
            for i, e in enumerate(elements):
                if e['tag'] == 'p' and e['text']:
                    self._extract_paragraph(e['el'])
                elif e['tag'] == 'tbl':
                    self._extract_table(e['el'])

        return self

    def save(self, output_base):
        """保存提取结果"""
        safe_title = re.sub(r'[<>:"/\\|?*]', '_', self.title)[:60].strip()
        out_dir = Path(output_base) / safe_title
        assets_dir = out_dir / 'assets' / 'images'
        assets_dir.mkdir(parents=True, exist_ok=True)

        # 导出图片
        for rid, rel_path in self.image_map.items():
            try:
                img_name = Path(rel_path).name
                img_data = self.zf.read(f'word/media/{img_name}')
                (assets_dir / img_name).write_bytes(img_data)
            except:
                pass

        # 写 paper.md
        paper_md = out_dir / 'paper.md'
        paper_md.write_text('\n\n'.join(self.blocks), encoding='utf-8')
        print(f"  paper.md: {len(self.blocks)} blocks")

        # 写 meta.json
        meta = {
            'source_docx': self.docx_path.name,
            'title': self.title,
            'block_count': len(self.blocks),
            'block_map': self.block_map,  # block_id → body元素索引
            'extracted_at': datetime.now().isoformat()
        }
        (out_dir / 'meta.json').write_text(json.dumps(meta, ensure_ascii=False, indent=2), encoding='utf-8')
        print(f"  meta.json: {len(self.blocks)} blocks")

        return out_dir


def main():
    if len(sys.argv) < 2:
        print("用法: python extract.py <docx_path> [output_dir]")
        sys.exit(1)

    docx_path = sys.argv[1]
    output_dir = sys.argv[2] if len(sys.argv) > 2 else 'output'

    print(f"提取: {docx_path}")
    extractor = DocxExtractor(docx_path)
    result = extractor.extract()
    if result:
        out = result.save(output_dir)
        print(f"输出: {out}")
        # 输出 meta 供 Skill 读取
        meta_path = out / 'meta.json'
        if meta_path.exists():
            print(f"META:{meta_path}")
    else:
        print("提取失败")
        sys.exit(1)

if __name__ == '__main__':
    main()
