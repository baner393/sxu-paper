#!/usr/bin/env python
# -*- coding: utf-8 -*-
import re

with open('xiaolei.md', 'r', encoding='utf-8') as f:
    original = f.read()

# 检查双引号字符的 Unicode 编码
print("检查原始文件中的双引号字符:")
for i, char in enumerate(original):
    if char in ['"', '"', '"', "'", "'", '「', '」']:
        print(f"  位置 {i}: 字符='{char}', Unicode={hex(ord(char))}")

# 测试正则表达式
test_text = '这是"测试内容"和"更多内容"'
print(f"\n测试文本: {test_text}")

# 测试不同的正则表达式
patterns = [
    (r'[""](.*?)[""]', "中文双引号"),
    (r'"(.*?)"', "英文双引号"),
    (r'["“](.*?)["”]', "Unicode双引号"),
    (r'[“](.*?)[”]', "精确Unicode双引号"),
]

for pattern, name in patterns:
    matches = re.findall(pattern, test_text)
    print(f"  {name}: {matches}")
