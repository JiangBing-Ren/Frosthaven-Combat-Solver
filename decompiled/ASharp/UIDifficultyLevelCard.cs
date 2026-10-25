using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Elements;
using GLOOM;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDifficultyLevelCard : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private UIHoverResizeEffects _descriptionResizeEffects;

	[SerializeField]
	private FHToggle _toggle;

	[SerializeField]
	private List<TextLocalizedListener> _descriptionLocalizes;

	[Space]
	[Header("Elements")]
	[SerializeField]
	private Image _illustration;

	[SerializeField]
	private Image _textBackground;

	[SerializeField]
	private Image _frameHighlight;

	[SerializeField]
	private Image _titleBackground;

	[SerializeField]
	private Image _indicatorHighlight;

	[Space]
	[Header("Elements")]
	[SerializeField]
	private Sprite _illustrationState_On;

	[SerializeField]
	private Sprite _illustrationState_Off;

	[SerializeField]
	private Sprite _textBackgroundState_On;

	[SerializeField]
	private Sprite _textBackgroundState_Off;

	[SerializeField]
	private Sprite _titleBackgroundState_On;

	[SerializeField]
	private Sprite _titleBackgroundState_Off;

	[SerializeField]
	private Color _descriptionState_On;

	[SerializeField]
	private Color _descriptionState_Off;

	[Space]
	[Header("Tooltip")]
	[SerializeField]
	private UITextTooltipTarget _tooltip;

	[field: SerializeField]
	public Selectable Selectable { get; private set; }

	public bool IsSelected { get; private set; }

	private void Awake()
	{
		UIAudioEffects component = GetComponent<UIAudioEffects>();
		if ((bool)component)
		{
			component.CanPlayClickSound = () => !IsSelected;
		}
	}

	public void OnDifficultySelected(bool isOn)
	{
		SetSelectedState(isOn);
	}

	public void SetMoveOnSelectableSelected(bool moveOnSelected)
	{
		_descriptionResizeEffects.MoveOnSelected = moveOnSelected;
	}

	[Obsolete("Descriptions are manually set in the prefab")]
	private void SetDescription(IList<string> locKeys)
	{
		HelperTools.NormalizePool(ref _descriptionLocalizes, _descriptionLocalizes[0].gameObject, _descriptionLocalizes[0].transform.parent, locKeys.Count);
		for (int i = 0; i < locKeys.Count; i++)
		{
			_descriptionLocalizes[i].SetTextKey(locKeys[i].RemoveChar('$'));
		}
		if (locKeys.Count == 0)
		{
			_descriptionLocalizes[0].gameObject.SetActive(value: true);
		}
	}

	public void SetModifierTooltip(IList<string> locKeys)
	{
		string text = string.Join("\n", locKeys.Select((string key) => LocalizationManager.GetTranslation(key.RemoveChar('$'))));
		if (_tooltip != null)
		{
			_tooltip.SetText(text);
		}
		if ((bool)_toggle)
		{
			_toggle.SetTooltipText(text, text, text);
		}
	}

	private void SetSelectedState(bool selected)
	{
		IsSelected = selected;
		if (IsSelected)
		{
			Select();
		}
		else
		{
			ResetState();
		}
	}

	private void Select()
	{
		_illustration.sprite = _illustrationState_On;
		_textBackground.sprite = _textBackgroundState_On;
		_frameHighlight.enabled = true;
		_titleBackground.sprite = _titleBackgroundState_On;
		_indicatorHighlight.enabled = true;
		foreach (TextLocalizedListener descriptionLocalize in _descriptionLocalizes)
		{
			descriptionLocalize.Text.color = _descriptionState_On;
		}
		_descriptionResizeEffects.SetLock(locked: true);
	}

	private void ResetState()
	{
		_illustration.sprite = _illustrationState_Off;
		_textBackground.sprite = _textBackgroundState_Off;
		_frameHighlight.enabled = false;
		_titleBackground.sprite = _titleBackgroundState_Off;
		_indicatorHighlight.enabled = false;
		foreach (TextLocalizedListener descriptionLocalize in _descriptionLocalizes)
		{
			descriptionLocalize.Text.color = _descriptionState_Off;
		}
		_descriptionResizeEffects.SetLock(locked: false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (_toggle.isOn)
		{
			return;
		}
		_illustration.sprite = _illustrationState_On;
		_textBackground.sprite = _textBackgroundState_Off;
		_frameHighlight.enabled = true;
		_titleBackground.sprite = _titleBackgroundState_On;
		foreach (TextLocalizedListener descriptionLocalize in _descriptionLocalizes)
		{
			descriptionLocalize.Text.color = _descriptionState_Off;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!_toggle.isOn)
		{
			ResetState();
		}
	}
}
