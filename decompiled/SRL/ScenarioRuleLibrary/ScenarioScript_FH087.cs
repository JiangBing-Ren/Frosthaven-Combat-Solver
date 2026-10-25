using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH087 : ScenarioScript
{
	private CObjectDoor _room4Door;

	private List<CMap> _cleanedMapsRequiredForDoor4;

	private List<EnemyState> _cleanedMonstersRequiredForDoor4;

	private static readonly string[] _brothersGuid = new string[2] { "1f3333b7-5f9c-cf48-a2a7-913b6bd23097", "2ad2a143-9730-58d4-55ea-c7963281691e" };

	private static CActor[] _brothers;

	protected override bool Initialize()
	{
		_cleanedMapsRequiredForDoor4 = ScenarioManager.CurrentScenarioState.Maps.Where((CMap m) => m.RoomName != "Room_23").ToList();
		_cleanedMonstersRequiredForDoor4 = (from m in _cleanedMapsRequiredForDoor4.SelectMany((CMap r) => r.Monsters)
			where !m.IsHiddenForCurrentPartySize
			select m).ToList();
		_room4Door = GetProp(11, 7, "ThickDoor") as CObjectDoor;
		_brothers = null;
		EnsureBrothersInitialized();
		return true;
	}

	protected override List<object> GetConclusionParametersInternal()
	{
		int num = 0;
		foreach (EnemyState item in ScenarioManager.CurrentScenarioState.AllyMonsters.Where((EnemyState x) => x.ClassID == "TheBrothersID"))
		{
			if (!item.IsDead)
			{
				num++;
			}
		}
		return new List<object> { num };
	}

	protected override void OnFinishedRevealingRoom()
	{
		if (_tickRoomRevealed?.RoomName == "Room_23")
		{
			GetScenarioModifier("FinalRoom_1").SetDeactivated(deactivate: false);
			GetScenarioModifier("FinalRoom_2").SetDeactivated(deactivate: false);
		}
		EnsureBrothersInitialized();
	}

	protected override void TickDeathInternal()
	{
		if (!_cleanedMapsRequiredForDoor4.Any((CMap m) => !m.Revealed) && _room4Door.DoorIsLocked && _cleanedMonstersRequiredForDoor4.TrueForAll((EnemyState m) => m.Actor?.IsDead ?? false))
		{
			_room4Door.UnlockLockedDoorWithoutOpening();
		}
	}

	public static bool ShouldBrotherUseAbility(CActor brother)
	{
		EnsureBrothersInitialized();
		if (brother == _brothers[0])
		{
			if (!_brothers[0].IsDead)
			{
				return true;
			}
		}
		else if (_brothers[0] == null || _brothers[0].IsDead)
		{
			return true;
		}
		return false;
	}

	public static bool ShouldBrotherUseAbility2(CActor brother)
	{
		EnsureBrothersInitialized();
		if (brother == _brothers[1] && (_brothers[0] == null || !_brothers[0].IsDead))
		{
			return true;
		}
		return false;
	}

	private static void EnsureBrothersInitialized()
	{
		for (int i = 0; i < _brothersGuid.Length; i++)
		{
			IntializeBrother(ScenarioManager.FindActor(_brothersGuid[i]));
		}
	}

	private static void IntializeBrother(CActor brother)
	{
		if (brother != null)
		{
			if (_brothers == null)
			{
				_brothers = new CActor[2];
			}
			if (brother.ChosenModelIndex == 0)
			{
				brother.CustomLocKey = "FH/TheRedBrother";
				_brothers[0] = brother;
			}
			else
			{
				brother.CustomLocKey = "FH/TheBlueBrother";
				_brothers[1] = brother;
			}
		}
	}
}
