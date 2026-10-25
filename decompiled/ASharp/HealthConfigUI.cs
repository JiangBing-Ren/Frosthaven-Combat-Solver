using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Health Level", fileName = "HealthConfig")]
public class HealthConfigUI : ScriptableObject
{
	public int health;

	public List<HealthZoomConfigUI> zoomConfigs;

	public HealthZoomConfigUI GetZoomConfig(float zoom)
	{
		return (from it in zoomConfigs
			where it.zoomMax >= zoom
			orderby it.zoomMax
			select it).FirstOrDefault();
	}
}
