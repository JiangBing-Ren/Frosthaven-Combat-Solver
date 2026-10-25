using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Common;

public struct ContextualData : IGeneralInfoData
{
	public delegate bool Validation();

	public List<CActor> Actors;

	public List<CObjectProp> Props;

	public CTile[] Tiles;

	public List<GameObject> UIObjects;

	public string Caption;

	public string ID;

	public Sprite Illustration;

	public bool FixedPosition;

	public Validation AdditionalValidation;

	public Validation NonRemovingValidation;

	public Action OnShown;

	public Action OnClosed;

	public bool BlockInput;

	public int Priority;

	public int OrderAdded;

	public string Group;

	public string OverrideFallbackPosition;

	public CActor Actor
	{
		get
		{
			if (Actors == null)
			{
				return null;
			}
			return Actors[0];
		}
		set
		{
			if (value != null)
			{
				Actors = new List<CActor> { value };
			}
			else
			{
				Actors = null;
			}
		}
	}

	public CObjectProp Prop
	{
		get
		{
			if (Props == null)
			{
				return null;
			}
			return Props[0];
		}
		set
		{
			if (value != null)
			{
				Props = new List<CObjectProp> { value };
			}
			else
			{
				Props = null;
			}
		}
	}

	public bool Validate()
	{
		if (AdditionalValidation != null && !AdditionalValidation())
		{
			return false;
		}
		if (Actors != null)
		{
			for (int num = Actors.Count - 1; num >= 0; num--)
			{
				if (Actors[num].IsDead)
				{
					Actors.RemoveAt(num);
				}
			}
			if (Actors.Count == 0)
			{
				return false;
			}
		}
		if (Props != null)
		{
			for (int num2 = Props.Count - 1; num2 >= 0; num2--)
			{
				if (Props[num2].IsDestroyed)
				{
					Props.RemoveAt(num2);
				}
			}
			if (Props.Count == 0)
			{
				return false;
			}
		}
		if (UIObjects != null)
		{
			for (int num3 = UIObjects.Count - 1; num3 >= 0; num3--)
			{
				if (UIObjects[num3] == null)
				{
					UIObjects.RemoveAt(num3);
				}
			}
		}
		return true;
	}
}
