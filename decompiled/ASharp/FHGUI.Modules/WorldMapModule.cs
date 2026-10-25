using FHGUI.Elements;
using UnityEngine;

namespace FHGUI.Modules;

public class WorldMapModule : FHMapViewModule
{
	[SerializeField]
	private FHWorldMapScene _worldMapRoot;

	protected override bool PlaySoundOnEnable => false;

	public void SetSceneRootReference(FHWorldMapScene root)
	{
		_worldMapRoot = root;
	}

	public void DeselectScenario()
	{
		_worldMapRoot.StopFocusingOnScenario();
	}
}
