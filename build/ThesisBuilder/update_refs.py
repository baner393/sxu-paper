#!/usr/bin/env python3
"""Replace AddReferences method in Program.cs"""
import os

CS = r"D:\360MoveData\Users\ban\Desktop\school_about\word_about\sxuyear\build\ThesisBuilder\Program.cs"

with open(CS, 'r', encoding='utf-8') as f:
    cs = f.read()

# Find AddReferences method
ref_start = cs.index('    public static void AddReferences(Body body)')
ref_end = cs.index('    public static Paragraph MakeAppendixTitle()')

old_refs = cs[ref_start:ref_end]

new_refs = '''    public static void AddReferences(Body body)
    {
        void AddRef(string text) { body.Append(new Paragraph(MakeRefPP(), new Run(MakeRP("SimSun", SZ_XIAOSI), new Text(text) { Space = SpaceProcessingModeValues.Preserve }))); }
        void AddRefItalic(string before, string title, string after)
        {
            var p = new Paragraph(MakeRefPP());
            if (before.Length > 0) p.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text(before) { Space = SpaceProcessingModeValues.Preserve }));
            p.Append(new Run(MakeRP("SimSun", SZ_XIAOSI, italic: true), new Text(title) { Space = SpaceProcessingModeValues.Preserve }));
            if (after.Length > 0) p.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text(after) { Space = SpaceProcessingModeValues.Preserve }));
            body.Append(p);
        }

        AddRef("[1] 户付费观看意愿的影响研究——基于体验经济理论视角[J]．今传媒，2026，34(03)：118-122．");
        AddRef("[2] 马吉英，陈浩．月流水破3.5亿，广西95后靠短剧逆袭成“霸总”[J]．中国企业家，2025，(10)：61-65．");
        AddRef("[3] 施乐乐，徐百灵．智能鸿沟下的老年人付费陷阱：微短剧市场的风险与防范[J]．视听，2025，(16)：2-5．");
        AddRef("[4] 张放．基于ELM模型的网络微短剧传播效果及影响因素研究[D]．导师：程前．江西师范大学，2025．");
        AddRef("[5] 程斯庆．心流理论视角下Z世代微短剧消费内容偏好与互动研究[D]．导师：侯迎忠．广东外语外贸大学，2025．");
        AddRef("[6] 朱潇．DTPB理论视角下“小程序剧”的用户付费意愿影响因素研究[D]．导师：杨吉．浙江传媒学院，2025．");
        AddRef("[7] 秦振宇．网络微短剧受众消费行为影响因素研究[D]．导师：常青．浙江传媒学院，2025．");
        AddRef("[8] 邱春香．感知收益与感知付出对微短剧用户付费意愿的影响研究[D]．导师：黄晓军．南昌大学，2025．");
        AddRef("[9] 杨子怡．短剧好看 付费套路别太难看[N]．人民邮电，2024-11-22(002)．");
        AddRef("[10] 黄洪涛，杨召奎．微短剧成文娱消费新风口，付费乱象待规范[N]．工人日报，2024-09-10(004)．");
        AddRef("[11] 龙思言．付费短剧迎“泼天富贵”，你掏腰包了吗[N]．三湘都市报，2024-02-27(A05)．Short Drama Marketing Strategies Under the New Media Context[J]．Proceedings of Business and Economic Studies，2026，9(3)：");
        AddRef("[12] 周莉．爱上微短剧无法自拔，该怎么办？[J]．民心，2026，(04)：56-57．");
        AddRef("[13] 张恒．AI短剧违规治理“找答案”比“提问题”重要[N]．河南商报，2026-04-09(A05)．");
        AddRefItalic("[14] ", "AdsDrama Debuts Short Drama Ad Platform", "[J]．Food and Beverage Close - Up，2026，");
        AddRefItalic("[15] ", "AdsDrama Introduces Short Drama Ad Platform", "[J]．Manufacturing Close - Up，2026，");
        AddRefItalic("[16] Ziru Li，Jingxue Zhao．", "From Short Shadow Plays to Shadow Puppet Shows：The Triple Media Forms and Narrative Transformation of Balinzuo Banner Shadow Puppetry", "[J]．Art and Design，2026，9(3)：");
        AddRefItalic("[17] ", "AdsDrama Introduces Short Drama Ad Platform", "[J]．Wireless News，2026，");
        AddRef("[18] 吴月玲，张田．微短剧：如何从“流量狂欢”转向品质坚守？[N]．中国艺术报，2026-02-09(001)．");
    }'''

cs = cs.replace(old_refs, new_refs)
print("References replaced")

with open(CS, 'w', encoding='utf-8', newline='\r\n') as f:
    f.write(cs)
print("Done")
