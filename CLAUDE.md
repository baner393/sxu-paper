# ThesisBuilder — 山西财经大学学年论文 Markdown→Word 自动排版工具

## 项目概述
将 Markdown 格式的学年论文自动转换为符合山西财经大学格式规范的 Word (.docx) 文档。
支持通过 Web UI 配置所有格式参数，并提供翻页预览功能。

## 技术栈
- **Backend**: Python 3.14, FastAPI, python-docx, Pandoc (已安装在 ~/.local/bin/pandoc)
- **Frontend**: React + Vite, react-pageflip (翻页预览)
- **运行环境**: WSL2, venv 在 ~/.venvs/thesis-builder

## 关键路径
- 项目根: `/mnt/d/360MoveData/Users/ban/Desktop/school_about/word_about/new_sxuyear`
- Pandoc: `~/.local/bin/pandoc` (v3.6.4)
- Python venv: `~/.venvs/thesis-builder/bin/python`
- 标准参考 DOCX: `template/山财学年论文模板.docx`

## 格式规范详见
`/mnt/d/360MoveData/Users/ban/Desktop/school_about/word_about/sxuyear/template/山西财经大学学年论文格式.md`

## API 契约
详见 `API_CONTRACT.md`

## 代码规范
- Python: type hints, dataclasses, Google-style docstrings
- 中文注释
- 模块化: 每个文件 < 300 行
