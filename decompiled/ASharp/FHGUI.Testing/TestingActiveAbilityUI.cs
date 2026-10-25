using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AStar;
using FH.YML;
using FHGUI.Common;
using FHGUI.Elements;
using FHRL;
using I2.Loc;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary;
using SharedLibrary.Client;
using UnityEngine;

namespace FHGUI.Testing;

public class TestingActiveAbilityUI : ListContainerBase<UIGeneralInfoSection, object, object>
{
	[Flags]
	private enum CharacterClass
	{
		None = 1,
		Drifter = 2,
		Blinkblade = 4,
		Bannerspear = 8,
		Deathwalker = 0x10,
		Boneshaper = 0x20,
		Geminate = 0x40,
		Infuser = 0x80,
		Pyroclast = 0x100,
		Shattersong = 0x200,
		PainConduit = 0x400,
		Snowdancer = 0x800,
		FrozenFist = 0x1000,
		HIVE = 0x2000,
		MetalMosaic = 0x4000,
		Deepwraith = 0x8000,
		CrashingTide = 0x10000,
		Trapper = 0x20000,
		All = ~None
	}

	private CActor _caster;

	private CActor _target;

	private ScenarioRuleLibrary.ScenarioState _scenario;

	private readonly Dictionary<CharacterClass, string> _characterToId = new Dictionary<CharacterClass, string>
	{
		{
			CharacterClass.Drifter,
			"DrifterID"
		},
		{
			CharacterClass.Blinkblade,
			"BlinkbladeID"
		},
		{
			CharacterClass.Bannerspear,
			"BannerSpearID"
		},
		{
			CharacterClass.Deathwalker,
			"DeathwalkerID"
		},
		{
			CharacterClass.Boneshaper,
			"BoneshaperID"
		},
		{
			CharacterClass.Geminate,
			"GeminateID"
		},
		{
			CharacterClass.Infuser,
			"InfuserID"
		},
		{
			CharacterClass.Pyroclast,
			"PyroclastID"
		},
		{
			CharacterClass.Shattersong,
			"ShattersongID"
		},
		{
			CharacterClass.PainConduit,
			"PainConduitID"
		},
		{
			CharacterClass.Snowdancer,
			"SnowdancerID"
		},
		{
			CharacterClass.FrozenFist,
			"FrozenFistID"
		},
		{
			CharacterClass.HIVE,
			"HIVEID"
		},
		{
			CharacterClass.MetalMosaic,
			"MetalMosaicID"
		},
		{
			CharacterClass.Deepwraith,
			"DeepwraithID"
		},
		{
			CharacterClass.CrashingTide,
			"CrashingTideID"
		},
		{
			CharacterClass.Trapper,
			"TrapperID"
		}
	};

	[SerializeField]
	private CharacterClass _characters = CharacterClass.All;

	public bool ShowItems = true;

	public bool ShowSummons = true;

	public bool ShowMonsters = true;

	public bool ShowScenarios = true;

	private List<string> _scenarioIds = new List<string>();

	private readonly List<CCustomLevelData> _scenarioData = new List<CCustomLevelData>();

	private IEnumerator Start()
	{
		yield return UIInfoTools.InitInstanceCrt();
		yield return GlobalSettings.InitInstanceCrt();
		DataLoadingFlags.HandleAbilitiesOnMainThread = true;
		MonsterClassManager.Load();
		CharacterClassManager.Load();
		_scenario = CreateMockScenario();
		ScenarioManager.Load(_scenario, firstLoad: true);
		ScenarioManager.CurrentScenarioState.ScenarioSetup = FHScenarioSetup.MakeDummy(new SharedLibrary.Random());
		ScenarioRuleClient.SetMessageHandler((CMessageData _) =>
		{
		});
		if (ShowScenarios)
		{
			_scenarioIds = YMLDefRepository.Repo.FHScenarios.Items.Select((ScenarioDef s) => s.LevelID).Distinct().ToList();
			_scenarioIds.Sort();
			foreach (string scenarioId in _scenarioIds)
			{
				CCustomLevelData customLevel = ScenarioRuleClient.SRLYML.GetCustomLevel(scenarioId);
				if (customLevel != null)
				{
					_scenarioData.Add(customLevel);
				}
			}
		}
		CreateMockActors();
		List<object> list = new List<object>();
		string[] classes = (from c in (CharacterClass[])Enum.GetValues(typeof(CharacterClass))
			where c != CharacterClass.All && c != CharacterClass.None && _characters.HasFlag(c)
			select _characterToId[c]).ToArray();
		list.AddRange(CharacterClassManager.Classes.Where((CCharacterClass c) => classes.Any((string cc) => cc.Contains(c.CharacterID, StringComparison.InvariantCultureIgnoreCase))));
		if (ShowSummons)
		{
			list.AddRange(CharacterClassManager.HeroSummonClasses);
		}
		if (ShowMonsters)
		{
			list.AddRange(MonsterClassManager.MonsterAndObjectClasses);
		}
		if (ShowItems)
		{
			list.AddRange(from s in ScenarioRuleClient.SRLYML.ItemCards
				select s.GetItem into s
				where GetActiveBonuses(s).Any()
				select s);
		}
		if (ShowScenarios)
		{
			list.AddRange(from s in _scenarioData
				select s.ScenarioState into s
				where GetActiveBonuses(s).Any()
				select s);
		}
		Init(null, list);
	}

	protected override void InitElement(UIGeneralInfoSection section, object obj, object listParams)
	{
		try
		{
			List<ActiveAbilityDTO> bonuses = new List<ActiveAbilityDTO>();
			if (obj is CCharacterClass cCharacterClass)
			{
				InitElement(section, cCharacterClass, out bonuses);
				section.name = cCharacterClass.CharacterYML.ID;
			}
			else if (obj is CMonsterClass cMonsterClass)
			{
				InitElement(section, cMonsterClass, out bonuses);
				section.name = cMonsterClass.MonsterYML.ID;
			}
			else if (obj is CHeroSummonClass cHeroSummonClass)
			{
				InitElement(section, cHeroSummonClass, out bonuses);
				section.name = cHeroSummonClass.ID;
			}
			else if (obj is CItem cItem)
			{
				InitElement(section, cItem, out bonuses);
				section.name = cItem.Name;
			}
			else if (obj is ScenarioAbilitiesYMLData scenarioAbilitiesYMLData)
			{
				InitElement(section, scenarioAbilitiesYMLData, out bonuses);
				section.name = scenarioAbilitiesYMLData.FileName;
			}
			else if (obj is ScenarioRuleLibrary.ScenarioState scenarioState)
			{
				InitElement(section, scenarioState, out bonuses);
				section.name = scenarioState.ID;
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	private void InitElement(UIGeneralInfoSection section, CCharacterClass character, out List<ActiveAbilityDTO> bonuses)
	{
		section.Label.Text.text = character.CharacterID;
		bonuses = GetActiveBonuses(character);
		List<ActiveAbilityDTO> b = new List<ActiveAbilityDTO>(bonuses);
		section.Fill(bonuses.Any(), (UIActiveAbilityListContainer container) =>
		{
			container.Init(null, b);
		});
	}

	private void InitElement(UIGeneralInfoSection section, CMonsterClass monster, out List<ActiveAbilityDTO> bonuses)
	{
		section.Label.Text.text = monster.ID;
		bonuses = GetActiveBonuses(monster);
		List<ActiveAbilityDTO> b = new List<ActiveAbilityDTO>(bonuses);
		section.Fill(bonuses.Any(), (UIActiveAbilityListContainer container) =>
		{
			container.Init(null, b);
		});
	}

	private void InitElement(UIGeneralInfoSection section, CItem item, out List<ActiveAbilityDTO> bonuses)
	{
		section.Label.Text.text = LocalizationManager.GetTranslation(item.Name);
		bonuses = GetActiveBonuses(item);
		List<ActiveAbilityDTO> b = new List<ActiveAbilityDTO>(bonuses);
		section.Fill(bonuses.Any(), (UIActiveAbilityListContainer container) =>
		{
			container.Init(null, b);
		});
	}

	private void InitElement(UIGeneralInfoSection section, CHeroSummonClass summon, out List<ActiveAbilityDTO> bonuses)
	{
		section.Label.Text.text = summon.SummonYML.ID;
		bonuses = GetActiveBonuses(summon);
		List<ActiveAbilityDTO> b = new List<ActiveAbilityDTO>(bonuses);
		section.Fill(bonuses.Any(), (UIActiveAbilityListContainer container) =>
		{
			container.Init(null, b);
		});
	}

	private void InitElement(UIGeneralInfoSection section, ScenarioAbilitiesYMLData scenario, out List<ActiveAbilityDTO> bonuses)
	{
		section.Label.Text.text = scenario.ScenarioAbilityID;
		bonuses = GetActiveBonuses(scenario);
		List<ActiveAbilityDTO> b = new List<ActiveAbilityDTO>(bonuses);
		section.Fill(bonuses.Any(), (UIActiveAbilityListContainer container) =>
		{
			container.Init(null, b);
		});
	}

	private void InitElement(UIGeneralInfoSection section, ScenarioRuleLibrary.ScenarioState scenario, out List<ActiveAbilityDTO> bonuses)
	{
		section.Label.Text.text = scenario.ID;
		bonuses = GetActiveBonuses(scenario);
		List<ActiveAbilityDTO> b = new List<ActiveAbilityDTO>(bonuses);
		section.Fill(bonuses.Any(), (UIActiveAbilityListContainer container) =>
		{
			container.Init(null, b);
		});
	}

	private void OnDestroy()
	{
		ScenarioManager.Unload();
		_scenario = null;
		_caster = null;
		_target = null;
	}

	private ScenarioRuleLibrary.ScenarioState CreateMockScenario()
	{
		List<CMap> list = new List<CMap>
		{
			new CMap("Room", EMapType.A, "ScenarioRoom", null, "")
			{
				MapTiles = 
				{
					new CMapTile(edgeTile: false, new TileIndex(0, 0), GloomUtility.VToCV(new Vector3(0f, 0f, 0f)), SharedClient.GlobalRNG),
					new CMapTile(edgeTile: false, new TileIndex(0, 1), GloomUtility.VToCV(new Vector3(0f, 0f, 1f)), SharedClient.GlobalRNG),
					new CMapTile(edgeTile: false, new TileIndex(1, 0), GloomUtility.VToCV(new Vector3(1f, 0f, 0f)), SharedClient.GlobalRNG),
					new CMapTile(edgeTile: false, new TileIndex(1, 1), GloomUtility.VToCV(new Vector3(1f, 0f, 1f)), SharedClient.GlobalRNG)
				}
			}
		};
		CMap.SetChildren(list);
		foreach (CMap item in list)
		{
			item.Revealed = true;
		}
		ApparanceStyle style = new ApparanceStyle(ScenarioPossibleRoom.EBiome.Crypt);
		ScenarioRuleLibrary.ScenarioState scenarioState = new ScenarioRuleLibrary.ScenarioState("Name", "Description", "ID", SharedClient.GlobalRNG.Next(), 1, EScenarioType.Custom, "FileName", new List<CObjective>(), new List<CObjective>(), new List<CScenarioModifier>(), style, new List<string>(), new List<string>())
		{
			Maps = list
		};
		typeof(ScenarioRuleLibrary.ScenarioState).GetProperty("Width", BindingFlags.Instance | BindingFlags.Public).SetValue(scenarioState, 1);
		typeof(ScenarioRuleLibrary.ScenarioState).GetProperty("Height", BindingFlags.Instance | BindingFlags.Public).SetValue(scenarioState, 1);
		return scenarioState;
	}

	private void CreateMockActors()
	{
		_caster = new CActor(CActor.EType.Ally, CharacterClassManager.Classes[0], new Point(0, 0), 1, 1, 1, "ID1", 0);
		_target = new CActor(CActor.EType.Ally, CharacterClassManager.Classes[1], new Point(1, 1), 1, 1, 1, "ID2", 0);
	}

	private List<ActiveAbilityDTO> GetActiveBonuses(CCharacterClass character)
	{
		List<ActiveAbilityDTO> list = new List<ActiveAbilityDTO>();
		foreach (CAbilityCard item in character.AbilityCardsPool)
		{
			CAbility[] array = CAbilityCard.GetAllAbilities(item).ToArray();
			foreach (CAbility ability in array)
			{
				if (GetActiveBonus(ability, item, out var bonus))
				{
					if (bonus.Ability.ActiveBonusData.GetMiscVar("OverrideDescForTarget").isUnknown)
					{
						list.Add(new ActiveAbilityDTO(bonus, _caster));
						continue;
					}
					list.Add(new ActiveAbilityDTO(bonus, _caster));
					list.Add(new ActiveAbilityDTO(bonus, _target));
				}
			}
		}
		return list;
	}

	private List<ActiveAbilityDTO> GetActiveBonuses(CMonsterClass monster)
	{
		List<ActiveAbilityDTO> list = new List<ActiveAbilityDTO>();
		foreach (CMonsterAbilityCard item in monster.AbilityCardsPool)
		{
			CAbility[] array = CAbilityCard.GetActionAbilities(item.Action).ToArray();
			foreach (CAbility ability in array)
			{
				if (GetActiveBonus(ability, item, out var bonus))
				{
					if (bonus.Ability.ActiveBonusData.GetMiscVar("OverrideDescForTarget").isUnknown)
					{
						list.Add(new ActiveAbilityDTO(bonus, _caster));
						continue;
					}
					list.Add(new ActiveAbilityDTO(bonus, _caster));
					list.Add(new ActiveAbilityDTO(bonus, _target));
				}
			}
		}
		return list;
	}

	private List<ActiveAbilityDTO> GetActiveBonuses(CItem item)
	{
		List<ActiveAbilityDTO> list = new List<ActiveAbilityDTO>();
		foreach (CAbility allAbility in CItem.GetAllAbilities(item.YMLData.GetItem))
		{
			if (GetActiveBonus(allAbility, item, out var bonus))
			{
				if (bonus.Ability.ActiveBonusData.GetMiscVar("OverrideDescForTarget").isUnknown)
				{
					list.Add(new ActiveAbilityDTO(bonus, _caster));
					continue;
				}
				list.Add(new ActiveAbilityDTO(bonus, _caster));
				list.Add(new ActiveAbilityDTO(bonus, _target));
			}
		}
		return list;
	}

	private List<ActiveAbilityDTO> GetActiveBonuses(CHeroSummonClass summon)
	{
		List<ActiveAbilityDTO> list = new List<ActiveAbilityDTO>();
		IEnumerable<CAbility> onSummonAbilities = summon.SummonYML.OnSummonAbilities;
		foreach (CAbility withChildAbility in CAbilityCard.GetWithChildAbilities(onSummonAbilities ?? Enumerable.Empty<CAbility>()))
		{
			if (GetActiveBonus(withChildAbility, withChildAbility.GetAbilityBaseCard() ?? CBaseCard.CreateDummy(), out var bonus))
			{
				if (bonus.Ability.ActiveBonusData.GetMiscVar("OverrideDescForTarget").isUnknown)
				{
					list.Add(new ActiveAbilityDTO(bonus, _caster));
					continue;
				}
				list.Add(new ActiveAbilityDTO(bonus, _caster));
				list.Add(new ActiveAbilityDTO(bonus, _target));
			}
		}
		onSummonAbilities = summon.SummonYML.OnDeathAbilities;
		foreach (CAbility withChildAbility2 in CAbilityCard.GetWithChildAbilities(onSummonAbilities ?? Enumerable.Empty<CAbility>()))
		{
			if (GetActiveBonus(withChildAbility2, withChildAbility2.GetAbilityBaseCard(), out var bonus2))
			{
				if (bonus2.Ability.ActiveBonusData.GetMiscVar("OverrideDescForTarget").isUnknown)
				{
					list.Add(new ActiveAbilityDTO(bonus2, _caster));
					continue;
				}
				list.Add(new ActiveAbilityDTO(bonus2, _caster));
				list.Add(new ActiveAbilityDTO(bonus2, _target));
			}
		}
		onSummonAbilities = summon.SummonYML.SpecialAbilities;
		foreach (CAbility withChildAbility3 in CAbilityCard.GetWithChildAbilities(onSummonAbilities ?? Enumerable.Empty<CAbility>()))
		{
			if (GetActiveBonus(withChildAbility3, withChildAbility3.GetAbilityBaseCard(), out var bonus3))
			{
				if (bonus3.Ability.ActiveBonusData.GetMiscVar("OverrideDescForTarget").isUnknown)
				{
					list.Add(new ActiveAbilityDTO(bonus3, _caster));
					continue;
				}
				list.Add(new ActiveAbilityDTO(bonus3, _caster));
				list.Add(new ActiveAbilityDTO(bonus3, _target));
			}
		}
		return list;
	}

	private List<ActiveAbilityDTO> GetActiveBonuses(ScenarioAbilitiesYMLData scenario)
	{
		List<ActiveAbilityDTO> list = new List<ActiveAbilityDTO>();
		if (scenario.ScenarioAbilities == null)
		{
			return list;
		}
		try
		{
			IEnumerable<CAbility> scenarioAbilities = scenario.ScenarioAbilities;
			foreach (CAbility withChildAbility in CAbilityCard.GetWithChildAbilities(scenarioAbilities ?? Enumerable.Empty<CAbility>()))
			{
				if (GetActiveBonus(withChildAbility, CBaseCard.CreateDummy(), out var bonus))
				{
					if (bonus.Ability.ActiveBonusData.GetMiscVar("OverrideDescForTarget").isUnknown)
					{
						list.Add(new ActiveAbilityDTO(bonus, _caster));
						continue;
					}
					list.Add(new ActiveAbilityDTO(bonus, _caster));
					list.Add(new ActiveAbilityDTO(bonus, _target));
				}
			}
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
		return list;
	}

	private List<ActiveAbilityDTO> GetActiveBonuses(ScenarioRuleLibrary.ScenarioState scenario)
	{
		List<ActiveAbilityDTO> list = new List<ActiveAbilityDTO>();
		try
		{
			foreach (CScenarioModifierApplyActiveBonusToActor item in scenario.ScenarioModifiers.OfType<CScenarioModifierApplyActiveBonusToActor>())
			{
				foreach (CAbility item2 in item.AllListedTriggerAbilities().SelectMany((CAbility ability) => CAbilityCard.GetWithChildAbilities(ability)))
				{
					if (GetActiveBonus(item2, item, out var bonus))
					{
						if (bonus.Ability.ActiveBonusData.GetMiscVar("OverrideDescForTarget").isUnknown)
						{
							list.Add(new ActiveAbilityDTO(bonus, _caster));
							continue;
						}
						list.Add(new ActiveAbilityDTO(bonus, _caster));
						list.Add(new ActiveAbilityDTO(bonus, _target));
					}
				}
			}
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
		return list;
	}

	private bool GetActiveBonus(CAbility ability, CBaseCard card, out CActiveBonus bonus)
	{
		bonus = null;
		bool flag;
		switch (ability.ActiveBonusData?.Duration)
		{
		case CActiveBonus.EActiveBonusDurationType.NA:
		case CActiveBonus.EActiveBonusDurationType.Summon:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			return false;
		}
		try
		{
			bonus = CActiveBonus.CreateActiveBonus(card, ability, _target, _caster, 0, null);
			if (bonus.IsHiddenInUI() || bonus.Ability.ActiveBonusData.Hidden || bonus.Ability.ActiveBonusData.HiddenWhenAura || bonus.Ability.GetMiscVar("DontShowActiveBonusInActorStatPanel").GetBool())
			{
				return false;
			}
			if (bonus.Ability.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
			{
				return false;
			}
			if (bonus.BaseCard is CItem cItem && cItem.GetVar("HideItemActiveForCaster").GetBool())
			{
				return false;
			}
			UIActiveAbility.CheckForLayoutToGenerate(bonus);
			return true;
		}
		catch (Exception)
		{
			Debug.LogError("Cannot load bonus " + ability.Name + " for card " + card.Name);
			return false;
		}
	}
}
