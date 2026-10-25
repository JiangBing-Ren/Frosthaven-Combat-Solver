using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using I2.Loc;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
[ExecuteAlways]
public class TextLocalizedListener : LocalizedListener
{
	public const string KEY_PARAM_NAME = "KEY";

	protected TextMeshProUGUI text;

	[SerializeField]
	private string key;

	[SerializeField]
	private List<LocalizationParam> parameters = new List<LocalizationParam>();

	[TextArea]
	[Tooltip("Format how the localized text is displayed.\nUse {KEY} to insert the localized text.\nUse {YOUR_PARAM_NAME} to insert the values of the named parameters.")]
	[SerializeField]
	private string format;

	private bool invalidate;

	public string this[string paramName]
	{
		get
		{
			return parameters.FirstOrDefault((LocalizationParam p) => p.Name.Equals(paramName, StringComparison.OrdinalIgnoreCase)).Value ?? "";
		}
		set
		{
			SetParam(paramName, value);
		}
	}

	public TextMeshProUGUI Text
	{
		get
		{
			Initialize();
			return text;
		}
	}

	private void Initialize()
	{
		if (text == null)
		{
			text = GetComponent<TextMeshProUGUI>();
		}
	}

	protected void Awake()
	{
		Initialize();
		I2.Loc.LocalizationManager.OnLocalizeEvent += HandleOnLanguageChanged;
		OnLanguageChanged();
	}

	private void OnDestroy()
	{
		I2.Loc.LocalizationManager.OnLocalizeEvent -= HandleOnLanguageChanged;
	}

	protected override void OnEnable()
	{
		if (invalidate)
		{
			invalidate = false;
			OnLanguageChanged();
		}
	}

	protected override void OnDisable()
	{
	}

	private void HandleOnLanguageChanged()
	{
		if (gameObject.activeInHierarchy)
		{
			OnLanguageChanged();
		}
		else
		{
			invalidate = true;
		}
	}

	protected override void OnLanguageChanged()
	{
		Initialize();
		if (key.IsNullOrEmpty() || this.text == null)
		{
			return;
		}
		string text = GLOOM.LocalizationManager.GetTranslation(key);
		if (format.IsNOTNullOrEmpty())
		{
			text = new LocalizationParam("KEY", text).Apply(format);
		}
		foreach (LocalizationParam parameter in parameters)
		{
			text = parameter.Apply(text);
		}
		this.text.text = text.Replace("\\n", "\n");
	}

	public void SetTextKey(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			this.key = "MISSING_KEY";
		}
		if (!(this.key == key))
		{
			this.key = key;
			OnLanguageChanged();
		}
	}

	public void ClearText()
	{
		key = string.Empty;
		if ((bool)text)
		{
			text.text = string.Empty;
		}
	}

	public void SetTextKey(string key, params LocalizationParam[] parameters)
	{
		this.key = key;
		this.parameters = parameters.ToList();
		OnLanguageChanged();
	}

	public void SetParameters(Dictionary<string, string> parameters)
	{
		this.parameters = parameters.Select((KeyValuePair<string, string> p) => new LocalizationParam(p)).ToList();
		OnLanguageChanged();
	}

	public void SetParameters(params LocalizationParam[] parameters)
	{
		this.parameters = parameters.ToList();
		OnLanguageChanged();
	}

	public void SetParametersFormatted(params string[] parameters)
	{
		this.parameters = parameters.Select((string s, int i) => new LocalizationParam($"{{{i}}}", s)).ToList();
		OnLanguageChanged();
	}

	public void SetParam(string paramName, uint paramValue)
	{
		SetParam(paramName, paramValue.ToString());
	}

	public void SetParam(string paramName, int paramValue)
	{
		SetParam(paramName, paramValue.ToString());
	}

	public void SetParam(string paramName, string paramValue)
	{
		for (int i = 0; i < parameters.Count; i++)
		{
			LocalizationParam value = parameters[i];
			if (value.EqualName(paramName))
			{
				value.Value = paramValue;
				parameters[i] = value;
				OnLanguageChanged();
				return;
			}
		}
		parameters.Add(new LocalizationParam(paramName, paramValue));
		OnLanguageChanged();
	}

	public void SetFormat(string format)
	{
		this.format = format;
		OnLanguageChanged();
	}

	private void OnValidate()
	{
		if (!string.IsNullOrWhiteSpace(format) && !format.Contains("{KEY}", StringComparison.OrdinalIgnoreCase))
		{
			if (format.Contains("{0}"))
			{
				format = format.Replace("{0}", "{KEY}");
			}
			else
			{
				Debug.LogError(name + " localization has formatting text without specified {KEY} parameter.", this);
			}
		}
	}
}
