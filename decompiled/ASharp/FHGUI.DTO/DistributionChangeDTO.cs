using System;
using System.Collections.Generic;
using FHRL;

namespace FHGUI.DTO;

[Serializable]
public class DistributionChangeDTO
{
	public int Gold;

	public OutpostResourceChange Resources;

	public List<ItemDef> Items;

	public DistributionChangeType ChangeType;

	public int LoseItem;

	public int LosePet;

	public ItemFilterFlags ItemFilter;
}
