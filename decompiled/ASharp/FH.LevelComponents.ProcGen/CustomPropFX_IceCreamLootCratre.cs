using SETUtil.Functional.Extend;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

public class CustomPropFX_IceCreamLootCratre : MonoBehaviour
{
	[SerializeField]
	private GameObject _default;

	[SerializeField]
	private GameObject _replaceWith;

	public void OnEnable()
	{
		if (transform.parent.When((Transform a) => MatchingCustomCase(a.parent) || MatchingCustomCase(a)))
		{
			_replaceWith.SetActive(value: true);
			_default.SetActive(value: false);
			_replaceWith.GetComponent<OutlineWrapper>().Then((OutlineWrapper a) =>
			{
				a.Refresh();
			});
		}
		static bool MatchingCustomCase(Transform t)
		{
			if ((bool)t)
			{
				return t.name.Contains("FH_Scenario_Campaign_126_SCENARIO_OBJECT_1");
			}
			return false;
		}
	}
}
