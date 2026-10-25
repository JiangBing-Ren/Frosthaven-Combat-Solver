using IKPn;
using UnityEngine;

namespace FH.Animation;

[ExecuteAlways]
public class IKPAnimationUtility_GrounderWeight : MonoBehaviour
{
	private IKPModule_LowerBody _module;

	[Range(0f, 1f)]
	public float grounderWeight;

	private float storedValue;

	private void Start()
	{
		_module = GetComponent<IKPModule_LowerBody>();
		if ((bool)_module)
		{
			float property = _module.GetProperty(IKPModule_LowerBody.p_grounderWeight);
			if (!Mathf.Approximately(grounderWeight, property))
			{
				grounderWeight = property;
				storedValue = property;
			}
		}
	}

	private void Update()
	{
		if (!_module)
		{
			return;
		}
		float property = _module.GetProperty(IKPModule_LowerBody.p_grounderWeight);
		if (!Mathf.Approximately(grounderWeight, property))
		{
			if (!Mathf.Approximately(grounderWeight, storedValue))
			{
				_module.SetProperty(IKPModule_LowerBody.p_grounderWeight, grounderWeight);
			}
			else
			{
				grounderWeight = property;
			}
		}
		storedValue = grounderWeight;
	}
}
