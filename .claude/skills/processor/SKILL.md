---
name: processor
description: DOCX 学术论文 ⇄ 可逆 Markdown 双向转换。提取 DOCX 为带锚点 Markdown 供 AI 编辑，编辑后回填原 DOCX 保留全部格式。当用户提到 /processor、DOCX转Markdown、论文提取、回填论文、paper extract、backfill 时使用。
---

# DOCX ⇄ Markdown 论文处理器

## 核心原则

```
DOCX → 程序解析 → 带锚点Markdown → AI编辑Markdown → 程序回填DOCX
```

- AI 永不直接操作 DOCX 结构
- 程序负责结构提取和回填
- AI 只编辑 Markdown 内容
- Block ID 是回填的唯一桥梁

## 两种模式

### 模式 1: 提取 (`/processor extract`)

```
DOCX → paper.md + meta.json + assets/images/
```

1. 扫描 `paper put here\just put here\` 下的 `.docx` 文件
2. 对每个 DOCX 执行 `python scripts/extract.py <docx_path>`
3. 输出到 `output/论文标题/` 目录
4. 输出结构：
   ```
   论文标题/
   ├── paper.md        # 带锚点 Markdown
   ├── meta.json       # 元数据
   └── assets/
       └── images/     # 图片
   ```

### 模式 2: 回填 (`/processor backfill`)

```
paper.md + 原DOCX → xxx_filled.docx
```

1. 读取 `paper.md` 和 `meta.json`
2. 通过 Block ID 定位原 DOCX 段落
3. 仅替换正文/表格内容，保留全部格式
4. 生成 `xxx_filled.docx`（**不覆盖原文件**）

## 执行流程

### 提取流程（详细）

1. 确认 `paper put here\just put here\` 下有 .docx 文件
2. 中间文件（preprocessor 输出）放在 `paper put here\preprocessor\`
3. 运行: `python .claude/skills/processor/scripts/extract.py "<docx路径>" "paper put here/preprocessor"`
4. 提取完成后，将输出复制到 `input/{下一编号}/`：
   - `paper.md` → `学年论文.md`
   - `assets/images/` → `photo/`
5. 将 paper.md 内容展示给 AI 编辑

## 提取器架构（关键设计决策）

### 锚点驱动提取

提取器使用 **两阶段锚点法**：
1. **Phase 1**: 扫描全部 body 元素，建立索引
2. **Phase 2**: 查找关键锚点段落（摘  要、Abstract、目  录、导论、参考文献、附  录、致  谢）
3. **Phase 3**: 按锚点划分区域，逐区提取

**为什么不用状态机**：原状态机方案在摘要区、参考文献区反复失败。锚点法每次定位准确，不受段落顺序影响。

### 区域划分规则

| 区域 | 起始锚点 | 结束锚点 | 提取内容 |
|------|----------|----------|----------|
| 封面 | 文档开始 | `abstract_zh_start` | 跳过 |
| 中文摘要 | `abstract_zh_start` | `abstract_en_start` | 标题 → `## h`，正文 → `abs`，关键词 → `kw` |
| 英文摘要 | `abstract_en_start` | `toc_start` | 同上 |
| 目录 | `toc_start` | `body_start` | 跳过 |
| 正文 | `body_start` | `ref_start` | 标题 `h`，段落 `p`，表格 `tbl`，图片 `img` |
| 参考文献 | `ref_start` | `appendix_start` | 标题 `h`，条目 `ref` |
| 附录/致谢 | `appendix_start` | 文档结束 | 标题 `h`，段落 `p` |

## 已知问题与修复记录

### 修复 #1: 摘要正文和参考文献丢失（2026-05-21）

**现象**: 中文摘要只抓到第一段，参考文献4条全部缺失。

**根因**: 
- 状态机用 `abstract_zh = True` 标志阻止后续段落提取
- 参考文献条目被 `not in_body` 区域的摘要判断逻辑拦截（`text.startswith('[')` 行永远执行不到）

**修复**: 重写为锚点驱动架构（见上方"提取器架构"）

### 修复 #2: 摘要标题丢失（2026-05-21）

**现象**: `## 摘  要` 和 `## Abstract` 没出现在 Markdown 中。

**根因**: 提取器对标题行执行了 `continue`（跳过）。

**修复**: 改为输出 `## {标题文本}` 并分配 `block:h*` 锚点。

### 修复 #3: 封面表格标题提取失败（已知，未修复）

**现象**: 输出文件夹名始终为 `：`（冒号）。

**根因**: `_find_cover_table` 中 `value = cells[1]` 取到了冒号列而非 cells[2] 的值列。

**影响**: 仅影响文件夹命名，不影响内容提取。后续需修复。

### 回填流程

1. 确认用户已编辑完 paper.md
2. 运行: `python .claude/skills/processor/scripts/backfill.py "<markdown目录>"`
3. 生成 `xxx_filled.docx`
4. 提示用户检查输出

## AI 编辑约束（必须在 SKILL.md 中声明）

编辑 paper.md 时：

### 允许
- 修改 `block:p*` 正文段落内容
- 修改 `block:tbl*` 表格单元格
- 修改 `block:eq*` 公式内容

### 禁止
- 删除任何 `<!-- block:... -->` 锚点注释
- 修改 Block ID 编号
- 修改 `block:img*` 图片路径
- 修改标题层级结构（`#` 数量）
- 修改 `assets/images/` 目录下的文件

## 锚点规范

详见 `references/anchor-spec.md`

| 类型 | 前缀 | 示例 |
|------|------|------|
| 段落 | `p` | `<!-- block:p182 -->` |
| 标题 | `h` | `<!-- block:h5 -->` |
| 图片 | `img` | `<!-- block:img_12 -->` |
| 表格 | `tbl` | `<!-- block:tbl_3 -->` |
| 公式 | `eq` | `<!-- block:eq_8 -->` |
| 参考文献 | `ref` | `<!-- block:ref_0 -->` |

## 环境要求

- Python 3.8+
- 依赖: `lxml`, `python-docx`
- 安装: `pip install lxml python-docx`
