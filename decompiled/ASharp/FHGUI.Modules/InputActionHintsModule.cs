using System.Collections.Generic;
using FHGUI.Elements;
using UnityEngine;

namespace FHGUI.Modules;

public class InputActionHintsModule : FHMapViewModule
{
	public struct Params
	{
		public List<ButtonsBarParam> ButtonsParams;
	}

	[Header("Components")]
	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private ButtonsBarElement m_Prototype;

	private List<ButtonsBarElement> m_Buttons = new List<ButtonsBarElement>(5);

	private bool m_Initialzied;

	protected override bool PlaySoundOnEnable => false;

	private void InitCache()
	{
		if (!m_Initialzied)
		{
			m_Initialzied = true;
			m_Container.GetComponentsInChildren(m_Buttons);
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		UIHelperUtil.UpdateUIList(@params.ButtonsParams?.Count ?? 0, m_Buttons, m_Prototype, m_Container, setElement);
		void setElement(int index, ButtonsBarElement b)
		{
			m_Buttons[index].SetParam(@params.ButtonsParams[index]);
		}
	}
}
