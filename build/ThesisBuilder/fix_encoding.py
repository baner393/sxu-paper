#!/usr/bin/env python3
"""Replace problematic Unicode chars in C# source with escape sequences."""
import os

CS = r"D:\360MoveData\Users\ban\Desktop\school_about\word_about\sxuyear\build\ThesisBuilder\Program.cs"

with open(CS, 'r', encoding='utf-8') as f:
    cs = f.read()

# Replace problematic Unicode chars with C# escape sequences
# These chars are fine INSIDE C# strings but can confuse some compilers
replacements = {
    '“': '\\u201c',  # LEFT DOUBLE QUOTATION MARK "
    '”': '\\u201d',  # RIGHT DOUBLE QUOTATION MARK "
    '—': '\\u2014',  # EM DASH —
    '‘': '\\u2018',  # LEFT SINGLE QUOTATION MARK '
    '’': '\\u2019',  # RIGHT SINGLE QUOTATION MARK '
}

for old, new in replacements.items():
    count = cs.count(old)
    if count > 0:
        cs = cs.replace(old, new)
        print(f"Replaced {count} occurrences of U+{ord(old):04X}")

with open(CS, 'w', encoding='utf-8', newline='\r\n') as f:
    f.write(cs)
print("Done - all problematic chars replaced with C# escape sequences")
