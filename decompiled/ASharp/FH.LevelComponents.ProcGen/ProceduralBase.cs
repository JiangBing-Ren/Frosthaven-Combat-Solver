using System;
using FH.ApparanceBaking;
using FH.Tools;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

[ExecuteAlways]
public class ProceduralBase : MonoBehaviour, IContentUpdateMonitor
{
	protected ApparanceStepDebugScaffolding apparanceDebug = new ApparanceStepDebugScaffolding();

	private ProceduralMapTile maptileParent;

	[NonSerialized]
	private bool preGenerated;

	public bool PreGenerated => preGenerated;

	private void Awake()
	{
		Transform transform = base.transform.Find("Generated Content");
		preGenerated = (bool)transform && transform.childCount > 0;
		ApparanceEntity component = GetComponent<ApparanceEntity>();
		if ((bool)component)
		{
			component.IsPopulated = false;
			apparanceDebug.RegisterEntity(component);
		}
		Watchman.Instance?.Complete(gameObject.name);
	}

	protected virtual void OnDestroy()
	{
		apparanceDebug.Dispose();
	}

	protected ProceduralMapTile GetRoom()
	{
		return GloomUtility.FindAncestorComponent<ProceduralMapTile>(this);
	}

	protected ProceduralStyle GetRoomStyle()
	{
		ProceduralMapTile room = GetRoom();
		if (room != null)
		{
			return GloomUtility.EnsureComponent<ProceduralStyle>(room.gameObject);
		}
		return null;
	}

	protected int GetRoomSeed()
	{
		ProceduralStyle roomStyle = GetRoomStyle();
		if (roomStyle != null)
		{
			return roomStyle.Seed;
		}
		return 0;
	}

	internal ProceduralStyle GetParentStyle()
	{
		ProceduralStyle roomStyle = GetRoomStyle();
		if (roomStyle != null)
		{
			return roomStyle;
		}
		return GetScenarioStyle();
	}

	internal ProceduralScenario GetScenario()
	{
		ProceduralScenario componentInParent = GetComponentInParent<ProceduralScenario>();
		if (componentInParent != null)
		{
			return componentInParent;
		}
		_ = gameObject.scene;
		if (gameObject.scene.isLoaded)
		{
			GameObject[] rootGameObjects = gameObject.scene.GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				ProceduralScenario component = rootGameObjects[i].GetComponent<ProceduralScenario>();
				if (component != null)
				{
					return component;
				}
			}
		}
		return null;
	}

	internal ProceduralStyle GetScenarioStyle()
	{
		ProceduralScenario scenario = GetScenario();
		if (scenario != null)
		{
			return GloomUtility.EnsureComponent<ProceduralStyle>(scenario.gameObject);
		}
		return null;
	}

	protected int GetScenarioSeed()
	{
		ProceduralStyle scenarioStyle = GetScenarioStyle();
		if (scenarioStyle != null)
		{
			return scenarioStyle.Seed;
		}
		return 0;
	}

	public virtual void NotifyContentPlacementStarted()
	{
	}

	public virtual void NotifyContentPlacementComplete()
	{
		EnvironmentLoader.ProceduralContentCreated(this);
	}

	private ProceduralMapTile GetProceduralMaptileParent()
	{
		return maptileParent ?? (maptileParent = gameObject.GetComponentInParent<ProceduralMapTile>()) ?? (maptileParent = gameObject.GetComponent<ProceduralMapTile>());
	}

	public virtual void NotifyContentRemovalStarted()
	{
	}

	public virtual void NotifyContentRemovalComplete()
	{
	}
}
