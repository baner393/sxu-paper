#!/usr/bin/env python3
"""启动 ThesisBuilder API 服务器。"""
import sys
from pathlib import Path

# 将 backend/ 加入 sys.path，使相对导入正常工作
backend_dir = Path(__file__).resolve().parent / "backend"
sys.path.insert(0, str(backend_dir.parent))

# 使用 backend 作为包导入
from backend.api.main import app

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
