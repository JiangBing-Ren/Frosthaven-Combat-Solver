using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Scenario.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Scenario.Elements;

public class ScenarioPartySlotDetailsElement : MonoBehaviour
{
	[SerializeField]
	private MultiplayerVisualsUIDataSO m_MultiplayerVisualsSO;

	[Header("Base Elements")]
	[SerializeField]
	private UIPartySlot m_PartySlot;

	[SerializeField]
	private UICharacterSpecificElementsContainer m_GaugeContainer;

	[SerializeField]
	private InteractabilityIsolatedUIControl m_IsolatedControl;

	[SerializeField]
	private Image m_FrameImage;

	[SerializeField]
	private Image m_FrameInnerImage;

	[Header("Health")]
	[SerializeField]
	private string m_HealthSpritePrefix = "<sprite name=\"Health\" tint>";

	[SerializeField]
	private TMP_Text m_TextHealth;

	[SerializeField]
	private Image m_ImageHealthOverlay;

	[SerializeField]
	private Image m_ImageHealthOverlayLine;

	[Header("Selection Phase")]
	[SerializeField]
	private Image m_ImageSelection;

	[SerializeField]
	private Sprite m_SpriteCards;

	[SerializeField]
	private Sprite m_SpriteResting;

	[SerializeField]
	private TMP_Text m_TextSelection;

	public void Init(ScenarioPartySlotDetailsDTO data, IEnumerable<UIPartySlotHints.HintedActionParams> actions = null, UIPartySlotInteractionEvents interactionEvents = null)
	{
		if ((bool)m_IsolatedControl)
		{
			m_IsolatedControl.ControlSecondIdentifier = data.PlayerActor?.ActorGuid;
		}
		if ((bool)m_PartySlot)
		{
			m_PartySlot.Init(data.PartySlotDto, null, actions, interactionEvents);
		}
		if ((bool)m_GaugeContainer)
		{
			m_GaugeContainer.Init(data.PlayerActor);
		}
		float num = ((data.IsAvailable && data.HealthMax > 0) ? ((float)(data.HealthMax - data.HealthCurrent) / (float)data.HealthMax) : 0f);
		if ((bool)m_ImageHealthOverlay)
		{
			m_ImageHealthOverlay.fillAmount = num;
		}
		if ((bool)m_ImageHealthOverlayLine)
		{
			m_ImageHealthOverlayLine.rectTransform.anchorMax = new Vector2(1f, num);
		}
		if ((bool)m_TextHealth)
		{
			string sourceText = $"{data.HealthCurrent}/{data.HealthMax}";
			m_TextHealth.enabled = !data.IsInCardsSelectionPhase;
			m_TextHealth.SetText(sourceText);
		}
		if ((bool)m_ImageSelection)
		{
			Sprite selectionSprite = GetSelectionSprite(data);
			m_ImageSelection.enabled = selectionSprite != null;
			m_ImageSelection.sprite = selectionSprite;
		}
		if ((bool)m_TextSelection)
		{
			string selectionText = GetSelectionText(data);
			m_TextSelection.enabled = selectionText != null;
			m_TextSelection.SetText(selectionText);
		}
		int id = (FFSNetwork.IsOnline ? (FFSNetwork.GetControllingPlayer(data.PlayerActor)?.PlayerID.ID ?? 0) : 0);
		MultiplayerVisualsUIDataSO.PlayerVisuals playerVisuals = m_MultiplayerVisualsSO.GetPlayerVisuals(id);
		if ((bool)m_FrameImage)
		{
			m_FrameImage.sprite = playerVisuals.InitiativeSlotFrame;
			m_FrameImage.enabled = FFSNetwork.IsOnline;
		}
		if ((bool)m_FrameInnerImage)
		{
			m_FrameInnerImage.sprite = playerVisuals.InitiativeSlotInnerFrame;
		}
	}

	private Sprite GetSelectionSprite(ScenarioPartySlotDetailsDTO data)
	{
		if (!data.IsInCardsSelectionPhase)
		{
			return null;
		}
		if (data.IsLongResting || data.IsShortResting)
		{
			return m_SpriteResting;
		}
		return m_SpriteCards;
	}

	private string GetSelectionText(ScenarioPartySlotDetailsDTO data)
	{
		if (!data.IsInCardsSelectionPhase)
		{
			return null;
		}
		if (data.IsLongResting)
		{
			return 99.ToString();
		}
		if (data.IsShortResting)
		{
			return "-";
		}
		return $"{data.CardsCurrent}/{data.CardsMax}";
	}
}
