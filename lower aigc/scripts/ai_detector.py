#!/usr/bin/env python3
"""
AI 生成内容检测器
基于困惑度、突发性、高频词、模板化表达等特征评估文本的 AI 率
"""

import re
import sys
import json
from pathlib import Path
from collections import Counter
from typing import Dict, List, Tuple

# AI 常用高频词（从词库提取的高频部分）
AI_HIGH_FREQ_WORDS = [
    "赋能", "深耕", "聚焦", "助力", "打造", "引领", "全方位", "多维度",
    "高质量", "沉浸式", "一站式", "闭环", "抓手", "底层逻辑", "顶层设计",
    "降本增效", "提质增效", "数智化", "落地", "沉淀", "给到", "响应",
    "同步", "对齐", "对标", "迭代", "优化", "跟进", "升级", "交付",
    "倒逼", "复盘", "梳理", "输出", "提炼", "包装", "上升", "方案",
    "协同", "联动", "透传", "打通", "发力", "兼容", "量化", "细分",
    "重塑", "蓄能", "引爆", "挖掘", "背书", "支撑", "协调", "支援",
    "加持", "加速", "共建", "共创", "融合", "拉通", "拉升", "洞察",
    "渗透", "辐射", "扩展", "开拓", "兜底", "容错", "解耦", "耦合",
    "复用", "封装", "抽象", "聚合", "集成", "拆解", "观察", "监控",
    "捕获", "分发", "分层", "迁移", "回溯", "回归", "通晒", "吃透",
    "死磕", "树立", "跨界", "共情", "演绎", "画饼", "反哺", "输血",
    "造血", "造势", "下沉", "拉新", "转化", "留存", "促活", "付费",
    "营收", "盈利", "获客", "激励", "激活", "推广", "投放", "导流",
    "覆盖", "曝光", "裂变", "增长", "优秀", "感恩", "订阅", "认证",
    "推送", "履约", "进化", "进军", "起飞", "收割", "共享", "重组",
    "收口", "转型", "围绕", "出击", "评估", "评审", "务实", "夯实",
    "预判", "深入", "打磨", "攻坚", "击穿", "破冰", "破题", "解题",
    "破圈", "破局", "突围", "补位", "抽离", "皮实", "本分", "重磅",
    "垂直", "精准", "持续", "灵活", "稳定", "可控", "活跃", "漏斗",
    "中台", "平台", "风口", "打法", "玩法", "矩阵", "纽带", "刺激",
    "规模", "场景", "渠道", "入口", "维度", "格局", "形态", "生态",
    "体系", "认知", "体感", "感知", "心智", "调性", "战役", "合力",
    "心力", "赛道", "基石", "基因", "因子", "模型", "通道", "链路",
    "水位", "水准", "姿态", "卡点", "卡位", "头部", "腰部", "痛点",
    "爽点", "痒点", "全域", "公域", "私域", "蓝海", "红海", "壁垒",
    "变量", "边界", "品牌", "阵地", "高地", "洼地", "革命", "变革",
    "内卷", "脑暴", "脑洞", "圈层", "层级", "段位", "环节", "困局",
    "话术", "文案", "议程", "触点", "势能", "流量", "资源", "排期",
    "埋点", "坑位", "楼层", "峰值", "漏洞", "风险", "瓶颈", "策略",
    "价值", "成本", "复利", "利器", "深度", "玩家", "小白", "韭菜",
    "羊毛", "福利", "套路", "情怀", "标准", "规范", "社群", "产业",
    "载体", "服务", "粘性", "属性", "地域", "终端", "版本", "口碑",
    "指标", "试点", "空白", "生命周期", "商业模式", "底层逻辑",
    "顶层设计", "解决方案", "私域流量", "垂直领域", "增长飞轮",
    "第二曲线", "用户心智", "用户体验", "用户画像", "用户粘性",
    "快速迭代", "持续迭代", "降维打击", "品效合一", "逻辑自洽"
]

# AI 常用连接词/线性衔接词
AI_CONNECTORS = [
    "此外", "然而", "值得注意的是", "综上所述", "总之", "因此",
    "所以", "同时", "另外", "除此之外", "不仅...而且", "一方面...另一方面",
    "首先", "其次", "再次", "最后", "与此同时", "总的来说",
    "具体而言", "换言之", "简言之", "事实上", "实际上", "显然",
    "毫无疑问", "不可否认", "必须指出", "需要强调", "重要的是"
]

# 模板化句式
AI_TEMPLATES = [
    "随着.*的发展", "在.*的背景下", "通过.*的方式", "基于.*的考虑",
    "从.*的角度", "在.*方面", "对于.*来说", "就.*而言",
    "关于.*的问题", "针对.*的情况", "考虑到.*因素", "根据.*数据",
    "研究表明", "调查显示", "数据显示", "分析发现",
    "本文认为", "本研究", "本论文", "笔者认为"
]


def clean_text(text: str) -> str:
    """清理文本，去除 Markdown 标记"""
    # 去除标题标记
    text = re.sub(r'^#+\s+', '', text, flags=re.MULTILINE)
    # 去除加粗、斜体标记
    text = re.sub(r'\*+', '', text)
    text = re.sub(r'_+', '', text)
    # 去除链接
    text = re.sub(r'\[([^\]]+)\]\([^\)]+\)', r'\1', text)
    # 去除图片
    text = re.sub(r'!\[([^\]]*)\]\([^\)]+\)', '', text)
    # 去除引用标记
    text = re.sub(r'^>\s+', '', text, flags=re.MULTILINE)
    # 去除列表标记
    text = re.sub(r'^[\-\*]\s+', '', text, flags=re.MULTILINE)
    text = re.sub(r'^\d+\.\s+', '', text, flags=re.MULTILINE)
    # 去除代码块
    text = re.sub(r'```[\s\S]*?```', '', text)
    text = re.sub(r'`[^`]+`', '', text)
    # 去除表格
    text = re.sub(r'\|.*\|', '', text)
    text = re.sub(r'^[\-\|:]+$', '', text, flags=re.MULTILINE)
    # 去除多余空白
    text = re.sub(r'\n\s*\n', '\n', text)
    return text.strip()


def split_sentences(text: str) -> List[str]:
    """将文本分割成句子"""
    # 中文句子分割
    sentences = re.split(r'[。！？；\n]', text)
    # 英文句子分割
    result = []
    for s in sentences:
        result.extend(re.split(r'[.!?;]\s+', s))
    return [s.strip() for s in result if s.strip() and len(s.strip()) > 5]


def split_paragraphs(text: str) -> List[str]:
    """将文本分割成段落"""
    paragraphs = re.split(r'\n\s*\n', text)
    return [p.strip() for p in paragraphs if p.strip() and len(p.strip()) > 10]


def calculate_high_freq_score(text: str) -> Tuple[float, List[str]]:
    """计算高频词得分（0-100，越高越可能是 AI）"""
    found_words = []
    total_count = 0

    for word in AI_HIGH_FREQ_WORDS:
        count = text.count(word)
        if count > 0:
            found_words.append(f"{word}({count})")
            total_count += count

    # 文本长度归一化
    text_len = len(text)
    if text_len == 0:
        return 0, []

    # 计算高频词密度（每千字出现次数）
    density = (total_count / text_len) * 1000

    # 密度转换为得分（0-100）
    # 密度 < 2: 低 AI 特征
    # 密度 2-5: 中等 AI 特征
    # 密度 > 5: 高 AI 特征
    if density < 2:
        score = density * 15
    elif density < 5:
        score = 30 + (density - 2) * 15
    else:
        score = 75 + min((density - 5) * 5, 25)

    return min(score, 100), found_words[:10]  # 只返回前10个


def calculate_sentence_uniformity(text: str) -> Tuple[float, float]:
    """计算句子长度均匀性得分（0-100，越高越可能是 AI）"""
    sentences = split_sentences(text)
    if len(sentences) < 3:
        return 0, 0

    # 计算句子长度
    lengths = [len(s) for s in sentences]
    avg_len = sum(lengths) / len(lengths)

    # 计算标准差
    variance = sum((l - avg_len) ** 2 for l in lengths) / len(lengths)
    std_dev = variance ** 0.5

    # 计算变异系数（CV）
    cv = std_dev / avg_len if avg_len > 0 else 0

    # CV 转换为得分
    # CV < 0.3: 非常均匀（高 AI）
    # CV 0.3-0.6: 中等均匀
    # CV > 0.6: 不均匀（低 AI）
    if cv < 0.3:
        score = 100 - cv * 100
    elif cv < 0.6:
        score = 70 - (cv - 0.3) * 100
    else:
        score = max(40 - (cv - 0.6) * 50, 0)

    return min(score, 100), cv


def calculate_paragraph_uniformity(text: str) -> Tuple[float, float]:
    """计算段落长度均匀性得分（0-100，越高越可能是 AI）"""
    paragraphs = split_paragraphs(text)
    if len(paragraphs) < 3:
        return 0, 0

    # 计算段落长度
    lengths = [len(p) for p in paragraphs]
    avg_len = sum(lengths) / len(lengths)

    # 计算标准差
    variance = sum((l - avg_len) ** 2 for l in lengths) / len(lengths)
    std_dev = variance ** 0.5

    # 计算变异系数（CV）
    cv = std_dev / avg_len if avg_len > 0 else 0

    # CV 转换为得分
    if cv < 0.3:
        score = 100 - cv * 100
    elif cv < 0.6:
        score = 70 - (cv - 0.3) * 100
    else:
        score = max(40 - (cv - 0.6) * 50, 0)

    return min(score, 100), cv


def calculate_connector_score(text: str) -> Tuple[float, List[str]]:
    """计算连接词得分（0-100，越高越可能是 AI）"""
    found_connectors = []
    total_count = 0

    for connector in AI_CONNECTORS:
        count = text.count(connector)
        if count > 0:
            found_connectors.append(f"{connector}({count})")
            total_count += count

    text_len = len(text)
    if text_len == 0:
        return 0, []

    # 计算连接词密度（每千字出现次数）
    density = (total_count / text_len) * 1000

    # 密度转换为得分
    if density < 1:
        score = density * 20
    elif density < 3:
        score = 20 + (density - 1) * 20
    else:
        score = 60 + min((density - 3) * 10, 40)

    return min(score, 100), found_connectors


def calculate_template_score(text: str) -> Tuple[float, List[str]]:
    """计算模板化表达得分（0-100，越高越可能是 AI）"""
    found_templates = []
    total_count = 0

    for template in AI_TEMPLATES:
        matches = re.findall(template, text)
        if matches:
            found_templates.append(f"{template}({len(matches)})")
            total_count += len(matches)

    text_len = len(text)
    if text_len == 0:
        return 0, []

    # 计算模板密度（每千字出现次数）
    density = (total_count / text_len) * 1000

    # 密度转换为得分
    if density < 1:
        score = density * 25
    elif density < 2:
        score = 25 + (density - 1) * 25
    else:
        score = 50 + min((density - 2) * 15, 50)

    return min(score, 100), found_templates


def detect_ai_content(text: str) -> Dict:
    """
    检测文本的 AI 生成概率

    返回:
        {
            "ai_score": 0-100,  # 总体 AI 率
            "level": "低/中/高",  # AI 率等级
            "details": {
                "high_freq": {"score": 0-100, "words": [...]},
                "sentence_uniformity": {"score": 0-100, "cv": 0-1},
                "paragraph_uniformity": {"score": 0-100, "cv": 0-1},
                "connectors": {"score": 0-100, "words": [...]},
                "templates": {"score": 0-100, "patterns": [...]}
            },
            "suggestions": [...]  # 改进建议
        }
    """
    # 清理文本
    cleaned = clean_text(text)

    # 计算各项得分
    high_freq_score, high_freq_words = calculate_high_freq_score(cleaned)
    sentence_score, sentence_cv = calculate_sentence_uniformity(cleaned)
    paragraph_score, paragraph_cv = calculate_paragraph_uniformity(cleaned)
    connector_score, connector_words = calculate_connector_score(cleaned)
    template_score, template_patterns = calculate_template_score(cleaned)

    # 计算加权总分（AI 率）
    weights = {
        "high_freq": 0.30,
        "sentence": 0.20,
        "paragraph": 0.20,
        "connector": 0.15,
        "template": 0.15
    }

    total_score = (
        high_freq_score * weights["high_freq"] +
        sentence_score * weights["sentence"] +
        paragraph_score * weights["paragraph"] +
        connector_score * weights["connector"] +
        template_score * weights["template"]
    )

    # 确定等级
    if total_score < 30:
        level = "低"
    elif total_score < 60:
        level = "中"
    else:
        level = "高"

    # 生成改进建议
    suggestions = []
    if high_freq_score > 50:
        suggestions.append("减少高频词使用，替换为更具体的表达")
    if sentence_score > 50:
        suggestions.append("调整句子长度，增加长短句变化")
    if paragraph_score > 50:
        suggestions.append("调整段落长度，避免过于均匀")
    if connector_score > 50:
        suggestions.append("减少线性连接词，使用更自然的过渡")
    if template_score > 50:
        suggestions.append("避免模板化句式，使用更个性化的表达")

    return {
        "ai_score": round(total_score, 1),
        "level": level,
        "details": {
            "high_freq": {
                "score": round(high_freq_score, 1),
                "words": high_freq_words
            },
            "sentence_uniformity": {
                "score": round(sentence_score, 1),
                "cv": round(sentence_cv, 3)
            },
            "paragraph_uniformity": {
                "score": round(paragraph_score, 1),
                "cv": round(paragraph_cv, 3)
            },
            "connectors": {
                "score": round(connector_score, 1),
                "words": connector_words
            },
            "templates": {
                "score": round(template_score, 1),
                "patterns": template_patterns
            }
        },
        "suggestions": suggestions
    }


def format_report(result: Dict, filename: str = "") -> str:
    """格式化检测报告"""
    lines = []
    lines.append("=" * 60)
    lines.append("AI 生成内容检测报告")
    if filename:
        lines.append(f"文件: {filename}")
    lines.append("=" * 60)
    lines.append("")

    # 总体评分
    score = result["ai_score"]
    level = result["level"]

    # 创建进度条
    bar_len = 30
    filled = int(score / 100 * bar_len)
    bar = "█" * filled + "░" * (bar_len - filled)

    lines.append(f"AI 率: [{bar}] {score}% ({level})")
    lines.append("")

    # 各项详情
    lines.append("-" * 60)
    lines.append("检测维度分析:")
    lines.append("-" * 60)

    details = result["details"]

    # 高频词
    hf = details["high_freq"]
    lines.append(f"1. 高频词使用:     {hf['score']}%")
    if hf["words"]:
        lines.append(f"   发现: {', '.join(hf['words'][:5])}")
    lines.append("")

    # 句子均匀性
    su = details["sentence_uniformity"]
    lines.append(f"2. 句子长度均匀性: {su['score']}% (变异系数: {su['cv']})")
    lines.append("")

    # 段落均匀性
    pu = details["paragraph_uniformity"]
    lines.append(f"3. 段落长度均匀性: {pu['score']}% (变异系数: {pu['cv']})")
    lines.append("")

    # 连接词
    cn = details["connectors"]
    lines.append(f"4. 线性连接词:     {cn['score']}%")
    if cn["words"]:
        lines.append(f"   发现: {', '.join(cn['words'][:5])}")
    lines.append("")

    # 模板化表达
    tp = details["templates"]
    lines.append(f"5. 模板化表达:     {tp['score']}%")
    if tp["patterns"]:
        lines.append(f"   发现: {', '.join(tp['patterns'][:3])}")
    lines.append("")

    # 改进建议
    if result["suggestions"]:
        lines.append("-" * 60)
        lines.append("改进建议:")
        lines.append("-" * 60)
        for i, suggestion in enumerate(result["suggestions"], 1):
            lines.append(f"{i}. {suggestion}")
        lines.append("")

    lines.append("=" * 60)
    return "\n".join(lines)


def compare_reports(before: Dict, after: Dict, filename: str = "") -> str:
    """生成对比报告"""
    lines = []
    lines.append("=" * 60)
    lines.append("AI 率对比报告")
    if filename:
        lines.append(f"文件: {filename}")
    lines.append("=" * 60)
    lines.append("")

    # 总体对比
    before_score = before["ai_score"]
    after_score = after["ai_score"]
    diff = before_score - after_score
    diff_pct = (diff / before_score * 100) if before_score > 0 else 0

    lines.append("总体 AI 率对比:")
    lines.append(f"  处理前: {before_score}% ({before['level']})")
    lines.append(f"  处理后: {after_score}% ({after['level']})")

    if diff > 0:
        lines.append(f"  降低幅度: {diff:.1f}% (下降 {diff_pct:.1f}%)")
    elif diff < 0:
        lines.append(f"  变化: {abs(diff):.1f}% (上升 {abs(diff_pct):.1f}%)")
    else:
        lines.append("  变化: 无变化")
    lines.append("")

    # 各维度对比
    lines.append("-" * 60)
    lines.append("各维度对比:")
    lines.append("-" * 60)
    lines.append(f"{'维度':<15} {'处理前':<10} {'处理后':<10} {'变化':<10}")
    lines.append("-" * 60)

    dimensions = [
        ("高频词", "high_freq"),
        ("句子均匀性", "sentence_uniformity"),
        ("段落均匀性", "paragraph_uniformity"),
        ("连接词", "connectors"),
        ("模板化表达", "templates")
    ]

    for name, key in dimensions:
        b_score = before["details"][key]["score"]
        a_score = after["details"][key]["score"]
        d = b_score - a_score

        if d > 0:
            change = f"↓{d:.1f}"
        elif d < 0:
            change = f"↑{abs(d):.1f}"
        else:
            change = "-"

        lines.append(f"{name:<15} {b_score:<10.1f} {a_score:<10.1f} {change:<10}")

    lines.append("")

    # 效果评估
    lines.append("-" * 60)
    lines.append("效果评估:")
    lines.append("-" * 60)

    if diff_pct >= 30:
        lines.append("✓ 降 AI 效果显著")
    elif diff_pct >= 15:
        lines.append("✓ 降 AI 效果明显")
    elif diff_pct >= 5:
        lines.append("△ 降 AI 效果一般")
    elif diff_pct > 0:
        lines.append("△ 降 AI 效果较弱")
    else:
        lines.append("✗ 未检测到明显改善")

    lines.append("")
    lines.append("=" * 60)

    return "\n".join(lines)


def main():
    """命令行入口"""
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

    if len(sys.argv) < 2:
        print("用法: python ai_detector.py <markdown_file> [--compare <after_file>]")
        print("      python ai_detector.py --before <before_file> --after <after_file>")
        sys.exit(1)

    if sys.argv[1] == "--compare" and len(sys.argv) >= 4:
        # 对比模式
        before_file = sys.argv[2]
        after_file = sys.argv[4]

        before_text = Path(before_file).read_text(encoding="utf-8")
        after_text = Path(after_file).read_text(encoding="utf-8")

        before_result = detect_ai_content(before_text)
        after_result = detect_ai_content(after_text)

        report = compare_reports(before_result, after_result, before_file)
        print(report)

    elif sys.argv[1] == "--before" and len(sys.argv) >= 4:
        # 对比模式（另一种语法）
        before_file = sys.argv[2]
        after_file = sys.argv[4]

        before_text = Path(before_file).read_text(encoding="utf-8")
        after_text = Path(after_file).read_text(encoding="utf-8")

        before_result = detect_ai_content(before_text)
        after_result = detect_ai_content(after_text)

        report = compare_reports(before_result, after_result, before_file)
        print(report)

    else:
        # 单文件检测模式
        filepath = sys.argv[1]
        text = Path(filepath).read_text(encoding="utf-8")

        result = detect_ai_content(text)
        report = format_report(result, filepath)
        print(report)

        # 输出 JSON 格式（供程序调用）
        if "--json" in sys.argv:
            print("\n" + json.dumps(result, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
