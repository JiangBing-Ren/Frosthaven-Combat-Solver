using FHRL;
using UnityEngine;

namespace FHGUI.Elements;

public class ResourcesBarElement : MonoBehaviour
{
	[SerializeField]
	private ResourceElement _gold;

	[SerializeField]
	private ResourceElement _lumber;

	[SerializeField]
	private ResourceElement _metal;

	[SerializeField]
	private ResourceElement _hide;

	[SerializeField]
	private ResourceElement _arrowvine;

	[SerializeField]
	private ResourceElement _axenut;

	[SerializeField]
	private ResourceElement _corpsecap;

	[SerializeField]
	private ResourceElement _flamefruit;

	[SerializeField]
	private ResourceElement _rockroot;

	[SerializeField]
	private ResourceElement _snowthistle;

	public void Init(int gold, OutpostResources resources, bool hideZeroResources = false)
	{
		if ((bool)_gold)
		{
			_gold.Init(gold);
		}
		Populate(_lumber, resources.Lumber);
		Populate(_metal, resources.Metal);
		Populate(_hide, resources.Hide);
		Populate(_arrowvine, resources.Arrowvine);
		Populate(_axenut, resources.Axenut);
		Populate(_corpsecap, resources.Corpsecap);
		Populate(_flamefruit, resources.Flamefruit);
		Populate(_rockroot, resources.Rockroot);
		Populate(_snowthistle, resources.Snowthistle);
		void Populate(ResourceElement element, int value)
		{
			if (!(element == null))
			{
				if (hideZeroResources && value == 0)
				{
					element.gameObject.SetActive(value: false);
				}
				else
				{
					element.gameObject.SetActive(value: true);
					element.Init(value);
				}
			}
		}
	}
}
