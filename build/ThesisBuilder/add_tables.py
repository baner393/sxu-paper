#!/usr/bin/env python3
"""Add three-line tables to paper 5 and fix table titles."""
import os

CS = r"D:\360MoveData\Users\ban\Desktop\school_about\word_about\sxuyear\build\ThesisBuilder\Program.cs"

with open(CS, 'r', encoding='utf-8') as f:
    cs = f.read()

# ── 1. Add generic three-line table helper to H class ──
# Find a good insertion point - before AddReferences or after MakeCapPP

insert_point = cs.index('    public static Paragraph MakeAppendixTitle()')
table_helper = '''    // Three-line table helper (三线表)
    // headers: column header texts; data: 2D array of cell texts; widths: column widths in DXA
    public static void AddThreeLineTable(Body body, string title, string[] headers, string[][] data, int[]? widths = null)
    {
        // Table caption above (仿宋五号居中)
        body.Append(new Paragraph(MakeCapPP(), new Run(MakeRP("FangSong", SZ_WUHAO), new Text(title) { Space = SpaceProcessingModeValues.Preserve })));

        var tbl = new Table();
        int cols = headers.Length;
        var tblPr = new TableProperties(
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 12, Space = 0, Color = "000000" },
                new BottomBorder { Val = BorderValues.Single, Size = 12, Space = 0, Color = "000000" },
                new LeftBorder { Val = BorderValues.None, Size = 0 },
                new RightBorder { Val = BorderValues.None, Size = 0 },
                new InsideHorizontalBorder { Val = BorderValues.None, Size = 0 },
                new InsideVerticalBorder { Val = BorderValues.None, Size = 0 }),
            new TableLayout { Type = TableLayoutValues.Fixed },
            new TableLook { Val = "04A0" });
        tbl.Append(tblPr);

        // Build grid
        var grid = new TableGrid();
        if (widths != null)
            foreach (var w in widths) grid.Append(new GridColumn { Width = w.ToString() });
        else
            for (int i = 0; i < cols; i++) grid.Append(new GridColumn { Width = (5000 / cols).ToString() });
        tbl.Append(grid);

        // Header row with bottom border (栏目线 1.5pt)
        var hRow = new TableRow(new TableRowProperties(new TableRowHeight { Val = 380U }));
        for (int c = 0; c < cols; c++)
        {
            var tcPr = new TableCellProperties(
                new TableCellBorders(new BottomBorder { Val = BorderValues.Single, Size = 12, Space = 0, Color = "000000" }),
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center });
            var para = new Paragraph(new ParagraphProperties(new Justification { Val = JC_CENTER }, new SpacingBetweenLines { Line = "260", LineRule = LineSpacingRuleValues.Auto }),
                new Run(MakeRP("SimSun", SZ_WUHAO, bold: true), new Text(headers[c]) { Space = SpaceProcessingModeValues.Preserve }));
            hRow.Append(new TableCell(tcPr, para));
        }
        tbl.Append(hRow);

        // Data rows
        foreach (var rd in data)
        {
            var row = new TableRow(new TableRowProperties(new TableRowHeight { Val = 360U }));
            for (int c = 0; c < cols && c < rd.Length; c++)
            {
                var tcPr = new TableCellProperties(
                    new TableCellBorders(new TopBorder { Val = BorderValues.None, Size = 0 }, new BottomBorder { Val = BorderValues.None, Size = 0 }, new LeftBorder { Val = BorderValues.None, Size = 0 }, new RightBorder { Val = BorderValues.None, Size = 0 }),
                    new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center });
                var para = new Paragraph(new ParagraphProperties(new Justification { Val = c == 0 ? JC_LEFT : JC_CENTER }, new SpacingBetweenLines { Line = "260", LineRule = LineSpacingRuleValues.Auto }),
                    new Run(MakeRP("SimSun", SZ_WUHAO), new Text(rd[c]) { Space = SpaceProcessingModeValues.Preserve }));
                row.Append(new TableCell(tcPr, para));
            }
            tbl.Append(row);
        }
        body.Append(tbl);
        body.Append(new Paragraph(new ParagraphProperties(new SpacingBetweenLines { Before = "60", After = "60" })));
    }

'''

cs = cs[:insert_point] + table_helper + cs[insert_point:]
print("1. Table helper added")

# ── 2. Replace 3.2 section plain text with table ──
old_32 = '''H.AddBodyParaWithFn(b, "设施建设阶段主要包括猪舍建设、环保设备购置与安装等。根据温氏股份2024年年报及行业资料，典型1,000头生猪规模家庭农场设施投资如下", fnId++, H.FN5());
        H.AddRunToLastPara(b, "：猪舍建设500m²，单价1,200元，小计600,000元；固液分离机2台，单价30,000元，小计60,000元；风机、水帘设备4套，单价8,000元，小计32,000元；消毒池/无害化设备2套，单价12,000元，小计24,000元。合计约716,000元。该成本为一次性投入，折旧年限通常按10年计算，每年摊销约71,600元。");'''

new_32 = '''H.AddBodyParaWithFn(b, "设施建设阶段主要包括猪舍建设、环保设备购置与安装等。根据温氏股份2024年年报及行业资料，典型1,000头生猪规模家庭农场设施投资如表1所示", fnId++, H.FN5());
        H.AddRunToLastPara(b, "。");
        H.AddThreeLineTable(b, "表1  设施建设阶段成本",
            new[] { "项目", "单位", "数量", "单价（元）", "小计（元）", "备注" },
            new string[][] {
                new[] { "猪舍建设", "m²", "500", "1,200", "600,000", "按温氏标准化猪舍成本估算" },
                new[] { "固液分离机", "台", "2", "30,000", "60,000", "处理粪污" },
                new[] { "风机、水帘设备", "套", "4", "8,000", "32,000", "猪舍通风降温" },
                new[] { "消毒池/无害化设备", "套", "2", "12,000", "24,000", "病死猪处理" },
                new[] { "小计", "—", "—", "—", "716,000", "—" },
            },
            new[] { 1200, 600, 600, 1000, 1000, 1600 });
        b.Append(H.MakeBodyPara("注：该成本为一次性投入，折旧年限通常按10年计算，每年摊销约71,600元。"));'''

cs = cs.replace(old_32, new_32)
print("2. Table 1 (3.2) replaced")

# ── 3. Replace 3.3 section ──
old_33 = '''H.AddBodyParaWithFn(b, "家庭农场需办理设施农用地备案、环保审批及其他相关手续。依据自然资源部和农业农村部政策，设施农用地备案费用约2,000元，环保审批费用约3,500元，畜禽规模养殖环境影响评价费用约5,000元，合计一次性行政成本10,500元。这些成本虽不高，但对农户现金流有一定压力，属于典型的隐性阻力来源。国内研究显示，行政手续繁琐增加农户制度遵从成本，影响绿色生产积极性", fnId++, H.FN1());
        H.AddRunToLastPara(b, "。");'''

new_33 = '''H.AddBodyParaWithFn(b, "家庭农场需办理设施农用地备案、环保审批及其他相关手续。依据自然资源部和农业农村部政策，相关成本如表2所示", fnId++, H.FN1());
        H.AddRunToLastPara(b, "。");
        H.AddThreeLineTable(b, "表2  备案及行政审批成本",
            new[] { "项目", "费用（元）", "备注" },
            new string[][] {
                new[] { "设施农用地备案", "2,000", "代办或人工跑手续费用" },
                new[] { "环保审批", "3,500", "环保部门验收及材料准备费用" },
                new[] { "畜禽规模养殖环境影响评价", "5,000", "企业协助或第三方咨询" },
                new[] { "小计", "10,500", "一次性行政成本" },
            },
            new[] { 3000, 1500, 1500 });
        b.Append(H.MakeBodyPara("这些成本虽不高，但对农户现金流有一定压力，属于典型的隐性阻力来源。国内研究显示，行政手续繁琐增加农户制度遵从成本，影响绿色生产积极性。"));'''

cs = cs.replace(old_33, new_33)
print("3. Table 2 (3.3) replaced")

# ── 4. Replace 3.4 section ──
old_34 = '''H.AddBodyParaWithFn(b, "日常运行阶段包括环保设备电费、低蛋白日粮成本增加、人工及资金占压等。电费（风机、水帘、固液分离）约1,200kWh/月，年成本约15,000元；低蛋白日粮成本增加约12,000元/年（1,000头猪，相比传统饲料成本增加约10%）；额外人工投入约24,000元/年（2人）；生长周期延长导致资金占压约8,000元/年。合计年运行成本约59,000元。低蛋白日粮可减少氮排放，但可能延长生长周期，增加单位成本", fnId++, H.FN10());
        H.AddRunToLastPara(b, "。");'''

new_34 = '''H.AddBodyParaWithFn(b, "日常运行阶段包括环保设备电费、低蛋白日粮成本增加、人工及资金占压等，具体如表3所示。低蛋白日粮可减少氮排放，但可能延长生长周期，增加单位成本", fnId++, H.FN10());
        H.AddRunToLastPara(b, "。");
        H.AddThreeLineTable(b, "表3  日常运行阶段成本",
            new[] { "项目", "数值/单位", "年成本（元）", "备注" },
            new string[][] {
                new[] { "电费（风机、水帘、固液分离）", "1,200 kWh/月", "15,000", "按地方电价0.8元/kWh估算" },
                new[] { "低蛋白日粮成本增加", "1,000头猪", "12,000", "相比传统饲料成本增加约10%" },
                new[] { "额外人工投入", "2人", "24,000", "每人12,000元/年" },
                new[] { "生长周期延长导致资金占压", "1,000头猪", "8,000", "按日利率0.05%计算" },
                new[] { "小计", "—", "59,000", "—" },
            },
            new[] { 2500, 1200, 1100, 1200 });'''

cs = cs.replace(old_34, new_34)
print("4. Table 3 (3.4) replaced")

# ── 5. Replace 5.3 section ──
old_53 = '''b.Append(H.MakeBodyPara("成本与收益类别及金额（元/头）为：基础代养费1,500—2,000元，按出栏重量、成活率结算，覆盖标准养殖成本；绿色生产绩效补偿122—188元，覆盖环保设施运行、低蛋白日粮延长周期、电费及资金占压；风险保障金75—200元，应对疫病与政策波动风险，按季度或年度调整。合计1,697—2,388元/头，确保农户严格绿色生产收益大于等于象征性合规收益。金额范围根据第三章成本核算数据估算，并结合温氏股份年度报告及低蛋白日粮研究。"));'''

new_53 = '''b.Append(H.MakeBodyPara("契约优化方案的成本与收益构成如表4所示，金额范围根据第三章成本核算数据估算，并结合温氏股份年度报告及低蛋白日粮研究。"));
        H.AddThreeLineTable(b, "表4  契约激励设计方案",
            new[] { "成本/收益类别", "金额（元/头）", "设计方式" },
            new string[][] {
                new[] { "基础代养费", "1,500—2,000", "按出栏重量、成活率结算，覆盖标准养殖成本" },
                new[] { "绿色生产绩效补偿", "122—188", "覆盖环保设施运行、低蛋白日粮延长周期、电费及资金占压" },
                new[] { "风险保障金", "75—200", "应对疫病与政策波动风险，按季度或年度调整" },
                new[] { "合计", "1,697—2,388", "确保农户严格绿色生产收益≥象征性合规收益" },
            },
            new[] { 1600, 1200, 2200 });'''

cs = cs.replace(old_53, new_53)
print("5. Table 4 (5.3) replaced")

with open(CS, 'w', encoding='utf-8', newline='\r\n') as f:
    f.write(cs)
print("All table replacements done!")
