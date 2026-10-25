using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Libs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReportIssueGui;

public static class Binder
{
	public struct GroupObject
	{
		public IList<string> Groups;

		public UnityEngine.Object Object;

		public UIBehaviour Control;

		public MonoBehaviour Behaviour;

		public Component Component;

		public GameObject GameObject;
	}

	private static readonly char[] GroupsSplitCharacter = new char[1] { ',' };

	private static readonly Dictionary<object, IList<GroupObject>> BehaviorToGroups = new Dictionary<object, IList<GroupObject>>();

	private static readonly StringBuilder TempString = new StringBuilder();

	private static readonly Queue<Transform> SearchQueue = new Queue<Transform>();

	public static void BindFields(object target, GameObject searchRoot = null)
	{
		if (searchRoot == null)
		{
			MonoBehaviour monoBehaviour = target as MonoBehaviour;
			if (!(monoBehaviour != null))
			{
				throw new InvalidOperationException("Cannot bind target " + target.GetType().Name + " in scene " + SceneManager.GetActiveScene().name + ": Cannot determine the search root.");
			}
			searchRoot = monoBehaviour.gameObject;
		}
		BehaviorToGroups.Remove(target);
		BehaviorToGroups.RemoveWhere((KeyValuePair<object, IList<GroupObject>> kv) => kv.Key == null);
		Type type = target.GetType();
		do
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty);
			foreach (FieldInfo fieldInfo in fields)
			{
				object obj = Array.Find(fieldInfo.GetCustomAttributes(typeof(BindAttribute), inherit: false), (object a) => a is BindAttribute);
				if (obj != null)
				{
					BindField(searchRoot, target, (BindAttribute)obj, fieldInfo);
				}
			}
			type = type.BaseType;
		}
		while (type != typeof(object));
	}

	public static void EnableControlsGroup(MonoBehaviour behaviour, string group, bool enable)
	{
		if (!BehaviorToGroups.TryGetValue(behaviour, out var value))
		{
			throw new InvalidOperationException("Missing controls group name " + group + " in script " + behaviour.name);
		}
		bool flag = false;
		int i = 0;
		for (int count = value.Count; i < count; i++)
		{
			if (value[i].Groups.Contains(group))
			{
				flag = true;
				Selectable selectable = value[i].Control as Selectable;
				if (selectable != null)
				{
					selectable.interactable = enable;
				}
			}
		}
		if (!flag)
		{
			throw new InvalidOperationException("Missing controls group name " + group + " in script " + behaviour.name);
		}
	}

	public static void ShowControlsGroup(MonoBehaviour behaviour, string group, bool show)
	{
		if (!BehaviorToGroups.TryGetValue(behaviour, out var value))
		{
			throw new InvalidOperationException("Missing controls group name " + group + " in script " + behaviour.name);
		}
		bool flag = false;
		int i = 0;
		for (int count = value.Count; i < count; i++)
		{
			if (value[i].Groups.Contains(group))
			{
				flag = true;
				if (value[i].Control != null)
				{
					value[i].Control.gameObject.SetActive(show);
				}
			}
		}
		if (!flag)
		{
			throw new InvalidOperationException("Missing controls group name " + group + " in script " + behaviour.name);
		}
	}

	public static void EnumGroup<TObj>(MonoBehaviour behaviour, string group, Action<TObj> callback) where TObj : UnityEngine.Object
	{
		if (!BehaviorToGroups.TryGetValue(behaviour, out var value))
		{
			throw new InvalidOperationException("Missing objects group name " + group + " in script " + behaviour.name);
		}
		bool flag = false;
		int i = 0;
		for (int count = value.Count; i < count; i++)
		{
			if (value[i].Groups.Contains(group))
			{
				flag = true;
				TObj val = value[i].Object as TObj;
				if (val != null)
				{
					callback(val);
				}
			}
		}
		if (!flag)
		{
			throw new InvalidOperationException("Missing objects group name " + group + " in script " + behaviour.name);
		}
	}

	public static IEnumerable<TObj> GetGroup<TObj>(MonoBehaviour behaviour, string group) where TObj : UnityEngine.Object
	{
		if (!BehaviorToGroups.TryGetValue(behaviour, out var groupObjects))
		{
			throw new InvalidOperationException("Missing objects group name " + group + " in script " + behaviour.name);
		}
		bool groupFound = false;
		int i = 0;
		int n = groupObjects.Count;
		while (i < n)
		{
			if (groupObjects[i].Groups.Contains(group))
			{
				groupFound = true;
				TObj val = groupObjects[i].Object as TObj;
				if (val != null)
				{
					yield return val;
				}
			}
			int num = i + 1;
			i = num;
		}
		if (!groupFound)
		{
			throw new InvalidOperationException("Missing objects group name " + group + " in script " + behaviour.name);
		}
	}

	private static void BindField(GameObject searchRoot, object target, BindAttribute attr, FieldInfo field)
	{
		if (!string.IsNullOrEmpty(attr.CanvasSource) && !searchRoot.name.Contains(attr.CanvasSource))
		{
			return;
		}
		Type type = (field.FieldType.IsArray ? field.FieldType.GetElementType() : field.FieldType);
		if (!typeof(Component).IsAssignableFrom(type) && type != typeof(GameObject))
		{
			throw new InvalidOperationException("Cannot bind field " + target.GetType().Name + "." + field.Name + " with type " + field.FieldType.Name + " in scene " + SceneManager.GetActiveScene().name + ": The type of the field should be either an Component or GameObject.");
		}
		if (attr.BindByType)
		{
			BindFieldByTypeOnly(searchRoot, target, attr, field);
			return;
		}
		searchRoot = (attr.ToParent ? searchRoot : GetObjectFromScene(searchRoot.transform, attr.Name, field));
		if (searchRoot == null)
		{
			throw new InvalidOperationException("Cannot bind field " + target.GetType().Name + "." + field.Name + " in scene " + SceneManager.GetActiveScene().name + ": Cannot find game object with this name in scene hieararchy.");
		}
		if (field.FieldType.IsArray)
		{
			List<UnityEngine.Object> list = new List<UnityEngine.Object>();
			if (typeof(Component).IsAssignableFrom(type))
			{
				Component[] componentsInChildren = searchRoot.GetComponentsInChildren(type, !attr.OnlyEnabled);
				foreach (Component component in componentsInChildren)
				{
					if (!attr.SkipParent || !(component.gameObject == searchRoot))
					{
						BindSingleObject(component, target, attr);
						list.Add(component);
					}
				}
			}
			else
			{
				int j = 0;
				for (int childCount = searchRoot.transform.childCount; j < childCount; j++)
				{
					GameObject gameObject = searchRoot.transform.GetChild(j).gameObject;
					if (!attr.OnlyEnabled || gameObject.activeInHierarchy)
					{
						BindSingleObject(gameObject, target, attr);
						list.Add(gameObject);
					}
				}
			}
			Array array = Array.CreateInstance(type, list.Count);
			int k = 0;
			for (int count = list.Count; k < count; k++)
			{
				array.SetValue(list[k], k);
			}
			field.SetValue(target, array);
		}
		else
		{
			UnityEngine.Object obj = searchRoot;
			if (typeof(Component).IsAssignableFrom(type))
			{
				obj = searchRoot.GetComponent(type);
			}
			BindSingleObject(obj, target, attr);
			field.SetValue(target, obj);
		}
	}

	private static void BindSingleObject(UnityEngine.Object obj, object target, BindAttribute attr)
	{
		UIBehaviour uIBehaviour = obj as UIBehaviour;
		if (uIBehaviour != null)
		{
			if (attr.Invisible)
			{
				uIBehaviour.enabled = false;
			}
			if (attr.Disabled)
			{
				Selectable selectable = uIBehaviour as Selectable;
				if (selectable != null)
				{
					selectable.interactable = false;
				}
			}
			if (attr.Checked)
			{
				Toggle toggle = uIBehaviour as Toggle;
				if (toggle != null)
				{
					toggle.isOn = true;
				}
			}
		}
		if (attr.Disabled)
		{
			GameObject gameObject = obj as GameObject;
			if (gameObject != null)
			{
				gameObject.SetActive(value: false);
			}
		}
		AddObjectToGroups(obj, target, attr);
	}

	private static void AddObjectToGroups(UnityEngine.Object obj, object target, BindAttribute attr)
	{
		if (attr.Groups != null)
		{
			IList<string> list = attr.Groups.Split(GroupsSplitCharacter, StringSplitOptions.RemoveEmptyEntries);
			if (list.Count == 0)
			{
				string name = obj.name;
				throw new InvalidOperationException("Invalid groups specification for object with name: " + name + " in scene " + SceneManager.GetActiveScene().name + ": '" + attr.Groups + "'");
			}
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				list[i] = list[i].Trim();
			}
			if (!BehaviorToGroups.TryGetValue(target, out var value))
			{
				value = new List<GroupObject>();
				BehaviorToGroups[target] = value;
			}
			GroupObject item = new GroupObject
			{
				Groups = list,
				Object = obj,
				Behaviour = (obj as MonoBehaviour),
				Component = (obj as Component),
				Control = (obj as UIBehaviour),
				GameObject = (obj as GameObject)
			};
			value.Add(item);
		}
	}

	private static void BindFieldByTypeOnly(GameObject obj, object target, BindAttribute attr, FieldInfo field)
	{
		if (!string.IsNullOrEmpty(attr.Name))
		{
			throw new InvalidOperationException("Trying to specify name while having BindByType flag is not allowed for " + target.GetType().Name + "." + field.Name + " with type " + field.FieldType.Name + " in scene " + SceneManager.GetActiveScene().name);
		}
		if (field.FieldType.IsArray)
		{
			Type elementType = field.FieldType.GetElementType();
			Component[] componentsInChildren = obj.transform.GetComponentsInChildren(elementType);
			List<Component> list = new List<Component>(componentsInChildren.Length);
			int i = 0;
			for (int num = componentsInChildren.Length; i < num; i++)
			{
				Component component = componentsInChildren[i];
				if (!attr.SkipParent || !(component.gameObject == obj))
				{
					list.Add(component);
				}
			}
			Array array = Array.CreateInstance(elementType, list.Count);
			int j = 0;
			for (int count = list.Count; j < count; j++)
			{
				array.SetValue(list[j], j);
			}
			field.SetValue(target, array);
		}
		else
		{
			Component componentInChildren = obj.transform.GetComponentInChildren(field.FieldType);
			if (componentInChildren == null)
			{
				throw new InvalidOperationException("Cannot bind field " + target.GetType().Name + "." + field.Name + " in scene " + SceneManager.GetActiveScene().name + ": Cannot find object with that component type.");
			}
			BindSingleObject(componentInChildren, target, attr);
			field.SetValue(target, componentInChildren);
		}
	}

	private static GameObject GetObjectFromScene(Transform parent, string name, FieldInfo field)
	{
		string name2 = (string.IsNullOrEmpty(name) ? NormalizeName(field.Name) : name);
		Type type = field.FieldType;
		if (field.FieldType.IsArray)
		{
			type = typeof(GameObject);
		}
		return GetObjectFromScene(parent, name2, type);
	}

	private static GameObject GetObjectFromScene(Transform startTransform, string name, Type type)
	{
		SearchQueue.Clear();
		SearchQueue.Enqueue(startTransform);
		while (SearchQueue.Count > 0)
		{
			Transform transform = SearchQueue.Dequeue();
			if (name == transform.name || name == NormalizeName(transform.name))
			{
				if (type == typeof(Text) && transform.GetComponent(type) == null)
				{
					Transform transform2 = transform.Find("Text");
					if (transform2?.GetComponent(type) != null)
					{
						return transform2.gameObject;
					}
				}
				if (type == typeof(GameObject) || transform.GetComponent(type) != null)
				{
					return transform.gameObject;
				}
			}
			int i = 0;
			for (int childCount = transform.childCount; i < childCount; i++)
			{
				Transform child = transform.GetChild(i);
				SearchQueue.Enqueue(child);
			}
		}
		return null;
	}

	private static string NormalizeName(string name)
	{
		TempString.Length = 0;
		TempString.Append(name);
		TempString.Replace("_", "");
		TempString.Replace(" ", "");
		TempString[0] = char.ToUpperInvariant(TempString[0]);
		return TempString.ToString();
	}
}
