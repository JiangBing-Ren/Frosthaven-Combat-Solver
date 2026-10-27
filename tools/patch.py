#!/usr/bin/env python3
"""
安全地做文本替换 —— **原子写 + 失败不落盘**。

两种用法：

1) 文件模式（**推荐**，最安全；完全避开 heredoc 转义与分隔符问题）：
       python3 tools/patch.py <目标文件> --old <旧文本文件> --new <新文本文件>

2) 标准输入模式（旧文本 + "|||" + 新文本）：
       python3 tools/patch.py <目标文件>
   ⚠ 旧文本与新文本里**不能**出现 "|||"。

设计要点（都是踩过坑换来的）：
  · **原子写**：先写临时文件再 os.replace —— 中途失败不会留下截断的文件。
  · **先断言后写**：old 在目标里必须**恰好出现 1 次**，否则**一个字节都不写**。
  · **绝不展开字面量 \\n / \\t**：C# 源码里大量存在字面转义序列，
    展开会把字符串变成真换行/真 tab（v1.30 连踩三次）。
    old/new 文件里直接写**真实换行**，字面的 \\n 原样保留。
"""

import io
import os
import sys
import tempfile


def atomic_write(path, text):
    d = os.path.dirname(os.path.abspath(path)) or '.'
    fd, tmp = tempfile.mkstemp(dir=d, prefix='.patch_', suffix='.tmp')
    try:
        with os.fdopen(fd, 'w', encoding='utf-8') as f:
            f.write(text)
        os.replace(tmp, path)
    except Exception:
        try:
            os.unlink(tmp)
        except OSError:
            pass
        raise


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        return 2
    target = sys.argv[1]

    old = new = None
    if '--old' in sys.argv and '--new' in sys.argv:
        oi = sys.argv.index('--old') + 1
        ni = sys.argv.index('--new') + 1
        old = io.open(sys.argv[oi], encoding='utf-8-sig').read()
        new = io.open(sys.argv[ni], encoding='utf-8-sig').read()
    else:
        spec = sys.stdin.read()
        if '|||' not in spec:
            print('  [XX] spec 里没有 ||| 分隔符（或请用 --old/--new 文件模式）')
            return 1
        old, new = spec.split('|||', 1)

    # ⚠⚠ **不做任何 expand**。
    #   C# 源码里大量存在字面的 "\n" / "\t"（转义序列），
    #   expand 会把它们变成**真换行/真 tab** → 源码字符串断行 → 编译失败。
    #   （v1.30 连续踩了三次：语法错误、count=0、混合换行。）
    #   ⇒ **只用文件模式**：old/new 文件里写**真实换行**，
    #     字面的 \n 就原样保留在文件里。

    if old == new:
        print('  [--] old == new，跳过')
        return 0

    # ★ 换行风格：**统一成 LF 比较与写回**。
    #   （经典坑：目标是 CRLF、调用方给 LF → 明明看着一样却 count=0；
    #     之前"写回时还原 CRLF"更糟 —— expand 产生的 LF 混进 CRLF 文件，
    #     留下混合换行，之后每处都难匹配。）
    raw = io.open(target, encoding='utf-8-sig', newline='').read()
    s = raw.replace('\r\n', '\n')
    old = old.replace('\r\n', '\n')
    new = new.replace('\r\n', '\n')

    n = s.count(old)
    if n != 1:
        print('  [XX] count=%d（要求恰好 1 处，目标文件未被修改）' % n)
        print('       old 前 80 字：%r' % old[:80])
        return 1

    s = s.replace(old, new)
    atomic_write(target, s)
    print('  [OK] %s' % old[:60].replace('\n', '\\n'))
    return 0


if __name__ == '__main__':
    sys.exit(main())
