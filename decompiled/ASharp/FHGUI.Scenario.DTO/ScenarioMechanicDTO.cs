using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Scenario.DTO;

public class ScenarioMechanicDTO
{
	public string ID;

	public bool Seen;

	public ScenarioMechanicsConfig.Entry.Category StyleCategory;

	public GameObject Content;

	public string SimpleContentKey;
}
