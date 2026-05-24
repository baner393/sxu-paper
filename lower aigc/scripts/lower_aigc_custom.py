#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
降 AI 处理脚本 - 根据用户自定义配置执行
"""

import re
import random
import sys
from pathlib import Path

# 高频词替换词库
# 核心原则：替换后的词不能在 AI_HIGH_FREQ_WORDS 列表里
WORD_REPLACEMENTS = {
    "赋能": ["帮着", "给...帮忙", "给...添把力", "拉一把"],
    "聚焦": ["盯着", "把注意力放在", "重点看", "主要研究"],
    "打造": ["做", "搞出", "弄出", "建起来"],
    "引领": ["带头", "走在前面", "引路", "当先锋"],
    "全方位": ["各个层面", "方方面面", "从头到尾", "里里外外"],
    "多维度": ["多角度", "方方面方面", "从不同角度看", "换个思路"],
    "高质量": ["好的", "过硬的", "拿得出手的", "像样的"],
    "沉浸式": ["深度体验", "全身心投入", "扎进去", "泡在里面"],
    "深耕": ["深入研究", "长期做", "专注", "扎根"],
    "助力": ["帮", "推一把", "添把火", "给点力"],
    "新质生产力": ["新的生产力", "先进生产力", "创新生产力"],
    "降本增效": ["省钱提效", "少花钱多办事", "降低成本提高效率"],
    "提质增效": ["提质提效", "做得更好更快", "改进品质加快效率"],
    "数智化": ["数字化智能化", "数字化转型", "智能化升级"],
    "落地": ["做成", "落实好", "做完", "推下去"],
    "沉淀": ["积累", "总结", "凝练"],
    "顶层设计": ["整体规划", "宏观规划", "统筹安排"],
    "底层逻辑": ["根本原理", "核心机制", "内在规律"],
    "抓手": ["切入点", "着手点", "关键点"],
    "闭环": ["完整循环", "完整流程", "完整体系"],
}

# 双音词→单音词替换
DOUBLE_TO_SINGLE = {
    "进行": "做",
    "实施": "做",
    "开展": "做",
    "实现": "达成",
    "促进": "推动",
    "提升": "提高",
    "增强": "加强",
    "优化": "改进",
    "完善": "健全",
    "推动": "促",
    "构建": "建",
    "打造": "建",
    "形成": "成",
    "建立": "建",
    "深化": "深入",
    "拓展": "扩展",
    "强化": "加强",
}

# 线性衔接词
LINEAR_CONNECTORS = [
    "此外",
    "另外",
    "与此同时",
    "值得注意的是",
    "需要指出的是",
    "不难发现",
    "显而易见",
    "事实上",
    "实际上",
    "换言之",
    "也就是说",
    "换句话说",
    "综上所述",
    "总之",
    "总而言之",
    "因此",
    "所以",
    "故而",
    "由此可见",
    "有鉴于此",
    "基于此",
    "由此",
    "进而",
    "从而",
    "于是",
    "那么",
    "首先",
    "其次",
    "再次",
    "最后",
    "一方面",
    "另一方面",
    "与此同时",
    "不仅...而且",
    "既...又",
    "虽然...但是",
    "尽管...然而",
]

# 模板化表达
TEMPLATE_EXPRESSIONS = [
    (r"随着(.{2,15})的(.{2,8})发展", r"在\1不断\2的过程中"),
    (r"在(.{2,15})的背景下", r"面对\1的情况"),
    (r"从(.{2,10})的角度", r"站在\1的立场上"),
    (r"在(.{2,10})方面", r"就\1而言"),
    (r"基于(.{2,15})的考虑", r"出于\1的需要"),
    (r"本研究", "本文"),
    (r"本文认为", "笔者以为"),
    (r"研究表明", "分析显示"),
]

def count_chars(text):
    """统计中文字符数"""
    return len(re.findall(r'[一-鿿]', text))

def replace_high_freq_words(text, frequency=500):
    """替换高频词"""
    chars_count = count_chars(text)
    replace_count = max(1, chars_count // frequency)

    words_found = []
    for word in WORD_REPLACEMENTS:
        if word in text:
            words_found.append(word)

    random.shuffle(words_found)
    replaced = 0
    for word in words_found[:replace_count]:
        if replaced >= replace_count:
            break
        alternatives = WORD_REPLACEMENTS[word]
        replacement = random.choice(alternatives)
        text = text.replace(word, replacement, 1)
        replaced += 1

    return text

def replace_double_to_single(text, frequency=1000):
    """双音词→单音词替换"""
    chars_count = count_chars(text)
    replace_count = max(1, chars_count // frequency)

    words_found = []
    for word in DOUBLE_TO_SINGLE:
        if word in text:
            words_found.append(word)

    random.shuffle(words_found)
    replaced = 0
    for word in words_found[:replace_count]:
        if replaced >= replace_count:
            break
        replacement = DOUBLE_TO_SINGLE[word]
        text = text.replace(word, replacement, 1)
        replaced += 1

    return text

def remove_linear_connectors(text, frequency=200):
    """删除线性衔接词"""
    chars_count = count_chars(text)
    remove_count = max(1, chars_count // frequency)

    connectors_found = []
    for conn in LINEAR_CONNECTORS:
        if conn in text:
            connectors_found.append(conn)

    random.shuffle(connectors_found)
    removed = 0
    for conn in connectors_found[:remove_count]:
        if removed >= remove_count:
            break
        text = text.replace(conn, "", 1)
        removed += 1

    return text

def apply_template_replacements(text, frequency=500):
    """替换模板化表达"""
    chars_count = count_chars(text)
    replace_count = max(1, chars_count // frequency)

    replaced = 0
    for pattern, replacement in TEMPLATE_EXPRESSIONS:
        if replaced >= replace_count:
            break
        new_text = re.sub(pattern, replacement, text, count=1)
        if new_text != text:
            text = new_text
            replaced += 1

    return text

def perturb_sentence_structure(text, intensity="high"):
    """扰动句式结构"""
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

        if len(sent) > 50 and random.random() < 0.3:
            # 尝试拆分长句
            parts = re.split(r'[，,；;]', sent)
            if len(parts) >= 3:
                # 随机选择拆分点
                split_idx = random.randint(1, len(parts) - 1)
                first_half = '，'.join(parts[:split_idx]) + '。'
                second_half = '，'.join(parts[split_idx:])
                result.append(first_half)
                result.append(second_half)
                continue

        result.append(sent)

    return ''.join(result)

def break_paragraph_uniformity(paragraphs, frequency=200):
    """打破段落均匀性"""
    if len(paragraphs) < 3:
        return paragraphs

    result = list(paragraphs)
    chars_count = sum(count_chars(p) for p in paragraphs)
    adjust_count = max(1, chars_count // frequency)

    for _ in range(adjust_count):
        if len(result) < 2:
            break

        # 随机选择策略
        strategy = random.choice(['merge', 'split', 'shorten'])

        if strategy == 'merge' and len(result) >= 2:
            # 合并两个相邻段落
            idx = random.randint(0, len(result) - 2)
            if not result[idx].startswith('#') and not result[idx + 1].startswith('#'):
                result[idx] = result[idx] + result[idx + 1]
                result.pop(idx + 1)

        elif strategy == 'split' and any(count_chars(p) > 100 for p in result):
            # 拆分长段落
            long_paras = [(i, p) for i, p in enumerate(result) if count_chars(p) > 100]
            if long_paras:
                idx, para = random.choice(long_paras)
                sentences = re.split(r'(?<=[。！？])', para)
                if len(sentences) >= 4:
                    mid = len(sentences) // 2
                    result[idx] = ''.join(sentences[:mid])
                    result.insert(idx + 1, ''.join(sentences[mid:]))

        elif strategy == 'shorten':
            # 缩短一个段落
            idx = random.randint(0, len(result) - 1)
            if count_chars(result[idx]) > 50:
                sentences = re.split(r'(?<=[。！？])', result[idx])
                if len(sentences) > 2:
                    keep = random.randint(1, len(sentences) - 1)
                    result[idx] = ''.join(sentences[:keep])

    return result

def remove_unfounded_adverbs(text, frequency=200):
    """删除无依据的副词形容词 - 只删AI模板词，保留人类常用词"""
    # 只删除AI常用的夸大/模板化副词，保留"非常""十分""极其"等人类常用词
    adverbs = [
        "显著", "明显", "深刻", "重大", "关键", "核心",
        "根本", "本质", "全面", "系统", "整体", "高效",
        "有力", "综合", "完整", "充分", "深入",
    ]

    chars_count = count_chars(text)
    remove_count = max(1, chars_count // frequency)

    removed = 0
    for adv in adverbs:
        if removed >= remove_count:
            break
        pattern = rf'(?<=[，。！？\n]){{1}}{adv}(?=[，。！？\n]){{1}}'
        new_text = re.sub(pattern, '', text, count=1)
        if new_text != text:
            text = new_text
            removed += 1

    return text

def insert_author_viewpoint(text, frequency=1000):
    """插入作者观点"""
    viewpoints = [
        "在我看来，",
        "就我个人的理解，",
        "从实际观察来看，",
        "据我所知，",
        "结合实际情况，",
        "在我看来，",
        "以我之见，",
        "从某种意义上说，",
    ]

    chars_count = count_chars(text)
    insert_count = max(1, chars_count // frequency)

    sentences = re.split(r'(?<=[。！？])', text)
    result = []
    inserted = 0

    for i, sent in enumerate(sentences):
        if inserted < insert_count and len(sent) > 30 and random.random() < 0.2:
            viewpoint = random.choice(viewpoints)
            result.append(viewpoint)
            inserted += 1
        result.append(sent)

    return ''.join(result)

def process_paragraph(para, config):
    """处理单个段落"""
    # 跳过标题、表格、代码块等
    if para.startswith('#') or para.startswith('|') or para.startswith('```'):
        return para
    if para.startswith('<!--') and '-->' in para:
        return para

    text = para

    # 应用各种处理
    if config['high_freq'] >= 4:
        text = replace_high_freq_words(text, frequency=500)
    elif config['high_freq'] >= 3:
        text = replace_high_freq_words(text, frequency=1000)
    else:
        text = replace_high_freq_words(text, frequency=2000)

    if config['double_single'] >= 3:
        text = replace_double_to_single(text, frequency=1000)
    else:
        text = replace_double_to_single(text, frequency=2000)

    if config['sentence_perturb'] >= 5:
        text = perturb_sentence_structure(text, intensity="high")
    elif config['sentence_perturb'] >= 3:
        text = perturb_sentence_structure(text, intensity="medium")

    if config['linear_conn'] >= 5:
        text = remove_linear_connectors(text, frequency=200)
    elif config['linear_conn'] >= 3:
        text = remove_linear_connectors(text, frequency=500)
    else:
        text = remove_linear_connectors(text, frequency=1500)

    if config['template'] >= 4:
        text = apply_template_replacements(text, frequency=500)
    else:
        text = apply_template_replacements(text, frequency=1000)

    if config['adverb'] >= 5:
        text = remove_unfounded_adverbs(text, frequency=200)
    elif config['adverb'] >= 3:
        text = remove_unfounded_adverbs(text, frequency=500)

    if config['viewpoint'] >= 4:
        text = insert_author_viewpoint(text, frequency=500)
    elif config['viewpoint'] >= 3:
        text = insert_author_viewpoint(text, frequency=1000)

    return text

def process_document(input_file, output_file, config):
    """处理整个文档"""
    with open(input_file, 'r', encoding='utf-8') as f:
        content = f.read()

    # 分割文档为段落
    paragraphs = content.split('\n\n')

    # 处理段落均匀性
    if config['uniformity'] >= 5:
        paragraphs = break_paragraph_uniformity(paragraphs, frequency=200)
    elif config['uniformity'] >= 3:
        paragraphs = break_paragraph_uniformity(paragraphs, frequency=500)

    # 处理每个段落
    processed = []
    for para in paragraphs:
        processed.append(process_paragraph(para, config))

    # 重组文档
    result = '\n\n'.join(processed)

    # 保存结果
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(result)

    return result

def main():
    if len(sys.argv) < 3:
        print("用法: python lower_aigc_custom.py <输入文件> <输出文件>")
        sys.exit(1)

    input_file = sys.argv[1]
    output_file = sys.argv[2]

    # 用户配置 - 调整到合理档位，避免过度处理
    config = {
        'high_freq': 4,        # 高频词替换：保持4档
        'double_single': 2,    # 双音词→单音词：降到2档，不要过度替换
        'sentence_perturb': 3, # 句式结构扰动：降到3档，避免拆太多长句
        'linear_conn': 4,      # 删除线性衔接词：降到4档，保留部分自然过渡
        'paragraph_jump': 3,   # 段结构跳跃：降到3档
        'uniformity': 4,       # 打破段落均匀性：降到4档
        'viewpoint': 3,        # 插入作者观点：保持3档
        'empty_term': 3,       # 空洞术语处理：保持3档
        'adverb': 2,           # 无依据副词删除：降到2档，少删人类常用词
        'template': 3,         # 模板化表达处理：补上缺失的配置
    }

    print(f"正在处理: {input_file}")
    print(f"输出文件: {output_file}")
    print(f"配置: {config}")

    result = process_document(input_file, output_file, config)

    print(f"处理完成！共 {count_chars(result)} 字")

if __name__ == "__main__":
    main()
