#!/usr/bin/env python3
"""Replace all content in Program.cs for the paid short drama paper."""
import os

CS = r"D:\360MoveData\Users\ban\Desktop\school_about\word_about\sxuyear\build\ThesisBuilder\Program.cs"

with open(CS, 'r', encoding='utf-8') as f:
    cs = f.read()

# ── 1. Output filename ──
cs = cs.replace('短视频对大学生学习行为的影响研究', '付费短剧的爽感机制与审美异化分析')

# ── 2. Footnote functions ──
old_fns = '    public static string FN1() => "王兴超, 田芳芳. 大学生短视频成瘾与学业成绩的关系——学习投入的中介作用和学业自我效能感的调节作用[J]. 华南师范大学学报(社会科学版), 2025(1):12-20.";\n    public static string FN2() => "Li Y, Wang H. Effects of short-form video app addiction on academic anxiety and academic engagement: The mediating role of mindfulness[J]. Frontiers in Psychology, 2024, 15:1428813.";\n    public static string FN3() => "Zhang L, Liu M. The effect of short-form video addiction on undergraduates’ academic procrastination: a moderated mediation model[J]. Frontiers in Psychology, 2023, 14:1298361.";\n    public static string FN4() => "中国互联网络信息中心. 第55次中国互联网络发展状况统计报告[R]. 北京: 中国互联网络信息中心, 2025.";'

new_fns = '    public static string FN1() => "户付费观看意愿的影响研究——基于体验经济理论视角[J]．今传媒，2026，34(03)：118-122．";\n    public static string FN2() => "马吉英，陈浩．月流水破3.5亿，广西95后靠短剧逆袭成“霸总”[J]．中国企业家，2025，(10)：61-65．";\n    public static string FN7() => "秦振宇．网络微短剧受众消费行为影响因素研究[D]．导师：常青．浙江传媒学院，2025．";\n    public static string FN8() => "邱春香．感知收益与感知付出对微短剧用户付费意愿的影响研究[D]．导师：黄晓军．南昌大学，2025．";\n    public static string FN9() => "杨子怡．短剧好看 付费套路别太难看[N]．人民邮电，2024-11-22(002)．";\n    public static string FN10() => "黄洪涛，杨召奎．微短剧成文娱消费新风口，付费乱象待规范[N]．工人日报，2024-09-10(004)．";\n    public static string FN13() => "张恒．AI短剧违规治理“找答案”比“提问题”重要[N]．河南商报，2026-04-09(A05)．";'

cs = cs.replace(old_fns, new_fns)
print("1. Footnotes OK")

# ── 3. Chinese abstract ──
old_cn_abs = '随着短视频平台的快速发展，其对大学生群体的影响日益凸显。本研究探讨短视频对大学生学习行为的双重影响，结合相关调查数据，分析其在碎片化知识获取与注意力分散等方面的作用机制。研究发现，短视频既为大学生拓展了学习资源，也带来了学习投入不足等问题，需引导大学生合理使用短视频工具。'
new_cn_abs = '近年来，付费短剧凭借高强度的“爽感”机制迅速占领短视频市场，其以密集反转、极致人设与情绪刺激为核心的内容策略，精准契合当代观众碎片化、即时满足的观看需求。本文聚焦于“爽感”机制的建构逻辑，从叙事节奏、角色设定及情感动员三个维度剖析其如何通过程式化套路激发用户付费冲动。研究发现，“爽感”虽在短期内有效提升用户黏性与变现效率，却也导致内容同质化严重、价值导向偏移等问题，进而引发观众的审美疲劳与信任危机。尤其在AI技术加持下，大量低成本、高产出的模板化短剧进一步加剧了创作惰性，使短剧逐渐沦为“感官快消品”，背离影视艺术应有的审美深度与人文关怀。在此基础上，本文引入“审美异化”理论框架，探讨付费短剧在资本驱动下如何扭曲观众的审美判断与文化期待。当“霸总”“逆袭”“复仇”等套路成为流量密码，观众的审美标准被算法与商业逻辑重塑，真实情感体验让位于机械刺激，最终形成“越看越倦、倦而仍看”的矛盾消费心理。文章呼吁行业从“流量导向”转向“品质共生”，推动短剧从“爽感依赖”迈向“价值共鸣”，实现可持续的精品化发展路径。'

cs = cs.replace(old_cn_abs, new_cn_abs)
print("2. CN abstract OK")

# ── 4. Keywords ──
cs = cs.replace('短视频；大学生；学习行为；学习投入；注意力分散', '付费短剧；爽感机制；审美异化；内容同质化；精品化转型')
print("3. CN keywords OK")

# ── 5. English abstract - replace MakeAbstractBody call with inline ──
# Original: b.Append(H.MakeAbstractBody());
# Also need to make MakeEngKeyPara accept param
# Let's replace the MakeAbstractBody() method content and MakeEngKeyPara method

# First, replace MakeAbstractBody method body
old_en_method_text = 'With the rapid development of short-video platforms, their impact on college students has become increasingly prominent. This study explores the dual effects of short videos on college students’ learning behavior, and analyzes its mechanism in fragmented knowledge acquisition and attention dispersion based on relevant survey data. The study finds that short videos not only expand learning resources for college students, but also bring problems such as insufficient learning engagement. It is necessary to guide college students to use short-video tools rationally.'

new_en_text = 'In recent years, paid short dramas have rapidly occupied the short video market with their high-intensity "pleasure" mechanism. Their content strategy, centered on dense reversals, ultimate character designs, and emotional stimulation, accurately meets the fragmented and instantly satisfied viewing needs of contemporary audiences. This article focuses on the construction logic of the "pleasure" mechanism, analyzing how it stimulates users’ payment impulse through programmatic routines from three dimensions: narrative rhythm, character setting, and emotional mobilization. Research has found that although "pleasure" effectively increases user stickiness and monetization efficiency in the short term, it also leads to serious content homogenization, value orientation deviation, and other problems, which in turn trigger audience aesthetic fatigue and trust crisis. Especially with the support of AI technology, a large number of low-cost and high-yield template based short dramas have further exacerbated creative inertia, making short dramas gradually become "sensory fast-moving consumer goods", deviating from the aesthetic depth and humanistic care that film and television art should have. On this basis, this article introduces the theoretical framework of "aesthetic alienation" to explore how paid short dramas distort the audience’s aesthetic judgments and cultural expectations under the drive of capital. When tactics such as "boss", "counterattack", and "revenge" become traffic codes, the audience’s aesthetic standards are reshaped by algorithms and commercial logic, and the real emotional experience gives way to mechanical stimulation, ultimately forming a contradictory consumer psychology of "watching more and more, tired but still watching". The article calls on the industry to shift from "traffic orientation" to "quality symbiosis", promote the transition of short dramas from "pleasure dependence" to "value resonance", and achieve a sustainable and high-quality development path.'

cs = cs.replace(old_en_method_text, new_en_text)
print("4. EN abstract OK")

# ── 6. Update MakeEngKeyPara to accept parameter ──
old_enkw_method = '''    public static Paragraph MakeEngKeyPara()
    {
        return new Paragraph(MakeKeyPP(),
            new Run(new RunProperties(
                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman" },
                new Bold(), new FontSize { Val = SZ_SIHAO }, new FontSizeComplexScript { Val = SZ_SIHAO }),
                new Text("Keywords: ") { Space = SpaceProcessingModeValues.Preserve }),
            new Run(new RunProperties(
                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "SimSun" },
                new FontSize { Val = SZ_XIAOSI }, new FontSizeComplexScript { Val = SZ_XIAOSI }),
                new Text("Short-video platforms; College students; Learning behavior; Learning engagement; Attention dispersion") { Space = SpaceProcessingModeValues.Preserve }));
    }'''

new_enkw_method = '''    public static Paragraph MakeEngKeyPara(string keywords)
    {
        return new Paragraph(MakeKeyPP(),
            new Run(new RunProperties(
                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman" },
                new Bold(), new FontSize { Val = SZ_SIHAO }, new FontSizeComplexScript { Val = SZ_SIHAO }),
                new Text("Keywords: ") { Space = SpaceProcessingModeValues.Preserve }),
            new Run(new RunProperties(
                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "SimSun" },
                new FontSize { Val = SZ_XIAOSI }, new FontSizeComplexScript { Val = SZ_XIAOSI }),
                new Text(keywords) { Space = SpaceProcessingModeValues.Preserve }));
    }'''

cs = cs.replace(old_enkw_method, new_enkw_method)

# Update the call site
cs = cs.replace('b.Append(H.MakeEngKeyPara());', 'b.Append(H.MakeEngKeyPara("paid short drama; Pleasure mechanism; Aesthetic alienation; Content homogenization; Refined transformation"));')
print("5. EN keywords OK")

# ── 7. Cover table ──
old_cover_title = '短视频对大学生行为的影响研究'
cs = cs.replace(old_cover_title, '付费短剧的“爽感”机制与审美异化分析')

old_cover_en = 'Research on Short Videos’ Impact on College Students’ Behaviors'
cs = cs.replace(old_cover_en, 'Analysis of the "Pleasure" Mechanism and Aesthetic Alienation in Paid Short Dramas')
print("6. Cover OK")

# ── 8. Acknowledgment ──
old_ack = '本论文的完成得益于指导教师的悉心指导和同学们的帮助。在论文写作过程中，我学习了文献查阅和数据分析的方法，也认识到自身在学术研究方面的不足。今后将继续努力，不断提升自己的学术素养和研究能力。感谢所有在论文写作过程中给予我支持和帮助的人。'
new_ack = '本论文的顺利完成，离不开指导教师的悉心指导与同学们的帮助。在论文写作过程中，我深入学习了文本分析法、案例研究法等学术研究方法，对付费短剧产业的发展逻辑与审美问题有了更深刻的认识。同时也认识到自身在理论建构与批判性分析方面尚存在诸多不足。今后将继续努力，不断提升自身的学术素养与研究能力，为数字内容产业的健康发展贡献自己的思考。感谢所有在论文写作过程中给予我支持与帮助的人。'
cs = cs.replace(old_ack, new_ack)
print("7. Ack OK")

with open(CS, 'w', encoding='utf-8', newline='\r\n') as f:
    f.write(cs)
print("\n=== All base replacements done ===")
