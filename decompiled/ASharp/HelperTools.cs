using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScenarioRuleLibrary;
using UnityEngine;

public static class HelperTools
{
	public static void NormalizePool<T>(ref List<T> pool, GameObject prefab, Transform parent, int newLength, Action<T> onCreateAction = null, Action<T> onHideAction = null) where T : Component
	{
		if (pool == null)
		{
			pool = new List<T>();
		}
		int count = pool.Count;
		for (int i = 0; i < Math.Min(newLength, count); i++)
		{
			pool[i].gameObject.SetActive(value: true);
		}
		for (int j = 0; j < newLength - count; j++)
		{
			T component = UnityEngine.Object.Instantiate(prefab).GetComponent<T>();
			onCreateAction?.Invoke(component);
			component.name = prefab.name;
			component.transform.SetParent(parent, worldPositionStays: false);
			component.gameObject.SetActive(value: true);
			pool.Add(component);
		}
		count = pool.Count;
		for (int k = 0; k < count - newLength; k++)
		{
			onHideAction?.Invoke(pool[k + newLength]);
			pool[k + newLength].gameObject.SetActive(value: false);
		}
	}

	public static void NormalizePool(ref List<GameObject> pool, GameObject prefab, Transform parent, int newLength)
	{
		if (pool == null)
		{
			pool = new List<GameObject>();
		}
		foreach (GameObject item in pool)
		{
			item.SetActive(value: true);
		}
		int count = pool.Count;
		for (int i = 0; i < newLength - count; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
			gameObject.transform.SetParent(parent, worldPositionStays: false);
			pool.Add(gameObject);
		}
		count = pool.Count;
		for (int j = 0; j < count - newLength; j++)
		{
			pool[j + newLength].SetActive(value: false);
		}
	}

	public static float HorizontalGroupPositionOffset(float orderNumber, float totalNumber, float elementWidth, float spacing)
	{
		return (orderNumber - (totalNumber - 1f) / 2f) * (elementWidth * totalNumber + spacing * (totalNumber - 1f)) / totalNumber;
	}

	public static Vector3 DeltaPositionToFitTheScreen(this RectTransform rectTransform, float margin = 0f)
	{
		Vector2 vector = new Vector2((float)Screen.width - margin, (float)Screen.height - margin);
		Vector2 vector2 = new Vector2(margin, margin);
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		if (array[0].x < vector2.x || array[0].y < vector2.y)
		{
			return new Vector3(Mathf.Max(0f, vector2.x - array[0].x), Mathf.Max(0f, vector2.y - array[0].y));
		}
		if (array[1].x < vector2.x || array[1].y > vector.y)
		{
			return new Vector3(Mathf.Max(0f, vector2.x - array[1].x), vector.y - array[1].y);
		}
		if (array[2].x > vector.x || array[2].y > vector.y)
		{
			return new Vector3(Mathf.Min(0f, vector.x - array[2].x), Mathf.Min(0f, vector.y - array[2].y));
		}
		if (array[3].x > vector.x || array[3].y < vector2.y)
		{
			return new Vector3(Mathf.Min(0f, vector.x - array[3].x), Mathf.Max(0f, vector2.y - array[3].y));
		}
		return Vector3.zero;
	}

	public static Vector3 DeltaPositionToFitTheScreen(this RectTransform rectTransform, Camera camera, float margin = 0f)
	{
		Vector2 vector = new Vector2((float)Screen.width - margin, (float)Screen.height - margin);
		Vector2 vector2 = new Vector2(margin, margin);
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		if (camera != null)
		{
			for (int i = 0; i < 4; i++)
			{
				array[i] = camera.WorldToScreenPoint(array[i]);
			}
		}
		if (array[0].x < vector2.x || array[0].y < vector2.y)
		{
			return new Vector3(Mathf.Max(0f, vector2.x - array[0].x), Mathf.Max(0f, vector2.y - array[0].y));
		}
		if (array[1].x < vector2.x || array[1].y > vector.y)
		{
			return new Vector3(Mathf.Max(0f, vector2.x - array[1].x), vector.y - array[1].y);
		}
		if (array[2].x > vector.x || array[2].y > vector.y)
		{
			return new Vector3(Mathf.Min(0f, vector.x - array[2].x), Mathf.Min(0f, vector.y - array[2].y));
		}
		if (array[3].x > vector.x || array[3].y < vector2.y)
		{
			return new Vector3(Mathf.Min(0f, vector.x - array[3].x), Mathf.Max(0f, vector2.y - array[3].y));
		}
		return Vector3.zero;
	}

	public static Vector3 DeltaWorldPositionToFitTheScreen(this RectTransform rectTransform, Camera camera, float margin = 0f)
	{
		Vector2 vector = new Vector2((float)Screen.width - margin, (float)Screen.height - margin);
		Vector2 vector2 = new Vector2(margin, margin);
		Vector3[] array = new Vector3[4];
		Vector3[] array2 = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Vector3 vector3 = camera.ScreenToWorldPoint(vector);
		Vector3 vector4 = camera.ScreenToWorldPoint(vector2);
		for (int i = 0; i < 4; i++)
		{
			array2[i] = camera.WorldToScreenPoint(array[i]);
		}
		if (array2[0].x < vector2.x || array2[0].y < vector2.y)
		{
			return new Vector3(Mathf.Max(0f, vector4.x - array[0].x), Mathf.Max(0f, vector4.y - array[0].y));
		}
		if (array2[1].x < vector2.x || array2[1].y > vector.y)
		{
			return new Vector3(Mathf.Max(0f, vector4.x - array[1].x), Mathf.Min(0f, vector3.y - array[1].y));
		}
		if (array2[2].x > vector.x || array2[2].y > vector.y)
		{
			return new Vector3(Mathf.Min(0f, vector3.x - array[2].x), Mathf.Min(0f, vector3.y - array[2].y));
		}
		if (array2[3].x > vector.x || array2[3].y < vector2.y)
		{
			return new Vector3(Mathf.Min(0f, vector3.x - array[3].x), Mathf.Max(0f, vector4.y - array[3].y));
		}
		return Vector3.zero;
	}

	public static Vector3 DeltaWorldPositionToFitRectTransform(this RectTransform rectTransform, Camera camera, RectTransform areaToFit, bool checkBothAxies = false)
	{
		Vector3[] array = new Vector3[4];
		Vector3[] array2 = new Vector3[4];
		Vector3[] array3 = new Vector3[4];
		rectTransform.GetWorldCorners(array2);
		areaToFit.GetWorldCorners(array);
		Vector2 vector = camera.WorldToScreenPoint(array[2]);
		Vector2 vector2 = camera.WorldToScreenPoint(array[0]);
		for (int i = 0; i < 4; i++)
		{
			array3[i] = camera.WorldToScreenPoint(array2[i]);
		}
		Vector3 result = Vector3.zero;
		if (array3[0].x < vector2.x || array3[0].y < vector2.y)
		{
			result = new Vector3(Mathf.Max(0f, array[0].x - array2[0].x), Mathf.Max(0f, array[0].y - array2[0].y));
			if (!checkBothAxies)
			{
				return result;
			}
		}
		if (array3[1].x < vector2.x || array3[1].y > vector.y)
		{
			Vector3 result2 = new Vector3(Mathf.Max(0f, array[0].x - array2[1].x), Mathf.Min(0f, array[2].y - array2[1].y));
			if (!checkBothAxies)
			{
				return result2;
			}
			result.x = ((Math.Abs(result.x) > Math.Abs(result2.x)) ? result.x : result2.x);
			result.y = ((Math.Abs(result.y) > Math.Abs(result2.y)) ? result.y : result2.y);
		}
		if (array3[2].x > vector.x || array3[2].y > vector.y)
		{
			Vector3 result3 = new Vector3(Mathf.Min(0f, array[2].x - array2[2].x), Mathf.Min(0f, array[2].y - array2[2].y));
			if (!checkBothAxies)
			{
				return result3;
			}
			result.x = ((Math.Abs(result.x) > Math.Abs(result3.x)) ? result.x : result3.x);
			result.y = ((Math.Abs(result.y) > Math.Abs(result3.y)) ? result.y : result3.y);
		}
		if (array3[3].x > vector.x || array3[3].y < vector2.y)
		{
			Vector3 result4 = new Vector3(Mathf.Min(0f, array[2].x - array2[3].x), Mathf.Max(0f, array[0].y - array2[3].y));
			if (!checkBothAxies)
			{
				return result4;
			}
			result.x = ((Math.Abs(result.x) > Math.Abs(result4.x)) ? result.x : result4.x);
			result.y = ((Math.Abs(result.y) > Math.Abs(result4.y)) ? result.y : result4.y);
		}
		return result;
	}

	public static bool FitsInRectTransform(this RectTransform rectTransform, Camera camera, RectTransform areaToFit)
	{
		Vector3[] array = new Vector3[4];
		Vector3[] array2 = new Vector3[4];
		Vector3[] array3 = new Vector3[4];
		rectTransform.GetWorldCorners(array2);
		areaToFit.GetWorldCorners(array);
		Vector2 vector = camera.WorldToScreenPoint(array[2]);
		Vector2 vector2 = camera.WorldToScreenPoint(array[0]);
		for (int i = 0; i < 4; i++)
		{
			array3[i] = camera.WorldToScreenPoint(array2[i]);
		}
		if (array3[0].x < vector2.x || array3[0].y < vector2.y)
		{
			return false;
		}
		if (array3[1].x < vector2.x || array3[1].y > vector.y)
		{
			return false;
		}
		if (array3[2].x > vector.x || array3[2].y > vector.y)
		{
			return false;
		}
		if (array3[3].x > vector.x || array3[3].y < vector2.y)
		{
			return false;
		}
		return true;
	}

	public static bool IsWidthInRectTransform(this RectTransform rectTransform, Camera camera, RectTransform areaToFit)
	{
		Vector3[] array = new Vector3[4];
		Vector3[] array2 = new Vector3[4];
		Vector3[] array3 = new Vector3[4];
		rectTransform.GetWorldCorners(array2);
		areaToFit.GetWorldCorners(array);
		Vector2 vector = camera.WorldToScreenPoint(array[2]);
		Vector2 vector2 = camera.WorldToScreenPoint(array[0]);
		for (int i = 0; i < 4; i++)
		{
			array3[i] = camera.WorldToScreenPoint(array2[i]);
		}
		if (array3[0].x < vector2.x || array3[2].x > vector.x)
		{
			return false;
		}
		return true;
	}

	public static Sprite GetSprite(this Sprite[] sprites, string name, bool toLower = true, bool useDefaultWhenMissing = true, Sprite defaultSprite = null)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		foreach (Sprite sprite in sprites)
		{
			if (!(sprite == null) && IsMatch(sprite.name, name, toLower))
			{
				return sprite;
			}
		}
		if (useDefaultWhenMissing)
		{
			Log.Warning("Can't find '" + name + "' sprite");
			Sprite sprite2;
			if (!(defaultSprite != null))
			{
				sprite2 = UIInfoTools.Instance.MissingSpriteIcon;
				if ((object)sprite2 == null)
				{
					return sprites.FirstOrDefault();
				}
			}
			else
			{
				sprite2 = defaultSprite;
			}
			return sprite2;
		}
		return null;
		static bool IsMatch(string spriteName, string targetName, bool ignoreCase)
		{
			int length = targetName.Length;
			int length2 = spriteName.Length;
			for (int j = 0; j < length2; j++)
			{
				int num = j;
				int k = 0;
				while (k < length && num < length2)
				{
					char c = spriteName[num];
					char c2 = targetName[k];
					if (c == ' ' || c == '_')
					{
						num++;
					}
					else if (c2 == ' ' || c2 == '_')
					{
						k++;
					}
					else
					{
						if (ignoreCase)
						{
							if (char.ToLowerInvariant(c) != char.ToLowerInvariant(c2))
							{
								break;
							}
						}
						else if (c != c2)
						{
							break;
						}
						num++;
						k++;
					}
				}
				for (; k < length && (targetName[k] == ' ' || targetName[k] == '_'); k++)
				{
				}
				if (k >= length)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static T GetObject<T>(this T[] objects, string name) where T : UnityEngine.Object
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		foreach (T val in objects)
		{
			if (!(val == null) && val.name.Replace(" ", "").Replace("_", "").ToLower()
				.Contains(name.Replace(" ", "").Replace("_", "").ToLower()))
			{
				return val;
			}
		}
		Log.Error("Can't find '" + name + "' object");
		if (objects.Length == 0)
		{
			return null;
		}
		return objects[0];
	}

	public static string GetObjectName<T>(this T[] objects, string name) where T : UnityEngine.Object
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		foreach (T val in objects)
		{
			if (!(val == null) && val.name.Replace(" ", "").Replace("_", "").ToLower()
				.Contains(name.Replace(" ", "").Replace("_", "").ToLower()))
			{
				return val.name;
			}
		}
		Log.Error("Can't find '" + name + "' object");
		return null;
	}

	public static bool IsDefaultAction(this CBaseCard.ActionType actionType)
	{
		if (actionType != CBaseCard.ActionType.DefaultAttackAction)
		{
			return actionType == CBaseCard.ActionType.DefaultMoveAction;
		}
		return true;
	}

	public static CBaseCard.ActionType GetOppositeAction(this CBaseCard.ActionType actionType)
	{
		return actionType switch
		{
			CBaseCard.ActionType.DefaultMoveAction => CBaseCard.ActionType.DefaultAttackAction, 
			CBaseCard.ActionType.BottomAction => CBaseCard.ActionType.TopAction, 
			CBaseCard.ActionType.TopAction => CBaseCard.ActionType.BottomAction, 
			_ => CBaseCard.ActionType.DefaultMoveAction, 
		};
	}

	public static string ToHtmlColor(this CActiveBonus activeBonus)
	{
		string text = BitConverter.ToString(Encoding.Default.GetBytes(activeBonus.BaseCard.Name)).Replace("-", "");
		if (text.Length > 6)
		{
			text = text.Substring(0, 6);
		}
		else if (text.Length < 6)
		{
			for (int i = text.Length; i < 6; i++)
			{
				text += "0";
			}
		}
		return text;
	}

	public static int ToSortedValue(this CardPileType cardPileType)
	{
		return cardPileType switch
		{
			CardPileType.Active => 0, 
			CardPileType.Round => 1, 
			CardPileType.Hand => 2, 
			CardPileType.Discarded => 3, 
			CardPileType.Taken => 4, 
			CardPileType.Lost => 5, 
			CardPileType.Permalost => 6, 
			_ => 6, 
		};
	}

	public static CBaseCard.ECardPile GetRecoverECardPile(this CardHandMode cardHandMode)
	{
		switch (cardHandMode)
		{
		case CardHandMode.RecoverDiscardedCard:
			return CBaseCard.ECardPile.Discarded;
		case CardHandMode.RecoverLostCard:
			return CBaseCard.ECardPile.Lost;
		default:
			Log.Warning($"Probably, an invalid recover card pile from cardHandMode:{cardHandMode.ToString()}! Please, review this!");
			return CBaseCard.ECardPile.None;
		}
	}

	public static string GetCorrectSprite(CAbility.EAbilityType abilityType)
	{
		string empty = string.Empty;
		switch (abilityType)
		{
		case CAbility.EAbilityType.AddHeal:
		case CAbility.EAbilityType.Overheal:
			return CAbility.EAbilityType.Heal.ToString();
		case CAbility.EAbilityType.AddRange:
			return "Range";
		case CAbility.EAbilityType.AttackersGainDisadvantage:
			return "Attack";
		case CAbility.EAbilityType.Kill:
			return "AA_Kill";
		case CAbility.EAbilityType.Advantage:
			return "Advantage";
		default:
			return abilityType.ToString();
		}
	}

	public static void TrimAll(this List<string> stringList)
	{
		for (int i = 0; i < stringList.Count; i++)
		{
			stringList[i] = stringList[i].Trim();
		}
	}
}
