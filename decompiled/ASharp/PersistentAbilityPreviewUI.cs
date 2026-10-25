using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class PersistentAbilityPreviewUI : MonoBehaviour
{
	[SerializeField]
	public PersistentAbilityPopup descriptionPopup;

	[SerializeField]
	private Transform dotsHolder;

	[SerializeField]
	private GameObject dotsPrefab;

	[SerializeField]
	private Image abilityIconImage;

	[SerializeField]
	private Image durationIconImage;

	[SerializeField]
	private GameObject abilityIconMultiBonus;

	private List<Image> dotsList;

	private Action onCancelActiveAbility;

	private Button button;

	public CActiveBonus ActiveBonus { get; private set; }

	public Selectable Selectable => button;

	private void Awake()
	{
		button = GetComponent<Button>();
	}

	public void SetAbilityBonus(CActiveBonus activeBonus, CActor actor, Transform holder, Action onCancelActiveAbility, int instances)
	{
		this.onCancelActiveAbility = onCancelActiveAbility;
		ActiveBonus = activeBonus;
		ToggleDisplayBonus(active: false);
		descriptionPopup.Init(activeBonus, actor, holder, instances);
		SetDotsState(activeBonus.TrackerIndex, (activeBonus.Layout?.TrackerPattern?.Count).GetValueOrDefault());
		abilityIconImage.sprite = UIInfoTools.Instance.GetActiveAbilityIcon(activeBonus);
		if (activeBonus.Duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			durationIconImage.gameObject.SetActive(value: false);
		}
		else
		{
			durationIconImage.gameObject.SetActive(value: true);
			durationIconImage.sprite = UIInfoTools.Instance.GetActiveAbilityDurationIcon(activeBonus.Duration);
		}
		abilityIconMultiBonus.SetActive(activeBonus.BaseCard.ActiveBonuses.Count > 1);
	}

	private void SetDotsState(int currentRound, int totalRounds)
	{
		HelperTools.NormalizePool(ref dotsList, dotsPrefab, dotsHolder, totalRounds);
		if (currentRound <= totalRounds)
		{
			for (int i = 0; i < currentRound; i++)
			{
				dotsList[i].color = UIInfoTools.Instance.pastRoundColor;
			}
			for (int j = currentRound + 1; j < totalRounds; j++)
			{
				dotsList[j].color = UIInfoTools.Instance.leftRoundColor;
			}
			if (currentRound < totalRounds)
			{
				dotsList[currentRound].color = UIInfoTools.Instance.currentRoundColor;
			}
		}
	}

	public void ToggleDisplayBonus(bool active)
	{
		if (active)
		{
			descriptionPopup.Show();
		}
		else
		{
			descriptionPopup.Hide();
		}
	}

	public void CancelActiveAbility()
	{
		onCancelActiveAbility?.Invoke();
	}

	public void OnActiveBonusTriggered()
	{
		descriptionPopup.OnActiveBonusTriggered();
	}

	private bool CanCancelActivesMP()
	{
		if (!Choreographer.s_Choreographer.PlayerToSelectAbilityCardsOrLongRest && (Choreographer.s_Choreographer.CurrentPlayerActor == null || ActiveBonus.Caster != Choreographer.s_Choreographer.CurrentPlayerActor))
		{
			return false;
		}
		if (!ActiveBonus.Actor.IsUnderMyControl() && (!(ActiveBonus.Actor is CHeroSummonActor cHeroSummonActor) || !cHeroSummonActor.Summoner.IsUnderMyControl()) && (!(ActiveBonus.Actor is CEnemyActor) || !ActiveBonus.Caster.IsUnderMyControl()))
		{
			return false;
		}
		return true;
	}

	public void SetInteractable(bool interactable)
	{
		button.interactable = interactable && ActiveBonus.Duration != CActiveBonus.EActiveBonusDurationType.Round && (!FFSNetwork.IsOnline || CanCancelActivesMP());
	}
}
