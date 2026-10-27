using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using System.Text;
using FHEngine.Model;

namespace FHEngine.Tests
{
    /// <summary>
    /// 阶段 1 数据校验台。
    ///
    /// 职责（阶段 1）：
    ///   · 加载 Data/*.json（由 tools/extract_rules.py 从明文 yml 提取）
    ///   · 断言条数与范围
    ///   · 抽样 3 条与源文件**逐字段**对照
    ///   · 校验 raw / resolved 双字段的一致性（哨兵值必须被解析）
    ///
    /// 与 FHSolver.Tests 一致的极简断言风格：[OK]/[XX] 计数，末尾汇总。
    /// </summary>
    internal static class Program
    {
        private static int _ok;
        private static int _fail;

        private static string _dataDir;

        private static void True(string label, bool cond)
        {
            if (cond) { _ok++; Console.WriteLine("  [OK] " + label); }
            else { _fail++; Console.WriteLine("  [XX] " + label); }
        }

        private static void Eq(string label, int expected, int actual)
        {
            bool ok = expected == actual;
            if (ok) { _ok++; Console.WriteLine(string.Format("  [OK] {0,-46} 期望{1} 实得{2}", label, expected, actual)); }
            else { _fail++; Console.WriteLine(string.Format("  [XX] {0,-46} 期望{1} 实得{2}", label, expected, actual)); }
        }

        private static string ResolveDataDir()
        {
            // 从 tests/FHEngine.Tests 往上两级 = 仓库根
            DirectoryInfo d = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            for (int i = 0; i < 6 && d != null; i++)
            {
                string cand = Path.Combine(d.FullName, "Data");
                if (Directory.Exists(cand)) return cand;
                d = d.Parent;
            }
            return null;
        }

        private static List<T> LoadList<T>(string name)
        {
            string path = Path.Combine(_dataDir, name);
            if (!File.Exists(path)) throw new FileNotFoundException("缺少数据文件: " + name, path);
            string json = File.ReadAllText(path, new UTF8Encoding(false));
            var ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            ser.RecursionLimit = 100;
            return ser.Deserialize<List<T>>(json);
        }

        private static Dictionary<string, T> LoadDict<T>(string name)
        {
            string path = Path.Combine(_dataDir, name);
            if (!File.Exists(path)) throw new FileNotFoundException("缺少数据文件: " + name, path);
            string json = File.ReadAllText(path, new UTF8Encoding(false));
            var ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            ser.RecursionLimit = 100;
            return ser.Deserialize<Dictionary<string, T>>(json);
        }

        private static int Main()
        {
            Console.WriteLine("===== FHEngine 阶段 1 数据校验 =====");
            _dataDir = ResolveDataDir();
            if (_dataDir == null)
            {
                Console.WriteLine("  [XX] 找不到 Data/ 目录（请先运行 tools/extract_rules.py）");
                return 1;
            }
            Console.WriteLine("数据目录: " + _dataDir);

            try
            {
                // ---------- 1. 条数 ----------
                Console.WriteLine();
                Console.WriteLine("== 提取条数 ==");
                List<AbilityCard> cards = LoadList<AbilityCard>("cards.json");
                List<Monster> monsters = LoadList<Monster>("monsters.json");
                List<MonsterCard> mcards = LoadList<MonsterCard>("monster_cards.json");
                List<AttackModifier> mods = LoadList<AttackModifier>("attack_modifiers.json");
                List<ModifierDeck> decks = LoadList<ModifierDeck>("modifier_decks.json");

                Eq("cards（BannerSpear 29 + Drifter 31）", 60, cards.Count);
                Eq("monsters（AlgoxScout×2 + AbaelHerder×2）", 4, monsters.Count);
                Eq("monster_cards（770-777 + 778-785）", 16, mcards.Count);
                Eq("attack_modifiers（全量定义）", 140, mods.Count);
                Eq("modifier_decks", 4, decks.Count);

                // ---------- 2. 抽样 1：62_BannerSpear_RallyingCry ----------
                Console.WriteLine();
                Console.WriteLine("== 抽样1：Card#62（源文件 62_BannerSpear_RallyingCry.yml）==");
                AbilityCard c62 = FindCard(cards, 62);
                True("存在 Id=62", c62 != null);
                if (c62 != null)
                {
                    Eq("  Initiative（源文件第7行 = 71）", 71, c62.Initiative);
                    Eq("  Level", 1, c62.Level);
                    Eq("  Discard 项数", 2, c62.Discard.Count);
                    True("  Discard 上半 = Discard", c62.Discard.Count > 0 && c62.Discard[0] == "Discard");
                    True("  Discard 下半 = Lost", c62.Discard.Count > 1 && c62.Discard[1] == "Lost");
                    Eq("  Top.XP", 1, c62.Top != null ? c62.Top.XP : -1);
                    Eq("  Bottom.XP", 2, c62.Bottom != null ? c62.Bottom.XP : -1);
                    int atk = -1;
                    if (c62.Top != null && c62.Top.Abilities.ContainsKey("TAttack"))
                        atk = c62.Top.Abilities["TAttack"].AttackAmount ?? -1;
                    Eq("  Top.TAttack.AttackAmount（源文件 = 3）", 3, atk);
                    string cond = null;
                    if (c62.Top != null && c62.Top.Abilities.ContainsKey("TAttack"))
                        cond = c62.Top.Abilities["TAttack"].Conditions as string;
                    True("  Top.TAttack.Conditions = Disarm", cond == "Disarm");
                }

                // ---------- 3. 抽样 2：MonsterCard#779 的 raw / resolved ----------
                Console.WriteLine();
                Console.WriteLine("== 抽样2：MonsterCard#779（AbaelHerder_DesperateHerd）==");
                MonsterCard mc779 = FindMonsterCard(mcards, 779);
                True("存在 Id=779", mc779 != null);
                if (mc779 != null)
                {
                    Eq("  Initiative", 72, mc779.Initiative);
                    True("  Shuffle = False", !mc779.Shuffle);
                    True("  RawValues 非空（溯源用）", mc779.RawValues.Count > 0);
                    True("  ResolvedValues 非空（下游只用这个）", mc779.ResolvedValues.Count > 0);

                    // 哨兵值必须被解析：raw 为负 → resolved 必须为正
                    int negRaw = 0, negResolvedWrong = 0;
                    foreach (RawValueEntry r in mc779.RawValues)
                    {
                        if (r.raw.HasValue && r.raw.Value < 0) negRaw++;
                    }
                    foreach (var kv in mc779.ResolvedValues)
                    {
                        var lv0 = kv.Value.ContainsKey("0") ? kv.Value["0"] : null;
                        if (lv0 == null) continue;
                        foreach (var f in lv0)
                        {
                            if (f.Value < 0) negResolvedWrong++;
                        }
                    }
                    True("  存在哨兵 raw 值（负数，需解析）: " + negRaw, negRaw > 0);
                    Eq("  resolved 中仍为负数的字段数（应为 0）", 0, negResolvedWrong);

                    // resolved 具体值：普通 AbaelHerder L0 的 AllyAttack.Attack 应 = 怪物 Attack(2)
                    int resolvedAttack = -1;
                    if (mc779.ResolvedValues.ContainsKey("AbaelHerderID"))
                    {
                        var byLv = mc779.ResolvedValues["AbaelHerderID"];
                        if (byLv.ContainsKey("0"))
                        {
                            var fields = byLv["0"];
                            const string key = "Abilities.ControlPig1.ControlActor.Abilities.AllyAttack.Attack";
                            if (fields.ContainsKey(key)) resolvedAttack = fields[key];
                        }
                    }
                    Eq("  AbaelHerderID L0 的 AllyAttack.Attack（= 怪物 Attack 2）", 2, resolvedAttack);
                }

                // ---------- 3b. 能力树结构（阶段 3 的 AI 依赖，不能扁平化）----------
                Console.WriteLine();
                Console.WriteLine("== 能力树结构（阶段 3 AI 需要：有没有 Move/Attack 节点、嵌套了什么）==");
                MonsterCard mc770 = FindMonsterCard(mcards, 770);
                True("存在 Id=770（Scout_CruelBow）", mc770 != null);
                if (mc770 != null)
                {
                    var ab = mc770.Abilities;
                    True("  770 有 AMove 节点", ab != null && ab.ContainsKey("AMove"));
                    True("  770 有 AAttack 节点", ab != null && ab.ContainsKey("AAttack"));
                    int a770 = -999;
                    if (ab != null && ab.ContainsKey("AAttack"))
                        a770 = ab["AAttack"].AttackAmount ?? -999;
                    Eq("  770 AAttack 的卡面原值（源文件 = -1）", -1, a770);
                    bool hasMoveType = ab != null && ab.ContainsKey("AMove")
                                      && ab["AMove"].Types != null
                                      && ab["AMove"].Types.Contains("Move");
                    True("  770 AMove 的类型含 Move", hasMoveType);
                }
                if (mc779 != null)
                {
                    var ab = mc779.Abilities;
                    True("  779 有 Move1 节点", ab != null && ab.ContainsKey("Move1"));
                    True("  779 有 ControlPig1 节点", ab != null && ab.ContainsKey("ControlPig1"));
                    bool nested = false;
                    int allyAtk = -999;
                    if (ab != null && ab.ContainsKey("ControlPig1"))
                    {
                        var cp = ab["ControlPig1"];
                        if (cp.Nested != null && cp.Nested.ContainsKey("ControlActor"))
                        {
                            var inner = cp.Nested["ControlActor"];
                            nested = inner.ContainsKey("AllyAttack");
                            if (nested) allyAtk = inner["AllyAttack"].AttackAmount ?? -999;
                        }
                    }
                    True("  779 ControlActor 嵌套含 AllyAttack", nested);
                    Eq("  779 AllyAttack 的卡面原值（源文件 = -2）", -2, allyAtk);
                }

                // ---------- 4. 抽样 3：牌组与入组关系 ----------
                Console.WriteLine();
                Console.WriteLine("== 抽样3：Standard 牌组（20 张，7 种牌）==");
                ModifierDeck std = null;
                foreach (ModifierDeck d in decks) if (d.Name == "Standard") std = d;
                True("存在 Standard 牌组", std != null);
                if (std != null)
                {
                    Eq("  TotalCards", 20, std.TotalCards);
                    Eq("  牌种数", 7, std.Cards.Count);
                    True("  IsMonsterDeck = False", !std.IsMonsterDeck);
                }

                // 牌组只引用 140 个 AMC 的一部分
                Dictionary<string, DeckMembership> mem = LoadDict<DeckMembership>("deck_membership.json");
                var used = new HashSet<string>();
                foreach (var kv in mem)
                    foreach (DeckEntry e in kv.Value.Cards) used.Add(e.Name);
                True("入组的 AMC 数 < 140（牌组只引用子集）: " + used.Count, used.Count < 140);
                True("入组 AMC 数 > 0: " + used.Count, used.Count > 0);

                // ---------- 5. 每个怪物引用的卡都能找到 ----------
                Console.WriteLine();
                Console.WriteLine("== 引用完整性 ==");
                var cardIds = new HashSet<int>();
                foreach (MonsterCard mc in mcards) cardIds.Add(mc.Id);
                int missing = 0;
                foreach (Monster m in monsters)
                    foreach (int id in m.AbilityCards)
                        if (!cardIds.Contains(id)) missing++;
                Eq("怪物引用的卡中缺失的条数（应为 0）", 0, missing);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("  [XX] 异常: " + e.Message);
                Console.WriteLine(e.StackTrace);
                _fail++;
            }

            Console.WriteLine();
            Console.WriteLine("===== 汇总: " + _ok + " 通过 / " + _fail + " 失败 =====");
            return _fail == 0 ? 0 : 1;
        }

        private static AbilityCard FindCard(List<AbilityCard> list, int id)
        {
            foreach (AbilityCard c in list) if (c != null && c.Id == id) return c;
            return null;
        }

        private static MonsterCard FindMonsterCard(List<MonsterCard> list, int id)
        {
            foreach (MonsterCard c in list) if (c != null && c.Id == id) return c;
            return null;
        }
    }
}
