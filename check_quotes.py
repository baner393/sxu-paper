#!/usr/bin/env python
# -*- coding: utf-8 -*-
import re

with open('xiaolei.md', 'r', encoding='utf-8') as f:
    original = f.read()

with open('lower aigc/output/xiaolei_processed.md', 'r', encoding='utf-8') as f:
    processed = f.read()

# 统计原始文件中的双引号
orig_left = original.count('“')  # "
orig_right = original.count('”')  # "
print(f'原始文件 - 左双引号: {orig_left}, 右双引号: {orig_right}')

# 统计处理后文件中的双引号
proc_left = processed.count('“')  # "
proc_right = processed.count('”')  # "
print(f'处理后文件 - 左双引号: {proc_left}, 右双引号: {proc_right}')

# 找出处理后文件中剩余的双引号内容
pattern = r'“(.*?)”'
matches = re.findall(pattern, processed)
print(f'\n处理后文件中剩余的双引号内容数量: {len(matches)}')
print('\n前20个剩余双引号内容:')
for i, m in enumerate(matches[:20]):
    if len(m) > 2:
        print(f'  {i+1}. "{m}"')
