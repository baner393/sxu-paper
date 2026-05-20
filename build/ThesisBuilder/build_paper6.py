#!/usr/bin/env python3
"""Generate paper 6 (Lao Qi Da quantifiers) - base changes."""
import os

CS = r"D:\360MoveData\Users\ban\Desktop\school_about\word_about\sxuyear\build\ThesisBuilder\Program.cs"

with open(CS, 'r', encoding='utf-8') as f:
    cs = f.read()

# 1. Output filename
cs = cs.replace('短视频对大学生学习行为的影响研究', '老乞大量词编排对汉语二语教学的启示')

# 2. Footnote functions - no footnotes needed (paper cites by author name, not [N])
old_fns = '    public static string FN1() => "王兴超, 田芳芳. 大学生短视频成瘾与学业成绩的关系——学习投入的中介作用和学业自我效能感的调节作用[J]. 华南师范大学学报(社会科学版), 2025(1):12-20.";\n    public static string FN2() => "Li Y, Wang H. Effects of short-form video app addiction on academic anxiety and academic engagement: The mediating role of mindfulness[J]. Frontiers in Psychology, 2024, 15:1428813.";\n    public static string FN3() => "Zhang L, Liu M. The effect of short-form video addiction on undergraduates’ academic procrastination: a moderated mediation model[J]. Frontiers in Psychology, 2023, 14:1298361.";\n    public static string FN4() => "中国互联网络信息中心. 第55次中国互联网络发展状况统计报告[R]. 北京: 中国互联网络信息中心, 2025.";'
new_fns = '    public static string FN1() => "";'

cs = cs.replace(old_fns, new_fns)

# 3. Chinese abstract
old_cn = '随着短视频平台的快速发展，其对大学生群体的影响日益凸显。本研究探讨短视频对大学生学习行为的双重影响，结合相关调查数据，分析其在碎片化知识获取与注意力分散等方面的作用机制。研究发现，短视频既为大学生拓展了学习资源，也带来了学习投入不足等问题，需引导大学生合理使用短视频工具。'
new_cn = '《老乞大》是元明清时期朝鲜半岛最重要的汉语会话教材，全书111则会话共使用量词141个，语言真实、场景丰富、教学理念超前，被誉为"迄今为止使用时间最长的对外汉语教材"。量词是汉语二语教学中的突出难点，而学界现有研究多集中于量词本体和汉语史考据，从教学编排角度对《老乞大》量词设计进行系统分析的研究尚属少见。本研究以《老乞大谚解》和《原本老乞大》为主要文本依据，采用定性与定量相结合的方法，对教材中的量词进行全面统计与分类，解析其"由易到难、难易相间"的梯度设计、"分散复现"的螺旋式强化机制以及量词教学与场景叙事的深度融合方式。在此基础上，将《老乞大》的量词编排方式与当代代表性汉语教材《汉语会话301句》进行对比分析，探讨历时教材对共时教学的借鉴价值。研究表明：《老乞大》的量词编排策略，尤其是分散复现的强化机制、量词教学与真实交际场景的融合路径，以及量词难易度与整体语篇节奏的协同安排，对当代汉语二语教材的量词编写仍具有切实的参考意义。'

cs = cs.replace(old_cn, new_cn)

# 4. Keywords
cs = cs.replace('短视频；大学生；学习行为；学习投入；注意力分散', '《老乞大》；量词编排；教材编写；对外汉语教学；汉语二语教学')

# 5. EN abstract
old_en = 'With the rapid development of short-video platforms, their impact on college students has become increasingly prominent. This study explores the dual effects of short videos on college students’ learning behavior, and analyzes its mechanism in fragmented knowledge acquisition and attention dispersion based on relevant survey data. The study finds that short videos not only expand learning resources for college students, but also bring problems such as insufficient learning engagement. It is necessary to guide college students to use short-video tools rationally.'
new_en = '"Lao Qi Da" is the most important Chinese conversation textbook on the Korean Peninsula in the Yuan, Ming and Qing Dynasties. There are 111 conversations in the whole book with a total of 141 quantifiers. The language is authentic, the scene is rich, and the teaching concept is advanced. It is known as "the longest-used Chinese textbook for foreigners so far". Quantifiers are a prominent difficulty in teaching Chinese as a second language, yet existing research has mostly focused on ontological studies of quantifiers and historical textual research on Chinese, with few systematic analyses of the design of quantifiers in "Lao Qi Da" from the perspective of pedagogical arrangement. This research takes "Lao Qi Da Yan Jie" and "Yuan Ben Lao Qi Da" as the main text basis, adopting a combination of qualitative and quantitative methods to conduct comprehensive statistics and classification of quantifiers in textbooks, and analyses their gradient design of "easy to difficult, difficult and easy", the spiral strengthening mechanism of "dispersed reproduction", and the deep integration of quantifier teaching and scene narrative. On this basis, the quantifier arrangement of "Lao Qi Da" is compared and analysed with the contemporary representative Chinese textbook "Chinese Conversation 301 Sentences", exploring the reference value of the textbooks for synchronic teaching. Research shows that the quantifier arrangement strategy of "Lao Qi Da", especially the strengthening mechanism of decentralised reproduction, the integration path of quantifier teaching and real communication scenarios, and the coordinated arrangement of quantifier difficulty and the overall discourse rhythm, still has practical reference significance for the quantifier preparation of contemporary Chinese second-language textbooks.'

cs = cs.replace(old_en, new_en)

# 6. EN keywords - update method def and call
old_enkw = 'Short-video platforms; College students; Learning behavior; Learning engagement; Attention dispersion'
new_enkw = '"Lao Qi Da"; quantifier arrangement; textbook preparation; teaching Chinese as a foreign language; teaching Chinese second language'

# Update the MakeEngKeyPara method body text
cs = cs.replace(old_enkw, new_enkw)

# Update method to accept param
old_enkw_method = '    public static Paragraph MakeEngKeyPara()\n    {\n        return new Paragraph(MakeKeyPP(),\n            new Run(new RunProperties(\n                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman" },\n                new Bold(), new FontSize { Val = SZ_SIHAO }, new FontSizeComplexScript { Val = SZ_SIHAO }),\n                new Text("Keywords: ") { Space = SpaceProcessingModeValues.Preserve }),\n            new Run(new RunProperties(\n                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "SimSun" },\n                new FontSize { Val = SZ_XIAOSI }, new FontSizeComplexScript { Val = SZ_XIAOSI }),\n                new Text("'
new_enkw_method2 = '") { Space = SpaceProcessingModeValues.Preserve }));\n    }'

# Find and replace
start_marker = cs.find('    public static Paragraph MakeEngKeyPara()')
end_marker = cs.find('    public static void SetupFootnotes', start_marker)
old_method = cs[start_marker:end_marker]

new_method = '    public static Paragraph MakeEngKeyPara(string keywords)\n    {\n        return new Paragraph(MakeKeyPP(),\n            new Run(new RunProperties(\n                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman" },\n                new Bold(), new FontSize { Val = SZ_SIHAO }, new FontSizeComplexScript { Val = SZ_SIHAO }),\n                new Text("Keywords: ") { Space = SpaceProcessingModeValues.Preserve }),\n            new Run(new RunProperties(\n                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "SimSun" },\n                new FontSize { Val = SZ_XIAOSI }, new FontSizeComplexScript { Val = SZ_XIAOSI }),\n                new Text(keywords) { Space = SpaceProcessingModeValues.Preserve }));\n    }\n\n'
cs = cs.replace(old_method, new_method)

# Update call site
cs = cs.replace('b.Append(H.MakeEngKeyPara());', 'b.Append(H.MakeEngKeyPara("\\"Lao Qi Da\\"; quantifier arrangement; textbook preparation; teaching Chinese as a foreign language; teaching Chinese second language"));')

# 7. Cover table - use embedded info from paper
old_cover = '''            ("中文题目", "短视频对大学生行为的影响研究", false),
            ("英文题目", "Research on Short Videos’ Impact on College Students’ Behaviors", true),
            ("姓名", "袁勋", false),
            ("学号", "202310010237", false),
            ("班级", "文化产业管理2班", false),
            ("专业", "文化产业管理", false),
            ("学院", "文化旅游与新闻艺术学院", false),
            ("指导教师", "李旭鹏 讲师", false),
            ("完成时间", "2026年5月5日", false),'''

new_cover = '''            ("中文题目", "《老乞大》量词编排对汉语二语教学的启示", false),
            ("英文题目", "Enlightenment of \\"Lao Qi Da\\" Quantifier Arrangement for the Teaching of Chinese Second Language", true),
            ("姓名", "姜如心", false),
            ("学号", "202315070107", false),
            ("班级", "2023汉语国际教育班", false),
            ("专业", "汉语国际教育", false),
            ("学院", "文化旅游与新闻艺术学院", false),
            ("指导教师", "郝美娟 副教授", false),
            ("完成时间", "2026年5月10日", false),'''

cs = cs.replace(old_cover, new_cover)

# 8. Acknowledgments
old_ack = '本论文的完成得益于指导教师的悉心指导和同学们的帮助。在论文写作过程中，我学习了文献查阅和数据分析的方法，也认识到自身在学术研究方面的不足。今后将继续努力，不断提升自己的学术素养和研究能力。感谢所有在论文写作过程中给予我支持和帮助的人。'
new_ack = '本论文的顺利完成，离不开指导教师的悉心指导与同学们的帮助。在论文写作过程中，我深入学习了文献研究法、定量分析法、定性分析法以及比较分析法等学术研究方法，对《老乞大》量词编排的教学规律及其对当代汉语二语教学的启示有了更深刻的认识。同时也认识到自身在语言教学理论研究方面尚存在诸多不足。今后将继续努力，不断提升自身的学术素养与研究能力，为汉语国际教育事业贡献自己的思考。感谢所有在论文写作过程中给予我支持与帮助的人。'

cs = cs.replace(old_ack, new_ack)

with open(CS, 'w', encoding='utf-8', newline='\r\n') as f:
    f.write(cs)
print("Base changes done!")
