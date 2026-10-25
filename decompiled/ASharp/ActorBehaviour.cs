using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Chronos;
using SETUtil.Extend;
using SETUtil.Materials;
using ScenarioRuleLibrary;
using UnityEngine;
using WorldspaceUI;

public class ActorBehaviour : MonoBehaviour
{
	private enum MoveState
	{
		Idle,
		RunStart,
		RunLoop,
		RunStop
	}

	public GameObject m_Hilight;

	public GameObject m_Base;

	[HideInInspector]
	public WorldspacePanelUIController m_WorldspacePanelUI;

	public GameObject WorldspacePanelUIPrefab;

	private CActor m_Actor;

	private Vector3 m_LocoIntermediateTarget;

	private Vector3 m_LocoLastWaypointTarget;

	private bool m_NewLocoTarget;

	private float _LocoSegmentsLagTimer;

	private float _LocoTeleportSloMoLagTimer;

	private bool m_Teleport;

	private bool m_IsSpawnTeleport;

	private bool m_PushPull;

	private bool m_LocoTeleport;

	private const float m_LocoTeleportThreshold = 2.2f;

	private float m_StartingDistanceToIntermediateTarget;

	private GameObject m_AnimatedGameObject;

	private FootstepSound m_Footsteps;

	private GameObject[] m_MultiBodies;

	private Animator m_Animator;

	private Vector3 m_VectorToTarget;

	private Vector3 m_StartPosition;

	private Vector3 _startStopOffset;

	private bool m_StartedWithMidpoints;

	private bool m_Jump;

	private Renderer[] m_Renderers;

	private bool m_PauseLoco;

	private CharacterManager m_ActorToFade;

	private bool m_ForcingPositionChangeCounter;

	private float m_InvisibleControl;

	private bool m_WasInvisible;

	private GameObject m_InvisibilityIdleEffect;

	private bool m_IsPushPullInProgress;

	private bool m_InitialisedWorldspaceUI;

	private float c_FixUpdateTimeStep = 0.02f;

	private static float c_LocoTeleportPeriod = 1.5f;

	private static float c_LocoTeleportHalfPeriod = c_LocoTeleportPeriod * 0.5f;

	private float _currentRareIdleInterval;

	private int _lastAnimatedCondition = -1;

	private bool _hasAlertIdle;

	private float _alertBlend;

	private bool _isInteracting;

	private bool _hasInteract;

	private float _interactBlend;

	private bool _skipInteractIntro;

	private AnimationClip _lastClip;

	private AnimationClip _fadeFromClip;

	private float _lastFadeFromTime;

	private float _crossfadeTimer;

	private bool _wasInIdle;

	private Transform[] _characterRig;

	private Vector3[] _characterRigPos;

	private Vector3[] _characterRigScale;

	private Quaternion[] _characterRigRot;

	private Transform _characterRigRoot;

	private readonly float _fadeTransitionTime = 0.15f;

	private float _currentFadeTransitionTime = 0.15f;

	private float _overrideFadeTransitionTime = -1f;

	private float _runSpeed;

	private float _runStartSpeed = -1f;

	private float _runStartLen;

	private float _runStartDuration;

	private float _runStopSpeed = -1f;

	private float _runStopLen;

	private float _runStopDuration;

	private float _jumpSpeed;

	private float m_LocoPeriod;

	private float m_LocoTimer;

	private MoveState _moveState;

	private bool _possibleFloorOffset;

	private float _targetYOffset;

	private int _yOffsetCastMask;

	private int _pendingBodyIndex;

	private PropHealthState[] _propHealthStates;

	private bool conditionIsPlaying;

	private List<CClientInteractableObject> interactObjects = new List<CClientInteractableObject>();

	private GameObject _cachedAttachedPropGameObject;

	private CObjectActor _cachedObjectActor;

	private static readonly CCondition.ENegativeCondition[] AnimatedConditions = new CCondition.ENegativeCondition[5]
	{
		CCondition.ENegativeCondition.Poison,
		CCondition.ENegativeCondition.Wound,
		CCondition.ENegativeCondition.Immobilize,
		CCondition.ENegativeCondition.Disarm,
		CCondition.ENegativeCondition.Stun
	};

	public CActor Actor
	{
		get
		{
			return m_Actor;
		}
		set
		{
			m_Actor = value;
		}
	}

	public bool IsMoving { get; private set; }

	public bool HasMultiBodies => m_MultiBodies != null;

	public GameObject m_RootGameObject { get; private set; }

	public Point _lastInteractingTile { get; private set; }

	private bool _hasRunStartAnim => _runStartSpeed >= 0f;

	private bool _hasRunStopAnim => _runStopSpeed >= 0f;

	public Renderer[] Renderers => m_Renderers;

	public float RunSpeed => _runSpeed;

	public bool IsJumping
	{
		get
		{
			if (m_Jump)
			{
				return _moveState != MoveState.Idle;
			}
			return false;
		}
	}

	public bool IsTeleporting => m_LocoTeleport;

	public bool IsTeleportFinishing => m_LocoTimer < c_LocoTeleportHalfPeriod;

	private void SetupAnimatorAndMeasureRunSpeed()
	{
		m_Animator.enabled = true;
		m_Animator.applyRootMotion = false;
		m_AnimatedGameObject.GetOrAdd<RootMotion>();
		_runStartSpeed = -1f;
		_runStopSpeed = -1f;
		_jumpSpeed = 0f;
		Transform[] componentsInChildren = m_AnimatedGameObject.GetComponentsInChildren<Transform>();
		_characterRigRoot = componentsInChildren.FirstOrDefault((Transform tr) => tr.name == "DEF-char_root");
		if (_characterRigRoot == null)
		{
			_characterRigRoot = componentsInChildren.FirstOrDefault((Transform tr) => tr.name == "C_rootSkel_JNT");
		}
		if (_characterRigRoot == null)
		{
			return;
		}
		AnimationClip[] animationClips = m_Animator.runtimeAnimatorController.animationClips;
		foreach (AnimationClip animationClip in animationClips)
		{
			if (animationClip.name.EndsWith("JumpLoop"))
			{
				animationClip.SampleAnimation(m_AnimatedGameObject, 0f);
				Vector3 position = _characterRigRoot.position;
				animationClip.SampleAnimation(m_AnimatedGameObject, animationClip.length);
				_jumpSpeed = (_characterRigRoot.position - position).magnitude / animationClip.length;
			}
			else if (animationClip.name.Contains("Run") || animationClip.name.Contains("moveForward") || animationClip.name.ToLower().Contains("_move_") || animationClip.name.EndsWith("MoveLoop") || animationClip.name.EndsWith("Move") || animationClip.name.EndsWith("_move"))
			{
				animationClip.SampleAnimation(m_AnimatedGameObject, 0f);
				Vector3 position2 = _characterRigRoot.position;
				animationClip.SampleAnimation(m_AnimatedGameObject, animationClip.length);
				_runSpeed = (_characterRigRoot.position - position2).magnitude / animationClip.length;
			}
			else if (animationClip.name.Contains("MoveStart"))
			{
				animationClip.SampleAnimation(m_AnimatedGameObject, 0f);
				Vector3 position3 = _characterRigRoot.position;
				animationClip.SampleAnimation(m_AnimatedGameObject, animationClip.length);
				_runStartLen = (_characterRigRoot.position - position3).magnitude;
				_runStartDuration = animationClip.length;
				_runStartSpeed = _runStartLen / _runStartDuration;
			}
			else if (animationClip.name.Contains("MoveStop"))
			{
				animationClip.SampleAnimation(m_AnimatedGameObject, 0f);
				Vector3 position4 = _characterRigRoot.position;
				animationClip.SampleAnimation(m_AnimatedGameObject, animationClip.length);
				_runStopLen = (_characterRigRoot.position - position4).magnitude;
				_runStopDuration = animationClip.length;
				_runStopSpeed = _runStopLen / _runStopDuration;
			}
		}
		m_AnimatedGameObject.transform.localPosition = Vector3.zero;
	}

	private void CacheRig()
	{
		List<Transform> list = new List<Transform>();
		SkinnedMeshRenderer[] componentsInChildren = m_AnimatedGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			list.AddRange(skinnedMeshRenderer.rootBone.GetComponentsInChildren<Transform>(includeInactive: true));
		}
		_characterRig = list.ToArray();
		_characterRigPos = new Vector3[_characterRig.Length];
		_characterRigScale = new Vector3[_characterRig.Length];
		_characterRigRot = new Quaternion[_characterRig.Length];
	}

	private void CheckForPropHealth()
	{
		_propHealthStates = null;
		if ((ScenarioManager.Scenario == null || ScenarioManager.Scenario.CurrentScenarioResult == SEventActorFinishedScenario.EScenarioResult.None) && m_Actor is CObjectActor && gameObject.activeInHierarchy)
		{
			StartCoroutine(CheckForPropHealthNextFrameCrt());
		}
	}

	private IEnumerator CheckForPropHealthNextFrameCrt()
	{
		if (!(m_Actor is CObjectActor propActor))
		{
			yield break;
		}
		GameObject propVisual = null;
		int cnt = 10;
		while (cnt > 0)
		{
			cnt--;
			yield return 0;
			propVisual = Choreographer.s_Choreographer.FindClientActorGameObject(propActor, shouldReturnDummyActorsProp: true);
			if ((bool)propVisual)
			{
				_propHealthStates = propVisual.GetComponentsInChildren<PropHealthState>(includeInactive: true);
				if (_propHealthStates != null && _propHealthStates.Length != 0)
				{
					break;
				}
			}
		}
		if ((bool)propVisual && (_propHealthStates == null || _propHealthStates.Length == 0))
		{
			ApparanceEntity ae = propVisual.GetComponentInChildren<ApparanceEntity>();
			float timeout = 60f;
			while (timeout > 0f && ae != null && ae.IsBusy)
			{
				timeout -= Time.unscaledDeltaTime;
				yield return 0;
			}
			_propHealthStates = propVisual.GetComponentsInChildren<PropHealthState>(includeInactive: true);
		}
	}

	private void CacheAttachedPropGameObject()
	{
		if (!(_cachedAttachedPropGameObject != null) && _cachedObjectActor?.AttachedProp != null)
		{
			_cachedAttachedPropGameObject = ((_cachedObjectActor?.AttachedProp != null) ? Choreographer.FindPropGameObject(_cachedObjectActor.AttachedProp) : null);
		}
	}

	public static void SetActor(GameObject gameObject, CActor actor, bool hideWorldSpacePanel = false)
	{
		ActorBehaviour actorBehaviour = GetActorBehaviour(gameObject, clearCache: true);
		if (Choreographer.s_Choreographer?.m_ActorBehaviourCache != null)
		{
			Choreographer.s_Choreographer.m_ActorBehaviourCache[gameObject] = actorBehaviour;
		}
		actorBehaviour.m_Actor = actor;
		actorBehaviour.m_RootGameObject = gameObject;
		actorBehaviour.m_MultiBodies = TryFindMutliBodies(gameObject);
		actorBehaviour.m_Animator = MF.GetGameObjectAnimator(actorBehaviour.m_RootGameObject);
		actorBehaviour.m_AnimatedGameObject = (actorBehaviour.m_Animator ? actorBehaviour.m_Animator.gameObject : null);
		actorBehaviour.m_Footsteps = (actorBehaviour.m_AnimatedGameObject ? actorBehaviour.m_AnimatedGameObject.GetComponent<FootstepSound>() : null);
		actorBehaviour._possibleFloorOffset = false;
		actorBehaviour._hasInteract = false;
		actorBehaviour._lastInteractingTile = default;
		actorBehaviour.SetupAnimatorAndMeasureRunSpeed();
		actorBehaviour.CheckForPropHealth();
		actorBehaviour.InitializeInteractions();
		actorBehaviour._cachedAttachedPropGameObject = null;
		actorBehaviour._cachedObjectActor = actorBehaviour.m_Actor as CObjectActor;
		actorBehaviour.CacheAttachedPropGameObject();
		actorBehaviour._yOffsetCastMask = 1 << LayerMask.NameToLayer("Water");
		actorBehaviour.StopLoco(null);
		actorBehaviour.m_LocoIntermediateTarget = gameObject.transform.position;
		actorBehaviour.TeleportToCurrentLocoTarget(isSpawnTeleport: true);
		actorBehaviour.InitCloth();
		if (!actor.IsDead)
		{
			actorBehaviour.CreateWorldSpaceGUIElements(gameObject, actor);
			actorBehaviour.m_WorldspacePanelUI.UpdateHealth(actor.MaxHealth, actor.Health, actor.OriginalMaxHealth, animate: false);
			actorBehaviour.UpdateWorldspaceConditionsUI();
			if (actor.Health <= 0 && !actor.IsDowned)
			{
				actorBehaviour.m_WorldspacePanelUI.Hide();
			}
			if (hideWorldSpacePanel)
			{
				actorBehaviour.m_WorldspacePanelUI.gameObject.SetActive(value: false);
			}
			if (actor is CPlayerActor cPlayerActor && cPlayerActor.Class.ID == "GeminateID" && cPlayerActor.Form != CPlayerActor.EForm.None)
			{
				int index = ((((cPlayerActor.Form == CPlayerActor.EForm.Choose && cPlayerActor.PendingForm != CPlayerActor.EForm.None) ? cPlayerActor.PendingForm : cPlayerActor.Form) != CPlayerActor.EForm.Melee) ? 1 : 0);
				actorBehaviour.SwitchBody(index);
			}
		}
		actorBehaviour.AdjustThreeHexObstacleBase();
		actorBehaviour.m_Renderers = actorBehaviour.m_Animator.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		actorBehaviour._currentRareIdleInterval = actorBehaviour.GetRandomRareIdleInterval();
		actorBehaviour._hasAlertIdle = MF.AnimatorHasParam(actorBehaviour.m_Animator, "Alert");
		actorBehaviour._hasInteract = MF.AnimatorHasParam(actorBehaviour.m_Animator, "InteractBlend");
		actorBehaviour.StopJumpEffects();
		actorBehaviour.CacheRig();
	}

	private void AdjustThreeHexObstacleBase()
	{
		if (Actor is CObjectActor { AttachedProp: not null } cObjectActor && m_Base.transform.localPosition.magnitude < 0.01f && cObjectActor.AttachedProp.PropType == EPropType.ThreeHexObstacle)
		{
			m_Base.transform.localPosition += new Vector3(0f, 0f, -1f);
		}
	}

	public static bool IsPaused(GameObject gameObject)
	{
		ActorBehaviour actorBehaviour = GetActorBehaviour(gameObject);
		if (!(actorBehaviour == null))
		{
			return actorBehaviour.m_PauseLoco;
		}
		return false;
	}

	public void PauseLoco(bool pause)
	{
		m_PauseLoco = pause;
	}

	public bool IsLocoPaused()
	{
		return m_PauseLoco;
	}

	public static void SetHilighted(GameObject gameObject, bool hilight)
	{
		if (gameObject != null)
		{
			ActorBehaviour actorBehaviour = GetActorBehaviour(gameObject);
			if (actorBehaviour != null)
			{
				if (actorBehaviour.m_Hilight != null)
				{
					actorBehaviour.m_Hilight.SetActive(hilight);
				}
				else
				{
					Log.Warning("Unable to get actor behaviour highlight");
				}
			}
		}
		else
		{
			Log.Warning("Unable to get actor behaviour for highlighting");
		}
	}

	public static bool IsHilighted(GameObject gameObject)
	{
		return GetActorBehaviour(gameObject).m_Hilight.activeSelf;
	}

	public static CActor GetActor(GameObject gameObject)
	{
		return GetActorBehaviour(gameObject)?.m_Actor;
	}

	public static void UpdateHealth(GameObject gameObject, int actorOriginalHealth = 0, bool updateUI = false, bool isActorDead = false)
	{
		GetActorBehaviour(gameObject)?.UpdateHealth(actorOriginalHealth, updateUI, isActorDead);
	}

	public void UpdateHealth(int actorOriginalHealth = 0, bool updateUI = false, bool isActorDead = false)
	{
		UpdatePropState(isActorDead);
		m_WorldspacePanelUI?.UpdateHealth(m_Actor.MaxHealth, m_Actor.Health, m_Actor.OriginalMaxHealth, animate: true, () =>
		{
			if (updateUI)
			{
				int num = Mathf.Max(m_Actor.Health, 0) - Mathf.Max(actorOriginalHealth, 0);
				if (num > 0)
				{
					NotifyUIHealthIncrease(num, m_Actor.Health > m_Actor.OriginalMaxHealth && m_Actor.MaxHealth > m_Actor.OriginalMaxHealth);
				}
				else if (num < 0)
				{
					NotifyUIHealthDecrease(num);
				}
			}
		});
	}

	private void NotifyUIHealthIncrease(int amount, bool isOverheal)
	{
		m_WorldspacePanelUI?.ShowHeal(amount, isOverheal);
	}

	private void NotifyUIHealthDecrease(int amount)
	{
		m_WorldspacePanelUI?.ShowDamage(amount);
	}

	public void UpdateRetaliateDamage(int retaliateDamage, bool forceUpdate = false)
	{
		m_WorldspacePanelUI?.UpdateRetaliateDamage(retaliateDamage, forceUpdate);
	}

	public static ActorBehaviour GetActorBehaviour(GameObject gameObject, bool clearCache = false)
	{
		if (gameObject == null)
		{
			return null;
		}
		if (Choreographer.s_Choreographer?.m_ActorBehaviourCache != null && Choreographer.s_Choreographer.m_ActorBehaviourCache.TryGetValue(gameObject, out var value))
		{
			if (!((value == null) | clearCache))
			{
				return value;
			}
			Choreographer.s_Choreographer.m_ActorBehaviourCache.Remove(gameObject);
		}
		ActorBehaviour componentInChildren = gameObject.GetComponentInChildren<ActorBehaviour>(includeInactive: true);
		if (componentInChildren == null)
		{
			return GetMultibodyActorBehaviour(gameObject);
		}
		return componentInChildren;
	}

	public static ActorBehaviour GetMultibodyActorBehaviour(GameObject gameObject)
	{
		ActorBehaviour actorBehaviour = gameObject.GetComponent<ActorBehaviour>();
		if (actorBehaviour == null && gameObject != null && gameObject.transform != null && gameObject.transform.parent != null)
		{
			actorBehaviour = gameObject.transform.parent.GetComponentInChildren<ActorBehaviour>(includeInactive: true);
		}
		return actorBehaviour;
	}

	public void CreateWorldSpaceGUIElements(GameObject actorGO, CActor actor)
	{
		CharacterManager component = actorGO.GetComponent<CharacterManager>();
		GameObject gameObject = ObjectPool.Spawn(WorldspacePanelUIPrefab, null, actorGO.transform.position, Quaternion.identity);
		gameObject.transform.SetParent(WorldspaceUITools.Instance.WorldspaceGUIPrefabLevel.transform, worldPositionStays: false);
		m_WorldspacePanelUI = gameObject.GetComponent<WorldspacePanelUIController>();
		m_WorldspacePanelUI.Init(actorGO, actor, component.Height);
		m_InitialisedWorldspaceUI = true;
	}

	public void ForceSetLocoIntermediateTarget(Vector3 target)
	{
		CharacterManager.GetCharacterManager(gameObject).OnBeforeForcedMovement();
		m_ForcingPositionChangeCounter = true;
		m_LocoIntermediateTarget = target;
	}

	public bool AtLocoIntermediateTarget()
	{
		Vector3 vector = m_AnimatedGameObject.transform.position - m_LocoIntermediateTarget;
		vector.y = 0f;
		return vector.magnitude <= 0.001f;
	}

	public void SetLocoTarget(Vector3 intermediateTarget, bool jump, List<CTile> waypoints, bool forceMove = false, bool dontTeleport = false, bool slideWithoutAnim = false, float slideSpeed = 1f, float forcedSpeed = 0f)
	{
		float num = _runSpeed;
		float num2 = _jumpSpeed;
		if (forcedSpeed != 0f)
		{
			slideSpeed = forcedSpeed;
			num = forcedSpeed;
			num2 = forcedSpeed;
		}
		m_LocoIntermediateTarget = intermediateTarget;
		if (waypoints != null && waypoints.Count > 0)
		{
			m_LocoLastWaypointTarget = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[waypoints[waypoints.Count - 1].m_ArrayIndex.X, waypoints[waypoints.Count - 1].m_ArrayIndex.Y].m_GameObject.transform.position;
		}
		else
		{
			m_LocoLastWaypointTarget = m_LocoIntermediateTarget;
		}
		m_StartingDistanceToIntermediateTarget = (m_LocoIntermediateTarget - transform.position).magnitude;
		m_StartPosition = m_RootGameObject.transform.position;
		m_VectorToTarget = (intermediateTarget - m_RootGameObject.transform.position).normalized;
		IsMoving = true;
		if (m_StartingDistanceToIntermediateTarget > 2.2f && !dontTeleport)
		{
			m_LocoTeleport = true;
			m_NewLocoTarget = true;
			m_LocoTimer = c_LocoTeleportPeriod;
			return;
		}
		_startStopOffset = Vector3.zero;
		SaveData.Instance.Global.InvokeGameSpeedChanged();
		float num3 = Vector3.Distance(m_StartPosition, m_LocoIntermediateTarget);
		if (slideWithoutAnim)
		{
			m_LocoPeriod = (m_LocoTimer = num3 / slideSpeed);
		}
		else if (jump)
		{
			ref float locoPeriod = ref m_LocoPeriod;
			ref float locoTimer = ref m_LocoTimer;
			float num4;
			if (num2 != 0f)
			{
				num4 = num3 / num2;
			}
			else
			{
				num4 = ((num == 0f) ? 1f : (num3 / num));
			}
			locoPeriod = (locoTimer = num4);
		}
		else
		{
			m_LocoPeriod = (m_LocoTimer = ((num == 0f) ? 1f : (num3 / num)));
			if (_hasRunStartAnim && _moveState == MoveState.Idle)
			{
				m_LocoTimer = _runStartDuration;
				_startStopOffset = m_VectorToTarget * _runStartLen;
			}
		}
		if (slideWithoutAnim)
		{
			_moveState = MoveState.RunLoop;
			m_Animator.SetInteger("RunState", 0);
		}
		else if (jump && !m_Jump)
		{
			m_Jump = jump;
			_moveState = MoveState.RunLoop;
			if (m_Animator.HasState(0, Animator.StringToHash("Jump Into")))
			{
				m_Animator.Play("Jump Into");
				m_Animator.SetBool("ExitJump", value: false);
			}
			else
			{
				m_Animator.Play("Run");
				m_Animator.SetInteger("RunState", (int)_moveState);
				Jump_Fallback jump_Fallback = m_Animator.gameObject.GetComponent<Jump_Fallback>();
				if (jump_Fallback == null)
				{
					jump_Fallback = m_Animator.gameObject.AddComponent<Jump_Fallback>();
				}
				jump_Fallback.enabled = true;
			}
		}
		else if (!jump && !m_NewLocoTarget)
		{
			if (_hasRunStartAnim && _moveState == MoveState.Idle)
			{
				m_Animator.Play("RunStart");
				_moveState = MoveState.RunStart;
			}
			else
			{
				m_Animator.Play("Run");
				_moveState = MoveState.RunLoop;
			}
			m_Animator.SetInteger("RunState", (int)_moveState);
		}
		if ((m_StartingDistanceToIntermediateTarget != 0f) | forceMove)
		{
			m_NewLocoTarget = true;
		}
		Log.SimpleInfo("Loco Target set for actor: " + Choreographer.GetActorIDForCombatLogIfNeeded(m_Actor));
	}

	public void FadeCharacterOnPosition(Point point)
	{
		FadeInPreviousCharacterOnPosition();
		CActor cActor = ScenarioManager.Scenario.FindActorAt(point, m_Actor);
		if (cActor == null)
		{
			if (m_Actor != null && !m_Actor.Tokens.HasKey(CCondition.EPositiveCondition.Invisible))
			{
				GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(m_Actor);
				if (gameObject != null)
				{
					CharacterManager.GetCharacterManager(gameObject).FadeInVisibility();
				}
			}
			return;
		}
		GameObject gameObject2 = Choreographer.s_Choreographer.FindClientActorGameObject(cActor);
		if (!(gameObject2 == null))
		{
			m_ActorToFade = CharacterManager.GetCharacterManager(gameObject2);
			if (m_ActorToFade != null && !m_ActorToFade.CharacterActor.Tokens.HasKey(CCondition.EPositiveCondition.Invisible))
			{
				m_ActorToFade.FadeOutVisibility();
			}
		}
	}

	public void FadeInPreviousCharacterOnPosition()
	{
		if (m_ActorToFade != null && !m_ActorToFade.CharacterActor.Tokens.HasKey(CCondition.EPositiveCondition.Invisible))
		{
			m_ActorToFade.FadeInVisibility();
		}
	}

	public void TeleportToLocation(Vector3 targetLocation)
	{
		m_LocoIntermediateTarget = targetLocation;
		m_Teleport = true;
		m_IsPushPullInProgress = false;
	}

	public void TeleportToCurrentLocoTarget(bool isSpawnTeleport = false)
	{
		_ = m_LocoIntermediateTarget;
		m_Teleport = true;
		m_IsPushPullInProgress = false;
		m_NewLocoTarget = false;
		m_IsSpawnTeleport = true;
	}

	public void PushPullToLocation(Vector3 targetLocation, bool dontCheckAnimator = false)
	{
		m_IsPushPullInProgress = true;
		m_LocoLastWaypointTarget = (m_LocoIntermediateTarget = targetLocation);
		if (m_Animator.HasState(0, Animator.StringToHash("PushPull")) | dontCheckAnimator)
		{
			MF.AnimatorPlay(m_Animator, "PushPull");
			m_PushPull = true;
		}
		else
		{
			TeleportToLocation(targetLocation);
		}
	}

	public void StopJumpEffects()
	{
		Jump_Fallback component = m_Animator.GetComponent<Jump_Fallback>();
		if ((bool)component)
		{
			component.ForceStop();
			return;
		}
		Renderer[] renderers = m_Renderers;
		foreach (Renderer renderer in renderers)
		{
			if (!(renderer != null))
			{
				continue;
			}
			Material[] materialClonesSlow = renderer.GetMaterialClonesSlow();
			foreach (Material material in materialClonesSlow)
			{
				if (!(material == null))
				{
					material.SetFloat(Jump_SMB.JumpEffectProperty, 0f);
				}
			}
		}
	}

	public void StopLoco(GameObject nextWaypoint)
	{
		m_NewLocoTarget = false;
		m_LocoTeleport = false;
		m_IsPushPullInProgress = false;
		m_PushPull = false;
		m_StartedWithMidpoints = false;
		IsMoving = false;
		SaveData.Instance.Global.InvokeGameSpeedChanged();
		if (m_Animator == null || m_Renderers == null || m_Renderers.Length == 0)
		{
			return;
		}
		if (m_Jump)
		{
			StopJumpEffects();
		}
		Renderer[] componentsInChildren;
		if (!nextWaypoint)
		{
			if (m_Jump)
			{
				m_Animator.SetBool("ExitJump", value: true);
				m_Jump = false;
				componentsInChildren = m_Animator.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren)
				{
					if (!(renderer != null))
					{
						continue;
					}
					Material[] materialClonesSlow = renderer.GetMaterialClonesSlow();
					foreach (Material material in materialClonesSlow)
					{
						if (!(material == null))
						{
							material.SetFloat("_Opacity", 1f);
						}
					}
				}
				if (!m_Animator.HasState(0, Animator.StringToHash("JumpLoop")))
				{
					_moveState = MoveState.Idle;
					m_Animator.SetInteger("RunState", 0);
					Jump_Fallback component = m_Animator.gameObject.GetComponent<Jump_Fallback>();
					if (component != null)
					{
						component.enabled = false;
					}
				}
			}
			else
			{
				_moveState = MoveState.Idle;
				m_Animator.SetInteger("RunState", 0);
				if ((bool)m_Footsteps)
				{
					m_Footsteps.StopClothSound();
				}
			}
		}
		componentsInChildren = m_Renderers;
		foreach (Renderer renderer2 in componentsInChildren)
		{
			if (!(renderer2 != null))
			{
				continue;
			}
			Material[] materialClonesSlow = renderer2.GetMaterialClonesSlow();
			foreach (Material material2 in materialClonesSlow)
			{
				if (!(material2 == null))
				{
					material2.SetFloat("_DeathDissolve", 0f);
				}
			}
		}
		Log.SimpleInfo("Loco stopped for actor: " + Choreographer.GetActorIDForCombatLogIfNeeded(m_Actor));
	}

	public bool AtLocoTarget()
	{
		if (!m_IsPushPullInProgress)
		{
			return !m_NewLocoTarget;
		}
		return false;
	}

	private void CheckInvisibility()
	{
		bool flag = Actor.Tokens.HasKey(CCondition.EPositiveCondition.Invisible);
		if (flag == m_WasInvisible)
		{
			return;
		}
		m_InvisibleControl += (flag ? c_FixUpdateTimeStep : (0f - c_FixUpdateTimeStep));
		m_InvisibleControl = Mathf.Min(Mathf.Max(m_InvisibleControl, 0f), 1f);
		Renderer[] renderers = m_Renderers;
		foreach (Renderer renderer in renderers)
		{
			if (!(renderer != null))
			{
				continue;
			}
			Material[] materialClonesSlow = renderer.GetMaterialClonesSlow();
			foreach (Material material in materialClonesSlow)
			{
				if (!(material == null) || !(renderer is ParticleSystemRenderer))
				{
					if (material.HasProperty("_Toggle_Dissolve"))
					{
						material.SetFloat("_Toggle_Dissolve", (m_InvisibleControl > 0f) ? 1 : 0);
					}
					if (material.HasProperty("_InvisibilityControl"))
					{
						material.SetFloat("_InvisibilityControl", m_InvisibleControl);
					}
				}
			}
		}
		if ((flag && m_InvisibleControl == 1f) || (!flag && m_InvisibleControl == 0f))
		{
			m_WasInvisible = flag;
		}
		if (flag)
		{
			if (m_InvisibilityIdleEffect == null)
			{
				m_InvisibilityIdleEffect = ObjectPool.Spawn(GlobalSettings.Instance.m_JumpMoveParticleEffects.InvisibilityIdle, m_Animator.transform);
				m_InvisibilityIdleEffect.GetComponent<ParticleSystem>().Play();
				ObjectPool.Spawn(GlobalSettings.Instance.m_JumpMoveParticleEffects.InvisibilitySmoke, m_Animator.transform).GetComponent<ParticleSystem>().Play();
			}
		}
		else if (m_InvisibilityIdleEffect != null)
		{
			m_InvisibilityIdleEffect.GetComponent<ParticleSystem>().Stop();
			ObjectPool.Recycle(m_InvisibilityIdleEffect, GlobalSettings.Instance.m_JumpMoveParticleEffects.InvisibilityIdle, "@ VFX Pool");
			m_InvisibilityIdleEffect = null;
		}
	}

	private void UpdatePropState(bool isActorDead = false)
	{
		if (_propHealthStates == null || _propHealthStates.Length == 0)
		{
			return;
		}
		PropHealthState[] propHealthStates = _propHealthStates;
		foreach (PropHealthState propHealthState in propHealthStates)
		{
			if (propHealthState == null || propHealthState.gameObject == null)
			{
				CheckForPropHealth();
				break;
			}
			propHealthState.UpdateState(m_Actor.Health, m_Actor.MaxHealth, isActorDead);
		}
	}

	private void SetTransformPosition(Vector3 position)
	{
		if (ScenarioScriptFrontend.ManualActorHeightControl)
		{
			position.y = m_RootGameObject.transform.position.y;
		}
		m_RootGameObject.transform.position = position;
		if (m_Actor is CObjectActor { AttachedProp: not null })
		{
			CacheAttachedPropGameObject();
			if (_cachedAttachedPropGameObject != null)
			{
				_cachedAttachedPropGameObject.transform.position = position;
			}
		}
		if (m_ForcingPositionChangeCounter)
		{
			CharacterManager.GetCharacterManager(gameObject).OnAfterForcedMovement();
			m_ForcingPositionChangeCounter = false;
		}
	}

	public void SetTransformRotation(Quaternion rotation)
	{
		m_RootGameObject.transform.rotation = rotation;
		if (_cachedAttachedPropGameObject != null && !(_cachedAttachedPropGameObject.transform.rotation == rotation))
		{
			_cachedAttachedPropGameObject.transform.rotation = rotation;
		}
	}

	public bool ShouldLookAtActors()
	{
		if (Actor is CObjectActor)
		{
			return false;
		}
		return true;
	}

	private void TrylookAtActors()
	{
		if (!ShouldLookAtActors())
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		int num = 0;
		if (Actor is CPlayerActor)
		{
			CMap hexMap = ScenarioManager.Tiles[Actor.ArrayIndex.X, Actor.ArrayIndex.Y].m_HexMap;
			foreach (CEnemyActor allEnemy in ScenarioManager.Scenario.AllEnemies)
			{
				if (!allEnemy.IsDead && ScenarioManager.Tiles[allEnemy.ArrayIndex.X, allEnemy.ArrayIndex.Y].m_HexMap == hexMap)
				{
					GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(allEnemy);
					if (gameObject != null)
					{
						zero += gameObject.transform.position;
						num++;
					}
				}
			}
		}
		else
		{
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				if (!playerActor.PhasedOut && !playerActor.IsDead)
				{
					GameObject gameObject2 = Choreographer.s_Choreographer.FindClientActorGameObject(playerActor);
					if (gameObject2 != null)
					{
						zero += gameObject2.transform.position;
						num++;
					}
				}
			}
		}
		if (num == 0)
		{
			foreach (CMapTile mapTile in ScenarioManager.Tiles[Actor.ArrayIndex.X, Actor.ArrayIndex.Y].m_HexMap.MapTiles)
			{
				zero += GloomUtility.CVToV(mapTile.Position);
				num++;
			}
		}
		if (num > 0)
		{
			zero /= (float)num;
			m_RootGameObject.transform.LookAt(zero);
		}
	}

	private void OnTeleportRotate()
	{
		if (m_IsSpawnTeleport)
		{
			CEnemyActor cEnemyActor = Actor as CEnemyActor;
			CHeroSummonActor cHeroSummonActor = Actor as CHeroSummonActor;
			if ((cEnemyActor != null && cEnemyActor.IsSummon) || cHeroSummonActor != null)
			{
				CActor cActor = (CActor)(((object)cEnemyActor?.Summoner) ?? ((object)cHeroSummonActor?.Summoner));
				if (cActor == null || cActor.IsDead)
				{
					TrylookAtActors();
					return;
				}
				GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(cActor);
				Vector3 position = m_RootGameObject.transform.position;
				Vector3 position2 = gameObject.transform.position;
				Vector3 worldPosition = position + (position - position2);
				m_RootGameObject.transform.LookAt(worldPosition);
			}
			else
			{
				TrylookAtActors();
			}
		}
		else
		{
			TrylookAtActors();
		}
	}

	private void UpdateLoco()
	{
		if (m_Actor == null || m_Animator == null || SaveData.Instance.Global.CurrentGameState != EGameState.Scenario)
		{
			return;
		}
		float deltaTime = Timekeeper.instance.m_GlobalClock.deltaTime;
		bool flag = false;
		bool flag2 = false;
		if (m_Teleport)
		{
			CharacterManager.GetCharacterManager(gameObject).OnBeforeForcedMovement();
			SetTransformPosition(m_LocoIntermediateTarget);
			CharacterManager.GetCharacterManager(gameObject).OnAfterForcedMovement();
			if (Actor is CObjectActor { AttachedProp: not null } cObjectActor && cObjectActor.AttachedProp.Rotation != null)
			{
				SetTransformRotation(Quaternion.Euler(GloomUtility.CVToV(cObjectActor.AttachedProp.Rotation)));
			}
			OnTeleportRotate();
			m_Teleport = false;
		}
		else if (m_NewLocoTarget)
		{
			bool flag3 = true;
			Vector3 vector = m_LocoIntermediateTarget - m_LocoLastWaypointTarget;
			flag = new Vector3(vector.x, 0f, vector.z).magnitude > 1f;
			Vector3 vector2 = m_LocoIntermediateTarget - m_RootGameObject.transform.position;
			float num = Vector3.Dot(new Vector3(vector2.x, 0f, vector2.z), new Vector3(m_VectorToTarget.x, 0f, m_VectorToTarget.z));
			float magnitude = (m_RootGameObject.transform.position - m_StartPosition).magnitude;
			bool flag4 = num <= 0f && magnitude > 0.25f;
			if (flag)
			{
				if (flag4)
				{
					m_NewLocoTarget = false;
					m_LocoTeleport = false;
					Log.SimpleInfo("Actor reached loco target: " + Choreographer.GetActorIDForCombatLogIfNeeded(m_Actor));
					flag3 = false;
				}
				m_StartedWithMidpoints = true;
			}
			else if (flag4)
			{
				StopLoco(Waypoint.GetNextWaypoint());
				flag3 = false;
			}
			bool flag5 = ShouldLookAtActors();
			if (flag3 || !flag5)
			{
				Vector3 vector3 = (m_LocoIntermediateTarget - m_RootGameObject.transform.position).normalized;
				Quaternion quaternion = Quaternion.LookRotation(vector3, m_RootGameObject.transform.up);
				if (!flag5)
				{
					CObjectActor cObjectActor2 = Actor as CObjectActor;
					quaternion = Quaternion.Euler(0f, cObjectActor2.AttachedProp.Rotation.Y, 0f);
					vector3 = quaternion * Vector3.forward;
				}
				if (vector3 != Vector3.zero)
				{
					float num2 = Vector3.Dot(vector3, m_RootGameObject.transform.forward);
					flag2 = num2 < 0.9f;
					float num3 = (num2 + 1.3f) / 1.7f * 10f;
					float t = (flag2 ? (num3 * deltaTime) : 1f);
					if (Quaternion.Angle(m_RootGameObject.transform.rotation, quaternion) > 0.1f)
					{
						if (!flag5)
						{
							m_NewLocoTarget = true;
						}
						SetTransformRotation(Quaternion.Lerp(m_RootGameObject.transform.rotation, quaternion, t));
					}
				}
			}
		}
		else if (m_PushPull)
		{
			Vector3 vector4 = m_RootGameObject.transform.position - m_LocoLastWaypointTarget;
			vector4.y = 0f;
			if (vector4.magnitude > 0.1f)
			{
				SetTransformPosition(Vector3.Lerp(m_RootGameObject.transform.position, m_LocoLastWaypointTarget, deltaTime * 10f));
			}
			else
			{
				m_IsPushPullInProgress = false;
				m_PushPull = false;
			}
		}
		else if (!m_StartedWithMidpoints)
		{
			SetTransformPosition(m_LocoIntermediateTarget);
		}
		bool flag6 = m_Jump && !m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Jump") && _jumpSpeed != 0f;
		if (m_NewLocoTarget & flag)
		{
			_LocoSegmentsLagTimer = 0.1f;
		}
		else
		{
			_LocoSegmentsLagTimer -= Time.deltaTime;
		}
		if (m_LocoTeleport)
		{
			_LocoTeleportSloMoLagTimer = 0.1f;
		}
		else
		{
			_LocoTeleportSloMoLagTimer -= Time.deltaTime;
		}
		if ((_LocoSegmentsLagTimer > 0f || m_NewLocoTarget) && !m_PauseLoco && !flag6)
		{
			if (m_LocoTeleport)
			{
				bool flag7 = m_LocoTimer > c_LocoTeleportHalfPeriod;
				float value = ((!flag7) ? (m_LocoTimer / c_LocoTeleportHalfPeriod) : (1f - (m_LocoTimer - c_LocoTeleportHalfPeriod) / c_LocoTeleportHalfPeriod));
				Renderer[] renderers = m_Renderers;
				foreach (Renderer renderer in renderers)
				{
					if (!(renderer != null))
					{
						continue;
					}
					Material[] materialClonesSlow = renderer.GetMaterialClonesSlow();
					foreach (Material material in materialClonesSlow)
					{
						if (!(material == null))
						{
							material.SetFloat("_DeathDissolve", value);
						}
					}
				}
				m_LocoTimer -= deltaTime;
				if (flag7)
				{
					m_LocoTimer = Math.Max(m_LocoTimer, c_LocoTeleportHalfPeriod - 0.001f);
				}
				if (flag7 && m_LocoTimer <= c_LocoTeleportHalfPeriod)
				{
					SetTransformPosition(m_LocoIntermediateTarget + (m_StartPosition - m_LocoIntermediateTarget).normalized * 0.01f);
				}
				if (m_LocoTimer < 0f)
				{
					m_LocoTeleport = false;
					m_LocoTimer = 0f;
					_LocoSegmentsLagTimer = 0f;
					SetTransformPosition(m_LocoIntermediateTarget);
					StopLoco(Waypoint.GetNextWaypoint());
				}
			}
			else
			{
				m_LocoTimer -= deltaTime;
				if (_moveState > MoveState.RunStart || _runStartSpeed > 0.001f)
				{
					Vector3 transformPosition = ((_cachedObjectActor != null || m_Jump) ? Vector3.Lerp(m_StartPosition, m_LocoIntermediateTarget + _startStopOffset, Math.Max(0f, 1f - m_LocoTimer / m_LocoPeriod)) : Vector3.LerpUnclamped(m_StartPosition, m_LocoIntermediateTarget + _startStopOffset, Math.Min(Math.Max(1f, _runSpeed / 2.2f), Math.Max(0f, 1f - m_LocoTimer / m_LocoPeriod))));
					SetTransformPosition(transformPosition);
				}
				if (m_LocoTimer < 0f && _moveState == MoveState.RunStart)
				{
					_moveState = MoveState.RunLoop;
					m_Animator.SetInteger("RunState", 2);
					m_LocoTimer = m_LocoPeriod;
				}
				if (_moveState >= MoveState.RunStart && !flag && _hasRunStopAnim && _runStopLen > 0.1f)
				{
					float magnitude2 = (m_LocoIntermediateTarget - m_RootGameObject.transform.position).magnitude;
					if (magnitude2 <= _runStopLen)
					{
						_ = _runStopLen;
						m_LocoPeriod = _runStopDuration;
						m_StartPosition = m_RootGameObject.transform.position;
						_moveState = MoveState.RunStop;
						m_Animator.SetInteger("RunState", 3);
					}
				}
			}
		}
		Animator animator = m_Animator;
		float speed;
		if (m_PauseLoco)
		{
			speed = 0f;
		}
		else
		{
			speed = ((m_LocoTeleport || _LocoTeleportSloMoLagTimer > 0f) ? 0.05f : Timekeeper.instance.m_GlobalClock.timeScale);
		}
		animator.speed = speed;
		if (m_Actor.IsDead)
		{
			m_Animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		}
		Vector3 euler = new Vector3(90f, 0f, -120f);
		m_Hilight.transform.rotation = Quaternion.Euler(euler);
		Quaternion transformRotation = Quaternion.FromToRotation(toDirection: new Vector3(0f, 1f, 0f), fromDirection: m_RootGameObject.transform.up) * m_RootGameObject.transform.rotation;
		SetTransformRotation(transformRotation);
	}

	private void LateUpdate()
	{
		if (m_Actor != null && !(m_Animator == null) && ScenarioManager.Scenario != null)
		{
			UpdateAnimCrossfade();
			UpdateActorMultiHexPosition();
		}
	}

	public void SwitchBody(int index, bool instant = false)
	{
		if (m_MultiBodies == null || m_MultiBodies.FirstOrDefault((GameObject go) => go.activeSelf) == m_MultiBodies[index])
		{
			return;
		}
		if (ScenarioManager.IsFastReloading || ScenarioManager.IsLoading)
		{
			instant = true;
		}
		_pendingBodyIndex = index;
		if (!instant && m_Animator.HasState(0, Animator.StringToHash("Switch")))
		{
			if (Actor is CPlayerActor cPlayerActor)
			{
				cPlayerActor.SwitchingForm = true;
			}
			MF.AnimatorPlay(m_Animator, "Switch");
			Choreographer.s_Choreographer.SetBlockClientMessageProcessing(active: true);
		}
		else
		{
			ApplySwitchBody(instant);
		}
	}

	public void ApplySwitchBody(bool instant = false)
	{
		m_MultiBodies.FirstOrDefault((GameObject go) => go.activeSelf).SetActive(value: false);
		m_MultiBodies[_pendingBodyIndex].SetActive(value: true);
		m_Animator = MF.GetGameObjectAnimator(m_RootGameObject);
		m_AnimatedGameObject = m_Animator.gameObject;
		m_Footsteps = m_MultiBodies[_pendingBodyIndex].GetComponent<FootstepSound>();
		SetupAnimatorAndMeasureRunSpeed();
		CacheRig();
		CPlayerActor cPlayerActor = Actor as CPlayerActor;
		if (!instant && m_Animator.HasState(0, Animator.StringToHash("SwitchEnd")))
		{
			if (cPlayerActor != null)
			{
				cPlayerActor.SwitchingForm = true;
			}
			Choreographer.s_Choreographer.SetBlockClientMessageProcessing(active: true);
			MF.AnimatorPlay(m_Animator, "SwitchEnd");
		}
		else
		{
			Choreographer.s_Choreographer.SetBlockClientMessageProcessing(active: false);
			m_Animator.Play("Idle-Run");
		}
		if (cPlayerActor != null)
		{
			cPlayerActor.SwitchingForm = false;
		}
	}

	public static GameObject[] TryFindMutliBodies(GameObject root)
	{
		IEnumerable<Transform> source = from t in root.GetComponentsInChildren<Transform>(includeInactive: true)
			where t.name.StartsWith("BodyVariant")
			select t;
		if (source.Any())
		{
			return source.Select((Transform t) => t.gameObject).ToArray();
		}
		return null;
	}

	private void Update()
	{
		if (ScenarioManager.Scenario != null && m_Actor != null)
		{
			UpdatePropState();
			if (!(m_Animator == null))
			{
				CheckInvisibility();
				UpdateIdleAnimations();
				UpdateLoco();
			}
		}
	}

	public void SkipInteractIntro(bool skip)
	{
		_skipInteractIntro = skip;
		if (skip)
		{
			_interactBlend = 1f;
		}
	}

	private void StopInteracting(bool forceBlend = false)
	{
		if (!_hasInteract)
		{
			return;
		}
		_isInteracting = false;
		_lastInteractingTile = default;
		foreach (CClientInteractableObject interactObject in interactObjects)
		{
			interactObject.SetInteracted(interacting: false);
		}
		interactObjects.Clear();
		if (forceBlend)
		{
			_interactBlend = 0f;
			m_Animator.SetFloat("InteractBlend", 0f);
		}
		m_RootGameObject.transform.hasChanged = true;
	}

	private void UpdateIdleInteractAnimations(out bool breakIdle)
	{
		breakIdle = false;
		if (m_Teleport)
		{
			return;
		}
		bool isInteracting = _isInteracting;
		_isInteracting = m_Actor.Interacting;
		_interactBlend = Mathf.Lerp(_interactBlend, _isInteracting ? 1f : 0f, Time.deltaTime * 7f);
		m_Animator.SetFloat("InteractBlend", _interactBlend);
		if (_isInteracting)
		{
			if (!isInteracting)
			{
				if (!_skipInteractIntro && m_Animator.HasState(0, Animator.StringToHash("InteractIntro")))
				{
					_interactBlend = 1f;
					m_Animator.SetFloat("InteractBlend", _interactBlend);
					breakIdle = true;
					MF.AnimatorPlay(m_Animator, "InteractIntro");
				}
				else
				{
					Point interactingTile = m_Actor.InteractingTile;
					CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[interactingTile.X, interactingTile.Y];
					interactObjects.Clear();
					foreach (CObjectProp prop in cClientTile.m_Tile.m_Props)
					{
						CClientInteractableObject cClientInteractableObject = Choreographer.FindPropGameObject(prop)?.GetComponent<CClientInteractableObject>();
						if (cClientInteractableObject != null)
						{
							cClientInteractableObject.SetInteracted(interacting: true);
							interactObjects.Add(cClientInteractableObject);
						}
					}
				}
			}
			if (_interactBlend >= 1f)
			{
				_skipInteractIntro = false;
			}
			if (m_RootGameObject.transform.hasChanged || _lastInteractingTile != Actor.InteractingTile)
			{
				RotateTowardsInteractTile();
			}
		}
		else if (isInteracting)
		{
			if (m_Animator.HasState(0, Animator.StringToHash("InteractOutro")))
			{
				_interactBlend = 0f;
				m_Animator.SetFloat("InteractBlend", _interactBlend);
				breakIdle = true;
				MF.AnimatorPlay(m_Animator, "InteractOutro");
			}
			StopInteracting();
		}
	}

	public void InitializeInteractions()
	{
		if (!(m_Animator == null) && _hasInteract)
		{
			m_Animator.SetFloat("InteractBlend", _interactBlend);
			RotateTowardsInteractTile();
		}
	}

	public void InitCloth()
	{
		Cloth[] componentsInChildren = m_Animator.gameObject.GetComponentsInChildren<Cloth>();
		Cloth[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		StartCoroutine(DelayedInitCloth(componentsInChildren));
		static IEnumerator DelayedInitCloth(Cloth[] cloths)
		{
			yield return null;
			foreach (Cloth obj in cloths)
			{
				obj.enabled = true;
				obj.ClearTransformMotion();
			}
		}
	}

	public void RotateTowardsInteractTile()
	{
		_lastInteractingTile = m_Actor.InteractingTile;
		if (_lastInteractingTile != default(Point))
		{
			CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[_lastInteractingTile.X, _lastInteractingTile.Y];
			m_RootGameObject.transform.LookAt(cClientTile.m_GameObject.transform.position);
			Cloth[] componentsInChildren = m_Animator.gameObject.GetComponentsInChildren<Cloth>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].ClearTransformMotion();
			}
		}
		m_RootGameObject.transform.hasChanged = false;
	}

	private void UpdateActorMultiHexPosition()
	{
		if (Actor?.MultiHexArrayIndexes == null || Actor.MultiHexArrayIndexes.Count == 0)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		foreach (Point multiHexArrayIndex in Actor.MultiHexArrayIndexes)
		{
			CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[multiHexArrayIndex.X, multiHexArrayIndex.Y];
			zero += cClientTile.m_GameObject.transform.position;
		}
		zero /= (float)Actor.MultiHexArrayIndexes.Count;
		zero.y = m_RootGameObject.transform.position.y;
		m_RootGameObject.transform.position = zero;
	}

	private void UpdateIdleAnimations()
	{
		if (_moveState > MoveState.Idle)
		{
			StopInteracting(forceBlend: true);
			return;
		}
		AnimatorStateInfo currentAnimatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
		if (_hasAlertIdle)
		{
			_alertBlend = Mathf.Lerp(_alertBlend, m_Actor.Alerted ? 1f : 0f, Time.deltaTime * 7f);
			m_Animator.SetFloat("Alert", _alertBlend);
		}
		if (!currentAnimatorStateInfo.IsName("Idle-Run") || m_Animator.GetNextAnimatorStateInfo(0).length != 0f)
		{
			return;
		}
		if (_hasInteract)
		{
			UpdateIdleInteractAnimations(out var breakIdle);
			if (breakIdle)
			{
				return;
			}
		}
		if (!(currentAnimatorStateInfo.normalizedTime >= _currentRareIdleInterval))
		{
			return;
		}
		string text;
		if (m_Actor.Alerted)
		{
			text = "AlertRareIdle";
		}
		else
		{
			text = (m_Actor.Interacting ? "InteractRareIdle" : "RareIdle");
		}
		_currentRareIdleInterval = GetRandomRareIdleInterval();
		_lastAnimatedCondition++;
		_lastAnimatedCondition %= AnimatedConditions.Length;
		string text2 = "";
		for (int i = 0; i < AnimatedConditions.Length; i++)
		{
			string text3 = text + "-" + AnimatedConditions[_lastAnimatedCondition];
			if (m_Animator.HasState(0, Animator.StringToHash(text3)) && Actor.Tokens.HasKey(AnimatedConditions[_lastAnimatedCondition], checkImmunities: false))
			{
				text2 = text3;
				break;
			}
			_lastAnimatedCondition++;
			_lastAnimatedCondition %= AnimatedConditions.Length;
		}
		if (text2 != "")
		{
			m_Animator.Play(text2);
			return;
		}
		int num = 0;
		for (int j = 0; j < 5; j++)
		{
			if (m_Animator.HasState(0, Animator.StringToHash((j == 0) ? text : (text + "-" + j))))
			{
				num++;
			}
		}
		if (num > 0)
		{
			int num2 = UnityEngine.Random.Range(0, num);
			m_Animator.Play((num2 == 0) ? text : (text + "-" + num2));
		}
	}

	private void UpdateAnimCrossfade()
	{
		bool flag = m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle-Run");
		if (flag)
		{
			_lastFadeFromTime = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		}
		AnimatorClipInfo[] currentAnimatorClipInfo = m_Animator.GetCurrentAnimatorClipInfo(0);
		if (currentAnimatorClipInfo.Length != 0)
		{
			AnimationClip clip = currentAnimatorClipInfo[0].clip;
			float weight = currentAnimatorClipInfo[0].weight;
			for (int i = 1; i < currentAnimatorClipInfo.Length; i++)
			{
				if (currentAnimatorClipInfo[i].weight > weight)
				{
					weight = currentAnimatorClipInfo[i].weight;
					clip = currentAnimatorClipInfo[i].clip;
				}
			}
			if (_wasInIdle && _lastClip != clip && _lastClip != null && !flag && !m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Summoned"))
			{
				_currentFadeTransitionTime = _fadeTransitionTime;
				if (_overrideFadeTransitionTime >= 0f)
				{
					_currentFadeTransitionTime = _overrideFadeTransitionTime;
					_overrideFadeTransitionTime = -1f;
				}
				_crossfadeTimer = _currentFadeTransitionTime;
				_fadeFromClip = _lastClip;
			}
			_lastClip = clip;
		}
		_wasInIdle = flag;
		if (!(_crossfadeTimer > 0f))
		{
			return;
		}
		for (int j = 0; j < _characterRig.Length; j++)
		{
			if (!(_characterRig[j] == null))
			{
				_characterRigPos[j] = _characterRig[j].localPosition;
				_characterRigScale[j] = _characterRig[j].localScale;
				_characterRigRot[j] = _characterRig[j].localRotation;
			}
		}
		float time = Mathf.Repeat(_lastFadeFromTime * _fadeFromClip.length + (_currentFadeTransitionTime - _crossfadeTimer), _fadeFromClip.length);
		_fadeFromClip.SampleAnimation(m_AnimatedGameObject, time);
		float num = 1f - _crossfadeTimer / _currentFadeTransitionTime;
		num *= num;
		for (int k = 0; k < _characterRig.Length; k++)
		{
			if (!(_characterRig[k] == null))
			{
				if (_characterRig[k] == _characterRigRoot)
				{
					_characterRigRoot.localPosition = _characterRigPos[k];
				}
				_characterRig[k].localPosition = Vector3.Lerp(_characterRig[k].localPosition, _characterRigPos[k], num);
				_characterRig[k].localRotation = Quaternion.Lerp(_characterRig[k].localRotation, _characterRigRot[k], num);
				_characterRig[k].localScale = Vector3.Lerp(_characterRig[k].localScale, _characterRigScale[k], num);
			}
		}
		_crossfadeTimer -= Timekeeper.instance.m_GlobalClock.deltaTime;
	}

	public void OverrideCrossfadeTime(float fromCalmIdle, float fromAlertIdle)
	{
		_overrideFadeTransitionTime = ((m_Actor != null && m_Actor.Alerted) ? fromAlertIdle : fromCalmIdle);
	}

	private float GetRandomRareIdleInterval()
	{
		IdleAnimParams component = transform.parent.GetComponent<IdleAnimParams>();
		if ((bool)component)
		{
			return (float)UnityEngine.Random.Range(component.MinIdleCyclesToRare, component.MaxIdleCyclesToRare - 1) + 0.98f;
		}
		return (float)UnityEngine.Random.Range(1, 4) + 0.98f;
	}

	public void UpdateWorldspaceConditionsUI()
	{
		if (m_InitialisedWorldspaceUI && !m_Actor.IsDead && !(m_WorldspacePanelUI == null))
		{
			CAbilityControlActor.EControlDurationType mindControlDuration = m_Actor.MindControlDuration;
			bool controlled = (mindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneAction || mindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneTurn) && m_Actor.MindControlIsFromEnemy;
			bool retaliate = m_Actor.CachedHasRetaliate || m_Actor.Class is CMonsterClass { Retaliate: >0 } || (m_Actor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.SummonData.Retaliate > 0);
			CPlayerActor cPlayerActor = m_Actor as CPlayerActor;
			bool flag = cPlayerActor != null;
			EffectsBar.DTO dto = new EffectsBar.DTO
			{
				Blessed = m_Actor.Tokens.HasKey(CCondition.EPositiveCondition.Bless),
				Invisible = m_Actor.Tokens.HasKey(CCondition.EPositiveCondition.Invisible),
				Strengthened = m_Actor.Tokens.HasKey(CCondition.EPositiveCondition.Strengthen),
				Immovable = m_Actor.Tokens.HasKey(CCondition.EPositiveCondition.Immovable),
				Regenerate = m_Actor.Tokens.HasKey(CCondition.EPositiveCondition.Regenerate),
				Ward = m_Actor.Tokens.HasKey(CCondition.EPositiveCondition.Ward),
				Mutate = m_Actor.Tokens.HasKey(CCondition.EPositiveCondition.Mutate),
				Cursed = m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Curse),
				Disarmed = m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Disarm),
				Immobilized = m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Immobilize),
				Muddled = m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Muddle),
				Poisoned = m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Poison),
				Stunned = (m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Stun) && !m_Actor.IsDowned),
				Wounded = m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Wound),
				Sleep = m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep),
				Brittle = m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Brittle),
				Impaired = m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Impair),
				Bane = m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Bane),
				Pressure = (flag ? cPlayerActor.Pressure : CPlayerActor.EPressure.None),
				ImprovedShortRest = (flag && cPlayerActor.CharacterClass.ImprovedShortRest),
				Pierce = false,
				Retaliate = retaliate,
				RetaliateRangeDamagePairs = m_Actor.CachedRetaliateRangeDamagePairs,
				BlockHealing = m_Actor.CachedHealingBlocked,
				NeutralizeShield = m_Actor.CachedShieldNeutralized,
				Controlled = controlled,
				Fly = m_Actor.Flying,
				ShadowMarked = m_Actor.IsShadowMarked,
				Doomed = m_Actor.CachedDoomActiveBonuses,
				CarryingQuestItem = m_Actor.CarriedQuestItems.Any(),
				ActiveItemEffectsOnMonsters = m_Actor.CachedActiveItemEffectBonuses,
				ActiveScenarioModifiersBonuses = m_Actor.CachedScenarioModifierBonuses,
				CharacterResources = m_Actor.CharacterResources
			};
			m_WorldspacePanelUI.UpdateEffects(dto);
			m_WorldspacePanelUI.UpdateShields(m_Actor.CachedShieldValue, m_Actor.CachedShieldNeutralized);
		}
	}

	private IEnumerator PlayConditionEffect(bool alreadyApplied, bool hasEffect, GameObject effectPrefab, EffectsBar.FEffect fEffect, Action onNewlyApplied)
	{
		while (conditionIsPlaying)
		{
			yield return null;
		}
		conditionIsPlaying = true;
		try
		{
			if (!alreadyApplied)
			{
				VFXShared.PlayEffect(effectPrefab, gameObject.transform, Vector3.zero, Quaternion.identity);
				onNewlyApplied?.Invoke();
			}
			else if (hasEffect && m_WorldspacePanelUI != null)
			{
				m_WorldspacePanelUI.AccentuateEffect(fEffect);
			}
		}
		catch (Exception ex)
		{
			Log.ErrorWithReport("Exception while trying to play condition effect on " + gameObject.name + "\n" + ex.Message + "\n" + ex.StackTrace);
			throw ex;
		}
		yield return new WaitForSeconds(1f);
		conditionIsPlaying = false;
	}

	public void TryPlayConditionEffect(bool alreadyApplied, bool hasEffect, GameObject effectPrefab, EffectsBar.FEffect fEffect, Action onNewlyApplied)
	{
		StartCoroutine(PlayConditionEffect(alreadyApplied, hasEffect, effectPrefab, fEffect, onNewlyApplied));
	}

	public Animator GetAnimator()
	{
		return m_Animator;
	}
}
