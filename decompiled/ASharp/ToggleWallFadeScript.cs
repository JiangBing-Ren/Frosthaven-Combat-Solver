using SETUtil.Materials;
using UnityEngine;

public class ToggleWallFadeScript : MonoBehaviour
{
	private Renderer[] rend;

	public bool ToggleWallFadeLocal;

	private void Start()
	{
		rend = gameObject.GetComponentsInChildren<Renderer>();
		if (ToggleWallFadeLocal)
		{
			Renderer[] array = rend;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].GetMaterialClonesSlow()[0].SetInt("_ToggleWallFadeLocal", 0);
			}
		}
		if (!ToggleWallFadeLocal)
		{
			Renderer[] array = rend;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].GetMaterialClonesSlow()[0].SetInt("_ToggleWallFadeLocal", 1);
			}
		}
	}

	private void Update()
	{
	}
}
