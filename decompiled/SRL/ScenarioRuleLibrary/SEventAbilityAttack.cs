using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityAttack : SEventAbility
{
	public CAbilityAttack.EAttackState AttackState { get; private set; }

	public List<CAttackEffect> AttackEffects { get; private set; }

	public bool IsTargetedAbility { get; private set; }

	public bool AreaEffect { get; private set; }

	public List<string> TargetIDs { get; private set; }

	public List<bool> Advantage { get; set; }

	public List<int> ModifiedStrength { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Advantage", Advantage);
		info.AddValue("AttackState", AttackState);
		info.AddValue("AttackEffects", AttackEffects);
		info.AddValue("IsTargetedAbility", IsTargetedAbility);
		info.AddValue("AreaEffect", AreaEffect);
		info.AddValue("TargetIDs", TargetIDs);
		info.AddValue("ModifiedStrength", ModifiedStrength);
	}

	public SEventAbilityAttack(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Advantage":
					Advantage = (List<bool>)info.GetValue("Advantage", typeof(List<bool>));
					break;
				case "AttackState":
					AttackState = (CAbilityAttack.EAttackState)info.GetValue("AttackState", typeof(CAbilityAttack.EAttackState));
					break;
				case "AttackEffects":
					AttackEffects = (List<CAttackEffect>)info.GetValue("AttackEffects", typeof(List<CAttackEffect>));
					break;
				case "IsTargetedAbility":
					IsTargetedAbility = info.GetBoolean("IsTargetedAbility");
					break;
				case "AreaEffect":
					AreaEffect = info.GetBoolean("AreaEffect");
					break;
				case "Targets":
					TargetIDs = (List<string>)info.GetValue("TargetIDs", typeof(List<string>));
					break;
				case "ModifiedStrength":
					ModifiedStrength = (List<int>)info.GetValue("ModifiedStrength", typeof(List<int>));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityAttack entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityAttack(SEventAbilityBaseData baseData, CAbilityAttack.EAttackState attackState, List<CAttackEffect> attackEffects, bool isTargetedAbility, bool areaEffect, List<string> targets, List<bool> advantage, List<int> modifiedStrength)
		: base(baseData)
	{
		AttackState = attackState;
		AttackEffects = attackEffects;
		IsTargetedAbility = isTargetedAbility;
		AreaEffect = areaEffect;
		TargetIDs = targets;
		Advantage = advantage;
		ModifiedStrength = modifiedStrength;
	}
}
