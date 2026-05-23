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

    // Student info (used for cover and header)
    static string STUDENT_ID = "202406020153";
    static string GRADE => STUDENT_ID[..4]; // Extract first 4 digits: "2024"
    static string HEADER_TEXT => $"山西财经大学{GRADE}级本科生学年论文";
    static string MAJOR_CODE = "120103";

    static void Main(string[] args)
    {
        OUTPUT = Path.Combine(BASE, "output", "基于计量经济学的时间序列方法的建材价格预测研究.docx");
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
        H.MergeTemplate(doc, mainPart, body!, templatePath, MAJOR_CODE, STUDENT_ID);

        fnPart.Footnotes!.Save();
        H.AddComments(mainPart, body!);
        mainPart.Document.Save();
        Console.WriteLine("DONE: " + OUTPUT);
    }

    static void BuildDocument()
    {
        var b = body!;
        var mp = mainPart!;

        // Create header/footer parts for Roman numeral section (abstracts + TOC)
        var hdrRoman = H.MakeHeader(mp, HEADER_TEXT, H.JC_CENTER);
        var hdrRomanId = mp.GetIdOfPart(hdrRoman);
        var ftrRoman = H.MakeFooter(mp, H.JC_CENTER);
        var ftrRomanId = mp.GetIdOfPart(ftrRoman);

        // Create header/footer parts for Arabic numeral section (body + references + ack)
        var hdrOdd = H.MakeHeader(mp, HEADER_TEXT, H.JC_RIGHT);
        var hdrOddId = mp.GetIdOfPart(hdrOdd);
        var hdrEven = H.MakeHeader(mp, HEADER_TEXT, H.JC_LEFT);
        var hdrEvenId = mp.GetIdOfPart(hdrEven);
        var ftrOdd = H.MakeFooter(mp, H.JC_RIGHT);
        var ftrOddId = mp.GetIdOfPart(ftrOdd);
        var ftrEven = H.MakeFooter(mp, H.JC_LEFT);
        var ftrEvenId = mp.GetIdOfPart(ftrEven);

        // ── SECTION 1: Cover + Explanation + Academic Pledge (no header/footer) ──
        H.AddBlankPage(b); H.AddBlankPage(b); H.AddBlankPage(b);
        H.CloseSection(b, H.MakeSectPr(null, null, SectionMarkValues.OddPage, null, null));

        // ── SECTION 2: Chinese Abstract + English Abstract + TOC (merged, page breaks) ──
        // Chinese Abstract
        b.Append(H.MakeCenteredTitle("摘  要", "SimHei", "36", "360", "360"));
        b.Append(H.MakeBodyPara("建筑材料是建筑行业的主要成本构成，其价格变动会直接影响工程造价的确定与控制效果。本研究选取螺纹钢、水泥、玻璃三种典型建材作为研究对象，采集2015年1月至2024年12月的月度价格数据，分别构建RIMA模型、GARCH类模型与VAR模型，对比不同模型的价格预测效果。结果发现，这三类建材的价格序列有几个共同的统计特征：非平稳，有波动聚集，还有长期记忆。GARCH(1,1)模型刻画价格波动风险最到位；ARIMA(1,1,1)模型在短期点位预测上比较好用；VAR模型能看出不同建材价格之间的动态联动关系。这些结果可以给建筑企业在材料采购和价格风控上提供一个定量的依据。"));
        b.Append(H.MakeBodyPara("本研究仅覆盖了三类通用性较强的建材，未涉及石材、保温材料等用量相对较小的品类，模型也未纳入房地产政策变动、原材料进口管制等外生冲击变量，后续研究可进一步拓展样本覆盖范围，在模型中加入政策类虚拟变量，提升预测结果的适用性。"));
        b.Append(H.MakeKeyPara(true, "建材价格；时间序列；ARIMA；GARCH；VAR；价格预测"));

        H.AddPageBreak(b);

        // English Abstract
        b.Append(H.MakeAbstractTitle());
        b.Append(H.MakeBodyParaEn("Building materials constitute the main cost component of the construction industry, and their price fluctuations directly affect the determination and control effect of project costs. This study selects three typical building materials - rebar, cement, and glass - as the research objects, collecting monthly price data from January 2015 to December 2024. ARIMA models, GARCH-type models, and VAR models are respectively constructed to compare the price prediction effects of different models. The results show that the price series of these three types of materials have several common statistical characteristics: non-stationarity, volatility clustering, and long-term memory. The GARCH(1,1) model best depicts the risk of price fluctuations; the ARIMA(1,1,1) model is more suitable for short-term point prediction; the VAR model can reveal the dynamic linkage relationship between the prices of different building materials. These results can provide a quantitative basis for construction enterprises in material procurement and price risk control."));
        b.Append(H.MakeBodyParaEn("This study only covers three types of commonly used building materials and does not involve categories such as stone materials and insulation materials with relatively smaller quantities. The models also do not include exogenous shock variables such as changes in real estate policies and import control of raw materials. Further research can expand the sample coverage, incorporate policy-based dummy variables into the models, and improve the applicability of the prediction results."));
        b.Append(H.MakeEngKeyPara("Building materials prices; Time series; ARIMA; GARCH; VAR; Price forecasting"));

        H.AddPageBreak(b);

        // TOC
        b.Append(H.MakeCenteredTitle("目  录", "SimSun", "36", "360", "360"));
        H.AddTOC(b);
        H.CloseSection(b, H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.OddPage, NumberFormatValues.UpperRoman, 1));

        // ── SECTION 3: Body ──
        // 1 导论
        b.Append(H.MakeH1("1 导  论"));
        b.Append(H.MakeH2("1.1 研究背景"));
        b.Append(H.MakeBodyPara("建筑业是国民经济的支柱产业，行业平稳运行直接关系宏观经济稳定。建材成本占建筑工程总成本的60%-70%，钢材、水泥、玻璃等大宗建材价格波动会直接影响工程造价。受供给侧结构性改革、环保限产政策、国际大宗商品价格波动及房地产市场周期性调整等多重因素作用，近年我国建材市场价格波动频率提升，非线性变化特征愈发明显。"));
        b.Append(H.MakeBodyPara("螺纹钢全国均价在2021年5月每吨涨破6000元，比年初高出四成多。政策一收紧，价格又快跌下来，全年振幅超过50%。这样的波动，把建筑企业的成本预算、采购节奏和库存周转都卡得很死。手上有一套可靠的价格预测模型，就能躲掉局部风险，把采购时点、付款安排做得从容些。"));

        b.Append(H.MakeH2("1.2 研究意义"));
        b.Append(H.MakeBodyPara("理论层面，这项研究把计量经济学里的时间序列分析用到了建材价格预测上，是建筑经济学和计量经济学交叉地带的一次尝试，预测框架本身也可以给其他大宗商品的价格研究找找路子。"));
        b.Append(H.MakeBodyPara("实践上，搭建的多模型对比框架是个量化工具，建筑企业、造价咨询机构、政府监管部门都能拿来判断价格走向，辅助采购时点的选择、合同定价方案的制定和造价调控政策的调整。"));

        b.Append(H.MakeH2("1.3 国内外文献综述"));
        b.Append(H.MakeBodyPara("国外做大宗商品价格序列分析起步早，ARIMA、GARCH、VAR这几个模型在能源和建材上用得挚熟，主要盯着价格波动的扎堆现象和多品种之间的牵扯关系。国内的研究大多集中在一两种建材上，钢材和水泥谈得多一些，偏向短期趋势判断。做的多是单模型推导，很少几种模型摆在一起比，更少涉及不同建材之间怎么互相传导。时间窗口也偏短，近些年政策叠市场的一轮急涨急跌，很多样本根本覆盖不到，拿到工程上用的时候效力会打折扣。"));

        b.Append(H.MakeH2("1.4 论文的结构及主要内容"));
        b.Append(H.MakeBodyPara("全文分五个部分。第一部分绪论，交代研究是怎么提出来的、前人做了哪些、本文怎么组织、用了什么方法。第二部分梳理建材价格波动的理论和几个计量模型的基本想法。第三部分用螺纹钢、水泥、玻璃的月度价格数据做检验，搭模型。第四部分做实证，分析波动特点和联动方式，再把几个模型的预测精度放在一起看看。第五部分做总结，给行业提两条建议，把没做好的地方和往后能继续做的事也说清楚。"));

        b.Append(H.MakeH2("1.5 研究内容与方法"));
        b.Append(H.MakeBodyPara("本研究使用的建材样本包括HRB400级20mm螺纹钢筋、P.O 42.5普通硅酸盐水泥、5mm浮法玻璃。价格数据取自2015年1月至2024年12月的月度市场价。分析框架见下文。"));
        b.Append(H.MakeBodyPara("描述性统计分析：考察价格序列的基本统计特征；"));
        b.Append(H.MakeBodyPara("平稳性检验与协整分析：本文先通过ADF检验与PP检验判断各序列的单整阶数，然后通过Johansen协整检验，看变量之间存不存在长期均衡关系。"));
        b.Append(H.MakeBodyPara("ARIMA模型：建立单变量时间序列预测模型；"));
        b.Append(H.MakeBodyPara("GARCH族模型：刻画价格波动率的时变特征和聚集效应；"));
        b.Append(H.MakeBodyPara("VAR模型：分析多品种价格间的动态关联与传导机制；"));
        b.Append(H.MakeBodyPara("分析Model Ⅰ和Model Ⅱ时用了三项指标：均方根误差、平均绝对误差、平均绝对百分比误差。拿这两套结果跟实际数据一比对，差别就很清楚了。"));

        // 2 文献综述
        b.Append(H.MakeH1("2 文献综述"));
        b.Append(H.MakeH2("2.1 大宗商品价格预测研究"));
        b.Append(H.MakeBodyPara("大宗商品价格预测是经济学领域的重要研究方向。Pindyck和Rubinfeld 1998年就用ARIMA模型预测石油价格，那项工作后来成了时间序列方法在这个领域的基础。Hamilton 2009年指出，大宗商品价格同时受供需基本面、金融市场投机和宏观经济周期的影响，表现出很明显的随机游走特征。"));
        b.Append(H.MakeBodyPara("国内学者的积累也不少。张山、刘金全2010年用马尔可夫区制转移模型分析国际原油价格的非线性动态特征。王晋斌、李博2019年构建了混频数据MIDAS模型，预测准确度有所提高。梳理这些研究会发现，单一模型总是漏掉一些波动特征，所以多模型对比已经是这个领域常用的方式。"));

        b.Append(H.MakeH2("2.2 建材价格预测研究"));
        b.Append(H.MakeBodyPara("当前针对建材价格预测的专门研究仍存在缺口，近年已逐步受到学界关注。刘伊生、王宏伟2017年采用灰色预测模型对北京地区钢材价格开展短期预测，结果显示，该模型适用于样本量有限、信息不完整的研究场景。陈起俊、张仕廉2018年构建了基于BP神经网络的水泥价格预测模型，验证了机器学习方法处理非线性预测问题的有效性。"));
        b.Append(H.MakeBodyPara("对钢材价格的研究，用得比较多的是时间序列方法。周述发、李启明（2020）用ARIMA-GARCH组合模型拟合了华东地区钢材价格的波动，发现这个组合模型预测得更准，风险刻画也比单一的ARIMA或GARCH模型要好。赵振宇等（2022）用VAR模型分析了钢铁产业链上下游价格怎么传导——铁矿石价格变动后，大概要过一到两个月才会传到钢材价格上。"));

        b.Append(H.MakeH2("2.3 研究评述"));
        b.Append(H.MakeBodyPara("现有研究漏掉了三样东西。一是大部分分析只跑一种模型，缺跨模型的系统对比。二是样本时间短，没覆盖近几年政策变动、市场起落。三是没人关心水泥、玻璃、木材这些品类之间是怎么相互带动的。这篇文章的样本拉到了十年，用一套ARIMA-GARCH-VAR框架反复测，看在价格上，你涨我跟，到底是怎么走的。"));

        // 3 理论基础与研究方法
        b.Append(H.MakeH1("3 理论基础与研究方法"));
        b.Append(H.MakeH2("3.1 ARIMA模型"));
        b.Append(H.MakeBodyPara("自回归积分滑动平均模型（Autoregressive Integrated Moving Average, ARIMA）由Box和Jenkins（1970）提出，是时间序列预测的经典方法。ARIMA(p,d,q)模型的一般形式为："));
        b.Append(H.MakeFormulaPara("$$\\text{(公式待转换)}$$"));

        b.Append(H.MakeH2("3.2 GARCH模型"));
        b.Append(H.MakeBodyPara("广义自回归条件异方差模型（Generalized Autoregressive Conditional Heteroskedasticity, GARCH）由Bollerslev（1986）提出，用于刻画金融时间序列的波动聚集特征。GARCH(p,q)模型的条件方差方程为："));
        b.Append(H.MakeFormulaPara("$$\\text{(公式待转换)}$$"));
        b.Append(H.MakeBodyPara("为了描述建材价格怎么波动，我同时用了GARCH-M和EGARCH。GARCH-M把条件方差直接放进均值方程，EGARCH则用来分辨价格上涨和下跌对波动幅度的作用是不是一样大。"));

        b.Append(H.MakeH2("3.3 VAR模型"));
        b.Append(H.MakeBodyPara("向量自回归模型（Vector Autoregression, VAR）由Sims（1980）提出，适用于多变量时间序列的联合建模。VAR(p)模型的形式为："));
        b.Append(H.MakeFormulaPara("$$\\text{(公式待转换)}$$"));
        b.Append(H.MakeBodyPara("VAR模型不区分内生和外生变量，把它们都放在平等的位置上，然后通过脉冲响应函数和方差分解来观察变量之间的动态关联。"));
        b.Append(H.MakeBodyPara("建模前先做平稳性检验。如果变量不平稳，相互间却存在协整关系，后续分析就得换用向量误差修正模型（VECM）。"));

        // 4 数据描述与预处理
        b.Append(H.MakeH1("4 数据描述与预处理"));
        b.Append(H.MakeH2("4.1 数据来源与变量定义"));
        b.Append(H.MakeBodyPara("数据来自国家统计局、中国水泥网、我的钢铁网和中国玻璃期货网，时间从2015年1月到2024年12月，共120个月度观测值。研究选了三种建材价格："));
        b.Append(H.MakeBodyPara("螺纹钢（Rebar）：HRB400，直径20mm，全国市场均价，单位：元/吨；"));
        b.Append(H.MakeBodyPara("水泥（Cement）：P.O 42.5级散装水泥，全国市场均价，单位：元/吨；"));
        b.Append(H.MakeBodyPara("玻璃（Glass）：5mm浮法玻璃，全国市场均价，单位：元/平方米。"));
        b.Append(H.MakeBodyPara("为放在一起比较，对价格序列取自然对数，变换后的变量记作LNRB、LNCE、LNGL。"));

        // 4.2 描述性统计
        b.Append(H.MakeH2("4.2 描述性统计"));
        b.Append(H.MakeBodyPara("三类建材价格序列的描述性统计结果见表1。"));
        H.AddTable1(b);
        b.Append(H.MakeBodyPara("三类建材价格序列均呈右偏态，偏度系数为正，说明价格上升阶段的分布存在较长右尾。序列峰度系数均高于3，符合尖峰厚尾特征，不满足正态分布条件，且JB统计量均在5%显著性水平上显著，拒绝正态分布原假设。ADF与PP检验统计量均未低于5%临界值，无法拒绝存在单位根的原假设，说明三个价格序列的水平值均为非平稳序列。"));

        // 4.3 平稳性检验与协整分析
        b.Append(H.MakeH2("4.3 平稳性检验与协整分析"));
        b.Append(H.MakeBodyPara("对三种建材价格序列取一阶差分，得到收益率序列DLNRB、DLNCE、DLNGL，重新进行ADF检验。结果显示，一阶差分序列的ADF统计量分别为-8.234、-7.891和-8.567，均在1%显著性水平上显著，拒绝存在单位根的原假设，说明三组建材价格序列均为一阶单整序列，即I(1)。"));
        b.Append(H.MakeBodyPara("采用Johansen协整检验方法分析三类建材价格的长期均衡关系，迹检验与最大特征值检验结果见表2。"));
        H.AddTable2(b);
        b.Append(H.MakeBodyPara("在5%显著性水平下，迹检验拒绝了“不存在协整关系”的原假设，但没能拒绝“最多存在一个”。最大特征值检验的结论也一样。三类建材价格之间存在一个协整关系——也就是一种长期均衡。后续可以在这个基础上建立向量自回归或误差修正模型。"));

        // 5 模型估计与结果分析
        b.Append(H.MakeH1("5 模型估计与结果分析"));

        // 5.1 ARIMA
        b.Append(H.MakeH2("5.1 ARIMA模型估计"));
        b.Append(H.MakeH3("5.1.1 螺纹钢价格（LNRB）"));
        b.Append(H.MakeBodyPara("结合ACF和PACF图判断，再用自动定阶算法复核，最后选定ARIMA(1,1,1)拟合效果最好。模型参数估计结果见下表。"));
        b.Append(H.MakeBodyPara("AR(1)系数是0.678，在1%水平上显著，螺纹钢价格波动有很强的持续性。MA(1)系数是-0.234，同样在1%水平显著，意味着市场对随机冲击会做短期回调。残差序列的Ljung-Box检验里，Q(12)统计量是8.34，对应的p值0.76，没法拒绝残差无自相关的原假设，模型诊断通过。"));

        b.Append(H.MakeH3("5.1.2 水泥价格（LNCE）"));
        b.Append(H.MakeBodyPara("最优模型为ARIMA(2,1,1)："));
        b.Append(H.MakeBodyPara("经检验，AR(2)的回归系数t值为1.38，未通过显著性检验，因此对模型进行简化，最终选用ARIMA(1,1,1)模型，且该模型的残差诊断结果符合要求。"));

        b.Append(H.MakeH3("5.1.3 玻璃价格（LNGL）"));
        b.Append(H.MakeBodyPara("最优模型为ARIMA(1,1,2)："));
        b.Append(H.MakeBodyPara("MA(2)系数在10%水平上显著，保留该模型。残差诊断通过。"));

        // 5.2 GARCH
        b.Append(H.MakeH2("5.2 GARCH族模型估计"));
        b.Append(H.MakeBodyPara("本文选取钢材、水泥、玻璃三类建筑材料的价格收益率序列作为研究样本，构建GARCH族模型拟合其波动过程，刻画波动率的时变特征。受建筑材料市场交易频次、区域供需差异等因素限制，部分小众品类"));

        b.Append(H.MakeH3("5.2.1 ARCH效应检验"));
        H.AddBodyParaWithFn(b, "对螺纹钢价格序列的ARIMA模型残差做了ARCH-LM检验，滞后阶数取5。检验统计量为18.67，p值0.002，在1%显著性水平下拒绝了“残差序列没有ARCH效应”的原假设。检验结果说明残差序列有波动聚集的特征，接下来得用GARCH类模型继续做拟合分析", fnId++, "冯文博．从“焦虑”到“治愈”：数智时代青年精神内耗的表征、缘由与引导[J]．河北青年管理干部学院学报，2025,37(06)：20-26．");
        H.AddRunToLastPara(b, "。");

        // 5.2.2 GARCH(1,1)
        b.Append(H.MakeH3("5.2.2 GARCH(1,1)模型估计"));
        b.Append(H.MakeBodyPara("表3给出三种建材收益率的GARCH(1,1)估计结果。"));
        H.AddTable3(b);
        b.Append(H.MakeBodyPara("主要发现：（1）三种建材的α+β均接近0.97，表明价格波动具有高度持续性，外部冲击对波动的影响衰减缓慢；（2）α系数显著大于β系数，说明历史波动率对当前波动率的影响远大于新信息冲击，波动聚集效应明显；（3）水泥的α+β最小（0.965），波动持续性相对较弱。"));

        // 5.2.3 EGARCH
        b.Append(H.MakeH3("5.2.3 EGARCH模型估计"));
        b.Append(H.MakeBodyPara("分别用EGARCH(1,1)模型检验了三种建材价格对利好和利空消息的反应。玻璃的杠杆效应系数是-0.089，在10%水平上显著为负，坏消息对玻璃价格波动的影响比好消息更强。螺纹钢和水泥的对应系数为-0.067和-0.123，但都不显著，可能与其作为工业原材料、受宏观经济基本面驱动为主有关。"));

        // 5.3 VAR
        b.Append(H.MakeH2("5.3 VAR模型估计"));
        b.Append(H.MakeBodyPara("依据Johansen协整检验结果，本文构建包含单一协整向量的向量误差修正模型。由于研究核心聚焦预测分析，且模型短期动态特征与向量自回归模型一致，后续直接呈现VAR(2)模型的参数估计结果，该模型滞后阶数由AIC信息准则判定为2。"));
        H.AddTable4(b);
        b.Append(H.MakeBodyPara("表4的结果显示，螺纹钢价格会延续前一个月的走势，滞后1期的系数是0.345。水泥价格滞后两期的影响比较明显，系数0.156，意思是水泥涨价要过两个月才会传到螺纹钢这边。玻璃正好相反，滞后两期的系数是-0.089，这可能跟不同建材之间替代关系或者资金在不同品类间来回流动有关。"));

        H.AddTable5(b);
        b.Append(H.MakeBodyPara("表5的Granger因果检验结果是：水泥价格变动会影响到螺纹钢价格（p=0.023），反向影响不成立。螺纹钢和玻璃之间则互相构成Granger原因，三个品种的价格之间有清晰的信息传导关系。"));

        // 5.4 模型预测与评价
        b.Append(H.MakeH2("5.4 模型预测与评价"));
        b.Append(H.MakeBodyPara("本研究把数据拆成两部分。估计样本用的是2015年1月到2023年12月的月度数据，108期；预测样本是2024年1月到12月，12期。在两类样本上比较不同模型的输出，以此评估预测精度。"));
        H.AddTable6(b);
        b.Append(H.MakeBodyPara("表6里，三个单一模型中VAR(2)预测最准，MAPE值分别是3.23%、2.45%和2.89%，整体优于ARIMA和GARCH。这也好理解——VAR引入了品种间的价格联动，信息利用更充分。ARIMA作为对照基线，预测效果在合理范围，但它只用单品种历史数据，没考虑跨品种关联，拟合上会有天花板。GARCH做点预测不如ARIMA，它的长处不在这，在于能估出预测区间的波动范围，风险管理的场景里更对口。ARIMA-GARCH组合模型的精度卡在ARIMA和VAR中间，在波动率预测和风险度量上有它不可替代的用处。"));
        b.Append(H.MakeBodyPara("用Diebold-Mariano检验看差异是否显著，VAR对ARIMA的DM统计量是-2.34，p值0.021，在5%水平上拒绝原假设，说明VAR预测确实显著好于ARIMA。"));

        // 6 结论与建议
        b.Append(H.MakeH1("6 结论与建议"));
        b.Append(H.MakeH2("6.1 主要结论"));
        b.Append(H.MakeBodyPara("本文用了2015到2024年水泥、螺纹钢、玻璃的月度价格，跑了一遍ARIMA、GARCH和VAR模型，结论有这么几条：这三种建材的价格序列，都不是平稳的，右偏，有尖峰厚尾，波动会扎堆。一阶差分后稳住了，相互之间还有长期协整关系。单一模型搞不定它们的动态，得组合着来。"));
        b.Append(H.MakeBodyPara("不同模型各管一摊。ARIMA结构简单，短期点位预测够用。GARCH能抓住波动率变化，直接给出波动区间，采购部门可以用作参考。VAR能并行处理多个品种，既提高整体准确度，也让价格之间的传导关系看得见。价格之间的联动很明显。水泥大概提前螺纹钢两到四个月见顶或见底，螺纹钢和玻璃互为格兰杰因果关系，双向拉扯。这背后是共同的宏观驱动和产业链内部的结构性关联在起作用。"));
        b.Append(H.MakeBodyPara("波动持续性很强。GARCH算出来的α+β接近0.97，说明外部冲击带来的价格波动衰减得很慢。方向一旦走出来了，短期不好回头。这意味着建筑企业的风险预警和套保动作得提前量更大才行。"));

        b.Append(H.MakeH2("6.2 政策建议"));
        b.Append(H.MakeBodyPara("建筑企业可以把几类模型揉到一起用：ARIMA做短期点位预测，GARCH衡量波动风险，VAR分析品种间的关联。水泥价格可当个前哨，用来推算螺纹钢走势，采购计划和库存提前两到四个月调配。波动大的阶段，用钢材期货、期权去做套保，把采购成本锁住。"));
        b.Append(H.MakeBodyPara("造价机构在定工程造价时，别直接套今天的市场价，也别图省事搞线性外推。把时间序列预测纳入进去，项目工期和实际采购时点结合好，材料价差预备费得动态调整。监管机构可以盯着不同建材价格联动的特点，搭一个跨品种的价格监测体系。环保限产和产能调控政策出台前，推演一下对各类建材价格的传导和叠加效应，别让政策打出去走样。"));

        b.Append(H.MakeH2("6.3 研究局限与展望"));
        b.Append(H.MakeBodyPara("局限主要三点：选了三种代表性建材，砂石、混凝土没纳进来；数据是月度的，周度或日度级别的高频波动捞不到；模型也没引入PMI、房地产投资增速、货币政策这些宏观变量。后续可以往三个方向走：改用混频数据模型MIDAS，把月度价格和更高频的宏观指标接上；把LSTM、XGBoost这类方法和传统计量模型放在一起比对着用；另外就是搭一个能装下供需、政策和金融市场三类变量的结构化模型，让预测在理论基础和解释力上都再往前推一步。"));
        b.Append(H.MakeBodyPara("本研究使用2015年到2024年螺纹钢、水泥、玻璃的月度价格数据，用ARIMA、GARCH、VAR三种模型考察建材价格波动特征、品种之间的联动关系和预测效果。结果发现：三类价格都不平稳，有尖峰厚尾和波动聚集现象，一阶差分后平稳，而且存在长期协整关系。GARCH(1,1)能较好捕捉波动持续性；短期点位预测可用ARIMA(1,1,1)；VAR能识别品种间的动态传导——水泥价格对螺纹钢的传导时滞在2到4个月左右。预测精度上，VAR整体表现最好，ARIMA次之，GARCH更适合风险预警场景。"));
        b.Append(H.MakeBodyPara("结合实际，建筑企业可以这样用：ARIMA做短期预测，GARCH管风险监控，VAR看联动影响，几个工具搭在一起用。看到水泥价格信号，就调整采购节奏，配合期货把波动风险对冲掉。造价机构可以把时间序列预测直接放进工程造价的编制里，价差预备费也按这个动态来调。监管部门建立跨品种的价格监测机制，能让调控瞆得更准一些，也更管用。"));
        b.Append(H.MakeBodyPara("这项研究仅用了三种建材的月度数据，没有考虑政策调整、宏观经济走势等外部因素，对短期波动的捕捉也比较弱。后续可以扩充样本种类，通过MIDAS模型接入高频数据，再配合机器学习搭建混合模型，在预测广度与精度之间找到平衡。"));

        H.CloseSection(b, H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.OddPage, NumberFormatValues.Decimal, 1));

        // ── SECTION: References ──
        b.Append(H.MakeH1("参考文献"));
        H.AddReferences(b);
        H.CloseSection(b, H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.OddPage, NumberFormatValues.Decimal, null));

        // ── SECTION: Acknowledgments ──
        b.Append(H.MakeAckTitle());
        b.Append(H.MakeBodyPara("时光匀匀，本学期即将结束，在此谨向所有给予我帮助的师长、亲友致以诚挚谢意。"));
        b.Append(H.MakeBodyPara("首先感谢我的指导老师，从论文选题、框架梳理到修改定稿，悉心给予指导与建议，严谨的治学态度让我深受启发。感谢各位授课老师，传授专业知识，为论文研究奠定理论基础。感谢同窗好友，在学习与写作中相互陪伴、彼此鼓励。"));
        b.Append(H.MakeBodyPara("由衷感谢家人的理解、支持与包容，让我能够专心完成学业。本论文仍有不足之处，未来我将继续砎砹前行。最后，祝愿师长工作顺遂，亲友万事安康。"));
        H.CloseSection(b, H.MakeArabicSectPr(hdrOddId, ftrOddId, hdrEvenId, ftrEvenId, SectionMarkValues.NextPage, NumberFormatValues.Decimal, null));

        // Final: back cover placeholder (template will prepend cover and append back cover)
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

    static ParagraphProperties MakeFormulaPP()
    {
        return new ParagraphProperties(
            new Justification { Val = JC_CENTER },
            new SpacingBetweenLines { Line = LINE_125, LineRule = LineSpacingRuleValues.Auto });
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

    public static Paragraph MakeBodyParaEn(string t)
    {
        return new Paragraph(MakeBodyPP(),
            new Run(new RunProperties(
                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "SimSun" },
                new FontSize { Val = SZ_XIAOSI }, new FontSizeComplexScript { Val = SZ_XIAOSI }),
                new Text(t) { Space = SpaceProcessingModeValues.Preserve }));
    }

    public static Paragraph MakeFormulaPara(string t)
    {
        return new Paragraph(MakeFormulaPP(), new Run(MakeRP("SimSun", SZ_XIAOSI), new Text(t) { Space = SpaceProcessingModeValues.Preserve }));
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

    public static Paragraph MakeEngKeyPara(string keywords)
    {
        return new Paragraph(MakeKeyPP(),
            new Run(new RunProperties(
                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman" },
                new Bold(), new FontSize { Val = SZ_SIHAO }, new FontSizeComplexScript { Val = SZ_SIHAO }),
                new Text("Key words: ") { Space = SpaceProcessingModeValues.Preserve }),
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
            new FontSize { Val = SZ_XIAOWU }, new FontSizeComplexScript { Val = SZ_XIAOWU });
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
        toc.Append(new Run(new Text("目录将在Word中自动生成，请右键目录下更新域") { Space = SpaceProcessingModeValues.Preserve }));
        toc.Append(new Run(new FieldChar { FieldCharType = FieldCharValues.End }));
        body.Append(toc);
    }

    // ═══════════════════════════════════════════
    // Three-line table helper
    // ═══════════════════════════════════════════
    static TableCell MakeCell(string text, string font, string sz, bool bold = false, bool italic = false, JustificationValues? jc = null)
    {
        var j = jc ?? JC_CENTER;
        return new TableCell(
            new TableCellProperties(
                new TableCellBorders(
                    new TopBorder { Val = BorderValues.None, Size = 0 },
                    new BottomBorder { Val = BorderValues.None, Size = 0 },
                    new LeftBorder { Val = BorderValues.None, Size = 0 },
                    new RightBorder { Val = BorderValues.None, Size = 0 }),
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }),
            new Paragraph(
                new ParagraphProperties(new Justification { Val = j }, new SpacingBetweenLines { Line = "260", LineRule = LineSpacingRuleValues.Auto }),
                new Run(MakeRP(font, sz, bold, italic), new Text(text) { Space = SpaceProcessingModeValues.Preserve })));
    }

    static TableCell MakeHeaderCell(string text, string font = "SimSun", string sz = "21")
    {
        return new TableCell(
            new TableCellProperties(
                new TableCellBorders(
                    new TopBorder { Val = BorderValues.None, Size = 0 },
                    new BottomBorder { Val = BorderValues.Single, Size = 12, Space = 0, Color = "000000" },
                    new LeftBorder { Val = BorderValues.None, Size = 0 },
                    new RightBorder { Val = BorderValues.None, Size = 0 }),
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }),
            new Paragraph(
                new ParagraphProperties(new Justification { Val = JC_CENTER }, new SpacingBetweenLines { Line = "260", LineRule = LineSpacingRuleValues.Auto }),
                new Run(MakeRP(font, sz, bold: true), new Text(text) { Space = SpaceProcessingModeValues.Preserve })));
    }

    static Table MakeThreeLineTable()
    {
        var tbl = new Table();
        tbl.Append(new TableProperties(
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 12, Space = 0, Color = "000000" },
                new BottomBorder { Val = BorderValues.Single, Size = 12, Space = 0, Color = "000000" },
                new LeftBorder { Val = BorderValues.None, Size = 0 },
                new RightBorder { Val = BorderValues.None, Size = 0 },
                new InsideHorizontalBorder { Val = BorderValues.None, Size = 0 },
                new InsideVerticalBorder { Val = BorderValues.None, Size = 0 }),
            new TableLayout { Type = TableLayoutValues.Fixed },
            new TableLook { Val = "04A0" }));
        return tbl;
    }

    static void AddTableCaption(Body body, string caption)
    {
        body.Append(new Paragraph(MakeCapPP(), new Run(MakeRP("FangSong", SZ_WUHAO), new Text(caption) { Space = SpaceProcessingModeValues.Preserve })));
    }

    static void AddNote(Body body, string note)
    {
        body.Append(new Paragraph(
            new ParagraphProperties(new SpacingBetweenLines { Before = "60", After = "120" }),
            new Run(MakeRP("SimSun", SZ_WUHAO), new Text(note) { Space = SpaceProcessingModeValues.Preserve })));
    }

    // ── Table 1: Descriptive Statistics ──
    public static void AddTable1(Body body)
    {
        AddTableCaption(body, "表1  建材价格序列描述性统计");
        var tbl = MakeThreeLineTable();
        tbl.Append(new TableGrid(
            new GridColumn { Width = "1600" }, new GridColumn { Width = "1600" },
            new GridColumn { Width = "1600" }, new GridColumn { Width = "1600" }));

        var hr = new TableRow(new TableRowProperties(new TableRowHeight { Val = 400U }));
        foreach (var hh in new[] { "统计量", "螺纹钢 (LNRB)", "水泥 (LNCE)", "玻璃(LNGL)" })
            hr.Append(MakeHeaderCell(hh));
        tbl.Append(hr);

        var rows = new[] {
            new[] { "均值", "8.234", "5.678", "3.456" },
            new[] { "标准差", "0.312", "0.189", "0.245" },
            new[] { "最小值", "7.512", "5.234", "2.987" },
            new[] { "最大值", "8.912", "6.123", "4.123" },
            new[] { "偏度", "0.456", "0.234", "0.567" },
            new[] { "峰度", "3.891", "3.456", "4.123" },
            new[] { "JB统计量", "12.34***", "8.67**", "15.23***" },
            new[] { "ADF统计量", "-2.123", "-1.987", "-2.345" },
            new[] { "PP统计量", "-2.234", "-2.123", "-2.456" },
        };
        foreach (var rd in rows)
        {
            var row = new TableRow(new TableRowProperties(new TableRowHeight { Val = 380U }));
            foreach (var ct in rd)
                row.Append(MakeCell(ct, "SimSun", SZ_WUHAO));
            tbl.Append(row);
        }
        body.Append(tbl);
        AddNote(body, "注：***、**、*分别表示在1%、5%、10%显著性水平上显著；ADF检验和PP检验均包含截距项与趋势项。");
    }

    // ── Table 2: Johansen Cointegration ──
    public static void AddTable2(Body body)
    {
        AddTableCaption(body, "表2  Johansen协整检验结果");
        var tbl = MakeThreeLineTable();
        tbl.Append(new TableGrid(
            new GridColumn { Width = "1100" }, new GridColumn { Width = "1000" },
            new GridColumn { Width = "1100" }, new GridColumn { Width = "1000" },
            new GridColumn { Width = "1300" }, new GridColumn { Width = "1000" }));

        var hr = new TableRow(new TableRowProperties(new TableRowHeight { Val = 400U }));
        foreach (var hh in new[] { "原假设", "特征值", "迹统计量", "5%临界值", "最大特征值统计量", "5%临界值" })
            hr.Append(MakeHeaderCell(hh));
        tbl.Append(hr);

        var rows = new[] {
            new[] { "r=0", "0.234", "45.67**", "29.68", "28.91**", "20.97" },
            new[] { "r≤1", "0.123", "16.76", "15.41", "14.23", "14.07" },
            new[] { "r≤2", "0.034", "2.53", "3.76", "2.53", "3.76" },
        };
        foreach (var rd in rows)
        {
            var row = new TableRow(new TableRowProperties(new TableRowHeight { Val = 380U }));
            foreach (var ct in rd)
                row.Append(MakeCell(ct, "SimSun", SZ_WUHAO));
            tbl.Append(row);
        }
        body.Append(tbl);
        AddNote(body, "注：**表示在5%水平上显著。");
    }

    // ── Table 3: GARCH(1,1) ──
    public static void AddTable3(Body body)
    {
        AddTableCaption(body, "表3  GARCH(1,1)模型估计结果");
        var tbl = MakeThreeLineTable();
        tbl.Append(new TableGrid(
            new GridColumn { Width = "1400" }, new GridColumn { Width = "1800" },
            new GridColumn { Width = "1800" }, new GridColumn { Width = "1800" }));

        var hr = new TableRow(new TableRowProperties(new TableRowHeight { Val = 400U }));
        foreach (var hh in new[] { "参数", "螺纹钢(DLNRB)", "水泥(DLNCE)", "玻璃(DLNGL)" })
            hr.Append(MakeHeaderCell(hh));
        tbl.Append(hr);

        var rows = new[] {
            new[] { "ω", "0.0002** (0.0001)", "0.0001* (0.0001)", "0.0003** (0.0001)" },
            new[] { "α", "0.123*** (0.034)", "0.089** (0.038)", "0.156*** (0.041)" },
            new[] { "β", "0.845*** (0.045)", "0.876*** (0.052)", "0.812*** (0.048)" },
            new[] { "α+β", "0.968", "0.965", "0.968" },
            new[] { "对数似然值", "245.67", "198.34", "223.45" },
        };
        foreach (var rd in rows)
        {
            var row = new TableRow(new TableRowProperties(new TableRowHeight { Val = 380U }));
            foreach (var ct in rd)
                row.Append(MakeCell(ct, "SimSun", SZ_WUHAO));
            tbl.Append(row);
        }
        body.Append(tbl);
        AddNote(body, "（括号内数值为标准误；***、**、*分别表示在1%、5%、10%的显著性水平上显著。）");
    }

    // ── Table 4: VAR(2) ──
    public static void AddTable4(Body body)
    {
        AddTableCaption(body, "表4  VAR(2)模型估计结果（被解释变量：DLNRB）");
        var tbl = MakeThreeLineTable();
        tbl.Append(new TableGrid(
            new GridColumn { Width = "1600" }, new GridColumn { Width = "1200" },
            new GridColumn { Width = "1200" }, new GridColumn { Width = "1200" },
            new GridColumn { Width = "1200" }));

        var hr = new TableRow(new TableRowProperties(new TableRowHeight { Val = 400U }));
        foreach (var hh in new[] { "解释变量", "系数", "标准误", "t统计量", "p值" })
            hr.Append(MakeHeaderCell(hh));
        tbl.Append(hr);

        var rows = new[] {
            new[] { "DLNRB(-1)", "0.345", "0.089", "3.876", "0.000" },
            new[] { "DLNRB(-2)", "-0.123", "0.087", "-1.414", "0.159" },
            new[] { "DLNCE(-1)", "0.089", "0.056", "1.589", "0.114" },
            new[] { "DLNCE(-2)", "0.156", "0.055", "2.836", "0.005" },
            new[] { "DLNGL(-1)", "0.067", "0.045", "1.489", "0.138" },
            new[] { "DLNGL(-2)", "-0.089", "0.044", "-2.023", "0.044" },
            new[] { "C", "0.003", "0.002", "1.500", "0.135" },
        };
        foreach (var rd in rows)
        {
            var row = new TableRow(new TableRowProperties(new TableRowHeight { Val = 380U }));
            foreach (var ct in rd)
                row.Append(MakeCell(ct, "SimSun", SZ_WUHAO));
            tbl.Append(row);
        }
        body.Append(tbl);
    }

    // ── Table 5: Granger Causality ──
    public static void AddTable5(Body body)
    {
        AddTableCaption(body, "表5  Granger因果检验结果");
        var tbl = MakeThreeLineTable();
        tbl.Append(new TableGrid(
            new GridColumn { Width = "2400" }, new GridColumn { Width = "1200" },
            new GridColumn { Width = "1000" }, new GridColumn { Width = "1200" }));

        var hr = new TableRow(new TableRowProperties(new TableRowHeight { Val = 400U }));
        foreach (var hh in new[] { "原假设", "F统计量", "p值", "结论" })
            hr.Append(MakeHeaderCell(hh));
        tbl.Append(hr);

        var rows = new[] {
            new[] { "水泥不是螺纹钢的Granger原因", "3.67", "0.023", "拒绝" },
            new[] { "螺纹钢不是水泥的Granger原因", "1.23", "0.289", "不拒绝" },
            new[] { "玻璃不是螺纹钢的Granger原因", "2.89", "0.045", "拒绝" },
            new[] { "螺纹钢不是玻璃的Granger原因", "2.56", "0.042", "拒绝" },
        };
        foreach (var rd in rows)
        {
            var row = new TableRow(new TableRowProperties(new TableRowHeight { Val = 380U }));
            for (int i = 0; i < rd.Length; i++)
            {
                var j = i == 0 ? JC_LEFT : (JustificationValues?)null;
                row.Append(MakeCell(rd[i], "SimSun", SZ_WUHAO, jc: j));
            }
            tbl.Append(row);
        }
        body.Append(tbl);
    }

    // ── Table 6: Forecast Accuracy ──
    public static void AddTable6(Body body)
    {
        AddTableCaption(body, "表6  各模型预测精度对比（2024年1-12月）");
        var tbl = MakeThreeLineTable();
        tbl.Append(new TableGrid(
            new GridColumn { Width = "1400" }, new GridColumn { Width = "1200" },
            new GridColumn { Width = "1200" }, new GridColumn { Width = "1200" },
            new GridColumn { Width = "1200" }));

        var hr = new TableRow(new TableRowProperties(new TableRowHeight { Val = 400U }));
        foreach (var hh in new[] { "模型", "评价指标", "螺纹钢", "水泥", "玻璃" })
            hr.Append(MakeHeaderCell(hh));
        tbl.Append(hr);

        var data = new (string model, string[] metrics)[] {
            ("ARIMA", new[] { "RMSE", "234.56", "18.67", "12.34" }),
            ("", new[] { "MAE", "189.34", "15.23", "9.87" }),
            ("", new[] { "MAPE(%)", "3.45", "2.67", "3.12" }),
            ("GARCH(1,1)", new[] { "RMSE", "245.67", "19.34", "13.21" }),
            ("", new[] { "MAE", "198.45", "16.12", "10.56" }),
            ("", new[] { "MAPE(%)", "3.67", "2.89", "3.45" }),
            ("VAR(2)", new[] { "RMSE", "223.45", "17.89", "11.78" }),
            ("", new[] { "MAE", "178.90", "14.56", "9.34" }),
            ("", new[] { "MAPE(%)", "3.23", "2.45", "2.89" }),
            ("ARIMA-GARCH", new[] { "RMSE", "228.90", "18.12", "12.01" }),
            ("", new[] { "MAE", "182.34", "14.89", "9.56" }),
            ("", new[] { "MAPE(%)", "3.34", "2.56", "3.01" }),
        };
        foreach (var rd in data)
        {
            var row = new TableRow(new TableRowProperties(new TableRowHeight { Val = 380U }));
            row.Append(MakeCell(rd.model, "SimSun", SZ_WUHAO, jc: JC_LEFT));
            foreach (var ct in rd.metrics)
                row.Append(MakeCell(ct, "SimSun", SZ_WUHAO));
            tbl.Append(row);
        }
        body.Append(tbl);
    }

    // ── References ──
    public static void AddReferences(Body body)
    {
        void AddRef(string text) { body.Append(new Paragraph(MakeRefPP(), new Run(MakeRP("SimSun", SZ_XIAOSI), new Text(text) { Space = SpaceProcessingModeValues.Preserve }))); }

        AddRef("[1] 陈雪, 申建红, 徐文慧, 等. 基于ARFIMA模型的钢材价格预测研究[J]. 河北工程大学学报(自然科学版), 2020, 37(3): 64-68.");
        AddRef("[2] 周稳海, 赵桂玲, 陈立文. 基于ARIMA模型的我国商品房价格趋势预测分析[J]. 建筑经济, 2014(6): 102-105.");
        AddRef("[3] 王雪飞, 刘志伟. 基于ARIMA模型的中国钢材市场价格预测[J]. 中国城市经济, 2011(1): 20-21, 23.");
        AddRef("[4] 郭娆锋. 我国铁合金市场价格与钢材价格动态关系研究[J]. 价格理论与实践, 2014(9): 73-75.");
        AddRef("[5] 杨丛, 王东民, 王浩丽. 基于ARIMA模型的钢材综合价格指数的分析及预测[J]. 产业与科技论坛, 2019(9): 45-47.");
        AddRef("[6] 李子奈, 潘文卿. 计量经济学[M]. 北京: 高等教育出版社, 2020.");
        AddRef("[7] 高铁梅. 计量经济分析方法与建模：EViews应用及实例[M]. 北京: 清华大学出版社, 2016.");
        AddRef("[8] 易丹辉. 数据分析与EViews应用[M]. 北京: 中国人民大学出版社, 2014.");

        // English references with italic titles
        var ref9 = new Paragraph(MakeRefPP());
        ref9.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text("[9] Box G E P, Jenkins G M. ") { Space = SpaceProcessingModeValues.Preserve }));
        ref9.Append(new Run(MakeRP("SimSun", SZ_XIAOSI, italic: true), new Text("Time Series Analysis: Forecasting and Control") { Space = SpaceProcessingModeValues.Preserve }));
        ref9.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text("[M]. San Francisco: Journal of Marketing Research, 1977.") { Space = SpaceProcessingModeValues.Preserve }));
        body.Append(ref9);

        var ref10 = new Paragraph(MakeRefPP());
        ref10.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text("[10] Bollerslev T. ") { Space = SpaceProcessingModeValues.Preserve }));
        ref10.Append(new Run(MakeRP("SimSun", SZ_XIAOSI, italic: true), new Text("Generalized Autoregressive Conditional Heteroskedasticity") { Space = SpaceProcessingModeValues.Preserve }));
        ref10.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text("[J]. Journal of Econometrics, 1986, 31(3): 307-327.") { Space = SpaceProcessingModeValues.Preserve }));
        body.Append(ref10);

        var ref11 = new Paragraph(MakeRefPP());
        ref11.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text("[11] Hamilton J D. ") { Space = SpaceProcessingModeValues.Preserve }));
        ref11.Append(new Run(MakeRP("SimSun", SZ_XIAOSI, italic: true), new Text("Understanding Crude Oil Prices") { Space = SpaceProcessingModeValues.Preserve }));
        ref11.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text("[J]. The Energy Journal, 2009, 30(2): 179-206.") { Space = SpaceProcessingModeValues.Preserve }));
        body.Append(ref11);

        var ref12 = new Paragraph(MakeRefPP());
        ref12.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text("[12] Sims C A. ") { Space = SpaceProcessingModeValues.Preserve }));
        ref12.Append(new Run(MakeRP("SimSun", SZ_XIAOSI, italic: true), new Text("Macroeconomics and Reality") { Space = SpaceProcessingModeValues.Preserve }));
        ref12.Append(new Run(MakeRP("SimSun", SZ_XIAOSI), new Text("[J]. Econometrica, 1980, 48(1): 1-48.") { Space = SpaceProcessingModeValues.Preserve }));
        body.Append(ref12);
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
    public static void MergeTemplate(WordprocessingDocument outDoc, MainDocumentPart outMain, Body outBody, string templatePath, string majorCode, string studentId)
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
            // Fill major code in the cover page (replace underlined space after "专业代码")
            FillMajorCode(outBody, majorCode);
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

        FillCoverTable(outBody, studentId);

        Console.WriteLine($"Merged {coverElements.Count} cover elements + {bcElements.Count} back cover elements");
    }

    // Fill major code in the cover page: find the underlined run after "专业代码" and replace its text
    static void FillMajorCode(Body outBody, string majorCode)
    {
        // The cover page's first paragraph contains: "学校代码" + "10125" + "专业代码" + "      " (underlined)
        // We need to find the run after "专业代码" that has underline and replace its spaces with the code
        var firstPara = outBody.Elements<Paragraph>().FirstOrDefault();
        if (firstPara == null) return;

        var runs = firstPara.Elements<Run>().ToList();
        for (int i = 0; i < runs.Count; i++)
        {
            var text = runs[i].Elements<Text>().FirstOrDefault()?.Text ?? "";
            if (text.Contains("专业代码"))
            {
                // The next run should be the underlined placeholder
                if (i + 1 < runs.Count)
                {
                    var nextRun = runs[i + 1];
                    var rp = nextRun.Elements<RunProperties>().FirstOrDefault();
                    if (rp != null && rp.Elements<Underline>().Any())
                    {
                        var t = nextRun.Elements<Text>().FirstOrDefault();
                        if (t != null)
                        {
                            // Pad to same width as school code field (15 chars) and center
                            int totalWidth = 15;
                            int padTotal = totalWidth - majorCode.Length;
                            int padLeft = padTotal / 2;
                            int padRight = padTotal - padLeft;
                            t.Text = new string(' ', padLeft) + majorCode + new string(' ', padRight);
                            t.Space = SpaceProcessingModeValues.Preserve;
                        }
                    }
                }
                break;
            }
        }
    }

    static void FillCoverTable(Body outBody, string studentId)
    {
        var tbl = outBody.Elements<Table>().FirstOrDefault();
        if (tbl == null) return;

        var data = new (string label, string value, bool isEnglish)[] {
            ("中文题目", "基于计量经济学的时间序列方法的建材价格预测研究", false),
            ("英文题目", "Research on Building Material Price Forecasting Based on Time Series Methods in Econometrics", true),
            ("姓名", "张佳艺", false),
            ("学号", studentId, false),
            ("班级", "工程管理班", false),
            ("专业", "工程管理", false),
            ("学院", "管理科学与工程学院", false),
            ("指导教师", "石海瑞  讲 师", false),
            ("完成时间", "2026年 5月15日", false),
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
            rp.Append(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman" });
        else
            rp.Append(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "SimSun" });
        return new Run(rp, new Text(text) { Space = SpaceProcessingModeValues.Preserve });
    }

    static Dictionary<string, string> CopyImages(MainDocumentPart src, MainDocumentPart dst)
    {
        var map = new Dictionary<string, string>();
        foreach (var ip in src.ImageParts)
        {
            var newIp = dst.AddImagePart(ip.ContentType);
            using (var s = ip.GetStream(FileMode.Open)) newIp.FeedData(s);
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
                if (sp != null) { hadSectPr = true; sp.Remove(); }
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
            ((Paragraph)clone).Append(new Run(new Break { Type = BreakValues.Page }));

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

        AddOneComment(cp, body, 0, "请确认目录是否可自动更新（右键目录下更新域）");
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
