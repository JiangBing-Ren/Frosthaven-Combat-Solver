using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ModalWindow : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_TextArea;

	[SerializeField]
	private ExtendedButton m_OkButton;

	[SerializeField]
	private ExtendedButton m_CancelButton;

	public void Init(string title, string message, UnityAction okCallback, UnityAction cancelCallback = null)
	{
		m_Title.text = title;
		m_TextArea.text = message;
		if (okCallback != null)
		{
			m_OkButton.onClick.AddListener(okCallback);
		}
		if (cancelCallback != null)
		{
			m_CancelButton.onClick.AddListener(cancelCallback);
			m_CancelButton.gameObject.SetActive(value: true);
		}
		else
		{
			m_CancelButton.gameObject.SetActive(value: false);
		}
		gameObject.SetActive(value: true);
	}
}
