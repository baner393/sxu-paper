processor Skill
  
.claude/skills/processor/
  ├── SKILL.md                    (2.7 KB)  —
  流程定义、双模式、AI约束
  ├── scripts/
  │   ├── extract.py              (14.2 KB) — DOCX→锚点Markdown    
  (zipfile+lxml)
  │   └── backfill.py             (6.8 KB)  — Markdown→DOCX回填    
  (python-docx)
  └── references/
      └── anchor-spec.md          (2.3 KB)  —
  锚点格式规范、block类型表

  两种模式：

  ┌──────────────────┬─────────────────────────────────────────┐   
  │       命令       │                  功能                   │   
  ├──────────────────┼─────────────────────────────────────────┤   
  │ /processor       │ DOCX → paper.md + meta.json +           │   
  │ extract          │ assets/images/                          │   
  ├──────────────────┼─────────────────────────────────────────┤   
  │ /processor       │ paper.md →                              │   
  │ backfill         │ xxx_filled.docx（不覆盖原文件）         │   
  └──────────────────┴─────────────────────────────────────────┘   

  核心防护：回填生成新文件、Block ID 永久不可变、AI 禁止操作 DOCX  
  结构。
