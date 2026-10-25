using System;

namespace Gloomhaven;

[Serializable]
public class ProductGroup
{
	public string Name;

	public Model[] Models = Array.Empty<Model>();

	public const string DEFAULT_PRODUCT_GROUP = "<other>";

	public bool MatchProductGroup(string product)
	{
		if (product.Contains(Name))
		{
			return true;
		}
		return Name == "<other>";
	}
}
