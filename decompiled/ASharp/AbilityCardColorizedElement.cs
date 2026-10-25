using UnityEngine;

public class AbilityCardColorizedElement : MonoBehaviour
{
	public enum AbilityCardColorizedElementType
	{
		None,
		Divider,
		AddedEffect,
		Conditional,
		Summon
	}

	[SerializeField]
	private AbilityCardColorizedElementType _styleKey;

	public AbilityCardColorizedElementType StyleKey => _styleKey;
}
