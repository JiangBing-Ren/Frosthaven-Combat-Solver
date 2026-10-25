using System;
using FHGUI.Common;
using FHGUI.Configs;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Monster", fileName = "Monster Config")]
public class MonsterConfigUI : ScriptableObject
{
	[Serializable]
	public struct ModelVariant
	{
		public string ModelName;

		public MonsterConfigUI ConfigUI;
	}

	[ReadOnlyField]
	public string ID;

	public ModelVariant[] Variants;

	[ConfigIcon(128, true)]
	public Sprite Portrait;

	[ConfigIcon(128, true)]
	public Sprite Illustration;

	public UIImageSettings CardHeaderSettings = UIImageSettings.Default;
}
