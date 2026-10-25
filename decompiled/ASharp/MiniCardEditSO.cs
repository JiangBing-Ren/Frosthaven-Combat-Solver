using System.Linq;
using Libs.Utils;
using SETUtil.Extend;
using UnityEngine;

public class MiniCardEditSO : ScriptableObject
{
	public Vector2 Size = new Vector2(300f, 40f);

	public float HoverScaleFactor = 1.33f;

	public Vector3 HoverMovement = new Vector3(-10f, 0f, 0f);

	public GameObject BasePrefab;

	public GameObject[] NonVariantPrefabs;

	[ContextMenu("Apply Deck Settings")]
	public void ApplyDeckSettings()
	{
		foreach (GameObject item in NonVariantPrefabs.Concat(BasePrefab.AsEnumerable()))
		{
			if (!(item == null))
			{
				RectTransform rectTransform = item.transform as RectTransform;
				RectTransform obj = rectTransform.FindTransformInChildren("Mini") as RectTransform;
				RectTransform overrideTransform = obj.FindTransformInChildren("Container_Initiative") as RectTransform;
				rectTransform.sizeDelta = Size;
				obj.sizeDelta = Size;
				AbilityCardUI component = item.GetComponent<AbilityCardUI>();
				component.defaultHighlightScale = HoverScaleFactor;
				component.defaultHoverMovement = HoverMovement;
				UIHoverScaleEffects orAdd = component.GetOrAdd<UIHoverScaleEffects>();
				orAdd.ScaleOnSelected = true;
				orAdd.ScaleFactor = HoverScaleFactor;
				orAdd.ScaleFactorOnClick = HoverScaleFactor;
				orAdd.OverrideTransform = overrideTransform;
				component._hoverScale = orAdd;
				UIHoverMoveEffects orAdd2 = component.GetOrAdd<UIHoverMoveEffects>();
				orAdd2.MoveOnSelected = true;
				orAdd2.MoveOffset = HoverMovement;
				orAdd2.MoveOffsetOnClick = HoverMovement;
				orAdd2.OverrideTransform = overrideTransform;
				component._hoverMove = orAdd2;
				ExtendedButton component2 = item.GetComponent<ExtendedButton>();
				component2.highlightScaleFactor = 1f;
				component2.hoverMovement = Vector3.zero;
				component2.overridedTargetRectScale = null;
				component2.overridedTargetRectMovement = null;
			}
		}
	}

	[ContextMenu("Link FX")]
	public void LinkFX()
	{
		foreach (GameObject item in NonVariantPrefabs.Concat(BasePrefab.AsEnumerable()))
		{
			if (!(item == null))
			{
				RectTransform rectTransform = ((item.transform as RectTransform).FindTransformInChildren("Mini") as RectTransform).FindTransformInChildren("Container_Initiative") as RectTransform;
				item.GetComponent<AbilityCardUI>().fxControls = rectTransform.GetComponentInChildren<UIFX_MaterialFX_Control>();
			}
		}
	}
}
