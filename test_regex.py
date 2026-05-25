#!/usr/bin/env python
# -*- coding: utf-8 -*-
import re

# 测试文本
test1 = '这是"测试内容"和"更多内容"'
test2 = '这是“测试内容”和“更多内容”'

print("测试1:", test1)
print("测试2:", test2)

# 测试不同的正则表达式
patterns = [
    (r'["“](.*?)["”]', "Unicode转义"),
    (r'[“](.*?)[”]', "精确Unicode"),
    (r'“(.*?)”', "完整Unicode"),
    (r'[""](.*?)[""]', "字面量"),
]

for pattern, name in patterns:
    try:
        matches1 = re.findall(pattern, test1)
        matches2 = re.findall(pattern, test2)
        print(f"{name}: test1={matches1}, test2={matches2}")
    except Exception as e:
        print(f"{name}: 错误 - {e}")

# 直接测试字符
print("\n字符编码检查:")
print(f"test1中的字符: {[hex(ord(c)) for c in test1 if ord(c) > 127]}")
print(f"test2中的字符: {[hex(ord(c)) for c in test2 if ord(c) > 127]}")
