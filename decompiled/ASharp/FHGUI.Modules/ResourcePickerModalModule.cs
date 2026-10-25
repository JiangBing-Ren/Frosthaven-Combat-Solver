using System;
using FHGUI.Common.Elements;
using UnityEngine;

namespace FHGUI.Modules;

public class ResourcePickerModalModule : FHMapViewModule
{
	[Header("Components")]
	[SerializeField]
	private UIPicker m_Picker;

	private Action OnCancel;

	public void Init(UIPicker.UIPickerData data, Predicate<UIPicker.UIPickerResult> onResult, Action onCancel)
	{
		OnCancel = onCancel;
		m_Picker.Init(data, onResult);
		m_Picker.OnHide = Cancel;
		if (data.targetPoint != null)
		{
			UIHelperUtil.TrySetPositionConstraint(m_Picker.gameObject, data.targetPoint, 1f, createConstraintIfMissing: true);
		}
		else
		{
			m_Picker.gameObject.transform.localPosition = Vector3.zero;
		}
	}

	public void Cancel()
	{
		OnCancel?.Invoke();
	}
}
