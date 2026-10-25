using UnityEngine;
using UnityEngine.UI;

namespace ReportIssueGui;

[ExecuteAlways]
public class InputFieldBar : MonoBehaviour
{
	public RectTransform BarBackground;

	public RectTransform BarHandle;

	public InputField InputField;

	private void Update()
	{
		if (!(BarBackground == null) && !(BarHandle == null) && !(InputField == null))
		{
			float num = (float)InputField.caretPosition / (float)InputField.text.Length;
			BarHandle.anchoredPosition = new Vector2(BarHandle.anchoredPosition.x, (0f - num) * BarBackground.rect.height);
		}
	}
}
