using System;
using System.Collections;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI;

public class FHInputEventElement : Selectable, IPointerClickHandler, IEventSystemHandler
{
	public enum UserInteractionType
	{
		None,
		LeftClick,
		RightClick,
		DoubleClick,
		LongPress,
		ExplicitButtonPress,
		MiddleClick
	}

	public interface IInputEventData
	{
		string GetTextKey();

		string GetTextOverride();

		string GetTooltipKey();

		string GetTooltipOverride();

		UserInteractionType GetInputAction();

		Sprite GetIconSprite();

		Action GetActionCallback();

		bool IsHidden();
	}

	[Header("Components")]
	[SerializeField]
	protected TMP_Text m_Label;

	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	protected Image m_Background;

	[SerializeField]
	protected TMP_Text m_Hint;

	private const float _doubleCLickDelay = 0.2f;

	private Coroutine _onClickCrt;

	private bool _pointerIn;

	private bool _initialized;

	public TMP_Text Label => m_Label;

	public Image Background => m_Background;

	public IInputEventData Data { get; private set; }

	protected virtual void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
		}
	}

	public virtual void Init(IInputEventData data)
	{
		InitCache();
		Data = data;
		Populate(data);
	}

	protected virtual void Populate(IInputEventData data)
	{
		if (m_Label != null)
		{
			m_Label.raycastTarget = false;
			string text = data.GetTextOverride() ?? ((!data.GetTextKey().IsNullOrEmpty()) ? LocalizationManager.GetTranslation(data.GetTextKey()) : string.Empty);
			m_Label.text = text;
			m_Label.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
		}
		if (m_Hint != null)
		{
			string text2 = data.GetTooltipOverride() ?? ((!data.GetTooltipKey().IsNullOrEmpty()) ? LocalizationManager.GetTranslation(data.GetTooltipKey()) : string.Empty);
			m_Hint.text = text2;
		}
		if (m_Icon != null)
		{
			m_Icon.sprite = data.GetIconSprite();
			m_Icon.enabled = m_Icon.sprite != null;
		}
		gameObject.SetActive(!data.IsHidden());
		UpdateHighlight();
	}

	private void UpdateHighlight()
	{
		if (m_Hint != null)
		{
			bool flag = currentSelectionState == SelectionState.Highlighted || _pointerIn;
			m_Hint.enabled = flag;
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		_pointerIn = true;
		base.OnPointerEnter(eventData);
		UpdateHighlight();
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		_pointerIn = false;
		base.OnPointerExit(eventData);
		UpdateHighlight();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (eventData.clickCount == 1)
			{
				_onClickCrt = StartCoroutine(OnClick_Crt());
			}
			else
			{
				OnDoubleClick();
			}
		}
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			OnRightClick();
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
	}

	private void CancelClick()
	{
		if (_onClickCrt != null)
		{
			StopCoroutine(_onClickCrt);
			_onClickCrt = null;
		}
	}

	private IEnumerator OnClick_Crt()
	{
		yield return new WaitForSeconds(0.2f);
		OnClick();
	}

	protected virtual void OnClick()
	{
		if (Data != null)
		{
			Data.GetActionCallback()?.Invoke();
		}
	}

	protected virtual void OnDoubleClick()
	{
		CancelClick();
		if (!ParseInputEventEvent(UserInteractionType.DoubleClick))
		{
			Data?.GetActionCallback()?.Invoke();
		}
	}

	protected virtual void OnRightClick()
	{
		ParseInputEventEvent(UserInteractionType.RightClick);
	}

	public bool ParseInputEventEvent(UserInteractionType incomingEventType, bool executeIfExplicit = false)
	{
		if (Data == null)
		{
			return false;
		}
		UserInteractionType inputAction = Data.GetInputAction();
		if (inputAction == incomingEventType || (executeIfExplicit && inputAction == UserInteractionType.ExplicitButtonPress))
		{
			Data.GetActionCallback()?.Invoke();
			return true;
		}
		return false;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_pointerIn = false;
	}

	public virtual void Clear()
	{
		Data = null;
	}
}
