using System;
using System.Linq;
using FHGUI;
using FHGUI.Common;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class AbilityCardActionLocalizedListener : MonoBehaviour
{
	private TMP_Text _text;

	[TextArea]
	[SerializeField]
	private string key;

	private const string CARD_LOC_SEPARATOR = " ";

	public string Key => key;

	public void Initialize()
	{
		if (_text == null)
		{
			_text = GetComponent<TMP_Text>();
		}
	}

	protected void OnEnable()
	{
		Initialize();
	}

	public void OnLanguageChanged(string cardName, CAction action, CardLayoutRow.PropertyLookupContext context = null, CardLayoutRow.GetStatValuesHandler getStatValues = null)
	{
		Initialize();
		string text = LocalizeAbility(key, cardName, action, context, getStatValues);
		_text.text = text;
	}

	public static string LocalizeAbility(string key, string cardName, CAction action, CardLayoutRow.PropertyLookupContext context = null, CardLayoutRow.GetStatValuesHandler getStatValues = null)
	{
		string text = "";
		try
		{
			if ((object)context == null)
			{
				context = new CardLayoutRow.PropertyLookupContext();
			}
			CAbility[] array = CAbilityCard.GetActionAbilities(action).ToArray();
			if (context.TargetingActor == null && array.Length != 0)
			{
				context.TargetingActor = array[0].TargetingActor;
			}
			if (!context.summonBaseHealthModifier.HasValue)
			{
				context.summonBaseHealthModifier = UICharacterUtils.GetSummonHealthModifier();
			}
			CardLayoutRow cardLayoutRow = new CardLayoutRow(key, cardName, array, calculateIsBasedOnX: ScenarioManager.Scenario != null && context.Actor != null, overrides: CAbilityCard.GetAbilityOverrides(action));
			text = CreateLayout.LocaliseText(key, skipWarnings: true);
			text = cardLayoutRow.PropertyLookup(text, context, getStatValues, DecorateAbilityText, throwErrorIfNotFound: false);
			if (text.Contains('$'))
			{
				text = CreateLayout.LocaliseText(text, skipWarnings: true);
			}
		}
		catch (Exception ex)
		{
			Log.Warning("Localization for ability text " + key + " in card " + cardName + " failed with " + ex.Message);
			text = "<color=red>?";
		}
		return text;
	}

	private static string DecorateAbilityText(CardLayoutRow.AbilityPropertyTextParams textParams)
	{
		if (textParams.Enhancement <= 0)
		{
			return textParams.Value;
		}
		return FHUILocFormatting.Buff(textParams.Value);
	}

	public string DecorateAbilityTextWithSprite(CardLayoutRow.AbilityPropertyTextParams textParams)
	{
		CAbility ability = textParams.Ability;
		if (!(ability is CAbilityAdvantage))
		{
			if (!(ability is CAbilityAttack))
			{
				if (!(ability is CAbilityAttackersGainDisadvantage))
				{
					if (!(ability is CAbilityHeal))
					{
						if (!(ability is CAbilityDamage))
						{
							if (!(ability is CAbilityLoot))
							{
								if (!(ability is CAbilityMove))
								{
									if (!(ability is CAbilityPull))
									{
										if (!(ability is CAbilityPush))
										{
											if (!(ability is CAbilityShield))
											{
												if (!(ability is CAbilityTeleport))
												{
													if (!(ability is CAbilityTeleport2))
													{
														if (ability is CAbilityXP)
														{
															return "<sprite name=XP" + textParams.Value + ">";
														}
														_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(ability);
														string result = default;
														return result;
													}
													return "<sprite name=Teleport> " + textParams.Value;
												}
												return "<sprite name=Teleport> " + textParams.Value;
											}
											return "<sprite name=Shield> " + textParams.Value;
										}
										return "<sprite name=Pull> " + textParams.Value;
									}
									return "<sprite name=Push> " + textParams.Value;
								}
								return "<sprite name=Move> " + textParams.Value;
							}
							return "<sprite name=Loot> " + textParams.Value;
						}
						return "<sprite name=Damage> " + textParams.Value;
					}
					return "<sprite name=Heal> " + textParams.Value;
				}
				return "<sprite name=Disadvantage> " + textParams.Value;
			}
			return "<sprite name=Attack> " + textParams.Value;
		}
		return "<sprite name=Advantage> " + textParams.Value;
	}

	public void SetKey(string newKey)
	{
		key = newKey;
	}
}
