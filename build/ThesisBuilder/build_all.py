#!/usr/bin/env python3
"""Comprehensive replacement for paper 6 (老年人再就业权益保障研究)."""
import os

CS = r"D:\360MoveData\Users\ban\Desktop\school_about\word_about\sxuyear\build\ThesisBuilder\Program.cs"

with open(CS, 'r', encoding='utf-8') as f:
    cs = f.read()

# Helper: escape special Unicode chars for C# string literals
def esc(s):
    return s.replace('\\', '\\\\').replace('"', '\\"').replace('“', '\\u201c').replace('”', '\\u201d').replace('—', '\\u2014').replace('‘', '\\u2018').replace('’', '\\u2019').replace('–', '\\u2013')

def h1(t): return f'        b.Append(H.MakeH1("{esc(t)}"));'
def h2(t): return f'        b.Append(H.MakeH2("{esc(t)}"));'
def h3(t): return f'        b.Append(H.MakeH3("{esc(t)}"));'
def para(t): return f'        b.Append(H.MakeBodyPara("{esc(t)}"));'

# ============================================================
# PART 1: Replace basic info (output filename, abstracts, keywords, cover, etc.)
# ============================================================

# 1. Output filename
k = '新时代青年精神内耗的成因及化解路径.docx'
v = '积极老龄化视角下老年人再就业权益保障研究.docx'
assert k in cs, "Output filename not found!"
cs = cs.replace(k, v)

# 2. Chinese abstract - old is a single line in the C# code
k = '新时代背景下，社会加速转型、数智技术深度渗透与多元文化思潮交织，使青年群体普遍面临学业、就业、人际、发展等多重压力，精神内耗已从个体心理现象上升为具有普遍性的社会问题，对青年身心健康、价值塑造、人格完善与成长发展构成现实影响。为系统把握这一议题的研究脉络，本文以“新时代青年精神内耗”为核心对象，采用文献研究法、跨学科研究法与归纳总结法，对国内近五年相关期刊论文、学位论文及研究成果进行全面梳理与整合论述。文章首先界定精神内耗的核心内涵与主要特征，归纳青年精神内耗在心理情绪、行为选择、价值认知、社会适应等方面的现实表征；其次从社会环境、高校教育、数字媒介、家庭影响及青年自身五个维度，系统梳理学界关于精神内耗生成原因的主要观点；再次整合提炼价值引领、教育优化、媒介治理、社会支持、个体调适等化解路径；最后对现有研究成果进行评述，指出研究不足并展望未来方向。研究表明，青年精神内耗是外部压力传导与内在认知失衡共同作用的结果，其治理需要构建社会、学校、家庭、个人协同联动的综合体系。'
assert k in cs, "Chinese abstract not found!"
cn_abs1 = esc('当前我国社会经济持续高速高质量发展，人口老龄化现象加速演进已经成为社会发展的必然结果，我国老年人口规模不断扩大，也出现了不同需求的老年群体。诸多身体条件尚可、渴望重新工作的老年群体，在上一份工作终止后就业受阻，使得劳动力资源没能得到充分利用。促进老年人再就业是实施积极应对人口老龄化国家战略的重要途径之一。在人口老龄化持续加剧与延迟退休政策逐步落地的背景下，我国老年人口再就业规模不断扩大，已经成为劳动力市场和养老保障体系中的重要变量。研究老年人再就业权益保障，回应老年人权益保障的现实诉求，有利于实现“老有所为”，推动我国经济发展。')
cn_abs2 = esc('本文主要研究从积极老龄化角度出发，结合我国人口老龄化现状以及相关的就老龄化政策，分析了我国老年人再就业现状与影响老年人再就业的因素，分别是个人自身、家庭环境、社会市场、政策制度四种因素，指出了当前老年人再就业可能遇到的困境，立足社会现实与老龄化发展趋势，提出了老年人再就业的保障路径：从政策完善、权益维护、岗位优化、技能提升、社会引导等多方面构建完整保障体系。')
v_cn = cn_abs1 + '\\n\\n' + cn_abs2
cs = cs.replace(k, v_cn)

# Wait - there was a second paragraph abstraction appended. Let me check what's after the old abstract.
# The old code has just b.Append(H.MakeBodyPara(old_cn_abstract)) - a single call.
# But our paper has TWO paragraphs. We need to change the single call to two calls.

# Actually the base script approach won't work cleanly for splitting one para into two.
# Let me take a different approach - replace the whole block around the Chinese abstract.
# I'll find the specific section and replace it.

# Let me find the markers and do a larger replacement.
print("Step 1 done - basic replacements prepared")

# Actually, let me just take a completely different approach.
# Instead of doing piecemeal replacements, let me find-tag-replace the entire BuildDocument method.

# First let me save what we have so far and write the full replacement differently.
# The piecemeal approach is too fragile for this task.

print("Switching to block replacement approach...")
