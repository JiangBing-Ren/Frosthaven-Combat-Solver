using System;
using System.Collections.Generic;
using AStar;
using ScenarioRuleLibrary;
using UnityEngine;

public class PropFading
{
	private readonly HashSet<PropFadeAnimation> _running = new HashSet<PropFadeAnimation>();

	private static readonly List<ScenarioManager.ObjectImportType> EXCLUDED_OBJECT_TYPES = new List<ScenarioManager.ObjectImportType>
	{
		ScenarioManager.ObjectImportType.Door,
		ScenarioManager.ObjectImportType.CarryableQuestItem,
		ScenarioManager.ObjectImportType.VFXProp,
		ScenarioManager.ObjectImportType.TerrainVisualEffect,
		ScenarioManager.ObjectImportType.TerrainLampVisualEffect
	};

	public void FadeAlongActorPath(List<CTile> waypoints, Transform actor, CActor actorObj)
	{
		foreach (CObjectProp item in ScenarioManager.CurrentScenarioState.GetPropsThreadSafe)
		{
			if (CheckPropCanBeFaded(item) && CheckPropIsAlongPath(item, waypoints) && (!(actorObj is CObjectActor cObjectActor) || cObjectActor.AttachedProp != item))
			{
				FadePropWithOverlappingActor(item, actor);
			}
		}
	}

	public void StopFade(Transform actor)
	{
		foreach (PropFadeAnimation item in _running)
		{
			item.RemoveOverlappingActor(actor);
		}
	}

	private static bool CheckPropIsAlongPath(CObjectProp prop, List<CTile> tiles)
	{
		if (tiles == null)
		{
			return false;
		}
		if (prop is CObjectObstacle obstacle)
		{
			return CheckObstacleIsAlongPath(obstacle, tiles);
		}
		foreach (CTile tile in tiles)
		{
			if (Equals(prop.ArrayIndex, tile.m_ArrayIndex))
			{
				return true;
			}
		}
		return false;
	}

	private static bool CheckPropCanBeFaded(CObjectProp prop)
	{
		return !EXCLUDED_OBJECT_TYPES.Contains(prop.ObjectType);
	}

	private static bool CheckObstacleIsAlongPath(CObjectObstacle obstacle, List<CTile> tiles)
	{
		foreach (CTile tile in tiles)
		{
			if (Equals(obstacle.ArrayIndex, tile.m_ArrayIndex))
			{
				return true;
			}
			foreach (TileIndex pathingBlocker in obstacle.PathingBlockers)
			{
				if (Equals(pathingBlocker, tile.m_ArrayIndex))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool Equals(TileIndex tileIndex, Point point)
	{
		if (tileIndex.X == point.X)
		{
			return tileIndex.Y == point.Y;
		}
		return false;
	}

	private void FadePropWithOverlappingActor(CObjectProp prop, Transform actor)
	{
		GameObject gameObject = Choreographer.FindPropGameObject(prop);
		if (!(gameObject == null))
		{
			if (!gameObject.TryGetComponent<PropFadeAnimation>(out var component))
			{
				component = gameObject.AddComponent<PropFadeAnimation>();
				component.SetPropData(prop);
				PropFadeAnimation propFadeAnimation = component;
				propFadeAnimation.OnSystemStopped = (Action<PropFadeAnimation>)Delegate.Combine(propFadeAnimation.OnSystemStopped, new Action<PropFadeAnimation>(PropStoppedFading));
			}
			component.AddOverlappingActor(actor);
			_running.Add(component);
		}
	}

	private void PropStoppedFading(PropFadeAnimation propFadeAnimation)
	{
		if (_running.Contains(propFadeAnimation))
		{
			_running.Remove(propFadeAnimation);
		}
	}
}
