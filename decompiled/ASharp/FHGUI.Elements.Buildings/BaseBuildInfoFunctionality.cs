using UnityEngine;

namespace FHGUI.Elements.Buildings;

public abstract class BaseBuildInfoFunctionality : MonoBehaviour, IBuildInfoFunctionality
{
	public void Hide()
	{
		gameObject.SetActive(value: false);
		OnHide();
	}

	protected virtual void OnHide()
	{
	}
}
