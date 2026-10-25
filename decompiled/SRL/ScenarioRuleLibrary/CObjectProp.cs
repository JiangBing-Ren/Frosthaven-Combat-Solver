using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;
using SharedLibrary;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjectProp : ISerializable, IDumpState
{
	public static readonly EPropType[] PropTypes = (EPropType[])Enum.GetValues(typeof(EPropType));

	public static readonly ESpecificPropType[] SpecificPropTypes = (ESpecificPropType[])Enum.GetValues(typeof(ESpecificPropType));

	public static readonly EVFXPropType[] VFXPropTypes = (EVFXPropType[])Enum.GetValues(typeof(EVFXPropType));

	public static readonly ScenarioManager.EPerPartySizeConfig[] PerPartySizeConfigOptions = ((ScenarioManager.EPerPartySizeConfig[])Enum.GetValues(typeof(ScenarioManager.EPerPartySizeConfig))).Where((ScenarioManager.EPerPartySizeConfig c) => c != ScenarioManager.EPerPartySizeConfig.ToElite).ToArray();

	[StateDump(Skip = true)]
	public bool OnlyVisuals;

	private Dictionary<int, ScenarioManager.EPerPartySizeConfig> ConfigPerPartySize;

	[NonSerialized]
	[StateDump(Skip = true)]
	public CSpawner RuntimeAttachedSpawner;

	public const string DummyPropClass = "PropDummyObject";

	protected string m_OwnerGuid;

	[StateDump(Skip = true)]
	public bool IsBaked { get; set; }

	public string PropGuid { get; private set; }

	public ScenarioManager.ObjectImportType ObjectType { get; private set; }

	public string PrefabName { get; set; }

	public TileIndex ArrayIndex { get; private set; }

	public bool NegativeHex { get; set; }

	[StateDump(Skip = true)]
	public CVector3 Position { get; private set; }

	public CVector3 Rotation { get; private set; }

	public virtual bool Activated { get; protected set; }

	public bool SkipVisuals
	{
		get
		{
			if (!(PrefabName == "SkipVisuals"))
			{
				return HasInitiative;
			}
			return true;
		}
	}

	public bool ReleasePFAfterDestroy { get; protected set; }

	public bool HasInitiative => GetObjectClass()?.HasCards ?? false;

	public Point VisualsForceLookAt { get; set; }

	public string VisualsForceLookAtRotateChild { get; set; }

	public string StartingMapGuid { get; set; }

	public string ActorActivated { get; protected set; }

	public string OwnerGUID
	{
		get
		{
			return m_OwnerGuid;
		}
		set
		{
			m_OwnerGuid = value;
		}
	}

	public ESpecificPropType SpecificPropType => SpecificPropTypes.SingleOrDefault((ESpecificPropType x) => x.ToString() == PrefabName);

	public EPropType PropType
	{
		get
		{
			ESpecificPropType specificPropType = SpecificPropType;
			if (specificPropType != ESpecificPropType.None)
			{
				return SpecificPropTypeToPropType(specificPropType);
			}
			return PropTypes.SingleOrDefault((EPropType x) => x.ToString() == PrefabName);
		}
	}

	public CActor Owner
	{
		get
		{
			CActor cActor = null;
			if (m_OwnerGuid != null)
			{
				cActor = ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == m_OwnerGuid);
				if (cActor == null)
				{
					cActor = ScenarioManager.Scenario.HeroSummons.SingleOrDefault((CHeroSummonActor s) => s.ActorGuid == m_OwnerGuid);
					if (cActor == null)
					{
						cActor = ScenarioManager.Scenario.AllMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == m_OwnerGuid);
					}
				}
			}
			return cActor;
		}
	}

	public CMap StartingMap => ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap map) => map.MapGuid == StartingMapGuid);

	public bool IsLootable
	{
		get
		{
			if (ObjectType != ScenarioManager.ObjectImportType.Chest && ObjectType != ScenarioManager.ObjectImportType.GoalChest && ObjectType != ScenarioManager.ObjectImportType.MoneyToken && ObjectType != ScenarioManager.ObjectImportType.CarryableQuestItem)
			{
				return ObjectType == ScenarioManager.ObjectImportType.Resource;
			}
			return true;
		}
	}

	public CTile PropTile => ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y];

	public bool IsDestroyed
	{
		get
		{
			if (!PropGuid.Contains("spawner_prop"))
			{
				return ScenarioManager.CurrentScenarioState.DestroyedProps.SingleOrDefault((CObjectProp x) => x.PropGuid == PropGuid) != null;
			}
			return false;
		}
	}

	public bool IgnoreEndOfTurnLooting { get; private set; }

	public CObjectiveFilter CanLootFilter { get; set; }

	public string CanLootLocKey { get; set; }

	public string PropCustomLocKey { get; set; }

	public string PropCustomDescriptionLocKey { get; set; }

	public List<object> CustomLocKeyArgs { get; set; }

	public PropHealthDetails PropHealthDetails { get; set; }

	public bool PropActorHasBeenAssigned { get; private set; }

	public CObjectActor RuntimeAttachedActor { get; private set; }

	public bool OverrideDisallowDestroyAndMove { get; private set; }

	public bool IsImmuneToDamage { get; set; }

	public string InstanceName => PrefabName + " : (" + PropGuid + ")";

	public bool HasLinkedProps
	{
		get
		{
			if (LinkedPropGuids != null)
			{
				return LinkedPropGuids.Count > 0;
			}
			return false;
		}
	}

	[JsonIgnore]
	public IEnumerable<CObjectProp> LinkedProps
	{
		get
		{
			if (LinkedPropGuids == null)
			{
				return null;
			}
			return ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp x) => LinkedPropGuids.Contains(x.PropGuid));
		}
	}

	[JsonProperty]
	public List<string> LinkedPropGuids { get; private set; }

	private CObjectClass GetObjectClass()
	{
		if (PropHealthDetails == null)
		{
			return null;
		}
		string ActorClass = PropHealthDetails.ActorClass;
		string classID;
		if (string.IsNullOrWhiteSpace(ActorClass))
		{
			classID = "PropDummyObject";
		}
		else if (MonsterClassManager.ObjectClasses.All((CObjectClass c) => c.ID != ActorClass))
		{
			classID = "PropDummyObject";
		}
		else
		{
			classID = ActorClass;
		}
		return MonsterClassManager.ObjectClasses.SingleOrDefault((CObjectClass x) => x.ID == classID);
	}

	public static EPropType SpecificPropTypeToPropType(ESpecificPropType specificPropType)
	{
		switch (specificPropType)
		{
		case ESpecificPropType.RockSingle:
		case ESpecificPropType.PlinthSingle:
		case ESpecificPropType.GlowingOrb:
		case ESpecificPropType.PowerTower:
		case ESpecificPropType.HulkingPillar:
		case ESpecificPropType.Pedestal:
		case ESpecificPropType.AlgoxAltar:
		case ESpecificPropType.MaintenanceShaft:
		case ESpecificPropType.DebrisPile:
		case ESpecificPropType.ShadowWallResidue:
		case ESpecificPropType.FrozenFistObstacle:
		case ESpecificPropType.SmallTree:
		case ESpecificPropType.Chasm:
		case ESpecificPropType.OrbOfBones:
		case ESpecificPropType.CagedVermlingScout:
		case ESpecificPropType.RockColumn:
		case ESpecificPropType.WeatheredAltar:
		case ESpecificPropType.HealingPillar:
		case ESpecificPropType.GlowingCrystal:
		case ESpecificPropType.Temporal:
		case ESpecificPropType.MysteriousMachine:
		case ESpecificPropType.PyroclastObstacle:
		case ESpecificPropType.TotemOfEarth:
		case ESpecificPropType.TotemOfRestoration:
		case ESpecificPropType.WarBanner:
		case ESpecificPropType.TotemOfBewilderment:
		case ESpecificPropType.PetOoze1HObstacle:
		case ESpecificPropType.Infuser1HObstacle:
		case ESpecificPropType.ItemWildGrowthSeedsObstacle:
			return EPropType.OneHexObstacle;
		case ESpecificPropType.SpecialConsole:
		case ESpecificPropType.LurkerStatue:
		case ESpecificPropType.PetOoze2HObstacle:
			return EPropType.TwoHexObstacle;
		case ESpecificPropType.BearTrap:
		case ESpecificPropType.BombTrap:
		case ESpecificPropType.DamageMine:
		case ESpecificPropType.PoisonMine:
		case ESpecificPropType.StunMine:
		case ESpecificPropType.TrapperTrap:
		case ESpecificPropType.DetonationCharge:
			return EPropType.Trap;
		case ESpecificPropType.PyroclastHazardTerrain:
			return EPropType.TerrainHotCoals;
		case ESpecificPropType.SnowdancerHazardTerrain:
			return EPropType.TerrainThorns;
		case ESpecificPropType.VestigeHazardTerrain:
			return EPropType.TerrainThorns;
		case ESpecificPropType.VestigeDifficultTerrain:
			return EPropType.TerrainRubble;
		case ESpecificPropType.LurkerWavethrowerWater:
		case ESpecificPropType.CrashingTideWater:
			return EPropType.TerrainWater;
		case ESpecificPropType.RockTriple:
		case ESpecificPropType.Bathysphere:
		case ESpecificPropType.TrueOakA:
		case ESpecificPropType.TrueOakB:
		case ESpecificPropType.PetOoze3HObstacle:
		case ESpecificPropType.Infuser3HObstacle:
			return EPropType.ThreeHexObstacle;
		case ESpecificPropType.SewerPipeSpawner:
		case ESpecificPropType.GraveSingleSpawner:
		case ESpecificPropType.GraveDoubleSpawner:
			return EPropType.Spawner;
		case ESpecificPropType.QuestDoll:
		case ESpecificPropType.QuestClaw:
		case ESpecificPropType.CoralPiece:
		case ESpecificPropType.LavaLootToken:
		case ESpecificPropType.MammothBark:
			return EPropType.QuestItem;
		case ESpecificPropType.TheFavorite:
		case ESpecificPropType.TheNewFavorite:
			return EPropType.CharacterResource;
		case ESpecificPropType.TreeBridgeA:
		case ESpecificPropType.TreeBridgeB:
			return EPropType.TerrainRubble;
		case ESpecificPropType.Sarcophagus:
			return EPropType.TerrainRubble;
		case ESpecificPropType.LootableCorpse:
		case ESpecificPropType.ReceptacleContainer:
			return EPropType.GoalChest;
		case ESpecificPropType.LootableCrate:
		case ESpecificPropType.LootableCrate127:
		case ESpecificPropType.FH_Scenario_Campaign_126_SCENARIO_OBJECT_1:
		case ESpecificPropType.SunAgate:
		case ESpecificPropType.LumberPile:
			return EPropType.CharacterResource;
		case ESpecificPropType.MovingPlatform:
		case ESpecificPropType.TemporalHex:
		case ESpecificPropType.PortalTerrain:
		case ESpecificPropType.DarkPitTerrain:
		case ESpecificPropType.NothingTerrain:
		case ESpecificPropType.MovingBoat136:
		case ESpecificPropType.MovingBoat136B:
		case ESpecificPropType.FlotsamOneHex:
		case ESpecificPropType.FlotsamTwoHex:
			return EPropType.TerrainVisualEffect;
		case ESpecificPropType.SnowDriftPile:
			return EPropType.OneHexObstacle;
		case ESpecificPropType.BlackCauldron:
			return EPropType.OneHexObstacle;
		case ESpecificPropType.StatueOfTheBuilder:
			return EPropType.OneHexObstacle;
		case ESpecificPropType.FloatingIce:
			return EPropType.None;
		case ESpecificPropType.Ladder:
		case ESpecificPropType.AbaelHallway:
			return EPropType.None;
		default:
			return EPropType.None;
		}
	}

	public static bool IsLootableObjectImportType(ScenarioManager.ObjectImportType objectType)
	{
		if (objectType != ScenarioManager.ObjectImportType.Chest && objectType != ScenarioManager.ObjectImportType.GoalChest && objectType != ScenarioManager.ObjectImportType.MoneyToken && objectType != ScenarioManager.ObjectImportType.CarryableQuestItem)
		{
			return objectType == ScenarioManager.ObjectImportType.Resource;
		}
		return true;
	}

	public List<CObjectProp> GetLinkedPropsTotal()
	{
		if (!HasLinkedProps)
		{
			return null;
		}
		HashSet<CObjectProp> hashSet = new HashSet<CObjectProp>();
		List<CObjectProp> list = new List<CObjectProp>();
		list.AddRange(LinkedProps);
		while (list.Count > 0)
		{
			CObjectProp cObjectProp = list[0];
			list.RemoveAt(0);
			hashSet.Add(cObjectProp);
			foreach (CObjectProp linkedProp in cObjectProp.LinkedProps)
			{
				if (!hashSet.Contains(linkedProp))
				{
					list.Add(linkedProp);
				}
			}
		}
		return hashSet.ToList();
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ReleasePFAfterDestroy", ReleasePFAfterDestroy);
		info.AddValue("PropGuid", PropGuid);
		info.AddValue("ObjectType", ObjectType);
		info.AddValue("PrefabName", PrefabName);
		info.AddValue("ArrayIndex", ArrayIndex);
		info.AddValue("Position", Position);
		info.AddValue("Rotation", Rotation);
		info.AddValue("VisualsForceLookAt", VisualsForceLookAt);
		info.AddValue("Activated", Activated);
		info.AddValue("NegativeHex", NegativeHex);
		info.AddValue("m_OwnerGuid", m_OwnerGuid);
		info.AddValue("StartingMapGuid", StartingMapGuid);
		info.AddValue("ActorActivated", ActorActivated);
		info.AddValue("ConfigPerPartySize", ConfigPerPartySize);
		info.AddValue("IgnoreEndOfTurnLooting", IgnoreEndOfTurnLooting);
		info.AddValue("PropHealthDetails", PropHealthDetails);
		info.AddValue("PropActorHasBeenAssigned", PropActorHasBeenAssigned);
		info.AddValue("OverrideDisallowDestroyAndMove", OverrideDisallowDestroyAndMove);
		info.AddValue("CanLootFilter", CanLootFilter);
		info.AddValue("CanLootLocKey", CanLootLocKey);
		info.AddValue("PropCustomLocKey", PropCustomLocKey);
		info.AddValue("PropCustomDescriptionLocKey", PropCustomDescriptionLocKey);
		info.AddValue("LinkedProps", LinkedPropGuids);
		info.AddValue("CustomLocKeyArgs", CustomLocKeyArgs);
		info.AddValue("IsBaked", IsBaked);
	}

	public CObjectProp(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "ReleasePFAfterDestroy":
					ReleasePFAfterDestroy = info.GetBoolean("ReleasePFAfterDestroy");
					break;
				case "IsBaked":
					IsBaked = info.GetBoolean("IsBaked");
					break;
				case "PropGuid":
					PropGuid = info.GetString("PropGuid");
					break;
				case "ObjectType":
					ObjectType = (ScenarioManager.ObjectImportType)info.GetValue("ObjectType", typeof(ScenarioManager.ObjectImportType));
					break;
				case "PrefabName":
					PrefabName = info.GetString("PrefabName");
					break;
				case "ArrayIndex":
					ArrayIndex = (TileIndex)info.GetValue("ArrayIndex", typeof(TileIndex));
					break;
				case "Position":
					Position = (CVector3)info.GetValue("Position", typeof(CVector3));
					break;
				case "Rotation":
					Rotation = (CVector3)info.GetValue("Rotation", typeof(CVector3));
					break;
				case "VisualsForceLookAt":
					VisualsForceLookAt = (Point)info.GetValue("VisualsForceLookAt", typeof(Point));
					break;
				case "Activated":
					Activated = info.GetBoolean("Activated");
					break;
				case "NegativeHex":
					NegativeHex = info.GetBoolean("NegativeHex");
					break;
				case "m_OwnerGuid":
					m_OwnerGuid = info.GetString("m_OwnerGuid");
					break;
				case "StartingMapGuid":
					StartingMapGuid = info.GetString("StartingMapGuid");
					break;
				case "ActorActivated":
					ActorActivated = info.GetString("ActorActivated");
					break;
				case "ConfigPerPartySize":
					ConfigPerPartySize = (Dictionary<int, ScenarioManager.EPerPartySizeConfig>)info.GetValue("ConfigPerPartySize", typeof(Dictionary<int, ScenarioManager.EPerPartySizeConfig>));
					break;
				case "IgnoreEndOfTurnLooting":
					IgnoreEndOfTurnLooting = info.GetBoolean("IgnoreEndOfTurnLooting");
					break;
				case "PropHealthDetails":
					PropHealthDetails = (PropHealthDetails)info.GetValue("PropHealthDetails", typeof(PropHealthDetails));
					break;
				case "PropActorHasBeenAssigned":
					PropActorHasBeenAssigned = info.GetBoolean("PropActorHasBeenAssigned");
					break;
				case "OverrideDisallowDestroyAndMove":
					OverrideDisallowDestroyAndMove = info.GetBoolean("OverrideDisallowDestroyAndMove");
					break;
				case "CanLootFilter":
					CanLootFilter = (CObjectiveFilter)info.GetValue("CanLootFilter", typeof(CObjectiveFilter));
					break;
				case "CanLootLocKey":
					CanLootLocKey = info.GetString("CanLootLocKey");
					break;
				case "PropCustomLocKey":
					PropCustomLocKey = info.GetString("PropCustomLocKey");
					break;
				case "PropCustomDescriptionLocKey":
					PropCustomDescriptionLocKey = info.GetString("PropCustomDescriptionLocKey");
					break;
				case "CustomLocKeyArgs":
					CustomLocKeyArgs = (List<object>)info.GetValue("CustomLocKeyArgs", typeof(List<object>));
					break;
				case "LinkedProps":
					LinkedPropGuids = (List<string>)info.GetValue("LinkedProps", typeof(List<string>));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjectProp entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		InternalOnDeserialized();
	}

	protected virtual void InternalOnDeserialized()
	{
		if (PrefabName == "GraveSingle")
		{
			PrefabName = ESpecificPropType.GraveSingleSpawner.ToString();
		}
		if (PrefabName == "GraveDouble")
		{
			PrefabName = ESpecificPropType.GraveDoubleSpawner.ToString();
		}
	}

	public override string ToString()
	{
		string text = (Activated ? "[Activated] " : "");
		return $"[{MF.RefStr(this)}] {text}{GetType().Name}({PrefabName} at {ArrayIndex}, GUID:{PropGuid})";
	}

	string IDumpState.ToStateDumpString(StateDump dump)
	{
		return $"{GetType().Name}({PrefabName} at {ArrayIndex}, GUID:{PropGuid})";
	}

	public CObjectProp(string prefabName, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid, string customGuid = null)
	{
		if (customGuid != null)
		{
			PropGuid = customGuid;
			OnlyVisuals = true;
		}
		else if (ScenarioManager.CurrentScenarioState != null && ScenarioManager.CurrentScenarioState.GuidRNG != null)
		{
			PropGuid = ScenarioManager.CurrentScenarioState.GetGUIDBasedOnGuidRNGState().ToString();
		}
		else
		{
			PropGuid = Guid.NewGuid().ToString();
		}
		StartingMapGuid = mapGuid;
		PrefabName = prefabName;
		ObjectType = type;
		ArrayIndex = arrayIndex;
		Position = position;
		Rotation = rotation;
		m_OwnerGuid = owner?.ActorGuid;
		ConfigPerPartySize = GetDefaultConfigPerPartySize();
		if (!DelayedLogSpawn())
		{
			LogSpawn();
		}
	}

	public CObjectProp(SharedLibrary.Random scenarioGenerationRNG, string prefabName, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
	{
		byte[] array = new byte[16];
		scenarioGenerationRNG.NextBytes(array);
		PropGuid = new Guid(array).ToString();
		StartingMapGuid = mapGuid;
		PrefabName = prefabName;
		ObjectType = type;
		ArrayIndex = arrayIndex;
		Position = position;
		Rotation = rotation;
		m_OwnerGuid = owner?.ActorGuid;
		ConfigPerPartySize = GetDefaultConfigPerPartySize();
		if (!DelayedLogSpawn())
		{
			LogSpawn();
		}
	}

	public bool DelayedLogSpawn()
	{
		return ObjectType == ScenarioManager.ObjectImportType.MoneyToken;
	}

	public void LogSpawn()
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventObjectProp(ESESubTypeObjectProp.Spawned, ObjectType, PropGuid, PrefabName, null, "Spawned", OwnerGUID, "", new List<CMap>(), ArrayIndex.ToPoint()));
	}

	public virtual void SetLocation(TileIndex arrayIndex, CVector3 position, CVector3 rotation)
	{
		ArrayIndex = arrayIndex;
		Position = position;
		Rotation = rotation;
	}

	public static bool TryChooseToSkipTrap(CActor actor, CObjectProp prop, CAbility ability = null, bool jump = false)
	{
		if (actor == null || prop == null)
		{
			return false;
		}
		if (!prop.WillActivate(actor, jump, actor.FlyingAfterConditions))
		{
			return true;
		}
		CVars cVars = new CVars();
		cVars.Set("TriggerProp", new TValue(prop));
		List<CItem> list = new List<CItem>();
		if (GameState.ShowItemSelection(actor, actor, ability, CItem.EItemTrigger.OnAutomaticActivateProp, onlyInActionPhase: false, killAfter: false, cVars, list) && list.Any((CItem x) => x.GetVar("StopPropActivate").GetBool()))
		{
			return true;
		}
		if (prop.ObjectType != ScenarioManager.ObjectImportType.Trap || prop.RuntimeAttachedActor != null)
		{
			return false;
		}
		if (actor.Class is CCharacterClass { Perks: var perks } && perks.Any((PerksYMLData x) => x.Flags.HasFlag(EPerkFlag.ChooseToNotTriggerTrap)))
		{
			return GameState.ShowGenericConfirmButton(actor, "PERK_ChooseNotTriggerTrap_Desc", "PERK_ChooseNotTriggerTrap_Confirm", null, "PERK_ChooseNotTriggerTrap_Trigger", showHeader: false);
		}
		return false;
	}

	public virtual bool AutomaticActivate(CActor actor, bool actorIsJumping = false, bool actorIsFlying = false)
	{
		if (ObjectType == ScenarioManager.ObjectImportType.Spawner)
		{
			return false;
		}
		return Activate(actor, null, actorIsJumping, actorIsFlying);
	}

	public virtual bool Activate(CActor actor, CActor creditActor = null, bool actorIsJumping = false, bool actorIsFlying = false)
	{
		if (ObjectType == ScenarioManager.ObjectImportType.VFXProp)
		{
			return false;
		}
		CObjectResource cObjectResource = this as CObjectResource;
		if (!Activated || (cObjectResource != null && cObjectResource.ResourceData.StayActiveAfterLooted))
		{
			Activated = true;
			ActorActivated = ((actor == null) ? string.Empty : actor.ActorGuid);
			if (cObjectResource != null)
			{
				if (!cObjectResource.ResourceData.StayActiveAfterLooted)
				{
					ScenarioManager.CurrentScenarioState.Props.Remove(this);
				}
				ScenarioManager.CurrentScenarioState.ActivatedProps.Add(this);
			}
			else if (ObjectType == ScenarioManager.ObjectImportType.Chest || ObjectType == ScenarioManager.ObjectImportType.GoalChest || ObjectType == ScenarioManager.ObjectImportType.MoneyToken || ObjectType == ScenarioManager.ObjectImportType.Trap || ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem || ObjectType == ScenarioManager.ObjectImportType.MonsterGrave)
			{
				ScenarioManager.CurrentScenarioState.Props.Remove(this);
				ScenarioManager.CurrentScenarioState.ActivatedProps.Add(this);
			}
			CActivateProp_MessageData message = new CActivateProp_MessageData(actor)
			{
				m_Prop = this,
				m_InitialLoad = false,
				m_CreditActor = creditActor,
				m_Rewards = creditActor?.LastLoot?.GetRewards()?.ToList(),
				m_CarriedByActor = (actor?.CarriedQuestItems != null && actor.CarriedQuestItems.Contains(this))
			};
			ScenarioRuleClient.MessageHandler(message);
			return true;
		}
		return false;
	}

	public virtual bool Deactivate()
	{
		return false;
	}

	public void DestroyPropFromAbility(CAbility ability)
	{
		if (IsDestroyed)
		{
			return;
		}
		DestroyProp(0f, sendMessageToClient: true, forceDestroy: false, ability);
		if (ability?.TargetingActor != null)
		{
			ability.TargetingActor.m_OnPropDestroyedListeners(ability.TargetingActor, ability, this);
			string text = string.Empty;
			if (this is CObjectHazardousTerrain)
			{
				text = "<sprite name=HazardHex>";
			}
			else if (PropType == EPropType.TerrainRubble)
			{
				text = "<sprite name=DifficultHex>";
			}
			else if (PropType == EPropType.TerrainWater)
			{
				text = "<sprite name=WaterHex>";
			}
			else if (this is CObjectIceTerrain)
			{
				text = "<sprite name=IceHex>";
			}
			else if (this is CObjectObstacle)
			{
				text = "<sprite name=ObstacleHex>";
			}
			else if (this is CObjectTrap)
			{
				text = "Trap";
			}
			if (!string.IsNullOrEmpty(text))
			{
				CCombatLogMessage_MessageData message = new CCombatLogMessage_MessageData
				{
					m_MessageKey = "COMBAT_LOG_DESTROY_PROP",
					m_Actors = new List<CActor> { ability.TargetingActor },
					m_Params = new List<string> { "Actor_0", text }
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	public virtual void DestroyProp(float spawnDelay = 0f, bool sendMessageToClient = true, bool forceDestroy = false, CAbility destroyedByAbility = null, bool checkLinkedProp = true)
	{
		if (IsDestroyed)
		{
			return;
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventObjectProp(ESESubTypeObjectProp.Destroyed, ObjectType, PropGuid, PrefabName, null, "Destroyed", m_OwnerGuid, "", new List<CMap>(), ArrayIndex.ToPoint()));
		if (!OnlyVisuals)
		{
			ScenarioManager.CurrentScenarioState.Props.Remove(this);
			ScenarioManager.CurrentScenarioState.DestroyedProps.Add(this);
		}
		PropTile.DelProp(this);
		if (sendMessageToClient)
		{
			CDestroyProp_MessageData message = new CDestroyProp_MessageData
			{
				m_Prop = this,
				m_DestroyDelay = spawnDelay,
				m_ForceDestroy = forceDestroy,
				m_PropTile = PropTile
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		if (HasLinkedProps & checkLinkedProp)
		{
			foreach (CObjectProp item in GetLinkedPropsTotal())
			{
				UnlinkProp(item);
				item.DestroyProp(spawnDelay, sendMessageToClient, forceDestroy, null, checkLinkedProp: false);
			}
		}
		if (destroyedByAbility != null)
		{
			SEventAbility.SEventAbilityBaseData baseData = new SEventAbility.SEventAbilityBaseData(destroyedByAbility.TargetingActor, ESESubTypeAbility.AbilityEnded, destroyedByAbility);
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityDestroyProp(PropType, SpecificPropType, baseData));
		}
		if (PropType == EPropType.TerrainWater && (destroyedByAbility == null || !(destroyedByAbility is CAbilityMoveTerrain)))
		{
			CheckRelocateWaterMonsters();
		}
	}

	public static void CheckRelocateWaterMonsters()
	{
		foreach (CEnemyActor allEnemy in ScenarioManager.Scenario.AllEnemies)
		{
			if (!allEnemy.MonsterClass.IsWaterMonster)
			{
				continue;
			}
			CTile actorTile = ScenarioManager.Tiles[allEnemy.ArrayIndex.X, allEnemy.ArrayIndex.Y];
			if (actorTile.IsWater)
			{
				continue;
			}
			List<CTile> list = new List<CTile>();
			CTile[,] tiles = ScenarioManager.Tiles;
			foreach (CTile cTile in tiles)
			{
				if (cTile != null && cTile.IsWater && cTile.LivingActor == null && cTile.m_HexMap.Revealed && ScenarioManager.PathFinder.GetNode(cTile.m_ArrayIndex).Walkable)
				{
					list.Add(cTile);
				}
			}
			if (list.Count > 0)
			{
				CTile cTile2 = list.OrderBy((CTile x) => Point.HexDist(x.m_ArrayIndex, actorTile.m_ArrayIndex)).FirstOrDefault();
				allEnemy.ArrayIndex = cTile2.m_ArrayIndex;
				CActorHasPushed_MessageData cActorHasPushed_MessageData = new CActorHasPushed_MessageData(allEnemy);
				cActorHasPushed_MessageData.m_PushAbility = null;
				cActorHasPushed_MessageData.m_ActorBeingPushed = allEnemy;
				ScenarioRuleClient.MessageHandler(cActorHasPushed_MessageData);
				CAbilityStopFlying.TriggerTrapsAndHazards(allEnemy, cTile2);
			}
		}
	}

	public virtual bool WillActivate(CActor actor, bool actorIsJumping, bool actorIsFlying)
	{
		if (ObjectType == ScenarioManager.ObjectImportType.Spawner)
		{
			return false;
		}
		if (!actorIsJumping)
		{
			return !actorIsFlying;
		}
		return false;
	}

	public virtual bool WillActivationKillActor(CActor actor)
	{
		return false;
	}

	public virtual bool WillActivationDamageActor(CActor actor)
	{
		return false;
	}

	public bool CanActorLoot(CActor actor)
	{
		if (CanLootFilter != null)
		{
			bool num = ScenarioScript.current == null || ScenarioScript.current.CanActorLoot(this, actor);
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == actor.ActorGuid);
			if (num)
			{
				return CanLootFilter.IsValidTarget(actorState);
			}
			return false;
		}
		return true;
	}

	public void SetConfigForPartySize(int partySize, ScenarioManager.EPerPartySizeConfig configToSet)
	{
		ConfigPerPartySize[partySize] = configToSet;
	}

	public ScenarioManager.EPerPartySizeConfig GetConfigForPartySize(int partySize)
	{
		if (ConfigPerPartySize == null || !ConfigPerPartySize.ContainsKey(partySize))
		{
			return ScenarioManager.EPerPartySizeConfig.Normal;
		}
		return ConfigPerPartySize[partySize];
	}

	public void SetIgnoreEndOfTurnLooting(bool shouldIgnore)
	{
		IgnoreEndOfTurnLooting = shouldIgnore;
	}

	public void SetCanLootFilter(CObjectiveFilter canLootFilter)
	{
		CanLootFilter = canLootFilter;
	}

	public void SetCanLootLocKey(string canLootLocKey)
	{
		CanLootLocKey = canLootLocKey;
	}

	public void SetActorAttachedAtRuntime(CObjectActor actorToAttach)
	{
		RuntimeAttachedActor = actorToAttach;
		PropActorHasBeenAssigned = true;
	}

	public void ClearAttachedRuntimeActor()
	{
		RuntimeAttachedActor = null;
		PropActorHasBeenAssigned = false;
	}

	public void SetNewStartingMapGuid(string startingMapGuid)
	{
		StartingMapGuid = startingMapGuid;
	}

	public void SetOverrideDisallowMoveOrDestroy(bool shouldDisallow)
	{
		OverrideDisallowDestroyAndMove = shouldDisallow;
	}

	public static CObjectObstacle FindPropWithPathingBlocker(Point pathingBlocker, ref CTile propTile, bool returnPathingBlockerTile = false)
	{
		for (int i = 0; i < ScenarioManager.CurrentScenarioState.Props.Count; i++)
		{
			if (!(ScenarioManager.CurrentScenarioState.Props[i] is CObjectObstacle cObjectObstacle))
			{
				continue;
			}
			foreach (TileIndex pathingBlocker2 in cObjectObstacle.PathingBlockers)
			{
				if (pathingBlocker2.X == pathingBlocker.X && pathingBlocker2.Y == pathingBlocker.Y)
				{
					propTile = (returnPathingBlockerTile ? ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y] : cObjectObstacle.PropTile);
					return cObjectObstacle;
				}
			}
		}
		return null;
	}

	public static CInteractableSpawner FindSpawnerWithPathingBlocker(Point pathingBlocker, ref CTile spawnerTile)
	{
		for (int i = 0; i < ScenarioManager.Height; i++)
		{
			for (int j = 0; j < ScenarioManager.Width; j++)
			{
				CTile cTile = ScenarioManager.Tiles[j, i];
				if (cTile == null)
				{
					continue;
				}
				foreach (CInteractableSpawner item in cTile.m_Spawners.OfType<CInteractableSpawner>())
				{
					if (!(item.Prop is CObjectObstacle { PathingBlockers: not null } cObjectObstacle))
					{
						continue;
					}
					foreach (TileIndex pathingBlocker2 in cObjectObstacle.PathingBlockers)
					{
						if (pathingBlocker2.X == pathingBlocker.X && pathingBlocker2.Y == pathingBlocker.Y)
						{
							spawnerTile = cTile;
							return item;
						}
					}
				}
			}
		}
		return null;
	}

	private static Dictionary<int, ScenarioManager.EPerPartySizeConfig> GetDefaultConfigPerPartySize()
	{
		return new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
		{
			{
				1,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				2,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				3,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				4,
				ScenarioManager.EPerPartySizeConfig.Normal
			}
		};
	}

	public static CTile FindPropTile(string name)
	{
		CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.Props.SingleOrDefault((CObjectProp s) => s.InstanceName == name) ?? ScenarioManager.CurrentScenarioState.ActivatedProps.SingleOrDefault((CObjectProp s) => s.InstanceName == name);
		if (cObjectProp != null)
		{
			return ScenarioManager.Tiles[cObjectProp.ArrayIndex.X, cObjectProp.ArrayIndex.Y];
		}
		return null;
	}

	public static bool IsProp(ScenarioManager.ObjectImportType type)
	{
		switch (type)
		{
		case ScenarioManager.ObjectImportType.Door:
		case ScenarioManager.ObjectImportType.GenericProp:
		case ScenarioManager.ObjectImportType.Chest:
		case ScenarioManager.ObjectImportType.GoalChest:
		case ScenarioManager.ObjectImportType.MoneyToken:
		case ScenarioManager.ObjectImportType.Trap:
		case ScenarioManager.ObjectImportType.Obstacle:
		case ScenarioManager.ObjectImportType.CarryableQuestItem:
		case ScenarioManager.ObjectImportType.Resource:
		case ScenarioManager.ObjectImportType.MonsterGrave:
		case ScenarioManager.ObjectImportType.VFXProp:
			return true;
		default:
			return false;
		}
	}

	public static int PropImportanceValue(EPropType type)
	{
		switch (type)
		{
		case EPropType.GoalChest:
		case EPropType.Spawner:
		case EPropType.PressurePlate:
		case EPropType.Portal:
			return 0;
		case EPropType.Chest:
			return 1;
		case EPropType.OneHexObstacle:
		case EPropType.TwoHexObstacle:
		case EPropType.ThreeHexObstacle:
		case EPropType.DarkPitObstacle:
		case EPropType.ThreeHexCurvedObstacle:
		case EPropType.ThreeHexStraightObstacle:
			return 2;
		case EPropType.Trap:
		case EPropType.TerrainHotCoals:
		case EPropType.TerrainWater:
		case EPropType.TerrainRubble:
		case EPropType.TerrainThorns:
			return 3;
		default:
			return 4;
		}
	}

	public static List<Tuple<int, string>> Compare(CObjectProp prop1, CObjectProp prop2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			if (prop1.ObjectType != prop2.ObjectType)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1601, "CObjectProp ObjectType does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName }
				});
			}
			if (prop1.PrefabName != prop2.PrefabName)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1602, "CObjectProp PrefabName does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName }
				});
			}
			if (!TileIndex.Compare(prop1.ArrayIndex, prop2.ArrayIndex))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1603, "CObjectProp ArrayIndex does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3]
					{
						"ArrayIndex",
						prop1.ArrayIndex.ToString(),
						prop2.ArrayIndex.ToString()
					}
				});
			}
			if (!CVector3.Compare(prop1.Position, prop2.Position))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1604, "CObjectProp Position does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3]
					{
						"Position",
						prop1.Position.ToString(),
						prop2.Position.ToString()
					}
				});
			}
			if (prop1.Activated != prop2.Activated)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1606, "CObjectProp Activated does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3]
					{
						"Activated",
						prop1.Activated.ToString(),
						prop2.Activated.ToString()
					}
				});
			}
			if (prop1.m_OwnerGuid != prop2.m_OwnerGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1607, "CObjectProp m_OwnerGuid does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3] { "m_OwnerGuid", prop1.m_OwnerGuid, prop2.m_OwnerGuid }
				});
			}
			if (prop1.StartingMapGuid != prop2.StartingMapGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1608, "CObjectProp StartingMapGuid does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3] { "StartingMapGuid", prop1.StartingMapGuid, prop2.StartingMapGuid }
				});
			}
			if (prop1.ActorActivated != prop2.ActorActivated)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1617, "CObjectProp ActorActivated does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3] { "ActorActivated", prop1.ActorActivated, prop2.ActorActivated }
				});
			}
			if (prop1.IgnoreEndOfTurnLooting != prop2.IgnoreEndOfTurnLooting)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1609, "CObjectProp IgnoreEndOfTurnLooting does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3]
					{
						"IgnoreEndOfTurnLooting",
						prop1.IgnoreEndOfTurnLooting.ToString(),
						prop2.IgnoreEndOfTurnLooting.ToString()
					}
				});
			}
			if (prop1.PropActorHasBeenAssigned != prop2.PropActorHasBeenAssigned)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1610, "CObjectProp PropActorHasBeenAssigned does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3]
					{
						"PropActorHasBeenAssigned",
						prop1.PropActorHasBeenAssigned.ToString(),
						prop2.PropActorHasBeenAssigned.ToString()
					}
				});
			}
			if (prop1.OverrideDisallowDestroyAndMove != prop2.OverrideDisallowDestroyAndMove)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1611, "CObjectProp OverrideDisallowDestroyAndMove does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3]
					{
						"OverrideDisallowDestroyAndMove",
						prop1.OverrideDisallowDestroyAndMove.ToString(),
						prop2.OverrideDisallowDestroyAndMove.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(prop1.ConfigPerPartySize, prop2.ConfigPerPartySize))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1612, "CObjectProp ConfigPerPartySize null state does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3]
					{
						"ConfigPerPartySize",
						(prop1.ConfigPerPartySize == null) ? "is null" : "is not null",
						(prop2.ConfigPerPartySize == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (prop1.ConfigPerPartySize.Count != prop2.ConfigPerPartySize.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1613, "CObjectProp total ConfigPerPartySize Count does not match.", new List<string[]>
					{
						new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
						new string[3]
						{
							"ObjectType",
							prop1.ObjectType.ToString(),
							prop2.ObjectType.ToString()
						},
						new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
						new string[3]
						{
							"ConfigPerPartySize Count",
							prop1.ConfigPerPartySize.Count.ToString(),
							prop2.ConfigPerPartySize.Count.ToString()
						}
					});
					break;
				}
				bool flag = false;
				foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item in prop1.ConfigPerPartySize)
				{
					if (!prop2.ConfigPerPartySize.ContainsKey(item.Key))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1614, "CObjectProp ConfigPerPartySize in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a key that is in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
							new string[3]
							{
								"ObjectType",
								prop1.ObjectType.ToString(),
								prop2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
							new string[3]
							{
								"ConfigPerPartySize Key",
								item.Key.ToString(),
								"Missing"
							}
						});
						flag = true;
					}
				}
				foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item2 in prop2.ConfigPerPartySize)
				{
					if (!prop1.ConfigPerPartySize.ContainsKey(item2.Key))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1615, "CObjectProp ConfigPerPartySize in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a key that is in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
							new string[3]
							{
								"ObjectType",
								prop1.ObjectType.ToString(),
								prop2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
							new string[3]
							{
								"ConfigPerPartySize Key",
								"Missing",
								item2.Key.ToString()
							}
						});
						flag = true;
					}
				}
				if (flag)
				{
					break;
				}
				foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item3 in prop1.ConfigPerPartySize)
				{
					if (item3.Value != prop2.ConfigPerPartySize[item3.Key])
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1616, "CObjectProp ConfigPerPartySize has key with differing values.", new List<string[]>
						{
							new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
							new string[3]
							{
								"ObjectType",
								prop1.ObjectType.ToString(),
								prop2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
							new string[3]
							{
								"ConfigPerPartySize Key",
								item3.Key.ToString(),
								item3.Key.ToString()
							},
							new string[3]
							{
								"ConfigPerPartySize Value",
								item3.Value.ToString(),
								prop2.ConfigPerPartySize[item3.Key].ToString()
							}
						});
						flag = true;
					}
				}
				break;
			}
			}
			list.AddRange(PropHealthDetails.Compare(prop1.PropHealthDetails, prop2.PropHealthDetails, isMPCompare));
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(1699, "Exception during CObjectProp compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}

	public void LinkToOtherProp(CObjectProp otherProp)
	{
		if (otherProp == null || otherProp.PropGuid == null)
		{
			throw new Exception($"Trying to link {this} to prop with empty guid");
		}
		if (LinkedPropGuids == null || !LinkedPropGuids.Contains(otherProp.PropGuid))
		{
			if (LinkedPropGuids == null)
			{
				LinkedPropGuids = new List<string>();
			}
			if (otherProp.LinkedPropGuids == null)
			{
				otherProp.LinkedPropGuids = new List<string>();
			}
			LinkedPropGuids.Add(otherProp.PropGuid);
			otherProp.LinkedPropGuids.Add(PropGuid);
		}
	}

	public void UnlinkProp(CObjectProp otherProp)
	{
		if (HasLinkedProps && otherProp.LinkedPropGuids != null && otherProp.LinkedPropGuids.Contains(PropGuid))
		{
			LinkedPropGuids.Remove(otherProp.PropGuid);
			otherProp.LinkedPropGuids.Remove(PropGuid);
		}
	}

	public virtual List<Point> GetTiles()
	{
		Point point = ArrayIndex.ToPoint();
		switch (SpecificPropType)
		{
		case ESpecificPropType.Sarcophagus:
		case ESpecificPropType.MovingPlatform:
		case ESpecificPropType.MovingBoat136:
		case ESpecificPropType.MovingBoat136B:
		case ESpecificPropType.FlotsamTwoHex:
		{
			float rotation = 360f - Rotation.Y;
			CTile tile = ScenarioManager.GetTile(point);
			CAreaEffect areaEffect = GetAreaEffect(PrefabName, PropType.ToString());
			return (from x in CAreaEffect.GetRotatedHexTiles(tile, tile, melee: true, areaEffect.Hexes, rotation)
				select x.m_ArrayIndex).ToList();
		}
		default:
			return new List<Point> { point };
		}
	}

	public virtual void AddTile(Point tile)
	{
	}

	public virtual void ClearTiles()
	{
	}

	public static CAreaEffect GetAreaEffect(string propName, string propType)
	{
		CAreaEffect cAreaEffect = null;
		switch (propType)
		{
		case "ThreeHexObstacle":
			return CardProcessingShared.CreateArea("0,0,R|-1,1,R|1,1,R", "Editor", "Editor");
		case "ThreeHexCurvedObstacle":
			return CardProcessingShared.CreateArea("0,0,R|-2,0,R|1,1,R", "Editor", "Editor");
		case "ThreeHexStraightObstacle":
			return CardProcessingShared.CreateArea("0,0,R|-2,0,R|2,0,R", "Editor", "Editor");
		default:
			if (!(propName == ESpecificPropType.Sarcophagus.ToString()))
			{
				if (propName == ESpecificPropType.MovingPlatform.ToString())
				{
					return CardProcessingShared.CreateArea("0,0,R|-2,0,R", "Editor", "Editor");
				}
				if (propName == ESpecificPropType.MovingBoat136.ToString())
				{
					return CardProcessingShared.CreateArea("0,0,R|-2,0,R", "Editor", "Editor");
				}
				if (propName == ESpecificPropType.MovingBoat136B.ToString())
				{
					return CardProcessingShared.CreateArea("0,0,R|-2,0,R", "Editor", "Editor");
				}
				if (propName == ESpecificPropType.FlotsamTwoHex.ToString())
				{
					return CardProcessingShared.CreateArea("0,0,R|-2,0,R", "Editor", "Editor");
				}
				return CardProcessingShared.CreateArea("0,0,R", "Editor", "Editor");
			}
			goto case "TwoHexObstacle";
		case "TwoHexObstacle":
			return CardProcessingShared.CreateArea("0,0,R|0,1,R", "Editor", "Editor");
		}
	}

	public virtual bool CanBeRemovedByPlayers()
	{
		if (OverrideDisallowDestroyAndMove)
		{
			return false;
		}
		if (SpecificPropType == ESpecificPropType.Sarcophagus || (PropHealthDetails != null && PropHealthDetails.HasHealth))
		{
			return false;
		}
		return true;
	}

	public virtual bool TreatAsNotFeatureless()
	{
		ESpecificPropType specificPropType = SpecificPropType;
		if (specificPropType == ESpecificPropType.MovingPlatform || (uint)(specificPropType - 81) <= 1u || (uint)(specificPropType - 108) <= 2u)
		{
			return true;
		}
		return false;
	}

	public virtual bool TreatAsFeatureless()
	{
		if (SpecificPropType == ESpecificPropType.DemonCorpsePile)
		{
			return true;
		}
		return false;
	}

	public void SetGUID(string guid)
	{
		PropGuid = guid;
	}
}
