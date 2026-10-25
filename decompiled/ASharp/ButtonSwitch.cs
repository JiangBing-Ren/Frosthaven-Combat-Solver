using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonSwitch : MonoBehaviour
{
	[SerializeField]
	private ExtendedToggle toggle;

	[SerializeField]
	private Image image;

	[SerializeField]
	private TextLocalizedListener text;

	[Header("On")]
	[SerializeField]
	private Sprite onSprite;

	[SerializeField]
	private string onText;

	[Header("Off")]
	[SerializeField]
	private Sprite offSprite;

	[SerializeField]
	private string offText;

	public UnityEvent OnSelected => toggle.onSelected;

	public bool IsOn
	{
		get
		{
			return toggle.isOn;
		}
		set
		{
			toggle.isOn = value;
		}
	}

	public Toggle.ToggleEvent OnValueChanged => toggle.onValueChanged;

	private void Awake()
	{
		toggle.onValueChanged.AddListener(Refresh);
		Refresh(toggle.isOn);
	}

	public void Refresh(bool on)
	{
		if (image != null)
		{
			image.sprite = (on ? onSprite : offSprite);
		}
		if (text != null)
		{
			text.SetTextKey(on ? onText : offText);
		}
	}

	public void SetValue(bool value)
	{
		toggle.SetValue(value);
		Refresh(value);
	}
}
