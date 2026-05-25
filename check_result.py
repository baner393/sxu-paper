#!/usr/bin/env python
# -*- coding: utf-8 -*-
import re

with open('lower aigc/output/xiaolei_processed.md', 'r', encoding='utf-8') as f:
    processed = f.read()

# 统计双引号数量
left_count = processed.count('"')  # "
right_count = processed.count('"')  # "
print(f'左双引号数量: {left_count}')
print(f'右双引号数量: {right_count}')

# 找出所有双引号内容
pattern = r'“(.*?)”'
matches = re.findall(pattern, processed)
print(f'双引号内容数量: {len(matches)}')

# 显示前10个
print('\n剩余双引号内容:')
for i, m in enumerate(matches[:10]):
    if len(m) > 2:
        print(f'  {i+1}. "{m}"')
