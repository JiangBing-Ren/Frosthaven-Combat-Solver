using System.Collections.Generic;
using UnityEngine;

namespace Gloomhaven;

[CreateAssetMenu(menuName = "Defs/Platforms/HardwareIdentification")]
public class HardwareIdentification : ScriptableObject
{
	public Vendor[] CPU;

	public Vendor[] GPU;

	public void IdentifyHardware(string cpuName, string gpuName, out Model identifiedCPU, out Model identifiedGPU)
	{
		identifiedCPU = IdentifyDevice(CPU, cpuName);
		identifiedGPU = IdentifyDevice(GPU, gpuName);
		static Model IdentifyDevice(IEnumerable<Vendor> vendors, string deviceName)
		{
			if (string.IsNullOrEmpty(deviceName))
			{
				return null;
			}
			string product = deviceName.RemoveChar('-');
			string product2 = deviceName.Replace("-", " ");
			string product3 = deviceName.RemoveSpaces();
			string product4 = deviceName.Replace(" ", "-");
			foreach (Vendor vendor in vendors)
			{
				if (vendor.MatchVendor(deviceName))
				{
					ProductGroup[] productGroups = vendor.ProductGroups;
					foreach (ProductGroup productGroup in productGroups)
					{
						if (productGroup.MatchProductGroup(deviceName))
						{
							Model[] models = productGroup.Models;
							foreach (Model model in models)
							{
								if (model.MatchModel(deviceName) || model.MatchModel(product) || model.MatchModel(product2) || model.MatchModel(product3) || model.MatchModel(product4))
								{
									return model;
								}
							}
						}
					}
				}
			}
			return null;
		}
	}

	internal static HardwareIdentification LoadResource()
	{
		return Resources.Load<HardwareIdentification>("Settings/KronosHardwareIdentification");
	}
}
