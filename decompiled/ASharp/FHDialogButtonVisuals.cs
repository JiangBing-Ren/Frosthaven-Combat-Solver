using System;
using Libs.Utils.Maths;
using UnityEngine;
using UnityEngine.UI;

public class FHDialogButtonVisuals : MonoBehaviour
{
	[Serializable]
	public struct VisualParams
	{
		public Vector2 SizeDelta;

		public Sprite SpriteNormal;

		public SpriteState SpriteState;

		public bool IsFlipped;

		public Sprite Icon;

		public Vector2 IconOffset;
	}

	[SerializeField]
	private FHButton m_Button;

	[SerializeField]
	private Image m_ImageIcon;

	public VisualParams DefaultVisuals;

	public void InitVisuals(VisualParams visualParams)
	{
		((RectTransform)transform).sizeDelta = visualParams.SizeDelta;
		m_Button.spriteState = visualParams.SpriteState;
		m_Button.image.sprite = visualParams.SpriteNormal;
		m_Button.image.rectTransform.rotation = Quaternion.Euler(0f, visualParams.IsFlipped ? 180f : 0f, 0f);
		bool flag = visualParams.Icon != null;
		if (m_ImageIcon != null)
		{
			m_ImageIcon.sprite = visualParams.Icon;
			m_ImageIcon.enabled = flag;
			m_ImageIcon.rectTransform.anchoredPosition = visualParams.IconOffset;
			m_ImageIcon.raycastTarget = Utl.Equals(visualParams.IconOffset, Vector2.zero);
		}
		m_Button.Text.gameObject.SetActive(!flag);
	}

	[ContextMenu("Use DefaultVisuals")]
	public void ResetVisuals()
	{
		InitVisuals(DefaultVisuals);
	}

	[ContextMenu("Set DefaultVisuals from current object state")]
	private void SetDefaultVisuals()
	{
		DefaultVisuals = new VisualParams
		{
			SizeDelta = ((RectTransform)transform).sizeDelta,
			SpriteNormal = m_Button.image.sprite,
			SpriteState = m_Button.spriteState,
			IsFlipped = false,
			Icon = m_ImageIcon.sprite,
			IconOffset = Vector2.zero
		};
	}
}
