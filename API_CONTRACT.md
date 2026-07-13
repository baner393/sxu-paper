# API Contract — ThesisBuilder

## Backend API (FastAPI, port 8000)

### POST /api/convert
Convert Markdown to DOCX.

**Request:**
```json
{
  "input_md": "input/1/学年论文.md",
  "template_path": "template/附件4.docx",
  "config": {
    "page": {
      "width_cm": 21.0,
      "height_cm": 29.7,
      "margin_top_cm": 3.0,
      "margin_bottom_cm": 2.5,
      "margin_left_cm": 2.5,
      "margin_right_cm": 2.0
    },
    "fonts": {
      "body_east_asian": "SimSun",
      "body_latin": "Times New Roman",
      "body_size_pt": 12,
      "heading1_font": "SimHei",
      "heading1_size_pt": 16,
      "heading2_font": "SimSun",
      "heading2_size_pt": 14,
      "heading3_font": "SimSun",
      "heading3_size_pt": 12,
      "heading3_bold": true,
      "abstract_title_font": "SimSun",
      "abstract_title_size_pt": 18,
      "keywords_label_font": "SimHei",
      "keywords_label_size_pt": 14,
      "ref_font": "SimSun",
      "ref_size_pt": 12,
      "caption_font": "FangSong",
      "caption_size_pt": 10.5,
      "header_font_east_asian": "SimSun",
      "header_font_latin": "Times New Roman",
      "header_size_pt": 9,
      "footer_size_pt": 9
    },
    "spacing": {
      "line_spacing": 1.25,
      "heading1_before_pt": 16,
      "heading1_after_pt": 16,
      "heading2_before_pt": 0,
      "heading2_after_pt": 0,
      "heading3_before_pt": 0,
      "heading3_after_pt": 0,
      "abstract_before_pt": 18,
      "abstract_after_pt": 18,
      "first_line_indent_chars": 2
    },
    "header": {
      "text": "山西财经大学{grade}级本科生学年论文",
      "odd_align": "right",
      "even_align": "left",
      "show": true
    },
    "footer": {
      "show_page_number": true,
      "abstract_format": "roman_upper",
      "body_format": "decimal",
      "abstract_align": "center",
      "odd_align": "right",
      "even_align": "left"
    },
    "sections": {
      "cover": true,
      "abstract_cn": true,
      "abstract_en": true,
      "toc": true,
      "body": true,
      "references": true,
      "appendix": true,
      "acknowledgment": true,
      "back_cover": true
    }
  }
}
```

**Response:**
```json
{
  "success": true,
  "output_path": "output/论文标题.docx",
  "preview_url": "/api/preview/论文标题",
  "pages": 25,
  "warnings": []
}
```

### POST /api/preview
Generate page images for flipbook preview.
**Request:** Same as /api/convert
**Response:**
```json
{
  "success": true,
  "total_pages": 25,
  "pages": [
    {"page": 1, "image_url": "/api/pages/论文标题/1.png", "width": 595, "height": 842},
    ...
  ]
}
```

### GET /api/pages/{title}/{page_num}
Serve a single page image (PNG).

### GET /api/templates
List available .docx templates in template/ directory.

### GET /api/inputs
List available input papers in input/ directory.

### GET /api/config/default
Return default config values.

### POST /api/config/save
Save a named config preset.

## Data Format: page images
- Format: PNG
- Resolution: 150 DPI (A4 = 1240x1754 px)
- Generated via LibreOffice headless: `libreoffice --headless --convert-to png`

## File Structure
```
new_sxuyear/
├── backend/
│   ├── thesis_builder/     # Core conversion engine
│   │   ├── parser.py       # Markdown parsing
│   │   ├── converter.py    # Pandoc + python-docx conversion
│   │   ├── styles.py       # Style constants
│   │   ├── template_merge.py
│   │   └── postprocess.py
│   ├── api/
│   │   ├── main.py         # FastAPI app
│   │   └── routes.py       # API routes
│   └── pyproject.toml
├── frontend/
│   ├── src/
│   │   ├── App.jsx
│   │   ├── components/
│   │   │   ├── ConfigPanel.jsx
│   │   │   ├── PreviewFlipbook.jsx
│   │   │   └── PageThumbnail.jsx
│   │   └── styles/
│   └── package.json
├── input/                  # Input papers (same format as before)
├── template/               # DOCX templates
├── output/                 # Generated DOCX files
└── API_CONTRACT.md
```
