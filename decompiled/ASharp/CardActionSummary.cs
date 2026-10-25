using System;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardActionSummary : MonoBehaviour
{
	public readonly struct Params
	{
		public readonly bool IsLost;

		public readonly bool IsPermaLost;

		public readonly bool HasRoundBonus;

		public readonly bool HasPermBonus;

		public readonly float Xp;

		public bool HasXp => Xp > 0f;

		public bool HasAnyBonusActions
		{
			get
			{
				if (!IsLost && !IsPermaLost && !HasRoundBonus && !HasPermBonus)
				{
					return HasXp;
				}
				return true;
			}
		}

		public Params(bool isLost, bool isPermaLost, bool hasRoundBonus, bool hasPermBonus, float xp)
		{
			IsLost = isLost;
			IsPermaLost = isPermaLost;
			HasRoundBonus = hasRoundBonus;
			HasPermBonus = hasPermBonus;
			Xp = xp;
		}
	}

	[SerializeField]
	private Image background;

	[SerializeField]
	private UIFX_MaterialFX_Control lost;

	[SerializeField]
	private UIFX_MaterialFX_Control permLost;

	[SerializeField]
	private UIFX_MaterialFX_Control roundBonus;

	[SerializeField]
	private UIFX_MaterialFX_Control permBonus;

	[SerializeField]
	private GameObject actionXP;

	[field: SerializeField]
	public CPlayerActor.EAgility RequiredAgility { get; private set; }

	public void Init(Params cornerActionsParams, bool doFit)
	{
		gameObject.SetActive(cornerActionsParams.HasAnyBonusActions);
		lost?.gameObject.SetActive(cornerActionsParams.IsLost);
		permLost?.gameObject.SetActive(cornerActionsParams.IsPermaLost);
		roundBonus?.gameObject.SetActive(cornerActionsParams.HasRoundBonus && !cornerActionsParams.HasPermBonus);
		permBonus?.gameObject.SetActive(cornerActionsParams.HasPermBonus);
		if (actionXP != null)
		{
			actionXP.gameObject.SetActive(cornerActionsParams.HasXp);
			TextMeshProUGUI componentInChildren = actionXP.GetComponentInChildren<TextMeshProUGUI>();
			float xp = cornerActionsParams.Xp;
			componentInChildren.text = xp.ToString();
		}
		if (doFit)
		{
			float spacing = 0f;
			float x = 0f;
			float size = 0f;
			FitCornerAction(cornerActionsParams.IsLost, lost?.gameObject, spacing, ref x, ref size);
			FitCornerAction(cornerActionsParams.IsPermaLost, permLost?.gameObject, spacing, ref x, ref size);
			FitCornerAction(cornerActionsParams.HasRoundBonus, roundBonus?.gameObject, spacing, ref x, ref size);
			FitCornerAction(cornerActionsParams.HasPermBonus, permBonus?.gameObject, spacing, ref x, ref size);
			FitCornerAction(cornerActionsParams.HasXp, actionXP, spacing, ref x, ref size);
			RectTransform rectTransform = transform as RectTransform;
			rectTransform.sizeDelta = new Vector2(size, rectTransform.sizeDelta.y);
		}
	}

	public Func<CAbility, bool> GetFilter()
	{
		if (RequiredAgility == CPlayerActor.EAgility.None)
		{
			return null;
		}
		return (CAbility ability) =>
		{
			if (ability.StartAbilityRequirements?.RequiredAgility != RequiredAgility)
			{
				CAbilityRequirements startAbilityRequirements = ability.StartAbilityRequirements;
				if (startAbilityRequirements == null)
				{
					return false;
				}
				return startAbilityRequirements.RequiredAgility == CPlayerActor.EAgility.None;
			}
			return true;
		};
	}

	public void SetColor(Color color)
	{
		if (!(background == null))
		{
			background.color = color;
		}
	}

	public void ToggleAnimateBurnIcon(bool active)
	{
		if (lost != null)
		{
			lost.ToggleEnable(active);
		}
		if (permLost != null)
		{
			permLost.ToggleEnable(active);
		}
	}

	private void FitCornerAction(bool active, GameObject el, float spacing, ref float x, ref float size)
	{
		if (!(el == null))
		{
			el.SetActive(active);
			if (active)
			{
				RectTransform rectTransform = el.transform as RectTransform;
				Rect rect = rectTransform.rect;
				rectTransform.localPosition = new Vector3(-1f * x, rectTransform.localPosition.y, 0f);
				float width = rect.width;
				size += width + spacing;
				x += width + spacing;
			}
		}
	}
}
