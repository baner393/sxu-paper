"""
Markdown → DOCX 回填器
用法: python backfill.py <markdown_dir> [original_docx]

从 paper.md + meta.json 读取编辑后的内容，回填到原 DOCX（生成 _filled.docx）。
仅替换正文/表格/公式内容，保留原 DOCX 所有格式。

依赖: python-docx (pip install python-docx)
"""

import json
import re
import sys
import shutil
from pathlib import Path
from docx import Document
from docx.shared import Pt

# ── 锚点正则 ──
BLOCK_RE = re.compile(r'<!--\s*block:(\w+)\s*-->')

def parse_markdown(md_path):
    """解析 paper.md, 返回 {block_id: content} 字典"""
    text = Path(md_path).read_text(encoding='utf-8')
    blocks = {}

    # 以 block 注释分割
    parts = BLOCK_RE.split(text)
    # parts = ['before', 'p182', '\ncontent...', 'h5', '\n# title...', ...]
    for i in range(1, len(parts), 2):
        block_id = parts[i]
        content = parts[i + 1] if i + 1 < len(parts) else ''
        # 清理内容
        content = content.strip()
        # 去掉 Markdown 标题标记符用于纯文本替换
        # (回填到 DOCX 时, 字体格式由原段落属性决定)
        blocks[block_id] = content

    return blocks


def backfill(docx_path, md_dir, output_path=None):
    """回填 Markdown 内容到 DOCX"""
    if output_path is None:
        output_path = str(Path(docx_path).parent / f"{Path(docx_path).stem}_filled.docx")

    # 复制原文件
    shutil.copy2(docx_path, output_path)

    # 读取编辑后的内容
    md_path = Path(md_dir) / 'paper.md'
    meta_path = Path(md_dir) / 'meta.json'

    if not md_path.exists():
        print(f"ERROR: 未找到 {md_path}")
        sys.exit(1)

    blocks = parse_markdown(md_path)

    # 验证
    if meta_path.exists():
        meta = json.loads(meta_path.read_text(encoding='utf-8'))
        original_count = meta.get('block_count', 0)
        current_count = len(blocks)
        if current_count != original_count:
            print(f"WARNING: block 数量变化 ({original_count} → {current_count}), 可能编辑有误")

    # 打开复制后的文档
    doc = Document(output_path)
    body = doc.element.body

    # 遍历 body 元素, 匹配 block ID
    nsmap = {
        'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main',
    }

    block_index = 0
    block_keys = list(blocks.keys())

    for child in body:
        tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag

        if tag == 'p':
            # 段落: 匹配 block:p{id} 或 block:h{id}
            if block_index >= len(block_keys):
                break
            bid = block_keys[block_index]
            content = blocks[bid]

            # 清理 Markdown 标记行和空行
            lines = content.split('\n')
            cleaned_lines = []
            for line in lines:
                line = line.strip()
                if line and not line.startswith('<!--') and not line.startswith('!['):
                    # 去掉 # 标题标记
                    line = re.sub(r'^#{1,3}\s+', '', line)
                    cleaned_lines.append(line)

            new_text = ' '.join(cleaned_lines)
            if new_text:
                # 替换段落中的所有文本
                for r in child.iter(f'{{{nsmap["w"]}}}r'):
                    for t in r.iter(f'{{{nsmap["w"]}}}t'):
                        t.text = ''
                # 写入第一个 run
                first_r = child.find(f'{{{nsmap["w"]}}}r')
                if first_r is not None:
                    first_t = first_r.find(f'{{{nsmap["w"]}}}t')
                    if first_t is not None:
                        first_t.text = new_text
                        first_t.set('{http://www.w3.org/XML/1998/namespace}space', 'preserve')

            block_index += 1

        elif tag == 'tbl':
            # 表格: 匹配 block:tbl{id}
            if block_index >= len(block_keys):
                break
            bid = block_keys[block_index]
            content = blocks[bid]

            # 解析 Markdown table
            md_rows = [line for line in content.split('\n') if line.strip().startswith('|')]
            data_rows = []
            for md_row in md_rows:
                if '---' in md_row:
                    continue  # skip separator
                cells = [c.strip() for c in md_row.split('|')[1:-1]]
                data_rows.append(cells)

            if data_rows:
                # 查找表格行
                trs = child.findall(f'{{{nsmap["w"]}}}tr')
                for ri, tr in enumerate(trs):
                    if ri >= len(data_rows):
                        break
                    tcs = tr.findall(f'{{{nsmap["w"]}}}tc')
                    for ci, tc in enumerate(tcs):
                        if ci >= len(data_rows[ri]):
                            break
                        # 替换单元格内文本
                        for r in tc.iter(f'{{{nsmap["w"]}}}r'):
                            for t in r.iter(f'{{{nsmap["w"]}}}t'):
                                t.text = ''
                        first_r = tc.find(f'{{{nsmap["w"]}}}r')
                        if first_r is not None:
                            first_t = first_r.find(f'{{{nsmap["w"]}}}t')
                            if first_t is not None:
                                first_t.text = data_rows[ri][ci]

            block_index += 1

        elif tag == 'sectPr':
            # 跳过节属性
            continue

    doc.save(output_path)
    print(f"回填完成: {output_path}")
    return output_path


def main():
    if len(sys.argv) < 2:
        print("用法: python backfill.py <markdown_dir> [original_docx]")
        print("  markdown_dir: 包含 paper.md 和 meta.json 的目录")
        print("  original_docx: 原 DOCX 路径 (默认从 meta.json 读取)")
        sys.exit(1)

    md_dir = sys.argv[1]

    # 读取 meta.json 获取原 DOCX 路径
    meta_path = Path(md_dir) / 'meta.json'
    if meta_path.exists():
        meta = json.loads(meta_path.read_text(encoding='utf-8'))
        docx_name = meta.get('source_docx', '')
    else:
        print("ERROR: 未找到 meta.json")
        sys.exit(1)

    if len(sys.argv) > 2:
        docx_path = sys.argv[2]
    else:
        # 尝试在 common locations 查找
        possible = [
            f"paper put here/just put here/{docx_name}",
            docx_name
        ]
        docx_path = None
        for p in possible:
            if Path(p).exists():
                docx_path = p
                break
        if not docx_path:
            print(f"ERROR: 未找到原 DOCX: {docx_name}")
            print("请提供原 DOCX 路径作为第二个参数")
            sys.exit(1)

    backfill(docx_path, md_dir)


if __name__ == '__main__':
    main()
