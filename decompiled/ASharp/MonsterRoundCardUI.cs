using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FHGUI;
using FHGUI.Common;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class MonsterRoundCardUI : MonoBehaviour, IAbilityCardUI, IPooleable, IGeneralInfoProvider
{
	[SerializeField]
	private MonsterUIDataSO _monsterDataSO;

	[Header("Content")]
	[SerializeField]
	private MonsterCardActionUI _cardActionUI;

	[SerializeField]
	private GameObject _containerShuffle;

	[SerializeField]
	private UITextTooltipTarget _tooltip;

	[Header("Variants")]
	[SerializeField]
	private MonsterCardSkin _defaultSkin;

	[SerializeField]
	private MonsterCardSkin _bossSkin;

	[SerializeField]
	private MonsterCardSkin _eliteSkin;

	[SerializeField]
	private MonsterCardSkin _neutralSkin;

	[SerializeField]
	private MonsterCardSkin _allySkin;

	private int _originalCardID;

	private MonsterCardConditionalElement[] _conditionalElements;

	private ConsumeButton[] _consumeButtons;

	private static readonly string[] baseSubtypes = new string[4] { "Move", "Attack", "Target", "Range" };

	public MonsterCardActionUI ActionUI => _cardActionUI;

	[field: SerializeField]
	public int CardID { get; private set; }

	public ObjectPool.ECardType CardType => ObjectPool.ECardType.Monster;

	public MonsterCardSkin Skin { get; private set; }

	public MonsterCardUIContext Context { get; private set; } = new MonsterCardUIContext();

	private MonsterCardConditionalElement[] ConditionalElements
	{
		get
		{
			if (_conditionalElements == null || !Application.isPlaying)
			{
				_conditionalElements = GetComponentsInChildren<MonsterCardConditionalElement>(includeInactive: true);
			}
			return _conditionalElements;
		}
	}

	private ConsumeButton[] ConsumeButtons
	{
		get
		{
			if (_consumeButtons == null || !Application.isPlaying)
			{
				_consumeButtons = GetComponentsInChildren<ConsumeButton>(includeInactive: true);
			}
			return _consumeButtons;
		}
	}

	private IEnumerable<MonsterCardSkin> GetAllSkins()
	{
		yield return _defaultSkin;
		yield return _bossSkin;
		yield return _eliteSkin;
		yield return _neutralSkin;
		yield return _allySkin;
	}

	private void Awake()
	{
		_originalCardID = CardID;
	}

	private void Start()
	{
		UITextTooltipTarget[] componentsInChildren = gameObject.GetComponentsInChildren<UITextTooltipTarget>(includeInactive: false);
		for (int i = 0; i < componentsInChildren?.Length; i++)
		{
			if (!(componentsInChildren[i] == null))
			{
				if (LocalizationGlossaryUtils.TryParseGlossaryTexts(componentsInChildren[i].gameObject, out var parsedText, checkChildren: false))
				{
					componentsInChildren[i].Initialize(UITooltipManager.Corner.Auto, new Vector2(5f, 0f), anchorToExactMouseTargetInstead: false, 300f, 50f, autoAdjustHeight: false, hideBackground: false);
					componentsInChildren[i].SetText(parsedText);
				}
				else
				{
					componentsInChildren[i].enabled = false;
				}
			}
		}
	}

	public void AnimateAppearance(Vector3 startPosition, int fullRotations, float rotationTime, Ease easeType)
	{
		transform.localPosition = startPosition;
		transform.localRotation = Quaternion.Euler(0f, -360 * fullRotations + 90, 0f);
		gameObject.SetActive(value: true);
		transform.DORotate(new Vector3(0f, 360 * fullRotations - 90, 0f), rotationTime).SetEase(easeType);
	}

	public void Init(MonsterCardYMLData cardYML, CEnemyActor enemyForBaseStats = null, bool showFaction = false, int? overrideLevel = null)
	{
		Init(cardYML, enemyForBaseStats, enemyForBaseStats?.MonsterClass, showFaction, overrideLevel);
	}

	public void Init(MonsterCardYMLData cardYML, CMonsterClass monsterClass, bool showFaction = false, int? overrideLevel = null)
	{
		Init(cardYML, null, monsterClass, showFaction, overrideLevel);
	}

	public void Init(MonsterCardUIContext context)
	{
		if (!(context == Context))
		{
			Init(context.CardYML, context.EnemyForBaseStats, context.FallbackMonsterClass, context.ShowFaction, context.OverrideLevel);
		}
	}

	private void Init(MonsterCardYMLData cardYML, CEnemyActor enemyForBaseStats, CMonsterClass fallbackMonsterClass, bool showFaction, int? overrideLevel = null)
	{
		Context = new MonsterCardUIContext
		{
			CardYML = cardYML,
			EnemyForBaseStats = enemyForBaseStats,
			FallbackMonsterClass = fallbackMonsterClass,
			ShowFaction = showFaction,
			OverrideLevel = overrideLevel
		};
		CardID = cardYML.ID;
		if (_cardActionUI.IsGenerated)
		{
			_cardActionUI.GenerateLayout(Context);
		}
		else
		{
			ConsumeButton[] consumeButtons = ConsumeButtons;
			for (int i = 0; i < consumeButtons.Length; i++)
			{
				consumeButtons[i].ResetState();
			}
			MonsterCardConditionalElement[] conditionalElements = ConditionalElements;
			for (int i = 0; i < conditionalElements.Length; i++)
			{
				conditionalElements[i].Init(Context);
			}
		}
		ActivateTargetCardSkin();
		Skin.InitCardSkin(Context);
		_containerShuffle.SetActive(cardYML.Shuffle);
		OnLocalize(Context.MonsterClass, Context.EnemyForBaseStats, Context.OverrideLevel);
	}

	public void ShowForLevel(int level)
	{
		Context.OverrideLevel = level;
		Init(Context);
	}

	public void MakeCard(MonsterCardYMLData card, CMonsterClass monsterClass = null)
	{
		_originalCardID = card.ID;
		Init(card, monsterClass);
		_cardActionUI.GenerateLayout(card, null, monsterClass);
	}

	private void EnsureCardYml()
	{
		MonsterCardUIContext context = Context;
		if (context.CardYML == null)
		{
			context.CardYML = ScenarioRuleClient.SRLYML.MonsterCards.FirstOrDefault((MonsterCardYMLData x) => x.ID == CardID);
		}
	}

	public void Localize()
	{
		OnLocalize(Context?.MonsterClass, Context?.EnemyForBaseStats, Context?.OverrideLevel);
	}

	private void OnLocalize(CMonsterClass monsterClass = null, CEnemyActor targetingActor = null, int? overrideLevel = null)
	{
		EnsureCardYml();
		AbilityCardActionLocalizedListener[] componentsInChildren = GetComponentsInChildren<AbilityCardActionLocalizedListener>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].OnLanguageChanged(Context.CardYML.Name, Context.CardYML.CardAction, new CardLayoutRow.PropertyLookupContext
			{
				Class = monsterClass,
				Actor = Context.EnemyForBaseStats,
				TargetingActor = targetingActor,
				OverrideLevel = overrideLevel
			}, (CAbility ability, int amount, string subtype, CAbilityOverride abilityOverride, int? oLevel) => GetStatValues(monsterClass, ability, amount, subtype, abilityOverride, targetingActor, oLevel));
		}
	}

	private static bool IsStatSubtype(string subtype)
	{
		if (!(subtype == "Target") && !(subtype == "Range"))
		{
			return subtype == "RetaliateRange";
		}
		return true;
	}

	private static bool IsStrengthSubtype(string subtype)
	{
		if (!subtype.IsNullOrEmpty())
		{
			switch (subtype)
			{
			default:
				return subtype == "Pierce";
			case "Attack":
			case "Move":
			case "Strength":
			case "Heal":
			case "Retaliate":
				return true;
			}
		}
		return true;
	}

	private static bool IsBaseStatValue(CAbility ability, string subtype, CAbilityOverride abilityOverride)
	{
		if (subtype.IsNullOrEmpty() || subtype == "Strength")
		{
			string text;
			if (ability is CAbilityMove)
			{
				text = "Move";
			}
			else
			{
				text = ((!(ability is CAbilityAttack)) ? subtype : "Attack");
			}
			subtype = text;
		}
		if (ability.StrengthIsBase)
		{
			if (string.IsNullOrEmpty(subtype))
			{
				goto IL_00f4;
			}
			switch (subtype)
			{
			case "Strength":
			case "Move":
			case "Attack":
				goto IL_00f4;
			}
		}
		if ((!(subtype == "Range") || !(abilityOverride?.RangeIsBase ?? ability.RangeIsBase)) && (!(subtype == "Target") || !(abilityOverride?.TargetIsBase ?? ability.TargetIsBase)))
		{
			return !subtype.In(baseSubtypes);
		}
		goto IL_00f4;
		IL_00f4:
		return true;
	}

	private static int GetFinalAmount(int amount, bool statIsBase, int? statValue)
	{
		if (statIsBase || !statValue.HasValue)
		{
			return amount;
		}
		return amount + statValue.Value;
	}

	private static CAbility.EAbilityStatType GetStatType(string subtype)
	{
		switch (subtype)
		{
		case "Range":
			return CAbility.EAbilityStatType.Range;
		case "Target":
			return CAbility.EAbilityStatType.NumberOfTargets;
		case "Tokens":
			return CAbility.EAbilityStatType.Tokens;
		case "Pierce":
			return CAbility.EAbilityStatType.Pierce;
		case "Strength":
		case "Move":
		case "Attack":
			return CAbility.EAbilityStatType.Strength;
		default:
			return CAbility.EAbilityStatType.None;
		}
	}

	private static AbilityData.StatIsBasedOnXData getStatIsBasedOnXData(CAbility ability, CMonsterClass monsterClass, string subtype)
	{
		List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries = ability.StatIsBasedOnXEntries;
		if ((statIsBasedOnXEntries != null && statIsBasedOnXEntries.Count == 0) || monsterClass == null)
		{
			return null;
		}
		CAbility.EAbilityStatType statType = GetStatType(subtype);
		return ability.StatIsBasedOnXEntries.SingleOrDefault((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == statType);
	}

	public static string GetStatValues(CMonsterClass monsterClass, CAbility ability, int amount, string subtype, CAbilityOverride abilityOverride, CEnemyActor targetingActor = null, int? overrideLevel = null)
	{
		AbilityData.StatIsBasedOnXData statIsBasedOnXData = getStatIsBasedOnXData(ability, monsterClass, subtype);
		bool flag = IsBaseStatValue(ability, subtype, abilityOverride);
		bool flag2 = !flag && (statIsBasedOnXData?.AddTo ?? true);
		if (monsterClass == null)
		{
			monsterClass = targetingActor?.MonsterClass;
		}
		BaseStats baseStats = ((!overrideLevel.HasValue) ? monsterClass?.CurrentMonsterStat : monsterClass?.GetMonsterStatsData(overrideLevel.Value));
		if (subtype == "Range")
		{
			amount = GetFinalAmount(amount, !flag2, overrideLevel.HasValue ? baseStats?.Range : monsterClass?.Range);
		}
		else if (subtype == "Target")
		{
			amount = GetFinalAmount(amount, !flag2, overrideLevel.HasValue ? baseStats?.Target : monsterClass?.Target);
		}
		else if (ability is CAbilityMove)
		{
			amount = GetFinalAmount(amount, flag, overrideLevel.HasValue ? baseStats?.Move : monsterClass?.Move);
		}
		else if (ability is CAbilityAttack)
		{
			int amount2 = amount;
			bool statIsBase = flag;
			int? statValue;
			if (targetingActor != null)
			{
				statValue = targetingActor.AttackStrengthForUI;
			}
			else
			{
				statValue = (overrideLevel.HasValue ? baseStats?.Attack : monsterClass?.Attack);
			}
			amount = GetFinalAmount(amount2, statIsBase, statValue);
		}
		if (ability.UseSpecialBaseStat && monsterClass != null && baseStats.SpecialBaseStats.ContainsKey(ability.AbilityType) && IsStrengthSubtype(subtype))
		{
			amount += baseStats.SpecialBaseStats[ability.AbilityType];
		}
		if (monsterClass == null && !flag)
		{
			if (amount < 0)
			{
				return amount.ToString();
			}
			return $"+{amount}";
		}
		return amount.ToString();
	}

	private void ActivateTargetCardSkin()
	{
		MonsterCardSkin targetSkin = GetTargetSkin();
		if (Skin == targetSkin)
		{
			return;
		}
		Skin = targetSkin;
		foreach (MonsterCardSkin allSkin in GetAllSkins())
		{
			allSkin.gameObject.SetActive(allSkin == Skin);
		}
	}

	private MonsterCardSkin GetTargetSkin()
	{
		CEnemyActor enemyForBaseStats = Context.EnemyForBaseStats;
		if (enemyForBaseStats == null)
		{
			return GetTargetSkin(Context.FallbackMonsterClass);
		}
		MonsterCardSkin monsterCardSkin = null;
		if (enemyForBaseStats.IsBoss)
		{
			monsterCardSkin = _bossSkin;
		}
		else if (enemyForBaseStats.IsElite())
		{
			monsterCardSkin = _eliteSkin;
		}
		else if (enemyForBaseStats.Type == CActor.EType.Ally)
		{
			monsterCardSkin = _allySkin;
		}
		else if (enemyForBaseStats.Type == CActor.EType.Neutral)
		{
			monsterCardSkin = _neutralSkin;
		}
		if (!monsterCardSkin)
		{
			return _defaultSkin;
		}
		return monsterCardSkin;
	}

	private MonsterCardSkin GetTargetSkin(CMonsterClass monsterClass)
	{
		if (monsterClass == null)
		{
			return _defaultSkin;
		}
		MonsterCardSkin monsterCardSkin = null;
		if (monsterClass.MonsterType == EMonsterType.Boss)
		{
			monsterCardSkin = _bossSkin;
		}
		else if (monsterClass.NonEliteVariant != null)
		{
			monsterCardSkin = _eliteSkin;
		}
		if (!monsterCardSkin)
		{
			return _defaultSkin;
		}
		return monsterCardSkin;
	}

	public void OnReturnedToPool()
	{
		Clean();
	}

	public void Clean()
	{
		foreach (MonsterCardSkin allSkin in GetAllSkins())
		{
			if (allSkin != null && allSkin.gameObject.activeSelf)
			{
				allSkin.gameObject.SetActive(value: false);
			}
		}
		Context = null;
		_cardActionUI.Clean();
		Skin = null;
	}

	public void OnRemovedFromPool()
	{
	}

	public IGeneralInfoData GetInfoData()
	{
		EnsureCardYml();
		return UIGeneralInfoUtils.GetMonsterCard(Context.CardYML, Context.EnemyForBaseStats, Context.FallbackMonsterClass);
	}
}
