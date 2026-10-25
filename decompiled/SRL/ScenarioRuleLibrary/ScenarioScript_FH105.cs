using System.Linq;
using Newtonsoft.Json;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH105 : ScenarioScript
{
	private const string _VFXPropNameMildWind = "WindCorridorMild105";

	private const string _VFXPropNameIntenseWind = "WindCorridorIntense105";

	private int m_CurrentAirFlowRow = 5;

	private bool m_Pushed;

	private bool m_WindActive = true;

	private CScenarioModifier _mod1;

	private CScenarioModifier _mod2;

	private CScenarioModifier _mod3;

	private CScenarioModifier _mod4;

	private CObjectProp m_vfxPropMildWind;

	protected override bool Initialize()
	{
		_mod1 = GetScenarioModifier("Mod1");
		_mod2 = GetScenarioModifier("Mod2");
		_mod3 = GetScenarioModifier("Mod3");
		_mod4 = GetScenarioModifier("Mod4");
		return base.Initialize();
	}

	protected override void TickPhaseInternal()
	{
		Log.SimpleInfo("TickPhaseInternal");
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
			m_Pushed = false;
			UpdateWindVFX();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
		{
			if (!m_WindActive && _tickActor is CPlayerActor actor)
			{
				GameState.ActorBeenDamaged(actor, 1, checkIfPlayerCanAvoidDamage: true);
			}
			if (m_Pushed || !m_WindActive)
			{
				break;
			}
			PlayInstenceWindVFX();
			for (int num = 23; num >= 9; num--)
			{
				foreach (CActor livingActor in ScenarioManager.Tiles[num, m_CurrentAirFlowRow].LivingActors)
				{
					if (livingActor.Type == CActor.EType.Enemy)
					{
						continue;
					}
					CTile cTile = ScenarioManager.Tiles[num + 1, m_CurrentAirFlowRow];
					if ((!cTile.IsWalkable && !livingActor.FlyingAfterConditions) || cTile.LivingActors.Count() != 0)
					{
						continue;
					}
					livingActor.ArrayIndex = cTile.m_ArrayIndex;
					CActorHasPushed_MessageData cActorHasPushed_MessageData = new CActorHasPushed_MessageData(livingActor);
					cActorHasPushed_MessageData.m_PushAbility = null;
					cActorHasPushed_MessageData.m_ActorBeingPushed = livingActor;
					ScenarioRuleClient.MessageHandler(cActorHasPushed_MessageData);
					if (livingActor.FlyingAfterConditions)
					{
						continue;
					}
					foreach (CObjectProp item in cTile.m_Props.ToList())
					{
						item.AutomaticActivate(livingActor);
					}
				}
			}
			m_CurrentAirFlowRow = ((m_CurrentAirFlowRow > 3) ? (m_CurrentAirFlowRow - 1) : 5);
			m_Pushed = true;
			break;
		}
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 104);
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			break;
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		if (!(_tickRoomRevealed.RoomName == "Room_2") || !m_WindActive)
		{
			return;
		}
		m_WindActive = false;
		_mod1.Hide();
		_mod2.Hide();
		_mod3.Show();
		_mod4.Show();
		foreach (CObjectProp item in _tickRoomRevealed.Props.ToList())
		{
			if (item is CObjectHazardousTerrain)
			{
				float y = 0.5f * (float)SharedClient.GlobalRNG.Next(180) + 0.75f * (float)SharedClient.GlobalRNG.Next(360);
				CVector3 rotation = new CVector3(0f, y, 0f);
				CObjectQuestItem cObjectQuestItem = new CObjectQuestItem("LavaLootToken", ScenarioManager.ObjectImportType.CarryableQuestItem, new TileIndex(item.PropTile.m_ArrayIndex), null, rotation, null, item.PropTile.m_HexMap.MapGuid);
				cObjectQuestItem.PropCustomDescriptionLocKey = "FH_105_Elemental_Alloy_Piece_D";
				item.PropTile.SpawnProp(cObjectQuestItem);
			}
		}
		UpdateWindVFX();
	}

	private void UpdateWindVFX()
	{
		CTile cTile = ScenarioManager.Tiles[10, m_CurrentAirFlowRow];
		if (m_vfxPropMildWind == null)
		{
			m_vfxPropMildWind = ScenarioManager.CurrentScenarioState.Props.Find((CObjectProp p) => p.PrefabName == "WindCorridorMild105");
		}
		if (m_vfxPropMildWind != null && (!m_WindActive || m_vfxPropMildWind.PropTile != cTile || m_vfxPropMildWind.Rotation == null || m_vfxPropMildWind.Rotation.Y != 180f))
		{
			m_vfxPropMildWind.DestroyProp();
		}
		m_vfxPropMildWind = cTile.m_Props.Find((CObjectProp p) => p.PrefabName == "WindCorridorMild105");
		if (m_vfxPropMildWind == null && m_WindActive)
		{
			cTile.SpawnProp(m_vfxPropMildWind = new CObjectProp("WindCorridorMild105", ScenarioManager.ObjectImportType.VFXProp, new TileIndex(cTile.m_ArrayIndex), null, new CVector3(0f, 180f, 0f), null, cTile.m_HexMap.MapGuid));
		}
	}

	private void PlayInstenceWindVFX()
	{
		CTile cTile = ScenarioManager.Tiles[10, m_CurrentAirFlowRow];
		ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
		{
			m_VFXPropType = "WindCorridorIntense105",
			m_ArrayIndex = cTile.m_ArrayIndex,
			m_Rotation = new CVector3(0f, 180f, 0f)
		});
		CWaitingForProlongedSpawns_MessageData message = new CWaitingForProlongedSpawns_MessageData();
		ScenarioRuleClient.MessageHandler(message);
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(m_CurrentAirFlowRow);
		string text2 = JsonConvert.SerializeObject(m_WindActive);
		string text3 = JsonConvert.SerializeObject(new string[2] { text, text2 });
		Trace("[ffa000]Serialize: " + text3 + "[-]", null, 173);
		return text3;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 179);
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			m_CurrentAirFlowRow = JsonConvert.DeserializeObject<int>(array[0]);
			m_WindActive = JsonConvert.DeserializeObject<bool>(array[1]);
		}
		UpdateWindVFX();
	}
}
