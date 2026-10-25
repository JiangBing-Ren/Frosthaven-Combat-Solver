using System;
using System.Linq;
using UnityEngine;

public class UIActivator : MonoBehaviour
{
	[Tooltip("Additional roots to be activated that are not children of this activator.")]
	public GameObject[] AdditionalRoots = Array.Empty<GameObject>();

	private void Awake()
	{
		foreach (GameObject item in AdditionalRoots.Concat(new GameObject[1] { gameObject }))
		{
			IExternallyAwaked[] componentsInChildren = item.GetComponentsInChildren<IExternallyAwaked>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].ExternalAwake();
			}
		}
	}

	private void Start()
	{
		foreach (GameObject item in AdditionalRoots.Concat(new GameObject[1] { gameObject }))
		{
			IExternallyStarted[] componentsInChildren = item.GetComponentsInChildren<IExternallyStarted>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].ExternalStart();
			}
		}
	}

	private void OnDestroy()
	{
		if (GloomUtility.IsAppQuitting)
		{
			return;
		}
		foreach (GameObject item in AdditionalRoots.Concat(new GameObject[1] { gameObject }))
		{
			IExternallyAwaked[] componentsInChildren = item.GetComponentsInChildren<IExternallyAwaked>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].ExternalDestroy();
			}
		}
	}
}
