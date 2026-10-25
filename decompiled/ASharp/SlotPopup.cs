using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class SlotPopup : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	private Transform holder;

	public void Init(Transform holder)
	{
		this.holder = holder;
	}

	public void Show()
	{
		if (holder != null)
		{
			transform.position = holder.position;
		}
		window.Show();
	}

	public void Hide()
	{
		window.Hide();
	}
}
