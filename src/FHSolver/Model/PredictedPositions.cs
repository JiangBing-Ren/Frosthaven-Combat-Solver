using System.Collections.Generic;

namespace FHSolver.Model
{
    /// <summary>
    /// 把"先动敌人的预测落点"写进单位快照。
    ///
    /// 为什么单独抽出来：这段逻辑踩过两次同一个坑
    /// （v1.32 改到空列表、v1.34 误放进"非敌对单位"的 else 分支），
    /// 而它埋在 Bridge 层无法离线回归。抽成纯函数后测试台可以直接锁住：
    /// **即使一个友方单位都没有，也照样生效。**
    /// </summary>
    public static class PredictedPositions
    {
        /// <summary>一条预测：谁、先攻多少、会走到哪。</summary>
        public struct Move
        {
            public string Guid;
            public int Initiative;
            public int X;
            public int Y;
        }

        /// <summary>
        /// 只改**先攻比我低**（会先动）的敌人 —— 后动的敌人这回合还没走，位置不变。
        /// 返回实际重写的单位数（便于日志/回归断言）。
        /// </summary>
        public static int Apply(List<UnitSnapshot> monsters, IReadOnlyList<Move> moves, int myInitiative)
        {
            if (monsters == null || moves == null || moves.Count == 0 || myInitiative <= 0) return 0;

            var byGuid = new Dictionary<string, Move>();
            foreach (Move m in moves)
            {
                if (!string.IsNullOrEmpty(m.Guid)) byGuid[m.Guid] = m;
            }

            int rewritten = 0;
            foreach (UnitSnapshot u in monsters)
            {
                if (u == null || string.IsNullOrEmpty(u.Guid)) continue;

                Move mv;
                if (!byGuid.TryGetValue(u.Guid, out mv)) continue;
                if (mv.Initiative <= 0 || mv.Initiative >= myInitiative) continue;

                u.X = mv.X;
                u.Y = mv.Y;
                rewritten++;
            }
            return rewritten;
        }
    }
}
