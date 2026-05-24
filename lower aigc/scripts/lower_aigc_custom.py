#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
降 AI 处理脚本 - 根据用户自定义配置执行
"""

import re
import random
import sys
from pathlib import Path

# 100%覆盖所有AI高频词的完整词库
WORD_REPLACEMENTS = {
    # 基础黑话
    "赋能": ["帮着", "支持", "给...用上"],
    "深耕": ["做了很久", "一直专注于"],
    "聚焦": ["盯着", "主要做", "重点关注"],
    "助力": ["帮", "推着"],
    "打造": ["做", "搞", "建"],
    "引领": ["带头", "领着"],
    "全方位": ["各方面"],
    "多维度": ["好几个角度"],
    "高质量": ["做得不错的", "靠谱的"],
    "沉浸式": ["全身心的", "很投入的"],
    "一站式": ["全包的", "啥都有"],
    "闭环": ["从头到尾", "能循环起来"],
    "抓手": ["办法", "切入点", "工具"],
    "底层逻辑": ["根本原因", "说白了就是"],
    "顶层设计": ["整体规划", "大方向"],
    "新质生产力": ["新的产能", "新的技术能力"],
    "降本增效": ["省钱", "提效率"],
    "提质增效": ["做得更好更快"],
    "数智化": ["数字化", "用上了AI"],
    "落地": ["落实好", "做成了"],
    "沉淀": ["攒下来", "积累下来"],
    "给到": ["给了", "提供"],
    "响应": ["回应", "回复"],
    "同步": ["跟大家说一声", "让大家信息对齐"],
    "对齐": ["把目标说清楚", "达成一致"],
    "对标": ["跟好的比", "找差距"],
    "迭代": ["更新", "改一改"],
    "优化": ["改一改", "调一调"],
    "跟进": ["盯着点", "跟着推进"],
    "升级": ["更新", "改得更好"],
    "交付": ["交出去", "做完给对方"],
    "倒逼": ["逼着", "靠着外部压力改"],
    "复盘": ["回头看看", "总结一下"],
    "梳理": ["理一理", "整理清楚"],
    "输出": ["拿出来", "给大家看"],
    "提炼": ["挑重点", "抽出来"],
    "包装": ["宣传一下", "弄好看点"],
    "上升": ["提上去", "升上去"],
    "方案": ["计划", "办法"],
    "协同": ["一起配合", "搭伙干"],
    "联动": ["一起动", "互相配合"],
    "透传": ["传下去", "告诉下面"],
    "打通": ["连起来", "弄通了"],
    "打平": ["扯平", "弄平衡"],
    "抹平": ["拉平", "消掉差距"],
    "发力": ["使劲", "集中力量干"],
    "兼容": ["能一起用", "适配"],
    "量化": ["用数字说", "算清楚"],
    "细分": ["拆小", "分成小块"],
    "重塑": ["重新做", "改头换面"],
    "蓄能": ["攒力气", "准备准备"],
    "引爆": ["带火", "搞火"],
    "挖掘": ["找一找", "挖一挖"],
    "背书": ["担保", "帮忙证明"],
    "支撑": ["撑着", "托着"],
    "协调": ["调一调", "帮着搭个手"],
    "支援": ["帮忙", "搭把手"],
    "加持": ["加个buff", "帮个忙"],
    "加速": ["加快", "快点弄"],
    "共建": ["一起做", "合伙干"],
    "共创": ["一起搞出来"],
    "融合": ["合到一起", "融进去"],
    "拉通": ["打通", "弄顺了"],
    "拉升": ["提一提", "涨上去"],
    "洞察": ["看明白", "摸清楚"],
    "渗透": ["进去", "扎进去"],
    "辐射": ["影响到", "带过去"],
    "扩展": ["扩大", "铺开"],
    "开拓": ["开新的", "闯一闯"],
    "兜底": ["兜着", "保证最差也这样"],
    "降级": ["降等级", "减配"],
    "容错": ["允许错", "留点余地"],
    "容灾": ["防灾难", "保稳定"],
    "解耦": ["拆开", "分开弄"],
    "耦合": ["绑一起", "互相影响"],
    "复用": ["拿来用", "重复用"],
    "封装": ["包起来", "藏细节"],
    "抽象": ["抽出来", "总结下"],
    "聚合": ["凑一起", "聚起来"],
    "集成": ["拼到一起", "合起来"],
    "拆解": ["拆开", "拆碎了"],
    "观察": ["看看", "盯着"],
    "监控": ["盯着", "看着点"],
    "上报": ["上报", "告诉上级"],
    "捕获": ["拿到", "抓住"],
    "分发": ["分下去", "发下去"],
    "分层": ["分层", "分等级"],
    "迁移": ["搬过去", "移过去"],
    "回溯": ["回头看", "追溯"],
    "回归": ["回去", "回到"],
    "通晒": ["公开给大家看", "亮出来"],
    "吃透": ["搞懂", "弄明白"],
    "死磕": ["死磕", "坚持到底"],
    "树立": ["建立", "立起来"],
    "跨界": ["跨行业", "跨领域"],
    "共情": ["理解别人", "感同身受"],
    "演绎": ["推理", "讲清楚"],
    "画饼": ["画饼", "吹牛皮"],
    "反哺": ["反过来帮", "回馈"],
    "输血": ["输资源", "外部帮忙"],
    "造血": ["自己攒能力", "自己搞"],
    "造势": ["搞声势", "炒热度"],
    "下沉": ["往下铺", "去小城市"],
    "拉新": ["拉新用户", "找新人"],
    "转化": ["变成付费用户", "下单"],
    "留存": ["留住用户", "不让人走"],
    "促活": ["让大家活跃点"],
    "付费": ["花钱", "买单"],
    "营收": ["收入", "赚的钱"],
    "盈利": ["赚钱", "有利润"],
    "获客": ["找客户", "拉客户"],
    "激励": ["鼓励", "给奖励"],
    "激活": ["叫醒用户", "弄活"],
    "推广": ["宣传", "推一推"],
    "投放": ["投广告", "放资源"],
    "导流": ["引流量", "导人"],
    "覆盖": ["涉及到", "覆盖到"],
    "曝光": ["让人看到", "露个脸"],
    "裂变": ["一传十十传百"],
    "增长": ["涨了", "变多了"],
    "优秀": ["不错", "挺好的"],
    "感恩": ["谢谢", "感谢"],
    "订阅": ["关注", "订上"],
    "认证": ["验证", "确认身份"],
    "推送": ["发消息", "推给你"],
    "履约": ["按约定做完", "兑现承诺"],
    "进化": ["进化", "越做越好"],
    "进军": ["进去", "闯进去"],
    "起飞": ["起飞", "爆了"],
    "收割": ["收割", "赚一波"],
    "共享": ["共享", "一起用"],
    "重组": ["重组", "重新搭"],
    "收口": ["收尾", "收个尾"],
    "转型": ["转型", "换方向"],
    "围绕": ["围绕", "围着"],
    "出击": ["出击", "冲出去"],
    "评估": ["评估", "看看"],
    "评审": ["评审", "把关"],
    "务实": ["务实", "实在点"],
    "夯实": ["夯实", "打基础"],
    "预判": ["预判", "提前猜"],
    "深入": ["深入", "好好研究"],
    "打磨": ["磨一磨", "改细节"],
    "攻坚": ["啃硬骨头", "解决难题"],
    "击穿": ["突破", "打穿"],
    "破冰": ["破冰", "打破尴尬"],
    "破题": ["破题", "找切入点"],
    "解题": ["解题", "解决问题"],
    "破圈": ["出圈", "让更多人知道"],
    "破局": ["打破僵局", "走出困境"],
    "突围": ["突围", "跑出来"],
    "补位": ["补位", "填坑"],
    "抽离": ["抽离", "跳出来看"],
    "皮实": ["皮实", "抗造"],
    "本分": ["本分", "老实"],
    "重磅": ["重磅", "大的"],
    "垂直": ["垂直", "专注的"],
    "精准": ["精准", "准的"],
    "持续": ["一直", "不停"],
    "灵活": ["灵活", "能变"],
    "稳定": ["稳定", "不变"],
    "可控": ["可控", "能管"],
    "活跃": ["活跃", "能动"],
    "漏斗": ["漏斗", "转化流程"],
    "中台": ["中台", "中间平台"],
    "平台": ["平台", "服务端"],
    "风口": ["风口", "机会"],
    "打法": ["办法", "路子"],
    "玩法": ["玩法", "法子"],
    "矩阵": ["矩阵", "一堆号"],
    "纽带": ["纽带", "连接的"],
    "刺激": ["刺激", "驱动"],
    "规模": ["规模", "大小"],
    "场景": ["情况", "时候"],
    "渠道": ["路子", "途径"],
    "入口": ["入口", "进门的地方"],
    "维度": ["角度", "方面"],
    "格局": ["格局", "视野"],
    "形态": ["形态", "样子"],
    "生态": ["圈子", "大环境"],
    "体系": ["一套东西", "系统"],
    "认知": ["印象", "认知"],
    "体感": ["感受", "体验"],
    "感知": ["感觉", "知道"],
    "心智": ["印象", "认知"],
    "调性": ["风格", "调调"],
    "战役": ["活动", "大项目"],
    "合力": ["合力", "一起使劲"],
    "心力": ["心力", "心思"],
    "赛道": ["行业", "领域"],
    "基石": ["基础", "底子"],
    "基因": ["基因", "特质"],
    "因子": ["因素", "变量"],
    "模型": ["模式", "模型"],
    "通道": ["路子", "通道"],
    "链路": ["流程", "路子"],
    "水位": ["标准", "水平"],
    "水准": ["水平", "标准"],
    "姿态": ["态度", "样子"],
    "卡点": ["卡点", "阻碍"],
    "卡位": ["卡位", "占位置"],
    "头部": ["头部", "顶尖的"],
    "腰部": ["腰部", "中间的"],
    "痛点": ["问题", "难处"],
    "爽点": ["爽点", "舒服的"],
    "痒点": ["痒点", "想要的"],
    "全域": ["所有地方", "全范围"],
    "公域": ["公共平台", "外面的"],
    "私域": ["自己的用户群"],
    "蓝海": ["新市场", "没人的地方"],
    "红海": ["竞争大的市场"],
    "壁垒": ["门槛", "障碍"],
    "变量": ["变量", "变化的"],
    "边界": ["边界", "范围"],
    "品牌": ["品牌", "牌子"],
    "阵地": ["阵地", "地盘"],
    "高地": ["优势", "好地方"],
    "洼地": ["劣势", "差地方"],
    "革命": ["革命", "大变革"],
    "变革": ["变革", "变化"],
    "内卷": ["内卷", "内部卷"],
    "脑暴": ["头脑风暴", "想点子"],
    "脑洞": ["脑洞", "想法"],
    "圈层": ["圈子", "圈层"],
    "层级": ["层级", "等级"],
    "段位": ["段位", "水平"],
    "环节": ["环节", "部分"],
    "困局": ["困境", "难局"],
    "话术": ["话术", "说法"],
    "文案": ["文案", "宣传文"],
    "议程": ["议题", "讨论的"],
    "触点": ["接触点", "碰到的"],
    "势能": ["优势", "积累的"],
    "流量": ["用户", "人"],
    "资源": ["资源", "东西"],
    "排期": ["排期", "时间安排"],
    "埋点": ["埋点", "数据点"],
    "坑位": ["位置", "名额"],
    "峰值": ["峰值", "最高的"],
    "漏洞": ["漏洞", "bug"],
    "风险": ["风险", "危险"],
    "瓶颈": ["瓶颈", "卡脖子的"],
    "策略": ["策略", "办法"],
    "价值": ["价值", "用的"],
    "成本": ["成本", "花的钱"],
    "复利": ["复利", "利滚利"],
    "利器": ["利器", "好用的工具"],
    "深度": ["深度", "深入的"],
    "玩家": ["玩家", "参与的人"],
    "小白": ["新手", "小白"],
    "韭菜": ["韭菜", "被割的"],
    "羊毛": ["羊毛", "优惠"],
    "福利": ["福利", "优惠"],
    "套路": ["套路", "坑"],
    "情怀": ["情怀", "感情"],
    "标准": ["标准", "规范"],
    "规范": ["规范", "规矩"],
    "社群": ["社群", "群"],
    "产业": ["产业", "行业"],
    "载体": ["载体", "东西"],
    "服务": ["服务", "帮忙"],
    "粘性": ["粘性", "忠诚度"],
    "属性": ["属性", "特点"],
    "地域": ["地区", "地方"],
    "终端": ["终端", "用户端"],
    "版本": ["版本", "版"],
    "口碑": ["口碑", "评价"],
    "指标": ["指标", "考核的"],
    "试点": ["试点", "测试的"],
    "空白": ["空白", "空的"],
    "生命周期": ["生命周期", "从生到死"],
    "商业模式": ["赚钱的路子", "商业模式"],
    "解决方案": ["解决办法", "方案"],
    "私域流量": ["自己的用户"],
    "用户心智": ["用户的印象"],
    "用户粘性": ["用户的忠诚度"],
    "用户体验": ["用着的感受"],
    "用户画像": ["用户的样子"],
    "快速迭代": ["快速更新", "不停改"],
    "持续迭代": ["一直更新", "慢慢改"],
    "降维打击": ["欺负人", "降维打"],
    "品效合一": ["又有品牌又有效果"],
    "逻辑自洽": ["说得通"],
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

    # 提取锚点注释（如果有）
    anchor = ""
    text = para
    if para.startswith('<!--') and '-->' in para:
        # 提取锚点注释
        end_idx = para.index('-->') + 3
        anchor = para[:end_idx]
        text = para[end_idx:].strip()

    # 如果文本为空，直接返回
    if not text:
        return para

    # 应用各种处理（最高档：替换所有符合条件的内容）
    if config['high_freq'] >= 6:
        # 最高档：替换所有高频词
        for word, alts in WORD_REPLACEMENTS.items():
            while word in text:
                text = text.replace(word, random.choice(alts), 1)
    elif config['high_freq'] >= 4:
        text = replace_high_freq_words(text, frequency=500)
    elif config['high_freq'] >= 3:
        text = replace_high_freq_words(text, frequency=1000)
    else:
        text = replace_high_freq_words(text, frequency=2000)

    if config['double_single'] >= 6:
        # 最高档：替换所有双音词
        for word, replacement in DOUBLE_TO_SINGLE.items():
            text = text.replace(word, replacement)
    elif config['double_single'] >= 3:
        text = replace_double_to_single(text, frequency=1000)
    else:
        text = replace_double_to_single(text, frequency=2000)

    if config['sentence_perturb'] >= 6:
        text = perturb_sentence_structure(text, intensity="high")
    elif config['sentence_perturb'] >= 5:
        text = perturb_sentence_structure(text, intensity="high")
    elif config['sentence_perturb'] >= 3:
        text = perturb_sentence_structure(text, intensity="medium")

    if config['linear_conn'] >= 6:
        # 最高档：删除所有线性连接词
        for conn in LINEAR_CONNECTORS:
            text = text.replace(conn, "")
    elif config['linear_conn'] >= 5:
        text = remove_linear_connectors(text, frequency=200)
    elif config['linear_conn'] >= 3:
        text = remove_linear_connectors(text, frequency=500)
    else:
        text = remove_linear_connectors(text, frequency=1500)

    if config['template'] >= 6:
        # 最高档：替换所有模板化表达
        for pattern, replacement in TEMPLATE_EXPRESSIONS:
            text = re.sub(pattern, replacement, text)
    elif config['template'] >= 4:
        text = apply_template_replacements(text, frequency=500)
    else:
        text = apply_template_replacements(text, frequency=1000)

    if config['adverb'] >= 6:
        # 最高档：删除所有无依据副词
        adverbs = [
            "非常", "极其", "十分", "相当", "颇为", "甚为",
            "极为", "显著", "明显", "深刻", "重大", "重要",
            "关键", "核心", "根本", "本质", "基本", "全面",
            "系统", "整体", "综合", "完整", "深入", "细致",
            "具体", "详细", "充分", "有效", "有力", "高效",
            "快速", "迅速", "持续", "不断", "长期", "始终", "一贯",
        ]
        for adv in adverbs:
            text = text.replace(adv, "")
    elif config['adverb'] >= 5:
        text = remove_unfounded_adverbs(text, frequency=200)
    elif config['adverb'] >= 3:
        text = remove_unfounded_adverbs(text, frequency=500)

    if config['viewpoint'] >= 6:
        # 最高档：在每个段落前插入作者观点
        viewpoints = ["在我看来，", "就我个人的理解，", "从实际观察来看，", "据我所知，"]
        sentences = re.split(r'(?<=[。！？])', text)
        result = []
        for i, sent in enumerate(sentences):
            if len(sent) > 30 and random.random() < 0.3:
                result.append(random.choice(viewpoints))
            result.append(sent)
        text = ''.join(result)
    elif config['viewpoint'] >= 4:
        text = insert_author_viewpoint(text, frequency=500)
    elif config['viewpoint'] >= 3:
        text = insert_author_viewpoint(text, frequency=1000)

    # 重新组合锚点注释和处理后的文本
    if anchor:
        return anchor + "\n" + text
    else:
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

    # 用户配置 - 全部最高档
    config = {
        'high_freq': 6,        # 高频词替换：最高档，替换所有高频词
        'double_single': 6,    # 双音词→单音词：最高档，替换所有双音词
        'sentence_perturb': 6, # 句式结构扰动：最高档
        'linear_conn': 6,      # 删除线性衔接词：最高档，删除所有连接词
        'paragraph_jump': 6,   # 段结构跳跃：最高档
        'uniformity': 6,       # 打破段落均匀性：最高档
        'viewpoint': 6,        # 插入作者观点：最高档
        'empty_term': 6,       # 空洞术语处理：最高档
        'adverb': 6,           # 无依据副词删除：最高档
        'template': 6,         # 模板化表达处理：最高档
    }

    print(f"正在处理: {input_file}")
    print(f"输出文件: {output_file}")
    print(f"配置: {config}")

    result = process_document(input_file, output_file, config)

    print(f"处理完成！共 {count_chars(result)} 字")

if __name__ == "__main__":
    main()
