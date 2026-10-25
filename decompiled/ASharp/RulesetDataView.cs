using System.Collections.Generic;
using System.Linq;
using Assets.Script.GUI.MainMenu.Modding;

public class RulesetDataView
{
	private string originalName;

	public List<IMod> originalMods;

	public string Name { get; set; }

	public List<IMod> Mods { get; set; }

	public HashSet<IMod> UnvalidMods { get; set; } = new HashSet<IMod>();

	public bool IsModified
	{
		get
		{
			if (!(Name != originalName) && !Mods.Except(originalMods).Any())
			{
				return originalMods.Except(Mods).Any();
			}
			return true;
		}
	}

	public bool IsValid()
	{
		return true;
	}

	public void SetUnvalidMods(List<IMod> mods)
	{
		UnvalidMods.Clear();
		UnvalidMods.UnionWith(mods);
	}
}
