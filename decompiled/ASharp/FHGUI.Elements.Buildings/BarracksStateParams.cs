using System;
using FHRL;

namespace FHGUI.Elements.Buildings;

public struct BarracksStateParams : IBuildingInfoParams
{
	public bool CanTrainSoldier;

	public uint AvailableSoldersToTrain;

	public uint SoldierCount;

	public uint MaxSoldierCount;

	public bool IsPartyLeader;

	public uint MaterialCost;

	public OutpostResources CurrentResources;

	public uint CurrentGold;

	public uint GoldCost;

	public uint SoldierPerBuy;

	public OutpostMaterialType PickedMaterialType;

	public int CurrentDefense;

	public Action<OutpostMaterialType> OnPickMaterialType;

	public BuildingFunctionalityAction TrainAction;

	public string Hint;
}
