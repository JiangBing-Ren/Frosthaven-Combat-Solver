using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary;
using UnityEngine;

public static class AutoLogUtility
{
	[Serializable]
	public class ButtonAndOrder : ISerializable
	{
		public string ButtonName;

		public int OrderInSiblings;

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("ButtonName", ButtonName);
			info.AddValue("OrderInSiblings", OrderInSiblings);
		}

		public ButtonAndOrder(SerializationInfo info, StreamingContext context)
		{
			foreach (SerializationEntry item in info)
			{
				try
				{
					string name = item.Name;
					if (!(name == "ButtonName"))
					{
						if (name == "OrderInSiblings")
						{
							OrderInSiblings = info.GetInt32("OrderInSiblings");
						}
						continue;
					}
					ButtonName = info.GetString("ButtonName");
					if (ButtonName.Contains("Character tab") && !ButtonName.Contains("ID"))
					{
						ECharacter eCharacter = CCharacterClass.CharacterNames.SingleOrDefault((ECharacter s) => ButtonName.Contains(s.ToString()));
						if (eCharacter != ECharacter.None)
						{
							ButtonName = ButtonName.Replace(eCharacter.ToString(), string.Empty);
							ButtonName = ButtonName + eCharacter.ToString() + "ID";
						}
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "Exception while trying to deserialize ButtonAndOrder entry " + item.Name);
					throw ex;
				}
			}
		}

		public ButtonAndOrder(string buttonName, int orderInSiblings)
		{
			ButtonName = buttonName;
			OrderInSiblings = orderInSiblings;
		}
	}

	public static List<ButtonAndOrder> GetGameObjectHierarchy(GameObject go)
	{
		List<ButtonAndOrder> list = new List<ButtonAndOrder>();
		Transform transform = go.transform;
		while (transform != null)
		{
			int orderInSiblings = 0;
			if (transform.parent != null && transform.parent.Find(transform.name) != transform)
			{
				orderInSiblings = transform.parent.Cast<Transform>().ToList().FindIndex((Transform a) => a == transform);
			}
			list.Insert(0, new ButtonAndOrder(transform.name, orderInSiblings));
			transform = transform.parent;
		}
		return list;
	}

	public static GameObject FindInTransform(Transform transform, ButtonAndOrder buttonAndOrderObj)
	{
		GameObject result = null;
		try
		{
			int orderInSiblings = buttonAndOrderObj.OrderInSiblings;
			result = ((orderInSiblings != 0) ? transform.Cast<Transform>().ToArray()[orderInSiblings].gameObject : transform.Find(buttonAndOrderObj.ButtonName)?.gameObject);
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception occurred at AutoLogUtility.FindInTransform");
		}
		return result;
	}

	public static float getSavedDelayTime(List<CAuto> events, int currentIndex)
	{
		float num = 1f;
		if (currentIndex < events.Count - 1)
		{
			DateTime timeStamp = events[currentIndex].TimeStamp;
			num = (float)events[currentIndex + 1].TimeStamp.Subtract(timeStamp).TotalSeconds;
			if ((double)num <= 0.1)
			{
				num = 1f;
			}
			else if (num > 20f)
			{
				num = 20f;
			}
		}
		return num;
	}
}
