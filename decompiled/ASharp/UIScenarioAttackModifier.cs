using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class UIScenarioAttackModifier : UIAttackModifier<Image>
{
	[SerializeField]
	private Material grayscaleMaterial;

	[SerializeField]
	private GameObject unfocusedMask;

	private void Awake()
	{
		if (attackModCounterPrefab == null)
		{
			attackModCounterPrefab = Singleton<ActorStatPanel>.Instance.CounterPrefab.GetComponent<Image>();
		}
	}

	public override void Init(AttackModifierYMLData modifier, int counters)
	{
		Init(modifier, counters, focused: true);
	}

	public void Init(AttackModifierYMLData modifier, int counters, bool focused)
	{
		base.Init(modifier, counters);
		SetUnfocused(!focused);
	}

	public void UpdateCounters(int counters, int used)
	{
		UpdateCounters(counters);
		int num = counters - used;
		for (int i = 0; i < num; i++)
		{
			attackModCounters[i].sprite = Singleton<ActorStatPanel>.Instance.AvailableCounter;
		}
		for (int j = num; j < counters; j++)
		{
			attackModCounters[j].sprite = Singleton<ActorStatPanel>.Instance.UsedCounter;
		}
		textValue.color = ((num == 0) ? UIInfoTools.DefInstance.greyedOutTextColor : UIInfoTools.Instance.GetAttackModifierColor(ModifierData.MathModifier));
		imageValue.material = ((num == 0) ? grayscaleMaterial : null);
	}

	public override void UpdateCounters(int counters)
	{
		base.UpdateCounters(counters);
		for (int i = 0; i < counters; i++)
		{
			attackModCounters[i].sprite = Singleton<ActorStatPanel>.Instance.AvailableCounter;
		}
	}

	public void SetUnfocused(bool unfocused)
	{
		unfocusedMask.SetActive(unfocused);
	}
}
