using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ControllerInputPointer : Singleton<ControllerInputPointer>
{
	[Serializable]
	private class CursorIcon
	{
		public ECursorType type;

		public Sprite icon;
	}

	[SerializeField]
	private Image m_Pointer;

	[SerializeField]
	private List<CursorIcon> m_Cursors;

	private static ECursorType m_CursorType;

	public static ECursorType CursorType
	{
		get
		{
			return m_CursorType;
		}
		set
		{
			if (m_CursorType != value)
			{
				m_CursorType = value;
				if (Singleton<InputFHManager>.Instance.UsingGamepad)
				{
					SetCursorType(value);
				}
			}
		}
	}

	protected override void AwakeInit()
	{
		m_Pointer.enabled = false;
		Singleton<InputFHManager>.Instance.InputContext.LastUsedInputControlSchemeChanged += OnLastUsedInputControlSchemeChanged;
		OnLastUsedInputControlSchemeChanged();
		Hide();
	}

	protected override void AwakenedDestroy()
	{
		if ((bool)Singleton<InputFHManager>.InstanceFast)
		{
			Singleton<InputFHManager>.InstanceFast.InputContext.LastUsedInputControlSchemeChanged -= OnLastUsedInputControlSchemeChanged;
		}
	}

	private void OnLastUsedInputControlSchemeChanged()
	{
		if (Singleton<InputFHManager>.InstanceFast.UsingGamepad)
		{
			m_Pointer.enabled = true;
			SetCursorType(CursorType);
		}
		else
		{
			m_Pointer.enabled = false;
		}
	}

	public static void SetCursorType(ECursorType type)
	{
		if (!(Singleton<ControllerInputPointer>.Instance == null) && Singleton<ControllerInputPointer>.Instance.gameObject.activeSelf)
		{
			Singleton<ControllerInputPointer>.Instance.m_Pointer.sprite = Singleton<ControllerInputPointer>.Instance.m_Cursors.First((CursorIcon it) => type == it.type).icon;
		}
	}

	public void Show()
	{
		if (!Singleton<ControllerInputPointer>.Instance.gameObject.activeSelf)
		{
			Singleton<ControllerInputPointer>.Instance.gameObject.SetActive(value: true);
			SetCursorType(CursorType);
		}
	}

	public void Hide()
	{
		if (!(Singleton<ControllerInputPointer>.Instance == null) && Singleton<ControllerInputPointer>.Instance.gameObject.activeSelf)
		{
			Singleton<ControllerInputPointer>.Instance.gameObject.SetActive(value: false);
		}
	}
}
