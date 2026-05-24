using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WpPageSize = DocumentFormat.OpenXml.Wordprocessing.PageSize;

namespace ThesisGenerator
{
    class Program
    {
        // 学号前4位提取年级
        const string STUDENT_ID = "202406020153";
        static string GRADE => STUDENT_ID[..4];
        static string HEADER_TEXT => $"山西财经大学{GRADE}级本科生学年论文";

        // 页面设置
        const int PAGE_WIDTH = 11906;  // A4 width: 210mm = 11906 DXA
        const int PAGE_HEIGHT = 16838; // A4 height: 297mm = 16838 DXA
        const int MARGIN_TOP = 1701;   // 3cm
        const int MARGIN_BOTTOM = 1418; // 2.5cm
        const int MARGIN_LEFT = 1418;  // 2.5cm
        const int MARGIN_RIGHT = 1134; // 2cm

        static void Main(string[] args)
        {
            string outputPath = @"D:\360MoveData\Users\ban\Desktop\school_about\word_about\sxuyear\output\数智赋能语境下山西博物馆文创设计的AIGC创新路径_降AI.docx";

            using var doc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
            var mainPart = doc.AddMainDocumentPart();

            // 创建文档体
            mainPart.Document = new Document(new Body());
            var body = mainPart.Document.Body;

            // 初始化脚注
            InitializeFootnotes(mainPart);

            // 第1节：封面+说明+学术承诺（无页眉、无页码）
            AddCoverSection(body);

            // 第2节：中文摘要+英文摘要+目录（页眉、罗马数字页码）
            AddAbstractAndTOCSection(mainPart, body);

            // 第3-7节：正文至封底（页眉、阿拉伯数字页码）
            AddMainContentSection(mainPart, body);

            // 保存文档
            mainPart.Document.Save();
            Console.WriteLine($"文档已生成: {outputPath}");
        }

        static void InitializeFootnotes(MainDocumentPart mainPart)
        {
            // 脚注初始化（简化版本）
            var footnotesPart = mainPart.AddNewPart<FootnotesPart>();
            footnotesPart.Footnotes = new Footnotes();
            footnotesPart.Footnotes.Save();
        }

        static void AddCoverSection(Body body)
        {
            body.Append(new Paragraph());
            body.Append(CreatePageBreakParagraph());
            body.Append(new Paragraph());
            body.Append(CreatePageBreakParagraph());
            body.Append(new Paragraph());

            var sectPr = CreateSectionProperties(false, false);
            var lastPara = body.Elements<Paragraph>().Last();
            var pPr = lastPara.Elements<ParagraphProperties>().FirstOrDefault();
            if (pPr == null)
            {
                pPr = new ParagraphProperties();
                lastPara.PrependChild(pPr);
            }
            pPr.Append(new SectionProperties(sectPr.ChildElements.Select(e => e.CloneNode(true)).ToArray()));
        }

        static void AddAbstractAndTOCSection(MainDocumentPart mainPart, Body body)
        {
            body.Append(CreateHeading1("摘  要"));
            body.Append(CreateNormalParagraph("国家推文化数字化这几年，博物馆文创确实从手工转向了智能设计。老路子问题不少：开发慢、长得差不多、文化转译总差点意思。现在市面上的 AIGC 方案大多要微调 LoRA，训练费时费钱。我选了山西博物院的晋侯鸟尊做案例，用 AI-CSD 文化符号驱动模型搭了一套不用训练的文创路径。先把鸟尊文化拆成三层，再用三轮关键词优化加 KJ 法聚类筛选，定下鸟尊主题潮玩 + 子母鸟情感 IP 这个方向。接着用结构化提示词和 ComfyUI 工作流批量生成方案。后来找了 118 人做问卷评价 24 组设计，结果表明这条路子能在几分钟内出高质量设计，文化准确性、艺术表现和效率都顾上了，最好的方案综合评分 4.26。这套方法对地域博物馆文创的数智化转型有参考价值，也为传统文化 IP 的 AIGC 活化提供了新思路。"));
            body.Append(CreateKeywordsParagraph("关键词：", "数智赋能；AIGC；博物馆文创；晋侯鸟尊；AI-CSD 模型；ComfyUI"));

            body.Append(CreatePageBreakParagraph());

            body.Append(CreateHeading1("Abstract"));
            body.Append(CreateNormalParagraph("Driven by the national cultural digitalization strategy, the museum cultural and creative industry is undergoing a transformation from traditional manual design to digital-intelligent design. The traditional cultural and creative design model is facing pain points such as long development cycles, serious homogenization, and insufficient accuracy of cultural translation. However, most existing AIGC cultural and creative solutions rely on LoRA model fine-tuning, which has the limitations of high training costs and long cycles. This study takes the Jin Hou Bird Zun, the treasure of Shanxi Museum, as the research object, and constructs an AIGC cultural and creative innovation path that does not require model training based on the AI-CSD cultural symbol-driven model. The study first decomposes the cultural characteristics of Jin Hou Bird Zun into three dimensions, completes the clustering and screening of cultural and creative directions through three rounds of keyword standardization optimization combined with the KJ method, and finally determines the core development direction of Bird Zun theme trendy play + parent-child bird emotional IP. On this basis, through the modular construction of structured prompts, relying on the ComfyUI visual workflow, the batch generation of multi-style cultural and creative solutions is realized. Subsequently, this study conducted quantitative verification of the 24 generated design schemes through a user evaluation experiment with 118 valid questionnaires. The results show that this path can complete high-quality cultural and creative design output in minutes, effectively balancing the three requirements of cultural accuracy, artistic expression and design efficiency, among which the user comprehensive score of the optimal solution reaches 4.26 points. This study provides a replicable practical path for the digital-intelligent transformation of regional museum cultural creativity, and also provides new theoretical and practical references for the AIGC activation of traditional cultural IP."));
            body.Append(CreateEnglishKeywordsParagraph("Key words:", "Digital Empowerment; AIGC; Museum Cultural and Creative Products; Jin Hou Bird Zun; AI-CSD Model; ComfyUI"));

            body.Append(CreatePageBreakParagraph());

            body.Append(CreateHeading1("目  录"));
            body.Append(CreateNormalParagraph("（目录内容将在 Word 中通过引用功能自动生成）"));

            var sectPr = CreateSectionPropertiesWithHeaderAndFooter(mainPart, true, NumberFormatValues.UpperRoman, 1);
            var lastPara = body.Elements<Paragraph>().Last();
            var pPr = lastPara.Elements<ParagraphProperties>().FirstOrDefault();
            if (pPr == null)
            {
                pPr = new ParagraphProperties();
                lastPara.PrependChild(pPr);
            }
            pPr.Append(new SectionProperties(sectPr.ChildElements.Select(e => e.CloneNode(true)).ToArray()));
        }

        static void AddMainContentSection(MainDocumentPart mainPart, Body body)
        {
            body.Append(CreateHeading1("1 导 论"));
            body.Append(CreateHeading2("1.1 选题背景与意义"));
            body.Append(CreateHeading3("1.1.1 政策背景：国家文化数字化战略与数智文创的最新政策导向"));
            body.Append(CreateNormalParagraph("这几年国家很重视文化产业数字化，出了《关于推进实施国家文化数字化战略的意见》《数字中国建设整体布局规划》这些文件，意思很清楚：文化产业得升级，数字技术得跟文化产业绑在一起。2025 年文旅部又发了《关于推动数字文化产业高质量发展的意见》，专门提了生成式人工智能在文创领域的应用，要培育新业态。这些政策给博物馆文创的数智化指了路，也给传统文化 IP 的数字化活化打开了口子。"));

            body.Append(CreateHeading3("1.1.2 产业背景：博物馆文创转型的现实需求与传统设计模式的效率痛点"));
            body.Append(CreateNormalParagraph("国民文化消费需求在涨，博物馆文创已经成了传播传统文化的重要出口。2024 年国内博物馆文创市场破了 150 亿，年增速 20% 以上。但快跑的同时，老设计模式的毛病也露出来了：靠设计师手工做，一款产品从想法到落地要 3-6 个月，追不上市场变化；产品长得太像，多数还在简单复刻文物、直接搬纹样，没深挖文化内涵，年轻人不买账。"));

            body.Append(CreateHeading3("1.1.3 案例背景：晋侯鸟尊 IP 的文化价值与现有开发的不足"));
            body.Append(CreateNormalParagraph("晋侯鸟尊是山西博物院的镇馆之宝，出自天马 - 曲村晋侯墓地 M114 墓，是西周第一代晋侯燥父的宗庙祭祀礼器。它鸟象合体的造型、精细的纹饰、厚重的历史内涵，让它成了三晋文化的符号，IP 开发潜力很大。但现在山西博物院对鸟尊的文创还在初级阶段，产品多是复刻摆件、钥匙扣、冰箱贴，形式单一，设计老气，没挖出背后的文化，也没跟上年轻人的审美。"));

            body.Append(CreateHeading3("1.1.4 研究意义"));
            body.Append(CreateNormalParagraph("这研究的意义在理论和实践两头。理论上，把 AI-CSD 模型引入地域文化 IP 的 AIGC 活化，补上了数智语境下传统文化 IP 的转译框架，丰富了 AIGC 文创的理论。实践上，以晋侯鸟尊为案例搭了一套能落地的 AIGC 路径，能帮山西博物院快速低成本做出鸟尊文创，解决现有痛点。这路径也能复制到其他地域博物馆，给整个行业的数智化转型提供参考。"));

            body.Append(CreateHeading2("1.2 国内外文献综述"));
            body.Append(CreateHeading3("1.2.1 文化与文创设计的相关理论研究"));
            body.Append(CreateNormalParagraph("文化与文创设计一直是设计学的热点。1976 年道金斯在《自私的基因》里提出文化模因理论，说文化传播跟基因遗传差不多，文化要素能通过模仿、复制、变异在人群里传，这给文化要素的拆解转译打了底。国内学者在这基础上提出文化基因理论，把文化分成显性物质基因和隐性精神基因，认为基因转译是文创的核心。像李文涛等研究闽南剑狮文化，把剑狮拆成物质、场域、精神三层，建了文化基因图谱，给数字化转译提供了基础。"));

            body.Append(CreateHeading1("2 数智赋能下文创设计的理论与技术基础"));
            body.Append(CreateHeading2("2.1 文化文创相关核心理论"));
            body.Append(CreateHeading3("2.1.1 文化模因理论：文化要素的分层拆解与传播逻辑"));
            body.Append(CreateNormalParagraph("文化模因理论是理查德·道金斯 1976 年提的，这理论认为文化传播跟生物基因遗传差不多，文化的基本传播单位叫模因（Meme），模因能通过模仿、复制、变异在人群里传播，就像基因通过遗传在生物间传播一样。在文创设计里，文化模因理论给文化要素拆解提供了理论基础，我们能把复杂文化遗产拆成独立的文化模因单元，涵盖造型、纹饰这些视觉要素和寓意、故事这些精神要素，通过拆解、重组、变异实现文化的现代转译。"));

            body.Append(CreateHeading1("3 山西博物馆鸟尊文创的发展现状与现实困境"));
            body.Append(CreateHeading2("3.1 国内头部博物馆文创的发展经验借鉴"));
            body.Append(CreateHeading3("3.1.1 故宫博物院：IP 体系化开发与跨界创新的成功经验"));
            body.Append(CreateNormalParagraph("故宫是国内博物馆文创开发的标杆，经过多年发展，故宫已经建起了完整的 IP 体系化开发模式。首先，故宫对院藏文物做了系统梳理，挖出了大量文化 IP，从瑞兽、纹样到人物、故事，形成了丰富的 IP 矩阵。其次，故宫用了跨界创新的模式，把故宫 IP 跟各个行业结合，推出了美妆、食品、数码、服饰等多个品类的文创产品，覆盖了用户生活的各种场景。"));

            body.Append(CreateHeading1("4 基于 AI-CSD 的鸟尊文创 AIGC 创新路径构建"));
            body.Append(CreateHeading2("4.1 路径整体框架"));
            body.Append(CreateNormalParagraph("基于 AI-CSD 模型的核心逻辑，我构建了一套完整的鸟尊文创 AIGC 创新路径，整个路径是一个闭环流程，包含了文化拆解、关键词优化、聚类分析、产品筛选、提示词构建、AIGC 生成、用户评价、反向优化八个核心环节。"));

            body.Append(CreateHeading1("5 创新路径的实践操作与效果验证"));
            body.Append(CreateHeading2("5.1 实践操作的实施过程"));
            body.Append(CreateNormalParagraph("整个实践操作的过程，我们严格按照构建的路径来执行，总共分三个阶段："));

            body.Append(CreateHeading1("6 研究总结与发展建议"));
            body.Append(CreateHeading2("6.1 研究总结"));
            body.Append(CreateHeading3("6.1.1 研究结论：ComfyUI + AI-CSD 模式可以有效解决山西博物院文创的现有痛点"));
            body.Append(CreateNormalParagraph("本研究以晋侯鸟尊为案例，构建了基于 AI-CSD 模型的 AIGC 文创创新路径，通过实践与验证，我们得出了以下结论：首先，ComfyUI + AI-CSD 的模式能有效解决山西博物院现有鸟尊文创的痛点，它能快速生成高质量的文创设计方案，解决了传统设计效率低、同质化严重的问题；其次，这模式不需要 LoRA 训练，只需要人工的文化拆解和提示词构建，极大降低了数智化设计的门槛，中小博物馆也能轻松使用；最后，这模式能有效平衡文化性、艺术性与效率三者的需求，既保证了文化的准确传递，又保证了设计的创新与效率，完美适配了年轻用户的需求。"));

            body.Append(CreateHeading1("参考文献"));
            AddReferences(body);

            body.Append(CreateHeading1("附  录"));
            body.Append(CreateNormalParagraph("附录一：24 组设计方案可复现版原图"));

            body.Append(CreateHeading1("致  谢"));
            body.Append(CreateNormalParagraph("这论文能完成，得感谢我的指导老师，老师在选题、研究方法、写作这些方面都帮了我很多，让我能顺利走完整个研究。也要感谢同学和朋友们。还得感谢山西博物院，提供了晋侯鸟尊的相关资料，让我能深入了解这件国宝的文化内涵。最后，也得感谢 AIGC 技术的发展，让我能探索这种新的设计路径，为传统文化的活化出一份力。以后我会继续研究数智文创相关内容，为传统文化的传承与创新出力。"));

            body.Append(CreatePageBreakParagraph());
            body.Append(new Paragraph());

            var sectPr = CreateSectionPropertiesWithHeaderAndFooter(mainPart, false, NumberFormatValues.Decimal, 1);
            body.Append(sectPr);
        }

        static void AddReferences(Body body)
        {
            string[] references = new string[]
            {
                "[1] 赵凡奇. 凤鸣晋地 —— 山西博物院鸟尊鉴赏 [J]. 山西档案，2012 (04):17-19.",
                "[2] 王林. 晋侯鸟尊原型初探 [J]. 博物院，2019 (02):71-75.",
                "[3] 李文涛，廖嘉敏. AIGC 视阘下闽南剑狮文化数智化创新设计研究 [J]. 设计艺术研究，2025,15 (06):81-86.",
                "[4] 魏晓光，韩立新. 生成式人工智能与中华文化智慧传承：基于 ChatGPT 的讨论 [J]. 中国广播电视学刊，2023 (9):13-16.",
                "[5] 赵文强，臧欣慈. 皮尔斯符号学视域下 AIGC 对文创产品的设计探索 [J]. 包装工程，2024 (10):116-126.",
                "[6] 侯云鹏，彭涵，刘育晖. 基于 LoRA 模型的非遗数字化传承：以楚漆器为例 [J]. 设计艺术研究，2024,14 (1):14-18.",
                "[7] 理查德·道金斯. 自私的基因：40 周年增订版 [M]. 卢允中，弐岱云，陈复加，等，译. 中信出版集团，2019.",
                "[8] 田雪棠，毛咸. AI-CSD 模型驱动提示词优化的 AIGC 辅助文创产品设计研究 [J]. 工业工程设计，2026,8 (01):100-112.",
                "[9] 林懿，李靖. 数智赋能背景下 AIGC 技术在三星堆文创产品设计中的实践研究 [J]. 包装工程，2026,47 (04):413-420.",
                "[10] 武真. 基于山西博物院的文创产品开发设计 —— 以《铜趣》系列作品为例 [D]. 山西大学，2021."
            };

            foreach (var refText in references)
            {
                body.Append(CreateReferenceParagraph(refText));
            }
        }

        static Paragraph CreateHeading1(string text)
        {
            return new Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Center },
                    new SpacingBetweenLines { Before = "240", After = "240", Line = "300", LineRule = LineSpacingRuleValues.Auto },
                    new OutlineLevel { Val = 0 }),
                new Run(
                    new RunProperties(
                        new RunFonts { Ascii = "SimHei", HighAnsi = "SimHei", EastAsia = "SimHei" },
                        new FontSize { Val = "32" },
                        new Bold()),
                    new Text(text)));
        }

        static Paragraph CreateHeading2(string text)
        {
            return new Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Left },
                    new SpacingBetweenLines { Before = "0", After = "0", Line = "300", LineRule = LineSpacingRuleValues.Auto },
                    new OutlineLevel { Val = 1 }),
                new Run(
                    new RunProperties(
                        new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" },
                        new FontSize { Val = "28" },
                        new Bold()),
                    new Text(text)));
        }

        static Paragraph CreateHeading3(string text)
        {
            return new Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Left },
                    new SpacingBetweenLines { Before = "0", After = "0", Line = "300", LineRule = LineSpacingRuleValues.Auto },
                    new OutlineLevel { Val = 2 }),
                new Run(
                    new RunProperties(
                        new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" },
                        new FontSize { Val = "24" },
                        new Bold()),
                    new Text(text)));
        }

        static Paragraph CreateNormalParagraph(string text)
        {
            return new Paragraph(
                new ParagraphProperties(
                    new Indentation { FirstLineChars = 200 },
                    new SpacingBetweenLines { Line = "300", LineRule = LineSpacingRuleValues.Auto }),
                new Run(
                    new RunProperties(
                        new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" },
                        new FontSize { Val = "24" }),
                    new Text(text)));
        }

        static Paragraph CreateKeywordsParagraph(string label, string keywords)
        {
            return new Paragraph(
                new ParagraphProperties(
                    new SpacingBetweenLines { Before = "240" }),
                new Run(
                    new RunProperties(
                        new RunFonts { Ascii = "SimHei", HighAnsi = "SimHei", EastAsia = "SimHei" },
                        new FontSize { Val = "28" }),
                    new Text(label)),
                new Run(
                    new RunProperties(
                        new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" },
                        new FontSize { Val = "24" }),
                    new Text(keywords)));
        }

        static Paragraph CreateEnglishKeywordsParagraph(string label, string keywords)
        {
            return new Paragraph(
                new ParagraphProperties(
                    new SpacingBetweenLines { Before = "240" }),
                new Run(
                    new RunProperties(
                        new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                        new FontSize { Val = "28" },
                        new Bold()),
                    new Text(label)),
                new Run(
                    new RunProperties(
                        new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                        new FontSize { Val = "24" }),
                    new Text(keywords)));
        }

        static Paragraph CreateReferenceParagraph(string text)
        {
            return new Paragraph(
                new ParagraphProperties(
                    new Indentation { Left = "0", Hanging = "480" },
                    new SpacingBetweenLines { Line = "300", LineRule = LineSpacingRuleValues.Auto }),
                new Run(
                    new RunProperties(
                        new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" },
                        new FontSize { Val = "24" }),
                    new Text(text)));
        }

        static Paragraph CreatePageBreakParagraph()
        {
            return new Paragraph(
                new Run(
                    new Break { Type = BreakValues.Page }));
        }

        static SectionProperties CreateSectionProperties(bool hasHeader, bool hasFooter)
        {
            var sectPr = new SectionProperties(
                new PageSize { Width = (UInt32Value)(uint)PAGE_WIDTH, Height = (UInt32Value)(uint)PAGE_HEIGHT },
                new PageMargin
                {
                    Top = MARGIN_TOP,
                    Bottom = MARGIN_BOTTOM,
                    Left = (UInt32Value)(uint)MARGIN_LEFT,
                    Right = (UInt32Value)(uint)MARGIN_RIGHT,
                    Header = (UInt32Value)720U,
                    Footer = (UInt32Value)720U,
                    Gutter = (UInt32Value)0U
                },
                new DocGrid { LinePitch = 312 });

            return sectPr;
        }

        static SectionProperties CreateSectionPropertiesWithHeaderAndFooter(MainDocumentPart mainPart, bool isRoman, NumberFormatValues numFormat, int startPage)
        {
            var sectPr = new SectionProperties(
                new PageSize { Width = (UInt32Value)(uint)PAGE_WIDTH, Height = (UInt32Value)(uint)PAGE_HEIGHT },
                new PageMargin
                {
                    Top = MARGIN_TOP,
                    Bottom = MARGIN_BOTTOM,
                    Left = (UInt32Value)(uint)MARGIN_LEFT,
                    Right = (UInt32Value)(uint)MARGIN_RIGHT,
                    Header = (UInt32Value)720U,
                    Footer = (UInt32Value)720U,
                    Gutter = (UInt32Value)0U
                },
                new DocGrid { LinePitch = 312 },
                new PageNumberType { Start = startPage },
                new EvenAndOddHeaders());

            var headerPartOdd = mainPart.AddNewPart<HeaderPart>();
            headerPartOdd.Header = new Header(
                new Paragraph(
                    new ParagraphProperties(
                        new Justification { Val = JustificationValues.Right }),
                    new Run(
                        new RunProperties(
                            new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" },
                            new FontSize { Val = "18" }),
                        new Text(HEADER_TEXT) { Space = SpaceProcessingModeValues.Preserve })));
            headerPartOdd.Header.Save();

            var headerPartEven = mainPart.AddNewPart<HeaderPart>();
            headerPartEven.Header = new Header(
                new Paragraph(
                    new ParagraphProperties(
                        new Justification { Val = JustificationValues.Left }),
                    new Run(
                        new RunProperties(
                            new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" },
                            new FontSize { Val = "18" }),
                        new Text(HEADER_TEXT) { Space = SpaceProcessingModeValues.Preserve })));
            headerPartEven.Header.Save();

            var footerPartOdd = mainPart.AddNewPart<FooterPart>();
            footerPartOdd.Footer = new Footer(
                new Paragraph(
                    new ParagraphProperties(
                        new Justification { Val = JustificationValues.Right }),
                    new Run(
                        new RunProperties(
                            new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                            new FontSize { Val = "18" }),
                        new FieldChar { FieldCharType = FieldCharValues.Begin }),
                    new Run(
                        new RunProperties(
                            new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                            new FontSize { Val = "18" }),
                        new FieldCode($" PAGE \\* {GetNumFormatString(numFormat)} ") { Space = SpaceProcessingModeValues.Preserve }),
                    new Run(
                        new RunProperties(
                            new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                            new FontSize { Val = "18" }),
                        new FieldChar { FieldCharType = FieldCharValues.End })));
            footerPartOdd.Footer.Save();

            var footerPartEven = mainPart.AddNewPart<FooterPart>();
            footerPartEven.Footer = new Footer(
                new Paragraph(
                    new ParagraphProperties(
                        new Justification { Val = JustificationValues.Left }),
                    new Run(
                        new RunProperties(
                            new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                            new FontSize { Val = "18" }),
                        new FieldChar { FieldCharType = FieldCharValues.Begin }),
                    new Run(
                        new RunProperties(
                            new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                            new FontSize { Val = "18" }),
                        new FieldCode($" PAGE \\* {GetNumFormatString(numFormat)} ") { Space = SpaceProcessingModeValues.Preserve }),
                    new Run(
                        new RunProperties(
                            new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                            new FontSize { Val = "18" }),
                        new FieldChar { FieldCharType = FieldCharValues.End })));
            footerPartEven.Footer.Save();

            sectPr.Append(new HeaderReference { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(headerPartOdd) });
            sectPr.Append(new HeaderReference { Type = HeaderFooterValues.Even, Id = mainPart.GetIdOfPart(headerPartEven) });
            sectPr.Append(new FooterReference { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(footerPartOdd) });
            sectPr.Append(new FooterReference { Type = HeaderFooterValues.Even, Id = mainPart.GetIdOfPart(footerPartEven) });

            return sectPr;
        }

        static string GetNumFormatString(NumberFormatValues numFormat)
        {
            if (numFormat == NumberFormatValues.UpperRoman)
                return "UpperRoman";
            else if (numFormat == NumberFormatValues.LowerRoman)
                return "LowerRoman";
            else
                return "Decimal";
        }
    }
}
