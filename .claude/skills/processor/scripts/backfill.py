"""
Markdown → DOCX 回填器
用法: python backfill.py <markdown_dir> [original_docx]

从 paper.md + meta.json 读取编辑后的内容，回填到原 DOCX（生成 _filled.docx）。
通过 block_map 定位每个 block 在 DOCX body 中的元素索引，精确回填。

依赖: python-docx (pip install python-docx)
"""

import json
import re
import sys
import shutil
from pathlib import Path
from docx import Document

# ── 锚点正则 ──
BLOCK_RE = re.compile(r'<!--\s*block:(\w+)\s*-->')


def parse_markdown(md_path):
    """解析 paper.md, 返回 {block_id: content} 字典"""
    text = Path(md_path).read_text(encoding='utf-8')
    blocks = {}

    # 以 block 注释分割
    parts = BLOCK_RE.split(text)
    for i in range(1, len(parts), 2):
        block_id = parts[i]
        content = parts[i + 1] if i + 1 < len(parts) else ''
        content = content.strip()
        blocks[block_id] = content

    return blocks


def clean_text_from_markdown(content):
    """从 Markdown 内容中提取纯文本，去掉标题标记、图片链接等"""
    lines = content.split('\n')
    cleaned_lines = []
    for line in lines:
        line = line.strip()
        if not line:
            continue
        if line.startswith('<!--'):
            continue
        if line.startswith('!['):
            continue
        # 去掉 # 标题标记
        line = re.sub(r'^#{1,3}\s+', '', line)
        cleaned_lines.append(line)
    return ' '.join(cleaned_lines)


def parse_markdown_table(content):
    """解析 Markdown 表格，返回二维列表"""
    md_rows = [line for line in content.split('\n') if line.strip().startswith('|')]
    data_rows = []
    for md_row in md_rows:
        if '---' in md_row:
            continue  # skip separator
        cells = [c.strip() for c in md_row.split('|')[1:-1]]
        data_rows.append(cells)
    return data_rows


def replace_paragraph_text(para, new_text):
    """替换段落文本，保留原格式"""
    ns = 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'
    # 清空所有 run 的文本
    for r in para.iter(f'{{{ns}}}r'):
        for t in r.iter(f'{{{ns}}}t'):
            t.text = ''
    # 写入第一个 run
    first_r = para.find(f'{{{ns}}}r')
    if first_r is not None:
        first_t = first_r.find(f'{{{ns}}}t')
        if first_t is not None:
            first_t.text = new_text
            first_t.set('{http://www.w3.org/XML/1998/namespace}space', 'preserve')


def replace_table_content(tbl, data_rows):
    """替换表格内容"""
    ns = 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'
    trs = tbl.findall(f'{{{ns}}}tr')
    for ri, tr in enumerate(trs):
        if ri >= len(data_rows):
            break
        tcs = tr.findall(f'{{{ns}}}tc')
        for ci, tc in enumerate(tcs):
            if ci >= len(data_rows[ri]):
                break
            # 清空单元格内文本
            for r in tc.iter(f'{{{ns}}}r'):
                for t in r.iter(f'{{{ns}}}t'):
                    t.text = ''
            first_r = tc.find(f'{{{ns}}}r')
            if first_r is not None:
                first_t = first_r.find(f'{{{ns}}}t')
                if first_t is not None:
                    first_t.text = data_rows[ri][ci]


def backfill(docx_path, md_dir, output_path=None):
    """回填 Markdown 内容到 DOCX"""
    if output_path is None:
        output_path = str(Path(docx_path).parent / "_backfill_temp.docx")

    # 复制原文件
    shutil.copy2(docx_path, output_path)

    # 读取编辑后的内容
    md_path = Path(md_dir) / 'paper.md'
    meta_path = Path(md_dir) / 'meta.json'

    if not md_path.exists():
        print(f"ERROR: 未找到 {md_path}")
        sys.exit(1)

    new_blocks = parse_markdown(md_path)

    # 读取 meta.json
    if meta_path.exists():
        meta = json.loads(meta_path.read_text(encoding='utf-8'))
        block_map = meta.get('block_map', {})
        original_count = meta.get('block_count', 0)
    else:
        print("ERROR: 未找到 meta.json")
        sys.exit(1)

    print(f"原始 block 数: {original_count}, 编辑后 block 数: {len(new_blocks)}")

    # 打开复制后的文档
    doc = Document(output_path)
    body = doc.element.body

    # 获取 body 的所有子元素列表
    body_children = list(body)

    # 统计
    replaced = 0
    cleared = 0
    skipped = 0

    # 处理每个原始 block
    for bid, elem_index in block_map.items():
        if elem_index >= len(body_children):
            print(f"WARNING: block {bid} 的元素索引 {elem_index} 超出范围")
            continue

        element = body_children[elem_index]
        tag = element.tag.split('}')[-1] if '}' in element.tag else element.tag

        if bid in new_blocks:
            # block 被保留，替换内容
            content = new_blocks[bid]

            if tag == 'p':
                new_text = clean_text_from_markdown(content)
                if new_text:
                    replace_paragraph_text(element, new_text)
                    replaced += 1
            elif tag == 'tbl':
                data_rows = parse_markdown_table(content)
                if data_rows:
                    replace_table_content(element, data_rows)
                    replaced += 1
        else:
            # block 被删除，清空内容
            if tag == 'p':
                replace_paragraph_text(element, '')
                cleared += 1
            elif tag == 'tbl':
                replace_table_content(element, [])
                cleared += 1

    doc.save(output_path)

    # 重命名
    if '_backfill_temp.docx' in output_path:
        final_path = str(Path(docx_path).parent / f"{Path(docx_path).stem}_filled.docx")
        if Path(final_path).exists():
            Path(final_path).unlink()
        Path(output_path).rename(final_path)
        output_path = final_path

    print(f"回填完成: 替换 {replaced} 个, 清空 {cleared} 个")
    print(f"输出: {output_path}")
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
