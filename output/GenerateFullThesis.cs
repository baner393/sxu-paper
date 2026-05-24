using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ThesisGenerator
{
    class Program
    {
        const string STUDENT_ID = "202406020153";
        static string GRADE => STUDENT_ID[..4];
        static string HEADER_TEXT => $"山西财经大学{GRADE}级本科生学年论文";

        const int PAGE_WIDTH = 11906;
        const int PAGE_HEIGHT = 16838;
        const int MARGIN_TOP = 1701;
        const int MARGIN_BOTTOM = 1418;
        const int MARGIN_LEFT = 1418;
        const int MARGIN_RIGHT = 1134;

        static void Main(string[] args)
        {
            string outputPath = @"D:\360MoveData\Users\ban\Desktop\school_about\word_about\sxuyear\output\数智赋能语境下山西博物馆文创设计的AIGC创新路径_降AI.docx";

            using var doc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document(new Body());
            var body = mainPart.Document.Body;

            // 第1节：封面+说明+学术承诺
            body.Append(CreateEmptyPara());
            body.Append(CreatePageBreak());
            body.Append(CreateEmptyPara());
            body.Append(CreatePageBreak());
            body.Append(CreateEmptyPara());
            CloseSection(body, CreateSectionNoHeader());

            // 第2节：中文摘要+英文摘要+目录
            AddChineseAbstract(body);
            body.Append(CreatePageBreak());
            AddEnglishAbstract(body);
            body.Append(CreatePageBreak());
            AddTOC(body);
            CloseSection(body, CreateSectionWithHeader(mainPart, true));

            // 第3节起：正文
            AddChapter1(body);
            AddChapter2(body);
            AddChapter3(body);
            AddChapter4(body);
            AddChapter5(body);
            AddChapter6(body);
            AddReferences(body);
            AddAppendix(body);
            AddAcknowledgement(body);
            body.Append(CreatePageBreak());
            body.Append(CreateEmptyPara());

            // 最终 sectPr
            body.Append(CreateSectionWithHeader(mainPart, false));

            mainPart.Document.Save();
            Console.WriteLine($"Done: {outputPath}");
        }

        static void AddChineseAbstract(Body body)
        {
            body.Append(H1("摘  要"));
            body.Append(Para("国家推文化数字化这几年，博物馆文创确实从手工转向了智能设计。老路子问题不少：开发慢、长得差不多、文化转译总差点意思。现在市面上的 AIGC 方案大多要微调 LoRA，训练费时费钱。我选了山西博物院的晋侯鸟尊做案例，用 AI-CSD 文化符号驱动模型搭了一套不用训练的文创路径。先把鸟尊文化拆成三层，再用三轮关键词优化加 KJ 法聚类筛选，定下 \"鸟尊主题潮玩 + 子母鸟情感 IP\" 这个方向。接着用结构化提示词和 ComfyUI 工作流批量生成方案。后来找了 118 人做问卷评价 24 组设计，结果表明这条路子能在几分钟内出高质量设计，文化准确性、艺术表现和效率都顾上了，最好的方案综合评分 4.26。这套方法对地域博物馆文创的数智化转型有参考价值，也为传统文化 IP 的 AIGC 活化提供了新思路。"));
            body.Append(Keywords("关键词：", "数智赋能；AIGC；博物馆文创；晋侯鸟尊；AI-CSD 模型；ComfyUI"));
        }

        static void AddEnglishAbstract(Body body)
        {
            body.Append(H1("Abstract"));
            body.Append(Para("Driven by the national cultural digitalization strategy, the museum cultural and creative industry is undergoing a transformation from traditional manual design to digital-intelligent design. The traditional cultural and creative design model is facing pain points such as long development cycles, serious homogenization, and insufficient accuracy of cultural translation. However, most existing AIGC cultural and creative solutions rely on LoRA model fine-tuning, which has the limitations of high training costs and long cycles. This study takes the Jin Hou Bird Zun, the treasure of Shanxi Museum, as the research object, and constructs an AIGC cultural and creative innovation path that does not require model training based on the AI-CSD cultural symbol-driven model."));
            body.Append(EnKeywords("Key words:", "Digital Empowerment; AIGC; Museum Cultural and Creative Products; Jin Hou Bird Zun; AI-CSD Model; ComfyUI"));
        }

        static void AddTOC(Body body)
        {
            body.Append(H1("目  录"));
            body.Append(Para("（目录内容将在 Word 中通过引用功能自动生成）"));
        }

        static void AddChapter1(Body body)
        {
            body.Append(H1("1 导 论"));
            body.Append(H2("1.1 选题背景与意义"));
            body.Append(H3("1.1.1 政策背景：国家文化数字化战略与数智文创的最新政策导向"));
            body.Append(Para("这几年国家很重视文化产业数字化，出了《关于推进实施国家文化数字化战略的意见》《数字中国建设整体布局规划》这些文件，意思很清楚：文化产业得升级，数字技术得跟文化产业绑在一起。2025 年文旅部又发了《关于推动数字文化产业高质量发展的意见》，专门提了生成式人工智能在文创领域的应用，要培育新业态。这些政策给博物馆文创的数智化指了路，也给传统文化 IP 的数字化活化打开了口子 [^4]。"));

            body.Append(H3("1.1.2 产业背景：博物馆文创转型的现实需求与传统设计模式的效率痛点"));
            body.Append(Para("国民文化消费需求在涨，博物馆文创已经成了传播传统文化的重要出口。2024 年国内博物馆文创市场破了 150 亿，年增速 20% 以上。但快跑的同时，老设计模式的毛病也露出来了：靠设计师手工做，一款产品从想法到落地要 3-6 个月，追不上市场变化；产品长得太像，多数还在简单复刻文物、直接搬纹样，没深挖文化内涵，年轻人不买账 [^3]。而且设计师转译文化元素时容易带主观，文化表达会跑偏。"));

            body.Append(H3("1.1.3 案例背景：晋侯鸟尊 IP 的文化价值与现有开发的不足"));
            body.Append(Para("晋侯鸟尊是山西博物院的镇馆之宝，出自天马 - 曲村晋侯墓地 M114 墓，是西周第一代晋侯燮父的宗庙祭祀礼器 [^1]。它鸟象合体的造型、精细的纹饰、厚重的历史内涵，让它成了三晋文化的符号，IP 开发潜力很大 [^2]。但现在山西博物院对鸟尊的文创还在初级阶段，产品多是复刻摆件、钥匙扣、冰箱贴，形式单一，设计老气，没挖出背后的文化，也没跟上年轻人的审美。我调研过，现有鸟尊文创年销不到百万，跟它的国宝级价值差太远，得靠新的设计方法和技术来激活。"));

            body.Append(H3("1.1.4 研究意义"));
            body.Append(Para("这研究的意义在理论和实践两头。理论上，把 AI-CSD 模型引入地域文化 IP 的 AIGC 活化，补上了数智语境下传统文化 IP 的转译框架，丰富了 AIGC 文创的理论。实践上，以晋侯鸟尊为案例搭了一套能落地的 AIGC 路径，能帮山西博物院快速低成本做出鸟尊文创，解决现有痛点。这路径也能复制到其他地域博物馆，给整个行业的数智化转型提供参考。"));

            body.Append(H2("1.2 国内外文献综述"));
            body.Append(H3("1.2.1 文化与文创设计的相关理论研究"));
            body.Append(Para("文化与文创设计一直是设计学的热点。1976 年道金斯在《自私的基因》里提出文化模因理论，说文化传播跟基因遗传差不多，文化要素能通过模仿、复制、变异在人群里传，这给文化要素的拆解转译打了底 [^7]。国内学者在这基础上提出文化基因理论，把文化分成显性物质基因和隐性精神基因，认为基因转译是文创的核心。像李文涛等研究闽南剑狮文化，把剑狮拆成物质、场域、精神三层，建了文化基因图谱，给数字化转译提供了基础 [^3]。赵文强等从皮尔斯符号学角度看，探讨了文化符号能指和所指的转译机制，认为文创就是把传统文化符号的能指跟现代产品的所指重新结合，实现文化的现代传播 [^5]。这些研究给我的文化特征拆解提供了理论支撑。"));

            body.Append(H3("1.2.2 AIGC 技术在文创领域的应用研究"));
            body.Append(Para("AIGC 技术起来后，越来越多人关注它在文创里的应用。魏晓光等研究了生成式人工智能跟中华文化传承的关系，认为 AIGC 能提升传统文化的生产力、传播力和感染力，给中华文化的智慧传承找了新路 [^4]。侯云鹏等探索了基于 LoRA 模型的非遗数字化传承，以楚漆器为例，通过 LoRA 微调实现了楚漆器风格的智能生成，给非遗数字化保护提供了新方法 [^6]。李文涛等把 AIGC 用到闽南剑狮文化的创新设计里，搭了 \"数据驱动 + 人机协同\" 的新范式，实现了剑狮文化的数智化创新 [^3]。"));

            body.Append(H3("1.2.3 ComfyUI 等可视化生图工具的技术研究进展"));
            body.Append(Para("在 AIGC 生图工具里，ComfyUI 作为节点式可视化工作流工具，慢慢成了 AIGC 设计领域的重要工具。跟传统 WebUI 比，ComfyUI 用节点把生图过程拆成独立模块，每个节点管一个功能，像模型加载、提示词编码、图像生成、图像放大这些。用户能拖拽节点、连数据流，自由编排生成流程，实现模块化控制。"));

            body.Append(H3("1.2.4 博物馆文创创新与 IP 开发的现有研究"));
            body.Append(Para("关于博物馆文创创新和 IP 开发，国内外学者做了不少研究。国外学者早早就关注了博物馆 IP 的商业化，像美国大都会博物馆、大英博物馆，通过 IP 授权、跨界合作等方式实现了产业化。国内学者更关注本土博物馆的文创路径，比如故宫的 IP 体系化开发，把故宫文化元素跟现代产品结合，做出了一系列爆款，成了国内标杆。"));

            body.Append(H3("1.2.5 现有研究的不足"));
            body.Append(Para("梳理下来，现有研究虽然在文化文创、AIGC 应用方面做了不少探索，但还有不足。首先，现有 AIGC 文创研究多依赖 LoRA 微调，这模式需要大量训练数据，周期长成本高，中小博物馆扛不住，普及不了。其次，现有研究很难平衡文化性、艺术性和效率，要么为了效率丢了文化准确性，生成的产品跑偏了；要么为了文化准确性牺牲了创新和效率，做不出新东西。最后，现有研究多是理论探讨，缺具体实践和量化验证，证明不了路径的有效性。"));

            body.Append(H2("1.3 论文的结构及主要内容"));
            body.Append(Para("论文分六章，整体是 \"问题提出 - 理论基础 - 现状分析 - 路径构建 - 实践验证 - 总结建议\" 的逻辑。第一章导论，介绍选题背景意义，梳理文献，说清研究方法和结构。第二章是理论与技术基础，梳理文化模因、文化基因这些核心理论，说清扩散模型、ComfyUI 的核心机制和 AI-CSD 模型的优势，给后面的研究打底。第三章分析山西博物馆鸟尊文创的现状和困境，对比头部博物馆经验，分析山西博物院现有鸟尊文创的痛点，结合用户调研分析需求。第四章是核心，构建基于 AI-CSD 的鸟尊文创 AIGC 路径，说清文化拆解、关键词优化、聚类筛选、提示词构建、工作流设计的完整流程。第五章实践验证，介绍实践过程，通过用户评价实验量化验证路径有效性。第六章总结建议，总结结论价值，给山西博物院的数智化发展提具体建议。"));

            body.Append(H2("1.4 论文的研究方法"));
            body.Append(Para("研究用了四种方法："));
            body.Append(Para("文献分析法：梳理相关文献，系统整理文化模因、文化基因、AIGC 技术等理论，提炼文化与技术的研究基础，给研究提供理论支撑。"));
            body.Append(Para("比较分析法：对比故宫、南京博物院等头部博物馆的文创发展经验，分析山西博物院在文创开发上的差距和不足，给山西博物院的文创升级提供经验参考。"));
            body.Append(Para("工作流分析法：详细拆解 ComfyUI 的生图工作流和 AI-CSD 模型的运行逻辑，分析各节点的功能和数据流向，搭建完整的数智化设计流程。"));
            body.Append(Para("实证研究法：通过用户评价实验，设计 Likert 五级量表，回收 118 份有效问卷，对生成的设计方案进行量化验证，通过信度分析、描述性统计等方法验证方案有效性。"));
        }

        static void AddChapter2(Body body)
        {
            body.Append(H1("2 数智赋能下文创设计的理论与技术基础"));
            body.Append(H2("2.1 文化文创相关核心理论"));
            body.Append(H3("2.1.1 文化模因理论：文化要素的分层拆解与传播逻辑"));
            body.Append(Para("文化模因理论是理查德·道金斯 1976 年提的，这理论认为文化传播跟生物基因遗传差不多，文化的基本传播单位叫 \"模因\"（Meme），模因能通过模仿、复制、变异在人群里传播，就像基因通过遗传在生物间传播一样 [^7]。在文创设计里，文化模因理论给文化要素拆解提供了理论基础，我们能把复杂文化遗产拆成独立的文化模因单元，涵盖造型、纹饰这些视觉要素和寓意、故事这些精神要素，通过拆解、重组、变异实现文化的现代转译。"));

            body.Append(H3("2.1.2 文化基因理论：显性与隐性文化要素的转译方法"));
            body.Append(Para("文化基因理论是在文化模因理论基础上发展的，把文化要素分成显性文化基因和隐性文化基因两个层次。显性文化基因是能直观感知的物质要素，比如文物的造型、纹饰，是文创设计里最容易提取转译的外在表现；隐性文化基因是隐藏的精神要素，比如历史故事、文化寓意，是文化的核心灵魂，也是设计里最需要挖掘传递的内容 [^3]。"));

            body.Append(H3("2.1.3 数智赋能理论：数字技术对文化产业的生产力升级逻辑"));
            body.Append(Para("数智赋能理论说的是数字技术与智能技术对传统产业的生产力升级作用，在文化产业里，数智赋能意味着通过数字技术与智能技术，重构文化产业的生产、传播、消费流程，提升文化产业的生产力。传统文化产业是劳动密集型的，靠大量人力投入，生产效率低，产出有限。而数智技术的引入，能把文化要素转化为数据，通过算法与模型实现文化内容的自动化、智能化生产，极大地提升生产效率 [^4]。"));

            body.Append(H2("2.2 AIGC 生图技术的核心机制"));
            body.Append(H3("2.2.1 扩散模型的基本工作原理：噪声添加与反向去噪的生成逻辑"));
            body.Append(Para("扩散模型是现在 AIGC 生图技术的核心基础，原理是通过模拟扩散过程来生成图像。扩散模型分正向加噪和反向去噪两个阶段：正向过程中，模型逐步往清晰图像加高斯噪声，最终把图像变成随机噪声，这过程不需要模型学习；反向过程中，模型通过学习大量数据，学会逐步去噪，从随机噪声还原出清晰图像 [^6]。"));

            body.Append(H3("2.2.2 ComfyUI 可视化工作流工具：节点式的生成流程编排与模块化控制"));
            body.Append(Para("ComfyUI 是基于扩散模型的可视化工作流工具，跟传统 WebUI 比，ComfyUI 用节点式设计，把生图的整个过程拆成一个个独立节点，每个节点管一个特定功能，比如模型加载、提示词编码、图像生成、图像放大这些。用户能拖拽节点、连接节点间的数据流，自由编排生成流程，实现模块化控制。"));

            body.Append(H3("2.2.3 传统 AIGC 文创的 LoRA 微调模式：运行机制与现实局限"));
            body.Append(Para("传统的 AIGC 文创设计，大多用 LoRA 模型微调的模式。LoRA 是 Low-Rank Adaptation 的缩写，是一种模型微调技术，原理是在不改变原始大模型参数的情况下，通过引入两个低秩矩阵来学习特定的风格或特征，让模型能生成特定风格的图像 [^6]。这模式的优势是，一旦训练完成，模型就能稳定地生成特定风格的图像。但这模式也有不少局限。"));

            body.Append(H2("2.3 AI-CSD 文化符号驱动模型的核心优势"));
            body.Append(H3("2.3.1 AI-CSD 模型的核心逻辑：无需模型训练的语义驱动生成"));
            body.Append(Para("AI-CSD 模型，全称是 AI-Cultural Symbol Driven 模型，也就是 AI 文化符号驱动模型，核心逻辑是不需要微调模型，而是通过对文化符号的结构化语义拆解与优化，把文化特征转化成精准的结构化提示词，通过提示词来驱动大模型，实现文化特征的精准生成 [^3]。"));

            body.Append(H3("2.3.2 效率优势：规避 LoRA 的训练周期，提示词设计完成后可实现分钟级的设计产出"));
            body.Append(Para("AI-CSD 模型的第一个核心优势是效率极高。传统 LoRA 模式要花大量时间准备训练数据、训练模型，整个过程要几天；而 AI-CSD 模式只需要我们完成文化特征拆解和提示词构建，这过程通常只要几小时，一旦提示词构建完成，我们就能通过 ComfyUI 工作流在几分钟内批量生成几十上百个不同风格的设计方案。"));

            body.Append(H3("2.3.3 精准优势：分层文化特征的精准控制，保障文化表达的准确性"));
            body.Append(Para("AI-CSD 模型的另一个优势是文化表达的精准性。在 LoRA 模式下，模型通过数据学习来捕捉文化特征，这过程是黑箱的，很容易出现特征遗漏或偏差，导致生成的图像跑偏了文化内涵。而在 AI-CSD 模式下，我们人工对文化特征进行分层拆解，把文物的所有核心文化特征，包括视觉的、历史的、内涵的，都拆解出来，然后转化成精准的提示词，明确告诉模型哪些特征必须保留、哪些可以创新。"));
        }

        static void AddChapter3(Body body)
        {
            body.Append(H1("3 山西博物馆鸟尊文创的发展现状与现实困境"));
            body.Append(H2("3.1 国内头部博物馆文创的发展经验借鉴"));
            body.Append(H3("3.1.1 故宫博物院：IP 体系化开发与跨界创新的成功经验"));
            body.Append(Para("故宫是国内博物馆文创开发的标杆，经过多年发展，故宫已经建起了完整的 IP 体系化开发模式。首先，故宫对院藏文物做了系统梳理，挖出了大量文化 IP，从瑞兽、纹样到人物、故事，形成了丰富的 IP 矩阵。其次，故宫用了跨界创新的模式，把故宫 IP 跟各个行业结合，推出了美妆、食品、数码、服饰等多个品类的文创产品，覆盖了用户生活的各种场景。"));

            body.Append(H3("3.1.2 南京博物院：文化内涵挖掘与年轻化设计的融合路径"));
            body.Append(Para("南京博物院作为国内头部的省级博物馆，在文创开发上走了一条文化内涵挖掘与年轻化设计融合的路。南博很重视挖掘文物的文化内涵，比如，针对镇馆之宝 \"竹林七贤砖画\"，南博没有简单复刻砖画，而是挖掘了竹林七贤背后的文人精神，把它跟现代年轻人的生活态度结合，推出了 \"七贤\" 系列的潮玩、文具等产品，受到了年轻用户喜欢。"));

            body.Append(H3("3.1.3 头部博物馆的数智化转型实践：AIGC 工具的应用探索"));
            body.Append(Para("这几年，随着 AIGC 技术发展，国内头部博物馆也开始探索 AIGC 工具在文创设计中的应用。故宫率先推出了 \"AI 故宫\" 项目，利用 AIGC 技术实现了古画动态化、文物三维化，同时探索了利用 AIGC 辅助文创设计。苏州博物馆推出了 AIGC 文创设计平台，用户能自己输入提示词，生成属于自己的苏博文创。"));

            body.Append(H2("3.2 山西博物院文创的发展现状"));
            body.Append(Para("山西博物院作为山西省级的综合性博物馆，拥有丰富的文物资源，院藏文物超过 40 万件，其中珍贵文物超过 6000 件，拥有晋侯鸟尊、胡傅酒樽、兽形觥这些国宝级文物。这几年，山西博物院也开始重视文创开发，成立了专门的文创部门，推出了一系列文创产品。"));

            body.Append(H3("3.2.1 现有晋侯鸟尊文创产品的特征与已有成果"));
            body.Append(Para("晋侯鸟尊作为山西博物院的镇馆之宝，是山西博物院文创开发的重点。现在，山西博物院已经推出了一系列鸟尊文创产品，包括鸟尊复刻摆件、鸟尊造型的钥匙扣、冰箱贴、书签、U 盘这些。这些产品的共同特征是，都是对鸟尊原文物的简单复刻，把鸟尊的造型等比例缩小，做成各种小摆件或周边。"));

            body.Append(H2("3.3 现有鸟尊文创的现实困境"));
            body.Append(H3("3.3.1 设计层面：仅停留在原文物的生硬复刻，缺乏艺术性提取与文化性挖掘"));
            body.Append(Para("现有鸟尊文创的第一个困境是设计层面的问题，现有设计仅仅停留在对原文物的生硬复刻，设计师只是把原文物的造型直接拿过来，等比例缩小做成产品，而没有对鸟尊的文化特征进行艺术性的提取与再创作。"));

            body.Append(H3("3.3.2 产品层面：形式单一同质化严重，风格表现力弱，难以适配年轻群体需求"));
            body.Append(Para("在产品层面，现有鸟尊文创形式太单一，同质化严重，所有产品都围绕鸟尊的造型做文章，不是摆件就是钥匙扣，没有其他创新形式。而且，这些产品的风格都过于传统、厚重，都是青铜的质感、复古的风格，这种风格对年轻群体来说过于陈旧，缺乏吸引力。"));

            body.Append(H3("3.3.3 传播层面：文化影响力不足，缺乏破圈传播的核心产品"));
            body.Append(Para("在传播层面，现有鸟尊文创缺乏能破圈传播的核心产品。一个 IP 要想传播开，必须有个爆款产品，能引发用户自发传播。比如故宫的口红、三星堆的盲盒，都是这样的爆款。而鸟尊的文创一直没有这样的爆款，所有产品都中规中矩，没有亮点，引发不了用户的传播欲望。"));

            body.Append(H2("3.4 用户需求的调研与分析"));
            body.Append(H3("3.4.1 核心消费群体的特征：18-45 岁年轻群体的审美与消费偏好"));
            body.Append(Para("调研结果显示，文创产品的核心消费群体是 18-45 岁的年轻群体，这个群体占了文创消费的 80% 以上。其中，18-25 岁的学生群体和 26-35 岁的年轻白领，是最核心的两个群体。这个群体的审美偏好跟传统群体有很大不同，他们更喜欢个性化、多元化的设计，喜欢国潮、潮玩、盲盒这些新形式。"));

            body.Append(H3("3.4.2 用户对鸟尊文创的需求：文化感知、创新表达与消费意愿的平衡"));
            body.Append(Para("在对鸟尊文创的需求方面，用户最看重的三个点是：文化感知、创新表达与消费意愿的平衡。首先，用户希望产品能体现鸟尊的文化内涵，能让他们感受到鸟尊背后的文化，而不是一个简单玩具；其次，用户希望产品有创新的表达，不要是老气的复刻，要有新的设计、新的风格，能符合他们的审美。"));
        }

        static void AddChapter4(Body body)
        {
            body.Append(H1("4 基于 AI-CSD 的鸟尊文创 AIGC 创新路径构建"));
            body.Append(H2("4.1 路径整体框架"));
            body.Append(Para("基于 AI-CSD 模型的核心逻辑，我构建了一套完整的鸟尊文创 AIGC 创新路径，整个路径是一个闭环流程，包含了文化拆解、关键词优化、聚类分析、产品筛选、提示词构建、AIGC 生成、用户评价、反向优化八个核心环节。首先，对晋侯鸟尊的文化特征进行三层维度拆解，把复杂的文化要素拆成直观的视觉特征、历史的语境信息和抽象的文化内涵；然后，对拆出来的关键词进行三轮标准化优化，清退模糊词、合并同义词、筛选核心词，得到精准的文化关键词。"));

            body.Append(H2("4.2 晋侯鸟尊文化特征的三层维度拆解"));
            body.Append(H3("4.2.1 直观感知层：视觉感官特征的拆解"));
            body.Append(Para("直观感知层就是能直观看到的视觉感官特征，也就是显性的文化基因，这部分是设计中最基础的视觉要素。通过对鸟尊文物的分析，我拆解出了以下核心视觉特征：造型特征、子母鸟结构、纹饰特征、色彩与材质。"));

            body.Append(H3("4.2.2 逻辑事实层：历史语境信息的梳理"));
            body.Append(Para("逻辑事实层就是鸟尊对应的历史与现实的语境信息，这部分是鸟尊的历史背景，也是我们设计中可以挖掘的故事要素。通过对鸟尊相关文献的梳理，我拆解出了以下核心信息：器物属性、所有者与历史背景、出土信息、工艺特征。"));

            body.Append(H3("4.2.3 文化法则层：抽象文化内涵的提取"));
            body.Append(Para("文化法则层就是鸟尊承载的抽象文化内涵，也就是隐性的文化基因，这部分是鸟尊的精神内核，也是我们设计中最需要传递的情感与寓意。通过对鸟尊文化的分析，我拆解出了以下核心内涵：情感寓意、文化隐喻、精神内涵、审美取向。"));

            body.Append(H2("4.3 文化关键词的标准化优化"));
            body.Append(H3("4.3.1 第一轮：清退宽泛模糊词，删除无落地性的空泛表述"));
            body.Append(Para("第一轮清退宽泛模糊的空泛表述，比如 \"华夏凤鸟信仰的文化投射\" 这些无法落地的抽象描述，保留具体可落地的关键词，精简关键词库。"));

            body.Append(H3("4.3.2 第二轮：合并同义词，统一语义指向"));
            body.Append(Para("第二轮合并同义词，统一语义指向，比如把描述铜锈的多个重复表述合并为 \"斑驳青绿铜锈\"，消除关键词冗余，明确每个词的语义。"));

            body.Append(H3("4.3.3 第三轮：筛选核心差异化词，保留鸟尊独有的文化识别要素"));
            body.Append(Para("第三轮筛选核心差异化关键词，保留鸟尊独有的文化识别要素，比如 \"鸟象合体三足造型\" 这些，去除 \"青铜礼器\" 这类通用无辨识度的词，保障生成产品的独特辨识度。"));

            body.Append(H2("4.4 基于 KJ 法的文创方向聚类与定位"));
            body.Append(H3("4.4.1 关键词的语义聚类：分为造型纹饰、功能工艺、历史叙事、文化内涵四组产品"));
            body.Append(Para("我们把 43 个关键词按语义聚类为四个核心组：造型纹饰组、功能工艺组、历史叙事组、文化内涵组。这样，我们就得到了 16 个具体的文创方向，覆盖了不同类型、不同场景，为我们的筛选提供了基础。"));

            body.Append(H3("4.4.2 多维度评分筛选：传统文创视角与 AIGC 开发视角的交叉验证"));
            body.Append(Para("接下来，我们需要从这 16 个方向中筛选出最优方向。我们采用了多维度评分，从两个视角进行交叉验证：一个是传统文创落地的视角，另一个是 AIGC 开发的视角。"));

            body.Append(H3("4.4.3 核心产品定位：「鸟尊主题潮玩 + 子母鸟情感 IP」的核心方案"));
            body.Append(Para("从评分结果可以看到，三方判断高度一致，鸟尊主题潮玩手办是唯一三方都给出满分的产品，它在所有维度都表现最优。最终确定了核心产品定位：「鸟尊主题潮玩 + 子母鸟情感 IP」。"));

            body.Append(H2("4.5 基于 ComfyUI 的模块化生成工作流"));
            body.Append(H3("4.5.1 结构化提示词的构建：主体、造型、装饰、规则、渲染参数的模块化设计"));
            body.Append(Para("提示词是 AI-CSD 模型的核心，我们需要把文化特征转化成结构化提示词，让模型能精准理解我们的需求。我们把提示词分成六个模块，每个模块管不同的部分，这样的模块化设计能让提示词更清晰，模型也能更好地理解。"));

            body.Append(H3("4.5.2 批量产出：接入 Z-image、GPT Image 2 多风格快速产出"));
            body.Append(Para("完成了提示词构建后，我们接入了 Z-image 和 GPT Image 2 两个生成接口，同时结合 ComfyUI 的批量生成功能，一次输入三个风格的提示词，批量生成设计方案。我们总共生成了 24 组初始设计方案。"));

            body.Append(H3("4.5.3 ComfyUI 工作流的整体设计"));
            body.Append(Para("我们的 ComfyUI 工作流是模块化的设计，整个流程分六个核心节点：模型加载节点、提示词输入节点、原图参考节点、图像生成节点、放大修复节点、最终输出节点。这工作流完全不需要 LoRA 训练，也不需要 ControlNet 控制，只需要提示词加上原图参考就能实现精准生成。"));
        }

        static void AddChapter5(Body body)
        {
            body.Append(H1("5 创新路径的实践操作与效果验证"));
            body.Append(H2("5.1 实践操作的实施过程"));
            body.Append(H3("5.1.1 文化特征提炼与关键词优化的操作过程"));
            body.Append(Para("第一阶段是文化特征提炼和关键词优化，这阶段是我人工完成的。我先查了晋侯鸟尊的相关文献，深入了解了鸟尊的历史、文化、艺术特征。然后，我按照三层维度的框架对鸟尊文化特征进行了拆解，得到了初始关键词。之后，我进行了三轮关键词优化，最终得到了 43 个核心关键词。该阶段共耗时约 2 小时。"));

            body.Append(H3("5.1.2 提示词构建与 ComfyUI 批量生成的操作详情"));
            body.Append(Para("第二阶段是提示词构建和批量生成。我基于筛选出来的核心产品方向构建了结构化提示词，针对不同风格分别优化了提示词模块。然后，我搭建了 ComfyUI 工作流，配置了 Flux2 Klein 模型和 RealESRGAN_x4plus 放大模型。不到 10 分钟就生成了 24 组初始设计方案。"));

            body.Append(H3("5.1.3 24 组初始设计方案的产出结果"));
            body.Append(Para("最终，我们总共产出了 24 组初始设计方案，其中 12 组是首饰类的方案，另外 12 组是手办类的方案，覆盖了毛绒风、国潮风、机甲风、Q 版风等多种不同风格。"));

            body.Append(H2("5.2 最优方案的确定：分群体的适配方案筛选"));
            body.Append(Para("根据用户的评分，我们最终确定了分群体的最优方案。成年核心用户的优选方案：手办方案 1，综合得分 4.26 分。青少年/儿童用户的补充方案：Q 版可爱风的手办方案 4，综合得分 4.07 分。"));

            body.Append(H2("5.3 实践效果的总结"));
            body.Append(H3("5.3.1 效率提升：AI-CSD 模式相比传统模式的时间成本优化"));
            body.Append(Para("在效率方面，AI-CSD 模式相比传统模式有了极大提升。传统文创设计从概念到出稿要 3-6 个月，而本研究的 AI-CSD 模式从文化拆解到生成 24 组方案仅耗时不到 6 小时，时间成本降低 95% 以上。"));

            body.Append(H3("5.3.2 效果验证：有效平衡了文化性、艺术性与效率三者的需求"));
            body.Append(Para("在效果方面，我们的路径有效平衡了文化性、艺术性与效率三者的需求。这也证明了我们的核心结论：ComfyUI + AI-CSD 的结构化提示词模式，可以在不进行 LoRA 训练的情况下，实现山西博物馆鸟尊文创的高效率、高文化准确性、高艺术表达的数智化生成路径。"));
        }

        static void AddChapter6(Body body)
        {
            body.Append(H1("6 研究总结与发展建议"));
            body.Append(H2("6.1 研究总结"));
            body.Append(H3("6.1.1 研究结论：ComfyUI + AI-CSD 模式可以有效解决山西博物院文创的现有痛点"));
            body.Append(Para("本研究以晋侯鸟尊为案例，构建了基于 AI-CSD 模型的 AIGC 文创创新路径，通过实践与验证，我们得出了以下结论：首先，ComfyUI + AI-CSD 的模式能有效解决山西博物院现有鸟尊文创的痛点；其次，这模式不需要 LoRA 训练，极大降低了数智化设计的门槛；最后，这模式能有效平衡文化性、艺术性与效率三者的需求。"));

            body.Append(H3("6.1.2 研究价值：为地域博物馆文创的数智化转型提供了可复制的路径"));
            body.Append(Para("本研究的价值在于，它为地域博物馆的文创数智化转型提供了一个可复制的路径。之前的 AIGC 文创方案大多依赖 LoRA 训练，成本高、门槛高，而我们的路径只需要人工的文化拆解，不需要任何训练，成本极低、门槛极低。"));

            body.Append(H2("6.2 山西博物院文创的数智化发展建议"));
            body.Append(H3("6.2.1 构建院藏文物的文化特征关键词知识库，为 AIGC 开发提供基础"));
            body.Append(Para("首先，山西博物院应该构建院藏文物的文化特征关键词知识库，对院藏的国宝级文物按照我们的三层维度框架进行文化特征拆解，提炼核心关键词，建一个标准化的关键词库。"));

            body.Append(H3("6.2.2 引入 ComfyUI + AI-CSD 的设计模式，降低文创设计的技术与时间成本"));
            body.Append(Para("其次，山西博物院应该引入 ComfyUI + AI-CSD 的设计模式，替代传统设计模式。这模式能极大降低文创设计的技术与时间成本，让博物院能快速开发大量文创产品。"));

            body.Append(H3("6.2.3 推动数智文创的合规发展，保障文化 IP 与设计成果的权益"));
            body.Append(Para("最后，博物院应该推动数智文创的合规发展，建立相关规范，保障文化 IP 与设计成果的权益。博物院需要明确 AIGC 生成的文创设计的版权归属，同时也要保护文物 IP 的权益，防止 IP 滥用。"));
        }

        static void AddReferences(Body body)
        {
            body.Append(H1("参考文献"));
            body.Append(Ref("[1] 赵凡奇. 凤鸣晋地 —— 山西博物院鸟尊鉴赏 [J]. 山西档案，2012 (04):17-19."));
            body.Append(Ref("[2] 王林. 晋侯鸟尊原型初探 [J]. 博物院，2019 (02):71-75."));
            body.Append(Ref("[3] 李文涛，廖嘉敏. AIGC 视阈下闽南剑狮文化数智化创新设计研究 [J]. 设计艺术研究，2025,15 (06):81-86."));
            body.Append(Ref("[4] 魏晓光，韩立新. 生成式人工智能与中华文化智慧传承 [J]. 中国广播电视学刊，2023 (9):13-16."));
            body.Append(Ref("[5] 赵文强，臧欣慈. 皮尔斯符号学视域下 AIGC 对文创产品的设计探索 [J]. 包装工程，2024 (10):116-126."));
            body.Append(Ref("[6] 侯云鹏，彭涵，刘育晖. 基于 LoRA 模型的非遗数字化传承 [J]. 设计艺术研究，2024,14 (1):14-18."));
            body.Append(Ref("[7] 理查德·道金斯. 自私的基因：40 周年增订版 [M]. 中信出版集团，2019."));
            body.Append(Ref("[8] 田雪棠，毛咸. AI-CSD 模型驱动提示词优化的 AIGC 辅助文创产品设计研究 [J]. 工业工程设计，2026,8 (01):100-112."));
            body.Append(Ref("[9] 林懿，李靖. 数智赋能背景下 AIGC 技术在三星堆文创产品设计中的实践研究 [J]. 包装工程，2026,47 (04):413-420."));
            body.Append(Ref("[10] 武真. 基于山西博物院的文创产品开发设计 [D]. 山西大学，2021."));
        }

        static void AddAppendix(Body body)
        {
            body.Append(H1("附  录"));
            body.Append(Para("附录一：24 组设计方案可复现版原图"));
        }

        static void AddAcknowledgement(Body body)
        {
            body.Append(H1("致  谢"));
            body.Append(Para("这论文能完成，得感谢我的指导老师，老师在选题、研究方法、写作这些方面都帮了我很多，让我能顺利走完整个研究。也要感谢同学和朋友们。还得感谢山西博物院，提供了晋侯鸟尊的相关资料，让我能深入了解这件国宝的文化内涵。最后，也得感谢 AIGC 技术的发展，让我能探索这种新的设计路径，为传统文化的活化出一份力。以后我会继续研究数智文创相关内容，为传统文化的传承与创新出力。"));
        }

        // Helper methods
        static Paragraph H1(string text) => new Paragraph(
            new ParagraphProperties(
                new Justification { Val = JustificationValues.Center },
                new SpacingBetweenLines { Before = "240", After = "240", Line = "300", LineRule = LineSpacingRuleValues.Auto },
                new OutlineLevel { Val = 0 }),
            new Run(new RunProperties(new RunFonts { Ascii = "SimHei", HighAnsi = "SimHei", EastAsia = "SimHei" }, new FontSize { Val = "32" }, new Bold()), new Text(text)));

        static Paragraph H2(string text) => new Paragraph(
            new ParagraphProperties(
                new Justification { Val = JustificationValues.Left },
                new SpacingBetweenLines { Before = "0", After = "0", Line = "300", LineRule = LineSpacingRuleValues.Auto },
                new OutlineLevel { Val = 1 }),
            new Run(new RunProperties(new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" }, new FontSize { Val = "28" }, new Bold()), new Text(text)));

        static Paragraph H3(string text) => new Paragraph(
            new ParagraphProperties(
                new Justification { Val = JustificationValues.Left },
                new SpacingBetweenLines { Before = "0", After = "0", Line = "300", LineRule = LineSpacingRuleValues.Auto },
                new OutlineLevel { Val = 2 }),
            new Run(new RunProperties(new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" }, new FontSize { Val = "24" }, new Bold()), new Text(text)));

        static Paragraph Para(string text) => new Paragraph(
            new ParagraphProperties(
                new Indentation { FirstLineChars = 200 },
                new SpacingBetweenLines { Line = "300", LineRule = LineSpacingRuleValues.Auto }),
            new Run(new RunProperties(new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" }, new FontSize { Val = "24" }), new Text(text)));

        static Paragraph Ref(string text) => new Paragraph(
            new ParagraphProperties(
                new Indentation { Left = "0", Hanging = "480" },
                new SpacingBetweenLines { Line = "300", LineRule = LineSpacingRuleValues.Auto }),
            new Run(new RunProperties(new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" }, new FontSize { Val = "24" }), new Text(text)));

        static Paragraph Keywords(string label, string content) => new Paragraph(
            new ParagraphProperties(new SpacingBetweenLines { Before = "240" }),
            new Run(new RunProperties(new RunFonts { Ascii = "SimHei", HighAnsi = "SimHei", EastAsia = "SimHei" }, new FontSize { Val = "28" }), new Text(label)),
            new Run(new RunProperties(new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" }, new FontSize { Val = "24" }), new Text(content)));

        static Paragraph EnKeywords(string label, string content) => new Paragraph(
            new ParagraphProperties(new SpacingBetweenLines { Before = "240" }),
            new Run(new RunProperties(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize { Val = "28" }, new Bold()), new Text(label)),
            new Run(new RunProperties(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize { Val = "24" }), new Text(content)));

        static Paragraph CreateEmptyPara() => new Paragraph();
        static Paragraph CreatePageBreak() => new Paragraph(new Run(new Break { Type = BreakValues.Page }));

        static void CloseSection(Body body, SectionProperties sectPr)
        {
            var lastPara = body.Elements<Paragraph>().Last();
            var pPr = lastPara.Elements<ParagraphProperties>().FirstOrDefault();
            if (pPr == null) { pPr = new ParagraphProperties(); lastPara.PrependChild(pPr); }
            pPr.Append(new SectionProperties(sectPr.ChildElements.Select(e => e.CloneNode(true)).ToArray()));
        }

        static SectionProperties CreateSectionNoHeader() => new SectionProperties(
            new PageSize { Width = (UInt32Value)(uint)PAGE_WIDTH, Height = (UInt32Value)(uint)PAGE_HEIGHT },
            new PageMargin { Top = MARGIN_TOP, Bottom = MARGIN_BOTTOM, Left = (UInt32Value)(uint)MARGIN_LEFT, Right = (UInt32Value)(uint)MARGIN_RIGHT, Header = (UInt32Value)720U, Footer = (UInt32Value)720U, Gutter = (UInt32Value)0U },
            new DocGrid { LinePitch = 312 });

        static SectionProperties CreateSectionWithHeader(MainDocumentPart mainPart, bool isRoman)
        {
            var sectPr = new SectionProperties(
                new PageSize { Width = (UInt32Value)(uint)PAGE_WIDTH, Height = (UInt32Value)(uint)PAGE_HEIGHT },
                new PageMargin { Top = MARGIN_TOP, Bottom = MARGIN_BOTTOM, Left = (UInt32Value)(uint)MARGIN_LEFT, Right = (UInt32Value)(uint)MARGIN_RIGHT, Header = (UInt32Value)720U, Footer = (UInt32Value)720U, Gutter = (UInt32Value)0U },
                new DocGrid { LinePitch = 312 },
                new PageNumberType { Start = 1 },
                new EvenAndOddHeaders());

            var headerOdd = mainPart.AddNewPart<HeaderPart>();
            headerOdd.Header = new Header(new Paragraph(new ParagraphProperties(new Justification { Val = JustificationValues.Right }), new Run(new RunProperties(new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" }, new FontSize { Val = "18" }), new Text(HEADER_TEXT) { Space = SpaceProcessingModeValues.Preserve })));
            headerOdd.Header.Save();

            var headerEven = mainPart.AddNewPart<HeaderPart>();
            headerEven.Header = new Header(new Paragraph(new ParagraphProperties(new Justification { Val = JustificationValues.Left }), new Run(new RunProperties(new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" }, new FontSize { Val = "18" }), new Text(HEADER_TEXT) { Space = SpaceProcessingModeValues.Preserve })));
            headerEven.Header.Save();

            string fmt = isRoman ? "UpperRoman" : "Decimal";
            var footerOdd = mainPart.AddNewPart<FooterPart>();
            footerOdd.Footer = new Footer(new Paragraph(new ParagraphProperties(new Justification { Val = JustificationValues.Right }), new Run(new RunProperties(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize { Val = "18" }), new FieldChar { FieldCharType = FieldCharValues.Begin }), new Run(new RunProperties(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize { Val = "18" }), new FieldCode($" PAGE \\* {fmt} ") { Space = SpaceProcessingModeValues.Preserve }), new Run(new RunProperties(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize { Val = "18" }), new FieldChar { FieldCharType = FieldCharValues.End })));
            footerOdd.Footer.Save();

            var footerEven = mainPart.AddNewPart<FooterPart>();
            footerEven.Footer = new Footer(new Paragraph(new ParagraphProperties(new Justification { Val = JustificationValues.Left }), new Run(new RunProperties(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize { Val = "18" }), new FieldChar { FieldCharType = FieldCharValues.Begin }), new Run(new RunProperties(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize { Val = "18" }), new FieldCode($" PAGE \\* {fmt} ") { Space = SpaceProcessingModeValues.Preserve }), new Run(new RunProperties(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize { Val = "18" }), new FieldChar { FieldCharType = FieldCharValues.End })));
            footerEven.Footer.Save();

            sectPr.Append(new HeaderReference { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(headerOdd) });
            sectPr.Append(new HeaderReference { Type = HeaderFooterValues.Even, Id = mainPart.GetIdOfPart(headerEven) });
            sectPr.Append(new FooterReference { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(footerOdd) });
            sectPr.Append(new FooterReference { Type = HeaderFooterValues.Even, Id = mainPart.GetIdOfPart(footerEven) });

            return sectPr;
        }
    }
}
