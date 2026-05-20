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
        OUTPUT = Path.Combine(BASE, "output", $"新时代青年精神内耗的成因及化解路径.docx");
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
        b.Append(H.MakeBodyPara("新时代背景下，社会加速转型、数智技术深度渗透与多元文化思潮交织，使青年群体普遍面临学业、就业、人际、发展等多重压力，精神内耗已从个体心理现象上升为具有普遍性的社会问题，对青年身心健康、价值塑造、人格完善与成长发展构成现实影响。为系统把握这一议题的研究脉络，本文以“新时代青年精神内耗”为核心对象，采用文献研究法、跨学科研究法与归纳总结法，对国内近五年相关期刊论文、学位论文及研究成果进行全面梳理与整合论述。文章首先界定精神内耗的核心内涵与主要特征，归纳青年精神内耗在心理情绪、行为选择、价值认知、社会适应等方面的现实表征；其次从社会环境、高校教育、数字媒介、家庭影响及青年自身五个维度，系统梳理学界关于精神内耗生成原因的主要观点；再次整合提炼价值引领、教育优化、媒介治理、社会支持、个体调适等化解路径；最后对现有研究成果进行评述，指出研究不足并展望未来方向。研究表明，青年精神内耗是外部压力传导与内在认知失衡共同作用的结果，其治理需要构建社会、学校、家庭、个人协同联动的综合体系。"));
        b.Append(H.MakeKeyPara(true, "新时代青年；精神内耗；成因；化解路径；思想政治教育"));
        H.AddPageBreak(b);
        b.Append(H.MakeAbstractTitle());
        b.Append(H.MakeAbstractBody());
        b.Append(H.MakeEngKeyPara("New Era Youth; Spiritual Involution; Causes; Solutions; Ideological and Political Education"));
        H.AddPageBreak(b);
        b.Append(H.MakeCenteredTitle("目  录", "SimSun", "36", "360", "360"));
        H.AddTOC(b);
        H.CloseSection(b, H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.OddPage, NumberFormatValues.UpperRoman, 1));

        // SECTION 3: Body
        b.Append(H.MakeH1("导  论"));
        b.Append(H.MakeH2("1.1 选题背景与意义"));
        b.Append(H.MakeBodyPara("中国特色社会主义进入新时代，社会结构深刻变革、发展节奏持续加快、数字技术全面融入日常生活，青年作为社会中最敏感、最活跃的群体，其精神世界与心理状态面临前所未有的冲击与挑战。从\u201c内卷\u201d\u201c躺平\u201d\u201c摆烂\u201d到\u201c精神内耗\u201d，一系列网络热词持续流行，直观反映出当代青年在理想与现实、竞争与倦怠、自我期待与社会评价之间的持续心理拉扯。精神内耗表现为自我怀疑、过度纠结、情绪内耗、行动力弱化、价值迷茫等状态，长期存在会导致心理疲惫、学习工作效率下降、社会适应能力不足，甚至影响正确世界观、人生观、价值观的形成。青年是国家的未来、民族的希望，其精神面貌直接关系到民族复兴大业的推进与社会活力的保持。在此背景下，系统梳理新时代青年精神内耗的研究成果，厘清其内涵表征、生成机理与治理路径，具有重要的理论价值与现实必要性。"));
        b.Append(H.MakeH2("1.2 国内外文献综述"));
        b.Append(H.MakeBodyPara("国内学界围绕青年精神内耗的研究自2022年以来迅速增多，已形成心理学、思想政治教育、社会学、教育学等多学科共同参与的研究格局。在概念界定与现实表征方面，研究者普遍认为精神内耗是个体在内在心理冲突、外部压力与自我调节失效共同作用下出现的心理资源持续消耗状态。在成因研究方面，学界已形成较为一致的多维分析框架，涵盖社会、高校、媒介、家庭、个体五个层面。在化解路径方面，主流观点强调协同治理，以思想政治教育、心理健康服务、网络治理、社会支持与个体调适相结合。国外学界没有完全对应的\u201c精神内耗\u201d概念，相关研究主要集中在心理耗竭、焦虑障碍、认知反刍、青年心理健康等领域。西方心理学将过度思考、反刍思维视为心理能量消耗的重要机制，提出认知行为疗法、正念训练、社会支持等干预策略。总体来看，国外研究侧重临床心理与社会政策，与中国文化语境、教育体制与青年发展现实存在差异，本土化转化与创新空间较大。"));
        b.Append(H.MakeH2("1.3 论文的结构及主要内容"));
        b.Append(H.MakeBodyPara("本文按照\u201c概念界定\u2014表征梳理\u2014成因归纳\u2014路径整合\u2014研究评述\u201d的逻辑展开。全文由导论、主体、结语三部分构成。导论说明选题背景意义、国内外研究现状、研究思路与研究方法；主体分为新时代青年精神内耗的内涵与表征、多维成因、化解路径三个核心模块；结语对全文进行总结，指出研究不足并展望未来方向。"));
        b.Append(H.MakeH2("1.4 论文的研究方法"));
        b.Append(H.MakeBodyPara("本文主要采用三种研究方法：一是文献研究法，系统整理与分析CNKI、万方等数据库中关于青年精神内耗的期刊论文、学位论文；二是跨学科研究法，融合思想政治教育学、心理学、社会学、教育学等学科理论与视角；三是归纳分析法，对文献中的核心观点、成因框架、对策路径进行分类、提炼与整合。"));

        b.Append(H.MakeH1("2 新时代青年精神内耗的内涵界定与现实表征"));
        b.Append(H.MakeH2("2.1 核心内涵界定"));
        b.Append(H.MakeBodyPara("\u201c精神内耗\u201d并非严格的学术概念，而是在社会生活中形成、被学界广泛使用的描述性概念。综合现有文献，其核心内涵可概括为：个体在内在思想矛盾、心理冲突与外部环境压力相互作用下，因自我调节失效、价值认知失衡而引发的心理资源持续消耗、精神动力不断衰减的负面精神状态。这一状态具有长期性、反复性、内在性特征，会直接影响情绪体验、行为选择与价值判断。李千惠（2025）从思想政治教育视角指出，精神内耗不仅是心理问题，更反映青年在世界观、人生观、价值观上的缺失与冲突。刘萍（2025）强调，精神内耗源于内在冲突、过度自我审视与情绪调节失效，典型表现为心理疲惫、决策困难、行动力下降。唐会君（2024）认为，精神内耗是个体受到外部刺激后无法实现内心自洽，在反复纠结与心理挣扎中消耗精神能量。综合学界观点，新时代青年精神内耗具有心理性、思想性、社会性三重属性，是个体成长困境与社会发展阶段性问题的集中体现。"));
        b.Append(H.MakeH2("2.2 新时代青年精神内耗的现实表征"));
        b.Append(H.MakeH3("2.2.1 心理情绪层面：焦虑纠结与自我否定"));
        b.Append(H.MakeBodyPara("青年在心理上常处于矛盾拉扯状态，表现为对过去反刍、对未来忧虑，难以活在当下。面对学业、就业、人际等问题时容易过度担忧，产生紧张、不安、烦躁等情绪；在自我认知上倾向于自我贬低、自我怀疑，习惯用他人标准评判自己，形成\u201c敏感\u2014自卑\u2014内耗\u201d的循环。"));
        b.Append(H.MakeH3("2.2.2 行为选择层面：拖延犹豫与行动弱化"));
        b.Append(H.MakeBodyPara("精神内耗最典型的行为表现是\u201c想得多、做得少\u201d。面对选择反复权衡、害怕失误、恐惧失败，导致决策困难；在学习与生活中出现明显拖延，任务越重要越逃避，最终形成\u201c拖延\u2014焦虑\u2014更拖延\u201d的恶性循环；部分青年出现社交回避，害怕评价、恐惧冲突，呈现\u201c社恐\u201d\u201c沉默\u201d\u201c疏离\u201d等状态。"));
        b.Append(H.MakeH3("2.2.3 价值认知层面：迷茫空虚与信念不足"));
        b.Append(H.MakeBodyPara("部分青年在多元思潮冲击下出现价值迷茫，人生目标模糊、奋斗动力不足，陷入\u201c意义焦虑\u201d；在功绩社会与功利主义影响下，将成功简单等同于成绩、学历、收入、地位，形成单一评价标准，一旦达不到预期便产生强烈挫败感；一些青年受虚无主义影响，对理想信念、责任担当、集体价值认同不足，精神世界缺乏稳定支撑。"));
        b.Append(H.MakeH3("2.2.4 社会适应层面：压力过载与发展失衡"));
        b.Append(H.MakeBodyPara("青年在快速变化的社会环境中适应压力增大，难以平衡理想与现实、个人与他人、自由与责任的关系；在高强度竞争下容易出现身心疲惫、生活节奏紊乱、抗挫折能力弱等问题；部分青年陷入\u201c卷不动又躺不平\u201d的两难困境，长期处于精神紧绷与无力改变的消耗状态，影响健康成长与全面发展。"));

        b.Append(H.MakeH1("3 新时代青年精神内耗的多维成因文献梳理"));
        b.Append(H.MakeH2("3.1 社会环境：转型压力与评价单一的外部挤压"));
        b.Append(H.MakeH3("3.1.1 社会加速与竞争压力加剧"));
        b.Append(H.MakeBodyPara("社会快速转型带来发展节奏加快，教育、就业、婚恋、住房等全周期压力叠加，使青年长期处于紧张状态。功绩社会强调高效、优秀、成功，形成单一化成功标准，青年被裹挟进持续竞争，产生\u201c不进则退\u201d的生存焦虑，不断自我施压，造成精神资源大量消耗。"));
        b.Append(H.MakeH3("3.1.2 多元思潮冲击与价值迷茫"));
        b.Append(H.MakeBodyPara("市场经济发展带来思想文化多元化，功利主义、享乐主义、虚无主义等不良思潮在一定范围内传播，影响青年价值判断。部分青年理想信念淡化、责任意识弱化、精神追求功利化，在价值选择中摇摆不定，内心缺乏稳定支撑，容易陷入迷茫与内耗。"));
        b.Append(H.MakeH3("3.1.3 不确定性增强与发展焦虑"));
        b.Append(H.MakeBodyPara("社会结构变化、行业迭代加速、未来预期不确定性提升，使青年对前途充满担忧。就业压力、阶层流动感受、发展空间焦虑相互叠加，形成弥散性社会心态，成为精神内耗的重要外部根源。"));
        b.Append(H.MakeH2("3.2 高校教育：思政引领与心理支持的供给不足"));
        b.Append(H.MakeH3("3.2.1 思想政治教育引领效能有待提升"));
        b.Append(H.MakeBodyPara("部分高校思政课存在重理论灌输、轻实践体验，重知识传授、轻价值引领的问题，对青年真实精神需求、心理困惑回应不够及时充分，在理想信念塑造、心态调节、人生规划指导等方面的针对性与实效性有待加强。"));
        b.Append(H.MakeH3("3.2.2 心理健康服务体系不够健全"));
        b.Append(H.MakeBodyPara("一些高校心理健康教育覆盖面不足、师资力量薄弱、预警与干预机制不完善，工作重心偏向问题矫治而非预防引导，难以在早期介入并缓解青年内耗情绪，导致小困扰逐渐演变为严重心理压力。"));
        b.Append(H.MakeH3("3.2.3 育人协同机制尚未完全形成"));
        b.Append(H.MakeBodyPara("思政教育、心理健康教育、专业教育、就业指导、人文关怀之间存在壁垒，未能形成全过程、全方位、全员育人格局，对青年精神困扰的系统性支持不足，难以从根源上化解内耗产生的现实土壤。"));
        b.Append(H.MakeH2("3.3 数字媒介：算法困局与焦虑传播的放大效应"));
        b.Append(H.MakeH3("3.3.1 信息茧房固化认知偏差"));
        H.AddBodyParaWithFn(b, "算法推荐技术使用户长期处于同质化信息环境中，视野受限、认知片面，负面情绪与极端观点被不断放大，加剧自我封闭与认知失衡，进一步强化内耗倾向", fnId++, H.FN2());
        H.AddRunToLastPara(b, "。");
        b.Append(H.MakeH3("3.3.2 虚拟社交异化与比较焦虑"));
        b.Append(H.MakeBodyPara("社交媒体呈现\u201c完美生活\u201d滤镜，引发大量无意识社会比较，使青年产生\u201c别人都很好，只有我很差\u201d的错觉，导致自我否定、自卑焦虑；虚拟交往削弱现实社交能力，造成孤独感与疏离感，与内耗相互强化。"));
        b.Append(H.MakeH3("3.3.3 流量逻辑驱动焦虑蔓延"));
        b.Append(H.MakeBodyPara("部分网络平台为追求流量刻意制造焦虑议题、放大负面情绪，形成\u201c焦虑生产\u2014传播\u2014感染\u2014群体内耗\u201d的循环，对青年心态产生持续冲击。"));
        b.Append(H.MakeH2("3.4 家庭影响：期待压力与沟通缺失的内在张力"));
        b.Append(H.MakeH3("3.4.1 过高期望带来心理负担"));
        b.Append(H.MakeBodyPara("许多家庭对子女寄予较高期待，过度关注成绩、前途、体面工作，使青年长期处于\u201c被评价\u201d\u201c被期待\u201d状态，害怕失败、害怕辜负家人，形成持续心理压力与自我苛责。"));
        b.Append(H.MakeH3("3.4.2 情感沟通不足与支持缺乏"));
        b.Append(H.MakeBodyPara("部分家庭缺乏有效情感交流，青年内心困惑、压力与负面情绪难以倾诉，只能向内积压，逐渐转化为自我攻击与精神内耗。"));
        b.Append(H.MakeH3("3.4.3 教育方式影响心理韧性"));
        b.Append(H.MakeBodyPara("过度保护或严厉管教容易导致青年自我认知脆弱、抗压能力不足，面对挫折易陷入消极思维，难以快速调整心态，加剧内耗。"));
        b.Append(H.MakeH2("3.5 个体自身：认知偏差与能力不足的内在短板"));
        b.Append(H.MakeH3("3.5.1 自我认知失衡"));
        b.Append(H.MakeBodyPara("青年容易出现两种极端：过度自卑、自我否定，或完美主义、自我苛责，难以客观认识与接纳自己，在\u201c理想自我\u201d与\u201c现实自我\u201d的差距中持续内耗。"));
        b.Append(H.MakeH3("3.5.2 价值观尚未成熟稳定"));
        H.AddBodyParaWithFn(b, "青年处于价值观塑形关键期，易受外界干扰，缺乏坚定价值内核，面对多元选择与复杂环境容易摇摆迷茫，消耗大量精神能量", fnId++, H.FN12());
        H.AddRunToLastPara(b, "。");
        b.Append(H.MakeH3("3.5.3 心理韧性与行动力不足"));
        b.Append(H.MakeBodyPara("情绪调节能力弱、抗挫折能力不足，遇到困难易陷入消极反刍；习惯于过度思考而缺乏行动，用\u201c想\u201d代替\u201c做\u201d，形成内耗闭环。"));
        b.Append(H.MakeH3("3.5.4 精神动力与意义感匮乏"));
        b.Append(H.MakeBodyPara("缺乏长远目标与理想追求，生活被动应付，难以从学习、奋斗与实践中获得意义感与价值感，容易陷入无目的、无动力的精神空虚状态。"));

        b.Append(H.MakeH1("4 新时代青年精神内耗化解路径的文献整合"));
        b.Append(H.MakeH2("4.1 强化价值引领，筑牢青年精神内核"));
        b.Append(H.MakeBodyPara("以思想政治教育为核心，将社会主义核心价值观融入青年成长全过程，引导青年树立正确世界观、人生观、价值观，增强价值定力；弘扬中华优秀传统文化、革命文化与社会主义先进文化，滋养青年心灵，培育理性平和、积极向上的心态；加强主流意识形态引导，抵制不良思潮，为青年提供稳定精神支撑。"));
        b.Append(H.MakeH2("4.2 优化高校育人体系，提升教育支持效能"));
        b.Append(H.MakeBodyPara("深化思政教育改革，创新教学方法，增强亲和力与针对性，将心理疏导、人生规划、价值引导融入课程教学；健全心理健康教育服务体系，完善筛查、预警、干预、转介机制，开展团体辅导、正念训练、压力管理等活动；构建协同育人格局，推动思政、心理、专业、就业、管理服务深度融合，形成全方位支持系统。"));
        b.Append(H.MakeH2("4.3 规范数字媒介生态，阻断焦虑传播链条"));
        b.Append(H.MakeBodyPara("加强网络空间治理，遏制焦虑营销、负面炒作、流量至上等现象，营造清朗网络环境；提升青年数字素养，引导理性上网、辨别信息、突破信息茧房、减少盲目攀比；鼓励平台生产正向内容，用温暖、励志、治愈的文化产品消解负面情绪，构建健康数字生态。"));
        b.Append(H.MakeH2("4.4 完善社会支持体系，营造宽松成长环境"));
        b.Append(H.MakeBodyPara("落实就业优先、教育公平、住房保障等政策，缓解青年生存发展压力；破除单一成功标准，倡导多元评价、包容试错、理性平和的社会氛围；整合社区、社会组织、企业资源，提供实践平台、就业帮扶、心理服务与社交机会，增强青年归属感与安全感。"));
        b.Append(H.MakeH2("4.5 激发个体内生动力，实现自我成长突围"));
        b.Append(H.MakeBodyPara("引导青年正确认识自我、接纳自我，纠正完美主义、过度自我否定等认知偏差，实现自我和解；加强情绪管理与心理韧性培养，提升抗挫折能力；鼓励以微小行动打破拖延，树立目标、强化执行，在实践中获得成就感与意义感；保持规律作息、适度运动、现实社交，维护身心平衡，从根源上减少内耗。"));

        b.Append(H.MakeH1("5 研究述评与结语"));
        b.Append(H.MakeH2("5.1 研究述评"));
        b.Append(H.MakeBodyPara("现有研究已较为清晰地界定精神内耗内涵与表征，形成社会、学校、媒介、家庭、个体五维成因框架，并提出协同化、系统性化解路径，研究视角多元、成果丰富。但仍存在不足：实证研究偏少，大样本量化与追踪研究不足；对职场青年、农村青年、灵活就业青年等细分群体研究不够均衡；实践层面可操作、可复制的具体干预模式较少；跨学科深度融合与统一研究范式仍需加强。"));
        b.Append(H.MakeH2("5.2 未来展望"));
        b.Append(H.MakeBodyPara("未来可加强实证调研与指标体系构建，提升研究科学性；聚焦不同青年群体开展分类研究；深化实践干预研究，推出课程、活动、平台等可落地方案；推进跨学科深度融合，形成更具本土化的理论体系与治理路径。"));
        b.Append(H.MakeH2("5.3 结语"));
        b.Append(H.MakeBodyPara("新时代青年精神内耗是社会转型、技术变革与青年成长阶段性特征共同作用的综合性问题，兼具心理、思想与社会属性。其核心成因在于外部压力过载与内在价值失衡，其有效化解必须依靠社会、学校、家庭、个人协同发力。通过价值引领铸魂、教育赋能提质、媒介规范清障、社会支持托底、个体自觉内生，能够帮助青年走出内耗困境，培育自尊自信、理性平和、积极向上的精神状态，促进青年健康成长与全面发展，为实现中华民族伟大复兴注入持久青春动能。"));

        H.CloseSection(b, H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.OddPage, NumberFormatValues.Decimal, 1));

        // SECTION: References
        b.Append(H.MakeH1("参考文献"));
        H.AddReferences(b);
        H.CloseSection(b, H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.OddPage, NumberFormatValues.Decimal, null));

        // SECTION: Appendix
        b.Append(H.MakeAppendixTitle());
        H.CloseSection(b, H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.OddPage, NumberFormatValues.Decimal, null));

        // SECTION: Acknowledgments
        b.Append(H.MakeAckTitle());
        b.Append(H.MakeBodyPara("本论文的顺利完成，离不开指导教师的悉心指导与同学们的帮助。在论文写作过程中，我深入学习了文献研究法、跨学科研究法与归纳总结法等学术研究方法，对新时代青年精神内耗的成因与化解路径这一议题有了更系统的认识。同时也认识到自身在理论分析与实证研究方面尚存在诸多不足。今后将继续努力，不断提升自身的学术素养与研究能力，为青年思想政治教育与心理健康教育贡献自己的力量。感谢所有在论文写作过程中给予我支持与帮助的人。"));
        H.CloseSection(b, H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.NextPage, NumberFormatValues.Decimal, null));        // SECTION 9: Back cover + Grade table (final, sectPr in body)
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

    public static Paragraph MakeEngKeyPara(string keywords)
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

    public static string FN2() => "冯文博．从\u201c焦虑\u201d到\u201c治愈\u201d：数智时代青年精神内耗的表征、缘由与引导[J]．河北青年管理干部学院学报，2025,37(06)：20-26．";
    public static string FN12() => "林崇德．发展心理学[M]．北京：人民教育出版社，2018．";

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
        void AddRef(string text) { body.Append(new Paragraph(MakeRefPP(), new Run(MakeRP("SimSun", SZ_XIAOSI), new Text(text) { Space = SpaceProcessingModeValues.Preserve }))); }

        AddRef("[1] 李千惠．思想政治教育视角下大学生\u201c精神内耗\u201d现象透视及对策研究[D]．河北科技大学，2025．");
        AddRef("[2] 冯文博．从\u201c焦虑\u201d到\u201c治愈\u201d：数智时代青年精神内耗的表征、缘由与引导[J]．河北青年管理干部学院学报，2025,37(06)：20-26．");
        AddRef("[3] 王庆林．高校思政教育纾解学生精神内耗的多维价值与实践路径研究[J]．时代青年，2025(35)：90-92．");
        AddRef("[4] 刘萍．\u201c精神内耗\u201d困境下青年价值观引导研究[D]．西南财经大学，2025．");
        AddRef("[5] 沙田永．基于消解大学生精神内耗现象的生命观教育研究[D]．山东师范大学，2025．");
        AddRef("[6] 薛静，余洋．青年官兵精神内耗的表现、成因及应对策略[J]．政工学刊，2025(05)：78-79．");
        AddRef("[7] 陈鹤鸣．青年群体精神内耗的现实样态、成因分析及纠治策略[J]．新东方，2025(01)：55-61．");
        AddRef("[8] 唐会君．大学生精神内耗的现实表征、归因与教育引导研究[D]．华中师范大学，2024．");
        AddRef("[9] 荆德亭．青年\u201c精神内耗\u201d的多维透视及其应对策略[J]．思想教育研究，2023(12)：87-92．");
        AddRef("[10] 王乐乐，李伟．纠结与治愈：青年精神内耗的表征、根源与应对[J]．中国青年研究，2023(03)：40-47．");
        AddRef("[11] 张耀灿，郑永廷．现代思想政治教育学[M]．北京：人民出版社，2006．");
        AddRef("[12] 林崇德．发展心理学[M]．北京：人民教育出版社，2018．");
        AddRef("[13] 郑永廷．思想政治教育方法论[M]．北京：高等教育出版社，2017．");
        AddRef("[14] 樊富珉．大学生心理健康教育研究[M]．北京：清华大学出版社，2018．");
    }    public static Paragraph MakeAppendixTitle()
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

                // This run has underline \u2014 extend its spaces
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
                    // Run has mostly spaces with some text \u2014 extend trailing spaces
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
            ("中文题目", "新时代青年精神内耗的成因及化解路径文献论述", false),
            ("英文题目", "On the Causes and Solutions of Youth Spiritual Involution in the New Era: A Literature Review", true),
            ("姓名", "高艺宁", false),
            ("学号", "202402030207", false),
            ("班级", "2024级应用统计学2班", false),
            ("专业", "应用统计学", false),
            ("学院", "统计学院", false),
            ("指导教师", "高宇钊 讲师", false),
            ("完成时间", "2026年5月16日", false),
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
