using System.Linq;
using Libs.Utils.Maths;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UICanvasGroupAlphaLink : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup[] m_ObservedGroups;

	private CanvasGroup _targetCanvasGroup;

	private void Awake()
	{
		_targetCanvasGroup = GetComponent<CanvasGroup>();
	}

	private void Update()
	{
		float num = 0f;
		foreach (CanvasGroup item in m_ObservedGroups.Where((CanvasGroup cg) => cg.isActiveAndEnabled))
		{
			if (Utl.GreaterThan(item.alpha, num))
			{
				num = item.alpha;
			}
		}
		if (!Utl.Equals(_targetCanvasGroup.alpha, num))
		{
			_targetCanvasGroup.alpha = num;
		}
	}
}
