using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class ModifiersDisplayController : MonoBehaviour
{
	[SerializeField]
	private UIAttackModifierCalculator characterMissMod;

	[SerializeField]
	private UIAttackModifierCalculator characterMinusTwoMod;

	[SerializeField]
	private UIAttackModifierCalculator characterMinusOneMod;

	[SerializeField]
	private UIAttackModifierCalculator characterZeroMod;

	[SerializeField]
	private UIAttackModifierCalculator characterPlusOneMod;

	[SerializeField]
	private UIAttackModifierCalculator characterPlusTwoMod;

	[SerializeField]
	private UIAttackModifierCalculator characterTimesTwoMod;

	[SerializeField]
	private RectTransform modifiersContent;

	[SerializeField]
	private GameObject modifiersHighlight;

	[SerializeField]
	private float animationTime = 0.5f;

	[SerializeField]
	private VerticalLayoutGroup popupGroup;

	[SerializeField]
	private ContentSizeFitter popupContentSizeFitter;

	[SerializeField]
	private CanvasGroup popupCanvasGroup;

	[Header("Conditionals")]
	[SerializeField]
	private UIScenarioAttackModifier modifierPrefab;

	[SerializeField]
	private RectTransform conditionalModifierContainer;

	private Tween currentAnimation;

	private bool active;

	private bool resetPositionToHide = true;

	private List<UIScenarioAttackModifier> conditionalPool = new List<UIScenarioAttackModifier>();

	private void Awake()
	{
		popupCanvasGroup = popupGroup.GetComponent<CanvasGroup>();
		popupContentSizeFitter = popupGroup.GetComponent<ContentSizeFitter>();
		Hide(immendiately: true);
	}

	public void Hide(bool immendiately = false)
	{
		if (active || immendiately)
		{
			active = false;
			if (currentAnimation != null)
			{
				currentAnimation.Kill();
				currentAnimation = null;
			}
			if (immendiately)
			{
				popupGroup.enabled = true;
				popupContentSizeFitter.enabled = true;
				HideFinish();
			}
			else
			{
				popupGroup.enabled = false;
				popupContentSizeFitter.enabled = false;
				currentAnimation = modifiersContent.DOAnchorPosX(0f, animationTime).OnComplete(HideFinish);
			}
		}
	}

	private void HideFinish()
	{
		currentAnimation = null;
		resetPositionToHide = true;
		UIManager.Instance.UnhighlightElement(modifiersHighlight.gameObject, unlockUI: false);
		popupCanvasGroup.alpha = 0f;
		popupCanvasGroup.interactable = false;
		popupCanvasGroup.blocksRaycasts = false;
	}

	public void Display(CCharacterClass characterClass, bool isFocused = true)
	{
		active = true;
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.AttackModifiersHovered));
		characterMissMod.SetupCounters("*0", null, characterClass, isFocused);
		characterMinusTwoMod.SetupCounters("-2", null, characterClass, isFocused);
		characterMinusOneMod.SetupCounters("-1", null, characterClass, isFocused);
		characterZeroMod.SetupCounters("+0", null, characterClass, isFocused);
		characterPlusOneMod.SetupCounters("+1", null, characterClass, isFocused);
		characterPlusTwoMod.SetupCounters("+2", null, characterClass, isFocused);
		characterTimesTwoMod.SetupCounters("*2", null, characterClass, isFocused);
		RefreshConditionalModifiers(characterClass, isFocused);
		UIManager.Instance.HighlightElement(modifiersHighlight.gameObject, fadeEverythingElse: false, lockUI: false);
		if (currentAnimation != null)
		{
			currentAnimation.Kill();
			currentAnimation = null;
		}
		popupGroup.enabled = false;
		popupContentSizeFitter.enabled = false;
		popupCanvasGroup.alpha = 1f;
		popupCanvasGroup.interactable = true;
		popupCanvasGroup.blocksRaycasts = true;
		if (resetPositionToHide)
		{
			resetPositionToHide = false;
			modifiersContent.anchoredPosition3D = new Vector3(0f, modifiersContent.anchoredPosition3D.y, modifiersContent.anchoredPosition3D.z);
		}
		currentAnimation = modifiersContent.DOAnchorPosX(modifiersContent.rect.width, animationTime).OnComplete(() =>
		{
			currentAnimation = null;
			popupGroup.enabled = true;
			popupContentSizeFitter.enabled = true;
		});
	}

	private void RefreshConditionalModifiers(CCharacterClass characterClass, bool isFocused = true)
	{
		List<IGrouping<string, AttackModifierYMLData>> list = (from it in characterClass.AttackModifierCards.Concat(characterClass.DiscardedAttackModifierCards)
			where it.IsConditionalModifier || it.IsBless || it.IsCurse
			group it by it.Name).ToList();
		HelperTools.NormalizePool(ref conditionalPool, modifierPrefab.gameObject, conditionalModifierContainer, list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			AttackModifierYMLData modif = list[num].First();
			conditionalPool[num].Init(modif, list[num].Count(), isFocused);
			conditionalPool[num].UpdateCounters(list[num].Count(), characterClass.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => x.Name == modif.Name));
		}
	}

	private void OnDisable()
	{
		if (currentAnimation != null)
		{
			currentAnimation.Kill();
			currentAnimation = null;
		}
	}

	public void SetUnfocused(bool unfocused)
	{
		if (IsOpen())
		{
			characterMissMod.SetUnfocused(unfocused);
			characterMinusTwoMod.SetUnfocused(unfocused);
			characterMinusOneMod.SetUnfocused(unfocused);
			characterZeroMod.SetUnfocused(unfocused);
			characterPlusOneMod.SetUnfocused(unfocused);
			characterPlusTwoMod.SetUnfocused(unfocused);
			characterTimesTwoMod.SetUnfocused(unfocused);
			for (int i = 0; i < conditionalPool.Count && conditionalPool[i].gameObject.activeSelf; i++)
			{
				conditionalPool[i].SetUnfocused(unfocused);
			}
		}
	}

	public bool IsOpen()
	{
		return active;
	}
}
