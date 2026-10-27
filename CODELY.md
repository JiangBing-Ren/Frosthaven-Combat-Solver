

## Codely Structured Memories

### User

### Feedback

### Project
- [2026-10-27 10:57:04] 项目 D:\1Mods\FH（Frosthaven 战斗求解器）当前 v1.36.0：BepInEx 5.4 插件（net472）+ 离线测试台（263 条断言）。架构 Bridge 只读快照→POCO，Solver 纯算法；词典序 Pareto + 束搜索 + 多样性分桶；修正牌为确定序列。规则事实：Strengthen = Advantage（不是 +1）；Poison = 攻击该目标 +1（Wound 才回合末掉血）；伤害顺序 = (基础+加减)×乘除 − max(0,护盾−穿透) → Brittle×2 → Ward floor(/2) → 0；CLootDeck.AllCards 是 LootCard[]。约定：改前 git status；改源码用 tools/patch.py；反射须登记 verify_reflection.py 并分清字段/属性（MiscAbilityData 三个开关是字段）；P1Runner.cs 为 ilspy 恢复产物。审计文档：build/rule-audit-combat.md、build/rule-audit-enemy.md。v1.35/1.36 已修：单体重击、落点重写、多回合先攻、焦点规则、优劣势选牌、反射错配、Strengthen/Poison、Target N、穿透/易碎/结界、战利品类型、死亡占位。仍未做：多格怪占位、焦点代价语义、Retaliate、免疫、牌堆洗牌、Curse/Bless 永久移出。本机无 bash/python3，用 dotnet build --no-restore + dotnet run 验证。


### Reference
- [2026-10-27 10:31:31] SOLVER_DESIGN.md（D:\1Mods\FH 根目录）是设计、踩坑与版本史的唯一真相源，每版末尾有变更记录（v1.35 章节含五个 P0 修复与「反射字段被当属性」的教训）；新会话应先读 README.md + SOLVER_DESIGN.md 末尾，再看源码。
