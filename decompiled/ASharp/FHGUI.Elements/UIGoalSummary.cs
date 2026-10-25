using System;
using FHGUI.DTO;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIGoalSummary : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	public struct Params
	{
		public GoalDTO DTO;

		public Action OnClickedCallback;

		public bool IgnoreLocalization;

		public string HintsLiteral;
	}

	private const string TOOLTIP_ROW_SPACING = "\n\n";

	public Image ImageHighlight;

	public Image ImageActive;

	public Image ImageCheck;

	public TMP_Text TextTitle;

	public TMP_Text TextDescription;

	public TMP_Text TextGoal;

	public TMP_Text TextReward;

	public TMP_Text TextRewardValue;

	public GameObject Highlighted;

	public UITooltipTarget TooltipTarget;

	public GameObject CustomTooltip;

	private Action _onClickCallback;

	private bool _isHighlighted;

	private void OnEnable()
	{
		UpdateHighlights();
	}

	private void OnDisable()
	{
		_onClickCallback = null;
	}

	public void Init(Params @params)
	{
		_onClickCallback = @params.OnClickedCallback;
		if (TextTitle != null)
		{
			TextTitle.text = @params.DTO.GetName(@params.IgnoreLocalization);
		}
		if (TextDescription != null)
		{
			TextDescription.text = @params.DTO.GetDescription(@params.IgnoreLocalization);
		}
		if (TextGoal != null)
		{
			TextGoal.text = @params.DTO.GetGoal(@params.IgnoreLocalization);
		}
		if ((bool)TextReward)
		{
			TextReward.text = @params.DTO.RewardLiteral;
		}
		if ((bool)TextRewardValue)
		{
			TextRewardValue.text = @params.DTO.RewardValue;
		}
		if ((bool)ImageCheck)
		{
			ImageCheck.gameObject.SetActive(@params.DTO.IsChecked);
		}
		if ((bool)ImageActive)
		{
			ImageActive.gameObject.SetActive(@params.DTO.IsChecked);
		}
		if ((bool)Highlighted)
		{
			Highlighted.gameObject.SetActive(@params.DTO.Highlihed);
		}
		SetTooltip(@params);
	}

	private void SetTooltip(Params @params)
	{
		if (TooltipTarget == null)
		{
			return;
		}
		if (TooltipTarget is UITextTooltipTarget)
		{
			string text = @params.DTO.GetName(@params.IgnoreLocalization);
			string description = @params.DTO.GetDescription(@params.IgnoreLocalization);
			string text2 = FHUILocFormatting.Bold(text) + "\n\n" + description + "\n\n" + FHUILocFormatting.Bold(@params.DTO.RewardLiteral);
			(TooltipTarget as UITextTooltipTarget)?.SetText(text2);
		}
		if (TooltipTarget is UIPrefabTooltipTarget)
		{
			(TooltipTarget as UIPrefabTooltipTarget).Initialize(CustomTooltip, (GameObject go) =>
			{
				UIGenericTooltipRenderer.Params obj = new UIGenericTooltipRenderer.Params
				{
					Icon = null,
					CaptionLiteral = @params.DTO.GetName(@params.IgnoreLocalization),
					DescriptionLiteral = @params.DTO.GetDescription(@params.IgnoreLocalization) + "\n\n" + LocalizationManager.GetTranslation("GUI_ADV_SIDE_SCENARIO_REWARD") + "\n" + @params.DTO.RewardLiteral,
					Hints = @params.HintsLiteral
				};
				go.GetComponent<UIGenericTooltipRenderer>()?.Init(obj);
			});
		}
	}

	private void UpdateHighlights()
	{
		if ((bool)ImageHighlight)
		{
			ImageHighlight.enabled = _isHighlighted;
		}
	}

	public void Clear()
	{
		if (ImageActive != null)
		{
			ImageActive.enabled = false;
		}
		if (ImageCheck != null)
		{
			ImageCheck.enabled = false;
		}
		if (TextTitle != null)
		{
			TextTitle.text = null;
		}
		if (TextDescription != null)
		{
			TextDescription.text = null;
		}
		if (TextGoal != null)
		{
			TextGoal.text = null;
		}
		if (TextReward != null)
		{
			TextReward.text = null;
		}
		_onClickCallback = null;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (_onClickCallback != null && ImageCheck != null && !ImageCheck.gameObject.activeSelf)
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance?.radioButtonPressAudioItem);
			}
			_onClickCallback?.Invoke();
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	public void OnPointerUp(PointerEventData eventData)
	{
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_isHighlighted = true;
		UpdateHighlights();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_isHighlighted = false;
		UpdateHighlights();
	}
}
