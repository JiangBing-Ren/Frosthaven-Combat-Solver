using UnityEngine;
using UnityEngine.UI;

public class FHPartyMarker : MonoBehaviour
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private GameObject[] _iconPartyPips;

	public void Init(int partyMemberCount)
	{
		Show();
		for (int i = 0; i < _iconPartyPips.Length; i++)
		{
			_iconPartyPips[i].SetActive(i < partyMemberCount);
		}
	}

	public void Show()
	{
		gameObject.SetActive(value: true);
	}

	public void Hide(bool instant = false)
	{
		gameObject.SetActive(value: false);
	}
}
