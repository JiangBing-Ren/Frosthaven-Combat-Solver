using System;
using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CActionAugmentation
{
	private string m_Name;

	private List<ElementInfusionBoardManager.EElement> m_Elements = new List<ElementInfusionBoardManager.EElement>();

	private int m_Trohpies;

	private int m_Resonance;

	private CAbility m_CostAbility;

	private List<ElementInfusionBoardManager.EElement> m_Infusions = new List<ElementInfusionBoardManager.EElement>();

	private List<CActionAugmentationOp> m_AugmentationOps = new List<CActionAugmentationOp>();

	private int m_XP;

	private AbilityConsume.ConsumeLayoutProperties m_LayoutProperties;

	private List<bool> m_IsAnyElement;

	private string m_PreviewEffectId;

	private string m_PreviewEffectText;

	private string m_ConsumeGroup;

	public string Name => m_Name;

	public List<ElementInfusionBoardManager.EElement> Elements => m_Elements;

	public int Trohpies => m_Trohpies;

	public int Resonance => m_Resonance;

	public CAbility CostAbility => m_CostAbility;

	public List<ElementInfusionBoardManager.EElement> Infusions => m_Infusions;

	public List<CActionAugmentationOp> AugmentationOps => m_AugmentationOps;

	public int XP => m_XP;

	public AbilityConsume.ConsumeLayoutProperties LayoutProperties => m_LayoutProperties;

	public Guid ActionID { get; set; }

	public string PreviewEffectId => m_PreviewEffectId;

	public string PreviewEffectText => m_PreviewEffectText;

	public string ConsumeGroup => m_ConsumeGroup;

	public bool ConsumeOnlyOneElement { get; private set; }

	public CAbilityRequirements Requirements { get; private set; }

	public CActionAugmentation()
	{
	}

	public CActionAugmentation(string name, List<ElementInfusionBoardManager.EElement> elements, int trophies, int resonance, CAbility costAbility, List<ElementInfusionBoardManager.EElement> infusions, List<CActionAugmentationOp> augmentationOps, int xp, AbilityConsume.ConsumeLayoutProperties layoutProperties, string previewEffectId, string previewEffectText, string consumeGroup, bool consumeOnlyOneElement = false, CAbilityRequirements requirements = null)
	{
		m_Name = name;
		m_Elements = elements;
		m_Trohpies = trophies;
		m_Resonance = resonance;
		m_CostAbility = costAbility;
		m_Infusions = infusions;
		m_AugmentationOps = augmentationOps;
		m_XP = xp;
		m_LayoutProperties = layoutProperties;
		m_PreviewEffectId = previewEffectId;
		m_PreviewEffectText = previewEffectText;
		m_ConsumeGroup = consumeGroup;
		ConsumeOnlyOneElement = consumeOnlyOneElement;
		Requirements = requirements;
		m_IsAnyElement = new List<bool>();
		foreach (ElementInfusionBoardManager.EElement element in elements)
		{
			m_IsAnyElement.Add(element == ElementInfusionBoardManager.EElement.Any);
		}
	}

	public CActionAugmentation Copy()
	{
		return new CActionAugmentation
		{
			ActionID = ActionID,
			m_Name = m_Name,
			m_Elements = m_Elements.ToList(),
			m_Trohpies = m_Trohpies,
			m_Resonance = m_Resonance,
			m_CostAbility = CAbility.CopyAbility(m_CostAbility, generateNewID: false),
			m_Infusions = m_Infusions.ToList(),
			m_AugmentationOps = m_AugmentationOps.Select((CActionAugmentationOp s) => s.Copy()).ToList(),
			m_XP = m_XP,
			m_LayoutProperties = m_LayoutProperties.Copy(),
			m_IsAnyElement = m_IsAnyElement.ToList(),
			m_PreviewEffectId = m_PreviewEffectId,
			m_PreviewEffectText = m_PreviewEffectText,
			m_ConsumeGroup = m_ConsumeGroup,
			ConsumeOnlyOneElement = ConsumeOnlyOneElement,
			Requirements = Requirements?.Copy()
		};
	}

	public void ResetElements()
	{
		for (int i = 0; i < Elements.Count; i++)
		{
			if (m_IsAnyElement[i])
			{
				Elements[i] = ElementInfusionBoardManager.EElement.Any;
			}
		}
	}

	public void ConsumeConsumables(CActor consumingActor)
	{
		ConsumeElements(consumingActor);
		ConsumeTrophies(consumingActor);
		ConsumeResonance(consumingActor);
	}

	protected void ConsumeElements(CActor consumingActor)
	{
		if (consumingActor.Class is CMonsterClass cMonsterClass)
		{
			if (cMonsterClass.NonEliteVariant == null)
			{
				_ = cMonsterClass.ID;
			}
			else
			{
				_ = cMonsterClass.NonEliteVariant.ID;
			}
			foreach (ElementInfusionBoardManager.EElement item in Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
			{
				ElementInfusionBoardManager.Consume(item, consumingActor);
			}
			{
				foreach (ElementInfusionBoardManager.EElement item2 in Elements.Where((ElementInfusionBoardManager.EElement x) => x == ElementInfusionBoardManager.EElement.Any))
				{
					_ = item2;
					List<ElementInfusionBoardManager.EElement> enemyConsumedElements = ElementInfusionBoardManager.GetEnemyConsumedElements(consumingActor);
					if (enemyConsumedElements != null && enemyConsumedElements.Count > 0)
					{
						ElementInfusionBoardManager.Consume(enemyConsumedElements[0], consumingActor);
						continue;
					}
					List<ElementInfusionBoardManager.EElement> availableElements = ElementInfusionBoardManager.GetAvailableElements(consumingActor);
					if (availableElements.Count > 0)
					{
						ElementInfusionBoardManager.Consume(availableElements[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(availableElements.Count)], consumingActor);
					}
				}
				return;
			}
		}
		foreach (ElementInfusionBoardManager.EElement item3 in Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
		{
			Log.Info("Element " + item3.ToString() + " has been consumed");
			ElementInfusionBoardManager.Consume(item3, consumingActor);
		}
		foreach (ElementInfusionBoardManager.EElement item4 in Elements.Where((ElementInfusionBoardManager.EElement x) => x == ElementInfusionBoardManager.EElement.Any))
		{
			_ = item4;
			List<ElementInfusionBoardManager.EElement> availableElements2 = ElementInfusionBoardManager.GetAvailableElements(consumingActor);
			if (availableElements2.Count > 0)
			{
				ElementInfusionBoardManager.Consume(availableElements2[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(availableElements2.Count)], consumingActor);
			}
		}
	}

	protected void ConsumeTrophies(CActor consumingActor)
	{
		if (Trohpies > 0 && consumingActor is CPlayerActor cPlayerActor && cPlayerActor.UsesTrophyTokens())
		{
			cPlayerActor.SetTrophyTokens(cPlayerActor.GetTrophyTokens() - Trohpies);
		}
	}

	protected void ConsumeResonance(CActor consumingActor)
	{
		if (Resonance > 0 && consumingActor is CPlayerActor cPlayerActor && cPlayerActor.UsesResonanceTokens())
		{
			cPlayerActor.SetResonanceTokens(cPlayerActor.GetResonanceTokens() - Resonance);
		}
	}
}
