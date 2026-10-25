using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[StateDump(UseToString = true, NoMembers = true)]
public class UIAttackModifierCalculator : MonoBehaviour
{
	[SerializeField]
	private TMP_Text attackModText;

	[SerializeField]
	private Image attackModImage;

	[SerializeField]
	private Color defaultTextColor;

	[SerializeField]
	private Transform container;

	[SerializeField]
	private GameObject unfocusedMask;

	private List<Image> attackModCounters = new List<Image>();

	public override string ToString()
	{
		return attackModText?.text;
	}

	public void SetupCounters(string counterID, CActor actor = null, CCharacterClass charClass = null, bool isFocused = true)
	{
		int availableCounters = 0;
		int usedCounters = 0;
		if (charClass == null)
		{
			if (actor == null)
			{
				availableCounters = MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
				usedCounters = MonsterClassManager.EnemyMonsterAttackModifierDeck.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
			}
			else if (actor is CEnemyActor enemyActor)
			{
				CMonsterAttackModifierDeck attackModifiersDeck = MonsterClassManager.GetAttackModifiersDeck(enemyActor);
				availableCounters = attackModifiersDeck.AttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
				usedCounters = attackModifiersDeck.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
			}
			else
			{
				Log.GUIError("Tried to setup counters for non-enemy actor without a character class");
			}
		}
		else
		{
			usedCounters = charClass.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
			availableCounters = charClass.AttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
		}
		SetupCounters(availableCounters, usedCounters);
		SetUnfocused(!isFocused);
	}

	public void SetupCounters(int availableCounters, int usedCounters)
	{
		attackModCounters.ForEach((Image x) =>
		{
			Object.Destroy(x.gameObject);
		});
		attackModCounters.Clear();
		for (int num = 0; num < availableCounters + usedCounters; num++)
		{
			attackModCounters.Add(Object.Instantiate(Singleton<ActorStatPanel>.Instance.CounterPrefab, (container == null) ? transform : container, worldPositionStays: false).GetComponent<Image>());
		}
		int num2 = availableCounters - 1;
		if (availableCounters == 0)
		{
			if (attackModText != null)
			{
				attackModText.color = UIInfoTools.DefInstance.greyedOutTextColor;
			}
			if (attackModImage != null)
			{
				attackModImage.color = UIInfoTools.DefInstance.greyedOutTextColor;
			}
			for (int num3 = 0; num3 < attackModCounters.Count; num3++)
			{
				attackModCounters[num3].sprite = Singleton<ActorStatPanel>.Instance.UsedCounter;
			}
			return;
		}
		if (attackModText != null)
		{
			attackModText.color = defaultTextColor;
		}
		if (attackModImage != null)
		{
			attackModImage.color = defaultTextColor;
		}
		for (int num4 = 0; num4 < attackModCounters.Count; num4++)
		{
			if (num4 <= num2)
			{
				attackModCounters[num4].sprite = Singleton<ActorStatPanel>.Instance.AvailableCounter;
			}
			else
			{
				attackModCounters[num4].sprite = Singleton<ActorStatPanel>.Instance.UsedCounter;
			}
		}
	}

	public void SetUnfocused(bool unfocused)
	{
		unfocusedMask.SetActive(unfocused);
	}

	private bool IsBasicModifier(AttackModifierYMLData modif, string counterId)
	{
		if (modif.MathModifier.Equals(counterId) && !modif.IsConditionalModifier && !modif.IsBless)
		{
			return !modif.IsCurse;
		}
		return false;
	}
}
