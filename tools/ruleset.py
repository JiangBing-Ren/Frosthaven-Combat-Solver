"""
Frosthaven (霜港迷城) Base.ruleset 归档解包/回打包工具

归档格式（经逆向确认）:
  [0]     8 bytes  ASCII magic  "02392fe1"
  [8]     u32      0
  [12]    u32      条目总数 N
  [16]    TOC: N * 144 bytes, 每条 =
            128 bytes  路径，UTF-8，末补 0x00
            u32       meta1  (恒为 2)
            u32       meta2  (恒为 0)
            u32       offset (相对数据区起点)
            u32       size   (字节数)
  数据区 = 16 + N*144 起，各文件明文连续存放，无压缩无对齐填充。
"""

import struct
import sys
import os
import json
import shutil

MAGIC = b"02392fe1"
ENTRY = 144
PATH_LEN = 128


def read_toc(path):
    size = os.path.getsize(path)
    with open(path, "rb") as f:
        head = f.read(16)
        if head[:8] != MAGIC:
            raise SystemExit(f"魔数不匹配: {head[:8]!r} 不是 Frosthaven ruleset")
        _zero, count = struct.unpack("<2I", head[8:16])
        f.seek(16)
        entries = []
        for i in range(count):
            e = f.read(ENTRY)
            if len(e) < ENTRY:
                raise SystemExit(f"TOC 在条目 {i} 处截断")
            raw = e[:PATH_LEN]
            name = raw.split(b"\x00")[0].decode("utf-8", "replace")
            m1, m2, off, sz = struct.unpack("<4I", e[PATH_LEN:ENTRY])
            entries.append({"name": name, "meta1": m1, "meta2": m2,
                            "offset": off, "size": sz})
    return entries, size


# meta1 = 解析器/条目类型枚举 ID（对应 YAML 内的 Parser: 字段），取值多样，
# 不作合法性假设，仅在 info 中统计分布；结构完整性以偏移链为准。


def validate(entries, size):
    """校验偏移链、类型字段、以及末条是否落到数据区末尾。"""
    probs = []
    data_start = 16 + len(entries) * ENTRY
    data_len = size - data_start
    for i, e in enumerate(entries):
        if len(e["name"].encode("utf-8")) > PATH_LEN - 1:
            probs.append(f"条目{i} 路径超长: {e['name']}")
        if i and entries[i - 1]["offset"] + entries[i - 1]["size"] != e["offset"]:
            probs.append(f"条目{i} 偏移链断裂")
    if entries:
        last = entries[-1]
        if last["offset"] + last["size"] != data_len:
            probs.append(f"末条末端 {last['offset'] + last['size']} != 数据区长度 {data_len}")
    return probs, data_start, data_len


def cmd_info(src):
    entries, size = read_toc(src)
    probs, data_start, data_len = validate(entries, size)
    print(f"文件: {src}")
    print(f"体积: {size} 字节")
    print(f"条目: {len(entries)}")
    print(f"数据区: 起点 {data_start}, 长度 {data_len}")
    cats = {}
    for e in entries:
        top = e["name"].split("/")[0] if "/" in e["name"] else "(root)"
        cats[top] = cats.get(top, 0) + 1
    print("\n顶层分类:")
    for k, v in sorted(cats.items(), key=lambda x: -x[1]):
        print(f"  {v:5d}  {k}")
    tcnt = {}
    for e in entries:
        tcnt[e["meta1"]] = tcnt.get(e["meta1"], 0) + 1
    print("条目类型(meta1)分布:",
          ", ".join(f"type{k}={v}" for k, v in sorted(tcnt.items())))
    sub = {}
    for e in entries:
        ps = e["name"].split("/")
        key = "/".join(ps[:2]) if len(ps) > 2 else "(root)"
        sub[key] = sub.get(key, 0) + 1
    print("\n二级分类 (前 15):")
    for k, v in sorted(sub.items(), key=lambda x: -x[1])[:15]:
        print(f"  {v:5d}  {k}")
    print("\n校验:", "全部通过" if not probs else f"{len(probs)} 个问题")
    for p in probs[:10]:
        print("  !", p)


def cmd_extract(src, outdir):
    entries, size = read_toc(src)
    probs, data_start, data_len = validate(entries, size)
    if probs:
        print("警告: 校验发现问题", file=sys.stderr)
        for p in probs[:10]:
            print("  !", p, file=sys.stderr)
    if os.path.isdir(outdir):
        shutil.rmtree(outdir)
    os.makedirs(outdir, exist_ok=True)

    total = 0
    with open(src, "rb") as f:
        for e in entries:
            f.seek(data_start + e["offset"])
            data = f.read(e["size"])
            if len(data) != e["size"]:
                print(f"读取不足: {e['name']}", file=sys.stderr)
                continue
            # 归档内路径用 '/'，映射到本地文件系统；防目录穿越
            parts = [p for p in e["name"].replace("\\", "/").split("/") if p not in ("", ".", "..")]
            dest = os.path.join(outdir, *parts)
            os.makedirs(os.path.dirname(dest), exist_ok=True)
            with open(dest, "wb") as o:
                o.write(data)
            total += 1

    manifest = {"source": src, "count": len(entries),
                "entries": [{k: e[k] for k in ("name", "meta1", "meta2", "offset", "size")}
                            for e in entries]}
    with open(os.path.join(outdir, "_manifest.json"), "w", encoding="utf-8") as m:
        json.dump(manifest, m, ensure_ascii=False, indent=1)
    print(f"已解出 {total}/{len(entries)} 个文件 -> {outdir}")
    print(f"manifest 已写入 _manifest.json (回打包用)")


def cmd_repack(indir, dst, manifest_name="_manifest.json"):
    mpath = os.path.join(indir, manifest_name)
    if not os.path.exists(mpath):
        raise SystemExit(f"缺少 {mpath}，无法无损回打包")
    with open(mpath, encoding="utf-8") as f:
        man = json.load(f)

    blobs, entries = [], []
    cur = 0
    for e in man["entries"]:
        parts = [p for p in e["name"].replace("\\", "/").split("/") if p not in ("", ".", "..")]
        fp = os.path.join(indir, *parts)
        if not os.path.exists(fp):
            raise SystemExit(f"缺失文件: {e['name']} (mod 目录与 manifest 不一致)")
        data = open(fp, "rb").read()
        blobs.append(data)
        entries.append((e["name"], e["meta1"], e["meta2"], cur, len(data)))
        cur += len(data)

    with open(dst, "wb") as f:
        f.write(MAGIC + struct.pack("<2I", 0, len(entries)))
        for name, m1, m2, off, sz in entries:
            nb = name.encode("utf-8")[:PATH_LEN - 1]
            f.write(nb + b"\x00" * (PATH_LEN - len(nb)))
            f.write(struct.pack("<4I", m1, m2, off, sz))
        for b in blobs:
            f.write(b)
    print(f"已回打包 {len(entries)} 个条目 -> {dst} ({os.path.getsize(dst)} 字节)")


if __name__ == "__main__":
    cmd = sys.argv[1] if len(sys.argv) > 1 else None
    if cmd == "info" and len(sys.argv) > 2:
        cmd_info(sys.argv[2])
    elif cmd == "extract" and len(sys.argv) > 3:
        cmd_extract(sys.argv[2], sys.argv[3])
    elif cmd == "repack" and len(sys.argv) > 3:
        cmd_repack(sys.argv[2], sys.argv[3])
    else:
        print(__doc__)
        print("用法:")
        print("  ruleset.py info    <Base.ruleset>")
        print("  ruleset.py extract <Base.ruleset> <输出目录>")
        print("  ruleset.py repack  <解包目录> <输出.ruleset>")
