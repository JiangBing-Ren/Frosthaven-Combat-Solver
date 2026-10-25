using System;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CCharacterResource
{
	private int m_Amount;

	public string ID { get; set; }

	public int Amount
	{
		get
		{
			return m_Amount;
		}
		set
		{
			m_Amount = Math.Max(0, value);
			if (ResourceData.MaxAmount.HasValue)
			{
				m_Amount = Math.Min(ResourceData.MaxAmount.Value, m_Amount);
			}
		}
	}

	public CharacterResourceData ResourceData => ScenarioRuleClient.SRLYML.CharacterResources.SingleOrDefault((CharacterResourceData x) => x.ID == ID);

	public CCharacterResource(string id, int amount)
	{
		ID = id;
		Amount = amount;
		if (ResourceData == null)
		{
			Log.ErrorWithReport("No resource data found for new CCharacterResource");
		}
	}
}
