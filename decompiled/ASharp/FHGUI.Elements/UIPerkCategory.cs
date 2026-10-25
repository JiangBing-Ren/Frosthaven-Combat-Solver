using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIPerkCategory : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private UIPerkElement m_IconPrototype;

	[SerializeField]
	private TMP_Text m_CategoryLabel;

	[SerializeField]
	private Image m_CategoryIcon;

	[SerializeField]
	private GameObject m_CategoryHeader;

	private List<UIPerkElement> m_Icons = new List<UIPerkElement>();

	private bool m_Initialzied;

	private void InitCache()
	{
		if (!m_Initialzied)
		{
			m_Initialzied = true;
			m_Container.GetComponentsInChildren(includeInactive: true, m_Icons);
		}
	}

	public void Init(List<PerkDTO> perks, UIPerkInteractionEvents perkInteractions, string categoryCaptionKey, bool showHeader = false)
	{
		InitCache();
		if ((bool)m_CategoryLabel)
		{
			m_CategoryLabel.text = LocalizationManager.GetTranslation(categoryCaptionKey);
		}
		if ((bool)m_CategoryHeader)
		{
			m_CategoryHeader.gameObject.SetActive(showHeader);
		}
		UIHelperUtil.UpdateUIList(perks.Count, m_Icons, m_IconPrototype, m_Container, onShow);
		void onShow(int index, UIPerkElement item)
		{
			item.Init(perks[index], perkInteractions);
		}
	}
}
