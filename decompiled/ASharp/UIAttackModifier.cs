using System.Collections.Generic;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAttackModifier<T> : MonoBehaviour where T : MonoBehaviour
{
	[SerializeField]
	protected GameObject rolling;

	[SerializeField]
	protected Image imageValue;

	[SerializeField]
	protected TextMeshProUGUI textValue;

	[SerializeField]
	protected Transform countersHolder;

	[SerializeField]
	protected GameObject content;

	[SerializeField]
	protected T attackModCounterPrefab;

	[SerializeField]
	protected Image highlight;

	protected List<T> attackModCounters = new List<T>();

	public AttackModifierYMLData ModifierData;

	private bool shown;

	public string ModifierId => ModifierData?.Name;

	public virtual void Init(AttackModifierYMLData modifier, int counters)
	{
		ModifierData = modifier;
		Sprite attackModifierIcon = UIInfoTools.Instance.GetAttackModifierIcon(modifier);
		imageValue.gameObject.SetActive(attackModifierIcon != null);
		imageValue.sprite = attackModifierIcon;
		string attackModifierText = UIInfoTools.Instance.GetAttackModifierText(modifier);
		textValue.gameObject.SetActive(attackModifierText.IsNOTNullOrEmpty() || attackModifierIcon == null);
		textValue.text = attackModifierText;
		textValue.color = UIInfoTools.Instance.GetAttackModifierColor(modifier.MathModifier);
		rolling.SetActive(modifier.Rolling);
		Show(counters > 0);
		UpdateCounters(counters);
	}

	public virtual void InitEmpty()
	{
		foreach (T attackModCounter in attackModCounters)
		{
			Object.Destroy(attackModCounter.gameObject);
		}
		attackModCounters.Clear();
		ModifierData = null;
		Show(show: false);
		UpdateCounters(0);
	}

	private void Show(bool show)
	{
		shown = show;
		if (shown)
		{
			content.SetActive(value: true);
			rolling.SetActive(ModifierData != null && ModifierData.Rolling);
		}
		else
		{
			content.SetActive(value: false);
			rolling.SetActive(value: false);
		}
	}

	public virtual void UpdateCounters(int counters)
	{
		highlight.enabled = false;
		if (!shown && counters > 0)
		{
			Show(show: true);
		}
		else if (shown && counters == 0)
		{
			Show(show: false);
		}
		HelperTools.NormalizePool(ref attackModCounters, attackModCounterPrefab.gameObject, countersHolder, counters);
	}

	public void Highlight()
	{
		highlight.enabled = true;
	}
}
