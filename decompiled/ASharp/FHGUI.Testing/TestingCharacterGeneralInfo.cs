using System.Linq;
using AStar;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Testing;

public class TestingCharacterGeneralInfo : MonoBehaviour
{
	public ECharacter Character;

	private GameObject _characterGameObject;

	private void OnEnable()
	{
		if (_characterGameObject != null)
		{
			ObjectPool.Recycle(_characterGameObject);
		}
		_characterGameObject = Character3DDisplayUtils.SpawnCharacter(Character);
		CCharacterClass characterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == Character.ToString());
		CPlayerActor characterActor = new CPlayerActor(new Point(0, 0), characterClass, 0, 0, 1, "", 0, Character.ToString(), 0, CPlayerActor.EAgility.None, CPlayerActor.EForm.None, CPlayerActor.EForm.None, CPlayerActor.EForm.None, 0, 0, 0);
		_characterGameObject.GetComponent<CharacterManager>().CharacterActor = characterActor;
		_characterGameObject.transform.SetParent(transform);
	}
}
