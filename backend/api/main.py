"""FastAPI 应用入口。

启动方式：
    cd backend
    uvicorn api.main:app --reload --host 0.0.0.0 --port 8000
"""

from __future__ import annotations

from pathlib import Path

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.staticfiles import StaticFiles

from .routes import router


# ═══════════════════════════════════════════
# 应用初始化
# ═══════════════════════════════════════════

app = FastAPI(
    title="ThesisBuilder API",
    description="山西财经大学学年论文 Markdown→DOCX 自动排版工具",
    version="0.1.0",
)

# CORS 中间件（允许前端开发服务器访问）
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# 注册路由
app.include_router(router, prefix="/api")


# ═══════════════════════════════════════════
# 静态文件服务
# ═══════════════════════════════════════════

# 项目根目录（backend 的上一级）
PROJECT_ROOT = Path(__file__).resolve().parent.parent.parent

# 输出目录（用于预览图片）
OUTPUT_DIR = PROJECT_ROOT / "output"
OUTPUT_DIR.mkdir(exist_ok=True)

# 挂载静态文件
app.mount("/output", StaticFiles(directory=str(OUTPUT_DIR)), name="output")


# ═══════════════════════════════════════════
# 健康检查
# ═══════════════════════════════════════════

@app.get("/")
async def root():
    """健康检查"""
    return {
        "service": "ThesisBuilder API",
        "version": "0.1.0",
        "status": "running",
    }


@app.get("/health")
async def health():
    """健康检查端点"""
    return {"status": "ok"}
