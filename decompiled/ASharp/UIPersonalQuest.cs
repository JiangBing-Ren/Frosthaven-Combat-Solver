using MapRuleLibrary.YML.PersonalQuests;
using UnityEngine;

public class UIPersonalQuest : MonoBehaviour
{
	[SerializeField]
	private TextLocalizedListener questName;

	[SerializeField]
	private TextLocalizedListener questDescription;

	[SerializeField]
	private TextLocalizedListener questObjective;

	public void SetPersonalQuest(PersonalQuestYMLData personalQuest)
	{
		if (personalQuest != null)
		{
			if (questName != null)
			{
				questName.SetTextKey(personalQuest.LocalisedName);
			}
			if (questDescription != null)
			{
				questDescription.SetTextKey(personalQuest.LocalisedDescription);
			}
			questObjective.SetTextKey(personalQuest.LocalisedObjectiveDescription);
		}
	}
}
