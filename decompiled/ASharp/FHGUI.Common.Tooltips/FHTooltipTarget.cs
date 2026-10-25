using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Common.Tooltips;

public abstract class FHTooltipTarget<T, T1> : UITooltipTarget where T : ITooltip<T1>
{
	protected TooltipPositionParams? _positionParams;

	protected T TooltipInstance { get; private set; }

	public bool ManualShowHide { get; protected set; }

	public RectTransform AnchorTarget { get; private set; }

	public abstract T TooltipIPrefab { get; }

	protected abstract void OnInitialize(T1 data, TooltipPositionParams? positionParams = null);

	public void Initialize(T1 data, TooltipPositionParams? positionParams = null, bool manualShowHide = false)
	{
		_positionParams = positionParams;
		ManualShowHide = manualShowHide;
		hideBackground = true;
		TooltipEnabled = data != null;
		try
		{
			if (TooltipInstance != null && TooltipInstance.GameObject != null && TooltipInstance.GameObject.activeSelf)
			{
				HideTooltip();
			}
		}
		catch (Exception)
		{
		}
		OnInitialize(data, positionParams);
	}

	public void ShowManually(float delay = -1f)
	{
		if (!ManualShowHide)
		{
			T tooltipInstance = TooltipInstance;
			Log.GUIWarning($"Trying to show tooltip {((tooltipInstance != null) ? tooltipInstance.GameObject.name : null)} that is not set to {ManualShowHide}");
		}
		else
		{
			ShowTooltip(delay);
		}
	}

	protected override void ShowTooltip(float delay = -1f)
	{
		if (!CanShowTooltip())
		{
			return;
		}
		UITooltipManager.ResetContent();
		UITooltipManager.ShowBackground(show: false);
		try
		{
			TooltipInstance = ObjectPool.Spawn(TooltipIPrefab.GameObject, UITooltipManager.GetTransform(), Vector3.zero, Quaternion.identity).GetComponent<T>();
			RectTransform rectTransform = null;
			rectTransform = (AnchorTarget = (_positionParams.HasValue ? (transform as RectTransform) : (((anchorToExactMouseTargetInstead && _eventData != null) ? _eventData.pointerEnter.transform : transform) as RectTransform)));
			OnShowInternal();
			base.ShowTooltip(delay);
			if (_positionParams.HasValue)
			{
				TooltipPositionParams value = _positionParams.Value;
				TooltipInstance.GameObject.transform.position = FindTooltipPos(value);
			}
			else
			{
				TooltipInstance.GameObject.transform.position = FindTooltipPos(TextAnchor.UpperLeft, rectTransform.position);
			}
			FitTooltipInScreen();
		}
		catch (Exception)
		{
			Log.GUIError("Cannot show tooltip");
			HideTooltip();
		}
	}

	public override void HideTooltip(float delay = -1f)
	{
		if (!tooltipShown)
		{
			return;
		}
		T tooltipInstance = TooltipInstance;
		if (tooltipInstance == null || tooltipInstance.GameObject.activeSelf)
		{
			try
			{
				ObjectPool.Recycle(TooltipInstance.GameObject, TooltipIPrefab.GameObject, "Tooltips Pool");
				OnHideInternal();
				base.HideTooltip(delay);
			}
			catch (Exception)
			{
				tooltipInstance = TooltipInstance;
				Log.GUIError("Error occured while hiding tooltip " + ((tooltipInstance != null) ? tooltipInstance.GameObject.name : null));
			}
			TooltipInstance = default;
		}
	}

	protected Vector2 GetCornerOffset(Vector2 pivot, TextAnchor anchor)
	{
		return anchor switch
		{
			TextAnchor.LowerCenter => new Vector2(0.5f - pivot.x, 0f - pivot.y), 
			TextAnchor.LowerLeft => new Vector2(0f - pivot.x, 0f - pivot.y), 
			TextAnchor.LowerRight => new Vector2(1f - pivot.x, 0f - pivot.y), 
			TextAnchor.MiddleCenter => new Vector2(0.5f - pivot.x, 0.5f - pivot.y), 
			TextAnchor.MiddleLeft => new Vector2(0f - pivot.x, 0.5f - pivot.y), 
			TextAnchor.MiddleRight => new Vector2(1f - pivot.x, 0.5f - pivot.y), 
			TextAnchor.UpperCenter => new Vector2(0.5f - pivot.x, 1f - pivot.y), 
			TextAnchor.UpperLeft => new Vector2(0f - pivot.x, 1f - pivot.y), 
			TextAnchor.UpperRight => new Vector2(1f - pivot.x, 1f - pivot.y), 
			_ => Vector2.zero, 
		};
	}

	protected Vector3 FindTooltipPos(TooltipPositionParams positionParams)
	{
		return FindTooltipPos(positionParams.Anchor, (positionParams.TransformAnchor != null) ? positionParams.TransformAnchor.position : positionParams.Pos);
	}

	protected Vector3 FindTooltipPos(TextAnchor anchor, Vector3 position)
	{
		RectTransform rectTransform = TooltipInstance.GameObject.transform as RectTransform;
		Vector2 cornerOffset = GetCornerOffset(rectTransform.pivot, anchor);
		Vector2 size = rectTransform.rect.size;
		return rectTransform.position + position - rectTransform.localToWorldMatrix.MultiplyPoint(new Vector3(cornerOffset.x * size.x, cornerOffset.y * size.y, 0f));
	}

	protected void FitTooltipInScreen()
	{
		if (TooltipInstance != null)
		{
			RectTransform obj = TooltipInstance.GameObject.transform as RectTransform;
			Vector3 vector = obj.DeltaWorldPositionToFitTheScreen(UIManager.Instance.UICamera, 20f);
			obj.position += vector;
		}
	}

	protected override bool CanShowTooltip()
	{
		return true;
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (!ManualShowHide)
		{
			base.OnPointerEnter(eventData);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (!ManualShowHide)
		{
			base.OnPointerExit(eventData);
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		if (!ManualShowHide)
		{
			base.OnSelect(eventData);
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		if (!ManualShowHide)
		{
			base.OnDeselect(eventData);
		}
	}

	protected abstract void OnShowInternal();

	protected abstract void OnHideInternal();
}
