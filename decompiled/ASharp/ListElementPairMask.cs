using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ListElementPairMask : UIBehaviour
{
	[SerializeField]
	private Graphic impairMask;

	[SerializeField]
	private Graphic pairMask;

	protected override void Awake()
	{
		base.Awake();
		Refresh();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Refresh();
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		Refresh();
	}

	private void Refresh()
	{
		bool flag = transform.GetSiblingIndex() % 2 == 0;
		if (pairMask != null)
		{
			pairMask.enabled = flag;
		}
		if (impairMask != null)
		{
			impairMask.enabled = !flag;
		}
	}
}
