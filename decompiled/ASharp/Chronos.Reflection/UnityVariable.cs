using System;
using System.Reflection;
using Chronos.Reflection.Internal;
using UnityEngine;

namespace Chronos.Reflection;

[Serializable]
public class UnityVariable : UnityMember
{
	private enum SourceType
	{
		Unknown,
		Field,
		Property
	}

	private SourceType sourceType;

	public FieldInfo fieldInfo { get; private set; }

	public PropertyInfo propertyInfo { get; private set; }

	public Type type
	{
		get
		{
			EnsureReflected();
			return sourceType switch
			{
				SourceType.Field => fieldInfo.FieldType, 
				SourceType.Property => propertyInfo.PropertyType, 
				_ => throw new UnityReflectionException(), 
			};
		}
	}

	public UnityVariable()
	{
	}

	public UnityVariable(string name)
		: base(name)
	{
	}

	public UnityVariable(string name, UnityEngine.Object target)
		: base(name, target)
	{
	}

	public UnityVariable(string component, string name)
		: base(component, name)
	{
	}

	public UnityVariable(string component, string name, UnityEngine.Object target)
		: base(component, name, target)
	{
	}

	public override void Reflect()
	{
		EnsureAssigned();
		EnsureTargeted();
		fieldInfo = null;
		propertyInfo = null;
		sourceType = SourceType.Unknown;
		MemberInfo memberInfo = UnityMemberHelper.ReflectVariable(reflectionTarget, name);
		fieldInfo = memberInfo as FieldInfo;
		propertyInfo = memberInfo as PropertyInfo;
		if (fieldInfo != null)
		{
			sourceType = SourceType.Field;
		}
		else if (propertyInfo != null)
		{
			sourceType = SourceType.Property;
		}
		isReflected = true;
	}

	public object Get()
	{
		EnsureReflected();
		return sourceType switch
		{
			SourceType.Field => fieldInfo.GetValue(reflectionTarget), 
			SourceType.Property => propertyInfo.GetValue(reflectionTarget, null), 
			_ => throw new UnityReflectionException(), 
		};
	}

	public T Get<T>()
	{
		return (T)Get();
	}

	public void Set(object value)
	{
		EnsureReflected();
		switch (sourceType)
		{
		case SourceType.Field:
			fieldInfo.SetValue(reflectionTarget, value);
			break;
		case SourceType.Property:
			propertyInfo.SetValue(reflectionTarget, value, null);
			break;
		default:
			throw new UnityReflectionException();
		}
	}

	public override bool Corresponds(UnityMember other)
	{
		if (other is UnityVariable)
		{
			return base.Corresponds(other);
		}
		return false;
	}
}
