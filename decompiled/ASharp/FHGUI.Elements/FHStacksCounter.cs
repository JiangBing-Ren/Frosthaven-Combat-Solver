using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FHGUI.Elements;

public class FHStacksCounter : MonoBehaviour
{
	[SerializeField]
	private RectTransform _stackContainer;

	[SerializeField]
	private FHStacksPip _stackPrefab;

	private List<FHStacksPip> _stacks;

	private bool _initialized;

	private void Awake()
	{
		InitCache();
	}

	private void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
			_stacks = _stackContainer.GetComponentsInChildren<FHStacksPip>(includeInactive: true).ToList();
		}
	}

	public void Init(int current, int max)
	{
		InitCache();
		UIHelperUtil.EnsureEnoughChildComponents(max, _stacks, _stackPrefab.gameObject, _stackContainer);
		for (int i = 0; i < _stacks.Count; i++)
		{
			_stacks[i].gameObject.SetActive(i < max);
		}
		for (int j = 0; j < _stacks.Count; j++)
		{
			_stacks[j].SetActive(j < current);
		}
	}
}
