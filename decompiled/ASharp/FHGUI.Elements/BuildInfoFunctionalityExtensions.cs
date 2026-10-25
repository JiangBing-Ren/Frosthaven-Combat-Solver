using System;
using FHGUI.DTO;
using FHGUI.Elements.Buildings;

namespace FHGUI.Elements;

public static class BuildInfoFunctionalityExtensions
{
	public static bool Init(this IBuildInfoFunctionality info, BuildingStateDTO building, IBuildingInfoParams data)
	{
		if (data == null)
		{
			return false;
		}
		try
		{
			if (!(info is Library_UIBuildingInfoFunctionality library_UIBuildingInfoFunctionality))
			{
				if (!(info is Garden_UIBuildInfoFunctionality garden_UIBuildInfoFunctionality))
				{
					if (!(info is SingleAction_UIBuildingInfoFunctionality singleAction_UIBuildingInfoFunctionality))
					{
						if (!(info is Stables_UIBuildInfoFunctionality stables_UIBuildInfoFunctionality))
						{
							if (!(info is Barracks_UIBuildInfoFunctionality barracks_UIBuildInfoFunctionality))
							{
								if (!(info is ResourceBuilding_UIBuildInfoFunctionality resourceBuilding_UIBuildInfoFunctionality))
								{
									if (!(info is Workshop_UIBuildInfoFunctionality workshop_UIBuildInfoFunctionality))
									{
										if (!(info is WorkshopUpgrade_UIBuildingFunctionality workshopUpgrade_UIBuildingFunctionality))
										{
											if (!(info is Walls_UIBuildInfoFunctionality walls_UIBuildInfoFunctionality))
											{
												if (!(info is Tavern_UIBuildInfoFunctionality tavern_UIBuildInfoFunctionality))
												{
													if (!(info is Inn_UIBuildInfoFunctionality inn_UIBuildInfoFunctionality))
													{
														if (!(info is Jeweller_UIBuildInfoFunctionality jeweller_UIBuildInfoFunctionality))
														{
															if (!(info is HallOfRevelry_UIBuildInfoFunctionality hallOfRevelry_UIBuildInfoFunctionality))
															{
																if (!(info is Info_UIBuildInfoFunctionality info_UIBuildInfoFunctionality))
																{
																	if (!(info is TownHall_UIBuildInfoFunctionality townHall_UIBuildInfoFunctionality))
																	{
																		throw new ArgumentOutOfRangeException("info");
																	}
																	townHall_UIBuildInfoFunctionality.Init(building, (TownHall_UIBuildInfoFunctionality.TownHallStateParams)(object)data);
																}
																else
																{
																	info_UIBuildInfoFunctionality.Init(building, (Info_UIBuildInfoFunctionality.InfoParams)(object)data);
																}
															}
															else
															{
																hallOfRevelry_UIBuildInfoFunctionality.Init(building, (HallOfRevelry_UIBuildInfoFunctionality.HallOfRevelryStateParams)(object)data);
															}
														}
														else
														{
															jeweller_UIBuildInfoFunctionality.Init(building, (Jeweller_UIBuildInfoFunctionality.JewellerStateParams)(object)data);
														}
													}
													else
													{
														inn_UIBuildInfoFunctionality.Init(building, (Inn_UIBuildInfoFunctionality.InnStateParams)(object)data);
													}
												}
												else
												{
													tavern_UIBuildInfoFunctionality.Init(building, (Tavern_UIBuildInfoFunctionality.TavernStateParams)(object)data);
												}
											}
											else
											{
												walls_UIBuildInfoFunctionality.Init(building, (Walls_UIBuildInfoFunctionality.WallsData)(object)data);
											}
										}
										else
										{
											workshopUpgrade_UIBuildingFunctionality.Init(building, (WorkshopUpgradeBuildingParams)(object)data);
										}
									}
									else
									{
										workshop_UIBuildInfoFunctionality.Init(building, (WorkshopBuildingParams)(object)data);
									}
								}
								else
								{
									resourceBuilding_UIBuildInfoFunctionality.Init(building, (ResourceBuildingData)(object)data);
								}
							}
							else
							{
								barracks_UIBuildInfoFunctionality.Init(building, (BarracksStateParams)(object)data);
							}
						}
						else
						{
							stables_UIBuildInfoFunctionality.Init(building, (Stables_UIBuildInfoFunctionality.StablesStateParams)(object)data);
						}
					}
					else
					{
						singleAction_UIBuildingInfoFunctionality.Init(building, (SingleActionBuildingParams)(object)data);
					}
				}
				else
				{
					garden_UIBuildInfoFunctionality.Init(building, (GardenStateParams)data);
				}
			}
			else
			{
				library_UIBuildingInfoFunctionality.Init(building, (Library_UIBuildingInfoFunctionality.LibraryStateParams)(object)data);
			}
		}
		catch (Exception ex)
		{
			Log.GUIError(string.Format("Setting build info params failed with exception {0}: {1} given for building {2} - given {3}", ex.Message, "IBuildingInfoParams", building.Def.Name, data.GetType()));
			return false;
		}
		return true;
	}
}
