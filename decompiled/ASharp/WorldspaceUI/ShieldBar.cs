using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WorldspaceUI;

public class ShieldBar : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> m_Shields;

	[SerializeField]
	private Sprite m_ShieldSprite;

	[SerializeField]
	private Sprite m_ShieldPiercedSprite;

	private List<Image> m_ShieldImages;

	private int m_CurrentActive;

	private bool m_IgnoreShields;

	private void Awake()
	{
		m_ShieldImages = new List<Image>();
		foreach (GameObject shield in m_Shields)
		{
			Image componentInChildren = shield.GetComponentInChildren<Image>(includeInactive: true);
			componentInChildren.sprite = m_ShieldSprite;
			m_ShieldImages.Add(componentInChildren);
		}
	}

	public void UpdateShields(int activeShields, bool ignore = false)
	{
		if (m_CurrentActive == activeShields && m_IgnoreShields == ignore)
		{
			return;
		}
		m_CurrentActive = activeShields;
		m_IgnoreShields = ignore;
		for (int i = 0; i < m_Shields.Count; i++)
		{
			if (i < m_CurrentActive)
			{
				m_ShieldImages[i].sprite = (ignore ? m_ShieldPiercedSprite : m_ShieldSprite);
				m_Shields[i].SetActive(value: true);
			}
			else
			{
				m_Shields[i].SetActive(value: false);
			}
		}
	}

	public void SetPierce(int pierce)
	{
		m_IgnoreShields = false;
		int num = ((m_CurrentActive <= m_ShieldImages.Count) ? m_CurrentActive : m_ShieldImages.Count);
		int num2 = ((pierce < 99999) ? Mathf.Max(0, num - pierce) : 0);
		for (int num3 = num; num3 > num2; num3--)
		{
			m_ShieldImages[num3 - 1].sprite = m_ShieldPiercedSprite;
		}
		for (int num4 = num2 - 1; num4 >= 0; num4--)
		{
			m_ShieldImages[num4].sprite = m_ShieldSprite;
		}
	}

	public void ResetShieldIcons()
	{
		m_IgnoreShields = false;
		foreach (Image shieldImage in m_ShieldImages)
		{
			shieldImage.sprite = m_ShieldSprite;
		}
	}
}
