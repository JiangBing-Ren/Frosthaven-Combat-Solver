using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityMerged : CAbility
{
	[JsonProperty]
	protected List<CAbility> m_MergedAbilities;

	protected List<CAbility> m_CopiedMergedAbilities;

	protected CAbility m_ActiveAbility;

	protected CActor m_StartTargetActor;

	protected CActor m_StartFilterActor;

	public List<CAbility> MergedAbilities => m_MergedAbilities;

	public List<CAbility> CopiedMergedAbilities => m_CopiedMergedAbilities;

	public CAbility ActiveAbility => m_ActiveAbility;

	public CAbilityMerged(CAbility abilityA, CAbility abilityB)
	{
		m_MergedAbilities = new List<CAbility>();
		m_CopiedMergedAbilities = new List<CAbility>();
		m_MergedAbilities.Add(abilityA);
		m_MergedAbilities.Add(abilityB);
		abilityA.ParentAbility = this;
		abilityB.ParentAbility = this;
	}

	public virtual void CopyMergedAbilities()
	{
		m_CopiedMergedAbilities.Clear();
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_StartTargetActor = targetActor;
		m_StartFilterActor = filterActor;
		SetHasStarted(val: true);
	}

	public virtual CAbility GetMergedWithAbility(CAbility ability)
	{
		if (!m_MergedAbilities[0].Equals(ability))
		{
			return m_MergedAbilities[0];
		}
		return m_MergedAbilities[1];
	}

	public override CAbilityOverride OverrideAbilityValues(CAbilityOverride abilityOverride, bool perform, CItem item = null, CAbilityFilterContainer filter = null, CActor targetActor = null, bool fromModifier = false)
	{
		CAbilityOverride result = null;
		if (item == null)
		{
			CAbility cAbility = m_CopiedMergedAbilities.SingleOrDefault((CAbility x) => x.Name == abilityOverride.ParentName);
			if (cAbility != null)
			{
				result = cAbility.OverrideAbilityValues(abilityOverride, perform, item, filter);
			}
			else
			{
				Log.ErrorWithReport("Error trying to override Merged ability");
			}
		}
		else
		{
			result = m_ActiveAbility.OverrideAbilityValues(abilityOverride, perform, item, filter, null, fromModifier);
		}
		return result;
	}

	public override void UndoOverride(CAbilityOverride abilityOverride, bool perform, CItem item = null)
	{
		if (item == null)
		{
			CAbility cAbility = m_CopiedMergedAbilities.SingleOrDefault((CAbility x) => x.Name == abilityOverride.ParentName);
			if (cAbility != null)
			{
				cAbility.UndoOverride(abilityOverride, perform, item);
			}
			else
			{
				Log.ErrorWithReport("Error trying to undo override Merged ability");
			}
		}
		else
		{
			m_ActiveAbility.UndoOverride(abilityOverride, perform, item);
		}
	}

	public override void ClearTargets()
	{
		m_ActiveAbility.ClearTargets();
	}

	public override void ToggleSingleTargetItem(CItem item)
	{
		m_ActiveAbility.ToggleSingleTargetItem(item);
	}

	public override void Restart()
	{
		m_ActiveAbility.Restart();
	}

	public override bool CanReceiveTileSelection()
	{
		return ActiveAbility?.CanReceiveTileSelection() ?? false;
	}

	public override bool RequiresWaypointSelection()
	{
		return ActiveAbility?.RequiresWaypointSelection() ?? false;
	}

	public override bool ShouldRestartAbilityWhenApplyingOverride(CAbilityOverride abilityOverride)
	{
		return ActiveAbility?.ShouldRestartAbilityWhenApplyingOverride(abilityOverride) ?? false;
	}
}
