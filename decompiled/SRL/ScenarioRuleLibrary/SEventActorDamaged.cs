using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorDamaged : SEventActor
{
	public int Avoided;

	public int Shielded;

	public int ItemShielded;

	public bool IsMelee;

	public int Shield;

	public int PoisonDamage;

	public List<PositiveConditionPair> PositiveConditionsBeforeDamage;

	public int HealthBeforeDamage;

	public int DamageTaken { get; private set; }

	public CActor.ECauseOfDamage CauseOfDamage { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DamageTaken", DamageTaken);
		info.AddValue("CauseOfDamage", CauseOfDamage);
		info.AddValue("Avoided", Avoided);
		info.AddValue("Shielded", Shielded);
		info.AddValue("ItemShielded", Shielded);
		info.AddValue("IsMelee", IsMelee);
		info.AddValue("Shield", Shield);
		info.AddValue("PoisonDamage", PoisonDamage);
		info.AddValue("PositiveConditionsBeforeDamage", PositiveConditionsBeforeDamage);
		info.AddValue("HealthBeforeDamage", HealthBeforeDamage);
	}

	public SEventActorDamaged(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "HealthBeforeDamage":
					HealthBeforeDamage = info.GetInt32("HealthBeforeDamage");
					break;
				case "PositiveConditionsBeforeDamage":
					PositiveConditionsBeforeDamage = (List<PositiveConditionPair>)info.GetValue("PositiveConditionsBeforeDamage", typeof(List<PositiveConditionPair>));
					break;
				case "DamageTaken":
					DamageTaken = info.GetInt32("DamageTaken");
					break;
				case "CauseOfDamage":
					CauseOfDamage = (CActor.ECauseOfDamage)info.GetValue("CauseOfDamage", typeof(CActor.ECauseOfDamage));
					break;
				case "Avoided":
					Avoided = info.GetInt32("Avoided");
					break;
				case "Shielded":
					Shielded = info.GetInt32("Shielded");
					break;
				case "ItemShielded":
					ItemShielded = info.GetInt32("ItemShielded");
					break;
				case "IsMelee":
					IsMelee = info.GetBoolean("IsMelee");
					break;
				case "Shield":
					Shield = info.GetInt32("Shield");
					break;
				case "PoisonDamage":
					PoisonDamage = info.GetInt32("PoisonDamage");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActorDamaged entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorDamaged(SEventActorBaseData baseData, int damageTaken, CActor.ECauseOfDamage causeOfDamage, int avoided = 0, int shielded = 0, int itemShielded = 0, bool isMelee = false, int poisonAmt = 0, int shield = 0, List<PositiveConditionPair> positiveConditionsBeforeDamage = null, int healthBeforeDamage = 0)
		: base(baseData)
	{
		DamageTaken = damageTaken;
		CauseOfDamage = causeOfDamage;
		Avoided = avoided;
		Shielded = shielded;
		ItemShielded = itemShielded;
		IsMelee = isMelee;
		PoisonDamage = poisonAmt;
		Shield = shield;
		PositiveConditionsBeforeDamage = positiveConditionsBeforeDamage;
		HealthBeforeDamage = healthBeforeDamage;
	}
}
