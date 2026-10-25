using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextMeshProHiperlinksHandler : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	private TextMeshProUGUI TextMeshPro;

	private void Awake()
	{
		TextMeshPro = GetComponent<TextMeshProUGUI>();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		int num = TMP_TextUtilities.FindIntersectingLink(TextMeshPro, InputFHManager.CursorPosition, UIManager.Instance.UICamera);
		if (num != -1)
		{
			TMP_LinkInfo tMP_LinkInfo = TextMeshPro.textInfo.linkInfo[num];
			Application.OpenURL(tMP_LinkInfo.GetLinkID());
		}
	}
}
