using System;
using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPersonalQuestCard : MonoBehaviour
{
	public GameObject Highlight;

	public Image ImageSeparatorGlow;

	public TMP_Text TextName;

	public TMP_Text TextDescription;

	public TMP_Text TextGoalDescription;

	public Button ButtonSelect;

	private Action _onButtonClick;

	private void Awake()
	{
		ButtonSelect.onClick.AddListener(OnButtonClick);
	}

	private void OnButtonClick()
	{
		if (Highlight != null && !Highlight.activeSelf)
		{
			AudioControllerUtils.PlaySound(UIInfoTools.Instance?.radioButtonPressAudioItem);
		}
		_onButtonClick?.Invoke();
	}

	public void Init(CharacterPersonalQuestDTO personalQuest, Action onButtonClick)
	{
		if (TextName != null)
		{
			UIHelperUtil.SetTextKey(TextName, personalQuest.Def.Name);
		}
		if (TextDescription != null)
		{
			UIHelperUtil.SetTextKey(TextDescription, personalQuest.Def.Description);
		}
		if (TextGoalDescription != null)
		{
			TextGoalDescription.text = personalQuest.GoalDescription;
		}
		_onButtonClick = onButtonClick;
	}

	public void ToggleHighlight(bool isHighlighted)
	{
		if (Highlight != null)
		{
			Highlight.SetActive(isHighlighted);
		}
		if (ImageSeparatorGlow != null)
		{
			ImageSeparatorGlow.enabled = isHighlighted;
		}
	}
}
