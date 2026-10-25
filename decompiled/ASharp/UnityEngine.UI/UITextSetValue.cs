namespace UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UITextSetValue : MonoBehaviour
{
	public string floatFormat = "0.00";

	private Text m_Text;

	public Text MyText
	{
		get
		{
			if (m_Text == null)
			{
				m_Text = GetComponent<Text>();
			}
			return m_Text;
		}
	}

	public void SetFloat(float value)
	{
		MyText.text = value.ToString(floatFormat);
	}
}
