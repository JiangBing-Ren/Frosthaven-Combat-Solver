using System;
using UnityEngine;

namespace ClockStone;

public abstract class RegisteredComponent : MonoBehaviour, IRegisteredComponent
{
	private bool isRegistered;

	private bool isUnregistered;

	protected virtual void Awake()
	{
		if (!isRegistered)
		{
			RegisteredComponentController._Register(this);
			isRegistered = true;
			isUnregistered = false;
		}
		else
		{
			Debug.LogWarning("RegisteredComponent: Awake() not correctly called. Object: " + name);
		}
	}

	protected virtual void OnDestroy()
	{
		if (isRegistered && !isUnregistered)
		{
			RegisteredComponentController._Unregister(this);
			isRegistered = false;
			isUnregistered = true;
		}
		else if (isRegistered || !isUnregistered)
		{
			Debug.LogWarning("RegisteredComponent: OnDestroy() not correctly called. Object: " + name + " isRegistered:" + isRegistered + " isUnregistered:" + isUnregistered);
		}
	}

	public Type GetRegisteredComponentBaseClassType()
	{
		return typeof(RegisteredComponent);
	}
}
