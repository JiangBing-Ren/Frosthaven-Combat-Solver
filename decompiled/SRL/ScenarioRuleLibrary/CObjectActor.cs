using System;
using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class CObjectActor : CEnemyActor
{
	private CObjectProp m_PropAttachedTo;

	private bool _doesNotBlock;

	private bool _ignoreActorCollision;

	public bool TreatAsObstacle;

	public CObjectProp AttachedProp => m_PropAttachedTo;

	public bool IsAttachedToProp => m_PropAttachedTo != null;

	public override bool DoesNotBlock
	{
		get
		{
			if (!base.DoesNotBlock)
			{
				return _doesNotBlock;
			}
			return true;
		}
	}

	public override bool IgnoreActorCollision
	{
		get
		{
			if (!base.IgnoreActorCollision)
			{
				return _ignoreActorCollision;
			}
			return true;
		}
	}

	public override List<CAbility.EAbilityType> Immunities
	{
		get
		{
			List<CAbility.EAbilityType> immunities = base.Immunities;
			if (IsAttachedToProp && AttachedProp.IsImmuneToDamage)
			{
				CAbility.EAbilityType[] array = CEnemyActor.damageAbilityTypes;
				foreach (CAbility.EAbilityType item in array)
				{
					if (!immunities.Contains(item))
					{
						immunities.Add(item);
					}
				}
			}
			return immunities;
		}
	}

	public override bool SkipVisualsAndGUI
	{
		get
		{
			if (m_PropAttachedTo != null && m_PropAttachedTo.SkipVisuals)
			{
				return !m_PropAttachedTo.HasInitiative;
			}
			return false;
		}
	}

	public CObjectActor(Point startPosition, CObjectClass objectClass, EType type, string actorGuid, int currentHealth, int maxHealth, int level, int standeeID, bool initial, int chosenModelIndex)
		: base(startPosition, objectClass, type, isSummon: false, string.Empty, actorGuid, currentHealth, maxHealth, level, standeeID, initial, chosenModelIndex)
	{
	}

	public override int Initiative()
	{
		if (InitiativeLockedDuringRound.HasValue)
		{
			return InitiativeLockedDuringRound.Value;
		}
		if (MonsterClass.RoundAbilityCard != null)
		{
			return MonsterClass.RoundAbilityCard.Initiative;
		}
		if (MonsterClass.FocusingInitiative.HasValue)
		{
			return MonsterClass.FocusingInitiative.Value;
		}
		if (PhaseManager.PhaseType < CPhase.PhaseType.MonsterClassesSelectAbilityCards)
		{
			return 0;
		}
		return 99;
	}

	public override string ActorLocKey()
	{
		if (IsAttachedToProp)
		{
			if (string.IsNullOrEmpty(m_PropAttachedTo.PropHealthDetails?.CustomLocKey))
			{
				return m_PropAttachedTo.PrefabName;
			}
			return m_PropAttachedTo.PropHealthDetails.CustomLocKey;
		}
		return base.ActorLocKey();
	}

	public void SetIgnoreActorCollision(bool enable)
	{
		_doesNotBlock = enable;
		_ignoreActorCollision = enable;
	}

	public override bool OnDeath(CActor targetingActor, ECauseOfDeath causeOfDeath, out bool startedOnDeathAbility, bool fromDeathAbilityComplete = false, CAbility causeOfDeathAbility = null, CAttackSummary.TargetSummary attackSummary = null)
	{
		if (IsAttachedToProp)
		{
			string text;
			if (targetingActor is CPlayerActor cPlayerActor)
			{
				text = cPlayerActor.CharacterClass.ID;
			}
			else
			{
				text = ((!(targetingActor is CHeroSummonActor { Summoner: var summoner })) ? "" : summoner?.CharacterClass.ID);
			}
			string characterID = text;
			SEventLogMessageHandler.AddEventLogMessage(new SEventObjectProp(ESESubTypeObjectProp.Died, m_PropAttachedTo.ObjectType, m_PropAttachedTo.PropGuid, m_PropAttachedTo.PrefabName, characterID, "Died", m_PropAttachedTo.OwnerGUID, "", new List<CMap>(), m_PropAttachedTo.ArrayIndex.ToPoint()));
			switch (m_PropAttachedTo.PropHealthDetails.DeathAction)
			{
			case PropHealthDetails.EDeathAction.Destruct:
				if (m_PropAttachedTo is CObjectObstacle)
				{
					m_PropAttachedTo.DestroyProp(causeOfDeathAbility?.SpawnDelay ?? 0f);
				}
				break;
			case PropHealthDetails.EDeathAction.Activate:
			{
				if (m_PropAttachedTo is CObjectDoor cObjectDoor)
				{
					cObjectDoor.SetExtraLockState(lockedStateToSet: false, openDoorIfUnlocked: true, targetingActor);
					break;
				}
				bool result = base.OnDeath(targetingActor, causeOfDeath, out startedOnDeathAbility, fromDeathAbilityComplete, causeOfDeathAbility);
				m_PropAttachedTo.Activate(targetingActor);
				return result;
			}
			}
		}
		return base.OnDeath(targetingActor, causeOfDeath, out startedOnDeathAbility, fromDeathAbilityComplete, causeOfDeathAbility);
	}

	protected override void OnHealthChanged(int oldHealth)
	{
		base.OnHealthChanged(oldHealth);
		if (IsAttachedToProp)
		{
			m_PropAttachedTo.PropHealthDetails.CurrentHealth = Health;
		}
	}

	public new CObjectActor Clone()
	{
		CObjectActor cObjectActor = (CObjectActor)MemberwiseClone();
		cObjectActor.Inventory.InventoryActor = cObjectActor;
		return cObjectActor;
	}

	public void SetAttachedToProp(CObjectProp propAttachedTo)
	{
		if (propAttachedTo.PropHealthDetails == null || !propAttachedTo.PropHealthDetails.HasHealth)
		{
			Log.Warning($"Unable to create dummy ObjectActor for {propAttachedTo.ObjectType.ToString()} prop with GUID {propAttachedTo.PropGuid}, The prop is not configured for health Correctly");
			return;
		}
		m_PropAttachedTo = propAttachedTo;
		Type = propAttachedTo.PropHealthDetails.ActorType;
	}

	public void ClearAttachedProp()
	{
		m_PropAttachedTo = null;
	}

	public override bool CanFitIntoHex(Point targetPos, int angle)
	{
		if (AttachedProp is CObjectObstacle cObjectObstacle)
		{
			CAbilityMove cAbilityMove = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability as CAbilityMove;
			if (cObjectObstacle.PathingBlockers != null)
			{
				foreach (Point blocker in from x in cObjectObstacle.CalcPathingBlockers(targetPos, angle)
					select x.ToPoint())
				{
					if (!GameState.IsTileInBounds(blocker))
					{
						continue;
					}
					CTile cTile = ScenarioManager.Tiles[blocker.X, blocker.Y];
					if (cTile == null)
					{
						return false;
					}
					if (cTile.m_Spawners.Count > 0)
					{
						return false;
					}
					if (cTile.m_Hex != null && cTile.m_Hex.Flags != 0 && !(cTile.m_Props.SingleOrDefault((CObjectProp x) => x is CObjectDoor) is CObjectDoor { DoorIsOpen: not false }))
					{
						return false;
					}
					if (cAbilityMove != null && cAbilityMove.TargetingActor is CPlayerActor)
					{
						List<Point> list = CAbilityMove.AllPlayerArrayIndexOnPathIncludingRepeats.ToList();
						foreach (Point item in cAbilityMove.TilesMoved)
						{
							list.Remove(item);
						}
						if (list.Contains(blocker))
						{
							return false;
						}
					}
					if (!cTile.m_Props.Contains(AttachedProp) && !cTile.LivingActors.Contains(this) && !cObjectObstacle.PathingBlockers.Any((TileIndex x) => x.ToPoint() == blocker))
					{
						if (cTile.LivingActors.Any())
						{
							return false;
						}
						if (cTile.m_Props.Any((CObjectProp x) => x is CObjectObstacle || x is CObjectDifficultTerrain || x is CObjectHazardousTerrain))
						{
							return false;
						}
						CNode node = ScenarioManager.PathFinder.GetNode(blocker);
						if (node == null || node.Blocked)
						{
							return false;
						}
					}
				}
			}
		}
		return base.CanFitIntoHex(targetPos, angle);
	}

	public bool CalcNeighbourTileAndRotation(Point targetPos, out Point newPos, out int newAngle)
	{
		newPos = ArrayIndex;
		newAngle = (int)AttachedProp.Rotation.Y;
		if (AttachedProp is CObjectObstacle cObjectObstacle)
		{
			List<(Point, int, int, int)> list = new List<(Point, int, int, int)>();
			Point point = Point.OddRToAxial(ArrayIndex);
			Point point2 = Point.OddRToAxial(targetPos);
			for (int i = (int)cObjectObstacle.Rotation.Y; (float)i <= cObjectObstacle.Rotation.Y + 360f; i += 60)
			{
				foreach (Point item in from x in cObjectObstacle.PathingBlockers
					select x.ToPoint() into x
					orderby Point.HexDist(x, targetPos)
					select x)
				{
					Point point3 = Point.OddRToAxial(item.Rotate(ArrayIndex, -(int)cObjectObstacle.Rotation.Y));
					Point point4 = point - point3;
					Point point5 = Point.AxialToOddR(Point.RotateAxial(point2 + point4, point2, i));
					if (CanFitIntoHex(point5, i))
					{
						list.Add((point5, i, Point.HexDist(point5, ArrayIndex), Math.Abs(i - (int)cObjectObstacle.Rotation.Y)));
					}
				}
			}
			if (list.Count > 0)
			{
				(newPos, newAngle, _, _) = list.OrderBy(((Point position, int angle, int tilesMoved, int angleChange) x) => x.tilesMoved).ThenBy(((Point position, int angle, int tilesMoved, int angleChange) x) => x.angleChange).FirstOrDefault();
				if (!(newPos != ArrayIndex))
				{
					return newAngle != (int)AttachedProp.Rotation.Y;
				}
				return true;
			}
		}
		else if (AttachedProp != null)
		{
			newPos = targetPos;
			return true;
		}
		return false;
	}

	public int GetValidRotation(Point targetPos)
	{
		if (AttachedProp is CObjectObstacle cObjectObstacle)
		{
			for (float num = cObjectObstacle.Rotation.Y; num <= cObjectObstacle.Rotation.Y + 360f; num += 60f)
			{
				int num2 = (int)num % 360;
				if (CanFitIntoHex(targetPos, num2))
				{
					return num2;
				}
			}
		}
		else if (AttachedProp != null)
		{
			return (int)AttachedProp.Rotation.Y;
		}
		return -1;
	}

	public void SyncPropLocation()
	{
		if (AttachedProp != null && !AttachedProp.ArrayIndex.ToPoint().Equals(ArrayIndex))
		{
			AttachedProp.PropTile.DelProp(AttachedProp, recursiveTiles: true);
			AttachedProp.SetLocation(new TileIndex(ArrayIndex), null, AttachedProp.Rotation);
			AttachedProp.PropTile.AddProp(AttachedProp, recursiveTiles: true);
		}
	}

	public override void LoadFields(ActorState state)
	{
		base.LoadFields(state);
		if (state is ObjectState objectState)
		{
			TreatAsObstacle = objectState.TreatAsObstacle;
		}
	}
}
