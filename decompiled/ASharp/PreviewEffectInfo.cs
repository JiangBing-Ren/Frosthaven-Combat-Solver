using System;
using UnityEngine;

[Serializable]
public class PreviewEffectInfo
{
	public Sprite previewEffectIcon;

	[TextArea]
	public string previewEffectText;

	public bool IsNotEmpty()
	{
		if (!(previewEffectIcon != null))
		{
			return previewEffectText.IsNOTNullOrEmpty();
		}
		return true;
	}
}
