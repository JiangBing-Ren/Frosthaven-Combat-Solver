using System;
using System.Collections.Generic;
using FHRL;

namespace FHGUI.DTO;

[Serializable]
public class DistributionsDTO
{
	public List<ResourceDistribution> Resources;

	public List<GoldDistribution> Gold;

	public List<CharacterItemDistribution> Items;

	public BuildingTypeDistribution BuildingType;

	public BuildingMaterialDistribution BuildingMaterial;

	public PetDistribution PetDistribution;
}
