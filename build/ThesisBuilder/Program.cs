using System;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

internal class Program
{
    static string BASE = @"D:\360MoveData\Users\ban\Desktop\school_about\word_about\sxuyear";
    static string OUTPUT;
    static MainDocumentPart? mainPart;
    static Body? body;
    static FootnotesPart? fnPart;
    static int fnId = 1;

    static void Main(string[] args)
    {
        OUTPUT = Path.Combine(BASE, "output", $"短视频对大学生学习行为的影响研究.docx");
        var outDir = Path.GetDirectoryName(OUTPUT);
        if (outDir != null) Directory.CreateDirectory(outDir);

        using var doc = WordprocessingDocument.Create(OUTPUT, WordprocessingDocumentType.Document);
        mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(new Body());
        body = mainPart.Document.Body!;

        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        stylesPart.Styles = new Styles(H.MakeDocDefaults());
        stylesPart.Styles.Save();

        // Enable "为尾部空格添加下划线" (underline trailing spaces)
        var settingsPart = mainPart.AddNewPart<DocumentSettingsPart>();
        using (var sw = new StreamWriter(settingsPart.GetStream(FileMode.Create)))
        {
            sw.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:settings xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">
  <w:compat><w:ulTrailSpace/></w:compat>
</w:settings>");
        }

        fnPart = mainPart.AddNewPart<FootnotesPart>();
        fnPart.Footnotes = new Footnotes();
        H.SetupFootnotes(fnPart);

        BuildDocument();

        // Insert template cover and back cover
        string templatePath = Path.Combine(BASE, "template", "附件4：本科学年论文封面、说明、承诺使用页、封底示例 .docx");
        H.MergeTemplate(doc, mainPart, body!, templatePath);

        fnPart.Footnotes!.Save();
        H.AddComments(mainPart, body!);
        mainPart.Document.Save();
        Console.WriteLine("DONE: " + OUTPUT);
    }

    static void BuildDocument()
    {
        var b = body!;
        var mp = mainPart!;

        var hdrRoman = H.MakeHeader(mp, "山西财经大学2026级本科生学年论文", H.JC_CENTER);
        var hdrRomanId = mp.GetIdOfPart(hdrRoman);
        var ftrRoman = H.MakeFooter(mp, H.JC_CENTER);
        var ftrRomanId = mp.GetIdOfPart(ftrRoman);

        var hdrOdd = H.MakeHeader(mp, "山西财经大学2026级本科生学年论文", H.JC_RIGHT);
        var hdrOddId = mp.GetIdOfPart(hdrOdd);
        var hdrEven = H.MakeHeader(mp, "山西财经大学2026级本科生学年论文", H.JC_LEFT);
        var hdrEvenId = mp.GetIdOfPart(hdrEven);
        var ftrOdd = H.MakeFooter(mp, H.JC_RIGHT);
        var ftrOddId = mp.GetIdOfPart(ftrOdd);
        var ftrEven = H.MakeFooter(mp, H.JC_LEFT);
        var ftrEvenId = mp.GetIdOfPart(ftrEven);

        // SECTION 1: Cover + Explanation + Academic Pledge
        H.AddBlankPage(b); H.AddBlankPage(b); H.AddBlankPage(b);
        H.CloseSection(b, H.MakeSectPr(null, null, SectionMarkValues.OddPage, null, null));

        // SECTION 2: Chinese Abstract + English Abstract + TOC (merged, page breaks between them)
        b.Append(H.MakeCenteredTitle("摘  要", "SimHei", "36", "360", "360"));
        b.Append(H.MakeBodyPara("随着短视频平台的快速发展，其对大学生群体的影响日益凸显。本研究探讨短视频对大学生学习行为的双重影响，结合相关调查数据，分析其在碎片化知识获取与注意力分散等方面的作用机制。研究发现，短视频既为大学生拓展了学习资源，也带来了学习投入不足等问题，需引导大学生合理使用短视频工具。"));
        b.Append(H.MakeKeyPara(true, "短视频；大学生；学习行为；学习投入；注意力分散"));
        H.AddPageBreak(b);
        b.Append(H.MakeAbstractTitle());
        b.Append(H.MakeAbstractBody());
        b.Append(H.MakeEngKeyPara());
        H.AddPageBreak(b);
        b.Append(H.MakeCenteredTitle("目  录", "SimSun", "36", "360", "360"));
        H.AddTOC(b);
        H.CloseSection(b, H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.OddPage, NumberFormatValues.UpperRoman, 1));

        // SECTION 3: Body
        b.Append(H.MakeH1("导  论"));
        H.AddBodyParaWithFn(b, "近年来，我国互联网用户规模持续增长，短视频作为新兴的媒介形式，已经成为大学生日常信息获取与娱乐的重要渠道", fnId++, H.FN4());
        H.AddBodyParaWithFn(b, "。根据中国互联网络信息中心的统计数据，截至2025年6月，我国短视频用户规模已达10.8亿，其中18-24岁的大学生群体占比超过15%", fnId++, H.FN4());
        H.AddRunToLastPara(b, "。短视频的快速普及，深刻改变了大学生的信息接收习惯，也对其学习行为产生了复杂的影响。");
        b.Append(H.MakeBodyPara("既有研究多关注短视频的消极影响，如成瘾问题、学业拖延等，但对其积极作用的探讨相对不足，因此本研究从双重影响的视角展开分析，为引导大学生合理使用短视频提供参考。"));

        b.Append(H.MakeH1("1 短视频对大学生学习行为的双重影响"));
        b.Append(H.MakeH2("1.1 积极影响"));

        b.Append(H.MakeH3("1.1.1 碎片化知识获取"));
        H.AddBodyParaWithFn(b, "短视频的短时长特征，适配了大学生的碎片化时间，使得他们可以在课间、通勤等零散时间获取知识", fnId++, H.FN2());
        H.AddRunToLastPara(b, "。不同于传统的长视频课程，短视频将复杂的知识点拆解为1-3分钟的内容，降低了知识获取的门槛，提升了学习的灵活性。例如，很多大学生会通过短视频学习外语单词、实验操作技巧等内容，有效补充了课堂学习的不足。");

        b.Append(H.MakeH3("1.1.2 学习资源的拓展"));
        b.Append(H.MakeBodyPara("短视频平台汇聚了大量的优质学习资源，涵盖了各个学科领域，打破了传统教育的地域与资源壁垒。来自不同地区的教师、行业专家都可以在平台上分享知识，使得大学生可以接触到课堂之外的优质内容，拓宽了学习的视野。"));

        b.Append(H.MakeH2("1.2 消极影响"));

        b.Append(H.MakeH3("1.2.1 注意力分散问题"));
        H.AddBodyParaWithFn(b, "短视频平台的算法推荐机制，会根据用户的喜好不断推送感兴趣的内容，容易导致大学生陷入刷不停的状态，分散了学习的注意力", fnId++, H.FN1());
        H.AddRunToLastPara(b, "。研究显示，短视频成瘾的大学生，其注意力控制能力显著低于普通用户，难以长时间专注于深度学习任务");
        H.AddFnRefEnd(b, fnId++, H.FN3());
        H.AddRunToLastPara(b, "。");

        b.Append(H.MakeH3("1.2.2 学习投入不足"));
        H.AddBodyParaWithFn(b, "过度使用短视频会占用大学生的学习时间，导致学习投入水平下降。王兴超等的研究发现，短视频成瘾程度越高的大学生，其学习投入水平越低，进而影响了学业成绩", fnId++, H.FN1());
        H.AddRunToLastPara(b, "。很多大学生会在学习过程中频繁刷短视频，打断了学习的连续性，降低了学习的效率。");

        b.Append(H.MakeH1("2 大学生短视频使用与学习行为的现状"));
        H.AddBodyParaWithFn(b, "为了更直观地了解大学生的短视频使用情况，本研究整理了相关调查数据，如表1所示，短视频成瘾与学习投入、学业成绩均呈显著负相关，说明过度使用短视频会对学习产生负面作用", fnId++, H.FN1());
        H.AddRunToLastPara(b, "。");

        H.AddTable1(b);
        H.AddFigure1(b, BASE);

        H.AddBodyParaWithFn(b, "同时，从使用用途来看，大学生使用短视频的主要目的仍以娱乐为主，仅有约23%的用户将其用于知识学习，如图1所示", fnId++, H.FN4());
        H.AddRunToLastPara(b, "。这说明当前大学生对短视频的学习价值挖掘仍有不足，大部分时间仍用于娱乐消遣。");
        b.Append(H.MakeBodyPara("综上，短视频对大学生学习行为的影响是双重的，既带来了学习资源的拓展与碎片化学习的便利，也带来了注意力分散、学习投入不足等问题。高校与家庭应引导大学生合理规划短视频使用时间，充分发挥其积极作用，规避消极影响，帮助大学生更好地利用短视频工具提升学习效果。"));

        var s5sp = H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.OddPage, NumberFormatValues.Decimal, 1);
        s5sp.Append(new FootnoteProperties(
            new FootnotePosition { Val = FootnotePositionValues.PageBottom },
            new NumberingFormat { Val = NumberFormatValues.Decimal },
            new NumberingRestart { Val = RestartNumberValues.EachPage },
            new NumberingStart { Val = 1 }));
        H.CloseSection(b, s5sp);

        // SECTION 6: References
        b.Append(H.MakeH1("参考文献"));
        H.AddReferences(b);
        H.CloseSection(b, H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.OddPage, NumberFormatValues.Decimal, null));

        // SECTION 7: Appendix
        b.Append(H.MakeAppendixTitle());
        H.CloseSection(b, H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.OddPage, NumberFormatValues.Decimal, null));

        // SECTION 8: Acknowledgments
        b.Append(H.MakeAckTitle());
        b.Append(H.MakeBodyPara("本论文的完成得益于指导教师的悉心指导和同学们的帮助。在论文写作过程中，我学习了文献查阅和数据分析的方法，也认识到自身在学术研究方面的不足。今后将继续努力，不断提升自己的学术素养和研究能力。感谢所有在论文写作过程中给予我支持和帮助的人。"));
        H.CloseSection(b, H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.NextPage, NumberFormatValues.Decimal, null));

        // SECTION 9: Back cover + Grade table (final, sectPr in body)
        H.AddBlankPage(b); H.AddBlankPage(b);
        b.Append(H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, null, NumberFormatValues.Decimal, null));
    }
}

internal static class H
{
    public const string SZ_XIAOER = "36";
    public const string SZ_SANHAO = "32";
    public const string SZ_SIHAO = "28";
    public const string SZ_XIAOSI = "24";
    public const string SZ_WUHAO = "21";
    public const string SZ_XIAOWU = "18";
    public const string LINE_125 = "300";
    public const string SP_H1 = "320";
    public const string SP_XIAOER = "360";
    const uint MG_TOP = 1701;
    const uint MG_BOTTOM = 1418;
    const uint MG_LEFT = 1418;
    const uint MG_RIGHT = 1134;
    const uint PG_W = 11906;
    const uint PG_H = 16838;
    public static readonly JustificationValues JC_CENTER = JustificationValues.Center;
    public static readonly JustificationValues JC_LEFT = JustificationValues.Left;
    public static readonly JustificationValues JC_RIGHT = JustificationValues.Right;

    static RunProperties MakeRP(string ea, string sz, bool bold = false, bool italic = false)
    {
        var rp = new RunProperties(
            new RunFonts { EastAsia = ea, Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
            new FontSize { Val = sz }, new FontSizeComplexScript { Val = sz });
        if (bold) rp.Append(new Bold());
        if (italic) rp.Append(new Italic());
        return rp;
    }

    static RunProperties MakeSupRP()
    {
        return new RunProperties(
            new VerticalTextAlignment { Val = VerticalPositionValues.Superscript },
            new FontSize { Val = SZ_XIAOWU }, new FontSizeComplexScript { Val = SZ_XIAOWU });
    }

    static ParagraphProperties MakeBodyPP()
    {
        return new ParagraphProperties(
            new Indentation { FirstLineChars = 200 },
            new SpacingBetweenLines { Line = LINE_125, LineRule = LineSpacingRuleValues.Auto });
    }

    static ParagraphProperties MakeH1PP()
    {
        return new ParagraphProperties(
            new Justification { Val = JC_CENTER },
            new SpacingBetweenLines { Before = SP_H1, After = SP_H1, Line = LINE_125, LineRule = LineSpacingRuleValues.Auto },
            new OutlineLevel { Val = 0 });
    }

    static ParagraphProperties MakeH2PP()
    {
        return new ParagraphProperties(
            new Justification { Val = JC_LEFT },
            new SpacingBetweenLines { Line = LINE_125, LineRule = LineSpacingRuleValues.Auto },
            new OutlineLevel { Val = 1 });
    }

    static ParagraphProperties MakeH3PP()
    {
        return new ParagraphProperties(
            new Justification { Val = JC_LEFT },
            new SpacingBetweenLines { Line = LINE_125, LineRule = LineSpacingRuleValues.Auto },
            new OutlineLevel { Val = 2 });
    }

    static ParagraphProperties MakeTitlePP(string before, string after)
    {
        return new ParagraphProperties(
            new Justification { Val = JC_CENTER },
            new SpacingBetweenLines { Before = before, After = after, Line = LINE_125, LineRule = LineSpacingRuleValues.Auto });
    }

    static ParagraphProperties MakeKeyPP()
    {
        return new ParagraphProperties(
            new SpacingBetweenLines { Before = "240", Line = LINE_125, LineRule = LineSpacingRuleValues.Auto });
    }

    static ParagraphProperties MakeRefPP()
    {
        return new ParagraphProperties(
            new Indentation { Hanging = "480" },
            new SpacingBetweenLines { Line = LINE_125, LineRule = LineSpacingRuleValues.Auto });
    }

    static ParagraphProperties MakeCapPP(string after = "120")
    {
        return new ParagraphProperties(
            new Justification { Val = JC_CENTER }, new SpacingBetweenLines { After = after });
    }

    static PageMargin MakeMargin()
    {
        return new PageMargin { Top = (int)MG_TOP, Right = MG_RIGHT, Bottom = (int)MG_BOTTOM, Left = MG_LEFT, Header = 720U, Footer = 720U, Gutter = 0U };
    }

    static PageSize MakePageSize() { return new PageSize { Width = PG_W, Height = PG_H }; }

    public static Paragraph MakeCenteredTitle(string t, string f, string sz, string b4, string af)
    {
        return new Paragraph(MakeTitlePP(b4, af), new Run(MakeRP(f, sz, bold: true), new Text(t) { Space = SpaceProcessingModeValues.Preserve }));
    }

    public static Paragraph MakeBodyPara(string t)
    {
        return new Paragraph(MakeBodyPP(), new Run(MakeRP("SimSun", SZ_XIAOSI), new Text(t) { Space = SpaceProcessingModeValues.Preserve }));
    }

    public static Paragraph MakeH1(string t)
    {
        return new Paragraph(MakeH1PP(), new Run(MakeRP("SimHei", SZ_SANHAO), new Text(t) { Space = SpaceProcessingModeValues.Preserve }));
    }

    public static Paragraph MakeH2(string t)
    {
        return new Paragraph(MakeH2PP(), new Run(MakeRP("SimSun", SZ_SIHAO, bold: true), new Text(t) { Space = SpaceProcessingModeValues.Preserve }));
    }

    public static Paragraph MakeH3(string t)
    {
        return new Paragraph(MakeH3PP(), new Run(MakeRP("SimSun", SZ_XIAOSI, bold: true), new Text(t) { Space = SpaceProcessingModeValues.Preserve }));
    }

    public static Paragraph MakeKeyPara(bool cn, string kw)
    {
        if (cn)
            return new Paragraph(MakeKeyPP(),
                new Run(MakeRP("SimHei", SZ_SIHAO, bold: true), new Text("关键词：") { Space = SpaceProcessingModeValues.Preserve }),
                new Run(MakeRP("SimSun", SZ_XIAOSI), new Text(kw) { Space = SpaceProcessingModeValues.Preserve }));
        return new Paragraph();
    }

    public static Paragraph MakeAbstractTitle()
    {
        return new Paragraph(MakeTitlePP(SP_XIAOER, SP_XIAOER),
            new Run(new RunProperties(
                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman" },
                new Bold(), new FontSize { Val = SZ_SANHAO }, new FontSizeComplexScript { Val = SZ_SANHAO }),
                new Text("Abstract") { Space = SpaceProcessingModeValues.Preserve }));
    }

    public static Paragraph MakeAbstractBody()
    {
        return new Paragraph(MakeBodyPP(),
            new Run(new RunProperties(
                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "SimSun" },
                new FontSize { Val = SZ_XIAOSI }, new FontSizeComplexScript { Val = SZ_XIAOSI }),
                new Text("With the rapid development of short-video platforms, their impact on college students has become increasingly prominent. This study explores the dual effects of short videos on college students' learning behavior, and analyzes its mechanism in fragmented knowledge acquisition and attention dispersion based on relevant survey data. The study finds that short videos not only expand learning resources for college students, but also bring problems such as insufficient learning engagement. It is necessary to guide college students to use short-video tools rationally.") { Space = SpaceProcessingModeValues.Preserve }));
    }

    public static Paragraph MakeEngKeyPara()
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
    }

    public static void SetupFootnotes(FootnotesPart part)
    {
        var sep = new Footnote { Type = FootnoteEndnoteValues.Separator, Id = -1 };
        sep.Append(new Paragraph(new ParagraphProperties(new SpacingBetweenLines { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto }), new Run(new SeparatorMark())));
        part.Footnotes!.Append(sep);
        var cs = new Footnote { Type = FootnoteEndnoteValues.ContinuationSeparator, Id = 0 };
        cs.Append(new Paragraph(new ParagraphProperties(new SpacingBetweenLines { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto }), new Run(new ContinuationSeparatorMark())));
        part.Footnotes.Append(cs);
    }

    static void AddFnContent(Body body, int id, string fnText)
    {
        var fn = new Footnote { Id = id };
        fn.Append(new Paragraph(
            new ParagraphProperties(new SpacingBetweenLines { Line = "240", LineRule = LineSpacingRuleValues.Auto }),
            new Run(new RunProperties(new VerticalTextAlignment { Val = VerticalPositionValues.Superscript }), new FootnoteReferenceMark()),
            new Run(MakeRP("SimSun", SZ_XIAOWU), new Text(" " + fnText) { Space = SpaceProcessingModeValues.Preserve })));
        body.Ancestors().OfType<Document>().First().MainDocumentPart!.FootnotesPart!.Footnotes!.Append(fn);
    }

    public static void AddFnRefEnd(Body body, int id, string fnText)
    {
        var p = body.Elements<Paragraph>().LastOrDefault(); if (p == null) return;
        p.Append(new Run(MakeSupRP(), new FootnoteReference { Id = id }));
        AddFnContent(body, id, fnText);
    }

    public static void AddRunToLastPara(Body body, string text)
    {
        var p = body.Elements<Paragraph>().LastOrDefault(); if (p == null) return;
        p.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
    }

    public static void AddBodyParaWithFn(Body body, string text, int fnId, string fnText)
    {
        var para = new Paragraph(MakeBodyPP());
        para.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
        para.Append(new Run(MakeSupRP(), new FootnoteReference { Id = fnId }));
        body.Append(para);
        AddFnContent(body, fnId, fnText);
    }

    public static string FN1() => "王兴超, 田芳芳. 大学生短视频成瘾与学业成绩的关系——学习投入的中介作用和学业自我效能感的调节作用[J]. 华南师范大学学报(社会科学版), 2025(1):12-20.";
    public static string FN2() => "Li Y, Wang H. Effects of short-form video app addiction on academic anxiety and academic engagement: The mediating role of mindfulness[J]. Frontiers in Psychology, 2024, 15:1428813.";
    public static string FN3() => "Zhang L, Liu M. The effect of short-form video addiction on undergraduates’ academic procrastination: a moderated mediation model[J]. Frontiers in Psychology, 2023, 14:1298361.";
    public static string FN4() => "中国互联网络信息中心. 第55次中国互联网络发展状况统计报告[R]. 北京: 中国互联网络信息中心, 2025.";

    public static void AddPageBreak(Body body)
    {
        var p = body.Elements<Paragraph>().LastOrDefault(); if (p == null) return;
        p.Append(new Run(new Break { Type = BreakValues.Page }));
    }

    public static void AddBlankPage(Body body)
    {
        body.Append(new Paragraph(new ParagraphProperties(new SpacingBetweenLines { Line = LINE_125, LineRule = LineSpacingRuleValues.Auto }), new Run(MakeRP("SimSun", SZ_XIAOSI), new Text("") { Space = SpaceProcessingModeValues.Preserve })));
    }

    public static DocDefaults MakeDocDefaults()
    {
        return new DocDefaults(
            new RunPropertiesDefault(new RunPropertiesBaseStyle(
                new RunFonts { EastAsia = "SimSun", Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                new FontSize { Val = SZ_XIAOSI }, new FontSizeComplexScript { Val = SZ_XIAOSI })),
            new ParagraphPropertiesDefault(new ParagraphPropertiesBaseStyle(
                new SpacingBetweenLines { Line = LINE_125, LineRule = LineSpacingRuleValues.Auto })));
    }

    // sectPr child order (OpenXML schema):
    // headerRef* footerRef* footnotePr? endnotePr? [type] pgSz pgMar ... docGrid pgNumType ... evenAndOddHeaders?
    public static SectionProperties MakeSectPr(string? hdrId, string? ftrId, SectionMarkValues? secType, NumberFormatValues? nf, int? start)
    {
        var sp = new SectionProperties();
        if (hdrId != null) sp.Append(new HeaderReference { Type = HeaderFooterValues.Default, Id = hdrId });
        if (ftrId != null) sp.Append(new FooterReference { Type = HeaderFooterValues.Default, Id = ftrId });
        if (secType.HasValue) sp.Append(new SectionType { Val = secType.Value });
        sp.Append(MakePageSize());
        sp.Append(MakeMargin());
        sp.Append(new DocGrid { LinePitch = 1 });
        if (nf != null) { var pnt = new PageNumberType { Format = nf }; if (start.HasValue) pnt.Start = start.Value; sp.Append(pnt); }
        return sp;
    }

    public static SectionProperties MakeArabicSectPr(string hdrOdd, string ftrOdd, string hdrEven, string ftrEven, SectionMarkValues? secType, NumberFormatValues? nf, int? start)
    {
        var sp = new SectionProperties();
        sp.Append(new HeaderReference { Type = HeaderFooterValues.Default, Id = hdrOdd });
        sp.Append(new FooterReference { Type = HeaderFooterValues.Default, Id = ftrOdd });
        sp.Append(new HeaderReference { Type = HeaderFooterValues.Even, Id = hdrEven });
        sp.Append(new FooterReference { Type = HeaderFooterValues.Even, Id = ftrEven });
        if (secType.HasValue) sp.Append(new SectionType { Val = secType.Value });
        sp.Append(MakePageSize());
        sp.Append(MakeMargin());
        sp.Append(new DocGrid { LinePitch = 1 });
        if (nf != null) { var pnt = new PageNumberType { Format = nf }; if (start.HasValue) pnt.Start = start.Value; sp.Append(pnt); }
        sp.Append(new EvenAndOddHeaders());
        return sp;
    }

    public static void CloseSection(Body body, SectionProperties sp)
    {
        var p = body.Elements<Paragraph>().LastOrDefault(); if (p == null) return;
        var pp = p.Elements<ParagraphProperties>().FirstOrDefault();
        if (pp == null) { pp = new ParagraphProperties(); p.PrependChild(pp); }
        pp.Append(sp);
    }

    public static HeaderPart MakeHeader(MainDocumentPart mp, string text, JustificationValues jc)
    {
        var hp = mp.AddNewPart<HeaderPart>();
        hp.Header = new Header(new Paragraph(
            new ParagraphProperties(
                new Justification { Val = jc },
                new ParagraphBorders(
                    new BottomBorder { Val = BorderValues.Single, Size = 6, Space = 1, Color = "000000" })),
            new Run(new RunProperties(new RunFonts { EastAsia = "SimSun", Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize { Val = SZ_XIAOWU }, new FontSizeComplexScript { Val = SZ_XIAOWU }),
                new Text(text) { Space = SpaceProcessingModeValues.Preserve })));
        hp.Header.Save(); return hp;
    }

    public static FooterPart MakeFooter(MainDocumentPart mp, JustificationValues jc)
    {
        var fp = mp.AddNewPart<FooterPart>();
        var para = new Paragraph(new ParagraphProperties(new Justification { Val = jc }));
        // PAGE field with 小五 font
        var rp = new RunProperties(
            new RunFonts { EastAsia = "SimSun", Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
            new FontSize { Val = SZ_XIAOWU },
            new FontSizeComplexScript { Val = SZ_XIAOWU });
        para.Append(new Run(new RunProperties(rp.CloneNode(true)), new FieldChar { FieldCharType = FieldCharValues.Begin }));
        para.Append(new Run(new RunProperties(rp.CloneNode(true)), new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve }));
        para.Append(new Run(new RunProperties(rp.CloneNode(true)), new FieldChar { FieldCharType = FieldCharValues.End }));
        fp.Footer = new Footer(para); fp.Footer.Save(); return fp;
    }

    public static void AddTOC(Body body)
    {
        var toc = new Paragraph();
        toc.Append(new Run(new FieldChar { FieldCharType = FieldCharValues.Begin }));
        toc.Append(new Run(new FieldCode(" TOC \\o \"1-2\" \\h \\z \\u ") { Space = SpaceProcessingModeValues.Preserve }));
        toc.Append(new Run(new FieldChar { FieldCharType = FieldCharValues.Separate }));
        toc.Append(new Run(new Text("目录将在Word中自动生成，请右键目录→更新域") { Space = SpaceProcessingModeValues.Preserve }));
        toc.Append(new Run(new FieldChar { FieldCharType = FieldCharValues.End }));
        body.Append(toc);
    }

    public static void AddTable1(Body body)
    {
        body.Append(new Paragraph(MakeCapPP(), new Run(MakeRP("FangSong", SZ_WUHAO), new Text("表1  大学生短视频使用与学习行为的相关性分析") { Space = SpaceProcessingModeValues.Preserve })));

        var tbl = new Table();
        tbl.Append(new TableProperties(
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 12, Space = 0, Color = "000000" },
                new BottomBorder { Val = BorderValues.Single, Size = 12, Space = 0, Color = "000000" },
                new LeftBorder { Val = BorderValues.None, Size = 0 }, new RightBorder { Val = BorderValues.None, Size = 0 },
                new InsideHorizontalBorder { Val = BorderValues.None, Size = 0 }, new InsideVerticalBorder { Val = BorderValues.None, Size = 0 }),
            new TableLayout { Type = TableLayoutValues.Fixed }, new TableLook { Val = "04A0" }));
        tbl.Append(new TableGrid(new GridColumn { Width = "2400" }, new GridColumn { Width = "2400" }, new GridColumn { Width = "1600" }, new GridColumn { Width = "1200" }));

        var hr = new TableRow(new TableRowProperties(new TableRowHeight { Val = 400U }));
        foreach (var hh in new[] { "变量", "与短视频成瘾的相关系数", "显著性", "样本量" })
            hr.Append(new TableCell(
                new TableCellProperties(new TableCellBorders(new BottomBorder { Val = BorderValues.Single, Size = 12, Space = 0, Color = "000000" }), new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }),
                new Paragraph(new ParagraphProperties(new Justification { Val = JC_CENTER }, new SpacingBetweenLines { Line = "260", LineRule = LineSpacingRuleValues.Auto }),
                    new Run(MakeRP("SimSun", SZ_WUHAO, bold: true), new Text(hh) { Space = SpaceProcessingModeValues.Preserve }))));
        tbl.Append(hr);

        var rows = new[] { new[] { "学习投入得分", "-0.32", "p<0.01", "1896" }, new[] { "学业成绩", "-0.28", "p<0.01", "1896" } };
        foreach (var rd in rows)
        {
            var row = new TableRow(new TableRowProperties(new TableRowHeight { Val = 380U }));
            foreach (var ct in rd)
                row.Append(new TableCell(
                    new TableCellProperties(new TableCellBorders(new TopBorder { Val = BorderValues.None, Size = 0 }, new BottomBorder { Val = BorderValues.None, Size = 0 }, new LeftBorder { Val = BorderValues.None, Size = 0 }, new RightBorder { Val = BorderValues.None, Size = 0 }), new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }),
                    new Paragraph(new ParagraphProperties(new Justification { Val = JC_CENTER }, new SpacingBetweenLines { Line = "260", LineRule = LineSpacingRuleValues.Auto }),
                        new Run(MakeRP("SimSun", SZ_WUHAO), new Text(ct) { Space = SpaceProcessingModeValues.Preserve }))));
            tbl.Append(row);
        }
        body.Append(tbl);
        body.Append(new Paragraph(new ParagraphProperties(new SpacingBetweenLines { Before = "60", After = "60" })));
    }

    public static void AddFigure1(Body body, string baseDir)
    {
        string imgPath = Path.Combine(baseDir, "input", "2", "photo", "fig1.png");
        if (!File.Exists(imgPath))
        {
            body.Append(new Paragraph(MakeCapPP(), new Run(MakeRP("FangSong", SZ_WUHAO), new Text("图1  大学生短视频使用用途占比（图片缺失）") { Space = SpaceProcessingModeValues.Preserve })));
            return;
        }
        var doc = body.Ancestors().OfType<Document>().First(); var mp = doc.MainDocumentPart!;
        var ip = mp.AddImagePart(ImagePartType.Png);
        using (var fs = new FileStream(imgPath, FileMode.Open)) { ip.FeedData(fs); }
        var rid = mp.GetIdOfPart(ip);
        long w = 3401600, h = 2551200;
        var drawing = new Drawing(new DW.Inline(
            new DW.Extent { Cx = w, Cy = h },
            new DW.EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
            new DW.DocProperties { Id = 1U, Name = "图1" },
            new DW.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks { NoChangeAspect = true }),
            new A.Graphic(new A.GraphicData(
                new PIC.Picture(
                    new PIC.NonVisualPictureProperties(
                        new PIC.NonVisualDrawingProperties { Id = 0U, Name = "fig1.png" },
                        new PIC.NonVisualPictureDrawingProperties()),
                    new PIC.BlipFill(
                        new A.Blip { Embed = rid },
                        new A.Stretch(new A.FillRectangle())),
                    new PIC.ShapeProperties(
                        new A.Transform2D(
                            new A.Offset { X = 0L, Y = 0L },
                            new A.Extents { Cx = w, Cy = h }),
                        new A.PresetGeometry { Preset = A.ShapeTypeValues.Rectangle })))
            { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }))
        { DistanceFromTop = 0U, DistanceFromBottom = 0U, DistanceFromLeft = 0U, DistanceFromRight = 0U });
        body.Append(new Paragraph(new ParagraphProperties(new Justification { Val = JC_CENTER }), new Run(drawing)));
        body.Append(new Paragraph(MakeCapPP("60"), new Run(MakeRP("FangSong", SZ_WUHAO), new Text("图1  大学生短视频使用用途占比") { Space = SpaceProcessingModeValues.Preserve })));
    }

    public static void AddReferences(Body body)
    {
        body.Append(new Paragraph(MakeRefPP(), new Run(MakeRP("SimSun", SZ_XIAOSI), new Text("[1] 王兴超, 田芳芳. 大学生短视频成瘾与学业成绩的关系——学习投入的中介作用和学业自我效能感的调节作用[J]. 华南师范大学学报(社会科学版), 2025(1):12-20.") { Space = SpaceProcessingModeValues.Preserve })));

        var r2 = new Paragraph(MakeRefPP());
        r2.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text("[2] Li Y, Wang H. ") { Space = SpaceProcessingModeValues.Preserve }));
        r2.Append(new Run(MakeRP("SimSun", SZ_XIAOSI, italic: true), new Text("Effects of short-form video app addiction on academic anxiety and academic engagement: The mediating role of mindfulness") { Space = SpaceProcessingModeValues.Preserve }));
        r2.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text("[J]. Frontiers in Psychology, 2024, 15:1428813.") { Space = SpaceProcessingModeValues.Preserve }));
        body.Append(r2);

        var r3 = new Paragraph(MakeRefPP());
        r3.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text("[3] Zhang L, Liu M. ") { Space = SpaceProcessingModeValues.Preserve }));
        r3.Append(new Run(MakeRP("SimSun", SZ_XIAOSI, italic: true), new Text("The effect of short-form video addiction on undergraduates’ academic procrastination: a moderated mediation model") { Space = SpaceProcessingModeValues.Preserve }));
        r3.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text("[J]. Frontiers in Psychology, 2023, 14:1298361.") { Space = SpaceProcessingModeValues.Preserve }));
        body.Append(r3);

        body.Append(new Paragraph(MakeRefPP(), new Run(MakeRP("SimSun", SZ_XIAOSI), new Text("[4] 中国互联网络信息中心. 第55次中国互联网络发展状况统计报告[R]. 北京: 中国互联网络信息中心, 2025.") { Space = SpaceProcessingModeValues.Preserve })));
    }

    public static Paragraph MakeAppendixTitle()
    {
        return new Paragraph(
            new ParagraphProperties(new Justification { Val = JC_CENTER }, new SpacingBetweenLines { Before = SP_H1, After = SP_H1, Line = LINE_125, LineRule = LineSpacingRuleValues.Auto }, new OutlineLevel { Val = 0 }),
            new Run(MakeRP("SimSun", SZ_SANHAO, bold: true), new Text("附  录") { Space = SpaceProcessingModeValues.Preserve }));
    }

    public static Paragraph MakeAckTitle()
    {
        return new Paragraph(
            new ParagraphProperties(new Justification { Val = JC_CENTER }, new SpacingBetweenLines { Before = SP_H1, After = SP_H1, Line = LINE_125, LineRule = LineSpacingRuleValues.Auto }, new OutlineLevel { Val = 0 }),
            new Run(MakeRP("SimSun", SZ_SANHAO, bold: true), new Text("致  谢") { Space = SpaceProcessingModeValues.Preserve }));
    }

    // ═══════════════════════════════════════════
    // Fix signature underline length
    // ═══════════════════════════════════════════
    static void FixSignatureUnderlines(OpenXmlElement el)
    {
        // Process all paragraphs in the element tree
        foreach (var p in el.Descendants<Paragraph>())
        {
            var pText = p.InnerText;
            // Only process paragraphs with signature-related content
            if (!pText.Contains("签名") && !pText.Contains("期") && !pText.Contains("指导教师"))
                continue;

            var runs = p.Elements<Run>().ToList();
            foreach (var r in runs)
            {
                var rp = r.Elements<RunProperties>().FirstOrDefault();
                if (rp == null) continue;
                var u = rp.Elements<Underline>().FirstOrDefault();
                if (u == null) continue;

                // This run has underline — extend its spaces
                var t = r.Elements<Text>().FirstOrDefault();
                if (t == null) continue;

                var text = t.Text ?? "";
                // Only affects runs that are primarily whitespace
                if (text.Trim().Length == 0 && text.Length > 0)
                {
                    // Double the whitespace count for longer underline
                    t.Text = new string(' ', Math.Max(24, text.Length * 2));
                }
                else if (text.Trim().Length > 0 && text.Contains(" ") && text.Length - text.Trim().Length > text.Trim().Length)
                {
                    // Run has mostly spaces with some text — extend trailing spaces
                    var trimmed = text.TrimEnd();
                    var spaceCount = text.Length - trimmed.Length;
                    t.Text = trimmed + new string(' ', Math.Max(24, spaceCount * 2));
                }
            }
        }
    }

    // ═══════════════════════════════════════════
    // Merge template cover/back cover
    // ═══════════════════════════════════════════
    public static void MergeTemplate(WordprocessingDocument outDoc, MainDocumentPart outMain, Body outBody, string templatePath)
    {
        if (!File.Exists(templatePath)) { Console.WriteLine("Template not found, skipping merge."); return; }

        using var tplDoc = WordprocessingDocument.Open(templatePath, false);
        var tplMain = tplDoc.MainDocumentPart!;
        var tplBody = tplMain.Document!.Body!;

        // Collect all body children and find section break positions
        var children = tplBody.ChildElements.ToList();
        var sectPrIndices = new List<int>(); // indices of paragraphs that contain sectPr in pPr
        int bodySectPrIndex = -1;

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] is Paragraph p)
            {
                var pp = p.Elements<ParagraphProperties>().FirstOrDefault();
                if (pp != null && pp.Elements<SectionProperties>().Any())
                    sectPrIndices.Add(i);
            }
            if (children[i] is SectionProperties)
                bodySectPrIndex = i;
        }

        Console.WriteLine($"Template sections: {sectPrIndices.Count} in-pPr, body sectPr at {bodySectPrIndex}");

        // The template has:
        // - Cover pages (paragraphs before the 2nd sectPr) - this is the last in-pPr sectPr before back cover
        // - Back cover (paragraphs after the 2nd sectPr, up to the body sectPr)
        // We want: cover content before our content, back cover after our content

        if (sectPrIndices.Count < 2) { Console.WriteLine("Template doesn't have expected structure."); return; }

        int coverEnd = sectPrIndices[1]; // index of 2nd sectPr paragraph (this ends the academic pledge section)
        int backCoverStart = sectPrIndices[1] + 1;
        int backCoverEnd = bodySectPrIndex >= 0 ? bodySectPrIndex : children.Count;

        // Copy images from template to output
        var imageMap = CopyImages(tplMain, outMain);

        // ── Prepend cover paragraphs (before our first section) ──
        var coverElements = new List<OpenXmlElement>();
        for (int i = 0; i <= coverEnd; i++)
        {
            var el = CloneWithoutSectPr(children[i], imageMap);
            if (el != null) { FixSignatureUnderlines(el); coverElements.Add(el); }
        }
        // Insert cover at the very beginning of our body
        OpenXmlElement? firstChild = outBody.Elements<Paragraph>().FirstOrDefault()
            ?? (OpenXmlElement?)outBody.Elements<Table>().FirstOrDefault();
        if (firstChild != null)
        {
            foreach (var el in coverElements)
                outBody.InsertBefore(el, firstChild);
        }

        // ── Append back cover paragraphs (after our last section, before final sectPr) ──
        var bcElements = new List<OpenXmlElement>();
        for (int i = backCoverStart; i < backCoverEnd; i++)
        {
            var el = CloneWithoutSectPr(children[i], imageMap);
            if (el != null) bcElements.Add(el);
        }
        // Insert before the final sectPr in our body
        var finalSectPr = outBody.Elements<SectionProperties>().FirstOrDefault();
        foreach (var el in bcElements)
        {
            if (finalSectPr != null)
                outBody.InsertBefore(el, finalSectPr);
            else
                outBody.Append(el);
        }

        // Fill in cover table with user data
        FillCoverTable(outBody);

        Console.WriteLine($"Merged {coverElements.Count} cover elements + {bcElements.Count} back cover elements");
    }

    static void FillCoverTable(Body outBody)
    {
        var tbl = outBody.Elements<Table>().FirstOrDefault();
        if (tbl == null) return;

        // Cover table row data: label, value, isEnglish
        var data = new (string label, string value, bool isEnglish)[] {
            ("中文题目", "短视频对大学生行为的影响研究", false),
            ("英文题目", "Research on Short Videos’ Impact on College Students’ Behaviors", true),
            ("姓名", "袁勋", false),
            ("学号", "202310010237", false),
            ("班级", "文化产业管理2班", false),
            ("专业", "文化产业管理", false),
            ("学院", "文化旅游与新闻艺术学院", false),
            ("指导教师", "李旭鹏 讲师", false),
            ("完成时间", "2026年5月5日", false),
        };

        var rows = tbl.Elements<TableRow>().ToList();
        for (int i = 0; i < rows.Count && i < data.Length; i++)
        {
            var cells = rows[i].Elements<TableCell>().ToList();
            if (cells.Count < 3) continue;
            var targetCell = cells[2]; // 3rd column (0-indexed: 2)

            // Remove existing empty paragraphs in the cell
            targetCell.RemoveAllChildren<Paragraph>();

            // Create filled paragraph
            var para = new Paragraph(
                new ParagraphProperties(
                    new SpacingBetweenLines { Line = "360", LineRule = LineSpacingRuleValues.Auto },
                    data[i].isEnglish
                        ? new Justification { Val = JustificationValues.Left }
                        : new Justification { Val = JustificationValues.Center }));

            if (data[i].isEnglish)
            {
                // English title: Times New Roman 四号 斜体
                para.Append(new Run(
                    new RunProperties(
                        new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman" },
                        new Italic(),
                        new FontSize { Val = SZ_SIHAO }, new FontSizeComplexScript { Val = SZ_SIHAO }),
                    new Text(data[i].value) { Space = SpaceProcessingModeValues.Preserve }));
            }
            else
            {
                // Chinese content: 宋体四号加粗, numbers TNR四号加粗, centered
                // Split text into Chinese parts and number/English parts
                var text = data[i].value;
                var currentRun = new System.Text.StringBuilder();
                var isCurrentLatin = false; // true for ASCII/digit chars

                for (int j = 0; j < text.Length; j++)
                {
                    var c = text[j];
                    var isLatin = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')
                        || c == '/' || c == '-' || c == '.' || c == ' ';
                    // Space can belong to either
                    if (c == ' ') isLatin = isCurrentLatin;

                    if (j == 0)
                    {
                        isCurrentLatin = isLatin;
                        currentRun.Append(c);
                    }
                    else if (isLatin == isCurrentLatin)
                    {
                        currentRun.Append(c);
                    }
                    else
                    {
                        // Flush current run
                        para.Append(MakeCoverRun(currentRun.ToString(), isCurrentLatin));
                        currentRun.Clear();
                        currentRun.Append(c);
                        isCurrentLatin = isLatin;
                    }
                }
                // Flush remaining
                if (currentRun.Length > 0)
                    para.Append(MakeCoverRun(currentRun.ToString(), isCurrentLatin));
            }

            targetCell.Append(para);
        }
    }

    static Run MakeCoverRun(string text, bool isLatin)
    {
        var rp = new RunProperties(
            new Bold(),
            new FontSize { Val = SZ_SIHAO }, new FontSizeComplexScript { Val = SZ_SIHAO });
        if (isLatin)
        {
            rp.Append(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman" });
        }
        else
        {
            rp.Append(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "SimSun" });
        }
        return new Run(rp, new Text(text) { Space = SpaceProcessingModeValues.Preserve });
    }

    static Dictionary<string, string> CopyImages(MainDocumentPart src, MainDocumentPart dst)
    {
        var map = new Dictionary<string, string>();
        foreach (var ip in src.ImageParts)
        {
            var newIp = dst.AddImagePart(ip.ContentType);
            using (var s = ip.GetStream(FileMode.Open))
                newIp.FeedData(s);
            var oldId = src.GetIdOfPart(ip);
            var newId = dst.GetIdOfPart(newIp);
            map[oldId] = newId;
        }
        return map;
    }

    static OpenXmlElement? CloneWithoutSectPr(OpenXmlElement el, Dictionary<string, string> imageMap)
    {
        var clone = el.CloneNode(true);

        // Skip SectionProperties at body level
        if (clone is SectionProperties) return null;

        bool hadSectPr = false;
        if (clone is Paragraph p)
        {
            var pp = p.Elements<ParagraphProperties>().FirstOrDefault();
            if (pp != null)
            {
                var sp = pp.Elements<SectionProperties>().FirstOrDefault();
                if (sp != null)
                {
                    hadSectPr = true;
                    sp.Remove();
                }
            }
        }

        // Force xml:space="preserve" on ALL Text elements
        foreach (var t in clone.Descendants<Text>())
        {
            // Use SetAttribute to ensure the xml:space attribute is written to XML
            t.Space = SpaceProcessingModeValues.Preserve;
            t.SetAttribute(new OpenXmlAttribute("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve"));
        }

        // Fix: also preserve rFonts attributes explicitly
        foreach (var rf in clone.Descendants<RunFonts>())
        {
            if (rf.Hint != null)
            {
                var hintVal = rf.Hint.Value;
                rf.SetAttribute(new OpenXmlAttribute("w", "hint", null, hintVal == FontTypeHintValues.EastAsia ? "eastAsia" : ""));
            }
        }

        // Add page break if we removed a sectPr
        if (hadSectPr)
        {
            ((Paragraph)clone).Append(new Run(new Break { Type = BreakValues.Page }));
        }

        // Update image relationship IDs
        UpdateBlipIds(clone, imageMap);

        return clone;
    }

    static void UpdateBlipIds(OpenXmlElement el, Dictionary<string, string> imageMap)
    {
        foreach (var blip in el.Descendants<A.Blip>())
        {
            if (blip.Embed != null && imageMap.TryGetValue(blip.Embed!.Value!, out var newId))
                blip.Embed = newId;
        }
    }

    public static void AddComments(MainDocumentPart mp, Body body)
    {
        var cp = mp.AddNewPart<WordprocessingCommentsPart>(); cp.Comments = new Comments();
        var cep = mp.AddNewPart<WordprocessingCommentsExPart>();
        using (var w = new StreamWriter(cep.GetStream(FileMode.Create)))
            w.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><w15:commentsEx xmlns:w15=\"http://schemas.microsoft.com/office/word/2012/wordml\" xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\" mc:Ignorable=\"w15\"/>");
        var cip = mp.AddNewPart<WordprocessingCommentsIdsPart>();
        using (var w = new StreamWriter(cip.GetStream(FileMode.Create)))
            w.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><w16cid:commentsIds xmlns:w16cid=\"http://schemas.microsoft.com/office/word/2016/wordml/cid\"/>");
        var pp = mp.AddNewPart<WordprocessingPeoplePart>();
        pp.People = new DocumentFormat.OpenXml.Office2013.Word.People(); pp.People.Save();

        AddOneComment(cp, body, 0, "请确认目录是否可自动更新（右键目录→更新域）");
        AddOneComment(cp, body, 1, "请确认三线表格式是否规范（顶线、栏目线为粗线，底线为粗线，无竖线）");
        AddOneComment(cp, body, 2, "请确认奇偶页页眉页脚是否规范（奇数页右对齐，偶数页左对齐）");
        AddOneComment(cp, body, 3, "请确认脚注是否规范（自动脚注，数字上标，字体）");
        cp.Comments.Save();
    }

    static void AddOneComment(WordprocessingCommentsPart cp, Body body, int id, string text)
    {
        cp.Comments!.Append(new Comment(new Paragraph(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }))) { Id = id.ToString(), Author = "ban", Initials = "ban", Date = DateTime.Now });
        var fp = body.Elements<Paragraph>().FirstOrDefault();
        if (fp != null) { fp.PrependChild(new CommentRangeStart { Id = id.ToString() }); fp.Append(new CommentRangeEnd { Id = id.ToString() }); fp.Append(new Run(new CommentReference { Id = id.ToString() })); }
    }
}
