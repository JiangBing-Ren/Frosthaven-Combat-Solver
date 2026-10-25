using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CInteractableSpawner : CSpawner
{
	public bool Deactivated { get; private set; }

	public string SpawnerPropType { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Deactivated", Deactivated);
		info.AddValue("SpawnerPropType", SpawnerPropType);
	}

	public CInteractableSpawner(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "Deactivated"))
				{
					if (name == "SpawnerPropType")
					{
						SpawnerPropType = info.GetString("SpawnerPropType");
					}
				}
				else
				{
					Deactivated = info.GetBoolean("Deactivated");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CInteractableSpawner entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CInteractableSpawner(string spawnerPropType, SpawnerData spawnerData, TileIndex arrayIndex, string startingMapGuid, string spawnerGuid = null, SpawnerData.ESpawnerEntryDifficulty spawnerEntryDifficulty = SpawnerData.ESpawnerEntryDifficulty.Default)
		: base(spawnerData, arrayIndex, startingMapGuid, spawnerGuid, spawnerEntryDifficulty)
	{
		SpawnerPropType = spawnerPropType;
		Deactivated = false;
	}

	public void CreateSpawnerProp(List<TileIndex> pathingBlockers = null, CVector3 rotation = null)
	{
		if (Prop == null)
		{
			Prop = new CObjectObstacle(SpawnerPropType, ScenarioManager.ObjectImportType.Spawner, ArrayIndex, null, rotation, pathingBlockers, null, StartingMapGuid, ignoresFlyAndJump: false, pathingBlockers != null && pathingBlockers.Count > 1);
			Prop.RuntimeAttachedSpawner = this;
			CSpawn_MessageData message = new CSpawn_MessageData(null)
			{
				m_SpawnDelay = 0f,
				m_Prop = Prop
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public override void HideSpawnerProp(bool forced = false)
	{
	}

	public override void SetActive(bool active, bool forceDontSpawn = false)
	{
		if (!Deactivated)
		{
			base.SetActive(active, forceDontSpawn);
		}
	}

	public void Deactivate()
	{
		if (!Deactivated)
		{
			SetActive(active: false);
			Deactivated = true;
			CDeactivatePropAnim_MessageData cDeactivatePropAnim_MessageData = new CDeactivatePropAnim_MessageData(null);
			cDeactivatePropAnim_MessageData.m_Prop = Prop;
			ScenarioRuleClient.MessageHandler(cDeactivatePropAnim_MessageData);
		}
	}
}
