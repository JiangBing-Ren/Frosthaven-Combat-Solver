using ScenarioRuleLibrary;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/DLC", fileName = "DLC Config")]
public class DLCConfig : ScriptableObject
{
	public enum EDLCState
	{
		Available,
		ComingSoon,
		Unavailable
	}

	public DLCRegistry.EDLCKey DLCKey;

	public Sprite MissingCharacterIcon;

	public Sprite[] PromotionalImages;

	public EDLCState State;

	public Sprite ShieldIcon;
}
