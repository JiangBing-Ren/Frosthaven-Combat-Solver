using System.Collections.Generic;
using FHGUI.Configs;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/OutpostResourceConfigUI", fileName = "OutpostResourceConfig")]
public class OutpostResourceConfigUI : ScriptableObject
{
	[PingInHierarchy]
	public string Id;

	[ConfigIcon(48, true)]
	public Sprite Icon;

	[ConfigIcon(48, true)]
	public Sprite IconStylized;

	[ConfigIcon(48)]
	public Sprite IconTintable;

	[ConfigIcon(48)]
	public Sprite BigIcon;

	[ConfigIcon(48)]
	public Sprite CategorySelected;

	[ConfigIcon(48)]
	public Sprite CategoryNormal;

	public IEnumerable<string> Validate()
	{
		if (string.IsNullOrEmpty(Id))
		{
			yield return "Invalid Id";
		}
		if (IconStylized == null || IconStylized.texture == null)
		{
			yield return "Stylized Icon is mandatory";
		}
	}
}
