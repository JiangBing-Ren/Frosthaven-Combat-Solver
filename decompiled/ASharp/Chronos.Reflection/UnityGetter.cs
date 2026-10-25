using System;
using System.Linq;
using System.Reflection;
using Chronos.Reflection.Internal;
using UnityEngine;

namespace Chronos.Reflection;

[Serializable]
public class UnityGetter : UnityMember
{
	private enum SourceType
	{
		Unknown,
		Field,
		Property,
		Method
	}

	private SourceType sourceType;

	[SerializeField]
	private string[] _parameterTypes;

	private Type[] __parameterTypes;

	public FieldInfo fieldInfo { get; private set; }

	public PropertyInfo propertyInfo { get; private set; }

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
			return sourceType switch
			{
				SourceType.Field => fieldInfo.FieldType, 
				SourceType.Property => propertyInfo.PropertyType, 
				SourceType.Method => methodInfo.ReturnType, 
				_ => throw new UnityReflectionException(), 
			};
		}
	}

	public UnityGetter()
	{
	}

	public UnityGetter(string name)
		: base(name)
	{
	}

	public UnityGetter(string name, UnityEngine.Object target)
		: base(name, target)
	{
	}

	public UnityGetter(string component, string name)
		: base(component, name)
	{
	}

	public UnityGetter(string component, string name, UnityEngine.Object target)
		: base(component, name, target)
	{
	}

	public UnityGetter(string name, Type[] parameterTypes)
		: base(name)
	{
		this.parameterTypes = parameterTypes;
	}

	public UnityGetter(string name, Type[] parameterTypes, UnityEngine.Object target)
		: this(name, parameterTypes)
	{
		base.target = target;
		Reflect();
	}

	public UnityGetter(string component, string name, Type[] parameterTypes)
		: base(component, name)
	{
		this.parameterTypes = parameterTypes;
	}

	public UnityGetter(string component, string name, Type[] parameterTypes, UnityEngine.Object target)
		: this(component, name, parameterTypes)
	{
		base.target = target;
		Reflect();
	}

	public override void Reflect()
	{
		EnsureAssigned();
		EnsureTargeted();
		fieldInfo = null;
		propertyInfo = null;
		this.methodInfo = null;
		sourceType = SourceType.Unknown;
		if (UnityMemberHelper.TryReflectVariable(out var variableInfo, out var exception, reflectionTarget, name))
		{
			fieldInfo = variableInfo as FieldInfo;
			propertyInfo = variableInfo as PropertyInfo;
			if (fieldInfo != null)
			{
				sourceType = SourceType.Field;
			}
			else if (propertyInfo != null)
			{
				sourceType = SourceType.Property;
			}
		}
		else
		{
			if (!UnityMemberHelper.TryReflectMethod(out var methodInfo, out exception, reflectionTarget, name, parameterTypes))
			{
				throw new UnityReflectionException("No matching field, property or method found.");
			}
			this.methodInfo = methodInfo;
			isExtension = methodInfo.IsExtension();
			sourceType = SourceType.Method;
		}
		isReflected = true;
	}

	public object Get(params object[] parameters)
	{
		EnsureReflected();
		return sourceType switch
		{
			SourceType.Field => fieldInfo.GetValue(reflectionTarget), 
			SourceType.Property => propertyInfo.GetValue(reflectionTarget, null), 
			SourceType.Method => UnityMemberHelper.InvokeMethod(reflectionTarget, methodInfo, isExtension, parameters), 
			_ => throw new UnityReflectionException(), 
		};
	}

	public T Get<T>(params object[] parameters)
	{
		return (T)Get(parameters);
	}

	public override bool Corresponds(UnityMember other)
	{
		bool flag = other is UnityGetter && base.Corresponds(other);
		flag &= parameterTypes == null == (((UnityGetter)other).parameterTypes == null);
		if (parameterTypes != null)
		{
			flag &= Enumerable.SequenceEqual(parameterTypes, ((UnityGetter)other).parameterTypes);
		}
		return flag;
	}
}
