using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using GLOOM;
using ScenarioRuleLibrary;

namespace FHGUI.Scenario.Services;

public class HexViewService : FHScenarioViewService
{
	public struct HexInfoParams
	{
		public CActor ActorOnTileWithIniative;

		public CActor ActorOnTile;

		public CSpawner Spawner;

		public List<CObjectProp> Props;

		public List<PropInfoParams> PropInfos;

		public bool IsEmpty
		{
			get
			{
				if (ActorOnTileWithIniative == null && Spawner == null && (Props == null || Props.Count == 0))
				{
					if (PropInfos != null)
					{
						return PropInfos.Count == 0;
					}
					return true;
				}
				return false;
			}
		}
	}

	public class PropInfoParams
	{
		public readonly ScenarioManager.ObjectImportType Type;

		private readonly string _title;

		public int Count;

		public string Description;

		public PropEffectsParams EffectsParams;

		public string Title
		{
			get
			{
				if (Count > 1)
				{
					return _title + " x" + Count;
				}
				return _title;
			}
		}

		public PropInfoParams(ScenarioManager.ObjectImportType type, string title, string description = null)
		{
			Type = type;
			_title = title;
			Description = description;
			Count = 1;
		}

		public PropInfoParams(CObjectProp prop, string title, string description)
			: this(prop.ObjectType, title, description)
		{
			if (prop is CObjectTrap trapData)
			{
				EffectsParams = new PropEffectsParams(trapData);
			}
			else if (prop is CObjectHazardousTerrain hazardousData)
			{
				EffectsParams = new PropEffectsParams(hazardousData);
			}
			else if (prop.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem)
			{
				EffectsParams = new PropEffectsParams
				{
					QuestItemDescription = description
				};
			}
		}
	}

	public class PropEffectsParams
	{
		public CActor Owner;

		public int DamageValue;

		public int HealValue;

		public int MoveCost;

		public List<CCondition.ENegativeCondition> Conditions;

		public List<CCondition.EPositiveCondition> PositiveConditions;

		public string QuestItemDescription;

		public PropEffectsParams()
		{
		}

		public PropEffectsParams(CObjectTrap trapData)
		{
			Owner = trapData.Owner;
			HealValue = trapData.HealValue;
			Conditions = trapData.Conditions;
			PositiveConditions = trapData.PositiveConditions;
			if (trapData.Damage)
			{
				DamageValue = ((trapData.Owner != null || trapData.HasCustomDamageSet) ? trapData.DamageValue : ScenarioManager.TrapDamage);
			}
		}

		public PropEffectsParams(CObjectHazardousTerrain hazardousData)
		{
			Owner = hazardousData.Owner;
			DamageValue = ScenarioManager.HazardousTerrainDamage;
		}
	}

	public HexViewService(FHScenarioViewController view)
		: base(view)
	{
	}

	public static HexInfoParams GenerateHexInfoParams(CTile tile)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (!tile.m_HexMap.Revealed && (tile.m_Hex2Map == null || !tile.m_Hex2Map.Revealed))
			{
				return default;
			}
			List<PropInfoParams> list = new List<PropInfoParams>();
			List<CObjectProp> list2 = new List<CObjectProp>();
			CActor cActor = ScenarioManager.Scenario.FindActorAt(tile.m_ArrayIndex);
			bool flag = false;
			List<CObjective> list3 = ScenarioManager.CurrentScenarioState.AllObjectives.Where((CObjective x) =>
			{
				if (x.CustomTileHoverLocKey.IsNOTNullOrEmpty())
				{
					EObjectiveType objectiveType = x.ObjectiveType;
					return objectiveType == EObjectiveType.ActorReachPosition || objectiveType == EObjectiveType.BringItemsToArea || objectiveType == EObjectiveType.ActorsEscaped || objectiveType == EObjectiveType.AnyActorReachPosition;
				}
				return false;
			}).ToList();
			if (list3.Any())
			{
				foreach (CObjective item in list3)
				{
					if (!item.CustomTileHoverLocKey.IsNullOrEmpty() && ((item is CObjective_ActorReachPosition cObjective_ActorReachPosition) ? cObjective_ActorReachPosition.ActorTargetPositions : ((item is CObjective_ActorsEscaped cObjective_ActorsEscaped) ? cObjective_ActorsEscaped.EscapePositions : ((item is CObjective_BringItemsToArea cObjective_BringItemsToArea) ? cObjective_BringItemsToArea.AreaPositions : ((!(item is CObjective_AnyActorReachPosition cObjective_AnyActorReachPosition)) ? null : cObjective_AnyActorReachPosition.ActorTargetPositions))))?.FirstOrDefault((TileIndex x) => x.X == tile.m_ArrayIndex.X && x.Y == tile.m_ArrayIndex.Y) != null)
					{
						list.Add(new PropInfoParams(ScenarioManager.ObjectImportType.Door, LocalizationManager.GetTranslation("DUNGEON_EXIT_TOOLTIP"), LocalizationManager.GetTranslation(item.CustomTileHoverLocKey)));
						flag = true;
					}
				}
			}
			if (!flag && ClientScenarioManager.s_ClientScenarioManager.DungeonExitTiles.Any((CClientTile cct) => cct.m_Tile == tile))
			{
				list.Add(new PropInfoParams(ScenarioManager.ObjectImportType.Door, LocalizationManager.GetTranslation("DUNGEON_EXIT_TOOLTIP"), LocalizationManager.GetTranslation("GUI_Exit_Tile")));
				flag = true;
			}
			foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.Where((CObjectProp x) => x.PropTile == tile || x.GetTiles().Contains(tile.m_ArrayIndex)).ToList())
			{
				if (cActor != null && prop.RuntimeAttachedActor == cActor && prop.HasInitiative)
				{
					continue;
				}
				string text = null;
				string description = ((!string.IsNullOrEmpty(prop.PropCustomDescriptionLocKey)) ? LocalizationManager.GetTranslation(prop.PropCustomDescriptionLocKey) : null);
				string text2 = ((!string.IsNullOrEmpty(prop.PropCustomLocKey)) ? prop.PropCustomLocKey : prop.PropHealthDetails?.CustomLocKey);
				int num = 0;
				switch (prop.ObjectType)
				{
				case ScenarioManager.ObjectImportType.MoneyToken:
				{
					text = LocalizationManager.GetTranslation("LOOT_TOKEN_TOOLTIP");
					PropInfoParams propInfoParams2 = list.FirstOrDefault((PropInfoParams pip) => pip.Type == prop.ObjectType);
					if (propInfoParams2 != null)
					{
						propInfoParams2.Count++;
						continue;
					}
					description = null;
					break;
				}
				case ScenarioManager.ObjectImportType.Chest:
					text = LocalizationManager.GetTranslation("TREASURE_CHEST_TOOLTIP");
					break;
				case ScenarioManager.ObjectImportType.GoalChest:
					text = LocalizationManager.GetTranslation((!text2.IsNullOrEmpty()) ? text2 : "GOAL_CHEST_TOOLTIP");
					break;
				case ScenarioManager.ObjectImportType.Door:
				{
					CObjectDoor cObjectDoor = (CObjectDoor)prop;
					if (cObjectDoor.DoorIsOpen)
					{
						text = LocalizationManager.GetTranslation("DOORWAY_TOOLTIP");
					}
					else
					{
						text = (cObjectDoor.DoorIsLocked ? LocalizationManager.GetTranslation("LOCKED_DOOR_TOOLTIP") : LocalizationManager.GetTranslation("CLOSED_DOOR_TOOLTIP"));
					}
					if (cObjectDoor.DoorType == CObjectDoor.EDoorType.EmptyDoor || cObjectDoor.DoorType == CObjectDoor.EDoorType.EmptyDoorThin)
					{
						continue;
					}
					break;
				}
				case ScenarioManager.ObjectImportType.CarryableQuestItem:
					text = LocalizationManager.GetTranslation(FHUILocFormatting.GetScenarioQuestItemName(prop));
					description = string.Empty;
					break;
				case ScenarioManager.ObjectImportType.Obstacle:
					text = GetObstacleTooltipTitle(prop);
					description = (prop.OverrideDisallowDestroyAndMove ? LocalizationManager.GetTranslation("IMMOVABLE_OBSTACLE_DESCR_TOOLTIP") : null);
					break;
				case ScenarioManager.ObjectImportType.Trap:
					text = LocalizationManager.GetTranslation(((CObjectTrap)prop).PrefabName);
					break;
				case ScenarioManager.ObjectImportType.PressurePlate:
					text = GetPressurePlateTooltipTitle(prop);
					description = null;
					break;
				case ScenarioManager.ObjectImportType.Portal:
					text = LocalizationManager.GetTranslation("PORTAL_TOOLTIP");
					description = LocalizationManager.GetTranslation("PORTAL_DESCR_TOOLTIP");
					break;
				case ScenarioManager.ObjectImportType.Shadow:
					text = LocalizationManager.GetTranslation("SHADOW_TOOLTIP");
					break;
				case ScenarioManager.ObjectImportType.TerrainHotCoals:
				case ScenarioManager.ObjectImportType.TerrainThorns:
					text = LocalizationManager.GetTranslation((!text2.IsNullOrEmpty()) ? text2 : "HAZARDOUS_TERRAIN_TOOLTIP");
					break;
				case ScenarioManager.ObjectImportType.TerrainWater:
				case ScenarioManager.ObjectImportType.TerrainRubble:
				{
					if (!WorldspaceStarHexDisplay.ShowAsDifficulTerrain(prop.PropTile))
					{
						continue;
					}
					string term;
					if (text2.IsNullOrEmpty())
					{
						term = (prop.PropTile.TreatAsHazardous ? "HAZARDOUS_DIFFICULT_TERRAIN_TOOLTIP" : "DIFFICULT_TERRAIN_TOOLTIP");
					}
					else
					{
						term = text2;
					}
					text = LocalizationManager.GetTranslation(term);
					num = 2;
					description = (prop.OverrideDisallowDestroyAndMove ? LocalizationManager.GetTranslation("IMMOVABLE_DIFFICULT_TERRAIN_DESCR_TOOLTIP") : null);
					break;
				}
				case ScenarioManager.ObjectImportType.TerrainIce:
					text = LocalizationManager.GetTranslation("FH_GenericOverlay_icyterrain");
					description = null;
					break;
				case ScenarioManager.ObjectImportType.GenericProp:
				case ScenarioManager.ObjectImportType.TerrainVisualEffect:
				case ScenarioManager.ObjectImportType.VFXProp:
					if (text2.IsNullOrEmpty())
					{
						continue;
					}
					text = LocalizationManager.GetTranslation(text2);
					description = null;
					break;
				case ScenarioManager.ObjectImportType.Resource:
				{
					CObjectResource cObjectResource = (CObjectResource)prop;
					PropInfoParams propInfoParams = list.FirstOrDefault((PropInfoParams pip) => pip.Type == prop.ObjectType);
					if (propInfoParams != null)
					{
						propInfoParams.Count++;
						continue;
					}
					text = LocalizationManager.GetTranslation((!text2.IsNullOrEmpty()) ? text2 : cObjectResource.ResourceData.ID);
					description = UIGeneralInfoUtils.GetResourceDescription(cObjectResource, ignoreCustomDescription: true);
					break;
				}
				default:
					continue;
				}
				if (text.IsNullOrEmpty())
				{
					Log.GUIError($"Missing title for prop type {prop.ObjectType}!");
					continue;
				}
				if (prop.CustomLocKeyArgs != null && !text2.IsNullOrEmpty())
				{
					text = string.Format(text, prop.CustomLocKeyArgs.ToArray());
				}
				PropInfoParams propInfoParams3 = new PropInfoParams(prop, text, description);
				if (tile.TreatAsDifficult)
				{
					num = 2;
				}
				if (num > 0)
				{
					PropInfoParams propInfoParams4 = propInfoParams3;
					if (propInfoParams4.EffectsParams == null)
					{
						propInfoParams4.EffectsParams = new PropEffectsParams
						{
							MoveCost = num
						};
					}
				}
				if (tile.TreatAsHazardous && !list.Any((PropInfoParams x) => x.EffectsParams.DamageValue > 0))
				{
					if (propInfoParams3.EffectsParams == null)
					{
						propInfoParams3.EffectsParams = new PropEffectsParams
						{
							DamageValue = ScenarioManager.HazardousTerrainDamage
						};
					}
					else
					{
						propInfoParams3.EffectsParams.DamageValue = ScenarioManager.HazardousTerrainDamage;
					}
				}
				list.Add(propInfoParams3);
				list2.Add(prop);
			}
			CActor cActor2 = cActor;
			if (cActor2 is CObjectActor { IsAttachedToProp: not false } cObjectActor && !cObjectActor.AttachedProp.HasInitiative)
			{
				cActor2 = null;
			}
			return new HexInfoParams
			{
				ActorOnTile = cActor,
				ActorOnTileWithIniative = cActor2,
				Spawner = tile.m_Spawners.FirstOrDefault(),
				PropInfos = list,
				Props = list2
			};
		}
	}

	public static string GetObstacleTooltipTitle(CObjectProp prop)
	{
		CObjectObstacle cObjectObstacle = prop as CObjectObstacle;
		bool flag = cObjectObstacle.BlockLOS && cObjectObstacle.IgnoresFlyAndJump;
		string text = string.Empty;
		if (prop.PropHealthDetails != null && !string.IsNullOrEmpty(prop.PropHealthDetails.CustomLocKey))
		{
			text = prop.PropHealthDetails.CustomLocKey;
		}
		else if (!string.IsNullOrEmpty(prop.PropCustomLocKey))
		{
			text = prop.PropCustomLocKey;
		}
		string term;
		if (string.IsNullOrEmpty(text))
		{
			term = (flag ? "WALL_TOOLTIP" : "OBSTACLE_TOOLTIP");
		}
		else
		{
			term = text;
		}
		return LocalizationManager.GetTranslation(term);
	}

	public static string GetPressurePlateTooltipTitle(CObjectProp prop)
	{
		string text = string.Empty;
		if (prop.PropHealthDetails != null && !string.IsNullOrEmpty(prop.PropHealthDetails.CustomLocKey))
		{
			text = prop.PropHealthDetails.CustomLocKey;
		}
		else if (!string.IsNullOrEmpty(prop.PropCustomLocKey))
		{
			text = prop.PropCustomLocKey;
		}
		return LocalizationManager.GetTranslation((!string.IsNullOrEmpty(text)) ? text : "PRESSURE_PLATE_TOOLTIP");
	}
}
