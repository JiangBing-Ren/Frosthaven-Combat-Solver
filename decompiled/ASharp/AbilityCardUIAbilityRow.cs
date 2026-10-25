using UnityEngine;

public class AbilityCardUIAbilityRow : MonoBehaviour
{
	[HideInInspector]
	[SerializeField]
	private string _abilityName;

	public string AbilityName => _abilityName;
}
