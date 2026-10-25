#!/usr/bin/env python3
"""
增量打补丁 —— 每个替换**立即写盘**，避免"部分成功也整体丢失"。

两次实战教训：
  1. v1.20：一个脚本里 4 个 rep，第 4 个失败 → 前 3 个已成功的也全丢
     （因为在内存里改完才写盘）。→ 改为成功即写盘。
  2. v1.21：本工具自身按行分割 spec，而 heredoc 传进来的是**真换行**，
     多行 new 只有第一行被写进去，其余静默丢弃。
     → 改为：整个 stdin 就是一个 old|||new 对，不再按行切。

用法（多行必须用字面量 \\n，或直接用文件方式）：
  printf '%s' 'OLD|||NEW' | python3 tools/patch.py <file>
"""
import io
import sys


def main():
    if len(sys.argv) < 2:
        print('usage: patch.py <file>   (stdin = OLD|||NEW)')
        return 2
    path = sys.argv[1]
    spec = sys.stdin.read()

    if '|||' not in spec:
        print('  [XX] spec 里没有 ||| 分隔符')
        return 1

    old, new = spec.split('|||', 1)
    # 支持字面量 \n / \t（调用方没传真换行时）
    old = old.replace('\\n', '\n').replace('\\t', '\t')
    new = new.replace('\\n', '\n').replace('\\t', '\t')

    if old == new:
        print('  [--] old == new，跳过')
        return 0

    s = io.open(path, encoding='utf-8-sig').read()
    n = s.count(old)
    if n != 1:
        print('  [XX] count=%d（要求恰好 1 处）  %s' % (n, old[:70].replace('\n', '\\n')))
        return 1

    s = s.replace(old, new)
    io.open(path, 'w', encoding='utf-8').write(s)
    print('  [OK] %s' % old[:60].replace('\n', '\\n'))
    return 0


if __name__ == '__main__':
    sys.exit(main())
