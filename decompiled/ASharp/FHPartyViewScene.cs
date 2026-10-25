using System.Collections.Generic;
using System.Linq;
using FH.Characters;
using FHGUI.DTO;
using FHRL;
using ScenarioRuleLibrary;
using UnityEngine;

public class FHPartyViewScene : MonoBehaviour, ISceneRoot
{
	private enum SceneActionLateUpdateType
	{
		None,
		Show,
		Hide
	}

	public struct PartySceneParams
	{
		public List<PartySlotDTO> SlotData;
	}

	[SerializeField]
	private Ambience _ambience;

	[SerializeField]
	private Character3DSlot[] _slots;

	private SceneActionLateUpdateType _lateAction;

	private PartySceneParams? _lateData;

	[field: SerializeField]
	public Camera Camera { get; private set; }

	public Transform Root => transform;

	public void Show(object data = null)
	{
		Show(immediate: true, data);
	}

	public void Show(bool immediate, object data = null)
	{
		_lateData = (PartySceneParams)data;
		_lateAction = SceneActionLateUpdateType.Show;
		if (immediate && _lateData.HasValue)
		{
			Show_Implementation(_lateData.Value);
			ClearLateAction();
		}
	}

	public void Hide()
	{
		_lateData = null;
		_lateAction = SceneActionLateUpdateType.Hide;
	}

	private void ClearLateAction()
	{
		_lateData = null;
		_lateAction = SceneActionLateUpdateType.None;
	}

	private void Show_Implementation(PartySceneParams partyParams)
	{
		gameObject.SetActive(value: true);
		_ambience.ApplyBlend(1f, 1f);
		for (int i = 0; i < _slots.Length; i++)
		{
			PartySlotDTO partySlotDTO = partyParams.SlotData[i];
			_slots[i].Init(partySlotDTO.Character);
		}
	}

	private void Hide_Implementation()
	{
		_ambience.ApplyBlend(0f, 0f);
		gameObject.SetActive(value: false);
		Character3DSlot[] slots = _slots;
		for (int i = 0; i < slots.Length; i++)
		{
			slots[i].Clear();
		}
	}

	private void LateUpdate()
	{
		if (_lateAction == SceneActionLateUpdateType.Show && _lateData.HasValue)
		{
			Show_Implementation(_lateData.Value);
		}
		else if (_lateAction == SceneActionLateUpdateType.Hide)
		{
			Hide_Implementation();
		}
		ClearLateAction();
	}

	public void Play3DAnimations(string animationName = null, bool freezeAnimation = false)
	{
		if (animationName != null)
		{
			for (int i = 0; i < _slots.Length; i++)
			{
				_slots[i].Play3DAnimation(animationName, freezeAnimation);
			}
		}
	}

	public void Freeze3DAnimatiors(bool freezeAnimator = false)
	{
		for (int i = 0; i < _slots.Length; i++)
		{
			_slots[i].Freeze3DAnimatior(freezeAnimator);
		}
	}

	public void PlayVFX3D(CharacterClassDef characterClass, string vfxName)
	{
		ECharacter eCharacter = CrossReference.ClassDefToECharacterModel(characterClass);
		for (int i = 0; i < _slots.Length; i++)
		{
			if (_slots[i].Character == eCharacter)
			{
				_slots[i].PlayVFX3D(vfxName);
			}
		}
	}

	public bool ShouldRefresh(object data = null)
	{
		return true;
	}

	public void HoverSlot(int slotId, bool isFlagRaised)
	{
		if (slotId > 0)
		{
			_slots.ElementAt(slotId - 1)?.SetHovered(isFlagRaised);
		}
	}

	public void OnSlotDrag(int slotId, Vector2 delta)
	{
		if (slotId > 0)
		{
			_slots.ElementAt(slotId - 1)?.RotateSlot(delta);
		}
	}
}
