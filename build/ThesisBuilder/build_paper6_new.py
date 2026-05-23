#!/usr/bin/env python3
"""Generate paper 6: 老年人再就业权益保障研究."""
import os

CS = r"D:\360MoveData\Users\ban\Desktop\school_about\word_about\sxuyear\build\ThesisBuilder\Program.cs"

with open(CS, 'r', encoding='utf-8') as f:
    cs = f.read()

def esc(s):
    return s.replace('\\', '\\\\').replace('"', '\\"').replace('“', '\\u201c').replace('”', '\\u201d').replace('—', '\\u2014').replace('‘', '\\u2018').replace('’', '\\u2019').replace('–', '\\u2013')

# 1. Output filename
cs = cs.replace('新时代青年精神内耗的成因及化解路径.docx', '老年人再就业权益保障研究.docx')

# 2. Chinese abstract - replace the single para with two paras
old_cn = 'b.Append(H.MakeBodyPara("新时代背景下，社会加速转型、数智技术深度渗透与多元文化思潮交织，使青年群体普遍面临学业、就业、人际、发展等多重压力，精神内耗已从个体心理现象上升为具有普遍性的社会问题，对青年身心健康、价值塑造、人格完善与成长发展构成现实影响。为系统把握这一议题的研究脉络，本文以“新时代青年精神内耗”为核心对象，采用文献研究法、跨学科研究法与归纳总结法，对国内近五年相关期刊论文、学位论文及研究成果进行全面梳理与整合论述。文章首先界定精神内耗的核心内涵与主要特征，归纳青年精神内耗在心理情绪、行为选择、价值认知、社会适应等方面的现实表征；其次从社会环境、高校教育、数字媒介、家庭影响及青年自身五个维度，系统梳理学界关于精神内耗生成原因的主要观点；再次整合提炼价值引领、教育优化、媒介治理、社会支持、个体调适等化解路径；最后对现有研究成果进行评述，指出研究不足并展望未来方向。研究表明，青年精神内耗是外部压力传导与内在认知失衡共同作用的结果，其治理需要构建社会、学校、家庭、个人协同联动的综合体系。"));'
new_cn = 'b.Append(H.MakeBodyPara("' + esc('当前我国社会经济持续高速高质量发展，人口老龄化现象加速演进已经成为社会发展的必然结果，我国老年人口规模不断扩大，也出现了不同需求的老年群体。诸多身体条件尚可、渴望重新工作的老年群体，在上一份工作终止后就业受阻，使得劳动力资源没能得到充分利用。促进老年人再就业是实施积极应对人口老龄化国家战略的重要途径之一。在人口老龄化持续加剧与延迟退休政策逐步落地的背景下，我国老年人口再就业规模不断扩大，已经成为劳动力市场和养老保障体系中的重要变量。研究老年人再就业权益保障，回应老年人权益保障的现实诉求，有利于实现“老有所为”，推动我国经济发展。') + '"));\n'
new_cn += '        b.Append(H.MakeBodyPara("' + esc('本文主要研究从积极老龄化角度出发，结合我国人口老龄化现状以及相关的就老龄化政策，分析了我国老年人再就业现状与影响老年人再就业的因素，分别是个人自身、家庭环境、社会市场、政策制度四种因素，指出了当前老年人再就业可能遇到的困境，立足社会现实与老龄化发展趋势，提出了老年人再就业的保障路径：从政策完善、权益维护、岗位优化、技能提升、社会引导等多方面构建完整保障体系。') + '"));'
cs = cs.replace(old_cn, new_cn)

# 3. Keywords
cs = cs.replace('新时代青年；精神内耗；成因；化解路径；思想政治教育', '积极老龄化；社会保障；老年人再就业；劳动权益保障')

# 4. English abstract
old_en = 'b.Append(H.MakeAbstractBody());'
new_en = 'b.Append(H.MakeBodyPara("' + esc('Currently, China\'s socioeconomic development is characterized by sustained high-speed and high-quality growth. The accelerating phenomenon of population aging has become an inevitable outcome of social development. The size of China\'s elderly population has continued to expand, and different needs among the elderly have emerged. Many elderly individuals with relatively good physical conditions who yearn to return to work find it difficult to find employment after their previous job ended, resulting in a lack of effective utilization of labor resources. Promoting the reemployment of the elderly is one of the key approaches to implementing a national strategy that actively addresses population aging. Against the backdrop of continued acceleration in population aging and the gradual implementation of delayed retirement policies, the reemployment of China\'s elderly population has become a significant variable in the labor market and the pension security system. Studying the protection of the rights and interests of the elderly in relation to their reemployment and responding to the practical demands of safeguarding their rights and interests is essential for achieving "active aging" and driving China\'s economic development.') + '"));\n'
new_en += '        b.Append(H.MakeBodyPara("' + esc('This article primarily examines the issue of reemployment among the elderly from the perspective of active aging, taking into account the current state of China\'s population aging and related policies on aging. It analyzes the current situation of reemployment among the elderly in China and the factors influencing their reemployment, which include personal factors, family environment, social market, and policy systems. It points out the difficulties that elderly individuals may face in reemployment today and proposes a comprehensive support path for their reemployment, based on social realities and the trends of population aging. This path involves the establishment of a complete support system through policy improvement, rights protection, job optimization, skill enhancement, and social guidance.') + '"));'
cs = cs.replace(old_en, new_en)

# 5. English keywords
cs = cs.replace('New Era Youth; Spiritual Involution; Causes; Solutions; Ideological and Political Education', 'Positive aging; social security; reemployment of the elderly; protection of labor rights')

# 6. Cover table data
old_cover = '''            ("中文题目", "新时代青年精神内耗的成因及化解路径文献论述", false),
            ("英文题目", "On the Causes and Solutions of Youth Spiritual Involution in the New Era: A Literature Review", true),
            ("姓名", "高艺宁", false),
            ("学号", "202402030207", false),
            ("班级", "2024级应用统计学2班", false),
            ("专业", "应用统计学", false),
            ("学院", "统计学院", false),
            ("指导教师", "高宇钊 讲师", false),
            ("完成时间", "2026年5月16日", false),'''
new_cover = '''            ("中文题目", "老年人再就业权益保障研究", false),
            ("英文题目", "Research on the Protection of Employment Rights of the Elderly", true),
            ("姓名", "谢玉章", false),
            ("学号", "202411020138", false),
            ("班级", "劳动与社会保障班", false),
            ("专业", "劳动与社会保障", false),
            ("学院", "公共管理学院", false),
            ("指导教师", "刘春荣 教授", false),
            ("完成时间", "2026年5月", false),'''
cs = cs.replace(old_cover, new_cover)

# 7. Acknowledgments
old_ack = '本论文的顺利完成，离不开指导教师的悉心指导与同学们的帮助。在论文写作过程中，我深入学习了文献研究法、跨学科研究法与归纳总结法等学术研究方法，对新时代青年精神内耗的成因与化解路径这一议题有了更系统的认识。同时也认识到自身在理论分析与实证研究方面尚存在诸多不足。今后将继续努力，不断提升自身的学术素养与研究能力，为青年思想政治教育与心理健康教育贡献自己的力量。感谢所有在论文写作过程中给予我支持与帮助的人。'
new_ack = '本论文的顺利完成，离不开指导教师刘春荣教授的悉心指导与帮助。在论文写作过程中，我系统学习了文献研究法等学术研究方法，对积极老龄化视角下老年人再就业权益保障问题有了更深入的认识。同时也认识到自身在学术研究方面尚存在诸多不足，今后将继续努力提升研究能力。感谢所有在论文写作过程中给予我支持与帮助的人。'
cs = cs.replace(old_ack, new_ack)

with open(CS, 'w', encoding='utf-8', newline='\r\n') as f:
    f.write(cs)
print("Basic replacements done!")
