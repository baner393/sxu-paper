#!/usr/bin/env python3
"""Fix Unicode quote escaping in Program.cs abstract sections."""
import os, re

CS = r"D:\360MoveData\Users\ban\Desktop\school_about\word_about\sxuyear\build\ThesisBuilder\Program.cs"

with open(CS, 'r', encoding='utf-8') as f:
    cs = f.read()

# Fix the Chinese abstract and all C# string literals that contain
# unescaped Chinese quotes (U+201C “ and U+201D ”)
# and right single quotes used as apostrophes (U+2019 ’)

# Strategy: find all string content between C# string delimiters and fix unescaped chars
# Actually, just do a global replacement in the file - ONLY within C# string contexts

# Replace Chinese double quotes with \u escapes
cs = cs.replace('“', '\\u201c')
cs = cs.replace('”', '\\u201d')

# Replace right single quote (smart apostrophe) with ’
cs = cs.replace('’', '\\u2019')

# Replace left single quote
cs = cs.replace('‘', '\\u2018')

# Replace em dash
cs = cs.replace('—', '\\u2014')

# Replace en dash
cs = cs.replace('–', '\\u2013')

with open(CS, 'w', encoding='utf-8', newline='\r\n') as f:
    f.write(cs)
print("Unicode escapes fixed!")
