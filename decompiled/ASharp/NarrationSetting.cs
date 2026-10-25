using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Services;
using FHRL;
using GLOOM;
using I2.Loc;

public class NarrationSetting
{
	public struct Setting
	{
		public struct ChoiceOption
		{
			public string Text;

			public Action Action;
		}

		public string Image;

		public string Text;

		public string TextLocalized;

		public string Voiceover;

		public ChoiceOption[] Options;
	}

	public List<Setting> Story = new List<Setting>();

	public bool NotEmpty => Story.Any();

	public static NarrationSetting FromCampaignSection(CampaignSectionDef def)
	{
		string text = "Event_" + def.ID.ID + "_Text";
		NarrationSetting narrationSetting = new NarrationSetting();
		narrationSetting.Story = new List<Setting>();
		for (int i = 1; i <= 100; i++)
		{
			string text2 = text + "_" + i;
			if (I2.Loc.LocalizationManager.GetTermData("FHS/" + text2) != null)
			{
				narrationSetting.Story.Add(new Setting
				{
					Text = text2
				});
			}
		}
		if (narrationSetting.Story.Count == 0)
		{
			narrationSetting.Story.Add(new Setting
			{
				Text = text
			});
		}
		return narrationSetting;
	}

	[Obsolete]
	public static NarrationSetting FromCampaignSectionRequirement(CampaignSectionDef def, CampaignEffect e)
	{
		string text = $"Event_{def.ID.ID}_{e.Type}_Text";
		if (!GLOOM.LocalizationManager.TryGetTranslation(text, out var _))
		{
			return null;
		}
		Setting item = new Setting
		{
			Text = text
		};
		return new NarrationSetting
		{
			Story = new List<Setting> { item }
		};
	}

	public static string FromCustomEventOutcome(OutpostEventState ev)
	{
		string text = string.Empty;
		if (ev.CustomOutcomeLocKeys == null)
		{
			return text;
		}
		foreach (OutpostEventState.CustomOutcomeKey customOutcomeLocKey in ev.CustomOutcomeLocKeys)
		{
			string translation = GLOOM.LocalizationManager.GetTranslation(customOutcomeLocKey.locKey);
			if (customOutcomeLocKey.appendLine)
			{
				text += "<br>";
			}
			if (customOutcomeLocKey.locParams != null)
			{
				string text2 = text;
				object[] locParams = customOutcomeLocKey.locParams;
				text = text2 + string.Format(translation, locParams);
			}
			else
			{
				text += translation;
			}
		}
		return text;
	}

	public static Setting FromOutpostEventOutcome(OutpostEventState ev)
	{
		string arg = "";
		if (ev.OutcomeVariant != 0)
		{
			arg = char.ToString((char)(65 + ev.OutcomeVariant));
		}
		string text = $"{EventIterationKey(ev.Def, ev.Completed ? ev.OutcomeIteration : (ev.OutcomeIteration - 1))}_Option_{ev.OutcomeOptionID}{arg}_Outcome";
		if (I2.Loc.LocalizationManager.GetTermData("FH/" + text) == null)
		{
			text = $"{EventIterationKey(ev.Def)}_Option_{ev.OutcomeOptionID}{arg}_Outcome";
		}
		return new Setting
		{
			Text = text
		};
	}

	public static Setting FromOutpostEventFront(OutpostEventState ev)
	{
		if (ev.OutcomeIteration == 0)
		{
			string text = EventIterationKey(ev.Def) + "_Front";
			return new Setting
			{
				Text = text
			};
		}
		return FromOutpostEventOutcome(ev);
	}

	public static Setting FromOutpostEventDef(OutpostEventDef def)
	{
		string text = def.ID.ID + "_Front";
		return new Setting
		{
			Text = text
		};
	}

	private static string EventIterationKey(OutpostEventDef def, int iteration = 0)
	{
		if (iteration == 0)
		{
			return def.ID.ID ?? "";
		}
		return $"{def.ID.ID}_{iteration}";
	}

	public static string EventAttackOutcomeKey(OutpostEventState ev)
	{
		string arg = "";
		if (ev.OutcomeVariant != 0)
		{
			arg = char.ToString((char)(65 + ev.OutcomeVariant));
		}
		string text = $"{ev.Def.ID.ID}_Attack_Reward_{ev.OptionSelectedID}{arg}";
		if (!GLOOM.LocalizationManager.TryGetTranslation(text, out var _))
		{
			text = ev.Def.ID.ID + "_Attack_Reward";
		}
		return text;
	}

	private static string EventIterationKey(OutpostEventState ev)
	{
		return EventIterationKey(ev.Def, ev.OutcomeIteration);
	}

	public static Setting FromOutpostEventOption(OutpostEventState ev, OutpostEventDef.Option option)
	{
		if (!string.IsNullOrEmpty(option.CustomLocKey))
		{
			if (option.objectParams != null && option.objectParams.Length != 0)
			{
				string translation = I2.Loc.LocalizationManager.GetTranslation(option.CustomLocKey);
				string[] array = new string[option.objectParams.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = option.objectParams[i].ToString();
					if (option.objectParams[i] is CharacterClassDef characterClassDef)
					{
						OutpostEventsService service = Singleton<FHMapChoreographer>.Instance.View.GetService<OutpostEventsService>();
						array[i] = service.GetHeroNameLocalized(characterClassDef.ID.ID);
					}
				}
				Setting result = default;
				object[] args = array;
				result.TextLocalized = string.Format(translation, args);
				return result;
			}
			return new Setting
			{
				Text = option.CustomLocKey
			};
		}
		string text = $"{EventIterationKey(ev)}_Option_{option.ID}";
		return new Setting
		{
			Text = text
		};
	}

	public static Setting FromOutpostEventOption(OutpostEventDef def, int option)
	{
		string text = $"{EventIterationKey(def)}_Option_{option}";
		return new Setting
		{
			Text = text
		};
	}

	public static NarrationSetting FromConclustion(ScenarioState scenario, CampaignController controller, ScenarioDef.Conclusion conclusion)
	{
		string text = scenario.GetScenarionLevelID(controller) + "_Outro_";
		if (I2.Loc.LocalizationManager.GetTermData("FHS/" + text + "1") == null)
		{
			text = text.RemoveSpaces().RemoveChar('\'');
			if (I2.Loc.LocalizationManager.GetTermData("FHS/" + text + "1") == null)
			{
				text += scenario.Def.Conclusions.IndexOf(conclusion) switch
				{
					1 => "B_", 
					2 => "C_", 
					_ => "A_", 
				};
			}
		}
		NarrationSetting narrationSetting = new NarrationSetting();
		for (int i = 1; i <= 100; i++)
		{
			string text2 = text + i;
			if (I2.Loc.LocalizationManager.GetTermData("FHS/" + text2) == null)
			{
				break;
			}
			narrationSetting.Story.Add(new Setting
			{
				Text = text2,
				Voiceover = text2,
				Image = text2
			});
		}
		return narrationSetting;
	}

	public static NarrationSetting FromIntro(ScenarioState scenario, CampaignController controller)
	{
		string text = scenario.GetScenarionLevelID(controller) + "_Intro_";
		NarrationSetting narrationSetting = new NarrationSetting();
		for (int i = 1; i <= 100; i++)
		{
			string text2 = text + i;
			string term = "FHS/" + text2;
			if (I2.Loc.LocalizationManager.GetTermData(term) == null)
			{
				text2 = text2.RemoveSpaces().RemoveChar('\'');
				term = "FHS/" + text2;
			}
			if (I2.Loc.LocalizationManager.GetTermData(term) == null)
			{
				break;
			}
			narrationSetting.Story.Add(new Setting
			{
				Text = text2,
				Voiceover = text2,
				Image = text2
			});
		}
		return narrationSetting;
	}

	public static NarrationSetting FromResolveChooseAlternativeLevel(string questionKey, string answer1Key, string answer2Key, string illustrationKey, FHMapChoreographer Choreographer)
	{
		NarrationSetting narrationSetting = new NarrationSetting();
		narrationSetting.Story = new List<Setting>(1)
		{
			new Setting
			{
				Text = questionKey,
				Image = illustrationKey,
				Options = new Setting.ChoiceOption[2]
				{
					new Setting.ChoiceOption
					{
						Text = answer1Key,
						Action = () =>
						{
							Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveChooseAlternativeScenarioLevel, false);
						}
					},
					new Setting.ChoiceOption
					{
						Text = answer2Key,
						Action = () =>
						{
							Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveChooseAlternativeScenarioLevel, true);
						}
					}
				}
			}
		};
		return narrationSetting;
	}

	public static NarrationSetting FromResolveChooseUnlockMandatoryScenario(string questionKey, string answer1Key, string answer2Key, string illustrationKey, int scenario1, int scenario2, FHMapChoreographer Choreographer)
	{
		NarrationSetting narrationSetting = new NarrationSetting();
		narrationSetting.Story = new List<Setting>(1)
		{
			new Setting
			{
				Text = questionKey,
				Voiceover = questionKey,
				Image = illustrationKey,
				Options = new Setting.ChoiceOption[2]
				{
					new Setting.ChoiceOption
					{
						Text = answer1Key,
						Action = () =>
						{
							Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveChooseUnlockMandatoryScenario, scenario1);
						}
					},
					new Setting.ChoiceOption
					{
						Text = answer2Key,
						Action = () =>
						{
							Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveChooseUnlockMandatoryScenario, scenario2);
						}
					}
				}
			}
		};
		return narrationSetting;
	}

	public static NarrationSetting FromAlternativeScenarioLevelChosen(ScenarioState scenario, CampaignController controller)
	{
		string text = scenario.GetScenarionLevelID(controller) + "_Choice_";
		NarrationSetting narrationSetting = new NarrationSetting();
		for (int i = 1; i <= 100; i++)
		{
			string text2 = text + i;
			TermData termData = I2.Loc.LocalizationManager.GetTermData("FHS/" + text2);
			if (termData == null)
			{
				break;
			}
			narrationSetting.Story.Add(new Setting
			{
				Text = text2,
				Voiceover = termData.Term.Substring(4, termData.Term.Length - 4),
				Image = text2
			});
		}
		return narrationSetting;
	}

	public static NarrationSetting FromResolveChooseCampaignSection(string questionKey, string answer1Key, string answer2Key, string answer1Section, string answer2Section, FHMapChoreographer Choreographer)
	{
		NarrationSetting narrationSetting = new NarrationSetting();
		if (!GLOOM.LocalizationManager.TryGetTranslation(answer1Key, out var translation))
		{
			answer1Key = "YES";
		}
		if (!GLOOM.LocalizationManager.TryGetTranslation(answer2Key, out translation))
		{
			answer2Key = "NO";
		}
		narrationSetting.Story = new List<Setting>(1)
		{
			new Setting
			{
				Text = questionKey,
				Options = new Setting.ChoiceOption[2]
				{
					new Setting.ChoiceOption
					{
						Text = answer1Key,
						Action = () =>
						{
							Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveChooseNextCampaignSection, answer1Section);
						}
					},
					new Setting.ChoiceOption
					{
						Text = answer2Key,
						Action = () =>
						{
							Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveChooseNextCampaignSection, answer2Section);
						}
					}
				}
			}
		};
		return narrationSetting;
	}
}
