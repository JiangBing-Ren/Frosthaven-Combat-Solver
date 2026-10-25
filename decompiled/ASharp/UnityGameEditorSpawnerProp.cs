using System.Linq;
using System.Runtime.Serialization;
using FHGUI.Common;
using ScenarioRuleLibrary;
using UnityEngine;

public class UnityGameEditorSpawnerProp : MonoBehaviour, IHoverable, IGeneralInfoProvider
{
	private ISerializable m_Spawner;

	private void Start()
	{
		gameObject.layer = LayerMask.NameToLayer("Hovering");
	}

	private bool TryGetObj()
	{
		try
		{
			if (gameObject == null)
			{
				return false;
			}
		}
		catch
		{
			return false;
		}
		if (m_Spawner == null)
		{
			m_Spawner = ScenarioManager.CurrentScenarioState.Spawners.OfType<CSpawner>().SingleOrDefault((CSpawner s) => s.Prop?.InstanceName == gameObject.name);
			if (m_Spawner == null)
			{
				m_Spawner = ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.SingleOrDefault((CObjectProp p) => p.InstanceName == gameObject.name);
			}
		}
		return m_Spawner != null;
	}

	public void OnCursorEnter()
	{
		if (GetComponent<UnityGameEditorObject>() == null)
		{
			Log.Error("No UnityGameEditorObject script found on Trap");
		}
		else if (TryGetObj() && SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			Singleton<ActorStatPanel>.Instance.Show(m_Spawner);
		}
	}

	public void OnCursorExit()
	{
		if (TryGetObj() && SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			Singleton<ActorStatPanel>.Instance.HideForSpawner(m_Spawner);
		}
	}

	public IGeneralInfoData GetInfoData()
	{
		if (!TryGetObj())
		{
			return null;
		}
		return UIGeneralInfoUtils.GetSpawnerData((CSpawner)m_Spawner);
	}
}
