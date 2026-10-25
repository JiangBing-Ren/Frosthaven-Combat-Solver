using System;
using System.Collections.Generic;
using System.Linq;
using EPOOutline;
using UnityEngine;

public class CustomActorOutlinableConfig : MonoBehaviour
{
	[Serializable]
	private struct ControllableOutlinable
	{
		public Outlinable Outlinable;

		public bool CanUseMaskedMode;
	}

	[SerializeField]
	private ControllableOutlinable[] _controllableOutlines;

	[SerializeField]
	private Outlinable[] _independantOutlines;

	public static bool TryFindInCharacter(CharacterManager manager, out CustomActorOutlinableConfig customConfig)
	{
		if (manager.TryGetComponent<CustomActorOutlinableConfig>(out customConfig))
		{
			return true;
		}
		foreach (Transform item in manager.transform)
		{
			if (item.TryGetComponent<CustomActorOutlinableConfig>(out customConfig))
			{
				return true;
			}
		}
		return false;
	}

	public int GetControllableOutlines(out IList<Outlinable> outlinables, out IList<bool> canUseMaskedMode)
	{
		outlinables = _controllableOutlines.Select((ControllableOutlinable o) => o.Outlinable).ToList();
		canUseMaskedMode = _controllableOutlines.Select((ControllableOutlinable o) => o.CanUseMaskedMode).ToList();
		return _controllableOutlines.Length;
	}
}
