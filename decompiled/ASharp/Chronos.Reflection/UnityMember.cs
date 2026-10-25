using System;
using UnityEngine;

namespace Chronos.Reflection;

[Serializable]
public abstract class UnityMember
{
	[SerializeField]
	private UnityEngine.Object _target;

	[SerializeField]
	private string _component;

	[SerializeField]
	private string _name;

	public UnityEngine.Object target
	{
		get
		{
			return _target;
		}
		set
		{
			_target = value;
			isTargeted = false;
		}
	}

	public string component
	{
		get
		{
			return _component;
		}
		set
		{
			_component = value;
			isTargeted = false;
			isReflected = false;
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
			isReflected = false;
		}
	}

	protected bool isTargeted { get; private set; }

	public bool isReflected { get; protected set; }

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

	protected UnityEngine.Object reflectionTarget { get; private set; }

	public UnityMember()
	{
	}

	public UnityMember(string name)
	{
		this.name = name;
	}

	public UnityMember(string name, UnityEngine.Object target)
	{
		this.name = name;
		this.target = target;
		Reflect();
	}

	public UnityMember(string component, string name)
	{
		this.component = component;
		this.name = name;
	}

	public UnityMember(string component, string name, UnityEngine.Object target)
	{
		this.component = component;
		this.name = name;
		this.target = target;
		Reflect();
	}

	protected void Target()
	{
		if (target == null)
		{
			throw new NullReferenceException("Target has not been specified.");
		}
		GameObject gameObject = target as GameObject;
		Component component = target as Component;
		if (gameObject != null || component != null)
		{
			if (!string.IsNullOrEmpty(this.component))
			{
				Component component2 = ((!(gameObject != null)) ? component.GetComponent(this.component) : gameObject.GetComponent(this.component));
				if (component2 == null)
				{
					throw new UnityReflectionException($"Target object does not contain a component of type '{this.component}'.");
				}
				reflectionTarget = component2;
			}
			else if (gameObject != null)
			{
				reflectionTarget = gameObject;
			}
			else
			{
				reflectionTarget = component.gameObject;
			}
		}
		else
		{
			ScriptableObject scriptableObject = target as ScriptableObject;
			if (!(scriptableObject != null))
			{
				throw new UnityReflectionException("Target should be a GameObject, a Component or a ScriptableObject.");
			}
			reflectionTarget = scriptableObject;
		}
	}

	public abstract void Reflect();

	public void EnsureReflected()
	{
		if (!isReflected)
		{
			Reflect();
		}
	}

	public void EnsureTargeted()
	{
		if (!isTargeted)
		{
			Target();
		}
	}

	public void EnsureAssigned()
	{
		if (!isAssigned)
		{
			throw new UnityReflectionException("Member hasn't been properly assigned.");
		}
	}

	public virtual bool Corresponds(UnityMember other)
	{
		if ((other != null || !isAssigned) && target == other.target && component == other.component)
		{
			return name == other.name;
		}
		return false;
	}
}
