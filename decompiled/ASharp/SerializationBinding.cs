using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ScenarioRuleLibrary.CustomLevels;

public class SerializationBinding : SerializationBinder
{
	private readonly string _assemblyInfoSaveDir;

	public SerializationBinding(string assemblyInfoSaveDir)
	{
		_assemblyInfoSaveDir = assemblyInfoSaveDir;
	}

	public override Type BindToType(string assemblyName, string typeName)
	{
		if (assemblyName.Contains("Assembly-CSharp,"))
		{
			assemblyName = assemblyName.Replace("Assembly-CSharp,", "GH.Runtime,");
		}
		if (typeName.Contains("Assembly-CSharp,"))
		{
			typeName = typeName.Replace("Assembly-CSharp,", "GH.Runtime,");
		}
		if (typeName.Contains("CardProcessing.RoguelikeProcessing.RoguelikePartyBoss"))
		{
			assemblyName = assemblyName.Replace("ScenarioRuleLibrary", "MapRuleLibrary");
			typeName = typeName.Replace("CardProcessing.RoguelikeProcessing.RoguelikePartyBoss", "MapRuleLibrary.OldAdventureMode.RoguelikePartyBoss");
			typeName = typeName.Replace("ScenarioRuleLibrary", "MapRuleLibrary");
		}
		else if (typeName.Contains("CardProcessing.RoguelikeProcessing.RoguelikePartyDifficulty"))
		{
			assemblyName = assemblyName.Replace("ScenarioRuleLibrary", "MapRuleLibrary");
			typeName = typeName.Replace("CardProcessing.RoguelikeProcessing.RoguelikePartyDifficulty", "MapRuleLibrary.Adventure.CAdventureDifficulty");
			typeName = typeName.Replace("ScenarioRuleLibrary", "MapRuleLibrary");
		}
		else if (typeName.Contains("CardProcessing.RoguelikeProcessing.EAdventureDifficulty"))
		{
			assemblyName = assemblyName.Replace("ScenarioRuleLibrary", "MapRuleLibrary");
			typeName = typeName.Replace("CardProcessing.RoguelikeProcessing.EAdventureDifficulty", "MapRuleLibrary.Adventure.EAdventureDifficulty");
			typeName = typeName.Replace("ScenarioRuleLibrary", "MapRuleLibrary");
		}
		else if (typeName.Contains("CardProcessing.RoguelikeProcessing.EAdventurePathDifficulty"))
		{
			assemblyName = assemblyName.Replace("ScenarioRuleLibrary", "MapRuleLibrary");
			typeName = typeName.Replace("CardProcessing.RoguelikeProcessing.EAdventurePathDifficulty", "MapRuleLibrary.Adventure.EAdventurePathDifficulty");
			typeName = typeName.Replace("ScenarioRuleLibrary", "MapRuleLibrary");
		}
		else if (typeName.Contains("CardProcessing.RoguelikeProcessing.RoguelikeScenarioPossibleRoom"))
		{
			typeName = typeName.Replace("CardProcessing.RoguelikeProcessing.RoguelikeScenarioPossibleRoom", "ScenarioRuleLibrary.YML.ScenarioPossibleRoom");
		}
		else if (typeName.Contains("CardProcessing"))
		{
			typeName = typeName.Replace("CardProcessing", "ScenarioRuleLibrary.YML");
		}
		else if (typeName.Contains("ScenarioRuleLibrary.CAbilityPreventDamage+PreventDamageState"))
		{
			typeName = typeName.Replace("PreventDamageState", "EPreventDamageState");
		}
		else if (typeName.Contains("ScenarioRuleLibrary.CAbilityDamage+DamageState"))
		{
			typeName = typeName.Replace("DamageState", "EDamageState");
		}
		else if (typeName.Contains("ScenarioRuleLibrary.CAbilityPull+ActorNameMoved"))
		{
			typeName = typeName.Replace("ActorNameMoved", "PulledActorStats");
		}
		else if (typeName.Contains("ScenarioRuleLibrary.CAbilityPush+ActorNameMoved"))
		{
			typeName = typeName.Replace("ActorNameMoved", "PushedActorStats");
		}
		else if (typeName.Contains("ScenarioRuleLibrary.CAbilityLoot+LootState"))
		{
			typeName = typeName.Replace("LootState", "ELootState");
		}
		else
		{
			if (typeName.Contains("LevelEditorStandardData"))
			{
				if (typeName.Contains("System.Collections.Generic.List"))
				{
					return typeof(List<CCustomLevelData>);
				}
				return typeof(CCustomLevelData);
			}
			if (typeName.Contains("ELevelPartyChoiceType"))
			{
				return typeof(ELevelPartyChoiceType);
			}
			if (typeName.Contains("ELevelMessageLayoutType"))
			{
				return typeof(CLevelMessage.ELevelMessageLayoutType);
			}
			if (typeName.Match("(?:[\\W]|^)(LevelMessage)(?=[\\W]|$)"))
			{
				if (typeName.Contains("System.Collections.Generic.List"))
				{
					return typeof(List<CLevelMessage>);
				}
				return typeof(CLevelMessage);
			}
			if (typeName.Contains("ELevelEventType"))
			{
				return typeof(CLevelEvent.ELevelEventType);
			}
			if (typeName.Match("(?:[\\W]|^)(LevelEvent)(?=[\\W]|$)"))
			{
				if (typeName.Contains("System.Collections.Generic.List"))
				{
					return typeof(List<CLevelEvent>);
				}
				return typeof(CLevelEvent);
			}
			if (typeName.Contains("ELevelMessagePredefinedDisplayTrigger"))
			{
				return typeof(CLevelTrigger.ELevelMessagePredefinedDisplayTrigger);
			}
			if (typeName.Contains("LevelMessageTrigger"))
			{
				return typeof(CLevelTrigger);
			}
			if (typeName.Contains("LevelMessagePage"))
			{
				if (typeName.Contains("System.Collections.Generic.List"))
				{
					return typeof(List<CLevelMessagePage>);
				}
				return typeof(CLevelMessagePage);
			}
			if (typeName.Contains("EControlBehaviourType"))
			{
				return typeof(CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType);
			}
			if (typeName.Contains("InteractibilityLimitedControl"))
			{
				if (typeName.Contains("System.Collections.Generic.List"))
				{
					return typeof(List<CLevelUIInteractionProfile.CLevelUIInteractionSpecific>);
				}
				return typeof(CLevelUIInteractionProfile.CLevelUIInteractionSpecific);
			}
			if (typeName.Contains("EIsolatedControlType"))
			{
				return typeof(CLevelUIInteractionProfile.EIsolatedControlType);
			}
			if (typeName.Contains("InteractabilityProfile"))
			{
				return typeof(CLevelUIInteractionProfile);
			}
			if (typeName.Contains("LevelMessageCameraPositionProfile"))
			{
				return typeof(CLevelCameraProfile);
			}
			if (typeName.Contains("System.Random"))
			{
				typeName = typeName.Replace("System", "SharedLibrary");
			}
		}
		return Type.GetType($"{typeName}, {assemblyName}");
	}
}
