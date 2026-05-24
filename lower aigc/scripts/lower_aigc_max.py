#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
降 AI 处理脚本 - 全部最高档
"""

import re
import random
import sys
from pathlib import Path

# 高频词替换词库
WORD_REPLACEMENTS = {
    '赋能': ['帮助', '支持', '提供动力', '助力', '给予力量'],
    '聚焦': ['关注', '集中精力', '重点研究', '着眼于', '把目光投向'],
    '打造': ['建设', '构建', '建立', '形成', '创造'],
    '引领': ['带头', '推动', '引导', '带动', '主导'],
    '全方位': ['各个层面', '多方面', '全面', '各方面', '方方面面'],
    '多维度': ['多角度', '多层面', '多方面', '不同视角', '多个角度'],
    '高质量': ['优质', '高水平', '出色', '精良', '上乘'],
    '沉浸式': ['深度体验', '全身心投入', '融入其中', '身临其境'],
    '深耕': ['深入研究', '长期从事', '专注', '扎根', '钻研'],
    '助力': ['推动', '促进', '帮助', '支持', '助推'],
    '新质生产力': ['新的生产力', '先进生产力', '创新生产力', '新型生产力'],
    '降本增效': ['降低成本提高效率', '节省开支提升效益', '压缩成本提升效能'],
    '提质增效': ['提升质量和效率', '改进品质加快效率', '优化质量增强效能'],
    '数智化': ['数字化智能化', '数字化转型', '智能化升级', '数字化赋能'],
    '落地': ['实施', '执行', '推行', '付诸实践', '落到实处'],
    '沉淀': ['积累', '总结', '凝练', '积淀'],
    '顶层设计': ['整体规划', '宏观规划', '统筹安排', '系统设计'],
    '底层逻辑': ['根本原理', '核心机制', '内在规律', '基本原理'],
    '抓手': ['切入点', '着力点', '关键点', '突破口'],
    '闭环': ['完整循环', '完整流程', '完整体系', '闭合回路'],
}

# 双音词→单音词替换
DOUBLE_TO_SINGLE = {
    '进行': '做', '实施': '做', '开展': '做', '实现': '达成',
    '促进': '推动', '提升': '提高', '增强': '加强', '优化': '改进',
    '完善': '健全', '推动': '促', '构建': '建', '打造': '建',
    '形成': '成', '建立': '建', '深化': '深入', '拓展': '扩展',
    '强化': '加强',
}

# 线性衔接词
LINEAR_CONNECTORS = [
    '此外', '另外', '与此同时', '值得注意的是', '需要指出的是',
    '不难发现', '显而易见', '事实上', '实际上', '换言之',
    '也就是说', '换句话说', '综上所述', '总之', '总而言之',
    '因此', '所以', '故而', '由此可见', '有鉴于此', '基于此',
    '由此', '进而', '从而', '于是', '那么', '首先', '其次',
    '再次', '最后', '一方面', '另一方面', '不仅...而且', '既...又',
]

# 模板化表达替换
TEMPLATE_REPLACEMENTS = [
    (r'随着(.{2,15})的(.{2,8})发展', r'在\1不断\2的过程中'),
    (r'在(.{2,15})的背景下', r'面对\1的情况'),
    (r'从(.{2,10})的角度', r'站在\1的立场上'),
    (r'在(.{2,10})方面', r'就\1而言'),
    (r'基于(.{2,15})的考虑', r'出于\1的需要'),
    (r'本研究', '本文'),
    (r'本文认为', '笔者以为'),
    (r'研究表明', '分析显示'),
]

# 无依据副词形容词
UNFOUNDED_WORDS = [
    '非常', '极其', '十分', '相当', '颇为', '甚为',
    '极为', '显著', '明显', '深刻', '重大', '重要',
    '关键', '核心', '根本', '本质', '基本', '全面',
    '系统', '整体', '综合', '完整', '深入', '细致',
    '具体', '详细', '充分', '有效', '有力', '高效',
    '快速', '迅速', '持续', '不断', '长期', '始终', '一贯',
]

def replace_all_high_freq(text):
    for word, alts in WORD_REPLACEMENTS.items():
        while word in text:
            text = text.replace(word, random.choice(alts), 1)
    return text

def replace_all_double_single(text):
    for word, replacement in DOUBLE_TO_SINGLE.items():
        text = text.replace(word, replacement)
    return text

def remove_all_connectors(text):
    for conn in LINEAR_CONNECTORS:
        text = text.replace(conn, '')
    return text

def apply_all_templates(text):
    for pattern, replacement in TEMPLATE_REPLACEMENTS:
        text = re.sub(pattern, replacement, text)
    return text

def perturb_sentences(text):
    sentences = re.split(r'([。！？])', text)
    result = []
    i = 0
    while i < len(sentences):
        sent = sentences[i]
        if i + 1 < len(sentences) and sentences[i + 1] in '。！？':
            sent += sentences[i + 1]
            i += 2
        else:
            i += 1

        # 长句拆分
        if len(sent) > 60 and random.random() < 0.5:
            parts = re.split(r'[，,；;]', sent)
            if len(parts) >= 3:
                split_idx = random.randint(1, len(parts) - 1)
                first_half = '，'.join(parts[:split_idx]) + '。'
                second_half = '，'.join(parts[split_idx:])
                result.append(first_half)
                result.append(second_half)
                continue

        result.append(sent)
    return ''.join(result)

def process_paragraph(para):
    # 跳过标题、锚点
    if para.startswith('#') or para.startswith('|') or para.startswith('```'):
        return para
    if para.startswith('<!--') and '-->' in para:
        return para

    text = para
    text = replace_all_high_freq(text)
    text = replace_all_double_single(text)
    text = remove_all_connectors(text)
    text = apply_all_templates(text)
    text = perturb_sentences(text)
    return text

def main():
    if len(sys.argv) < 3:
        print("用法: python lower_aigc_max.py <输入文件> <输出文件>")
        sys.exit(1)

    input_file = sys.argv[1]
    output_file = sys.argv[2]

    # 读取文件
    with open(input_file, 'r', encoding='utf-8') as f:
        content = f.read()

    # 处理文档
    paragraphs = content.split('\n\n')
    processed = []
    for para in paragraphs:
        processed.append(process_paragraph(para))

    result = '\n\n'.join(processed)

    # 保存
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(result)

    # 统计中文字符数
    chinese_chars = len(re.findall(r'[一-鿿]', result))
    print(f'处理完成！共 {chinese_chars} 个中文字符')

if __name__ == "__main__":
    main()
