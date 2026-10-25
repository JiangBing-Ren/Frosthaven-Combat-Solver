using System;
using System.Reflection;
using UnityEngine;

namespace FHSolver.Bridge
{
    /// <summary>
    /// 本地化查询 —— 走游戏自己的接口，不自己维护中英文对照表。
    ///
    /// 游戏用的是 I2 Localization（被包了一层）：
    ///   GLOOM.LocalizationManager.GetTranslation(term, ...)   ← Assembly-CSharp
    ///     └─ I2.Loc.LocalizationManager.GetTranslation(...)
    ///
    /// 卡牌的本地化键就是 <c>CAbilityCard.Name</c>（见 AbilityCardUI.MakeAbilityCard：
    /// <c>CardName = cardData.Name</c>），单位的本地化键是 <c>CActor.ActorLocKey()</c>
    /// （怪物 → Class.LocKey，玩家 → CharacterClass.LocKey）。
    ///
    /// 这里用反射调用而不是编译期引用 Assembly-CSharp：
    /// 本工程一直刻意不引用 Assembly-CSharp（依赖图极深），只在运行时按名取类型。
    /// </summary>
    internal static class Loc
    {
        private static Type _type;
        private static MethodInfo _method;
        private static bool _tried;
        private static int _misses;

        /// <summary>取不到的键数（用于判断是否用错了键名）。</summary>
        public static int Misses => _misses;

        /// <summary>
        /// 取本地化文本。取不到就原样返回 key（并计数），绝不返回 "UNDEFINED" 之类。
        /// </summary>
        public static string T(string key)
        {
            if (string.IsNullOrEmpty(key)) return key;

            if (!_tried)
            {
                _tried = true;
                try
                {
                    _type = Type.GetType("GLOOM.LocalizationManager, Assembly-CSharp");
                    if (_type == null)
                    {
                        _type = Type.GetType("I2.Loc.LocalizationManager, Assembly-CSharp");
                    }

                    if (_type != null)
                    {
                        // GLOOM 版有 10 个参数（全有默认值）；I2 版签名不同，退回按参数个数找
                        foreach (MethodInfo m in _type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                        {
                            if (m.Name != "GetTranslation") continue;
                            ParameterInfo[] ps = m.GetParameters();
                            if (ps.Length == 10 && ps[0].ParameterType == typeof(string))
                            {
                                _method = m;
                                break;
                            }
                        }
                        if (_method == null)
                        {
                            foreach (MethodInfo m in _type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                            {
                                if (m.Name != "GetTranslation") continue;
                                ParameterInfo[] ps = m.GetParameters();
                                if (ps.Length >= 1 && ps[0].ParameterType == typeof(string))
                                {
                                    _method = m;
                                    break;
                                }
                            }
                        }
                    }

                    Plugin.Log?.LogInfo(_type == null
                        ? "[FHSolver] 本地化：找不到 LocalizationManager，将直接显示内部名"
                        : $"[FHSolver] 本地化: {_type.FullName} 方法={( _method != null ? "GetTranslation" : "未找到" )}");
                }
                catch (Exception e)
                {
                    Plugin.Log?.LogWarning($"[FHSolver] 本地化初始化失败: {e.Message}");
                }
            }

            if (_method == null) return key;

            try
            {
                object[] args =
                {
                    key,        // term
                    true,       // fixForRTL
                    0,          // maxLineLengthForRTL
                    true,       // ignoreRTLnumbers
                    false,      // applyParameters
                    null,       // localParametersRoot
                    null,       // overrideLanguage
                    true,       // skipWarnings —— 不往游戏日志里刷警告
                    false,      // useDefaultIfMissing
                    false       // returnNullIfNotFound
                };
                string r = _method.Invoke(null, args) as string;
                if (string.IsNullOrEmpty(r) || r.StartsWith("UNDEFINED", StringComparison.Ordinal))
                {
                    _misses++;
                    return key;
                }
                return StripTags(r);
            }
            catch (Exception)
            {
                return key;
            }
        }

        /// <summary>I2 的文本可能带富文本标签，纯文本输出前剥掉。</summary>
        private static string StripTags(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.IndexOf('<') < 0) return s;

            var sb = new System.Text.StringBuilder(s.Length);
            bool inTag = false;
            foreach (char c in s)
            {
                if (c == '<') { inTag = true; continue; }
                if (c == '>') { inTag = false; continue; }
                if (!inTag) sb.Append(c);
            }
            return sb.ToString().Trim();
        }
    }
}
