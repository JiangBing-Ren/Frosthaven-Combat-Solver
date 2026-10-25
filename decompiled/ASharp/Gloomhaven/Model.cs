using System;

namespace Gloomhaven;

[Serializable]
public class Model
{
	public string Name;

	public float Score;

	public bool MatchModel(string product)
	{
		return product.Contains(Name);
	}
}
