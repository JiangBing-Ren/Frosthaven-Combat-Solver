using System;
using System.Linq;
using System.Reflection;
using Chronos.Reflection.Internal;
using UnityEngine;

namespace Chronos.Reflection;

[Serializable]
public class UnityMethod : UnityMember, ISerializationCallbackReceiver
{
	[SerializeField]
	private string[] _parameterTypes;

	private Type[] __parameterTypes;

	public MethodInfo methodInfo { get; private set; }

	public bool isExtension { get; private set; }

	public Type[] parameterTypes
	{
		get
		{
			return __parameterTypes;
		}
		set
		{
			__parameterTypes = value;
			isReflected = false;
		}
	}

	public Type returnType
	{
		get
		{
			EnsureReflected();
			return methodInfo.ReturnType;
		}
	}

	public void OnAfterDeserialize()
	{
		if (_parameterTypes != null)
		{
			parameterTypes = _parameterTypes.Select((string typeName) => TypeSerializer.Deserialize(typeName)).ToArray();
		}
	}

	public void OnBeforeSerialize()
	{
		if (parameterTypes != null)
		{
			_parameterTypes = parameterTypes.Select((Type type) => TypeSerializer.Serialize(type)).ToArray();
		}
	}

	public UnityMethod()
	{
	}

	public UnityMethod(string name)
		: base(name)
	{
	}

	public UnityMethod(string name, UnityEngine.Object target)
		: base(name, target)
	{
	}

	public UnityMethod(string component, string name)
		: base(component, name)
	{
	}

	public UnityMethod(string component, string name, UnityEngine.Object target)
		: base(component, name, target)
	{
	}

	public UnityMethod(string name, Type[] parameterTypes)
		: base(name)
	{
		this.parameterTypes = parameterTypes;
	}

	public UnityMethod(string name, Type[] parameterTypes, UnityEngine.Object target)
		: this(name, parameterTypes)
	{
		base.target = target;
		Reflect();
	}

	public UnityMethod(string component, string name, Type[] parameterTypes)
		: base(component, name)
	{
		this.parameterTypes = parameterTypes;
	}

	public UnityMethod(string component, string name, Type[] parameterTypes, UnityEngine.Object target)
		: this(component, name, parameterTypes)
	{
		base.target = target;
		Reflect();
	}

	public override void Reflect()
	{
		EnsureAssigned();
		EnsureTargeted();
		methodInfo = UnityMemberHelper.ReflectMethod(reflectionTarget, name, parameterTypes);
		isExtension = methodInfo.IsExtension();
		isReflected = true;
	}

	public object Invoke(params object[] parameters)
	{
		EnsureReflected();
		return UnityMemberHelper.InvokeMethod(reflectionTarget, methodInfo, isExtension, parameters);
	}

	public T Invoke<T>(params object[] parameters)
	{
		return (T)Invoke(parameters);
	}

	public override bool Corresponds(UnityMember other)
	{
		if (other is UnityMethod && base.Corresponds(other))
		{
			return Enumerable.SequenceEqual(parameterTypes, ((UnityMethod)other).parameterTypes);
		}
		return false;
	}
}
