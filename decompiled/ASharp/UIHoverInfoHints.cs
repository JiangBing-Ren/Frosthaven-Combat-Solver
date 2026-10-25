using UnityEngine;

public class UIHoverInfoHints : MonoBehaviour
{
	[Header("Hints")]
	[SerializeField]
	private GameObject m_ContainerPrimary;

	[SerializeField]
	private GameObject m_ContainerSecondary;

	[SerializeField]
	private GameObject m_ContainerInfo;

	public void Init(bool isPrimaryActive, bool isSecondaryActive, bool isInfoActive)
	{
		SetObjectActive(m_ContainerPrimary, isPrimaryActive);
		SetObjectActive(m_ContainerSecondary, isSecondaryActive);
		SetObjectActive(m_ContainerInfo, isInfoActive);
	}

	private static void SetObjectActive(GameObject o, bool active)
	{
		if (o.activeSelf != active)
		{
			o.SetActive(active);
		}
	}
}
