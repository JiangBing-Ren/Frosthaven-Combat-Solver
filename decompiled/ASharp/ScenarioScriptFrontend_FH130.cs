using System.Collections.Generic;
using System.Linq;
using FH.ApparanceBaking;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

internal class ScenarioScriptFrontend_FH130 : ScenarioScriptFrontend
{
	private ScenarioScript_FH130 _script130 => (ScenarioScript_FH130)_script;

	protected override void InitializeInternal()
	{
	}

	private CTile FindNextTile(CTile tile)
	{
		return ScenarioManager.GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ELeft);
	}

	protected override void TickInternal()
	{
		if (!_script130.startedIceRowMovement)
		{
			return;
		}
		Dictionary<CObjectProp, ScenarioScript_FH130.IceMoveParams> movements = _script130.movements;
		bool flag = true;
		foreach (ScenarioScript_FH130.IceMoveParams movement in movements.Values)
		{
			bool flag2 = false;
			if (_script130.isPickingWhereToDropAgateFromDamage || movements.Values.Any((ScenarioScript_FH130.IceMoveParams x) => x.actorsToDamageHazardous.Count > 0))
			{
				flag = false;
				continue;
			}
			CTile destTile = movement.destTile;
			GameObject gameObject = Choreographer.FindPropGameObject(movement.iceProp);
			if (gameObject != null)
			{
				CTile cTile = FindNextTile(movement.iceProp.PropTile);
				if (cTile == null || cTile.m_ArrayIndex.X < destTile.m_Hex.ArrayIndex.X)
				{
					cTile = destTile;
				}
				string tileGuid = cTile.m_Hex.TileGuid;
				GameObject gameObject2 = EnvironmentLoader.Instance.GetTile(tileGuid) ?? GameObject.Find(tileGuid);
				if (gameObject2 == null)
				{
					continue;
				}
				foreach (CObjectProp item in movement.propsToMove)
				{
					GameObject gameObject3;
					if (item == movement.iceProp)
					{
						gameObject3 = gameObject;
					}
					else
					{
						gameObject3 = Choreographer.FindPropGameObject(item);
						if (gameObject3.transform.position.x < gameObject.transform.position.x)
						{
							continue;
						}
					}
					if (gameObject3 == null)
					{
						continue;
					}
					gameObject3.transform.position = Vector3.MoveTowards(gameObject3.transform.position, gameObject2.transform.position, 2f * Time.deltaTime);
					if (!(Vector3.Distance(gameObject3.transform.position, gameObject2.transform.position) <= 0.002f))
					{
						continue;
					}
					if (item.ArrayIndex.ToPoint() != cTile.m_ArrayIndex)
					{
						item.PropTile.DelProp(item);
						item.SetLocation(new TileIndex(cTile.m_ArrayIndex), null, item.Rotation);
						item.PropTile.AddProp(item);
						foreach (CActor item2 in movement.actorsToMove)
						{
							item2.ArrayIndex = cTile.m_ArrayIndex;
						}
						if (item == movement.iceProp && movement.isIceMovement)
						{
							foreach (CActor otherActor in cTile.LivingActors.Where((CActor x) => !movement.actorsToMove.Contains(x)))
							{
								if (!(otherActor is CEnemyActor))
								{
									movement.actorsToDamageHazardous.Add(otherActor);
									if (!movement.actorsToMove.Any((CActor x) => x.ArrayIndex == otherActor.ArrayIndex))
									{
										movement.actorsToMove.Add(otherActor);
									}
								}
							}
						}
					}
					if (destTile == cTile)
					{
						flag2 = true;
					}
				}
				foreach (CActor item3 in movement.actorsToMove)
				{
					GameObject gameObject4 = Choreographer.s_Choreographer.FindClientActorGameObject(item3);
					if (!(gameObject4 == null))
					{
						ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(gameObject4);
						gameObject4.transform.position = gameObject.transform.position;
						actorBehaviour.SetLocoTarget(gameObject.transform.position, jump: false, null, forceMove: false, dontTeleport: false, slideWithoutAnim: true);
					}
				}
			}
			if (!flag2)
			{
				flag = false;
			}
		}
		if (flag)
		{
			_script130.startedIceRowMovement = false;
			CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.Null, new CAbilityFilterContainer(), isMonster: false, isTargetedAbility: false);
			cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			cAbility.MiscAbilityData.AutotriggerAbility = true;
			cAbility.MiscAbilityData.IgnoreStun = true;
			cAbility.SkipAnim = true;
			cAbility.SetMiscVar("ContinueIceRowMovement", true);
			PhaseManager.StartAbilities(cAbility, ScenarioManager.Scenario.PlayerActors[0], stackInsteadOfInject: true);
		}
	}
}
