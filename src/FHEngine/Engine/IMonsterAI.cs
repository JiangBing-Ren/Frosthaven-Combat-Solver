using System.Collections.Generic;
using FHEngine.Model;

namespace FHEngine.Engine
{
    /// <summary>
    /// 怪物 AI 的**纯函数**输出：这一回合会怎么走、打谁。
    ///
    /// 纯函数意味着：相同输入 → 相同输出，**不读取任何隐藏状态、不产生随机**。
    /// 随机源（行动卡抽取、修正牌序）全部作为**入参**显式传入。
    /// </summary>
    public sealed class MonsterPlan
    {
        /// <summary>移动路径（含起点与每一步）。空表示不移动。</summary>
        public IReadOnlyList<HexCoord> Path;

        /// <summary>攻击的目标单位 Id 列表（顺序 = 攻击结算顺序）。空表示不攻击。</summary>
        public IReadOnlyList<string> Targets;

        /// <summary>焦点单位 Id（用于展示"它在追谁"）。</summary>
        public string FocusActorId;

        /// <summary>是否走的是"够不着 → 逼近"路径（真实 AI 的 fallback）。</summary>
        public bool IsApproachOnly;

        /// <summary>调试/对拍用的判定依据。</summary>
        public string Reason;
    }

    /// <summary>
    /// 怪物 AI —— 严格按"找焦点 → 用实际移动力优化攻击 → 只执行卡面所示"实现。
    ///
    /// 契约：
    ///   1. **纯函数**：只读入参，不改任何状态，不产生随机。
    ///   2. 焦点选择：距离最近者；同距时按先攻打破平局（**该规则待 hook 验证**，见假设清单 A3）。
    ///   3. 落点优化：在焦点下找"能打到它 + 移动代价最小"的格子。
    ///   4. **只执行卡面所示**：卡面有 Move 节点才移动，有 Attack 节点才攻击；
    ///      卡面没有的节点**不臆造**（不能因为"怪物有 Attack 属性"就凭空生成一次攻击）。
    /// </summary>
    public interface IMonsterAI
    {
        /// <summary>
        /// 规划一个怪物本回合的行动。
        /// </summary>
        /// <param name="monster">怪物状态（含已解析的 Move/Attack/Range/Target）。</param>
        /// <param name="map">地图（阻挡、地形）。</param>
        /// <param name="actors">场上所有单位（焦点候选）。</param>
        /// <param name="card">本回合翻出的行动卡（**结构化能力树**，来自 monster_cards.json）。</param>
        MonsterPlan Plan(
            MonsterState monster,
            MapState map,
            IReadOnlyList<ActorState> actors,
            MonsterCard card);
    }

    /// <summary>阶段 2 的空壳实现。阶段 3 落地。</summary>
    public sealed class DefaultMonsterAI : IMonsterAI
    {
        public MonsterPlan Plan(
            MonsterState monster,
            MapState map,
            IReadOnlyList<ActorState> actors,
            MonsterCard card)
        {
            throw new System.NotImplementedException("阶段 3 实现");
        }
    }

    /// <summary>
    /// 战斗引擎接口。
    ///
    /// 核心不变量：
    ///   · 状态只能通过 EventApplier.Apply 改变；
    ///   · 每个决策点必须挂起等待外部注入；
    ///   · EventLog 按 seq 单调递增，可完整重放。
    /// </summary>
    public interface ICombatEngine
    {
        /// <summary>当前状态（只读快照语义）。</summary>
        BattleState State { get; }

        /// <summary>已产生的事件日志。</summary>
        IReadOnlyList<EngineEvent> EventLog { get; }

        /// <summary>当前挂起的决策请求；null 表示无挂起。</summary>
        DecisionRequest PendingDecision { get; }

        /// <summary>
        /// 推进战斗，直到：回合结束、或需要决策（此时挂起并返回）。
        /// </summary>
        /// <param name="decisions">决策提供者；引擎挂起时向它请求。</param>
        /// <returns>本次推进产生的事件数。</returns>
        int Step(IDecisionProvider decisions);

        /// <summary>跑完整场（测试/回放用）。</summary>
        IReadOnlyList<EngineEvent> RunToCompletion(IDecisionProvider decisions);
    }
}
