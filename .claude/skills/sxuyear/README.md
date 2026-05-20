# sxuyear

将学年论文 Markdown 文件转换为符合山西财经大学论文规范的 Word 文档（.docx）。

## 触发方式

输入 `/sxuyear`、提到"学年论文"、"论文格式转换"、"生成Word论文"时自动使用。

## 依赖

- minimax-docx skill

## 文件结构

```
sxuyear/
├── SKILL.md          # Skill 定义与执行流程
├── input/            # 输入 Markdown 论文文件及图片
│   ├── 学年论文.md    # 论文正文（Markdown 格式）
│   ├── 1/            # 第1篇文章的图片目录
│   │   └── photo/    # 按文章顺序存放的图文件（PNG/JPG）
│   ├── 2/            # 第2篇文章的图片目录
│   │   └── photo/
│   └── ...
├── output/           # 输出 .docx 文件
└── template/         # 格式规范模板
```

> **注意**：论文中的图片需按文章顺序放入 `sxuyear\input\{数字编号}\photo` 目录中。编号从 1 开始递增，与 Markdown 中图片引用的顺序一致。
