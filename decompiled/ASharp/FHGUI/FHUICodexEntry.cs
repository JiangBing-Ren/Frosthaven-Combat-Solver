using FHGUI.DTO.Outpost;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI;

public class FHUICodexEntry : MonoBehaviour
{
	public struct TopicData
	{
		public string CaptionKey;

		public string[] DescriptionKeys;

		public Sprite Illustration;
	}

	[Header("Components")]
	[SerializeField]
	protected TextLocalizedListener m_Caption;

	[SerializeField]
	protected Image m_Illustration;

	[SerializeField]
	protected TextLocalizedListener m_Description;

	public CodexConfig.CodexEntry Data { get; protected set; }

	public void Populate(TopicData data)
	{
		if ((bool)m_Illustration)
		{
			m_Illustration.sprite = data.Illustration;
		}
		if ((bool)m_Caption)
		{
			m_Caption.SetTextKey(data.CaptionKey);
		}
		if (!m_Description)
		{
			return;
		}
		m_Description.SetTextKey(data.DescriptionKeys[0]);
		if (data.DescriptionKeys.Length > 1)
		{
			for (int i = 1; i < data.DescriptionKeys.Length; i++)
			{
				Object.Instantiate(m_Description.gameObject, m_Description.transform.parent).GetComponent<TextLocalizedListener>().SetTextKey(data.DescriptionKeys[i]);
			}
		}
	}

	public virtual void Setup(CodexConfig.CodexEntry entry)
	{
		Data = entry;
	}
}
