using System;
using DG.Tweening;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class InitiativeTrackEnemyBehaviourExtension : InitiativeTrackActorBehaviourExtension
{
	[Header("Card appearance animation settings")]
	[SerializeField]
	private Ease easeType;

	[SerializeField]
	private Vector3 movementAmount = new Vector3(-5f, -5f);

	[SerializeField]
	private float moveTime = 0.3f;

	[SerializeField]
	private Vector3 scaleAnimationOrigin = new Vector3(0.8f, 0.8f);

	[SerializeField]
	private float scaleTime = 0.5f;

	[SerializeField]
	private float fadeTime = 0.5f;

	[SerializeField]
	private GameObject monsterBasePrefab;

	[SerializeField]
	private Transform monsterBaseHolder;

	private MonsterBaseUI monsterBaseUI;

	private CMonsterAbilityCard monsterAbilityCard;

	private void Awake()
	{
		monsterBaseUI = UnityEngine.Object.Instantiate(monsterBasePrefab, monsterBaseHolder).GetComponent<MonsterBaseUI>();
		monsterBaseUI.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		LayoutElement layoutElement = monsterBaseUI.gameObject.AddComponent<LayoutElement>();
		layoutElement.preferredWidth = monsterBaseUI.GetComponent<RectTransform>().sizeDelta.x;
		layoutElement.preferredHeight = monsterBaseUI.GetComponent<RectTransform>().sizeDelta.y;
	}

	public override void OnSelect()
	{
	}

	public override void OnDeselect()
	{
		monsterBaseUI.gameObject.SetActive(value: false);
	}

	public override void OnAvatarHighlight(bool active)
	{
		if (PhaseManager.PhaseType != CPhase.PhaseType.MonsterClassesSelectAbilityCards && !(GameState.ThreadSleepReason == "ShowingNextMonsterCards"))
		{
			monsterBaseUI.TogglePreview(active: false);
		}
	}

	public override void OnSetAttributesDirect(InitiativeTrack.ActorAttributesParams aaParams)
	{
		CEnemyActor cEnemyActor = (CEnemyActor)aaParams.Actor;
		CMonsterClass cMonsterClass = ((aaParams.Class != null) ? ((CMonsterClass)aaParams.Class) : ((CMonsterClass)cEnemyActor.Class));
		monsterAbilityCard = (aaParams.ShowNextCard ? cMonsterClass.NextAbilityCard : cMonsterClass.RoundAbilityCard);
		monsterBaseUI.GenerateCard(monsterAbilityCard, cEnemyActor, generationProgressive: true, cMonsterClass);
	}

	public void SetCardHolder(Transform cardsHolder)
	{
		monsterBaseHolder = cardsHolder;
		if (monsterBaseUI != null)
		{
			monsterBaseUI.transform.SetParent(monsterBaseHolder, worldPositionStays: false);
		}
	}

	public void AnimateCardHighlight(float orderNumber, float cardsTotal, Action onCompleteCallback, float delay = 0f)
	{
		monsterBaseUI.transform.SetAsLastSibling();
		if (monsterAbilityCard != null)
		{
			monsterBaseUI.AnimateAppearance(monsterAbilityCard, Base.Actor, fadeTime, moveTime, scaleTime, delay, scaleAnimationOrigin, movementAmount, easeType, () =>
			{
				onCompleteCallback?.Invoke();
			});
		}
		else
		{
			onCompleteCallback?.Invoke();
		}
	}

	public void FinishCardHighlightAnimation(float orderNumber, float cardsTotal, Action onCompleteCallback)
	{
		monsterBaseUI.FinishAnimateAppearance();
		OnCardHighlightAnimationComplete(animated: false);
		onCompleteCallback();
	}

	public void OnCardHighlightAnimationComplete(bool animated = true)
	{
		Base.RefreshInitiative();
		Base.Avatar.m_InitiativeText.enabled = true;
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(monsterBaseUI.gameObject);
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		OnAvatarHighlight(active: false);
	}
}
