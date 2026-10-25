using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCardUIAbilityRowPreview : MonoBehaviour
{
	[SerializeField]
	private RectTransform _fullCardTransform;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private Image _abilityPreview;

	private List<AbilityCardUIAbilityRow> _abilityRows;

	private AbilityCardUIAbilityRow _activeRow;

	private RectTransform _abilityPreviewRT;

	private void Awake()
	{
		_abilityRows = new List<AbilityCardUIAbilityRow>(_fullCardTransform.GetComponentsInChildren<AbilityCardUIAbilityRow>());
		if (_abilityPreview != null)
		{
			_abilityPreviewRT = _abilityPreview.transform as RectTransform;
		}
		if (_fullCardTransform != null)
		{
			_fullCardTransform = GetComponentInParent<FullAbilityCard>()?.transform as RectTransform;
		}
	}

	private void Start()
	{
		Hide();
	}

	private void OnDisable()
	{
		Hide();
	}

	public void OnAbilityStart(string abilityName)
	{
		AbilityCardUIAbilityRow abilityCardUIAbilityRow = _abilityRows.FirstOrDefault((AbilityCardUIAbilityRow r) => r.AbilityName == abilityName);
		if (abilityCardUIAbilityRow != null && abilityCardUIAbilityRow.isActiveAndEnabled)
		{
			Show(abilityCardUIAbilityRow);
		}
	}

	public void OnAbilityEnd(string abilityName)
	{
		Hide();
	}

	public void Reset()
	{
		Hide();
	}

	private void Hide()
	{
		_canvasGroup.alpha = 0f;
		if (_abilityPreview != null)
		{
			_abilityPreview.enabled = false;
		}
		_activeRow = null;
	}

	private void Show(AbilityCardUIAbilityRow row)
	{
		_canvasGroup.alpha = 1f;
		_activeRow = row;
		if (_abilityPreview != null)
		{
			_abilityPreview.enabled = true;
			RectTransform rectTransform = _activeRow.transform as RectTransform;
			_abilityPreviewRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rectTransform.rect.height);
			_abilityPreviewRT.position = rectTransform.position;
		}
	}
}
