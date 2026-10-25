using System;
using System.Collections.Generic;
using FHGUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UIGeneralInfoNavigator : ListContainerBase<Image, UIGeneralInfoModule.GeneralInfoListDTO, IGeneralInfoData>
{
	[SerializeField]
	private Button m_ButtonPrevious;

	[SerializeField]
	private Button m_ButtonNext;

	[Space]
	[SerializeField]
	private Sprite m_SpriteActive;

	[SerializeField]
	private Sprite m_SpriteInactive;

	private Action _onPrev;

	private Action _onNext;

	private void Awake()
	{
		m_ButtonPrevious.onClick.AddListener(() =>
		{
			_onPrev?.Invoke();
		});
		m_ButtonNext.onClick.AddListener(() =>
		{
			_onNext?.Invoke();
		});
	}

	protected override void InitElement(Image listElement, IGeneralInfoData elementParams, UIGeneralInfoModule.GeneralInfoListDTO listParams)
	{
		Sprite newSprite = ((elementParams == listParams.CurrentData) ? m_SpriteActive : m_SpriteInactive);
		UIHelperUtil.SetSpriteAndTweenIt(listElement, newSprite, newEnabledState: true);
	}

	protected override void InitList(UIGeneralInfoModule.GeneralInfoListDTO listParams, IList<IGeneralInfoData> listElementParams)
	{
		base.InitList(listParams, listElementParams);
		bool flag = listElementParams.Count > 1;
		m_ButtonPrevious.gameObject.SetActive(flag);
		m_ButtonNext.gameObject.SetActive(flag);
		int num = listElementParams.IndexOf(listParams.CurrentData);
		m_ButtonNext.interactable = flag && num < listElementParams.Count - 1;
		m_ButtonPrevious.interactable = flag && num > 0;
		_onPrev = listParams.OnPrev;
		_onNext = listParams.OnNext;
	}
}
