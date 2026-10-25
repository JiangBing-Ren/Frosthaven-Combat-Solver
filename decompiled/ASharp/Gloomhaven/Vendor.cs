using System;
using System.Linq;

namespace Gloomhaven;

[Serializable]
public class Vendor
{
	public string Name;

	public string[] AlternativeNames = Array.Empty<string>();

	public ProductGroup[] ProductGroups = Array.Empty<ProductGroup>();

	public bool MatchVendor(string product)
	{
		if (!product.Contains(Name))
		{
			return AlternativeNames.Any(product.Contains);
		}
		return true;
	}
}
