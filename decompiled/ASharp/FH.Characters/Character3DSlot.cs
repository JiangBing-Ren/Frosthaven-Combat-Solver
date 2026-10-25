using System;
using DG.Tweening;
using FH.LevelComponents.EnvironmentFX;
using FHGUI.Elements;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FH.Characters;

public class Character3DSlot : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	[SerializeField]
	private Transform _platform;

	[SerializeField]
	private Transform _characterSpawnPoint;

	[SerializeField]
	private ReflectionProbeSpawner _reflectionProbeSpawner;

	[SerializeField]
	private Character3DDisplayManager _display;

	[Header("Slot rotate")]
	[SerializeField]
	private DragRotateEffect _rotateEffect;

	[SerializeField]
	private float _rotateStep = 10f;

	[SerializeField]
	private float _rotateStepDuration = 0.1f;

	[Header("States visuals")]
	[SerializeField]
	private GameObject[] _highlight;

	[SerializeField]
	private GameObject[] _selection;

	[SerializeField]
	private GameObject _emptySlot;

	private Action<bool> _slotHoveredEvent;

	private Action<bool> _slotSelectedEvent;

	public bool Selected { get; private set; }

	public bool Hovered { get; private set; }

	public Transform CharacterSpawnPoint => _characterSpawnPoint;

	public ECharacter Character { get; private set; }

	public void Init(ECharacter character, string skin = null, string playAnimation = null)
	{
		bool flag = character != ECharacter.None;
		bool flag2 = Character != character;
		Character = character;
		SetHovered(hoverIn: false);
		Deselect();
		if (_emptySlot != null)
		{
			_emptySlot.SetActive(!flag);
		}
		if (!flag)
		{
			_display.Hide(this);
			return;
		}
		_characterSpawnPoint.transform.localRotation = Quaternion.identity;
		if (!flag2)
		{
			return;
		}
		_display.Display(this, character, null, "Selection Screen");
		foreach (CharacterManager currentModel in _display.CurrentModels)
		{
			Character3DDisplayUtils.SetupReflectionProbe(currentModel.gameObject, _reflectionProbeSpawner.Probe);
			Character3DDisplayUtils.SetupHeroMaterials(currentModel.gameObject);
		}
	}

	public void Play3DAnimation(string animationName, bool freezeAnimation)
	{
		if (Character != ECharacter.None)
		{
			_display.PlayAnimation(animationName, freezeAnimation);
		}
	}

	public void Freeze3DAnimatior(bool freezeAnimator)
	{
		if (Character != ECharacter.None)
		{
			_display.FreezeAnimator(freezeAnimator);
		}
	}

	public void PlayVFX3D(string vfxName)
	{
		if (Character != ECharacter.None)
		{
			_display.PlayVFX3D(vfxName);
		}
	}

	public void Clear()
	{
		Deselect();
		Character = ECharacter.None;
		_display.Hide(this);
	}

	public void SetHovered(bool hoverIn)
	{
		Hovered = hoverIn;
		if (_highlight != null)
		{
			GameObject[] highlight = _highlight;
			for (int i = 0; i < highlight.Length; i++)
			{
				highlight[i].SetActive(hoverIn);
			}
		}
	}

	public void SetSelected(bool selected)
	{
		if (selected)
		{
			Select();
		}
		else
		{
			Deselect();
		}
	}

	public void Select()
	{
		Selected = true;
		UpdateSelectedState();
	}

	public void Deselect()
	{
		Selected = false;
		UpdateSelectedState();
	}

	private void UpdateSelectedState()
	{
		if (_selection != null)
		{
			GameObject[] selection = _selection;
			for (int i = 0; i < selection.Length; i++)
			{
				selection[i].SetActive(Selected);
			}
		}
	}

	public void RotateLeft()
	{
		_characterSpawnPoint.DORotate(new Vector3(0f, _rotateStep, 0f), _rotateStepDuration, RotateMode.FastBeyond360).SetRelative(isRelative: true);
	}

	public void RotateRight()
	{
		_characterSpawnPoint.DORotate(-1f * new Vector3(0f, _rotateStep, 0f), _rotateStepDuration, RotateMode.FastBeyond360).SetRelative(isRelative: true);
	}

	public void RotateSlot(Vector2 delta)
	{
		_characterSpawnPoint.RotateAround(Vector3.up, delta.x * Time.deltaTime);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		SetHovered(hoverIn: true);
		_slotHoveredEvent?.Invoke(obj: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		SetHovered(hoverIn: false);
		_slotHoveredEvent?.Invoke(obj: false);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		SetSelected(selected: true);
		_slotSelectedEvent?.Invoke(obj: true);
	}

	public void WireEvents(Action<bool> onSelect, Action<bool> onHover)
	{
		_slotHoveredEvent = onHover;
		_slotSelectedEvent = onSelect;
	}
}
