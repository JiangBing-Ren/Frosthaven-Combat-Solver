using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Elements.PuzzleBook;

public class PuzzleStepListContainer : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	protected RectTransform m_Container;

	[Header("Prefabs")]
	[SerializeField]
	private UIPersonalQuestStepProgress _progressStepPrefab;

	[SerializeField]
	private UIPersonalQuestStepCheckbox _checkboxStepPrefab;

	[SerializeField]
	private UIPersonalQuestStepIcon _iconStepPrefab;

	protected List<PersonalQuestStepUI> m_ListElements = new List<PersonalQuestStepUI>();

	private int m_ActiveCount;

	private List<PersonalQuestStepUI> _cache = new List<PersonalQuestStepUI>();

	public IEnumerable<PersonalQuestStepUI> Elements => m_ListElements.Take(m_ActiveCount);

	private IEnumerable<UIPersonalQuestStepProgress> _progressElements => m_ListElements.OfType<UIPersonalQuestStepProgress>();

	private IEnumerable<UIPersonalQuestStepCheckbox> _stepElements => m_ListElements.OfType<UIPersonalQuestStepCheckbox>();

	private IEnumerable<UIPersonalQuestStepIcon> _iconElements => m_ListElements.OfType<UIPersonalQuestStepIcon>();

	private void InitCache()
	{
		if (m_ListElements.Count <= 0)
		{
			m_Container.GetComponentsInChildren(m_ListElements);
		}
	}

	public void Init(object listParams, IList<PersonalQuestStepDTO> steps)
	{
		InitCache();
		_cache.Clear();
		_cache.AddRange(m_ListElements);
		for (int i = 0; i < steps.Count; i++)
		{
			PersonalQuestStepDTO step = steps[i];
			if (!GetOrCreate(step, out var element))
			{
				m_ListElements.Remove(element);
			}
			m_ListElements.Insert(i, element);
		}
		int num = (m_ActiveCount = steps?.Count ?? 0);
		HelperTools.NormalizePool(ref m_ListElements, null, m_Container, num);
		for (int j = 0; j < num; j++)
		{
			m_ListElements[j].Init(steps[j]);
		}
	}

	private PersonalQuestStepUI GetPrefab(PersonalQuestStepDTO step)
	{
		if (!step.IsCheckbox)
		{
			if ((object)step.Icon != null)
			{
				return _iconStepPrefab;
			}
			return _progressStepPrefab;
		}
		return _checkboxStepPrefab;
	}

	private bool GetOrCreate(PersonalQuestStepDTO step, out PersonalQuestStepUI element)
	{
		PersonalQuestStepUI prefab = GetPrefab(step);
		element = _cache.FirstOrDefault((PersonalQuestStepUI t) => t.GetType() == prefab.GetType());
		if (element == null)
		{
			element = Object.Instantiate(prefab, m_Container);
			return true;
		}
		_cache.Remove(element);
		return false;
	}
}
