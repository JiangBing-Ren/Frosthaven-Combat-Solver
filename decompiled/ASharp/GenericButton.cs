using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GenericButton : MonoBehaviour
{
	[SerializeField]
	private Button m_Button;

	[SerializeField]
	private TextMeshProUGUI m_Text;

	public bool IsActive => gameObject.activeSelf;

	private event Action m_OnClick;

	private void Awake()
	{
		SetActive(active: false);
	}

	public void SetActive(bool active)
	{
		gameObject.SetActive(active);
	}

	public void SetInteractable(bool interactable)
	{
		m_Button.interactable = interactable;
	}

	public void SetText(string text)
	{
		m_Text.text = text;
	}

	public void OnClick()
	{
		if (m_OnClick != null)
		{
			m_OnClick();
		}
	}

	public void AddOnClickListener(Action action)
	{
		m_OnClick += action;
	}

	public void RemoveOnClickListener(Action action)
	{
		m_OnClick -= action;
	}

	public void RemoveAllOnClickListeners()
	{
		m_OnClick = null;
	}
}
