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

    static string STUDENT_ID = "202310010115";
    static string GRADE => STUDENT_ID[..4];
    static string HEADER_TEXT => $"山西财经大学{GRADE}级本科生学年论文";

    static void Main(string[] args)
    {
        OUTPUT = Path.Combine(BASE, "output", "鲁南苏北红色文旅资源开发与汉文化共生.docx");
        var outDir = Path.GetDirectoryName(OUTPUT);
        if (outDir != null) Directory.CreateDirectory(outDir);

        using var doc = WordprocessingDocument.Create(OUTPUT, WordprocessingDocumentType.Document);
        mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(new Body());
        body = mainPart.Document.Body!;

        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        stylesPart.Styles = new Styles(H.MakeDocDefaults());
        stylesPart.Styles.Save();

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

        var hdrRoman = H.MakeHeader(mp, HEADER_TEXT, H.JC_CENTER);
        var hdrRomanId = mp.GetIdOfPart(hdrRoman);
        var ftrRoman = H.MakeFooter(mp, H.JC_CENTER);
        var ftrRomanId = mp.GetIdOfPart(ftrRoman);

        var hdrOdd = H.MakeHeader(mp, HEADER_TEXT, H.JC_RIGHT);
        var hdrOddId = mp.GetIdOfPart(hdrOdd);
        var hdrEven = H.MakeHeader(mp, HEADER_TEXT, H.JC_LEFT);
        var hdrEvenId = mp.GetIdOfPart(hdrEven);
        var ftrOdd = H.MakeFooter(mp, H.JC_RIGHT);
        var ftrOddId = mp.GetIdOfPart(ftrOdd);
        var ftrEven = H.MakeFooter(mp, H.JC_LEFT);
        var ftrEvenId = mp.GetIdOfPart(ftrEven);

        // SECTION 1: Cover + Explanation + Academic Pledge
        H.AddBlankPage(b); H.AddBlankPage(b); H.AddBlankPage(b);
        H.CloseSection(b, H.MakeSectPr(null, null, SectionMarkValues.OddPage, null, null));

        // SECTION 2: Chinese Abstract + English Abstract + TOC
        b.Append(H.MakeCenteredTitle("摘  要", "SimHei", "36", "360", "360"));
        b.Append(H.MakeBodyPara("鲁南苏北一带，既有很深厚的汉文化底蕴，也保留了不少红色革命记忆。这两种文化长期在同一个地方共存，给区域文旅发展带来了独特的资源条件。此文选了扬·阿斯曼的文化记忆理论和皮埃尔·诺拉的记忆场所理论当作分析工具，挑出汉王镇、台儿庄古城、亳州博物馆这三个典型案例，去研究红色文旅开发和汉文化传承之间的共生机制。结果表明，这三个案例分别形成了三种不同的共生模式：一种是叠合式叙事共生，一种是张力式戏剧共生，还有一种是隔代式转译共生。虽然它们在记忆主体、时间跨度、空间布局上各有不同，但都指向了同一个结论——红色文化和汉文化的共生，不是简单地把资源堆在一起，也不是把几条旅游线路串联，而是在物理空间和文化记忆层面进行了深度共构。这两者互为语境、互相支撑，在融合当中慢慢形成了新的意义系统。最后，这篇文章基于记忆叠层理论提出了一些区域文旅发展的具体策略，可以为省际交界地区多元文化遗产的整合与利用提供一点参考。"));
        b.Append(H.MakeKeyPara(true, "红色文旅；汉文化；文化记忆；集体记忆；共生机制"));
        H.AddPageBreak(b);
        b.Append(H.MakeAbstractTitle());
        b.Append(H.MakeAbstractBody());
        b.Append(H.MakeEngKeyPara("Red Cultural Tourism; Han Culture; Cultural Memory; Collective Memory; Symbiosis Mechanism"));
        H.AddPageBreak(b);
        b.Append(H.MakeCenteredTitle("目  录", "SimSun", "36", "360", "360"));
        H.AddTOC(b);
        H.CloseSection(b, H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.OddPage, NumberFormatValues.UpperRoman, 1));

        // SECTION 3: Body
        b.Append(H.MakeH1("1 导 论"));
        b.Append(H.MakeH2("1.1 研究背景与问题提出"));
        b.Append(H.MakeBodyPara("鲁南苏北地处苏皖鲁豫四省交界处，又是我国东部南北过渡带的重要区域，故而汉文化对这里影响极其明显：徐州为两汉文化的发源地，境内汉代遗迹星罗棋布，亳州以曹操宗族墓群及建安文化著称于世，台儿庄因京杭大运河而兴盛，南北商贸往来频繁，民俗文化南北交融。更难得的是，20世纪上半叶当地革命斗争十分活跃，淮海战役、台儿庄大战、铁道游击队、鲁南抗日民主政权诸种历史事件都发生于此，因此红色文化实为鲁南苏北区域文化中举足轻重、无可替代的一部分。"));
        b.Append(H.MakeBodyPara("楚汉文化、运河文化、黄河文化、红色文化、儒家文化、民俗文化相互交织，不可移动文物、历史遗迹、非物质文化遗产项目和传统村落数量众多，形成了辨识度很高的地域文化体系。不过长期以来，学界和行业对红色文化与汉文化的研究基本处于分开进行的状态，红色文化侧重革命精神传承与爱国主义教育，汉文化侧重遗产保护利用与文旅IP打造，在实际开发中常常是分开推进，“参观红色遗址、游览汉代景区”成为最常见的组合方式。两种文化资源是否存在更深层次的内在联系，怎样从简单并置走向真正共生，还需要更深入的理论探讨。"));
        b.Append(H.MakeBodyPara("这一问题之所以紧迫，不只是因为要整合资源，更关系到省际交界地区文旅整体竞争力的提升。苏皖鲁豫交界区域在文旅融合过程中，普遍存在协同不够、资源整合不足、品牌形象分散等问题，文化内涵挖掘不够深入，大多停留在观光层面，体验式、参与式、沉浸式产品供给不足，迫切需要从资源本身的内在联系出发，找到文化共生的深层逻辑，打造更有特色的区域文旅IP。"));
        b.Append(H.MakeBodyPara("由于鲁南苏北地区既有历史厚度又有着鲜明的革命印记，因此红色文旅开发与汉文化传承二者的关系到底是资源相互竞争、简单拼凑，还是共生共构，是一个值得厘清的问题。若确为共生关系，那么其赖以实现的机制是什么？不同空间载体所呈现的共生形态又有何具体差异？故而本文对这些问题作了系统、有层次的探索。"));

        b.Append(H.MakeH2("1.2 理论视角：文化记忆理论及其适用性"));
        b.Append(H.MakeBodyPara("本文对扬阿斯曼的文化记忆理论做了多层次的运用：阿斯曼把记忆划分为两类，即交往记忆，主要用于同代人之间的日常交流，有明确的时间边界，以及文化记忆，是以制度化、媒介化的方式固定下来的，故能跨越世代长期保存，文字、仪式、纪念建筑都可作为文化记忆的媒介。更重要的是，其根本目的是帮助社会建构集体认同、强化身份归属感。"));
        b.Append(H.MakeBodyPara("皮埃尔·诺拉提出的“记忆场所”理论，是从空间的角度给记忆研究做了补充。按照他的看法，现代社会里记忆跟历史已经慢慢分开了，那些原本自然而然延续下来的传统，得靠纪念馆、博物馆、遗址、档案这些具体空间，才能重新被唤醒。这类场所一方面承载着历史留下来的物质痕迹，另一方面也是当代社会重要的精神载体。这两个理论搭配起来用，为本文分析红色文化和汉文化怎么实现共生，提供了一个比较可靠的分析工具。"));
        b.Append(H.MakeBodyPara("将文化记忆理论应用到红色文旅及汉文化共生的研究中，有十分明确而突出的理论优势：其一，红色文化与汉文化属于两条并行的文化记忆谱系，共处同一空间但属于不同记忆主体，汉文化指向民族、古代，红色文化指向阶级、近现代，故二者的张力及融合是文化记忆在时空维度上复杂构成极好的例证。其二，文旅开发本身就是对文化记忆的再媒介化过程，旅游场景、表演仪式、叙事设计、数字技术诸种形式都用来把文献、遗址、民俗中所载之记忆转化为当代人可体验、可消费的文化产品。更重要的是，已有研究十分清楚地论证了红色旅游本身就是传承红色记忆的一种仪式，因此该思路也极其适用于红色文化与汉文化共生的分析。"));
        b.Append(H.MakeBodyPara("现有研究已经显示，文化记忆理论在红色文化研究中具有较强的解释力。有学者从功能记忆和存储记忆转化的角度提出，可以通过叙事转型、情感仪式设计、符号转译、沉浸式体验等方式，推动红色文化从存储记忆转向功能记忆。这项研究把这一理论框架从单一红色文化内部，拓展到红色文化与传统文化之间的跨类型记忆互动，分析多谱系文化记忆在文旅空间中的共生逻辑。同时也有学者以共生理论为基础，从共生单元、共生界面、共生环境三个方面构建红色旅游共生发展体系，为这项研究分析红色文化与汉文化的共生关系提供了方法参考，可以把两种文化看作文旅系统中的共生单元，把文旅开发活动看作二者的共生界面。"));

        b.Append(H.MakeH2("1.3 文化记忆与集体记忆的辨析"));
        b.Append(H.MakeBodyPara("为了之后要进行具体分析，因此有必要厘清文化记忆和集体记忆两个概念的关系：集体记忆是由法国社会学家哈布瓦赫系统、明确地提出的概念，他论证了记忆的社会建构性质，即记忆绝不是个体孤立的行为，而是社会框架中集体塑造、集体传承的结果。更具体地说，哈布瓦赫认为集体记忆以社会群体为载体，以代际口头传递、社会交往为基本方式延续，但无庸讳言，其理论没有直接解决记忆如何跨越数百年乃至上千年加以保存的问题。"));
        b.Append(H.MakeBodyPara("这一理论空白由扬·阿斯曼的文化记忆理论补上。阿斯曼明确提出，文化记忆是集体记忆的特殊形式，核心特征是经过制度化处理和媒介化固定的长时段集体记忆。二者的关系可以概括为：集体记忆包含交往记忆和文化记忆两个层次，交往记忆是代际口头传承的记忆，有一定时间限制；文化记忆依靠文本、仪式、纪念碑等媒介实现跨代传承，时间跨度可以长达千年。"));
        b.Append(H.MakeBodyPara("从这个角度看，红色文化同时具备交往记忆和文化记忆两种属性，亲历者的记忆正在逐渐淡化，更需要纪念馆、仪式、影视作品等媒介来固定和传承；汉文化则完全属于文化记忆，传承完全依靠文本、遗迹、博物馆等媒介化方式。"));
        b.Append(H.MakeBodyPara("这一理论区分对理解红色文化与汉文化的共生关系很重要，两种文化记忆虽然对应不同历史时期和记忆主体，但在依赖媒介、依托空间等方面有很高的相似性。也就是说，红色文化与汉文化的共生不只是资源上的共生，更是记忆方式上的共生。"));

        b.Append(H.MakeH2("1.4 案例选择与研究设计"));
        b.Append(H.MakeBodyPara("这项研究采用多案例比较的实证研究方法，选取汉王镇、台儿庄古城、亳州博物馆三个案例地。选择案例时兼顾最大差异取样和理论抽样两项原则，三个地点分别属于江苏、山东、安徽三省，资源类型分别是红色旧址与乡村文旅复合型、重大战争遗址型文旅古镇、博物馆型文化空间，在共生形态上表现出不同的文化叠层特点与融合路径。通过对比三个案例共生模式的异同，可以更系统地回答红色文化与汉文化以何种方式实现共生这一核心问题。"));

        // Section 2
        b.Append(H.MakeH1("2 鲁南苏北地区的文化地理与资源禀赋"));
        b.Append(H.MakeH2("2.1 作为记忆叠层的地理空间"));
        b.Append(H.MakeBodyPara("鲁南苏北位于淮河以北、黄河以南，地处华北平原东南边缘，自古以来就是南北文化交流融合的通道。从历史地理角度看，这片土地经历了多次文化叠加：先秦时期属于徐淮夷文化和齐鲁文化的边缘地带；汉代随着彭城也就是徐州的崛起，成为两汉文化的核心区域之一；魏晋以后，大运河开通带来南北商贸文化、漕运文化和移民文化不断融入；20世纪的革命浪潮，又在鲁南苏北留下鲜明的红色印记。"));
        b.Append(H.MakeBodyPara("由于多种文化在时间、空间上都互为叠层，故此区域的文化地理有十分明显、可辨析的记忆叠层特征：不同历史时期的文化遗存共存于同一空间，既有连续传承，又必然有矛盾冲突。楚汉文化、运河文化、黄河文化、红色文化、儒家文化、民俗文化诸种文化类型绝不是彼此孤立的资源单元，而是历史上彼此渗透、彼此塑造的结果。"));
        b.Append(H.MakeBodyPara("鲁南苏北这片区域，文化堆叠的分布并不均匀，空间上呈现出比较明显的差异化格局。徐州是两汉文化的发源地，同时也是淮海战役的核心战场，红色文化和汉文化在空间上的重叠程度是最高的；台儿庄地区靠着大运河带来的商贸基础，再加上1938年那场大战留下的创伤记忆，形成了运河文化和红色文化深度交织的特点；亳州则是建安文化和老庄哲学的一个重要承载地，红色印记相对弱一些，红色文旅的开发更多是以爱国主义教育基地挂牌、把革命文物故事融入展示这些方式为主。这样一种空间格局，正好为做比较案例研究提供了比较理想的样本。"));
        b.Append(H.MakeBodyPara("从文化地理学的角度看待鲁南苏北的文化叠层，即它绝不是个别现象，而是省际交界地带文化形成的普遍规律。因为该地区长期处于行政边界的边缘，故同时受周边诸种核心文化圈的辐射影响，又保留自身文化特色，因此形成了多条文化谱系交错共存的文化生态，这也是当地文旅融合之复杂性所在，亦是其文化价值之所在。"));

        b.Append(H.MakeH2("2.2 红色文化与汉文化的双重资源格局"));
        b.Append(H.MakeBodyPara("红色文化资源方面，鲁南苏北地区的红色遗存类型丰富、层级完整、分布密集，覆盖从抗日战争到解放战争的全过程，包括重大战役遗址、革命政权旧址、重要革命人物活动地、烈士陵园、纪念馆群等多种类型。台儿庄古城深入挖掘115师运河支队和大战文化资源，推出九大类研学精品课程，先后成为全国中小学生研学实践教育基地，有五十多家高校在古城设立研学基地。"));
        b.Append(H.MakeBodyPara("汉文化资源方面，鲁南苏北同样是两汉文化遗产的重要分布区域。徐州拥有狮子山楚王陵、龟山汉墓、汉画像石馆等重量级汉代遗存，是国内汉文化旅游的核心城市。亳州有曹操宗族墓群，馆藏珍贵文物超过五千件，建筑具有浓郁汉代风格，现为国家AAAA级旅游景区和国家二级博物馆。铜山古称大彭氏国，拥有五千多年历史，文化类型多元，彭祖文化、儒家文化、两汉文化、红色文化、山水文化在此交融，文物遗存数量多，名胜古迹有八十多处。"));
        b.Append(H.MakeBodyPara("红色文化与汉文化在空间上绝不是彼此割裂、互不干扰，而是有十分明确、可感知的空间重叠，徐州铜山区汉王镇便是一个极其典型的示范：当地因汉高祖刘邦得名，且有公元前205年楚汉相争时刘邦绝境拔剑、石裂泉涌、振奋军心的典故，故拔剑泉的传说流传千年。更重要的是，此处又是渡江战役总前委旧址，因此汉代传说与红色记忆在此交相辉映、彼此生辉。"));
        b.Append(H.MakeBodyPara("空间重叠绝非偶然，历史上地理位置重要的区域，本就由于战略价值而成为不同时期权力表达、军事行动的交汇点。汉王镇所在的云龙湖周边历来是徐州西南方向的军事要地，刘邦在此屯兵、刘邓大军在此筹划渡江，皆与此处的战略位置有直接关系。换言之，此种历史地理的特殊性，本身就是红色文化与汉文化空间共生最自然、最有力的物理基础。"));

        b.Append(H.MakeH2("2.3 共生理论的引入"));
        b.Append(H.MakeBodyPara("共生最早是生物学概念，指不同生物物种之间形成互利共存的生态关系。在文旅研究领域，有学者借用这一概念分析文化产业与旅游产业的融合发展，提出文化产业与旅游产业跨界共生、赋能融合，能够有效推动异质性产业要素集聚整合与顺畅流通，进而实现原有产业价值链的重塑与创新。这项研究进一步把这一框架用到文化类型之间的共生研究，把红色文化与汉文化看作文旅系统中的两个共生单元，把文旅开发实践看作二者的共生界面，区域社会文化环境则是共生环境。这一理论框架有助于系统分析两种文化在文旅开发中的互动逻辑与发展路径。"));
        b.Append(H.MakeBodyPara("从共生理论出发将红色文化与汉文化共生关系划分为三个明确、有层次的层面：第一是资源共生，即两种文化在物理空间上彼此共存，因而成为文旅产品组合开发的物质基础。第二是叙事共生，即两种文化记忆在文旅开发中形成互文关系，以共同主题加以联结，彼此互为阐释。第三是价值共生，在国家认同、地方认同、文化自信的构建中二者能形成功能互补的关系。"));

        // Section 3
        b.Append(H.MakeH1("3 汉王镇案例：记忆叠合中的叠合式叙事共生"));
        b.Append(H.MakeH2("3.1 汉文化与红色文化的空间叠合"));
        b.Append(H.MakeBodyPara("汉王镇位于江苏省徐州市铜山区西南部，东接铜山镇，东南邻三堡镇，南和西南与安徽省淮北市段园镇接壤，区域面积63.93平方千米。镇名本身就带有浓厚的汉文化渊源，汉王二字直接指向汉高祖刘邦。据当地传说，公元前205年楚汉相争时，刘邦军队行至此处，人困马乏、缺水断粮，刘邦拔剑刺地，泉水喷涌而出，三军士气大振，拔剑泉由此得名，流传至今已有两千多年。拔剑泉不但是汉王镇的地理标志，也是当地汉文化记忆最直观的物质载体，既是地理景观，也是被赋予传奇色彩的汉王叙事的实物锚点。围绕这一核心符号，汉王镇积累了丰富的汉代传说与民间记忆。"));
        b.Append(H.MakeBodyPara("与此同时，汉王镇也承载着厚重的红色记忆。北望村的郝家大院是渡江战役总前委旧址，由三座清代古民居组成，总占地面积九百多平方米，原有清代建筑一百余间，现存六十多间，被古建筑学界认定为徐州乡间现存规模最大的古建筑群。1949年2月到3月，邓小平、刘伯承、陈毅、粟裕、谭震林等老一辈无产阶级革命家在此运筹帷幄，研究制定渡江战役方案，《京沪杭战役实施纲领》《第三野战军渡江作战预备命令》在此酝酿形成，为渡江作战和解放全国奠定了重要基础。淮海战役胜利后，1949年1月15日，中国人民解放军第三野战军移驻大北望村，并在郝家大院设立指挥部，这里成为渡江战役前期决策和部队整编的核心场所。"));
        b.Append(H.MakeBodyPara("汉文化与红色文化在汉王镇的叠合，不只是空间上共处一地，更是深层意义上的同根同源。拔剑泉绝处逢生的传说，和渡江战役运筹帷幄的历史，时间相隔两千年，叙事结构却十分相似，都是军事危急时刻的关键转折，都是军事领袖在生死关头做出正确决断。这种叙事结构上的同源性，为两种文化记忆实现叠合式共生提供了内在动力。"));

        b.Append(H.MakeH2("3.2 在叠合中建立意义关联"));
        b.Append(H.MakeBodyPara("由于汉王镇的文化共生实践有明显的叠合式特点，故可引入叠合式叙事共生的概念，即两种文化记忆在同一物理空间、同一叙事框架中无缝融合，且以同一叙事主题彼此联结。"));
        b.Append(H.MakeBodyPara("汉王镇的共生实践拥有的明确特点在于，并未把汉文化、红色文化割裂呈现，而是以统一的军事重镇叙事框架把二者天然地融合在一起。刘邦拔剑泉的传说讲的是扭转战局的军事奇迹，渡江战役总前委旧址讲的是决定国家命运的军事谋划，两者都围绕将领运筹、转危为安展开叙事，又处在同一英雄叙事的语境之中，故而彼此形成意义呼应。因此，此种叠合式共生是最稳定、最自然的共生类型，因为两种文化记忆在叙事结构上本有同源性，故文旅开发时毋需刻意嫁接，只需挖掘共同的主题内核即可自然融合。"));

        // Section 4
        b.Append(H.MakeH1("4 台儿庄古城案例：创伤记忆与运河文脉的张力式戏剧共生"));
        b.Append(H.MakeH2("4.1 抗战文化作为红色记忆的核心"));
        b.Append(H.MakeBodyPara("如果说汉王镇的共生模式是叠合式，台儿庄古城则表现出更具张力的共生形态，也就是张力式戏剧共生。台儿庄古城的文化底色十分复杂，这里既是京杭大运河沿线的商贸重镇，被誉为天下第一庄，拥有水街水巷、运河码头、会馆商铺等完整的运河商贸文化体系；同时也是1938年台儿庄大战的发生地，千年古城在战火中被毁，成为中华民族抗战史上不朽的精神丰碑。"));
        b.Append(H.MakeBodyPara("台儿庄大战发生在1938年3月至4月，是抗战初期中国军队取得的重大胜利。中国军队以三万将士牺牲的代价歼灭日军一万两千人，影响全国抗日形势，震惊世界，被誉为中华民族扬威不屈之地。这场战役让台儿庄在中国近现代史上留下不可磨灭的红色印记。台儿庄的红色旅游，不仅承载着当地人民在中国共产党领导下反抗侵略、争取民族解放的珍贵记忆，也体现出国共两党合作、维护民族统一、致力于民族复兴的时代内涵。大战遗址至今清晰可见，大战遗址公园和清真寺西小讲堂的墙体上，战争留下的弹孔依然密集，成为这段历史最震撼人心的无声见证。"));

        b.Append(H.MakeH2("4.2 共生中的张力与融合"));
        b.Append(H.MakeBodyPara("和汉王镇汉文化与红色文化叙事结构相似不同，台儿庄的两种文化记忆之间存在更复杂的张力关系。运河文化代表商贸繁荣、南北交融、生活安宁的和平景象，抗战文化代表战火纷飞、家国危难、壮烈牺牲的创伤景象，两种景象在同一座古城空间里并存，并非天然和谐，而是在戏剧性冲突中形成特殊的共生力量。"));
        b.Append(H.MakeBodyPara("在具体共生实践上，台儿庄古城采取红色为底色、运河为本色的策略。一方面，把抗战文化作为古城最重要的精神底色，红色文化是台儿庄古城最鲜明的精神标识。景区以台儿庄大战遗址公园为核心，推出红色研学、实景演艺、沉浸式讲解等产品，《台儿庄 1938》通过实景还原、演员互动、场景递进的方式，再现烽火岁月里的民族气节与抗争精神，让红色文化走出书本、走进现场，让历史变得可感可知。"));
        b.Append(H.MakeBodyPara("另一方面，把传统文化和非遗元素大量融入红色叙事。鲁南山花皮影第四代传承人陈守科创作出以台儿庄大战为背景的皮影戏《莫忘 1938》，把传统皮影戏和红色题材结合难度较大，唱腔、唱词都经过创新，从方言改为普通话，都是为了更好地传播。复兴楼四楼剧场的《台儿庄大战》全息剧目，利用全息成像技术让历史人物走到观众面前。东门城墙上的缘梦台儿庄古城光影夜游项目，打造全国首个以城墙为载体的红色文化光影展演体系，实现运河商贸盛世与浴血抗战场景在同一空间的时空对话。"));

        // Section 5
        b.Append(H.MakeH1("5 亳州博物馆案例：隔代式转译共生"));
        b.Append(H.MakeH2("5.1 博物馆作为两种记忆的交汇空间"));
        b.Append(H.MakeBodyPara("亳州博物馆位于安徽省亳州市谯城区，占地2.5万平方米，建筑面积5200平方米。博物馆建筑为仿汉城堡样式，主体建筑坐北朝南，大门采用汉阙形制，整体汉风巍峨，掩映在绿树翠竹之间。基本陈列《穿越五千年——亳州文化寻源》分为涡河文明、商汤都亳、道源圣地、汉魏风骨、天下望州、亳商市井、近现代亳州七个专题，共展出文物七百多件套，按照历史发展脉络，展现亳州深厚的文化底蕴与多样的文物遗存。"));
        b.Append(H.MakeBodyPara("亳州是国家历史文化名城，一座既古老又充满活力的城市。涡河两岸先后孕育出老子、庄子、曹操、华佗等上百位文治武功、名留青史的先贤人物。汉魏风骨是亳州文化的核心标志，曹操宗族墓群出土的珍贵文物、曹魏建安文化的深厚底蕴，构成亳州博物馆最主要的展陈内容和最鲜明的文化符号。"));
        b.Append(H.MakeBodyPara("亳州博物馆把红色记忆纳入展陈体系之中，以近现代亳州专题形式系统、有层次地展示亳州革命史，即抗日战争、解放战争时期的重要事件及相关英雄人物事迹，因此亳州博物馆先后荣获国家二级博物馆、长三角及全国部分省市百佳公共文化空间、安徽省爱国主义教育示范基地、省社会科学知识普及基地、省国防教育基地诸种称号。换言之，亳州博物馆在功能上真正担负起历史文化展示与爱国主义教育的双重使命，前者为汉文化的传承传播，后者为红色记忆的固定、激活。"));

        b.Append(H.MakeH2("5.2 隔代对话的意义生产"));
        b.Append(H.MakeBodyPara("亳州博物馆在红色文化和汉文化的融合实践里头，表现出比较明显的隔代式共生特征，具体可以从两个方面来说。"));
        b.Append(H.MakeBodyPara("一方面，馆里的整体展陈是按照历史发展的脉络来走的，把红色内容放在了叙事的收尾部分。基本陈列《穿越五千年——亳州文化寻源》是从涡河文明开始讲起，接着依次展示商汤文化、道家文化、汉魏文化和明清商业文化，最后用近现代亳州的革命历程来收尾。这么安排叙事，里头其实隐含了一层递进关系：亳州悠久的古代文明给这座城市攒下了扎实的文化根基和精神内涵，而近现代的革命斗争，可以看作是这条文化脉络在新的历史条件下的一种延续和提升。两种不同时代的记忆被放在同一条历史轴线上，由此形成了一种由古代文明打基础、近现代革命接续传承的意义体系。"));
        b.Append(H.MakeBodyPara("另一方面，馆内空间符号以汉文化风格为主，与红色记忆结合起来：博物馆本身为仿汉城堡式建筑，汉阙大门是鲜明、有力的标志性形象，入口处即有商汤、曹操、华佗诸人的雕像，由此极自然地引出亳州地域文化名人谱系。更难得的是，红色教育的内容被有机、细腻地嵌入于汉文化空间之中，爱国主义教育示范基地、国防教育基地诸种功能定位都成为汉文化体验过程中顺理成章的一部分，观众在感受汉文化氛围时便潜移默化地接受了红色教育。因此其空间层面的共生方式堪称绝妙，毋需为红色记忆单独划出展示区域，只需依托汉文化空间，在汉风审美的体验中自然而然地叠加红色教育内容。"));
        b.Append(H.MakeBodyPara("从文化记忆理论的角度来看，隔代式转译共生的核心运行机制，是借助一个历时性的叙事结构，来消解不同文化记忆在共时状态下可能产生的张力。像汉王镇和台儿庄古城这两个案例，红色文化和汉文化属于同一空间里并存的两类记忆，它们之间的张力得靠专门的叙事整合手段去协调。但在亳州博物馆这边，古代文化记忆和红色记忆被统一纳入从古代到近现代的历史发展脉络当中，用传承与发展的叙事逻辑，把潜在的冲突给弱化了。这也是隔代式共生最有代表性的特点——博物馆依托自身作为历史梳理和展示空间的独特属性，把不同时期的记忆片段串联成了一条完整的地方文明发展历程。"));

        // Section 6
        b.Append(H.MakeH1("6 比较分析：三种共生模式的异同与记忆叠层理论"));
        b.Append(H.MakeH2("6.1 共生类型的比较框架"));
        b.Append(H.MakeBodyPara("通过对三个案例的深入分析，可以总结出红色文化与汉文化共生的三种典型模式，三种模式在多个维度上存在结构性差异。"));
        b.Append(H.MakeBodyPara("1）叠合式叙事共生（汉王镇）"));
        b.Append(H.MakeBodyPara("叠合式叙事共生，是指两种文化记忆在同一空间里高度契合，依托相近的叙事逻辑自然融合，不需要刻意嫁接，就能在统一主题下形成相互呼应的效果。"));
        b.Append(H.MakeBodyPara("2）张力式戏剧共生（台儿庄）"));
        b.Append(H.MakeBodyPara("张力式戏剧共生，是两种文化记忆在气质上形成明显反差，通过和平与战争、繁荣与创伤的对比，制造出强烈的体验冲击力，在反差与对话中实现共生。"));
        b.Append(H.MakeBodyPara("3）隔代式转译共生（亳州博物馆）"));
        b.Append(H.MakeBodyPara("隔代式转译共生，是把不同时代的记忆放进一条完整的历史时间线中，以“传承—延续—升华”的逻辑串联，用历史纵深化解文化间的张力，实现平稳衔接。"));

        b.Append(H.MakeH2("6.2 共生机制的核心要素"));
        b.Append(H.MakeBodyPara("从对若干案例的对比分析中可以清楚地看到，红色文化和汉文化要形成比较成熟的共生状态，必然要有若干要素彼此配合、协同作用。"));
        b.Append(H.MakeBodyPara("一是空间叠合是共生关系形成的基础条件，三个案例有一个可被严格考察的共同特点：汉文化遗存及红色文化资源在物理空间上高度重合。具体而言，汉王镇拔剑泉传说的发生地与渡江战役总前委旧址在同一区域，台儿庄运河古城及大战遗址亦在同一城区范围内，毫州博物馆更是将汉魏文物与近现代革命历史资料置于同一展陈空间之中。更重要的是，此种空间重叠绝非偶然，而是区域历史地理特殊性的必然结果：地理位置重要的军事要地、交通枢纽、文化中心，历来就是不同历史阶段诸种历史叙事交汇、叠压之处。故而对文旅规划工作有极其直接而明确的启示：省际交界区域的红色文化、汉文化资源绝不是零散分布的，而是集中于有战略价值、交通便利的历史古镇及古城。因此，文旅线路的规划宜主动、充分地利用这类记忆叠层特征明显的区域作为核心节点。"));
        b.Append(H.MakeBodyPara("二是叙事整合是共生真正落地的根本机制，故而汉王镇英雄叙事结构上的种种相似性、台儿庄古城和平场景与战争记忆的鲜明对照、亳州博物馆以历史发展脉络将古今内容自然、妥帖地串联起来，都离不开一个统一、有力的叙事框架，也都要以该框架来统筹、衔接两种文化记忆的内涵。因此若没有合理的叙事整合，两种文化极易互相割裂，单纯的资源叠加绝不能让文化共生产生更切实、更丰厚的综合价值。"));
        b.Append(H.MakeBodyPara("三是体验创新对文化记忆的转化有直接、明确的促进作用，汉王镇沉浸式剧本杀、台儿庄古城夜间光影秀及红色主题皮影戏，以及亳州博物馆诸种公众研学课程、文创产品，都是红色文化与汉文化共生理念很好的具体落实：即红色文化绝不能停留在抽象叙事层面，必然要落到游客能感受、能参与、能体验的具体环节上。因此，把本来以静态形式存在的存储记忆转化为能与现实生活发生真实联结的功能记忆，最自然也最有效的方式就是采用形式更新颖、方式更活泼的体验设计。"));

        b.Append(H.MakeH2("6.3 理论启示：记忆叠层视角"));
        b.Append(H.MakeBodyPara("对众案例进行综合比对后不难发现，鲁南苏北地区红色文化与汉文化形成的共生关系，本质上可以被看作一种记忆叠层的现象。记忆叠层所指的，是不同历史阶段形成的文化记忆在同一地理空间中不断积淀，并在积淀的基础上完成资源整合与内容再生产，一处地域在漫长的历史进程中承载了多个时代的文化记忆，这些记忆并不会相互替代，而是以层层叠加、互为背景的方式存在，最终构成复合型的文化生态系统。"));
        b.Append(H.MakeBodyPara("从记忆叠层的角度可以有层次地考察红色文化与汉文化共生的关系：即二者绝不是人为把两种文化简单拼凑在一起，而是历史长期发展过程中自然形成的内在逻辑。汉文化是区域底层的文化记忆，故为这片土地打下最扎实、最根本的文化根基，而红色文化是叠加于汉文化之上的另一层记忆，在此基础上形成新的意义结构，此结构既继承汉文化原有的审美价值、符号资源，又让红色文化获得更丰厚的历史内涵，也因此获得更牢固、更自觉的地方认同基础。"));
        b.Append(H.MakeBodyPara("该理论视角对文旅实践有着逻辑严密的启示：第一，记忆叠层的特点决定了文旅开发中不能再把红色文化、汉文化当作两个要单独串联的景区资源来处理，而宜将二者放在同一个体验空间之中，作为并行呈现的不同文化层次加以设计。由此引出第二个启示，即研究者及开发者宜主动关注不同文化记忆衔接的界面，系统考察哪些空间节点更有利于两种记忆产生意义上的交汇、共鸣。汉王镇的拔剑泉、台儿庄的大战遗址公园、亳州博物馆的展陈空间都是此类节点极好的范例。更重要的是，记忆叠层相关策略的根本逻辑是对文化层次做整体性解读，绝不是单纯抽取某一层来做体验设计，因此游客在空间中所得的真实价值，绝不是孤立感受某一种文化记忆，而是在同一空间中感受不同文化层次彼此对话、交映生辉的现场感。"));

        // Section 7
        b.Append(H.MakeH1("7 结论与建议"));
        b.Append(H.MakeH2("7.1 主要结论"));
        b.Append(H.MakeBodyPara("这项研究以文化记忆理论作为分析基础，通过对汉王镇、台儿庄古城、亳州博物馆这三个案例的实地调研和实证分析，把鲁南苏北地区红色文旅开发与汉文化传承之间的内在共生逻辑给系统梳理了一遍，也对不同的共生模式做了比较清晰的归纳。从研究结果来看，红色文化和汉文化在这一区域形成了三种有代表性的共生路径：一个是汉王镇的叠合式叙事共生，一个是台儿庄古城的张力式戏剧共生，再一个是亳州博物馆的隔代式转译共生。这三种模式在空间形态、文化关系、内容呈现方式上各有各的特点和差异，但都完成了从资源简单并置，到意义层面深度共生的有效转变。"));
        b.Append(H.MakeBodyPara("从目前所论三种共生模式背后的基本逻辑可以十分清晰地看到，其都以记忆叠层为根本前提：不同历史阶段留下的文化记忆在同一个地理空间中层层沉积，又以叙事整合、体验创新、产业赋能诸种方式转化为有市场价值的文旅产品。因此空间叠合是共生关系形成的现实基础，叙事整合是促成共生真正落地的核心机制，体验创新是文化活化极好的抓手，产业赋能则是各项行动持续、健康发展的可靠保障。"));
        b.Append(H.MakeBodyPara("红色文化与汉文化的共生绝不是人为设计、生硬拼接的结果，而是长期历史发展过程中自然形成的内在文化逻辑：汉文化是鲁南苏北地区底层的文化记忆，因此是地方认同塑造、文化符号开发最坚实的基础，而红色文化作为上层记忆，又给地方文化注入了明确的时代精神及国家叙事的内涵。故此二者在同一空间内相互激活、彼此支撑，形成了鲁南苏北地区特色鲜明、辨识度极高的完整、健康的文化生态系统。"));

        b.Append(H.MakeH2("7.2 文旅开发启示"));
        b.Append(H.MakeBodyPara("基于以上研究，提出以下文旅开发建议。"));
        b.Append(H.MakeBodyPara("一是要树立文化共生的开发理念。省际交界地区得打破那种把红色文化和传统文化分开来搞开发的惯性思维，转向以记忆叠层为特点的整体开发模式。在文旅规划阶段，就要从资源普查入手，把汉文化和红色文化在空间上的重叠区域、在叙事上的互文点都识别出来，把这些地方当成文旅线路设计的核心枢纽。像苏皖鲁豫这一带的省际交界地区，搞文旅融合就得坚持规划先行，用系统性的布局去带动红色旅游的融合发展。"));
        b.Append(H.MakeBodyPara("二是要建设体验叠加型产品体系，宜借台儿庄古城“一日双节”设计思路之长，推出让游客在同一空间内感受多重文化记忆的旅游产品：即推出“汉风红韵”主题研学课程，创排汉文化符号与红色故事结合的演艺节目，设计汉服体验与红色教育基地彼此衔接的游览线路，由此让两种文化记忆在游客体验中自然交融。"));
        b.Append(H.MakeBodyPara("三是要系统地构建以记忆为轴心的品牌叙事，即让区域文旅品牌统一形成以汉文化历史底蕴、红色文化精神内涵为基本内容的“英雄之地、文明赓续”主题叙事，由此自然、妥帖地打造有感染力、有温度的品牌故事。毋庸讳言，省际交界地区目前最突出的短板就是缺乏统一的区域文旅IP及宣传标识，因此宜在记忆叠层达成共识的基础上，尽快塑造统一品牌形象。"));
        b.Append(H.MakeBodyPara("四是要有计划地建立跨区域协同发展机制，因为鲁南苏北地区横贯江苏、安徽、山东、河南四省，行政跨度很大，故宜探索建立区域性的文旅协同机制，促进跨省资源共享、线路共建、品牌共创。地方政府要主动、稳妥地做好文化资源的保护、管理、利用诸种工作，在规划衔接、利益分配、标准互认诸方面作出制度设计。"));

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
        b.Append(H.MakeBodyPara("本论文的写作过程是一次系统学习学术研究方法的宝贵经历。在导师的悉心指导下，我对鲁南苏北地区红色文旅与汉文化的共生关系有了更深入的认识，也初步掌握了文献研究、案例比较等基本方法。感谢导师在选题确定、资料搜集、论文修改各环节给予的耐心指导，感谢同学们在讨论交流中提供的启发与帮助。今后将继续努力，不断提升自身的学术素养与研究能力。"));
        H.CloseSection(b, H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.NextPage, NumberFormatValues.Decimal, null));

        // Back cover
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
                new Text("The southern Shandong and northern Jiangsu region possesses both deep-rooted Han cultural heritage and rich red revolutionary memories. The long-term coexistence of these two cultural systems provides unique resource conditions for regional cultural tourism development. This study employs Jan Assmann’s cultural memory theory and Pierre Nora’s sites of memory theory as analytical frameworks, selecting three typical cases—Hanwang Town, Taierzhuang Ancient City, and Bozhou Museum—to investigate the symbiotic mechanisms between red cultural tourism development and Han cultural inheritance. The findings reveal that the three cases have formed three distinct symbiotic models: superimposed narrative symbiosis, tension-based dramatic symbiosis, and cross-generational translational symbiosis. Although they differ in memory subjects, temporal spans, and spatial configurations, they all point to the same conclusion: the symbiosis of red culture and Han culture is neither a simple accumulation of resources nor a mere linkage of tourism routes, but rather a deep co-construction at the levels of physical space and cultural memory. The two systems serve as context and support for each other, gradually forming a new system of meaning through integration. Finally, based on the memory stratification theory, this paper proposes specific strategies for regional cultural tourism development, offering reference for the integration and utilization of diverse cultural heritage in inter-provincial border areas.") { Space = SpaceProcessingModeValues.Preserve }));
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

    public static string FN2() => "参考文献[3]：邵明华, 刘鹏.红色文化旅游共生发展系统研究——基于对山东沂蒙的考察[J].山东大学学报(哲学社会科学版), 2021(4).";
    public static string FN12() => "参考文献[4]：李卫飞, 方世敏, 阎友兵, 马丽君.红色旅游传承红色记忆的理论逻辑与动态过程[J].自然资源学报, 2021, 36(11): 2736-2747.";

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

    public static void AddReferences(Body body)
    {
        void AddRef(string text) { body.Append(new Paragraph(MakeRefPP(), new Run(MakeRP("SimSun", SZ_XIAOSI), new Text(text) { Space = SpaceProcessingModeValues.Preserve }))); }

        AddRef("[1] 郭旭冉.推进文旅深度融合 打造省际交界发展新高地——苏皖鲁豫省际交界地区文旅融合创新路径研究[N].淮北日报, 2026-04-27(A04).");
        AddRef("[2] 陈冲.全国媒体探秘“苏北第一区”的诗与远方[N].江南游报, 2025-11-20.");
        AddRef("[3] 邵明华, 刘鹏.红色文化旅游共生发展系统研究——基于对山东沂蒙的考察[J].山东大学学报(哲学社会科学版), 2021(4).");
        AddRef("[4] 李卫飞, 方世敏, 阎友兵, 马丽君.红色旅游传承红色记忆的理论逻辑与动态过程[J].自然资源学报, 2021, 36(11): 2736-2747.");
        AddRef("[5] 盆鑫雨.传统文化赋能 台儿庄古城以国潮为桥,让运河文脉活在当下[EB/OL].海报新闻, (2026-04-30).http://w.dzwww.com/sjb/index.php?c=ad&a=show&id=p2zLZGcEvGa.");
        AddRef("[6] 铜山汉王镇:数字赋能为基层减负 推动文旅产业高质量发展[EB/OL].新华日报, (2025-07-09).https://www.sohu.com/a/911986941_121455647.");
        AddRef("[7] 大皖新闻.“蛙·两季”农文旅项目火爆出圈,徐州汉王镇解锁乡村振兴新密码[EB/OL].(2025-11-13).http://m.ahwang.cn/newsflash/20251113/2933530.html.");
        AddRef("[8] 王雨萌.“浸”入红色记忆,莫忘1938[N].大众日报, 2025-10-05.");
        AddRef("[9] 王雨萌.台儿庄红色游,找到与历史对话的独特方式[EB/OL].(2025-10-05).https://www.jnnews.tv/yaowen/2025/10-05/brYlY2l1.html.");
        AddRef("[10] 亳州博物馆.亳州博物馆2025年实现“破圈”发展创新活化文化资源构建文旅融合新范式[EB/OL].(2025-12-18).https://www.bzbwg.com/page/3/3306.html.");
        AddRef("[11] 亳州市博物馆.让文物融入生活:亳州市博物馆文创空间正式开放[EB/OL].(2025-06-24).https://bzbwg.com/page/3/3213.html.");
        AddRef("[12] 王雨萌.沉浸式体验让红色记忆“活”起来[N].大众日报, 2025-10-05.");
        AddRef("[13] 陶梓童.探访“苏北第一区”徐州铜山:文旅与科技共舞,绘就高质量发展新图景[EB/OL].澎湃新闻, (2025-11-17).https://thirdpage.thepaper.cn/h5/jrtt/31981950.");
        AddRef("[14] 张扬, 李春元.徐州铜山:拓展博物馆功能 开拓文化新空间[EB/OL].中国江苏网, (2023-05-18).https://jsnews.jschina.com.cn/xz/a/202305/t20230518_3216867.shtml.");
        AddRef("[15] 枣庄市台儿庄区.枣庄市台儿庄区积极探索红色文化资源活化利用新路径[EB/OL].(2023-12-01).http://sdzzwm.com/mobile/news/show-24189.html.");
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
        foreach (var p in el.Descendants<Paragraph>())
        {
            var pText = p.InnerText;
            if (!pText.Contains("签名") && !pText.Contains("期") && !pText.Contains("指导教师"))
                continue;

            var runs = p.Elements<Run>().ToList();
            foreach (var r in runs)
            {
                var rp = r.Elements<RunProperties>().FirstOrDefault();
                if (rp == null) continue;
                var u = rp.Elements<Underline>().FirstOrDefault();
                if (u == null) continue;

                var t = r.Elements<Text>().FirstOrDefault();
                if (t == null) continue;

                var text = t.Text ?? "";
                if (text.Trim().Length == 0 && text.Length > 0)
                {
                    t.Text = new string(' ', Math.Max(24, text.Length * 2));
                }
                else if (text.Trim().Length > 0 && text.Contains(" ") && text.Length - text.Trim().Length > text.Trim().Length)
                {
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

        var children = tplBody.ChildElements.ToList();
        var sectPrIndices = new List<int>();
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

        if (sectPrIndices.Count < 2) { Console.WriteLine("Template doesn't have expected structure."); return; }

        int coverEnd = sectPrIndices[1];
        int backCoverStart = sectPrIndices[1] + 1;
        int backCoverEnd = bodySectPrIndex >= 0 ? bodySectPrIndex : children.Count;

        var imageMap = CopyImages(tplMain, outMain);

        var coverElements = new List<OpenXmlElement>();
        for (int i = 0; i <= coverEnd; i++)
        {
            var el = CloneWithoutSectPr(children[i], imageMap);
            if (el != null) { FixSignatureUnderlines(el); coverElements.Add(el); }
        }
        OpenXmlElement? firstChild = outBody.Elements<Paragraph>().FirstOrDefault()
            ?? (OpenXmlElement?)outBody.Elements<Table>().FirstOrDefault();
        if (firstChild != null)
        {
            foreach (var el in coverElements)
                outBody.InsertBefore(el, firstChild);
        }

        var bcElements = new List<OpenXmlElement>();
        for (int i = backCoverStart; i < backCoverEnd; i++)
        {
            var el = CloneWithoutSectPr(children[i], imageMap);
            if (el != null) bcElements.Add(el);
        }
        var finalSectPr = outBody.Elements<SectionProperties>().FirstOrDefault();
        foreach (var el in bcElements)
        {
            if (finalSectPr != null)
                outBody.InsertBefore(el, finalSectPr);
            else
                outBody.Append(el);
        }

        FillMajorCode(outBody, "120210");
        FillCoverTable(outBody);

        Console.WriteLine($"Merged {coverElements.Count} cover elements + {bcElements.Count} back cover elements");
    }

    static void FillMajorCode(Body outBody, string majorCode)
    {
        // Find the first paragraph containing "专业代码"
        var para = outBody.Elements<Paragraph>().FirstOrDefault(p => p.InnerText.Contains("专业代码"));
        if (para == null) return;

        var runs = para.Elements<Run>().ToList();
        for (int i = 0; i < runs.Count; i++)
        {
            var t = runs[i].Elements<Text>().FirstOrDefault();
            if (t == null || t.Text == null || !t.Text.Contains("专业代码")) continue;

            // Next run should be the underline placeholder
            if (i + 1 >= runs.Count) break;
            var nextRun = runs[i + 1];
            var nextText = nextRun.Elements<Text>().FirstOrDefault();
            if (nextText == null) continue;

            // Replace with centered major code (total width 15 chars, matching school code alignment)
            int totalWidth = 15;
            int padLeft = (totalWidth - majorCode.Length) / 2;
            int padRight = totalWidth - majorCode.Length - padLeft;
            nextText.Text = new string(' ', padLeft) + majorCode + new string(' ', padRight);
            nextText.Space = SpaceProcessingModeValues.Preserve;
            break;
        }
    }

    static void FillCoverTable(Body outBody)
    {
        var tbl = outBody.Elements<Table>().FirstOrDefault();
        if (tbl == null) return;

        var data = new (string label, string value, bool isEnglish)[] {
            ("中文题目", "鲁南苏北红色文旅资源开发与汉文化共生", false),
            ("英文题目", "Red Cultural Tourism Development and Han Cultural Symbiosis in Southern Shandong and Northern Jiangsu", true),
            ("姓名", "杨晓磊", false),
            ("学号", "202310010115", false),
            ("班级", "文化产业管理一班", false),
            ("专业", "文化产业管理", false),
            ("学院", "文化旅游与新闻艺术学院", false),
            ("指导教师", "高慧慧", false),
            ("完成时间", "2026年5月18日", false),
        };

        var rows = tbl.Elements<TableRow>().ToList();
        for (int i = 0; i < rows.Count && i < data.Length; i++)
        {
            var cells = rows[i].Elements<TableCell>().ToList();
            if (cells.Count < 3) continue;
            var targetCell = cells[2];

            targetCell.RemoveAllChildren<Paragraph>();

            var para = new Paragraph(
                new ParagraphProperties(
                    new SpacingBetweenLines { Line = "360", LineRule = LineSpacingRuleValues.Auto },
                    data[i].isEnglish
                        ? new Justification { Val = JustificationValues.Left }
                        : new Justification { Val = JustificationValues.Center }));

            if (data[i].isEnglish)
            {
                para.Append(new Run(
                    new RunProperties(
                        new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman" },
                        new Italic(),
                        new FontSize { Val = SZ_SIHAO }, new FontSizeComplexScript { Val = SZ_SIHAO }),
                    new Text(data[i].value) { Space = SpaceProcessingModeValues.Preserve }));
            }
            else
            {
                var text = data[i].value;
                var currentRun = new System.Text.StringBuilder();
                var isCurrentLatin = false;

                for (int j = 0; j < text.Length; j++)
                {
                    var c = text[j];
                    var isLatin = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')
                        || c == '/' || c == '-' || c == '.' || c == ' ';
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
                        para.Append(MakeCoverRun(currentRun.ToString(), isCurrentLatin));
                        currentRun.Clear();
                        currentRun.Append(c);
                        isCurrentLatin = isLatin;
                    }
                }
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

        foreach (var t in clone.Descendants<Text>())
        {
            t.Space = SpaceProcessingModeValues.Preserve;
            t.SetAttribute(new OpenXmlAttribute("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve"));
        }

        foreach (var rf in clone.Descendants<RunFonts>())
        {
            if (rf.Hint != null)
            {
                var hintVal = rf.Hint.Value;
                rf.SetAttribute(new OpenXmlAttribute("w", "hint", null, hintVal == FontTypeHintValues.EastAsia ? "eastAsia" : ""));
            }
        }

        if (hadSectPr)
        {
            ((Paragraph)clone).Append(new Run(new Break { Type = BreakValues.Page }));
        }

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
