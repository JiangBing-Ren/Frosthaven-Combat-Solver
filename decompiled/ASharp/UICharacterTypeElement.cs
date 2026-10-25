using ScenarioRuleLibrary;
using UnityEngine;

public class UICharacterTypeElement : MonoBehaviour, ICharacterSpecificUIElement
{
	[SerializeField]
	private ECharacter _characterType;

	public bool IsCharacterElement(CPlayerActor playerActor)
	{
		if (playerActor != null)
		{
			return playerActor.CharacterClass.CharacterModel == _characterType;
		}
		return false;
	}

	public void Init(CPlayerActor playerActor)
	{
		gameObject.SetActive(IsCharacterElement(playerActor));
	}
}
