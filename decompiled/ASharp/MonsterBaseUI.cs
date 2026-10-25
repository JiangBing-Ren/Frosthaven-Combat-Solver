using System;
using System.Collections.Generic;
using DG.Tweening;
using FHGUI.Common;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterBaseUI : MonoBehaviour
{
	[SerializeField]
	private string unknownSymbol;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI baseInitiativeText;

	[SerializeField]
	private TextMeshProUGUI baseHealthText;

	[SerializeField]
	private TextMeshProUGUI baseMoveText;

	[SerializeField]
	private TextMeshProUGUI baseAttackText;

	[SerializeField]
	private TextMeshProUGUI baseRangeText;

	[SerializeField]
	private Transform effectsDesriptionHolder;

	[SerializeField]
	private Transform monsterCardHolder;

	[SerializeField]
	private Image shuffleIcon;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private Image textBackgroundOpacityImage;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private GameObject roleGameObject;

	[SerializeField]
	private TextLocalizedListener roleText;

	private CMonsterAbilityCard m_DeferredMonsterCard;

	private CEnemyActor m_DeferredEnemyActor;

	private Coroutine makeMergingCoroutine;

	private MonsterRoundCardUI monsterRoundCardUI;

	private int monsterRoundCardId;

	private CEnemyActor enemyActor;

	private int baseHealthValue;

	private int baseMoveValue;

	private int baseAttackValue;

	private int baseRangeValue;

	private int baseHealValue;

	private int baseTargetValue;

	private int baseRetaliateValue;

	private Dictionary<CAbility.EAbilityType, int> specialBaseStats = new Dictionary<CAbility.EAbilityType, int>();

	private Sequence fadeSequence;

	private List<LayoutRow> LayoutRows = new List<LayoutRow>();

	public CEnemyActor Enemy => enemyActor;

	public void TogglePreview(bool active)
	{
		if (active && m_DeferredMonsterCard != null && PhaseManager.PhaseType >= CPhase.PhaseType.MonsterClassesSelectAbilityCards && m_DeferredEnemyActor != null)
		{
			GenerateCard(m_DeferredMonsterCard, m_DeferredEnemyActor);
			m_DeferredMonsterCard = null;
			m_DeferredEnemyActor = null;
		}
		gameObject.SetActive(active);
	}

	private void SetBaseStats(CMonsterAbilityCard roundCard, CEnemyActor sourceEnemy, bool clearInstant = true, CMonsterClass sourceMonsterClass = null)
	{
		if (roundCard == null)
		{
			return;
		}
		CMonsterClass cMonsterClass = ((sourceEnemy != null) ? sourceEnemy.MonsterClass : sourceMonsterClass);
		if (cMonsterClass == null)
		{
			return;
		}
		specialBaseStats.Clear();
		enemyActor = sourceEnemy;
		if (monsterRoundCardId != roundCard.ID)
		{
			if (monsterRoundCardUI != null)
			{
				ObjectPool.RecycleCard(monsterRoundCardId, ObjectPool.ECardType.Monster, monsterRoundCardUI.gameObject);
			}
			monsterRoundCardUI = ObjectPool.SpawnCard(roundCard.ID, ObjectPool.ECardType.Monster, monsterCardHolder).GetComponent<MonsterRoundCardUI>();
		}
		RectTransform component = monsterRoundCardUI.GetComponent<RectTransform>();
		Vector2 vector = (component.pivot = new Vector2(0.5f, 0.5f));
		component.anchoredPosition = Vector2.zero;
		component.anchorMin = vector;
		component.anchorMax = vector;
		component.localScale = Vector3.one;
		monsterRoundCardId = roundCard.ID;
		if (sourceEnemy != null)
		{
			monsterRoundCardUI.Init(roundCard.GetMonsterCardYML, sourceEnemy, UICharacterUtils.HasFactions());
		}
		else
		{
			monsterRoundCardUI.Init(roundCard.GetMonsterCardYML, cMonsterClass, UICharacterUtils.HasFactions());
		}
		GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(sourceEnemy);
		if (gameObject != null)
		{
			CharacterManager characterManager = CharacterManager.GetCharacterManager(gameObject);
			backgroundImage.sprite = characterManager.CharacterCardBackgroundSprite;
		}
		baseHealValue = 0;
		baseHealthValue = sourceEnemy?.MaxHealth ?? 0;
		baseMoveValue = sourceEnemy?.MonsterClass.Move ?? cMonsterClass.Move;
		baseAttackValue = sourceEnemy?.AttackStrengthForUI ?? cMonsterClass.Attack;
		baseRangeValue = sourceEnemy?.MonsterClass.Range ?? cMonsterClass.Range;
		baseTargetValue = sourceEnemy?.MonsterClass.Target ?? cMonsterClass.Target;
		switch (sourceEnemy?.Type ?? CActor.EType.Enemy)
		{
		case CActor.EType.Ally:
			roleText.SetTextKey("GUI_ALLY");
			roleGameObject.SetActive(value: true);
			break;
		case CActor.EType.Enemy2:
			roleText.SetTextKey("GUI_ENEMY2");
			roleGameObject.SetActive(value: true);
			break;
		case CActor.EType.Neutral:
			roleText.SetTextKey("GUI_NEUTRAL");
			roleGameObject.SetActive(value: true);
			break;
		default:
			roleGameObject.SetActive(value: false);
			break;
		}
		foreach (CAbility ability in roundCard.Action.Abilities)
		{
			Dictionary<CAbility.EAbilityType, int> dictionary = sourceEnemy?.MonsterClass.CurrentMonsterStat.SpecialBaseStats ?? cMonsterClass.CurrentMonsterStat.SpecialBaseStats;
			if (ability.UseSpecialBaseStat && cMonsterClass.CurrentMonsterStat.SpecialBaseStats.ContainsKey(ability.AbilityType))
			{
				specialBaseStats.Add(ability.AbilityType, dictionary[ability.AbilityType]);
			}
		}
		if (sourceEnemy != null)
		{
			foreach (CAbility ability2 in sourceEnemy.MonsterClass.BaseAction.Abilities)
			{
				if (ability2.AbilityType == CAbility.EAbilityType.Heal)
				{
					baseRangeValue = ability2.Range;
					baseHealValue = ability2.Strength;
				}
			}
		}
		titleText.text = LocalizationManager.GetTranslation((sourceEnemy != null) ? sourceEnemy.ActorLocKey() : cMonsterClass.LocKey);
		baseInitiativeText.text = roundCard.Initiative.ToString();
		baseHealthText.text = baseHealthValue.ToString();
		baseMoveText.text = baseMoveValue.ToString();
		baseAttackText.text = ((baseAttackValue > 0) ? baseAttackValue.ToString() : unknownSymbol);
		baseRangeText.text = ((baseRangeValue == 1) ? unknownSymbol : baseRangeValue.ToStringWithInfinity());
	}

	public void GenerateCard(CMonsterAbilityCard monsterCard, CEnemyActor enemy, bool generationProgressive = false, CMonsterClass monsterClass = null)
	{
		if (monsterCard == null || (PhaseManager.PhaseType < CPhase.PhaseType.MonsterClassesSelectAbilityCards && monsterClass?.NextAbilityCard != monsterCard))
		{
			return;
		}
		m_DeferredEnemyActor = null;
		m_DeferredMonsterCard = null;
		if (enemyActor == enemy && monsterRoundCardId == monsterCard.ID)
		{
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(enemy);
			if (gameObject != null)
			{
				CharacterManager characterManager = CharacterManager.GetCharacterManager(gameObject);
				backgroundImage.sprite = characterManager.CharacterCardBackgroundSprite;
			}
		}
		SetBaseStats(monsterCard, enemy, !generationProgressive, monsterClass);
		shuffleIcon.enabled = monsterCard.Shuffle;
	}

	public void AnimateAppearance(CMonsterAbilityCard monsterAbilityCard, CActor actor, float fadeTime, float moveTime, float scaleTime, float delay, Vector3 scaleAnimationOrigin, Vector3 moveDisplacement, Ease easeType, Action onCompleteCallback)
	{
		container.localScale = scaleAnimationOrigin;
		canvasGroup.alpha = 0f;
		container.anchoredPosition3D = moveDisplacement;
		gameObject.SetActive(value: true);
		fadeSequence = DOTween.Sequence().AppendInterval(delay).Insert(delay, canvasGroup.DOFade(1f, fadeTime))
			.Insert(delay, container.DOScale(Vector3.one, scaleTime))
			.Insert(delay, container.DOAnchorPos(Vector3.zero, moveTime).SetEase(easeType))
			.OnComplete(() =>
			{
				onCompleteCallback();
			});
	}

	public void FinishAnimateAppearance()
	{
		if (fadeSequence != null)
		{
			fadeSequence.Kill();
			fadeSequence = null;
		}
		StopAllCoroutines();
		gameObject.SetActive(value: true);
		container.anchoredPosition3D = Vector3.zero;
		container.localScale = Vector3.one;
		canvasGroup.alpha = 1f;
	}
}
