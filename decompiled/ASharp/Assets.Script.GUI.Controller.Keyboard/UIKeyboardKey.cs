using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Script.GUI.Controller.Keyboard;

public class UIKeyboardKey : MonoBehaviour
{
	[Serializable]
	public class KeyCodeEvent : UnityEvent<KeyCode>
	{
	}

	[Serializable]
	private class HighlightConfig
	{
		[SerializeField]
		private Graphic graphic;

		[SerializeField]
		private Color defaultColor;

		[SerializeField]
		private Color highlightColor;

		public void Reset()
		{
			graphic.color = defaultColor;
		}

		public void Highlight()
		{
			graphic.color = highlightColor;
		}
	}

	[SerializeField]
	private KeyCode keyCode;

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private HighlightConfig[] highlights;

	[SerializeField]
	private TextMeshProUGUI text;

	public KeyCodeEvent OnClicked;

	private bool isHighlighted;

	private void Awake()
	{
		button.onMouseEnter.AddListener(Highlight);
		button.onMouseExit.AddListener(Unhighlight);
		button.onClick.AddListener(() =>
		{
			OnClicked.Invoke(keyCode);
		});
		if (keyCode != KeyCode.None)
		{
			SetKeyCode(keyCode);
		}
	}

	private void Highlight()
	{
		isHighlighted = true;
		if (text != null)
		{
			text.color = Color.black;
		}
		for (int i = 0; i < highlights.Length; i++)
		{
			highlights[i].Highlight();
		}
	}

	private void Unhighlight()
	{
		isHighlighted = false;
		if (text != null)
		{
			text.color = UIInfoTools.DefInstance.basicTextColor;
		}
		for (int i = 0; i < highlights.Length; i++)
		{
			highlights[i].Reset();
		}
	}

	public void SetKeyCode(KeyCode keyCode)
	{
		this.keyCode = keyCode;
		if (text != null)
		{
			text.text = KeyCodeConverter.ConvertToValue(keyCode);
		}
		Unhighlight();
	}

	private void OnDisable()
	{
		if (isHighlighted)
		{
			Unhighlight();
		}
	}
}
