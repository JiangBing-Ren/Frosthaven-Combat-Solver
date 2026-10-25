using System;
using UnityEngine;

namespace Chronos.Reflection;

[Serializable]
public class AnimatorParameter
{
	[SerializeField]
	private Animator _target;

	[SerializeField]
	private string _name;

	public Animator target
	{
		get
		{
			return _target;
		}
		set
		{
			_target = value;
			isLinked = false;
		}
	}

	public string name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
			isLinked = false;
		}
	}

	public AnimatorControllerParameter parameterInfo { get; private set; }

	public bool isLinked { get; private set; }

	public bool isAssigned
	{
		get
		{
			if (target != null)
			{
				return !string.IsNullOrEmpty(name);
			}
			return false;
		}
	}

	public Type type => parameterInfo.type switch
	{
		AnimatorControllerParameterType.Float => typeof(float), 
		AnimatorControllerParameterType.Int => typeof(int), 
		AnimatorControllerParameterType.Bool => typeof(bool), 
		AnimatorControllerParameterType.Trigger => null, 
		_ => throw new NotImplementedException(), 
	};

	public AnimatorParameter()
	{
	}

	public AnimatorParameter(string name)
	{
		this.name = name;
	}

	public AnimatorParameter(string name, Animator target)
	{
		this.name = name;
		this.target = target;
		Link();
	}

	public void Link()
	{
		if (target == null)
		{
			throw new UnityException("Target has not been defined.");
		}
		AnimatorControllerParameter[] parameters = target.parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			if (animatorControllerParameter.name == name)
			{
				parameterInfo = animatorControllerParameter;
				return;
			}
		}
		throw new UnityException($"Animator parameter not found: '{name}'.");
	}

	protected void EnsureLinked()
	{
		if (!isLinked)
		{
			Link();
		}
	}

	public object Get()
	{
		EnsureLinked();
		return parameterInfo.type switch
		{
			AnimatorControllerParameterType.Float => (object)target.GetFloat(parameterInfo.nameHash), 
			AnimatorControllerParameterType.Int => target.GetInteger(parameterInfo.nameHash), 
			AnimatorControllerParameterType.Bool => target.GetBool(parameterInfo.nameHash), 
			AnimatorControllerParameterType.Trigger => throw new UnityException("Cannot get the value of a trigger parameter."), 
			_ => throw new NotImplementedException(), 
		};
	}

	public T Get<T>() where T : struct
	{
		return (T)Get();
	}

	public void Set(object value)
	{
		EnsureLinked();
		switch (parameterInfo.type)
		{
		case AnimatorControllerParameterType.Float:
			target.SetFloat(parameterInfo.nameHash, (float)value);
			break;
		case AnimatorControllerParameterType.Int:
			target.SetInteger(parameterInfo.nameHash, (int)value);
			break;
		case AnimatorControllerParameterType.Bool:
			target.SetBool(parameterInfo.nameHash, (bool)value);
			break;
		case AnimatorControllerParameterType.Trigger:
			throw new UnityException("Cannot set the value of a trigger parameter.");
		default:
			throw new NotImplementedException();
		}
	}

	public void SetTrigger()
	{
		EnsureLinked();
		if (parameterInfo.type != AnimatorControllerParameterType.Trigger)
		{
			throw new UnityException("Parameter is not a trigger.");
		}
		target.SetTrigger(parameterInfo.nameHash);
	}

	public void ResetTrigger()
	{
		EnsureLinked();
		if (parameterInfo.type != AnimatorControllerParameterType.Trigger)
		{
			throw new UnityException("Parameter is not a trigger.");
		}
		target.ResetTrigger(parameterInfo.nameHash);
	}

	public bool Corresponds(AnimatorParameter other)
	{
		if ((other != null || !isAssigned) && target == other.target)
		{
			return name == other.name;
		}
		return false;
	}
}
