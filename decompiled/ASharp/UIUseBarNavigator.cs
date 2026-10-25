using System.Collections.Generic;
using System.Linq;
using DevLocker.GFrame.Input.UIScope;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIUseBarNavigator : MonoBehaviour
{
	[SerializeField]
	private SelectionControllerScopeElement _selectionController;

	private IReadOnlyList<Selectable> GetSelectableElementsList()
	{
		return _selectionController.StartSelections.ToList();
	}

	private void Start()
	{
		OnBarChanged();
	}

	public void OnBarChanged()
	{
		_selectionController.StartSelections = (from slot in GetComponentsInChildren<IUIUseSlot>(includeInactive: true)
			select slot.GetSelectable() into selectable
			where selectable != null && selectable.gameObject.activeSelf
			select selectable).ToList();
		gameObject.SetActive(_selectionController.StartSelections.Count > 0);
	}

	public void SelectNext()
	{
		CycleUsableSelection(next: true);
	}

	public void SelectPrev()
	{
		CycleUsableSelection(next: false);
	}

	private void CycleUsableSelection(bool next)
	{
		IReadOnlyList<Selectable> selectableElementsList = GetSelectableElementsList();
		if (selectableElementsList.Count != 0)
		{
			int num = selectableElementsList.FindIndex((Selectable it) => it.gameObject == EventSystem.current.currentSelectedGameObject);
			int num2 = (next ? ((num + 1) % selectableElementsList.Count) : (num - 1));
			if (num2 < 0)
			{
				num2 += selectableElementsList.Count;
			}
			selectableElementsList[num2].Select();
		}
	}
}
