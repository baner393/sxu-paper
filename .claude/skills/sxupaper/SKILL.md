---
name: sxupaper
description: 学年论文全流程处理编排器。当用户输入 /sxupaper、提到"论文全流程"、"一键处理论文"、"论文处理流程"时使用。自动协调 processor、lower-aigc、sxuyear 三个模块完成论文处理。
---

# sxupaper Skill

学年论文全流程处理编排器，自动协调 processor、lower-aigc、sxuyear 三个模块完成论文处理。

## 触发条件

- 用户输入 `/sxupaper`
- 用户提到"论文全流程"、"一键处理论文"、"论文处理流程"

## 处理场景

根据用户提供的论文类型，自动选择对应流程：

### 场景 A：修后论文（降 AI）流程

适用于已经修改过的论文，需要降 AI 处理后回填原格式。

```
输入 DOCX → processor提取 → [json + assets/image + md] → lower-aigc降AI → 降AI版md → processor回填 → 输出DOCX
```

**流程步骤**：
1. 使用带锚点的 `processor` 提取 DOCX 文件
2. 获得三个中间产物：json、assets/image、md
3. 对 md 文件使用 `lower-aigc` 进行降 AI 处理
4. 将降 AI 后的 md 与 json、assets/image 进行 `processor` 回填
5. 输出处理完成的 DOCX 文件

### 场景 B：无修论文流程

适用于原始论文，可选择是否降 AI，最终生成符合规范的论文文档。

```
输入DOCX → processor提取 → md → [可选] lower-aigc降AI → sxuyear生成 → 输出DOCX
```

**流程步骤**：
1. 使用不带锚点的 `processor` 提取 DOCX 文件
2. 获得 md 文件
3. 询问用户是否需要降 AI 处理
   - 是：使用 `lower-aigc` 处理得到降 AI 版 md
   - 否：直接使用原始 md
4. 使用 `sxuyear` 生成符合山西财经大学规范的论文文档
5. 输出处理完成的 DOCX 文件

## 输入

- DOCX 论文文件路径
- 处理场景选择（A 或 B）
- 是否需要降 AI（场景 B 可选）

## 输出

- 处理完成的 DOCX 文件

## 工作流程

### 场景 A：修后论文（降 AI）流程

1. **读取输入文件**：获取 DOCX 文件路径
2. **调用 processor 提取**：使用带锚点模式提取
   - 生成 json 锚点文件
   - 提取 assets/image 图片资源
   - 提取 md 文本内容
3. **评估原始 AI 率**：扫描 md 文件，显示 AI 率报告
4. **调用 lower-aigc 降 AI**：处理 md 文件
   - 应用降 AI 味规则
   - 生成降 AI 版 md
5. **评估新 AI 率**：扫描处理后的 md，显示 AI 率报告
6. **显示对比报告**：展示降 AI 效果
7. **调用 processor 回填**：将降 AI 后的 md 回填到原 DOCX
   - 使用 json 锚点文件定位
   - 保留 assets/image 图片资源
   - 保持原格式不变
8. **输出结果**：生成处理完成的 DOCX 文件

### 场景 B：无修论文流程

1. **读取输入文件**：获取 DOCX 文件路径
2. **询问是否降 AI**：让用户选择是否需要降 AI 处理
3. **调用 processor 提取**：使用不带锚点模式提取
   - 提取 md 文本内容
4. **如果需要降 AI**：
   - 评估原始 AI 率
   - 调用 lower-aigc 处理
   - 评估新 AI 率
   - 显示对比报告
5. **调用 sxuyear 生成**：将 md 转换为符合规范的 DOCX
   - 应用山西财经大学论文格式
   - 生成规范的论文文档
6. **输出结果**：生成处理完成的 DOCX 文件

## 依赖模块

- `processor`：DOCX 学术论文 ⇄ 可逆 Markdown 双向转换
- `lower-aigc`：AI 生成内容特征弱化与人类写作风格增强
- `sxuyear`：将学年论文 Markdown 转换为符合山西财经大学规范的 Word 文档

## 使用示例

### 示例 1：修后论文降 AI

```
用户：/sxupaper
Claude：请选择处理场景：
1. 修后论文（降 AI）流程
2. 无修论文流程

用户：1
Claude：请提供 DOCX 文件路径

用户：D:\论文\修改后的论文.docx
Claude：[自动执行场景 A 流程，输出降 AI 后的论文]
```

### 示例 2：无修论文直接生成

```
用户：/sxupaper
Claude：请选择处理场景：
1. 修后论文（降 AI）流程
2. 无修论文流程

用户：2
Claude：请提供 DOCX 文件路径

用户：D:\论文\原始论文.docx
Claude：是否需要降 AI 处理？

用户：不需要
Claude：[自动执行场景 B 流程（不降 AI），输出符合规范的论文]
```

## 注意事项

1. 确保输入的 DOCX 文件格式正确
2. 处理过程中会生成中间文件，处理完成后可选择清理
3. 降 AI 处理会改变文本表达，但保持原文观点不变
4. 最终输出的 DOCX 会符合山西财经大学论文规范

## 文件结构

```
sxupaper/
├── SKILL.md                    # Skill 定义文件（本文件）
├── references/
│   └── workflow.md             # 工作流详细说明
└── scripts/                    # 辅助脚本（如需要）
```

## 参考资源

- 工作流详细说明：`references/workflow.md`
- processor 模块：参见 processor Skill
- lower-aigc 模块：参见 lower-aigc Skill
- sxuyear 模块：参见 sxuyear Skill
