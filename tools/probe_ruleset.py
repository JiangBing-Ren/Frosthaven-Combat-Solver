import struct, sys

p = r"D:\Steam\steamapps\common\Frosthaven\Frosthaven_Data\StreamingAssets\Rulebase\Base.ruleset"
ENTRY = 144

f = open(p, 'rb')
import os
total = os.path.getsize(p)
head = f.read(16)
print("filesize:", total)
print("magic raw:", head[:8])
print("u32@8 :", struct.unpack('<I', head[8:12])[0])
count = struct.unpack('<I', head[12:16])[0]
print("u32@12 (count candidate):", count)

f.seek(16)
ents = []
N = min(count, 25)
for i in range(N):
    e = f.read(ENTRY)
    if len(e) < ENTRY:
        print("!! truncated at", i)
        break
    path = e[:128].split(b'\x00')[0].decode('utf-8', 'replace')
    f1, f2, f3, f4 = struct.unpack('<4I', e[128:144])
    ents.append((path, f1, f2, f3, f4))

print("\n--- first entries (path, f1,f2,f3,f4) ---")
for i, (path, f1, f2, f3, f4) in enumerate(ents):
    print(f"{i:3d} {path:55s} {f1:6d} {f2:6d} {f3:8d} {f4:8d}")

# analyze increments of f3 vs f4
print("\n--- consistency check: f3[i]+f4[i] vs f3[i+1] ---")
ok = 0
for i in range(len(ents) - 1):
    exp = ents[i][3] + ents[i][4]
    act = ents[i + 1][3]
    if exp == act:
        ok += 1
    elif i < 8:
        print(f"  i={i}: f3+f4={exp}  next f3={act}  delta={act-exp}")

print(f"\nexact matches f3+f4==next f3 : {ok}/{len(ents)-1}")

data_start = 16 + count * ENTRY
print("implied data_start (16 + count*144):", data_start)
print("remaining bytes for data:", total - data_start)

# peek at implied data area
if data_start < total:
    f.seek(data_start)
    print("\n--- bytes at implied data_start ---")
    print(repr(f.read(200)))

# also scan: where does the TOC end (last plausible printable path)?
f.seek(16)
last_good = 0
for i in range(count):
    e = f.read(ENTRY)
    if len(e) < ENTRY:
        break
    chunk = e[:128]
    printable = all(32 <= b < 127 or b == 0 for b in chunk.rstrip(b'\x00'))
    if printable and chunk.split(b'\x00')[0]:
        last_good = i
    else:
        print(f"\nfirst non-printable-ish entry at index {i}")
        break
print("last plausible TOC index:", last_good, "-> TOC bytes:", 16 + (last_good + 1) * ENTRY)
f.close()
