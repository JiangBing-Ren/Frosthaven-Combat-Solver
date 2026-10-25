namespace ScenarioRuleLibrary;

public class ScenarioScript_FH061 : ScenarioScript
{
	private int[] _doors = new int[6] { 5, 6, 9, 3, 13, 10 };

	protected override void OnDoorOpenedInternal(CObjectDoor door, CActor actor, CActor creditActor, bool visualOnly)
	{
		for (int i = 0; i < _doors.Length; i += 2)
		{
			if (door.ArrayIndex.X == _doors[i] && door.ArrayIndex.Y == _doors[i + 1])
			{
				switch (i / 2)
				{
				case 0:
					perform("Door1Player");
					perform("Door1Moon");
					break;
				case 1:
					perform("Door2Monster");
					perform("Door2Moon");
					break;
				case 2:
					perform("Door3Player");
					perform("Door3Moon");
					break;
				}
				break;
			}
		}
		void perform(string mod)
		{
			foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
			{
				if (PerformScenarioModifier(mod, allActor, out var sm))
				{
					sm.SetDeactivated(deactivate: false);
					sm.SetHiddenState(shouldSetHidden: false);
				}
			}
		}
	}
}
