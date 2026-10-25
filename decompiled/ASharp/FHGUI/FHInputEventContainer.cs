using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FHGUI;

public class FHInputEventContainer : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, ICanvasRaycastFilter, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public class Params : FHInputEventElement.IInputEventData
	{
		public string HintKey;

		public string HitLocalizedOverride;

		public string TooltipKey;

		public string TooltipLocalizedOverride;

		public Sprite ActionIcon;

		public Action ActionCallback;

		public FHInputEventElement.UserInteractionType InteractionType;

		public string GetTextKey()
		{
			return HintKey;
		}

		public string GetTextOverride()
		{
			return HitLocalizedOverride;
		}

		public string GetTooltipKey()
		{
			return TooltipKey;
		}

		public string GetTooltipOverride()
		{
			return TooltipLocalizedOverride;
		}

		public FHInputEventElement.UserInteractionType GetInputAction()
		{
			return InteractionType;
		}

		public Sprite GetIconSprite()
		{
			return ActionIcon;
		}

		public Action GetActionCallback()
		{
			return ActionCallback;
		}

		public bool IsHidden()
		{
			if (HintKey.IsNOTNullOrEmpty())
			{
				return false;
			}
			if (HitLocalizedOverride.IsNOTNullOrEmpty())
			{
				return false;
			}
			if (ActionIcon != null)
			{
				return false;
			}
			return true;
		}
	}

	[Header("Components")]
	[SerializeField]
	protected RectTransform m_Container;

	[SerializeField]
	protected FHInputEventElement m_PrototypeHint;

	[Header("Settings")]
	[SerializeField]
	protected bool m_AutoShowOnHighlight;

	[SerializeField]
	protected bool m_PermanentlyActive;

	protected List<FHInputEventElement> m_Elements = new List<FHInputEventElement>();

	protected bool m_Initialized;

	private const float DOUBLE_CLICK_DELAY = 0.2f;

	private Coroutine m_onClickCoroutine;

	private bool _dragging;

	public bool HasInteractions { get; private set; }

	protected virtual void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_Elements = new List<FHInputEventElement>(5);
			m_Container.GetComponentsInChildren(includeInactive: true, m_Elements);
			SetHintsVisible(visible: false);
		}
	}

	public virtual void Init(IEnumerable<FHInputEventElement.IInputEventData> hints)
	{
		InitCache();
		ClearData();
		if (m_PrototypeHint != null)
		{
			List<FHInputEventElement.IInputEventData> hintsList = hints.ToList();
			HasInteractions = hintsList.Count > 0;
			UIHelperUtil.UpdateUIList(hintsList.Count, m_Elements, m_PrototypeHint, m_Container, (int i, FHInputEventElement element) =>
			{
				PopulateElement(element, hintsList[i], i, hintsList.Count((FHInputEventElement.IInputEventData data) => !data.IsHidden()));
			});
		}
		else
		{
			HasInteractions = false;
		}
	}

	protected virtual void PopulateElement(FHInputEventElement element, FHInputEventElement.IInputEventData elementData, int index, int totalCount)
	{
		element.Init(elementData);
	}

	public virtual void SetHintsVisible(bool visible)
	{
		m_Container.gameObject.SetActive(visible || m_PermanentlyActive);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.dragging)
		{
			if (m_onClickCoroutine != null)
			{
				StopCoroutine(m_onClickCoroutine);
			}
			return;
		}
		bool flag = m_Elements.Any((FHInputEventElement e) =>
		{
			FHInputEventElement.IInputEventData data = e.Data;
			return data != null && data.GetInputAction() == FHInputEventElement.UserInteractionType.DoubleClick;
		});
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (!flag)
			{
				OnClick();
			}
			else if (eventData.clickCount == 1)
			{
				m_onClickCoroutine = StartCoroutine(OnClick_Crt());
			}
			else
			{
				OnDoubleClick();
			}
		}
		else if (eventData.button == PointerEventData.InputButton.Right)
		{
			OnRightClick();
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
		if (m_AutoShowOnHighlight)
		{
			SetHintsVisible(visible: true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (m_AutoShowOnHighlight)
		{
			SetHintsVisible(visible: false);
		}
	}

	private void CancelClick()
	{
		if (m_onClickCoroutine != null)
		{
			StopCoroutine(m_onClickCoroutine);
			m_onClickCoroutine = null;
		}
	}

	private IEnumerator OnClick_Crt()
	{
		yield return new WaitForSeconds(0.2f);
		OnClick();
	}

	public virtual void OnClick()
	{
		if (_dragging)
		{
			return;
		}
		foreach (FHInputEventElement element in m_Elements)
		{
			element.ParseInputEventEvent(FHInputEventElement.UserInteractionType.LeftClick);
		}
	}

	public virtual void OnDoubleClick()
	{
		CancelClick();
		foreach (FHInputEventElement element in m_Elements)
		{
			element.ParseInputEventEvent(FHInputEventElement.UserInteractionType.DoubleClick);
		}
	}

	public virtual void OnRightClick()
	{
		foreach (FHInputEventElement element in m_Elements)
		{
			element.ParseInputEventEvent(FHInputEventElement.UserInteractionType.RightClick);
		}
	}

	public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, screenPoint, eventCamera, out var _))
		{
			return false;
		}
		return true;
	}

	protected void ClearData()
	{
		HasInteractions = false;
		if (m_Elements != null)
		{
			for (int i = 0; i < m_Elements.Count(); i++)
			{
				m_Elements[i].Clear();
			}
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		_dragging = true;
	}

	public virtual void OnDrag(PointerEventData eventData)
	{
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		_dragging = false;
	}
}
