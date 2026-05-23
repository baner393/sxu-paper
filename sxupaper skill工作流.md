# sxupaper skill工作流

本项目包含三个核心功能模块，preprocssor 负责实现 DOCX 学术论文与 Markdown 的可逆双向转换，可提取 DOCX 为带锚点 Markdown 供 AI 编辑，编辑后可回填原 DOCX 并完整保留全部格式；sxuyear 负责将学年论文 Markdown 文件转换为符合山西财经大学论文规范的 Word 文档（\.docx）；lower\-aigc 用于处理 Markdown 文件，降低 AI 生成内容特征，增强人类写作风格后输出处理后的 Markdown 文件。

整体工作流分为两种处理场景，对应不同的论文处理需求，完整流程如下：

```mermaid
flowchart LR
    subgraph 修后论文（降AI）流程
        A[修后论文（降AI）] --> B[带锚点的 preprocessor]
        B --> C[json]
        B --> D[assets/image]
        B --> E[md]
        E --> F[降AI<br/>lower-aigc]
        F --> G[降AI版 md]
        G & C & D --> H[propressor 回填]
        H --> I[处理完成]
    end
    subgraph 无修论文流程
        J[无修论文] --> K[不带描点版的 preprocessor → md<br/><small>correction（待开发）</small>]
        K --> L[降AI<br/>lower-aigc]
        K --> M[不降AI]
        L --> N[降A md]
        N --> O[sxu year]
        M --> O
        O --> P[处理完成]
    end```

针对修后论文的降 AI 处理场景，首先使用带锚点的 preprocessor 对论文进行预处理，拆分出 json、assets/image 和 md 三类中间产物，针对拆分出的 md 文件使用 lower\-aigc 进行降 AI 处理，得到降 AI 版 md，之后将降 AI 后的 md 与之前拆分出的 json、assets/image 产物进行 propressor 回填，完成整个处理流程。

针对无修论文的处理场景，首先使用不带描点版的 preprocessor 处理得到 md 文件，其中 correction 模块目前待开发，之后根据是否需要降 AI 分为两个处理分支，需要降 AI 时，对 md 文件使用 lower\-aigc 进行降 AI 处理得到降 A md，不需要降 AI 则直接使用原始 md，两类处理后的文件都会送入 sxuyear 进行处理，最终生成符合规范的论文文档。

> （注：文档部分内容可能由 AI 生成）
