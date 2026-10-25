using FH.Characters;
using ScenarioRuleLibrary;
using UnityEngine;

public class FHCharacterSelectionScene : MonoBehaviour, ISceneRoot
{
	[SerializeField]
	private Character3DSlot _slot;

	[SerializeField]
	private Ambience _ambience;

	private ECharacter _character;

	[field: SerializeField]
	public Camera Camera { get; private set; }

	public Transform Root => transform;

	public void Show(object data = null)
	{
		gameObject.SetActive(value: true);
		if (_ambience != null)
		{
			_ambience.ApplyBlend(1f, 1f);
		}
		_character = (ECharacter)data;
		DisplayCharacter(_character);
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
		_ambience.ApplyBlend(0f, 0f);
		_slot.Clear();
	}

	public bool ShouldRefresh(object data = null)
	{
		return _character != (ECharacter)data;
	}

	private void DisplayCharacter(ECharacter character, string skin = null, string playAnimation = null)
	{
		if (_character != ECharacter.None)
		{
			_slot.Clear();
		}
		_character = character;
		if (_character != ECharacter.None)
		{
			_slot.Init(character, skin, playAnimation);
		}
	}

	public void HoverSlot(bool hoverIn)
	{
		_slot.SetHovered(hoverIn);
	}

	public void SetSelected(bool selected)
	{
		_slot.SetSelected(selected);
	}
}
