using UnityEngine;

namespace ClockStone;

internal static class PoolableExtensions
{
	internal static void _SetActive(this GameObject obj, bool active)
	{
		obj.SetActive(active);
	}

	internal static bool _GetActive(this GameObject obj)
	{
		return obj.activeInHierarchy;
	}
}
