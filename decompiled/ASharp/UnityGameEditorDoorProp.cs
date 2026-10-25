using System.Linq;
using FH.LevelComponents.ProcGen;
using FHGUI.Common;
using ScenarioRuleLibrary;
using UnityEngine;

public class UnityGameEditorDoorProp : MonoBehaviour, IHoverable, IGeneralInfoProvider
{
	public CObjectDoor.EDoorType m_DoorType;

	public bool m_IsDungeonEntrance;

	public bool m_IsDungeonExit;

	public bool m_InitiallyVisable;

	public CObjectDoor.ELockType m_LockType;

	private CapsuleCollider _hoverCapsule;

	public CObjectDoor DoorInCurrentState { get; private set; }

	public bool IsDoorOpen { get; private set; }

	private bool IsRuntimeAttachedActorAlive
	{
		get
		{
			if (DoorInCurrentState != null && DoorInCurrentState.RuntimeAttachedActor != null)
			{
				return !DoorInCurrentState.RuntimeAttachedActor.IsDead;
			}
			return false;
		}
	}

	private void Start()
	{
		gameObject.layer = LayerMask.NameToLayer("Hovering");
	}

	private void OnValidate()
	{
	}

	public Vector3 ForwardVector()
	{
		if (!ApparanceLayer.IsThinDoor(m_DoorType))
		{
			return transform.forward;
		}
		return transform.right;
	}

	public Vector3 RightVector()
	{
		if (!ApparanceLayer.IsThinDoor(m_DoorType))
		{
			return transform.right;
		}
		return -transform.forward;
	}

	public CObjectDoor.ELockType LockType()
	{
		return m_LockType;
	}

	private CObjectDoor GetDoorFromScenario()
	{
		return DoorInCurrentState = ScenarioManager.CurrentScenarioState.DoorProps.OfType<CObjectDoor>().SingleOrDefault((CObjectDoor s) => s?.InstanceName == gameObject.name);
	}

	public void CreateHoverCapsule(GameObject door)
	{
		float num = 2f;
		float radius = 0.8f;
		ProceduralTile component = door.GetComponent<ProceduralTile>();
		if (component != null)
		{
			num = component.ContentHeight;
		}
		_hoverCapsule = gameObject.AddComponent<CapsuleCollider>();
		_hoverCapsule.height = num;
		_hoverCapsule.radius = radius;
		_hoverCapsule.center = new Vector3(0f, num * 0.5f, 0f);
		_hoverCapsule.isTrigger = true;
	}

	public void OnCursorEnter()
	{
		if (!IsDoorOpen)
		{
			IsDoorOpen = MF.GameObjectAnimatorControllerIsCurrentState(gameObject, "Open");
			if (_hoverCapsule != null)
			{
				_hoverCapsule.enabled = !IsDoorOpen;
			}
			if (DoorInCurrentState == null)
			{
				CObjectDoor cObjectDoor = (DoorInCurrentState = GetDoorFromScenario());
			}
			if (IsRuntimeAttachedActorAlive && SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
			{
				Singleton<ActorStatPanel>.Instance.Show(DoorInCurrentState.RuntimeAttachedActor);
			}
		}
	}

	public void OnCursorExit()
	{
		if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			Singleton<UIDoorInfoPanel>.Instance?.Hide();
			if (IsRuntimeAttachedActorAlive)
			{
				Singleton<ActorStatPanel>.Instance.HideForActor(DoorInCurrentState.RuntimeAttachedActor);
			}
		}
	}

	public IGeneralInfoData GetInfoData()
	{
		if (DoorInCurrentState == null)
		{
			CObjectDoor cObjectDoor = (DoorInCurrentState = GetDoorFromScenario());
		}
		if (DoorInCurrentState != null)
		{
			return UIGeneralInfoUtils.GetPropData(DoorInCurrentState, gameObject.GetComponent<UnityGameEditorObject>());
		}
		return null;
	}
}
