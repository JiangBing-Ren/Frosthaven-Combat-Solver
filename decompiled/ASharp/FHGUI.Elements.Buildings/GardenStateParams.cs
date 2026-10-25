using System;
using FHRL;

namespace FHGUI.Elements.Buildings;

public class GardenStateParams : IBuildingInfoParams
{
	public record GardenPlotData
	{
		public OutpostHerbType Herb;

		public bool Unlocked;

		public uint UnlockLevel;
	}

	public GardenPlotData[] Plots;

	public GardenState.PlantStateType PlantState;

	public OutpostResources Resources;

	public bool IsPartyLeader;

	public Action<OutpostHerbType[]> Plant;
}
