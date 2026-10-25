using UnityEngine;
using UnityEngine.UI;

public class SummonCardUI : MonoBehaviour
{
	[field: SerializeField]
	public AbilityCardActionLocalizedListener Health { get; private set; }

	[field: SerializeField]
	public EnhancementButton HealthEnhancementButton { get; private set; }

	[field: SerializeField]
	public AbilityCardActionLocalizedListener Attack { get; private set; }

	[field: SerializeField]
	public EnhancementButton AttackEnhancementButton { get; private set; }

	[field: SerializeField]
	public AbilityCardActionLocalizedListener Movement { get; private set; }

	[field: SerializeField]
	public EnhancementButton MovementEnhancementButton { get; private set; }

	[field: SerializeField]
	public AbilityCardActionLocalizedListener Range { get; private set; }

	[field: SerializeField]
	public EnhancementButton RangeEnhancementButton { get; private set; }

	[field: SerializeField]
	public TextLocalizedListener SummonName { get; private set; }

	[field: SerializeField]
	public Image Icon { get; private set; }
}
