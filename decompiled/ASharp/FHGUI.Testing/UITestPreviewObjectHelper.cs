using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Libs.Utils.Syntax;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Testing;

public class UITestPreviewObjectHelper : MonoBehaviour
{
	[SerializeField]
	private RectTransform _container;

	[Header("Prefabs")]
	[SerializeField]
	private GameObject _inputPrototype;

	[SerializeField]
	private GameObject _intPrototype;

	[SerializeField]
	private GameObject _floatPrototype;

	[SerializeField]
	private GameObject _dropdownPrototype;

	[SerializeField]
	private GameObject _togglePrototype;

	[SerializeField]
	private GameObject _objectPrototype;

	private object _trackedObject;

	private string _serialized;

	private static string Serialize(object obj)
	{
		using MemoryStream memoryStream = new MemoryStream();
		new BinaryFormatter().Serialize(memoryStream, obj);
		return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
	}

	private void LateUpdate()
	{
		_ = _trackedObject;
	}

	public void Show(object obj)
	{
		Cleanup();
		_trackedObject = obj;
		FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
		ShowObjectFields(obj, fields, _container);
	}

	private void ShowObjectFields(object obj, FieldInfo[] fields, RectTransform container)
	{
		foreach (FieldInfo fieldInfo in fields)
		{
			object value = fieldInfo.GetValue(obj);
			CreateGameObject(obj, value, fieldInfo, container);
		}
	}

	private GameObject CreateGameObject(object obj, object value, FieldInfo field, RectTransform container)
	{
		if (value is string)
		{
			return createStringGameObject(obj, field, container);
		}
		if (!field.FieldType.IsNumericType())
		{
			if (!(value is bool))
			{
				if (value is Enum)
				{
					return createEnumGameObject(obj, field, container);
				}
				if (field.FieldType.IsClass || field.FieldType.IsValueType)
				{
					return createNonValueType(obj, field, container);
				}
				return null;
			}
			return createBoolGameObject(obj, field, container);
		}
		return createNumericGameObject(ref obj, field, container);
	}

	private GameObject createStringGameObject(object obj, FieldInfo field, RectTransform container)
	{
		string text = field.Name;
		string textWithoutNotify = (string)field.GetValue(obj);
		GameObject gameObject = UnityEngine.Object.Instantiate(_inputPrototype, container);
		TMP_InputField input = gameObject.GetComponentInChildren<TMP_InputField>();
		gameObject.transform.Find("Label").GetComponent<TMP_Text>().text = text;
		input.SetTextWithoutNotify(textWithoutNotify);
		input.onValueChanged.AddListener((string arg0) =>
		{
			field.SetValueDirect(__makeref(obj), input.text);
		});
		return gameObject;
	}

	private GameObject createNumericGameObject(ref object obj, FieldInfo field, RectTransform container)
	{
		string text = field.Name;
		object value = field.GetValue(obj);
		GameObject gameObject = null;
		gameObject = ((!(field.FieldType == typeof(int)) && !(field.FieldType == typeof(uint)) && !(field.FieldType == typeof(byte))) ? _floatPrototype : _intPrototype);
		object oo = obj;
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, container);
		TMP_InputField input = gameObject2.GetComponentInChildren<TMP_InputField>();
		gameObject2.transform.Find("Label").GetComponent<TMP_Text>().text = text;
		input.SetTextWithoutNotify(value.ToString());
		input.onValueChanged.AddListener((string arg0) =>
		{
			field.SetValueDirect(__makeref(oo), Convert.ChangeType(input.text, field.FieldType));
		});
		return gameObject2;
	}

	private GameObject createBoolGameObject(object obj, FieldInfo field, RectTransform container)
	{
		string text = field.Name;
		bool isOnWithoutNotify = (bool)field.GetValue(obj);
		GameObject gameObject = UnityEngine.Object.Instantiate(_objectPrototype, container);
		Toggle toggle = gameObject.GetComponentInChildren<Toggle>();
		gameObject.transform.Find("Label").GetComponent<TMP_Text>().text = text;
		toggle.SetIsOnWithoutNotify(isOnWithoutNotify);
		toggle.onValueChanged.AddListener((bool arg0) =>
		{
			field.SetValue(obj, toggle.isOn);
		});
		return gameObject;
	}

	private GameObject createEnumGameObject(object obj, FieldInfo field, RectTransform container)
	{
		_ = field.Name;
		object value = field.GetValue(obj);
		GameObject gameObject = UnityEngine.Object.Instantiate(_dropdownPrototype, _container);
		TMP_Dropdown dropdown = gameObject.GetComponentInChildren<TMP_Dropdown>();
		gameObject.transform.Find("Label").GetComponent<TMP_Text>().text = field.Name;
		dropdown.ClearOptions();
		dropdown.options = (from ee in Enum.GetNames(value.GetType())
			select new TMP_Dropdown.OptionData
			{
				text = ee
			}).ToList();
		dropdown.SetValueWithoutNotify((int)value);
		dropdown.onValueChanged.AddListener((int arg0) =>
		{
			field.SetValue(obj, Enum.ToObject(field.FieldType, dropdown.value));
		});
		return gameObject;
	}

	private GameObject createNonValueType(object obj, FieldInfo field, RectTransform container)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(_objectPrototype, _container);
		gameObject.transform.Find("Label").GetComponent<TMP_Text>().text = field.Name;
		Transform transform = gameObject.transform.Find("Container");
		object value = field.GetValue(obj);
		FieldInfo[] fields = value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
		ShowObjectFields(value, fields, transform as RectTransform);
		return gameObject;
	}

	private void Cleanup()
	{
		foreach (Transform item in _container.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}
}
