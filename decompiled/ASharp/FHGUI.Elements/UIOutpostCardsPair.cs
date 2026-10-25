using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using GLOOM;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements;

public class UIOutpostCardsPair : MonoBehaviour
{
	public Color ColorLevelUnavailable = Color.white;

	public Color ColorLevelAvailable = Color.yellow;

	public TMP_Text TextLevel;

	public string LocKeyLevel = "GUI_LEVEL";

	public UIOutpostCardSlot First;

	public UIOutpostCardSlot Last;

	[SerializeField]
	protected UIPartySlotHints m_InteractionHintsLeft;

	[SerializeField]
	protected UIPartySlotHints m_InteractionHintsRight;

	private const bool IS_MINI_CARD = false;

	public void Init(int level, CardDTO firstCardDto, CardDTO lastCardDto, UICardInteractionEvents interactionEvents, bool isLevelAvailable)
	{
		string text = $"{LocalizationManager.GetTranslation(LocKeyLevel)} {level}";
		TextLevel.text = text;
		TextLevel.color = (isLevelAvailable ? ColorLevelAvailable : ColorLevelUnavailable);
		First.Init(firstCardDto, isMiniCard: false, interactionEvents, fitInParent: true);
		Last.Init(lastCardDto, isMiniCard: false, interactionEvents, fitInParent: true);
		UpdateInteractionHints(m_InteractionHintsLeft, GetInteractions(!firstCardDto.IsDisabled));
		UpdateInteractionHints(m_InteractionHintsRight, GetInteractions(!lastCardDto.IsDisabled));
	}

	private IEnumerable<UIPartySlotHints.HintedActionParams> GetInteractions(bool isLevelAvailable)
	{
		if (isLevelAvailable)
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_ACTION_ADD"),
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ColorSchemeID = "Add"
			};
		}
		yield return new UIPartySlotHints.HintedActionParams
		{
			HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.MiddleClick, "GUI_INSPECT"),
			ColorSchemeID = "Inspect"
		};
	}

	private void UpdateInteractionHints(UIPartySlotHints componet, IEnumerable<UIPartySlotHints.HintedActionParams> actions)
	{
		if ((object)componet != null)
		{
			if (actions != null)
			{
				componet.Init(actions);
			}
			else
			{
				componet.Clear();
			}
		}
	}

	public void SetHoverReparenting(RectTransform reparentTargetTransform)
	{
		First.SetHoverReparenting(reparentTargetTransform);
		Last.SetHoverReparenting(reparentTargetTransform);
	}

	public void Clear()
	{
		First.Clear();
		Last.Clear();
		m_InteractionHintsLeft.Clear();
		m_InteractionHintsRight.Clear();
	}
}
