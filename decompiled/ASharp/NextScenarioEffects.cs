using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary;

[Serializable]
public class NextScenarioEffects : ISerializable
{
	public List<ElementInfusionBoardManager.EElement> Infusions;

	public int Damage;

	public List<NegativeConditionPair> EnemyNegativeConditions { get; private set; }

	public List<PositiveConditionPair> EnemyPositiveConditions { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Infusions", Infusions);
		info.AddValue("Damage", Damage);
		info.AddValue("EnemyNegativeConditions", EnemyNegativeConditions);
		info.AddValue("EnemyPositiveConditions", EnemyPositiveConditions);
	}

	private NextScenarioEffects(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Infusions":
					Infusions = (List<ElementInfusionBoardManager.EElement>)info.GetValue(item.Name, typeof(List<ElementInfusionBoardManager.EElement>));
					break;
				case "Damage":
					Damage = info.GetInt32(item.Name);
					break;
				case "EnemyNegativeConditions":
					EnemyNegativeConditions = (List<NegativeConditionPair>)info.GetValue(item.Name, typeof(List<NegativeConditionPair>));
					break;
				case "EnemyPositiveConditions":
					EnemyPositiveConditions = (List<PositiveConditionPair>)info.GetValue(item.Name, typeof(List<PositiveConditionPair>));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Exception while trying to deserialize NextScenarioEffects entry " + item.Name);
				throw ex;
			}
		}
	}

	public NextScenarioEffects()
	{
		Infusions = new List<ElementInfusionBoardManager.EElement>();
		EnemyNegativeConditions = new List<NegativeConditionPair>();
		EnemyPositiveConditions = new List<PositiveConditionPair>();
	}
}
