using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace WorldspaceUI;

public class EffectsBar : MonoBehaviour
{
	[Flags]
	public enum FEffect
	{
		None = 0,
		Blessed = 1,
		Cursed = 2,
		Disarmed = 4,
		Immobilized = 8,
		Invisible = 0x10,
		Muddled = 0x20,
		Pierce = 0x40,
		Poisoned = 0x80,
		Retaliate = 0x100,
		Strengthened = 0x200,
		Stunned = 0x400,
		Wounded = 0x800,
		Advantage = 0x1000,
		Disadvantage = 0x2000,
		Controlled = 0x4000,
		StopFlying = 0x8000,
		ImprovedShortRest = 0x10000,
		Flying = 0x20000,
		Sleep = 0x40000,
		BlockHealing = 0x80000,
		NeutralizeShield = 0x100000,
		Baned = 0x200000,
		Regenerate = 0x400000,
		Ward = 0x800000,
		Brittle = 0x1000000,
		Impaired = 0x2000000,
		Mutate = 0x4000000,
		ShadowMark = 0x8000000,
		Push = 0x10000000,
		Pull = 0x20000000
	}

	[Serializable]
	public class EffectRef
	{
		public FEffect m_EffectType;

		public GameObject m_EffectObject;

		private Tween tween;

		private Action onCancel;

		public bool IsAnimating => tween != null;

		public void SetAnimation(Tween tween, Action onCancel = null)
		{
			CancelAnimation();
			this.tween = tween;
			this.onCancel = onCancel;
		}

		public void CancelAnimation()
		{
			if (tween != null)
			{
				tween.Kill();
				tween = null;
				m_EffectObject.transform.localScale = Vector3.one;
				onCancel?.Invoke();
			}
		}
	}

	public class DTO
	{
		public bool Blessed;

		public bool Controlled;

		public bool Cursed;

		public bool Disarmed;

		public bool Immobilized;

		public bool Invisible;

		public bool Muddled;

		public bool Pierce;

		public bool Poisoned;

		public bool Retaliate;

		public Dictionary<int, int> RetaliateRangeDamagePairs;

		public bool Strengthened;

		public bool Stunned;

		public bool Wounded;

		public bool Immovable;

		public bool ImprovedShortRest;

		public bool Fly;

		public List<CActiveBonus> Doomed;

		public List<CActiveBonus> ActiveItemEffectsOnMonsters;

		public bool Sleep;

		public bool CarryingQuestItem;

		public bool BlockHealing;

		public bool NeutralizeShield;

		public bool Regenerate;

		public bool Bane;

		public bool Ward;

		public bool Mutate;

		public bool Brittle;

		public bool Impaired;

		public List<CCharacterResource> CharacterResources;

		public bool ShadowMarked;

		public CPlayerActor.EPressure Pressure;

		public List<CActiveBonus> ActiveScenarioModifiersBonuses;
	}

	[SerializeField]
	private List<EffectRef> m_EffectsRefs;

	private FEffect m_EffectState;

	[SerializeField]
	private EffectRetaliateList _retaliateList;

	[SerializeField]
	private EffectRetaliate _retaliateEffect;

	[SerializeField]
	private EffectRetaliate _stunEffect;

	[SerializeField]
	private EffectRetaliate _immobilizeEffect;

	[SerializeField]
	private EffectRetaliate _disarmEffect;

	[SerializeField]
	private EffectRetaliate _impairEffect;

	[SerializeField]
	private EffectRetaliate _sleepingEffect;

	[SerializeField]
	private CustomEffect customEffectPrefab;

	[SerializeField]
	private List<CustomEffect> m_CustomEffectsSlots = new List<CustomEffect>();

	private Dictionary<string, CustomEffect> m_CustomEffectsRefs = new Dictionary<string, CustomEffect>();

	private List<string> doomedEffects = new List<string>();

	private List<string> itemEffects = new List<string>();

	private List<string> resourcesEffects = new List<string>();

	private List<string> scenarioEffects = new List<string>();

	[Header("Replace condition")]
	[SerializeField]
	private float replaceShowConditionDuration = 1f;

	[SerializeField]
	private float replaceShowReplaceIconDelay;

	[SerializeField]
	private float replaceShowReplaceIconDuration = 1f;

	[SerializeField]
	private float replaceIconHighlightFadeOutDuration = 1f;

	[SerializeField]
	private float replaceConditionRotateDelay;

	[SerializeField]
	private float replaceConditionRotateDuration = 1f;

	[SerializeField]
	private float replaceIconShineDelay;

	[SerializeField]
	private float replaceIconShineDuration = 1f;

	[SerializeField]
	private float replaceAnimationSpeed = 1f;

	[SerializeField]
	private GameObject replaceGameObject;

	[SerializeField]
	private Image replaceShine;

	[SerializeField]
	private Image replaceGlow;

	[SerializeField]
	private Image replaceConditionIcon;

	[SerializeField]
	private CanvasGroup replaceIconCanvasGroup;

	public EffectRef GetEffectUI(FEffect eff)
	{
		return m_EffectsRefs.FirstOrDefault((EffectRef x) => x.m_EffectType == eff);
	}

	private void Awake()
	{
		SetBlinkActive(FEffect.Poisoned, isActive: false, forceAction: true);
		SetBlinkActive(FEffect.Wounded, isActive: false, forceAction: true);
		if (_retaliateEffect != null)
		{
			_retaliateEffect.Initialise();
		}
		if (_stunEffect != null)
		{
			_stunEffect.Initialise();
		}
		if (_immobilizeEffect != null)
		{
			_immobilizeEffect.Initialise();
		}
		if (_disarmEffect != null)
		{
			_disarmEffect.Initialise();
		}
		if (_impairEffect != null)
		{
			_impairEffect.Initialise();
		}
		if (_sleepingEffect != null)
		{
			_sleepingEffect.Initialise();
		}
		if (replaceShine != null)
		{
			replaceShine.material = new Material(replaceShine.material);
		}
	}

	private FEffect ToEffect(CCondition.EPositiveCondition condition)
	{
		return condition switch
		{
			CCondition.EPositiveCondition.Advantage => FEffect.Advantage, 
			CCondition.EPositiveCondition.Bless => FEffect.Blessed, 
			CCondition.EPositiveCondition.Invisible => FEffect.Invisible, 
			CCondition.EPositiveCondition.Strengthen => FEffect.Strengthened, 
			CCondition.EPositiveCondition.Regenerate => FEffect.Regenerate, 
			CCondition.EPositiveCondition.Ward => FEffect.Ward, 
			CCondition.EPositiveCondition.Mutate => FEffect.Mutate, 
			_ => FEffect.None, 
		};
	}

	public void UpdateEffects(DTO dto)
	{
		if (dto.Blessed != m_EffectState.HasFlag(FEffect.Blessed))
		{
			ToggleEffect(FEffect.Blessed);
		}
		if (dto.Controlled != m_EffectState.HasFlag(FEffect.Controlled))
		{
			ToggleEffect(FEffect.Controlled);
		}
		if (dto.Cursed != m_EffectState.HasFlag(FEffect.Cursed))
		{
			ToggleEffect(FEffect.Cursed);
		}
		if (dto.Disarmed != m_EffectState.HasFlag(FEffect.Disarmed))
		{
			ToggleEffect(FEffect.Disarmed);
		}
		if (dto.Immobilized != m_EffectState.HasFlag(FEffect.Immobilized))
		{
			ToggleEffect(FEffect.Immobilized);
		}
		if (dto.Invisible != m_EffectState.HasFlag(FEffect.Invisible))
		{
			ToggleEffect(FEffect.Invisible);
		}
		if (dto.Muddled != m_EffectState.HasFlag(FEffect.Muddled))
		{
			ToggleEffect(FEffect.Muddled);
		}
		if (dto.Pierce != m_EffectState.HasFlag(FEffect.Pierce))
		{
			ToggleEffect(FEffect.Pierce);
		}
		if (dto.Poisoned != m_EffectState.HasFlag(FEffect.Poisoned))
		{
			ToggleEffect(FEffect.Poisoned);
		}
		if (dto.Retaliate != m_EffectState.HasFlag(FEffect.Retaliate))
		{
			ToggleEffect(FEffect.Retaliate);
		}
		if (dto.Strengthened != m_EffectState.HasFlag(FEffect.Strengthened))
		{
			ToggleEffect(FEffect.Strengthened);
		}
		if (dto.Stunned != m_EffectState.HasFlag(FEffect.Stunned))
		{
			ToggleEffect(FEffect.Stunned);
		}
		if (dto.Wounded != m_EffectState.HasFlag(FEffect.Wounded))
		{
			ToggleEffect(FEffect.Wounded);
		}
		_ = dto.ImprovedShortRest;
		m_EffectState.HasFlag(FEffect.ImprovedShortRest);
		if (dto.Fly != m_EffectState.HasFlag(FEffect.Flying))
		{
			ToggleEffect(FEffect.Flying);
		}
		if (dto.Sleep != m_EffectState.HasFlag(FEffect.Sleep))
		{
			ToggleEffect(FEffect.Sleep);
		}
		if (dto.BlockHealing != m_EffectState.HasFlag(FEffect.BlockHealing))
		{
			ToggleEffect(FEffect.BlockHealing);
		}
		if (dto.NeutralizeShield != m_EffectState.HasFlag(FEffect.NeutralizeShield))
		{
			ToggleEffect(FEffect.NeutralizeShield);
		}
		if (dto.Regenerate != m_EffectState.HasFlag(FEffect.Regenerate))
		{
			ToggleEffect(FEffect.Regenerate);
		}
		if (dto.Bane != m_EffectState.HasFlag(FEffect.Baned))
		{
			ToggleBaneEffect(dto.Bane);
		}
		if (dto.Ward != m_EffectState.HasFlag(FEffect.Ward))
		{
			ToggleEffect(FEffect.Ward);
		}
		if (dto.Mutate != m_EffectState.HasFlag(FEffect.Mutate))
		{
			ToggleEffect(FEffect.Mutate);
		}
		if (dto.Brittle != m_EffectState.HasFlag(FEffect.Brittle))
		{
			ToggleEffect(FEffect.Brittle);
		}
		if (dto.Impaired != m_EffectState.HasFlag(FEffect.Impaired))
		{
			ToggleEffect(FEffect.Impaired);
		}
		if (doomedEffects.Count != 0 || dto.Doomed.Count != 0)
		{
			List<string> list = doomedEffects.ToList();
			foreach (CActiveBonus item in dto.Doomed)
			{
				string text = item.BaseCard.ID.ToString();
				list.Remove(text);
				ToggleDoomEffect(text, item);
			}
			foreach (string item2 in list)
			{
				UntoggleDoomEffect(item2);
			}
		}
		if (itemEffects.Count != 0 || !dto.ActiveItemEffectsOnMonsters.IsNullOrEmpty())
		{
			List<string> list2 = itemEffects.ToList();
			foreach (CActiveBonus activeItemEffectsOnMonster in dto.ActiveItemEffectsOnMonsters)
			{
				string text2 = activeItemEffectsOnMonster.BaseCard.ID.ToString();
				list2.Remove(text2);
				ToggleItemEffect(text2, activeItemEffectsOnMonster);
			}
			foreach (string item3 in list2)
			{
				UntoggleItemEffect(item3);
			}
		}
		List<string> list3 = resourcesEffects.ToList();
		if (!dto.CharacterResources.IsNullOrEmpty())
		{
			foreach (CCharacterResource characterResource in dto.CharacterResources)
			{
				if (characterResource.Amount > 0)
				{
					list3.Remove(characterResource.ID);
					ToggleCharacterResourceEffect(characterResource);
				}
			}
		}
		foreach (string item4 in list3)
		{
			UntoggleCharacterResourceEffect(item4);
		}
		UpdatePressureEffect(dto.Pressure);
		UpdateRetaliateEffect(dto.RetaliateRangeDamagePairs);
		if (dto.ShadowMarked)
		{
			ToggleShadowMarkedEffect();
		}
		else
		{
			UntoggleShadowMarkedEffect();
		}
		if (dto.CarryingQuestItem)
		{
			ToggleCarryingQuestItemEffect();
		}
		else
		{
			UntoggleCarryingQuestItemEffect();
		}
		if (scenarioEffects.Count == 0)
		{
			List<CActiveBonus> activeScenarioModifiersBonuses = dto.ActiveScenarioModifiersBonuses;
			if (activeScenarioModifiersBonuses == null || activeScenarioModifiersBonuses.Count <= 0)
			{
				return;
			}
		}
		List<string> list4 = scenarioEffects.ToList();
		foreach (CActiveBonus activeScenarioModifiersBonuse in dto.ActiveScenarioModifiersBonuses)
		{
			string text3 = $"{activeScenarioModifiersBonuse.BaseCard.ID.ToString()} {activeScenarioModifiersBonuse.Ability.ID}";
			if (activeScenarioModifiersBonuse.BaseCard.CardType == CBaseCard.ECardType.Dummy)
			{
				text3 = $"{activeScenarioModifiersBonuse.Ability.Name} {activeScenarioModifiersBonuse.Ability.ID}";
			}
			list4.Remove(text3);
			ToggleScenarioModEffect(text3, activeScenarioModifiersBonuse);
		}
		foreach (string item5 in list4)
		{
			UntoggleScenarioModEffect(item5);
		}
	}

	private void ToggleCharacterResourceEffect(CCharacterResource characterResource)
	{
		if (!resourcesEffects.Contains(characterResource.ID) && characterResource?.ResourceData?.Hidden != true)
		{
			AddCustomEffect(characterResource.ID, UIInfoTools.Instance.GetActiveAbilityIcon(characterResource.ResourceData.Sprite), LocalizationManager.GetTranslation(characterResource.ResourceData.LocKey));
			resourcesEffects.Add(characterResource.ID);
		}
	}

	public void UntoggleCharacterResourceEffect(string id)
	{
		if (resourcesEffects.Contains(id))
		{
			resourcesEffects.Remove(id);
			RemoveCustomEffect(id);
		}
	}

	private void ToggleItemEffect(string id, CActiveBonus bonus)
	{
		if (!itemEffects.Contains(id))
		{
			string descriptionTooltip = null;
			if (bonus.Ability.AbilityText.IsNOTNullOrEmpty())
			{
				descriptionTooltip = string.Format(CreateLayout.LocaliseText(bonus.Ability.AbilityText), CreateLayout.LocaliseText(bonus.Caster.Class.LocKey));
			}
			else if (bonus.Ability.PreviewEffectText.IsNOTNullOrEmpty())
			{
				descriptionTooltip = PreviewEffectGenerator.GetPreviewEffectTextWithVars(bonus.Ability);
			}
			AddCustomEffect(id, UIInfoTools.Instance.GetActiveAbilityIcon("Hover" + bonus.Caster.GetPrefabName(), bonus.Caster.GetPrefabName()), "<size=+2><color=#" + UIInfoTools.DefInstance.basicTextColor.ToHex() + ">" + LocalizationManager.GetTranslation(bonus.BaseCard.Name) + "</color></size>", descriptionTooltip);
			itemEffects.Add(id);
		}
	}

	public void UntoggleItemEffect(string id)
	{
		if (itemEffects.Contains(id))
		{
			RemoveCustomEffect(id);
		}
	}

	public void ToggleDoomEffect(string id, CAbility ability, CDoom doom)
	{
		ToggleDoomEffect(id, ability, ability.GetAbilityBaseCard(), doom.ActiveBonusLayout);
	}

	public void ToggleDoomEffect(string id, CActiveBonus bonus)
	{
		if (bonus.Layout?.ListLayouts != null)
		{
			ToggleDoomEffect(id, bonus.Ability, bonus.BaseCard, bonus.Layout.ListLayouts.FirstOrDefault());
		}
	}

	private void ToggleDoomEffect(string id, CAbility ability, CBaseCard card, string activeBonusLayout)
	{
		if (!doomedEffects.Contains(id))
		{
			AddCustomEffect(id, UIInfoTools.Instance.GetPreviewEffectConfig(ECharacter.Doomstalker.ToString()).previewEffectIcon, "<size=+2><color=#" + UIInfoTools.DefInstance.basicTextColor.ToHex() + ">" + LocalizationManager.GetTranslation(card.Name) + "</color></size>", "<color=#" + UIInfoTools.Instance.GetCharacterColor(ECharacter.Doomstalker).ToHex() + ">" + LocalizationManager.GetTranslation("ChooseAbility") + ":</color> " + ActivePropertyLookup(ability, LocalisationAndPropertyLookup(ability, activeBonusLayout)));
			doomedEffects.Add(id);
		}
	}

	private void ToggleBaneEffect(bool hasBane)
	{
		EffectRef effectRef = ToggleEffect(FEffect.Baned);
		if (hasBane)
		{
			effectRef.m_EffectObject.GetComponent<UITextTooltipTarget>().SetText(string.Format(LocalizationManager.GetTranslation("Glossary_Bane"), 10));
		}
	}

	private void ToggleScenarioModEffect(string id, CActiveBonus bonus)
	{
		try
		{
			if (bonus.Layout?.ListLayouts == null)
			{
				UIActiveAbility.CheckForLayoutToGenerate(bonus);
				if (bonus.Layout?.ListLayouts == null)
				{
					return;
				}
			}
		}
		catch (Exception exception)
		{
			Log.GUIException(exception, string.Format("{0} failed for bonus with id {1}!", "ToggleScenarioModEffect", bonus.ID));
			return;
		}
		string descriptionTooltip = null;
		if (!string.IsNullOrEmpty(bonus.Ability.ActiveBonusData.ListLayoutOverride.Item1))
		{
			List<string> list = bonus.Ability.ListLayoutOverrideParams(bonus);
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (LocalizationManager.TryGetTranslation(list[i], out var translation))
					{
						list[i] = translation;
					}
				}
			}
			int strength = bonus.Ability.Strength;
			descriptionTooltip = CreateLayout.LocaliseText(bonus.Ability.ActiveBonusData.ListLayoutOverride.Item1, skipWarnings: false, strength.ToString(), list?.FirstOrDefault());
		}
		if (scenarioEffects.Contains(id) && m_CustomEffectsRefs.ContainsKey(id))
		{
			m_CustomEffectsRefs[id].ShowEffect(UIInfoTools.Instance.GetActiveAbilityIcon(bonus.Layout.IconNames.FirstOrDefault()), null, descriptionTooltip);
			return;
		}
		AddCustomEffect(id, UIInfoTools.Instance.GetActiveAbilityIcon(bonus.Layout.IconNames.FirstOrDefault()), null, descriptionTooltip);
		scenarioEffects.Add(id);
	}

	public void UntoggleDoomEffect(string id)
	{
		if (doomedEffects.Contains(id))
		{
			RemoveCustomEffect(id);
		}
	}

	public void UntoggleScenarioModEffect(string id)
	{
		if (scenarioEffects.Contains(id))
		{
			scenarioEffects.Remove(id);
			RemoveCustomEffect(id);
		}
	}

	private string ActivePropertyLookup(CAbility ability, string text)
	{
		while (text.Contains('^'.ToString()))
		{
			string text2 = CardLayoutRow.GetKey(text, '^').Replace(" ", "");
			int num = 0;
			if (text2 == ability.Name)
			{
				num += ability.Strength;
			}
			text = CardLayoutRow.ReplaceKey(text, '^', num.ToString());
		}
		return text;
	}

	private string LocalisationAndPropertyLookup(CAbility ability, string desc)
	{
		if (desc == null)
		{
			desc = string.Empty;
		}
		desc = CreateLayout.LocaliseText(desc);
		desc = (desc.Contains('*'.ToString()) ? CardLayoutRow.ReplaceKey(desc, '*', ability.Strength.ToString()) : desc);
		return desc;
	}

	public void AccentuateEffect(FEffect effect)
	{
		if (m_EffectState.HasFlag(effect))
		{
			EffectRef effectRef = m_EffectsRefs.Single((EffectRef ef) => ef.m_EffectType == effect);
			if (effectRef != null && !effectRef.IsAnimating)
			{
				RectTransform component = effectRef.m_EffectObject.GetComponent<RectTransform>();
				component.localScale = new Vector3(0.4f, 0.4f, 0.4f);
				component.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack, 2.5f);
			}
		}
	}

	public void ImmobilizedWarnEffect(bool active)
	{
		if (_immobilizeEffect != null)
		{
			_immobilizeEffect.Warn(active);
		}
	}

	public void DisarmedWarnEffect(bool active)
	{
		if (_disarmEffect != null)
		{
			_disarmEffect.Warn(active);
		}
	}

	public void ImpairedWarnEffect(bool active)
	{
		if (_impairEffect != null)
		{
			_impairEffect.Warn(active);
		}
	}

	public void StunnedWarnEffect(bool active)
	{
		if (_stunEffect != null)
		{
			_stunEffect.Warn(active);
		}
	}

	public void SleepingWarnEffect(bool active)
	{
		if (_sleepingEffect != null)
		{
			_sleepingEffect.Warn(active);
		}
	}

	private EffectRef ToggleEffect(FEffect effect)
	{
		m_EffectState = (FEffect)m_EffectState.GetWithInvertedFlag(effect);
		EffectRef effectRef = m_EffectsRefs.SingleOrDefault((EffectRef ef) => ef.m_EffectType == effect);
		if (effectRef != null && !effectRef.IsAnimating)
		{
			effectRef.m_EffectObject.SetActive(m_EffectState.HasFlag(effect));
		}
		return effectRef;
	}

	public void EnableFlag(FEffect flag)
	{
		EffectRef effectRef = m_EffectsRefs.Single((EffectRef ef) => ef.m_EffectType == flag);
		m_EffectState |= flag;
		effectRef.m_EffectObject.SetActive(value: true);
	}

	public void DisableFlag(FEffect flag)
	{
		m_EffectState &= ~flag;
		EffectRef effectRef = m_EffectsRefs.Single((EffectRef ef) => ef.m_EffectType == flag);
		effectRef.CancelAnimation();
		effectRef.m_EffectObject.SetActive(value: false);
	}

	public void SetBlinkActive(FEffect effect, bool isActive = false, bool forceAction = false)
	{
		if (m_EffectState.HasFlag(effect) | forceAction)
		{
			IEffectBlink component = m_EffectsRefs.Single((EffectRef ef) => ef.m_EffectType == effect).m_EffectObject.GetComponent<IEffectBlink>();
			if (component != null)
			{
				component.enabled = isActive;
			}
		}
	}

	public void DisableFlags()
	{
		m_EffectsRefs.ForEach((EffectRef it) =>
		{
			m_EffectState &= ~it.m_EffectType;
			it.m_EffectObject.SetActive(value: false);
			it.CancelAnimation();
		});
		foreach (KeyValuePair<string, CustomEffect> customEffectsRef in m_CustomEffectsRefs)
		{
			customEffectsRef.Value.Hide();
			m_CustomEffectsSlots.Add(customEffectsRef.Value);
		}
		m_CustomEffectsRefs.Clear();
		doomedEffects.Clear();
		itemEffects.Clear();
		scenarioEffects.Clear();
	}

	public void ToggleEffect(FEffect effect, bool active)
	{
		if (active != m_EffectState.HasFlag(effect))
		{
			ToggleEffect(effect);
		}
	}

	public void RetaliateActiveEffect(int distanceToAttacker)
	{
		if (_retaliateList != null)
		{
			_retaliateList.ActivateEffects(distanceToAttacker);
		}
	}

	public void RetaliateWarnEffect(bool active, int distanceToAttacker)
	{
		if (_retaliateList != null)
		{
			_retaliateList.WarnEffects(active, distanceToAttacker);
		}
	}

	public void CancelRetaliateEffect()
	{
		if (_retaliateList != null)
		{
			_retaliateList.CancelEffects();
		}
	}

	public void RemoveCustomEffect(string id)
	{
		if (m_CustomEffectsRefs.ContainsKey(id))
		{
			m_CustomEffectsRefs[id].Hide();
			m_CustomEffectsSlots.Add(m_CustomEffectsRefs[id]);
			m_CustomEffectsRefs.Remove(id);
		}
	}

	public void AddCustomEffect(string id, Sprite icon, string titleTooltip = null, string descriptionTooltip = null, bool blink = false)
	{
		CustomEffect customEffect;
		if (m_CustomEffectsRefs.ContainsKey(id))
		{
			customEffect = m_CustomEffectsRefs[id];
		}
		else if (m_CustomEffectsSlots.Count > 0)
		{
			customEffect = m_CustomEffectsSlots[m_CustomEffectsSlots.Count - 1];
			m_CustomEffectsSlots.RemoveAt(m_CustomEffectsSlots.Count - 1);
		}
		else
		{
			customEffect = UnityEngine.Object.Instantiate(customEffectPrefab, transform);
		}
		m_CustomEffectsRefs[id] = customEffect;
		customEffect.ShowEffect(icon, titleTooltip, descriptionTooltip, blink);
	}

	public void SetCustomEffectBlinkActive(string id, bool active)
	{
		if (m_CustomEffectsRefs.ContainsKey(id))
		{
			m_CustomEffectsRefs[id].EnableBlink(active);
		}
	}

	public void ResetEffectBlink()
	{
		m_EffectsRefs.ForEach((EffectRef it) =>
		{
			IEffectBlink component = it.m_EffectObject.GetComponent<IEffectBlink>();
			if ((object)component != null)
			{
				component.enabled = false;
			}
		});
	}

	public void ResetCustomEffectBlink()
	{
		foreach (CustomEffect value in m_CustomEffectsRefs.Values)
		{
			value.EnableBlink(blink: false);
		}
	}

	public void UntoggleCarryingQuestItemEffect()
	{
		RemoveCustomEffect(ScenarioManager.ObjectImportType.CarryableQuestItem.ToString());
	}

	public void ToggleCarryingQuestItemEffect()
	{
		AddCustomEffect(ScenarioManager.ObjectImportType.CarryableQuestItem.ToString(), UIInfoTools.Instance.CarryableQuestItemWorldSprite);
	}

	public void UntoggleShadowMarkedEffect()
	{
		RemoveCustomEffect(ScenarioManager.ObjectImportType.Shadow.ToString());
	}

	public void ToggleShadowMarkedEffect()
	{
		AddCustomEffect(ScenarioManager.ObjectImportType.Shadow.ToString(), UIInfoTools.Instance.GetActiveAbilityIcon("DWLShadow"), LocalizationManager.GetTranslation("FH/Deathwalker"), LocalizationManager.GetTranslation("FH/090_character_ability_cards_deathwalker_call_to_the_abyss_2"));
	}

	public void UpdatePressureEffect(CPlayerActor.EPressure pressure)
	{
		RemoveCustomEffect("PressureEffect");
		if (pressure != CPlayerActor.EPressure.None)
		{
			AddCustomEffect("PressureEffect", UIInfoTools.Instance.GetActiveAbilityIcon("MMPressure" + pressure), LocalizationManager.GetTranslation("FH/FS_Character_MetalMosaic"), pressure.ToString());
		}
	}

	private void UpdateRetaliateEffect(Dictionary<int, int> retaliateRangeDamagePairs)
	{
		if (_retaliateList != null)
		{
			_retaliateList.Init(retaliateRangeDamagePairs, retaliateRangeDamagePairs?.OrderBy((KeyValuePair<int, int> x) => x.Key)?.ToList());
		}
	}

	public void ReplaceCondition(FEffect oldCondition, CCondition.EPositiveCondition newCondition)
	{
		ReplaceCondition(oldCondition, ToEffect(newCondition));
	}

	public void ReplaceCondition(FEffect oldCondition, FEffect newCondition)
	{
		EffectRef effectRef = m_EffectsRefs.Single((EffectRef ef) => ef.m_EffectType == oldCondition);
		EffectRef newEffectRef = m_EffectsRefs.Single((EffectRef ef) => ef.m_EffectType == newCondition);
		effectRef.m_EffectObject.SetActive(value: true);
		effectRef.SetAnimation(effectRef.m_EffectObject.transform.DOScale(Vector3.zero, replaceShowConditionDuration * replaceAnimationSpeed).SetEase(Ease.InBack).OnComplete(() =>
		{
			effectRef.m_EffectObject.SetActive(m_EffectState.HasFlag(oldCondition));
			effectRef.CancelAnimation();
		}));
		replaceGameObject.SetActive(value: false);
		replaceGameObject.transform.localScale = Vector3.zero;
		replaceIconCanvasGroup.alpha = 0f;
		replaceIconCanvasGroup.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
		replaceGlow.SetAlpha(1f);
		replaceGlow.transform.localScale = Vector3.one;
		replaceShine.material.SetTextureOffset("_Mask", new Vector2(0f, -1f));
		replaceConditionIcon.sprite = effectRef.m_EffectObject.GetComponent<Image>().sprite;
		newEffectRef.m_EffectObject.SetActive(value: false);
		newEffectRef.m_EffectObject.transform.localScale = Vector3.zero;
		Sequence sequence = DOTween.Sequence(replaceGameObject);
		sequence.AppendInterval(replaceShowConditionDuration * replaceAnimationSpeed).AppendCallback(() =>
		{
			replaceGameObject.SetActive(value: true);
		}).Append(replaceGameObject.transform.DOScale(Vector3.one, replaceShowConditionDuration * replaceAnimationSpeed).SetEase(Ease.OutBack))
			.AppendInterval(replaceShowReplaceIconDelay * replaceAnimationSpeed)
			.Append(DOTween.To((float val) =>
			{
				replaceIconCanvasGroup.alpha = 0.3f + val * 0.7f;
				replaceIconCanvasGroup.transform.localScale = Vector3.one * (0.3f + 0.7f * val);
			}, 0f, 1f, replaceShowReplaceIconDuration * replaceAnimationSpeed).SetTarget(replaceIconCanvasGroup))
			.Append(DOTween.To((float val) =>
			{
				replaceGlow.SetAlpha(1f - val);
				replaceGlow.transform.localScale = new Vector3(1f + val * 0.3f, 1f + val * 0.3f);
			}, 0f, 1f, replaceIconHighlightFadeOutDuration * replaceAnimationSpeed).SetTarget(replaceGlow))
			.AppendInterval(replaceIconShineDelay * replaceAnimationSpeed)
			.Append(DOTween.To((float val) =>
			{
				replaceShine.material.SetTextureOffset("_Mask", new Vector2(0f, -1f + 2f * val));
			}, -1f, 1f, replaceIconShineDuration * replaceAnimationSpeed).SetTarget(replaceShine))
			.AppendInterval(replaceConditionRotateDelay * replaceAnimationSpeed)
			.Append(replaceGameObject.transform.DORotate(new Vector3(0f, 90f, 0f), replaceConditionRotateDuration / 2f * replaceAnimationSpeed))
			.AppendCallback(() =>
			{
				replaceConditionIcon.sprite = newEffectRef.m_EffectObject.GetComponentInChildren<Image>().sprite;
				replaceIconCanvasGroup.alpha = 0f;
			})
			.Append(replaceGameObject.transform.DORotate(new Vector3(0f, 0f, 0f), replaceConditionRotateDuration / 2f * replaceAnimationSpeed))
			.AppendInterval(replaceShowReplaceIconDelay * replaceAnimationSpeed)
			.Append(replaceGameObject.transform.DOScale(Vector3.zero, replaceShowConditionDuration * replaceAnimationSpeed).SetEase(Ease.InBack))
			.AppendCallback(() =>
			{
				replaceGameObject.SetActive(value: false);
				newEffectRef.m_EffectObject.SetActive(value: true);
				newEffectRef.m_EffectObject.transform.localScale = Vector3.zero;
			})
			.Append(newEffectRef.m_EffectObject.transform.DOScale(Vector3.one, replaceShowConditionDuration * replaceAnimationSpeed).SetEase(Ease.OutBack))
			.AppendCallback(() =>
			{
				newEffectRef.m_EffectObject.SetActive(m_EffectState.HasFlag(newCondition));
				newEffectRef.CancelAnimation();
			});
		newEffectRef.SetAnimation(sequence, () =>
		{
			replaceGameObject.SetActive(value: false);
		});
	}

	private void OnDisable()
	{
		m_EffectsRefs.ForEach((EffectRef it) =>
		{
			it.CancelAnimation();
		});
	}
}
