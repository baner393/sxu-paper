#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
降 AI 处理脚本 - 完整版，覆盖所有高频词
"""
import re
import random
import sys
from pathlib import Path

# 完整的高频词替换词库（从word-bank.md补全，覆盖所有200+AI高频词）
WORD_REPLACEMENTS = {
    # 基础黑话
    "赋能": ["帮", "支持", "给...用上"],
    "深耕": ["做了很久", "专注于"],
    "聚焦": ["关注", "盯着", "主要做"],
    "助力": ["帮", "推动"],
    "打造": ["做", "建", "搞"],
    "引领": ["带头", "领先"],
    "全方位": ["各方面"],
    "多维度": ["数个角度"],
    "高质量": ["做好的", "靠谱的"],
    "沉浸式": ["全身心的", "投入的"],
    "一站式": ["全包", "什么都有"],
    "闭环": ["从头到尾", "形成循环"],
    "抓手": ["切入点", "办法", "工具"],
    "底层逻辑": ["根本原因", "核心原理", "说白了"],
    "顶层设计": ["整体规划", "大方向"],
    "新质生产力": ["新的产能", "新的技术能力"],
    "降本增效": ["省钱", "提效率"],
    "提质增效": ["做得更好更快"],
    "数智化": ["数字化", "用上了AI"],
    "落地": ["将计划、方案转化为实际行动并取得成果"],
    "沉淀": ["将知识和经验积累起来"],
    "给到": ["提供", "给予"],
    "响应": ["回应", "回复"],
    "同步": ["同时", "让人们信息对齐"],
    "对齐": ["把目标说清楚", "达成一致"],
    "对标": ["跟好的比", "找差距"],
    "迭代": ["更新", "改一改"],
    "优化": ["改一改", "调一调"],
    "跟进": ["盯点", "跟推进"],
    "升级": ["更新", "变好"],
    "交付": ["交出去", "做完给对方"],
    "倒逼": ["逼", "靠外部压力改"],
    "复盘": ["回头看看", "总结"],
    "梳理": ["理一理", "整理清楚"],
    "输出": ["拿出来", "给人们看"],
    "提炼": ["挑重点", "抽出来"],
    "包装": ["宣传", "弄好看点"],
    "上升": ["提上去", "升上去"],
    "方案": ["计划", "办法"],
    "协同": ["全配合", "搭伙干"],
    "联动": ["全动", "互相配合"],
    "透传": ["传下去", "告诉下面"],
    "打通": ["连起来", "弄通"],
    "发力": ["使劲", "集中力量干"],
    "量化": ["用数字说", "算清楚"],
    "细分": ["拆成小份", "分成小块"],
    "重塑": ["重新做", "改头换面"],
    "蓄能": ["聚力", "准备准备"],
    "引爆": ["带火", "火"],
    "挖掘": ["找一找", "挖一挖"],
    "背书": ["担保", "帮忙证明"],
    "支撑": ["撑", "托"],
    "协调": ["调一调", "相互搭把手"],
    "支援": ["帮忙", "搭把手"],
    "加持": ["加个buff", "帮忙"],
    "加速": ["加快", "快点弄"],
    "共建": ["全做", "合伙干"],
    "共创": ["一起"],
    "融合": ["合成一体", "融进去"],
    "拉通": ["打通", "弄顺"],
    "拉升": ["提一提", "涨上去"],
    "洞察": ["看明白", "摸清楚"],
    "渗透": ["进去", "扎进去"],
    "辐射": ["影响到", "带过去"],
    "扩展": ["扩大", "铺开"],
    "开拓": ["开新", "闯"],
    "兜底": ["兜", "保证最差也这样"],
    "解耦": ["拆开", "分开弄"],
    "复用": ["拿来用", "重复用"],
    "集成": ["拼到全", "合起来"],
    "拆解": ["拆开", "拆碎"],
    "通晒": ["公开给人们看", "亮出来"],
    "吃透": ["搞懂", "弄明白"],
    "破圈": ["出圈", "让更多人知道"],
    "破局": ["打破僵局", "走出困境"],
    "触达": ["找到", "联系上"],
    "链路": ["流程", "路子"],
    "赛道": ["行业", "领域"],
    "痛点": ["问题", "难处"],
    "私域": ["自己的用户群"],
    "流量": ["用户", "人"],
    "拉新": ["拉新用户", "找新人"],
    "转化": ["变成付费用户", "下单"],
    "留存": ["留住用户", "不让人走"],
    "促活": ["让人们活跃点"],
    "营收": ["收入", "赚的钱"],
    "盈利": ["赚钱", "有利润"],
    "获客": ["找客户", "拉客户"],
    "激活": ["叫醒沉睡的用户"],
    "推广": ["宣传", "推一推"],
    "覆盖": ["涉及到", "覆盖到"],
    "曝光": ["让人看到", "露个脸"],
    "裂变": ["一传十十传百"],
    "增长": ["涨", "变多"],
    "履约": ["按约定做完", "兑现承诺"],
    "打磨": ["磨一磨", "改细节"],
    "攻坚": ["啃硬骨头", "解决难题"],
    "击穿": ["突破", "打穿"],
    "中台": ["中间的平台"],
    "风口": ["风口", "机会"],
    "打法": ["办法", "路子"],
    "矩阵": ["一堆账号/产品"],
    "场景": ["情况", "时候"],
    "渠道": ["路子", "途径"],
    "维度": ["角度", "方面"],
    "生态": ["圈子", "大环境"],
    "体系": ["一套东西", "系统"],
    "心智": ["印象", "认知"],
    "壁垒": ["门槛", "障碍"],
    "下沉": ["往下铺", "去小城市"],
    "用户心智": ["用户的印象"],
    "用户粘性": ["用户的忠诚度"],
    "用户体验": ["用的感受"],
    "私域流量": ["自己的用户"],
    "降维打击": ["欺负人", "降维打"],
    "品效合一": ["又有品牌又有效果"],
    "逻辑自洽": ["说通", "前后一致"],
    # 补充的常见AI词
    "颗粒度": ["细致程度", "细节"],
    "势能": ["积累的优势", "底气"],
    "体感": ["感觉", "体验"],
    "感知": ["感觉到", "体会到"],
    "调性": ["风格", "调调"],
    "战役": ["活动", "行动"],
    "合力": ["全使劲", "共同的力量"],
    "心力": ["精力", "心劲"],
    "基石": ["基础", "根基"],
    "基因": ["底子", "本质"],
    "因子": ["因素", "变量"],
    "模型": ["模式", "框架"],
    "通道": ["渠道", "路径"],
    "水位": ["水平", "标准"],
    "水准": ["水平", "程度"],
    "姿态": ["态度", "架势"],
    "卡点": ["卡住的地方", "难点"],
    "卡位": ["占位", "找准位置"],
    "头部": ["顶尖的", "头部的"],
    "腰部": ["中间的", "中等的"],
    "爽点": ["让人满意的地方", "亮点"],
    "痒点": ["让人想要的地方", "期待"],
    "全域": ["所有地方", "全范围"],
    "公域": ["公共平台", "开放流量"],
    "蓝海": ["没怎么竞争的市场", "新领域"],
    "红海": ["竞争激烈的市场", "人们都抢做"],
    "变量": ["变化的因素", "不确定的"],
    "边界": ["界限", "范围"],
    "阵地": ["地盘", "位置"],
    "高地": ["优势位置", "制高点"],
    "洼地": ["劣势位置", "短板"],
    "革命": ["大变革", "颠覆"],
    "变革": ["改变", "改革"],
    "内卷": ["过度竞争", "互相卷"],
    "圈层": ["圈子", "群体"],
    "环节": ["步骤", "部分"],
    "困局": ["困境", "难办的情况"],
    "话术": ["说法", "套路"],
    "触点": ["接触点", "接触的地方"],
    "峰值": ["最高点", "最高峰"],
    "漏洞": ["问题", "Bug"],
    "风险": ["危险", "隐患"],
    "瓶颈": ["卡住的地方", "难点"],
    "策略": ["办法", "招数"],
    "价值": ["用处", "好处"],
    "成本": ["花的钱", "代价"],
    "深度": ["深入程度", "透彻程度"],
    "口碑": ["名声", "评价"],
    "指标": ["数据", "衡量标准"],
    "试点": ["试验", "先试试"],
    "空白": ["空缺", "没人做的"],
    "生命周期": ["从头到尾的过程"],
    "商业模式": ["赚钱的办法"],
    "解决方案": ["解决办法", "应对方案"],
    "垂直领域": ["细分行业", "专门的领域"],
    "增长飞轮": ["增长的循环", "越转越快"],
    "第二曲线": ["新的增长点", "第二条路"],
    "快速迭代": ["快速更新", "快速改进"],
    "持续迭代": ["不断更新", "不断改进"],
    "有机结合": ["自然地结合", "融为一体"],
    "深度挖掘": ["深入寻找", "仔细挖"],
    "精准定位": ["找准位置", "对准目标"],
    "高效协同": ["配合好", "全干快"],
    "资源整合": ["把资源凑全", "统筹资源"],
    "价值共创": ["全创造价值"],
    "生态共建": ["全搭建环境"],
    "用户画像": ["用户的特点", "目标人群"],
    "用户旅程": ["用户的体验过程"],
    "用户留存": ["留住用户"],
    "用户增长": ["用户变多"],
    "流量变现": ["把流量变成钱"],
    "私域运营": ["经营自己的用户群"],
    "内容生态": ["内容的环境", "内容体系"],
    "数据驱动": ["靠数据来做决定"],
    "技术赋能": ["用技术来帮忙"],
    "数字化转型": ["改成用数字技术"],
    "智能化升级": ["变更智能"],
    "场景化营销": ["在具体场景下推广"],
    "全渠道": ["所有渠道", "线上线下都做"],
    "新零售": ["新的卖货方式"],
    "新消费": ["新的消费方式"],
    "新基建": ["新的基础设施"],
    "产业互联网": ["行业的数字化"],
    "消费互联网": ["面向消费者的数字化"],
    # 补充缺失的词
    "兼容": ["能全用", "配合上"],
    "容错": ["允许犯错", "有缓冲"],
    "耦合": ["绑在一块", "互相牵连"],
    "封装": ["包起来", "打包"],
    "抽象": ["笼统的", "不具体的"],
    "聚合": ["凑到一起", "聚起来"],
    "观察": ["看看", "留意"],
    "监控": ["盯", "看"],
    "捕获": ["抓到", "拿到"],
    "分发": ["分出去", "发下去"],
    "分层": ["分等级", "分层次"],
    "迁移": ["搬过去", "转过去"],
    "回溯": ["回头查", "追回去"],
    "回归": ["回到", "回到原来"],
    "死磕": ["死盯", "硬啃"],
    "树立": ["立起来", "建立"],
    "跨界": ["跨行", "不务正业"],
    "共情": ["感同身受", "理解对方"],
    "演绎": ["推导", "展示"],
    "画饼": ["许愿", "开空头支票"],
    "反哺": ["回馈", "反向支持"],
    "输血": ["给资源", "外部支援"],
    "造血": ["自己造血", "自力更生"],
    "造势": ["造声势", "搞热度"],
    "付费": ["花钱", "付钱"],
    "激励": ["鼓励", "奖励"],
    "投放": ["投进去", "放出去"],
    "导流": ["引流量", "把人引过去"],
    "优秀": ["不错", "挺好的"],
    "感恩": ["感谢", "谢谢"],
    "订阅": ["关注", "追更"],
    "认证": ["验证", "确认身份"],
    "推送": ["发通知", "推给你"],
    "进化": ["升级", "变好"],
    "进军": ["进入", "杀入"],
    "起飞": ["爆发", "飞速增长"],
    "收割": ["割韭菜", "赚一波"],
    "共享": ["全用", "公用"],
    "重组": ["重新组合", "重新安排"],
    "收口": ["收尾", "结束"],
    "转型": ["转变", "换方向"],
    "围绕": ["围", "以...为中心"],
    "出击": ["主动干", "出手"],
    "评估": ["评估", "衡量"],
    "评审": ["审核", "检查"],
    "务实": ["实在", "踏实"],
    "夯实": ["打牢", "巩固"],
    "预判": ["预估", "猜"],
    "深入": ["钻进去", "深入研究"],
    "打磨": ["磨", "改细节"],
    "攻坚": ["啃硬骨头", "解决难题"],
    "击穿": ["突破", "打穿"],
    "破冰": ["打破僵局", "打开局面"],
    "破题": ["找突破口", "想出路"],
    "解题": ["解决问题", "找答案"],
    "突围": ["冲出去", "突破"],
    "补位": ["补上", "顶上"],
    "抽离": ["抽出来", "跳出来看"],
    "皮实": ["抗造", "耐操"],
    "本分": ["老实", "本分"],
    "重磅": ["重大", "很重要"],
    "垂直": ["专注", "细分"],
    "精准": ["准确", "精确"],
    "持续": ["一直", "不断"],
    "灵活": ["随机应变", "不死板"],
    "稳定": ["稳", "靠谱"],
    "可控": ["能控制", "在掌控中"],
    "活跃": ["热闹", "积极"],
    "漏斗": ["转化流程", "筛选过程"],
    "平台": ["地方", "场子"],
    "玩法": ["方法", "花样"],
    "纽带": ["联系", "桥梁"],
    "刺激": ["激发", "刺激"],
    "规模": ["体量", "大小"],
    "入口": ["门", "入口处"],
    "格局": ["局面", "大势"],
    "形态": ["样子", "形式"],
    "认知": ["认识", "理解"],
    "品牌": ["牌子", "招牌"],
    "脑暴": ["头脑风暴", "全想"],
    "脑洞": ["创意", "想象力"],
    "层级": ["层次", "等级"],
    "段位": ["水平", "级别"],
    "文案": ["文字", "稿子"],
    "议程": ["议题", "讨论内容"],
    "资源": ["东西", "可用的"],
    "排期": ["安排时间", "排时间"],
    "埋点": ["数据采集点", "监控点"],
    "坑位": ["位置", "名额"],
    "楼层": ["层", "楼"],
    "复利": ["利滚利", "复合收益"],
    "利器": ["好工具", "法宝"],
    "玩家": ["参与者", "做的人"],
    "小白": ["新手", "入门的"],
    "韭菜": ["被割的", "小白用户"],
    "羊毛": ["优惠", "福利"],
    "福利": ["好处", "优惠"],
    "套路": ["招数", "手段"],
    "情怀": ["感情", "情结"],
    "标准": ["规矩", "要求"],
    "规范": ["规矩", "标准"],
    "社群": ["群", "圈子"],
    "产业": ["行业", "领域"],
    "载体": ["承载的东西", "容器"],
    "服务": ["帮忙", "服务"],
    "粘性": ["依赖度", "忠诚度"],
    "属性": ["特点", "性质"],
    "地域": ["地区", "地方"],
    "终端": ["设备", "端"],
    "版本": ["版", "更新"],
    "话语权": ["说话的份量", "影响力"],
    "透明度": ["公开程度", "透明程度"],
    # 从 word-bank.md 补充的词
    "摸索": ["试着找", "摸索着来"],
    "踩坑": ["遇到问题", "踩了雷"],
    "填坑": ["解决问题", "补漏洞"],
    "报备": ["报告", "备案"],
    "串联": ["串起来", "连起来"],
    "打平": ["拉平", "平衡"],
    "抹平": ["消除差距", "拉平"],
    "咬合": ["配合", "衔接"],
    "穿梭": ["来回跑", "来回移动"],
    "降级": ["降档次", "降低级别"],
    "容灾": ["防灾", "保证稳定"],
    "抓包": ["抓数据", "抓网络包"],
    "上报": ["报告", "向上汇报"],
    "回流": ["流回来", "回来"],
    "回跳": ["退回", "跳回去"],
    "造市": ["开拓市场", "搞市场"],
    "造事": ["搞事情", "制造事件"],
    "邀请": ["请", "约"],
    "比心": ["点赞", "感谢"],
    "下跪 / 致敬": ["佩服", "尊敬"],
    "证言": ["证明", "作证"],
    "确认": ["核实", "确定"],
    "预言": ["预测", "猜测"],
    "变迁": ["变化", "变动"],
    "返佣": ["返利", "提成"],
    "定量": ["用数据说", "量化"],
    "定性": ["凭感觉", "主观判断"],
    "制约": ["限制", "约束"],
    "约束": ["限制", "管着"],
    "触及": ["碰到", "接触到"],
    "触发": ["引发", "引起"],
    "操盘": ["管", "负责"],
    "思考": ["想", "琢磨"],
    "反思": ["反省", "回头想"],
    "精简": ["简化", "砍掉"],
    "真香": ["打脸", "真好"],
    "自洽": ["说得通", "前后一致"],
}

# 双音词→单音词替换（共100个，AI常用双音词，替换后自然不生硬）
DOUBLE_TO_SINGLE = {
    "进行": "做",
    "实施": "做",
    "开展": "做",
    "实现": "成",
    "促进": "推",
    "提升": "提",
    "增强": "加",
    "推进": "推",
    "推动": "推",
    "加强": "加",
    "加大": "加",
    "加快": "快",
    "提高": "提",
    "降低": "降",
    "减少": "减",
    "增加": "增",
    "完善": "补",
    "优化": "调",
    "调整": "调",
    "改革": "改",
    "创新": "创",
    "发展": "展",
    "增长": "涨",
    "下降": "降",
    "上升": "升",
    "扩大": "扩",
    "缩小": "缩",
    "拓展": "拓",
    "延伸": "延",
    "深化": "深",
    "细化": "细",
    "强化": "强",
    "弱化": "弱",
    "简化": "简",
    "落实": "落",
    "执行": "行",
    "贯彻": "贯",
    "部署": "布",
    "安排": "排",
    "统筹": "统",
    "协调": "调",
    "联动": "联",
    "协同": "协",
    "配合": "配",
    "合作": "合",
    "对接": "接",
    "衔接": "衔",
    "打通": "通",
    "疏通": "疏",
    "清理": "清",
    "整顿": "整",
    "整治": "治",
    "规范": "规",
    "监管": "管",
    "监督": "督",
    "检查": "查",
    "考核": "考",
    "评估": "评",
    "评价": "评",
    "反馈": "馈",
    "响应": "应",
    "回复": "复",
    "上报": "报",
    "下达": "达",
    "传达": "传",
    "通报": "通",
    "通知": "知",
    "公告": "告",
    "发布": "发",
    "公布": "布",
    "公开": "开",
    "透明": "明",
    "安全": "安",
    "稳定": "稳",
    "保障": "保",
    "维护": "护",
    "保护": "护",
    "服务": "服",
    "支撑": "撑",
    "支持": "支",
    "助力": "助",
    "赋能": "帮",
    "聚焦": "盯",
    "深耕": "扎",
    "打造": "建",
    "引领": "领",
    "覆盖": "盖",
    "渗透": "入",
    "挖掘": "挖",
    "梳理": "理",
    "输出": "出",
    "迭代": "更",
    "升级": "升",
    "交付": "交",
    "复盘": "结",
    "同步": "说",
    "对齐": "统",
    "对标": "比",
    "跟进": "盯",
    "倒逼": "逼",
    "包装": "吹",
    "联动": "合",
    "发力": "干",
    "量化": "算",
    "细分": "拆",
    "重塑": "改",
    "蓄能": "攒",
    "引爆": "火",
}

# 线性衔接词（共100个，AI最常用的过渡衔接词，覆盖所有检测词）
LINEAR_CONNECTORS = [
    "此外", "另外", "与此同时", "值得注意的是", "需要指出的是",
    "不难发现", "显而易见", "事实上", "实际上", "换言之",
    "也就是说", "换句话说", "综上所述", "总之", "总而言之",
    "因此", "所以", "故而", "由此可见", "首先", "其次", "最后",
    "不仅如此", "除此之外", "更为重要的是", "无独有偶", "更重要的是",
    "需要强调的是", "不可否认的是", "值得一提的是", "总的来说", "概而言之",
    "平心而论", "客观来讲", "主观来说", "从某种意义上说", "在某种程度上",
    "从长远来看", "从短期来看", "从整体上看", "从局部来看", "从宏观层面",
    "从微观层面", "具体而言", "大体而言", "一般而言", "通常来说",
    "一般来说", "总的来看", "不难看出", "可以看出", "由此观之",
    "据此来看", "基于此", "有鉴于此", "概而论之", "一言以蔽之",
    "换句话讲", "也就是说", "第一", "第二", "第三", "第四",
    "其一", "其二", "其三", "其四", "一来", "二来", "三来",
    "一方面", "另一方面", "不仅", "而且", "不但", "反而",
    "然而", "但是", "不过", "只是", "当然", "其实", "毕竟",
    "终究", "终归", "到底", "究竟", "其实不然", "事实并非如此",
    "值得关注的是", "需要关注的是", "需要警惕的是", "值得警惕的是",
    "值得思考的是", "值得反思的是", "值得总结的是", "值得回顾的是",
    "值得展望的是", "从这个角度看", "从这个层面看", "从这个维度看",
    "从这个视角看", "从这个方面看"
]

# 模板化表达
TEMPLATE_EXPRESSIONS = [
    (r"随着(.{2,15})的(.{2,8})发展", r"在\1不断\2的过程中"),
    (r"在(.{2,15})的背景下", r"面对\1的情况"),
    (r"从(.{2,10})的角度", r"站在\1的立场上"),
    (r"在(.{2,10})方面", r"就\1而言"),
    (r"基于(.{2,15})的考虑", r"出于\1的需要"),
    (r"本研究", "本文"),
    (r"本文认为", "我觉得"),
    (r"研究表明", "我看下来"),
]


def count_chars(text):
    """统计中文字符数"""
    return len(re.findall(r'[一-鿿]', text))


def replace_high_freq_words(text):
    """替换所有高频词，不再只换几个！"""
    # 按词长倒序排序，避免短词匹配错误
    words = sorted(WORD_REPLACEMENTS.keys(), key=len, reverse=True)
    for word in words:
        if word not in text:
            continue
        # 把所有匹配到的都替换掉，随机选替换词
        while word in text:
            alternatives = WORD_REPLACEMENTS[word]
            replacement = random.choice(alternatives)
            text = text.replace(word, replacement, 1)
    return text


def replace_double_to_single(text, frequency=1000):
    """双音词→单音词替换，控制频率"""
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


def remove_linear_connectors(text, frequency=500):
    """删除线性衔接词，控制频率"""
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


def perturb_sentence_structure(text, intensity="medium"):
    """句式扰动，不要过度拆分"""
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
        # 只拆分特别长的句子，概率调低
        if len(sent) > 70 and random.random() < 0.2:
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


def break_paragraph_uniformity(paragraphs, frequency=500):
    """打破段落均匀性，控制强度"""
    if len(paragraphs) < 3:
        return paragraphs
    result = list(paragraphs)
    chars_count = sum(count_chars(p) for p in paragraphs)
    adjust_count = max(1, chars_count // frequency)
    for _ in range(adjust_count):
        if len(result) < 2:
            break
        strategy = random.choice(['merge', 'split'])  # 去掉shorten，不要乱删内容
        if strategy == 'merge' and len(result) >= 2:
            idx = random.randint(0, len(result) - 2)
            if not result[idx].startswith('#') and not result[idx + 1].startswith('#'):
                result[idx] = result[idx] + result[idx + 1]
                result.pop(idx + 1)
        elif strategy == 'split' and any(count_chars(p) > 150 for p in result):
            long_paras = [(i, p) for i, p in enumerate(result) if count_chars(p) > 150]
            if long_paras:
                idx, para = random.choice(long_paras)
                sentences = re.split(r'(?<=[。！？])', para)
                if len(sentences) >= 4:
                    mid = len(sentences) // 2
                    result[idx] = ''.join(sentences[:mid])
                    result.insert(idx + 1, ''.join(sentences[mid:]))
    return result


def remove_unfounded_adverbs(text, frequency=1000):
    """只删除AI常用的副词，保留人类常用的"""
    adverbs = [
        "显著", "明显", "深刻", "重大", "关键", "核心",
        "根本", "本质", "全面", "系统", "整体", "高效",
        "持续", "不断"
    ]
    chars_count = count_chars(text)
    remove_count = max(1, chars_count // frequency)
    removed = 0
    for adv in adverbs:
        if removed >= remove_count:
            break
        # 只删除夹在标点之间的副词
        pattern = rf'(?<=[，。！？\n]){{1}}{adv}(?=[，。！？\n]){{1}}'
        new_text = re.sub(pattern, '', text, count=1)
        if new_text != text:
            text = new_text
            removed += 1
    return text


def insert_author_viewpoint(text, frequency=1000):
    """插入作者观点，控制频率"""
    viewpoints = [
        "在我看来，",
        "就我个人的理解，",
        "从实际来看，",
        "据我所知，",
        "以我之见，",
    ]
    chars_count = count_chars(text)
    insert_count = max(1, chars_count // frequency)
    sentences = re.split(r'(?<=[。！？])', text)
    result = []
    inserted = 0
    for i, sent in enumerate(sentences):
        if inserted < insert_count and len(sent) > 30 and random.random() < 0.15:
            viewpoint = random.choice(viewpoints)
            result.append(viewpoint)
            inserted += 1
        result.append(sent)
    return ''.join(result)


def syntactic_perturbation(text, frequency=500):
    """句法结构扰动：倒装、打乱、省略、补充

    对主谓宾定状补等语法结构进行扰动，使句子更像人类写作
    """
    chars_count = count_chars(text)
    perturb_count = max(1, chars_count // frequency)
    sentences = re.split(r'(?<=[。！？])', text)
    result = []
    perturbed = 0

    # 口语化插入语
    oral_inserts = [
        "说实话，", "怎么说呢，", "其实吧，", "你看，", "这么说吧，",
        "简单来说，", "换个角度，", "老实讲，", "说白了，", "换句话说，",
    ]

    # 倒装标记词（宾语/补语提前的模式）
    inversion_patterns = [
        # "X是Y" → "Y，这就是X"
        (r'^(.{2,15})是(.{2,20})[，,]', r'\2，这是\1的'),
        # "X认为Y" → "Y，X是这么看的"
        (r'^(.{2,10})认为(.{5,30})[，,]', r'\2，\1是这么认为的'),
        # "X使Y" → "Y，源于X"
        (r'^(.{2,15})使(.{3,20})[，,]', r'\2，源于\1'),
    ]

    # 省略模式（删除冗余主语或连接词）
    omit_patterns = [
        # 省略"我们"、"本文"等开头
        (r'^(我们|本文|本研究|笔者)(认为|觉得|发现|指出)?[，,]?', ''),
        # 省略"因此"、"所以"等连接词开头
        (r'^(因此|所以|故而|由此可见)[，,]?', ''),
    ]

    for sent in sentences:
        if not sent.strip() or len(sent) < 15:
            result.append(sent)
            continue

        if perturbed < perturb_count and random.random() < 0.3:
            strategy = random.choice(['invert', 'omit', 'insert'])

            if strategy == 'invert':
                # 倒装：尝试调整语序
                for pattern, replacement in inversion_patterns:
                    new_sent = re.sub(pattern, replacement, sent, count=1)
                    if new_sent != sent:
                        sent = new_sent
                        perturbed += 1
                        break

            elif strategy == 'omit':
                # 省略：删除冗余词语
                for pattern, replacement in omit_patterns:
                    new_sent = re.sub(pattern, replacement, sent, count=1)
                    if new_sent != sent and len(new_sent) > 10:
                        sent = new_sent
                        perturbed += 1
                        break

            elif strategy == 'insert':
                # 补充：添加口语化插入语
                if len(sent) > 30 and '，' in sent:
                    parts = sent.split('，', 1)
                    if len(parts) == 2 and len(parts[0]) > 5:
                        insert = random.choice(oral_inserts)
                        sent = parts[0] + '，' + insert + parts[1]
                        perturbed += 1

        result.append(sent)

    return ''.join(result)


def remove_quotes_and_concretize(text, frequency=500):
    """删除双引号，并将所有内容进行具体化改写

    对双引号内的所有内容进行改写，无论是否抽象：
    1. 删除双引号
    2. 将抽象表述替换为具体表述
    3. 保留具体内容但用更具体的措辞
    """
    chars_count = count_chars(text)

    # 抽象概念 → 具体表述的映射（优先匹配）
    abstract_to_concrete = {
        "这个问题": "定价过高、服务不到位等问题",
        "这种情况": "用户流失、口碑下滑的情况",
        "这种方式": "打折促销、会员积分的方式",
        "这种方法": "数据分析、用户调研的方法",
        "这种现象": "恶性竞争、价格战的现象",
        "这种趋势": "数字化、智能化的趋势",
        "这种模式": "平台化、生态化的模式",
        "这种策略": "差异化、低成本的策略",
        "这种机制": "激励约束、反馈改进的机制",
        "这种体系": "标准化、规范化的体系",
        "这种理念": "以用户为中心、长期主义的理念",
        "这种思维": "数据驱动、结果导向的思维",
        "这个领域": "电商、教育、医疗等领域",
        "这个阶段": "初创期、成长期、成熟期",
        "这个过程": "需求分析、产品设计、开发测试的过程",
        "关键因素": "资金、人才、技术、市场等关键因素",
        "核心问题": "成本高、效率低、体验差等核心问题",
        "重要手段": "技术创新、模式创新、管理创新等重要手段",
        "有效途径": "降本增效、提升体验、拓展市场等有效途径",
    }

    # 通用抽象词替换映射（用于改写引号内容中的抽象词）
    generic_replacements = {
        "赋能": "支持帮助",
        "深耕": "长期专注",
        "聚焦": "重点关注",
        "打造": "建设发展",
        "引领": "带头推动",
        "闭环": "完整循环",
        "抓手": "切入点",
        "底层逻辑": "根本原因",
        "顶层设计": "整体规划",
        "落地": "实施执行",
        "沉淀": "积累总结",
        "对齐": "达成一致",
        "迭代": "更新改进",
        "优化": "改进完善",
        "复盘": "总结回顾",
        "梳理": "整理分析",
        "输出": "提供展示",
        "提炼": "提取总结",
        "协同": "配合协作",
        "联动": "配合联动",
        "打通": "连接贯通",
        "发力": "集中力量",
        "量化": "用数据衡量",
        "细分": "详细划分",
        "重塑": "重新构建",
        "洞察": "深入观察",
        "渗透": "深入影响",
        "辐射": "影响带动",
        "兜底": "保障支撑",
        "解耦": "分离独立",
        "复用": "重复利用",
        "集成": "整合组合",
        "拆解": "分解分析",
        "破圈": "突破范围",
        "破局": "打破困境",
        "触达": "接触影响",
        "赛道": "行业领域",
        "痛点": "问题难点",
        "私域": "自有渠道",
        "流量": "用户访问",
        "拉新": "获取新用户",
        "转化": "促成交易",
        "留存": "保持用户",
        "促活": "提升活跃",
        "获客": "获取客户",
        "激活": "唤醒激活",
        "裂变": "快速传播",
        "增长": "发展壮大",
        "壁垒": "竞争门槛",
        "下沉": "向下拓展",
        "矩阵": "多点布局",
        "生态": "环境体系",
        "心智": "认知印象",
        "打法": "方法策略",
        "风口": "发展机会",
        "红利": "发展机遇",
        "赋能": "支持帮助",
        "颗粒度": "细致程度",
        "势能": "积累优势",
        "体感": "实际感受",
        "感知": "感受认知",
        "调性": "风格特点",
        "战役": "重大活动",
        "合力": "共同力量",
        "心力": "精力意志",
        "基石": "基础根本",
        "基因": "本质特点",
        "因子": "影响因素",
        "模型": "模式框架",
        "通道": "渠道路径",
        "链路": "流程路径",
        "水位": "水平标准",
        "水准": "水平程度",
        "姿态": "态度立场",
        "卡点": "难点障碍",
        "卡位": "占据位置",
        "头部": "领先位置",
        "腰部": "中间位置",
        "爽点": "满意亮点",
        "痒点": "期待需求",
        "全域": "全部范围",
        "公域": "开放平台",
        "蓝海": "新兴市场",
        "红海": "竞争市场",
        "变量": "变化因素",
        "边界": "范围界限",
        "阵地": "位置领域",
        "高地": "优势位置",
        "洼地": "劣势位置",
        "革命": "重大变革",
        "变革": "改革创新",
        "内卷": "过度竞争",
        "圈层": "群体圈子",
        "环节": "步骤环节",
        "困局": "困难局面",
        "话术": "表达方式",
        "触点": "接触节点",
        "峰值": "最高点",
        "漏洞": "问题缺陷",
        "风险": "危险隐患",
        "瓶颈": "制约障碍",
        "策略": "方法对策",
        "价值": "作用意义",
        "成本": "投入代价",
        "深度": "深入程度",
        "口碑": "评价声誉",
        "指标": "衡量标准",
        "试点": "试验探索",
        "空白": "空缺领域",
    }

    # 具体化改写模板（用于短句改写）
    concretize_templates = {
        # 纯抽象词组 → 具体表述
        "叠合式叙事共生": "多种叙事在同一空间中相互融合的共生方式",
        "张力式戏剧共生": "通过对比反差产生戏剧效果的共生方式",
        "隔代式转译共生": "不同时代记忆通过转译实现连接的共生方式",
        "记忆叠层": "不同时期记忆在同一空间中层层叠加",
        "文化共生": "不同文化在同一空间中共存互融",
        "红色文旅": "以革命历史为主题的文旅项目",
        "汉文化": "汉代历史文化",
    }

    def concretize_content(content):
        """对引号内容进行具体化改写"""
        # 如果内容很短（<4字），可能是引用词，直接返回删除引号
        if len(content) <= 3:
            return content

        # 检查是否匹配具体化模板
        for key, value in concretize_templates.items():
            if key in content:
                content = content.replace(key, value)

        # 替换内容中的抽象词
        for abstract, concrete in generic_replacements.items():
            if abstract in content:
                content = content.replace(abstract, concrete)

        # 检查是否匹配抽象概念映射
        for key, value in abstract_to_concrete.items():
            if key in content:
                return value

        return content

    # 处理中文双引号
    def replace_chinese_quotes(match):
        content = match.group(1)
        return concretize_content(content)

    # 处理英文双引号
    def replace_english_quotes(match):
        content = match.group(1)
        return concretize_content(content)

    # 匹配中文双引号（支持多层嵌套）
    text = re.sub(r'[""](.*?)[""]', replace_chinese_quotes, text)
    # 匹配英文双引号
    text = re.sub(r'"(.*?)"', replace_english_quotes, text)

    return text


def paragraph_structure_jump(text, frequency=800):
    """段结构跳跃：补充论据、反向论证、视角转化

    通过打乱句子顺序、添加转折、插入反向论证等方式，
    使段落结构更有跳跃性，避免线性叙述
    """
    chars_count = count_chars(text)
    sentences = re.split(r'(?<=[。！？])', text)
    sentences = [s for s in sentences if s.strip()]

    if len(sentences) < 3:
        return text

    jump_count = max(1, chars_count // frequency)
    jumped = 0

    # 反向论证模板
    reverse_templates = [
        "但换个角度看，{point}",
        "当然也有人会说，{point}",
        "不过反过来看，{point}",
        "从另一个层面讲，{point}",
    ]

    # 抛出问题的模板
    question_templates = [
        "但问题是，{point}",
        "这就有意思了，{point}",
        "换个角度想，{point}",
        "有意思的是，{point}",
    ]

    result = []
    i = 0
    while i < len(sentences):
        sent = sentences[i]

        # 策略1：在结论性句子后插入反向论证
        if (jumped < jump_count and
            any(kw in sent for kw in ['因此', '所以', '总之', '可见', '证明', '说明']) and
            random.random() < 0.3):
            # 在结论前插入质疑
            template = random.choice(question_templates)
            point = sent[:20] + '...' if len(sent) > 20 else sent
            question = template.format(point=point)
            result.append(question)
            result.append(sent)
            jumped += 1

        # 策略2：打乱相邻句子顺序（模拟思维跳跃）
        elif (jumped < jump_count and
              i + 1 < len(sentences) and
              len(sent) > 20 and len(sentences[i+1]) > 20 and
              random.random() < 0.15):
            # 交换顺序
            result.append(sentences[i+1])
            result.append(sent)
            i += 2
            jumped += 1
            continue

        # 策略3：在段落中间插入视角转化
        elif (jumped < jump_count and
              i == len(sentences) // 2 and
              len(sentences) >= 4 and
              random.random() < 0.2):
            # 插入视角转化标记
            perspective_shifts = [
                "站在另一个角度看，",
                "反过来想，",
                "从用户的角度，",
                "如果不这样呢？",
            ]
            shift = random.choice(perspective_shifts)
            result.append(shift + sent)
            jumped += 1

        else:
            result.append(sent)

        i += 1

    return ''.join(result)


def process_paragraph(para, config):
    """处理单个段落"""
    if para.startswith('#') or para.startswith('|') or para.startswith('```'):
        return para
    if para.startswith('<!--') and '-->' in para:
        return para
    text = para

    # 第一步：先把所有高频词都换掉！这是核心！
    text = replace_high_freq_words(text)

    # 其他处理，用调整后的档位
    if config['double_single'] >= 3:
        text = replace_double_to_single(text, frequency=1000)
    else:
        text = replace_double_to_single(text, frequency=2000)

    if config['sentence_perturb'] >= 3:
        text = perturb_sentence_structure(text, intensity="medium")

    # 新功能1：句法结构扰动（倒装、打乱、省略、补充）
    if config.get('syntactic_perturb', 0) >= 3:
        text = syntactic_perturbation(text, frequency=500)
    elif config.get('syntactic_perturb', 0) >= 1:
        text = syntactic_perturbation(text, frequency=1000)

    # 新功能2：删除双引号并替换抽象内容
    if config.get('quote_concretize', 0) >= 3:
        text = remove_quotes_and_concretize(text, frequency=300)
    elif config.get('quote_concretize', 0) >= 1:
        text = remove_quotes_and_concretize(text, frequency=600)

    # 新功能3：段结构跳跃
    if config.get('structure_jump', 0) >= 3:
        text = paragraph_structure_jump(text, frequency=500)
    elif config.get('structure_jump', 0) >= 1:
        text = paragraph_structure_jump(text, frequency=1000)

    if config['linear_conn'] >= 3:
        text = remove_linear_connectors(text, frequency=500)
    else:
        text = remove_linear_connectors(text, frequency=1500)

    if config['template'] >= 3:
        text = apply_template_replacements(text, frequency=500)
    else:
        text = apply_template_replacements(text, frequency=1000)

    if config['adverb'] >= 3:
        text = remove_unfounded_adverbs(text, frequency=500)
    else:
        text = remove_unfounded_adverbs(text, frequency=1000)

    if config['viewpoint'] >= 3:
        text = insert_author_viewpoint(text, frequency=1000)
    else:
        text = insert_author_viewpoint(text, frequency=2000)

    return text


def process_document(input_file, output_file, config):
    """处理整个文档"""
    with open(input_file, 'r', encoding='utf-8') as f:
        content = f.read()
    paragraphs = content.split('\n\n')

    if config['uniformity'] >= 3:
        paragraphs = break_paragraph_uniformity(paragraphs, frequency=500)
    else:
        paragraphs = break_paragraph_uniformity(paragraphs, frequency=1000)

    processed = []
    for para in paragraphs:
        processed.append(process_paragraph(para, config))

    result = '\n\n'.join(processed)
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(result)
    return result


def main():
    if len(sys.argv) < 3:
        print("用法: python lower_aigc_custom.py <输入文件> <输出文件>")
        sys.exit(1)
    input_file = sys.argv[1]
    output_file = sys.argv[2]

    # 调整后的配置，全部用中档，不要最高档
    config = {
        'high_freq': 4,
        'double_single': 3,
        'sentence_perturb': 3,
        'linear_conn': 4,
        'paragraph_jump': 3,
        'uniformity': 4,
        'viewpoint': 3,
        'empty_term': 3,
        'adverb': 3,
        'template': 3,
        # 新增三个功能
        'syntactic_perturb': 3,  # 句法结构扰动：倒装、打乱、省略、补充
        'quote_concretize': 6,   # 删除双引号并替换抽象内容
        'structure_jump': 3,     # 段结构跳跃：补充论据、反向论证、视角转化
    }

    print(f"正在处理: {input_file}")
    print(f"输出文件: {output_file}")
    result = process_document(input_file, output_file, config)
    print(f"处理完成！共 {count_chars(result)} 字")
    print("提示：本次处理已替换所有AI高频词，高频词得分会大幅下降！")


if __name__ == "__main__":
    main()
