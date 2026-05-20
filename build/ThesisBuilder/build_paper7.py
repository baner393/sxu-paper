#!/usr/bin/env python3
"""Generate paper 7 (spiritual involution) - all changes."""
import os
CS = r"D:\360MoveData\Users\ban\Desktop\school_about\word_about\sxuyear\build\ThesisBuilder\Program.cs"
with open(CS, 'r', encoding='utf-8') as f:
    cs = f.read()

# ── 1. Output filename ──
cs = cs.replace('短视频对大学生学习行为的影响研究', '新时代青年精神内耗的成因及化解路径')

# ── 2. Footnotes ──
old_fns = '    public static string FN1() => "王兴超, 田芳芳. 大学生短视频成瘾与学业成绩的关系——学习投入的中介作用和学业自我效能感的调节作用[J]. 华南师范大学学报(社会科学版), 2025(1):12-20.";\n    public static string FN2() => "Li Y, Wang H. Effects of short-form video app addiction on academic anxiety and academic engagement: The mediating role of mindfulness[J]. Frontiers in Psychology, 2024, 15:1428813.";\n    public static string FN3() => "Zhang L, Liu M. The effect of short-form video addiction on undergraduates’ academic procrastination: a moderated mediation model[J]. Frontiers in Psychology, 2023, 14:1298361.";\n    public static string FN4() => "中国互联网络信息中心. 第55次中国互联网络发展状况统计报告[R]. 北京: 中国互联网络信息中心, 2025.";'

# Wait, let me find the actual old_fns in the file
idx = cs.index('    public static string FN1()')
idx2 = cs.index('    public static void AddPageBreak', idx)
actual_old_fns = cs[idx:idx2]

new_fns = '    public static string FN2() => "冯文博．从“焦虑”到“治愈”：数智时代青年精神内耗的表征、缘由与引导[J]．河北青年管理干部学院学报，2025,37(06)：20-26．";\n    public static string FN12() => "林崇德．发展心理学[M]．北京：人民教育出版社，2018．";\n\n'
cs = cs.replace(actual_old_fns, new_fns)

# ── 3. Chinese abstract ──
old_cn = '随着短视频平台的快速发展，其对大学生群体的影响日益凸显。本研究探讨短视频对大学生学习行为的双重影响，结合相关调查数据，分析其在碎片化知识获取与注意力分散等方面的作用机制。研究发现，短视频既为大学生拓展了学习资源，也带来了学习投入不足等问题，需引导大学生合理使用短视频工具。'
new_cn = '新时代背景下，社会加速转型、数智技术深度渗透与多元文化思潮交织，使青年群体普遍面临学业、就业、人际、发展等多重压力，精神内耗已从个体心理现象上升为具有普遍性的社会问题，对青年身心健康、价值塑造、人格完善与成长发展构成现实影响。为系统把握这一议题的研究脉络，本文以"新时代青年精神内耗"为核心对象，采用文献研究法、跨学科研究法与归纳总结法，对国内近五年相关期刊论文、学位论文及研究成果进行全面梳理与整合论述。文章首先界定精神内耗的核心内涵与主要特征，归纳青年精神内耗在心理情绪、行为选择、价值认知、社会适应等方面的现实表征；其次从社会环境、高校教育、数字媒介、家庭影响及青年自身五个维度，系统梳理学界关于精神内耗生成原因的主要观点；再次整合提炼价值引领、教育优化、媒介治理、社会支持、个体调适等化解路径；最后对现有研究成果进行评述，指出研究不足并展望未来方向。研究表明，青年精神内耗是外部压力传导与内在认知失衡共同作用的结果，其治理需要构建社会、学校、家庭、个人协同联动的综合体系。'
cs = cs.replace(old_cn, new_cn)

# ── 4. Keywords ──
cs = cs.replace('短视频；大学生；学习行为；学习投入；注意力分散', '新时代青年；精神内耗；成因；化解路径；思想政治教育')

# ── 5. EN abstract ──
old_en = 'With the rapid development of short-video platforms, their impact on college students has become increasingly prominent. This study explores the dual effects of short videos on college students’ learning behavior, and analyzes its mechanism in fragmented knowledge acquisition and attention dispersion based on relevant survey data. The study finds that short videos not only expand learning resources for college students, but also bring problems such as insufficient learning engagement. It is necessary to guide college students to use short-video tools rationally.'
new_en = 'In the new era, with accelerated social transformation, deep penetration of digital intelligence technology and intertwining of diverse cultural trends, young people are generally facing multiple pressures such as study, employment, interpersonal relationship and development. Spiritual involution has risen from an individual psychological phenomenon to a universal social problem. To systematically grasp the research context of this topic, this paper takes "youth spiritual involution in the new era" as the core object, adopts literature research, interdisciplinary research and induction methods, and comprehensively sorts out and integrates relevant research achievements in China in the past five years. Firstly, this paper defines the core connotation and main characteristics, and summarizes its manifestations in psychological emotion, behavior choice, value cognition and social adaptation. Secondly, it systematically sorts out academic views on the causes from five dimensions: social environment, university education, digital media, family influence and youth themselves. Thirdly, it integrates solutions such as value guidance, education optimization, media governance, social support and individual adjustment. Based on a systematic literature review, this paper further clarifies the research picture and provides literature support for university education practice and youth development policy.'
cs = cs.replace(old_en, new_en)

# ── 6. EN keywords ──
old_enkw = 'Short-video platforms; College students; Learning behavior; Learning engagement; Attention dispersion'
new_enkw = 'New Era Youth; Spiritual Involution; Causes; Solutions; Ideological and Political Education'
cs = cs.replace(old_enkw, new_enkw)

# ── 7. Update MakeEngKeyPara to accept param ──
start_mk = cs.find('    public static Paragraph MakeEngKeyPara()')
end_mk = cs.find('    public static void SetupFootnotes', start_mk)
old_method = cs[start_mk:end_mk]
new_method = '    public static Paragraph MakeEngKeyPara(string keywords)\n    {\n        return new Paragraph(MakeKeyPP(),\n            new Run(new RunProperties(\n                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman" },\n                new Bold(), new FontSize { Val = SZ_SIHAO }, new FontSizeComplexScript { Val = SZ_SIHAO }),\n                new Text("Keywords: ") { Space = SpaceProcessingModeValues.Preserve }),\n            new Run(new RunProperties(\n                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "SimSun" },\n                new FontSize { Val = SZ_XIAOSI }, new FontSizeComplexScript { Val = SZ_XIAOSI }),\n                new Text(keywords) { Space = SpaceProcessingModeValues.Preserve }));\n    }\n\n'
cs = cs.replace(old_method, new_method)
cs = cs.replace('b.Append(H.MakeEngKeyPara());', 'b.Append(H.MakeEngKeyPara("New Era Youth; Spiritual Involution; Causes; Solutions; Ideological and Political Education"));')

# ── 8. Cover table ──
old_cover_start = cs.find('            ("中文题目", "短视频对大学生行为的影响研究"')
old_cover_end = cs.find('        };\n\n        var rows = tbl.Elements', old_cover_start) + 10
old_cover = cs[old_cover_start:old_cover_end]

new_cover = '''            ("中文题目", "新时代青年精神内耗的成因及化解路径文献论述", false),
            ("英文题目", "On the Causes and Solutions of Youth Spiritual Involution in the New Era: A Literature Review", true),
            ("姓名", "高艺宁", false),
            ("学号", "202402030207", false),
            ("班级", "2024级应用统计学2班", false),
            ("专业", "应用统计学", false),
            ("学院", "统计学院", false),
            ("指导教师", "高宇钊 讲师", false),
            ("完成时间", "2026年5月16日", false),
        };

'''

cs = cs[:old_cover_start] + new_cover + cs[old_cover_end:]

# ── 9. Acknowledgements ──
old_ack = '本论文的完成得益于指导教师的悉心指导和同学们的帮助。在论文写作过程中，我学习了文献查阅和数据分析的方法，也认识到自身在学术研究方面的不足。今后将继续努力，不断提升自己的学术素养和研究能力。感谢所有在论文写作过程中给予我支持和帮助的人。'
new_ack = '本论文的顺利完成，离不开指导教师的悉心指导与同学们的帮助。在论文写作过程中，我深入学习了文献研究法、跨学科研究法与归纳总结法等学术研究方法，对新时代青年精神内耗的成因与化解路径这一议题有了更系统的认识。同时也认识到自身在理论分析与实证研究方面尚存在诸多不足。今后将继续努力，不断提升自身的学术素养与研究能力，为青年思想政治教育与心理健康教育贡献自己的力量。感谢所有在论文写作过程中给予我支持与帮助的人。'
cs = cs.replace(old_ack, new_ack)

with open(CS, 'w', encoding='utf-8', newline='\r\n') as f:
    f.write(cs)
print("Paper 7 base changes done!")
