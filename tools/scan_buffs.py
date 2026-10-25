import os, re, glob, collections

root = 'extracted/Base_ruleset/Global/Characters'
stats = collections.Counter()
examples = collections.defaultdict(list)
files = glob.glob(root + '/*/AbilityCards/*.yml')

AB = re.compile(r'^[ \t]+ActiveBonus:[ \t]*$', re.M)
END = re.compile(r'^[ \t]{1,16}\S', re.M)

for f in files:
    txt = open(f, encoding='utf-8').read()
    for m in AB.finditer(txt):
        start = m.end()
        nxt = END.search(txt, start)
        block = txt[start:start + (nxt.start() if nxt else 3000)]

        dur = re.search(r'^[ \t]+Duration:[ \t]*(\w+)', block, re.M)
        beh = re.findall(r'^[ \t]+Behaviou?r?s?:[ \t]*(\w+)', block, re.M)
        hidden = re.search(r'^[ \t]+Hidden:[ \t]*True', block, re.M) is not None
        togg = re.search(r'^[ \t]+IsToggleBonus:[ \t]*True', block, re.M) is not None
        val = re.findall(r'^[ \t]+ValidAbilityType:[ \t]*(\w+)', block, re.M)
        dur = dur.group(1) if dur else 'NA'
        for b in (beh or ['<none>']):
            key = (dur, b, hidden, togg, tuple(sorted(set(val))) or '-')
            stats[key] += 1
            if len(examples[key]) < 2:
                examples[key].append(os.path.basename(f))

print('total ActiveBonus blocks:', sum(stats.values()))
print()
print('(Duration, Behaviour, Hidden, IsToggle, ValidAbilityTypes) -> count   example')
for k, v in sorted(stats.items(), key=lambda x: -x[1])[:34]:
    print('  %-62s %5d   %s' % (str(k), v, examples[k][:1]))
