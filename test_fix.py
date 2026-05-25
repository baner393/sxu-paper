#!/usr/bin/env python
# -*- coding: utf-8 -*-
import sys
sys.path.insert(0, '.claude/skills/lower-aigc/scripts')
from lower_aigc_custom import remove_quotes_and_concretize

# 测试文本（使用中文双引号）
test = '三案例分别呈现"叠合式叙事共生""张力式戏剧共生"与"隔代式转译共生"三种模式'
print(f"原文: {test}")

result = remove_quotes_and_concretize(test)
print(f"结果: {result}")
print()

# 测试更多例子
tests = [
    '红色文化与汉文化的"共生"并非简单的资源叠加',
    '通过"复盘"来"优化"方案',
    '所谓"记忆叠层"是指',
]

for t in tests:
    result = remove_quotes_and_concretize(t)
    print(f"原文: {t}")
    print(f"结果: {result}")
    print()
