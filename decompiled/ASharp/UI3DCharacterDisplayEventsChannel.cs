using System;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "UI Events/3D Character Display Events Channel", fileName = "UI3DCharacterDisplayEventsChannel")]
public class UI3DCharacterDisplayEventsChannel : ScriptableObject
{
	[NonSerialized]
	public UnityEvent<ECharacter, string, string> OnDisplay;

	[NonSerialized]
	public UnityEvent OnHide;

	[NonSerialized]
	private Character3DDisplayManager _instance;

	private bool IsValid => _instance != null;

	private void OnEnable()
	{
		if (OnDisplay == null)
		{
			OnDisplay = new UnityEvent<ECharacter, string, string>();
		}
		if (OnHide == null)
		{
			OnHide = new UnityEvent();
		}
		_instance = null;
	}

	private void OnDisable()
	{
		if (OnDisplay != null)
		{
			OnDisplay.RemoveAllListeners();
		}
		if (OnHide != null)
		{
			OnHide.RemoveAllListeners();
		}
	}

	public void SetReference(Character3DDisplayManager instance)
	{
		if (_instance != null && _instance != instance)
		{
			Log.GUIWarning(name + " already has an instance referenced, it will be overridden!" + $" Old instance: [{_instance}]", this);
		}
		_instance = instance;
	}

	public void Display(Component request, ECharacter character, string skin = null, string playAnimation = "Selection Screen")
	{
		if (IsValid)
		{
			_instance.Display(request, character, skin, playAnimation);
			OnDisplay.Invoke(character, skin, playAnimation);
		}
	}

	public void Hide(Component request)
	{
		if (IsValid)
		{
			_instance.Hide(request);
			OnHide.Invoke();
		}
	}
}
