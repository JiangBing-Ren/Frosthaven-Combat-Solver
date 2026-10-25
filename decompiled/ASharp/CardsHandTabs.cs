using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class CardsHandTabs : MonoBehaviour
{
	[SerializeField]
	private GameObject characterTabPrefab;

	[SerializeField]
	private ToggleGroup toggleGroup;

	[SerializeField]
	private float higlightTimeAnimation = 1f;

	private List<CharacterSymbolTab> tabsList = new List<CharacterSymbolTab>();

	private const string TabName = "Character tab";

	private Tween highlightAnim;

	public void Init(List<CPlayerActor> players)
	{
		HelperTools.NormalizePool(ref tabsList, characterTabPrefab, transform, players.Count);
		int num = 0;
		foreach (CPlayerActor player in players)
		{
			tabsList[num].name = "Character tab" + player.Class.ID;
			tabsList[num].Init(toggleGroup, player, OnTabClick);
			num++;
		}
	}

	public void Reset(CPlayerActor actorToReset = null)
	{
		foreach (CharacterSymbolTab tabs in tabsList)
		{
			if (actorToReset == null || actorToReset == tabs.PlayerActor)
			{
				tabs.UpdateSelectedCardsNum((tabs?.PlayerActor != null) ? (Singleton<CardsHandManager>.Instance.GetSelectedCards(tabs.PlayerActor.ID)?.Count ?? 0) : 0);
			}
		}
		RefreshConnectionState();
		UpdateTabsInteraction();
		toggleGroup.SetAllTogglesOff();
		tabsList.FirstOrDefault()?.Select();
	}

	public void UpdateSelectedCardsNum(CPlayerActor playerActor, int cardsNumber)
	{
		foreach (CharacterSymbolTab tabs in tabsList)
		{
			if (tabs.PlayerActor == playerActor)
			{
				tabs.UpdateSelectedCardsNum(cardsNumber);
				break;
			}
		}
	}

	public void SelectTab(CPlayerActor playerActor)
	{
		foreach (CharacterSymbolTab tabs in tabsList)
		{
			if (tabs.PlayerActor == playerActor)
			{
				tabs.Select();
				break;
			}
		}
	}

	private void OnTabClick(CPlayerActor playerActor)
	{
		Log.GUIInfo("OnTabClick click");
		InitiativeTrack.Instance.Select(playerActor);
	}

	public void UpdateTabsInteraction(bool interactable = true)
	{
		foreach (CharacterSymbolTab tabs in tabsList)
		{
			tabs.SetInteractable(interactable && !tabs.PlayerActor.IsDead);
			tabs.RefreshDead();
		}
	}

	public void RefreshConnectionState()
	{
		bool flag = false;
		foreach (CharacterSymbolTab tabs in tabsList)
		{
			tabs.RefreshPlayerOnlineState();
			flag |= FFSNetwork.IsOnline && tabs.PlayerActor.IsUnderMyControl();
		}
		if (flag)
		{
			StartHighlightAnim();
		}
		else
		{
			StoptHighlightAnim();
		}
	}

	private void StartHighlightAnim()
	{
		if (highlightAnim == null)
		{
			highlightAnim = DOTween.To(Highlight, 0f, 1f, higlightTimeAnimation).SetLoops(-1, LoopType.Restart);
		}
	}

	private void StoptHighlightAnim()
	{
		if (highlightAnim != null)
		{
			highlightAnim.Kill();
			highlightAnim = null;
		}
	}

	private void Highlight(float percent)
	{
		foreach (CharacterSymbolTab tabs in tabsList)
		{
			tabs.SetHighlightProgress(percent);
		}
	}

	private void OnDisable()
	{
		StoptHighlightAnim();
	}

	private void OnEnable()
	{
		RefreshConnectionState();
	}

	public void SelectNextTab()
	{
		List<CharacterSymbolTab> list = tabsList.FindAll((CharacterSymbolTab it) => !it.PlayerActor.IsDead || it.IsSelected);
		int num = list.FindIndex((CharacterSymbolTab it) => it.IsSelected);
		list[(num + 1) % list.Count].Click();
	}

	public void SelectPreviousTab()
	{
		List<CharacterSymbolTab> list = tabsList.FindAll((CharacterSymbolTab it) => !it.PlayerActor.IsDead || it.IsSelected);
		int num = list.FindIndex((CharacterSymbolTab it) => it.IsSelected);
		((num == 0) ? list.Last() : list[num - 1]).Click();
	}
}
