using System;

namespace FHGUI.DTO;

[Serializable]
public class ResourceMod
{
	public string Name;

	public int Value;

	public int Mod;

	public int MaxMod;

	public Action<int> Change;

	public int Price = -1;
}
