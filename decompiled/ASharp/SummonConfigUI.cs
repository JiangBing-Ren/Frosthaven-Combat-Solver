using FHGUI.Configs;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "UI Config/Summon", fileName = "Summon Config")]
public class SummonConfigUI : ScriptableObject
{
	public string ID;

	[ConfigIcon(128, true)]
	public Sprite Portrait;

	[ConfigIcon(128, true)]
	public Sprite Illusration;

	[FormerlySerializedAs("Summoner")]
	public CharacterConfigUI SummonerConfig;

	public string GetSearchFilterFromId()
	{
		return ID.Remove(ID.Length - 2).SplitCamelCase();
	}
}
