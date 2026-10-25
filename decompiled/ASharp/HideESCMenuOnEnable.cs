using UnityEngine;

public class HideESCMenuOnEnable : MonoBehaviour
{
	private void OnEnable()
	{
		Singleton<ESCMenu>.Instance?.Hide();
	}
}
