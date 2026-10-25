using System;
using System.Collections;
using System.Collections.Generic;
using Chronos;
using DG.Tweening;
using DevLocker.GFrame.Input;
using Libs.GameConsole;
using Libs.Utils.Maths;
using SETUtil.Types;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class CameraController : MonoBehaviour
{
	[Flags]
	public enum ECameraInput
	{
		None = 0,
		Move = 1,
		Zoom = 2,
		Rotate = 4
	}

	public static CameraController s_CameraController;

	public Camera m_Camera;

	public GameObject m_DebugMenu;

	public float m_CameraRotationSpeed = 360f;

	[Tooltip("The height in meters focus point lifted above the ground")]
	public float m_FocusPointHeight = 1.5f;

	[Range(0f, 1f)]
	public float m_FocusSpeed = 0.75f;

	public bool m_EnableEnemyForceRotation = true;

	public bool m_EnablePlayerForceRotation = true;

	public bool m_EnableRotateBack = true;

	public Bounds m_FocalBounds;

	public float m_DragDeadZone = 10f;

	public static bool s_Rotated;

	public static bool s_Translated;

	public static Plane s_GroundPlane = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));

	public static Vector3 s_InitialFocusPoint = new Vector3(0f, 0f, 0f);

	public static float s_InitialHorizontalAngle;

	private Dictionary<object, ECameraInput> m_InputDisablers = new Dictionary<object, ECameraInput>();

	private HashSet<object> m_UINavStickNavigationUsers = new HashSet<object>();

	private InputBinding m_UINavDPadInputMask = new InputBinding("<Gamepad>/dpad");

	public float m_ZoomWheelScalar = 50f;

	public bool m_DragStarted;

	public const float c_MaximumFOV = 75f;

	private const float c_ZoomInterpScalar = 10f;

	private const float c_FocalPointScalar = 5f;

	private const float c_HorizontalRotationScalar = 2.4f;

	private GameObject m_InitialCamera;

	private float m_InitialCameraHeight;

	private Vector3 m_FocalPoint = new Vector3(0f, 0f, 0f);

	private Vector3 m_oldTargetFocalPoint;

	private float m_DeltaAngle;

	private float m_OldLength;

	private float m_TargetZoom;

	private Vector2? m_DragStartCursorPosition;

	private Vector3 m_TranslationStartMousePosOnPlane;

	private Vector3 m_TranslationStartTargetFocalPoint;

	private GameObject m_TargetFocalPointGameObject;

	private CInteractable m_TargetFocalPointInteractable;

	private Vector3 m_CameraToFocalTargetDiff;

	private float m_CameraStartAngle;

	private bool m_RotationStarted;

	private bool m_TranslationStarted;

	private float m_CameraDefaultRadius;

	private float m_DefaultZoomFactor;

	private bool m_HasFocus = true;

	private float m_CameraGameHorizontalAngle;

	private Vector3 m_CameraLeftVector;

	private bool m_PauseDuringTransition;

	private bool m_TranslationKeyPressed;

	private int m_ForceRotate;

	private bool m_ForceRotated;

	[Space]
	[Header("FreeCam Settings")]
	private float m_CameraSensitivity = 10f;

	[SerializeField]
	private float m_LeftJoySpeed = 5f;

	[SerializeField]
	private float m_RightJoySpeed = 70f;

	[SerializeField]
	private float m_ShiftJoySpeed = 5f;

	private float m_ClimbSpeed = 4f;

	private int m_CameraMoveSpeedTextID = 2;

	private float m_RotationX;

	private float m_RotationY;

	private bool m_IsFreeCamEnabled;

	private float m_LastRealTime;

	private Vector3 m_CurrentCameraPos;

	private Quaternion m_CurrentCameraRot;

	private bool m_HoldFreeCamPosition;

	private bool isRotatingLeft;

	private bool isRotatingRight;

	private float rotateSpeed = 4f;

	private int rotateSpeedTextID = 3;

	private float rotateDepth = 3f;

	private Vector3 rotateDirection;

	private Vector3 rotatePoint;

	private bool smoothRotating;

	private bool smoothRotatingRight;

	private float newSmoothRotateAngle;

	private float distanceFromTheTarget;

	private bool startComplete;

	private bool isInitialised;

	private IEnumerator m_CameraToProfileRoutine;

	private Coroutine m_CameraZoomFOVRoutine;

	public Action<Vector3> onArrived;

	private const string LOG_CAMERA_FORMAT = "<color=#A3DA77FF>[CAMERA]</color> {0}";

	private ICameraBehavior overriddenBehavior;

	private static bool _allowFreeCam = false;

	private CanvasGroup m_WorldUICanvasGroup;

	public Vector3 m_TargetFocalPoint { get; set; }

	public bool m_EnableCameraShake { get; set; }

	public Vector3 m_CameraShakePosition { get; set; }

	public Quaternion m_CameraShakeRotation { get; set; }

	public bool m_IsCameraCodeControlDisabled { get; set; }

	public ECameraInput CameraInputDisabled { get; private set; }

	public float m_CameraMoveSpeed { get; set; }

	public float m_ZoomOutExtraHeight { get; set; }

	public float m_DefaultFOV { get; set; }

	public float m_MinimumFOV { get; set; }

	public float m_ExtraMinimumFOV { get; set; }

	public float m_ZoomFactor => CalculateZoomFactor(m_Camera.fieldOfView);

	public bool StartComplete => startComplete;

	public Vector3 FocusPoint => m_FocalPoint;

	public float Zoom => m_TargetZoom;

	public ICameraBehavior OverriddenBehavior => overriddenBehavior;

	public float TotalMinimumFOV => m_MinimumFOV + m_ExtraMinimumFOV;

	private void Awake()
	{
		s_CameraController = this;
	}

	private void Start()
	{
		m_HasFocus = Application.isFocused;
		m_InitialCamera = m_Camera.gameObject;
		m_InitialCameraHeight = m_Camera.transform.position.y;
		m_MinimumFOV = 30f;
		m_DefaultFOV = 75f;
		m_TargetZoom = m_DefaultFOV / 1.5f;
		m_TargetFocalPoint = s_InitialFocusPoint;
		m_FocalPoint = m_TargetFocalPoint;
		CameraInputDisabled = ECameraInput.None;
		InputFHManager.Controls.UI.Navigate.bindingMask = m_UINavDPadInputMask;
		m_ZoomOutExtraHeight = 10f;
		m_CameraMoveSpeed = 14f;
		m_IsCameraCodeControlDisabled = false;
		m_EnableCameraShake = false;
		m_CameraShakePosition = Vector3.zero;
		m_CameraShakeRotation = Quaternion.identity;
		if (DebugMenu.Instance == null)
		{
			m_DebugMenu.SetActive(value: true);
			DebugMenu.Instance.Init();
			m_DebugMenu.SetActive(value: false);
		}
		PlayableDirector component = gameObject.GetComponent<PlayableDirector>();
		if (component != null)
		{
			component.played += OnStartPlaying;
			component.stopped += OnStopPlaying;
		}
		startComplete = true;
	}

	private void OnDestroy()
	{
		overriddenBehavior?.OnDestroy();
		if (InputFHManager.Controls != null)
		{
			InputFHManager.Controls.DisableAll(this);
			InputFHManager.Controls.UI.Navigate.bindingMask = null;
		}
	}

	public void InitCamera()
	{
		m_FocalPoint = m_Camera.transform.position + m_Camera.transform.forward * 100f;
		float num = ((m_Camera.transform.position.y != m_FocalPoint.y) ? (m_Camera.transform.position.y / (m_Camera.transform.position.y - m_FocalPoint.y)) : 0f);
		m_FocalPoint = m_Camera.transform.position + (m_FocalPoint - m_Camera.transform.position) * num;
		m_TargetFocalPoint = m_FocalPoint;
		Vector3 vector = m_FocalPoint - m_Camera.transform.position;
		vector.y = 0f;
		m_CameraDefaultRadius = vector.magnitude;
		m_CameraLeftVector = -m_Camera.transform.right;
		s_InitialHorizontalAngle = 57.29578f * Mathf.Acos(Vector3.Dot(m_CameraLeftVector, m_Camera.transform.forward));
		m_InitialCameraHeight = m_Camera.transform.position.y;
		m_CameraGameHorizontalAngle = s_InitialHorizontalAngle;
		m_TargetZoom = m_Camera.fieldOfView;
		SetCameraDirection(s_InitialHorizontalAngle);
		m_DefaultZoomFactor = m_ZoomFactor;
		FocalBounds component = m_Camera.GetComponent<FocalBounds>();
		if (component == null)
		{
			SetFocalBoundsForScenario(new Vector3(0f, 0f, 0f), new Vector3(1000f, 1000f, 1000f));
		}
		else
		{
			SetFocalBoundsForScenario(component.Center, component.Bounds);
		}
		TileBehaviour[] array = UnityEngine.Object.FindObjectsOfType<TileBehaviour>();
		if (array.Length != 0)
		{
			s_CameraController.m_FocalBounds = default;
			TileBehaviour[] array2 = array;
			foreach (TileBehaviour tileBehaviour in array2)
			{
				s_CameraController.m_FocalBounds.Encapsulate(tileBehaviour.gameObject.transform.position);
			}
		}
		isInitialised = true;
	}

	private void OnApplicationFocus(bool focus)
	{
		m_HasFocus = focus;
	}

	private void OnEnable()
	{
		m_HasFocus = Application.isFocused;
		InputFHManager.Controls.Enable(this, InputFHManager.Controls.Camera);
		InputFHManager.Controls.Disable(this, InputFHManager.Controls.CameraFree);
	}

	private void OnDisable()
	{
		InputFHManager.Controls?.DisableAll(this);
	}

	public void OnStartPlaying(PlayableDirector pb)
	{
		m_IsCameraCodeControlDisabled = true;
	}

	public void OnStopPlaying(PlayableDirector pb)
	{
		m_IsCameraCodeControlDisabled = false;
	}

	public void SetCamera(GameObject newCamera)
	{
		if (newCamera == null)
		{
			m_Camera.gameObject.SetActive(value: false);
			m_InitialCamera.SetActive(value: true);
			m_Camera = m_InitialCamera.GetComponent<Camera>();
		}
		else if (!(newCamera == m_Camera?.gameObject))
		{
			m_Camera.gameObject.SetActive(value: false);
			m_Camera = newCamera.GetComponent<Camera>();
			InitCamera();
		}
	}

	public void SetFocalBoundsForScenario(Vector3 center, Vector3 bounds)
	{
		SetFocalBoundsForScenario(new Bounds(center, bounds));
	}

	public void SetFocalBoundsForScenario(Bounds bounds)
	{
		m_FocalBounds = bounds;
	}

	private float CalculateZoomFactor(float zoom)
	{
		return (zoom - m_MinimumFOV) / (m_DefaultFOV - m_MinimumFOV);
	}

	private float CalculateZoomByZoomFactor(float zoomFactor)
	{
		return zoomFactor * (m_DefaultFOV - m_MinimumFOV) + m_MinimumFOV;
	}

	public float CalculateYForZoom(float zoom)
	{
		return CalculateYForZoomFactor(CalculateZoomFactor(zoom));
	}

	private float CalculateYForZoomFactor(float zoomFactor)
	{
		float num = Mathf.Max((zoomFactor - m_DefaultZoomFactor) / (1f - m_DefaultZoomFactor), 0f);
		return m_InitialCameraHeight + num * m_ZoomOutExtraHeight;
	}

	public float? CalculateZoomForY(float y)
	{
		if (m_ZoomOutExtraHeight <= 0f)
		{
			return null;
		}
		float num = (y - m_InitialCameraHeight) / m_ZoomOutExtraHeight;
		if (num <= 0f)
		{
			return null;
		}
		float zoomFactor = num * (1f - m_DefaultZoomFactor) + m_DefaultZoomFactor;
		return CalculateZoomByZoomFactor(zoomFactor);
	}

	public void RemoveFocusFromObject(GameObject gameobject)
	{
		if (m_TargetFocalPointGameObject == gameobject)
		{
			m_TargetFocalPointGameObject = null;
		}
	}

	public bool IsFocalPointGameObject(GameObject go)
	{
		return m_TargetFocalPointGameObject == go;
	}

	private bool IsCameraMovementAllowed(GameObject forGameobject)
	{
		if (SaveData.Instance.Global.AllowAutomaticTacticalCameraMovement)
		{
			return true;
		}
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(forGameobject);
		if (actorBehaviour == null)
		{
			return true;
		}
		if (actorBehaviour.Actor.Type == CActor.EType.Player)
		{
			if (!FFSNetwork.IsOnline)
			{
				return true;
			}
			if (actorBehaviour.Actor == Choreographer.s_Choreographer.CurrentActor && !Choreographer.s_Choreographer.ThisPlayerHasTurnControl)
			{
				return true;
			}
		}
		return false;
	}

	public void SetFocalPointGameObject(GameObject gameobject)
	{
		if (IsCameraMovementAllowed(gameobject))
		{
			m_TargetFocalPointGameObject = gameobject;
			if ((bool)gameobject)
			{
				m_TargetFocalPointInteractable = gameobject.GetComponent<CInteractable>();
			}
			else
			{
				m_TargetFocalPointInteractable = null;
			}
		}
	}

	public void SetOptimalViewPoint(Vector3 srcActorPos, Vector3 dstActorPos, CActor.EType actorType)
	{
		if ((actorType != CActor.EType.Enemy || m_EnableEnemyForceRotation) && (actorType != CActor.EType.HeroSummon || m_EnableEnemyForceRotation) && (actorType != CActor.EType.Player || m_EnablePlayerForceRotation) && SaveData.Instance.Global.AllowAutomaticTacticalCameraRotation)
		{
			Vector3 normalized = (Camera.main.WorldToScreenPoint(dstActorPos) - Camera.main.WorldToScreenPoint(srcActorPos)).normalized;
			if (Mathf.Acos(Mathf.Abs(Vector3.Dot(normalized, Vector3.up))) / MathF.PI * 180f < 20f)
			{
				m_ForceRotated = true;
				m_ForceRotate = 1;
			}
		}
	}

	public void ResetOptimalViewPoint()
	{
		if (m_ForceRotated && m_EnableRotateBack)
		{
			m_ForceRotate = -1;
		}
		m_ForceRotated = false;
	}

	public void SmartFocus(GameObject target, bool pauseDuringTransition = false)
	{
		if (target == null)
		{
			return;
		}
		if (m_IsFreeCamEnabled)
		{
			Log.GUIInfo("Skip SmartFocus because free cam is enabled");
		}
		else
		{
			if (!IsCameraMovementAllowed(target))
			{
				return;
			}
			SetFocalPointGameObject(null);
			Vector3 allAveragePositions = Choreographer.s_Choreographer.GetAllAveragePositions();
			Vector3 position = target.transform.position;
			Vector3 vector = (allAveragePositions + position) / 2f - m_TargetFocalPoint;
			Vector3 vector2 = m_Camera.WorldToScreenPoint(allAveragePositions - vector);
			vector2.z = 0.5f;
			Vector3 vector3 = m_Camera.WorldToScreenPoint(position - vector);
			vector3.z = 0.5f;
			Vector3 point = m_Camera.WorldToScreenPoint(position);
			point.z = 0f;
			Vector3 vector4 = vector3 - vector2;
			RectTransform screenVisibleArea = UIManager.Instance.ScreenVisibleArea;
			Vector3[] array = new Vector3[4];
			screenVisibleArea.GetWorldCorners(array);
			Vector3 min = UIManager.Instance.UICamera.WorldToScreenPoint(array[0]);
			Vector3 max = UIManager.Instance.UICamera.WorldToScreenPoint(array[2]);
			Bounds bounds = default;
			min.z = 0f;
			bounds.min = min;
			max.z = 1f;
			bounds.max = max;
			if (bounds.Contains(point))
			{
				Log.GUIInfo("Skip SmartFocus original pos is visible");
				return;
			}
			Log.GUIInfo("SmartFocus pause transition " + pauseDuringTransition + " " + TimeManager.IsPaused);
			if (pauseDuringTransition && !TimeManager.IsPaused)
			{
				m_PauseDuringTransition = pauseDuringTransition;
				TimeManager.PauseTime();
			}
			if (!bounds.IntersectRay(new Ray(bounds.center, vector4.normalized), out var distance) || distance < 0f)
			{
				m_TargetFocalPoint = position;
				return;
			}
			if (distance > vector4.magnitude)
			{
				m_TargetFocalPoint = allAveragePositions;
				return;
			}
			float t = distance / vector4.magnitude;
			m_TargetFocalPoint = Vector3.Lerp(position, allAveragePositions, t);
		}
	}

	public void SetFocalPointBetweenTwoObjects(GameObject firstObject, GameObject secondObject)
	{
		if (IsCameraMovementAllowed(firstObject))
		{
			m_TargetFocalPointGameObject = null;
			Vector3 targetFocalPoint = (firstObject.transform.position + secondObject.transform.position) / 2f;
			m_TargetFocalPoint = targetFocalPoint;
			if (!BothOjbectsOnScreen(firstObject, secondObject))
			{
				StartCoroutine(ZoomOutUntilBothObjectsVisible(firstObject, secondObject));
			}
		}
	}

	public void SetOverriddenBehavior(ICameraBehavior behavior)
	{
		ClearOverriddenBehavior();
		overriddenBehavior = behavior;
		behavior.Enable(this);
	}

	public void ClearOverriddenBehavior()
	{
		overriddenBehavior?.Disable(this);
		overriddenBehavior = null;
	}

	public void ClearOverriddenBehavior(ECameraBehaviorType type)
	{
		if (overriddenBehavior != null && overriddenBehavior.IsType(type))
		{
			ClearOverriddenBehavior();
		}
	}

	public void ClearOverriddenBehavior(string id)
	{
		if (overriddenBehavior != null && overriddenBehavior.ID == id)
		{
			ClearOverriddenBehavior();
		}
	}

	private IEnumerator ZoomOutUntilBothObjectsVisible(GameObject firstObject, GameObject secondObject)
	{
		m_TargetZoom += Timekeeper.instance.m_GlobalClock.deltaTime * m_ZoomWheelScalar;
		if (!BothOjbectsOnScreen(firstObject, secondObject))
		{
			yield return Timekeeper.instance.WaitForSeconds(0.005f);
			if (m_TargetZoom < 75f)
			{
				StartCoroutine(ZoomOutUntilBothObjectsVisible(firstObject, secondObject));
			}
		}
	}

	public void ZoomInUtilBothObjectOnBorder(GameObject firstObject, GameObject secondObject)
	{
		m_TargetFocalPointGameObject = null;
		Vector3 targetFocalPoint = (firstObject.transform.position + secondObject.transform.position) / 2f;
		m_TargetFocalPoint = targetFocalPoint;
		if (BothOjbectsOnScreen(firstObject, secondObject))
		{
			StartCoroutine(ZoomInUtilBothObjectOnBorderCoroutine(firstObject, secondObject));
		}
		else
		{
			StartCoroutine(ZoomOutUntilBothObjectsVisible(firstObject, secondObject));
		}
	}

	private IEnumerator ZoomInUtilBothObjectOnBorderCoroutine(GameObject firstObject, GameObject secondObject)
	{
		m_TargetFocalPointGameObject = null;
		Vector3 targetFocalPoint = (firstObject.transform.position + secondObject.transform.position) / 2f;
		m_TargetFocalPoint = targetFocalPoint;
		yield return null;
		m_TargetZoom -= Timekeeper.instance.m_GlobalClock.deltaTime * m_ZoomWheelScalar;
		if (!BothOjbectsOnScreenBorder(firstObject, secondObject))
		{
			yield return Timekeeper.instance.WaitForSeconds(0.005f);
			if (m_TargetZoom > TotalMinimumFOV)
			{
				StartCoroutine(ZoomInUtilBothObjectOnBorderCoroutine(firstObject, secondObject));
			}
		}
	}

	private bool BothOjbectsOnScreenBorder(GameObject firstObject, GameObject secondObject)
	{
		Vector3 vector = Camera.main.WorldToScreenPoint(firstObject.transform.position);
		Vector3 vector2 = Camera.main.WorldToScreenPoint(secondObject.transform.position);
		int num = (int)((float)Screen.width / 3.5f);
		int num2 = Screen.height / 5;
		if ((vector.x > (float)num && vector.x < (float)(Screen.width - num) && vector.y > (float)num2 && vector.y < (float)(Screen.height - num2)) || (vector2.x < (float)num && vector2.x > (float)(Screen.width - num) && vector2.y < (float)num2 && vector2.y > (float)(Screen.height - num2)))
		{
			return false;
		}
		return true;
	}

	private bool BothOjbectsOnScreen(GameObject firstObject, GameObject secondObject)
	{
		Vector3 vector = Camera.main.WorldToScreenPoint(firstObject.transform.position);
		Vector3 vector2 = Camera.main.WorldToScreenPoint(secondObject.transform.position);
		if (vector.x < 0f || (vector.x > (float)Screen.width && vector.y < 0f) || (vector.y > (float)Screen.height && vector2.x < 0f) || (vector2.x > (float)Screen.width && vector2.y < 0f) || vector2.y > (float)Screen.height)
		{
			return false;
		}
		return true;
	}

	public void DisableCameraInput(bool disableInput, object source)
	{
		DisableCameraInput(disableInput ? ((ECameraInput)(-1)) : ECameraInput.None, source);
	}

	public void DisableCameraInput(ECameraInput input, object source)
	{
		if (input != ECameraInput.None && !IsCameraMovementAllowed(Choreographer.s_Choreographer.FindClientActorGameObject(Choreographer.s_Choreographer.CurrentActor)))
		{
			return;
		}
		if (input == ECameraInput.None)
		{
			m_InputDisablers.Remove(source);
		}
		else
		{
			m_InputDisablers[source] = input;
		}
		ECameraInput eCameraInput = ECameraInput.None;
		foreach (KeyValuePair<object, ECameraInput> inputDisabler in m_InputDisablers)
		{
			eCameraInput |= inputDisabler.Value;
		}
		if (CameraInputDisabled == eCameraInput || InputFHManager.Controls == null)
		{
			return;
		}
		CameraInputDisabled = eCameraInput;
		foreach (InputAction allAction in InputFHManager.Controls.GetAllActions())
		{
			if (allAction.enabled)
			{
				allAction.Reset();
			}
		}
	}

	public void ToggleStickUINavigation(object source, bool enable)
	{
		int count = m_UINavStickNavigationUsers.Count;
		if (enable)
		{
			m_UINavStickNavigationUsers.Add(source);
		}
		else
		{
			m_UINavStickNavigationUsers.Remove(source);
		}
		if (count != m_UINavStickNavigationUsers.Count)
		{
			InputFHManager.Controls.UI.Navigate.bindingMask = ((m_UINavStickNavigationUsers.Count > 0) ? ((InputBinding?)null) : new InputBinding?(m_UINavDPadInputMask));
		}
	}

	public void MoveToLook(Vector3 locationToLookAt, float movementSpeed, bool keepCameraLockedUponCompletion = false, Action onCompletedCallback = null)
	{
		m_IsCameraCodeControlDisabled = true;
		distanceFromTheTarget = m_TargetFocalPoint.x - m_Camera.transform.position.x;
		m_TargetFocalPoint = locationToLookAt;
		Vector3 vector = new Vector3(locationToLookAt.x - distanceFromTheTarget, m_Camera.transform.position.y, locationToLookAt.z);
		float duration = (vector - transform.position).magnitude / ((movementSpeed == 0f) ? 1f : movementSpeed);
		m_Camera.transform.DOMove(vector, duration).SetEase(Ease.InOutCubic).OnComplete(() =>
		{
			m_IsCameraCodeControlDisabled = keepCameraLockedUponCompletion;
			onCompletedCallback?.Invoke();
		});
	}

	public CLevelCameraProfile GetCurrentCameraProfile()
	{
		return new CLevelCameraProfile
		{
			FocalPosition = new CVector3(m_TargetFocalPoint.x, m_TargetFocalPoint.y, m_TargetFocalPoint.z),
			CameraPosition = new CVector3(m_Camera.transform.position.x, m_Camera.transform.position.y, m_Camera.transform.position.z),
			CameraFieldOfView = m_Camera.fieldOfView
		};
	}

	public void SetCameraWithProfile(CLevelCameraProfile profileToSetTo)
	{
		if (profileToSetTo.CameraPosition != null && profileToSetTo.FocalPosition != null)
		{
			transform.position = GloomUtility.CVToV(profileToSetTo.CameraPosition);
			m_TargetFocalPoint = GloomUtility.CVToV(profileToSetTo.FocalPosition);
			m_TargetZoom = profileToSetTo.CameraFieldOfView;
			m_CameraGameHorizontalAngle = Quaternion.FromToRotation(m_CameraLeftVector, (m_TargetFocalPoint - transform.position).normalized).eulerAngles.y;
			SetCameraDirection(m_CameraGameHorizontalAngle);
		}
	}

	public void SetCameraWithMessageProfile(CLevelCameraProfile profileToSetTo)
	{
		if (m_CameraToProfileRoutine != null)
		{
			StopCoroutine(m_CameraToProfileRoutine);
			m_CameraToProfileRoutine = null;
		}
		SetFocalPointGameObject(null);
		m_CameraToProfileRoutine = SetCameraProfileOverTime(profileToSetTo);
		StartCoroutine(m_CameraToProfileRoutine);
	}

	private IEnumerator SetCameraProfileOverTime(CLevelCameraProfile profileToSetTo)
	{
		if (profileToSetTo.CameraPosition != null && profileToSetTo.FocalPosition != null)
		{
			DisableCameraInput(disableInput: true, this);
			_ = transform.position;
			Vector3 vector = GloomUtility.CVToV(profileToSetTo.CameraPosition);
			Vector3 startFocal = m_FocalPoint;
			Vector3 endFocal = GloomUtility.CVToV(profileToSetTo.FocalPosition);
			endFocal.y = startFocal.y;
			float startFOV = m_Camera.fieldOfView;
			float endFOV = profileToSetTo.CameraFieldOfView;
			float startCameraAngle = m_CameraGameHorizontalAngle;
			float endCameraAngle = Quaternion.FromToRotation(m_CameraLeftVector, (endFocal - vector).normalized).eulerAngles.y;
			float deltaCameraAngle = endCameraAngle - startCameraAngle;
			if (deltaCameraAngle > 180f)
			{
				deltaCameraAngle -= 360f;
			}
			if (deltaCameraAngle < -180f)
			{
				deltaCameraAngle += 360f;
			}
			float timeToTake = 1f;
			for (float timeElapsed = 0f; timeElapsed < timeToTake; timeElapsed += Timekeeper.instance.m_GlobalClock.unscaledDeltaTime)
			{
				float num = timeElapsed / timeToTake;
				float t = num * num * (3f - 2f * num);
				m_TargetFocalPoint = Vector3.Lerp(startFocal, endFocal, t);
				m_TargetZoom = Mathf.Lerp(startFOV, endFOV, t);
				m_CameraGameHorizontalAngle = startCameraAngle + Mathf.Lerp(0f, deltaCameraAngle, t);
				SetCameraDirection(m_CameraGameHorizontalAngle);
				yield return null;
			}
			m_TargetFocalPoint = endFocal;
			m_TargetZoom = endFOV;
			m_CameraGameHorizontalAngle = endCameraAngle;
			SetCameraDirection(m_CameraGameHorizontalAngle);
			DisableCameraInput(disableInput: false, this);
		}
	}

	public void ZoomFOV(float increaseAmount, float duration, Action onZoomed = null)
	{
		float zoom = Mathf.Clamp(m_TargetZoom + increaseAmount, TotalMinimumFOV, m_DefaultFOV);
		ZoomToFOV(zoom, duration, onZoomed);
	}

	public void ZoomToPercent(float percent, float duration, Action onZoomed = null)
	{
		ZoomToFOV(CalculateZoomByPercent(percent), duration, onZoomed);
	}

	public float CalculateZoomByPercent(float percent)
	{
		return TotalMinimumFOV + (m_DefaultFOV - TotalMinimumFOV) * percent;
	}

	public void ZoomToFOV(float zoom, float duration, Action onZoomed = null)
	{
		CancelZoom();
		Log.InfoFormat("<color=#A3DA77FF>[CAMERA]</color> {0}", "Zoom to " + zoom + " from " + Zoom);
		if (duration == 0f)
		{
			ResetZoomTo(zoom);
		}
		else
		{
			m_CameraZoomFOVRoutine = StartCoroutine(ZoomTo(zoom, duration, onZoomed));
		}
	}

	public void CancelZoom()
	{
		ClearOverriddenBehavior(ECameraBehaviorType.Zoom);
		if (m_CameraZoomFOVRoutine != null)
		{
			StopCoroutine(m_CameraZoomFOVRoutine);
			m_CameraZoomFOVRoutine = null;
		}
	}

	private IEnumerator ZoomTo(float zoom, float duration, Action onZoomed = null)
	{
		float startFOV = m_Camera.fieldOfView;
		float timeElapsed = 0f;
		DisableCameraInput(disableInput: true, this);
		for (; timeElapsed < duration; timeElapsed += Timekeeper.instance.m_GlobalClock.unscaledDeltaTime)
		{
			float num = timeElapsed / duration;
			float t = num * num * (3f - 2f * num);
			m_TargetZoom = Mathf.Lerp(startFOV, zoom, t);
			yield return null;
		}
		m_TargetZoom = zoom;
		DisableCameraInput(disableInput: false, this);
		m_CameraZoomFOVRoutine = null;
		onZoomed?.Invoke();
	}

	public void PrintCurrentCameraDetails()
	{
		Log.Info("Current Camera Details:\nFocalPoint:(" + m_FocalPoint.x + ", " + m_FocalPoint.y + ", " + m_FocalPoint.z + ")\nPosition:(" + m_Camera.transform.position.x + ", " + m_Camera.transform.position.y + ", " + m_Camera.transform.position.z + ")\nRotation:(" + m_Camera.transform.rotation.eulerAngles.x + ", " + m_Camera.transform.rotation.eulerAngles.y + ", " + m_Camera.transform.rotation.eulerAngles.z + ")\nFOV:" + m_Camera.fieldOfView);
	}

	private void SetCameraDirection(float angle)
	{
		if (!m_IsFreeCamEnabled)
		{
			Vector3 vector = Quaternion.AngleAxis(angle, Vector3.up) * m_CameraLeftVector;
			m_Camera.transform.position = m_FocalPoint - vector;
			UpdateCameraToFocalTargetDiff();
		}
	}

	private void UpdateCameraToFocalTargetDiff()
	{
		m_CameraToFocalTargetDiff = m_Camera.transform.position - m_FocalPoint;
		m_CameraToFocalTargetDiff.y = 0f;
		m_CameraToFocalTargetDiff.Normalize();
		m_CameraToFocalTargetDiff *= m_CameraDefaultRadius;
	}

	private void CheckDevModeKeys()
	{
		if (Keyboard.current != null && !InputFHManager.GetKey(Key.LeftShift))
		{
			if (InputFHManager.GetKey(Key.Backquote) && InputFHManager.GetKey(Key.Digit1))
			{
				DebugMenu.Instance.StallMainThread(1);
			}
			if (InputFHManager.GetKey(Key.Backquote) && InputFHManager.GetKey(Key.Digit2))
			{
				DebugMenu.Instance.StallMainThread(2);
			}
			if (InputFHManager.GetKey(Key.Backquote) && InputFHManager.GetKey(Key.Digit3))
			{
				DebugMenu.Instance.StallMainThread(3);
			}
			if (InputFHManager.GetKey(Key.Backquote) && InputFHManager.GetKey(Key.Digit4))
			{
				DebugMenu.Instance.StallMainThread(4);
			}
			if (InputFHManager.GetKey(Key.Backquote) && InputFHManager.GetKey(Key.Digit5))
			{
				DebugMenu.Instance.StallMainThread(5);
			}
			if (InputFHManager.GetKey(Key.Backquote) && InputFHManager.GetKey(Key.Digit6))
			{
				DebugMenu.Instance.StallMainThread(6);
			}
			if (InputFHManager.GetKeyDown(Key.F1))
			{
				DebugMenu.Instance.ToggleMenu();
			}
			if (InputFHManager.GetKeyDown(Key.F2))
			{
				GloomUtility.ToggleGUI();
			}
			if (InputFHManager.GetKeyDown(Key.F3))
			{
				DebugMenu.Instance.TogglePause();
			}
			if (InputFHManager.GetKeyDown(Key.F6))
			{
				ToggleFreeCam();
			}
			if (InputFHManager.GetKey(Key.PageUp))
			{
				DebugMenu.Instance.SpeedUp();
			}
			if (InputFHManager.GetKey(Key.PageDown))
			{
				DebugMenu.Instance.SlowDown();
			}
			if (InputFHManager.GetKeyDown(Key.LeftBracket))
			{
				RotateAround(left: true);
			}
			if (InputFHManager.GetKeyDown(Key.RightBracket))
			{
				RotateAround(left: false);
			}
			if (InputFHManager.GetKeyDown(Key.F4))
			{
				DebugMenu.Instance.ChangeWallFadeMode();
			}
			if (InputFHManager.GetKeyDown(Key.F7))
			{
				ResetCameraPosition();
			}
			if (InputFHManager.GetKey(Key.F7))
			{
				LevelEditorController.RegenerateApparance();
			}
		}
	}

	private void CheckPerfTestKeys()
	{
		if (InputFHManager.GetKey(Key.LeftShift))
		{
			if (InputFHManager.GetKeyDown(Key.F1))
			{
				PerformanceUtility.TurnOffPointLightShadows();
			}
			if (InputFHManager.GetKeyDown(Key.F2))
			{
				PerformanceUtility.Combine();
			}
			if (InputFHManager.GetKeyDown(Key.F3))
			{
				PerformanceUtility.CanApplyChaos = !PerformanceUtility.CanApplyChaos;
				Log.Info($"CanApplyChaos:{PerformanceUtility.CanApplyChaos}");
			}
			if (InputFHManager.GetKeyDown(Key.F11))
			{
				Log.Info($"VSync was:{QualitySettings.vSyncCount}");
				QualitySettings.vSyncCount = 0;
			}
			if (InputFHManager.GetKeyDown(Key.F12))
			{
				PerformanceUtility.SetShadowsOff();
			}
		}
	}

	public void SetCameraDirectionAndFocalPoint(Transform pointOfView, Vector3 focalPoint, int forwardAxis = 2)
	{
		if (!m_IsFreeCamEnabled)
		{
			m_TargetFocalPoint = focalPoint;
			Vector3 vector = forwardAxis switch
			{
				1 => pointOfView.up, 
				0 => pointOfView.right, 
				_ => pointOfView.forward, 
			};
			Vector3 normalized = (focalPoint - pointOfView.position).normalized;
			Vector3 vector2 = Mathf.Sign(Vector3.Dot(vector, normalized)) * vector;
			m_CameraGameHorizontalAngle = Quaternion.FromToRotation(m_CameraLeftVector, vector2.normalized).eulerAngles.y;
			m_CameraGameHorizontalAngle = Mathf.Round(m_CameraGameHorizontalAngle / 45f) * 45f;
			SetCameraDirection(m_CameraGameHorizontalAngle);
		}
	}

	public void RotateAround(bool left)
	{
		if (left)
		{
			if (isRotatingLeft)
			{
				isRotatingLeft = false;
				return;
			}
			rotatePoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, rotateDepth));
			rotateDirection = new Vector3(0f, -1f, 0f);
			isRotatingLeft = true;
			isRotatingRight = false;
		}
		else if (isRotatingRight)
		{
			isRotatingRight = false;
		}
		else
		{
			rotatePoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, rotateDepth));
			rotateDirection = new Vector3(0f, 1f, 0f);
			isRotatingLeft = false;
			isRotatingRight = true;
		}
	}

	private void SmoothRotate()
	{
		float num = 0f;
		if (smoothRotatingRight)
		{
			if (m_CameraGameHorizontalAngle >= 360f)
			{
				m_CameraGameHorizontalAngle -= 360f;
				newSmoothRotateAngle -= 360f;
			}
			num = m_CameraGameHorizontalAngle + m_CameraRotationSpeed * Timekeeper.instance.m_GlobalClock.unscaledDeltaTime;
			if (num > newSmoothRotateAngle)
			{
				num = newSmoothRotateAngle;
			}
		}
		else
		{
			if (m_CameraGameHorizontalAngle <= 0f)
			{
				m_CameraGameHorizontalAngle = 360f - m_CameraGameHorizontalAngle;
				newSmoothRotateAngle += 360f;
			}
			num = m_CameraGameHorizontalAngle - m_CameraRotationSpeed * Timekeeper.instance.m_GlobalClock.unscaledDeltaTime;
			if (num < newSmoothRotateAngle)
			{
				num = newSmoothRotateAngle;
			}
		}
		m_CameraGameHorizontalAngle = Mathf.Lerp(m_CameraGameHorizontalAngle, num, 0.5f);
		SetCameraDirection(m_CameraGameHorizontalAngle);
		if (Mathf.Abs(m_CameraGameHorizontalAngle - newSmoothRotateAngle) < 0.5f)
		{
			m_CameraGameHorizontalAngle = Mathf.Round(m_CameraGameHorizontalAngle);
			smoothRotating = false;
		}
	}

	public void ResetCameraRotation()
	{
		SetCameraDirection(m_CameraGameHorizontalAngle);
	}

	private void RotateAroundController()
	{
		float num = Time.realtimeSinceStartup - m_LastRealTime;
		m_Camera.transform.RotateAround(rotatePoint, rotateDirection, rotateSpeed * num);
		m_LastRealTime = Time.realtimeSinceStartup;
		if (InputFHManager.GetKey(Key.A) || InputFHManager.GetKey(Key.LeftArrow) || InputFHManager.GetKey(Key.S) || InputFHManager.GetKey(Key.DownArrow) || InputFHManager.GetKey(Key.W) || InputFHManager.GetKey(Key.UpArrow) || InputFHManager.GetKey(Key.D) || InputFHManager.GetKey(Key.RightArrow))
		{
			isRotatingLeft = false;
			isRotatingRight = false;
		}
		if (InputFHManager.GetKey(Key.Numpad8))
		{
			rotateDepth += 0.1f;
			rotatePoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, rotateDepth));
		}
		if (InputFHManager.GetKey(Key.Numpad2))
		{
			if (rotateDepth - 0.1f <= 0.5f)
			{
				rotateDepth = 0.5f;
			}
			else
			{
				rotateDepth -= 0.1f;
			}
			rotatePoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, rotateDepth));
		}
		if (InputFHManager.GetKey(Key.Numpad4))
		{
			if (isRotatingLeft)
			{
				rotateSpeed += 0.05f;
			}
			else if (rotateSpeed - 0.05f <= 0f)
			{
				rotateSpeed = 0f;
			}
			else
			{
				rotateSpeed -= 0.05f;
			}
			DebugMenu.Instance.DisplayDebugText(rotateSpeedTextID, "Rotate Speed: " + rotateSpeed.ToString("0.00"));
		}
		if (InputFHManager.GetKey(Key.Numpad6))
		{
			if (isRotatingRight)
			{
				rotateSpeed += 0.05f;
			}
			else if (rotateSpeed - 0.05f <= 0f)
			{
				rotateSpeed = 0f;
			}
			else
			{
				rotateSpeed -= 0.05f;
			}
			DebugMenu.Instance.DisplayDebugText(rotateSpeedTextID, "Rotate Speed: " + rotateSpeed.ToString("0.00"));
		}
		CheckDevModeKeys();
	}

	[ContextMenu("Test Free Cam Position")]
	private void TestFreeCamPosition()
	{
		SetFreeCamPosition(default);
	}

	public void SetFreeCamPosition(TransformData trdt)
	{
		m_IsFreeCamEnabled = true;
		m_HoldFreeCamPosition = true;
		Cursor.lockState = CursorLockMode.Locked;
		Vector3 currentCameraPos = (m_Camera.transform.position = trdt.position);
		m_CurrentCameraPos = currentCameraPos;
		Quaternion currentCameraRot = (m_Camera.transform.rotation = trdt.rotation);
		m_CurrentCameraRot = currentCameraRot;
		m_RotationX = m_Camera.transform.eulerAngles.y;
		m_RotationY = 0f - m_Camera.transform.eulerAngles.x;
	}

	[ConsoleCommand(Command = "allow_free_cam")]
	public static void AllowFreeCamMode(IConsole console)
	{
		console.WriteLine($"Free cam is now available. Use {KeyCode.F4} to fly.");
		_allowFreeCam = true;
	}

	public void ToggleFreeCam()
	{
		if (_allowFreeCam)
		{
			if (m_IsFreeCamEnabled && !m_HoldFreeCamPosition)
			{
				m_HoldFreeCamPosition = true;
				Cursor.lockState = CursorLockMode.None;
				return;
			}
			m_IsFreeCamEnabled = true;
			m_HoldFreeCamPosition = false;
			m_LastRealTime = Time.realtimeSinceStartup;
			Cursor.lockState = CursorLockMode.Locked;
			m_CurrentCameraPos = m_Camera.transform.position;
			m_CurrentCameraRot = m_Camera.transform.rotation;
			m_RotationX = m_Camera.transform.eulerAngles.y;
			m_RotationY = 0f - m_Camera.transform.eulerAngles.x;
		}
	}

	public void ResetCameraPosition()
	{
		if (Main.s_DevMode || Main.s_InternalRelease)
		{
			m_IsFreeCamEnabled = false;
			m_HoldFreeCamPosition = false;
			Cursor.lockState = CursorLockMode.None;
			m_Camera.transform.position = m_CurrentCameraPos;
			m_Camera.transform.rotation = m_CurrentCameraRot;
		}
	}

	private void FreeCamController()
	{
		if (m_HoldFreeCamPosition)
		{
			CheckDevModeKeys();
			m_LastRealTime = Time.realtimeSinceStartup;
			return;
		}
		if (InputFHManager.GetKey(Key.NumpadPlus) || InputFHManager.GetKey(Key.Equals))
		{
			m_LeftJoySpeed = Mathf.Min(50f, m_LeftJoySpeed + 0.05f);
			m_CameraMoveSpeed += 0.05f;
			m_ClimbSpeed += 0.014285715f;
			DebugMenu.Instance.DisplayDebugText(m_CameraMoveSpeedTextID, "Cam Speed: " + m_CameraMoveSpeed.ToString("0.00") + " Vertical: " + m_ClimbSpeed.ToString("0.00"));
			Log.Info($"Movement speed increased by {0.5f}, current speed {m_LeftJoySpeed}");
		}
		if (InputFHManager.GetKey(Key.NumpadMinus) || InputFHManager.GetKey(Key.Minus))
		{
			m_LeftJoySpeed = Mathf.Max(0f, m_LeftJoySpeed - 0.05f);
			m_CameraMoveSpeed -= 0.05f;
			m_ClimbSpeed -= 0.014285715f;
			DebugMenu.Instance.DisplayDebugText(m_CameraMoveSpeedTextID, "Camera Speed: " + m_CameraMoveSpeed.ToString("0.00") + " Vertical: " + m_ClimbSpeed.ToString("0.00"));
			Log.Info($"Movement speed decreased by {0.5f}, current speed {m_LeftJoySpeed}");
		}
		float num = Time.realtimeSinceStartup - m_LastRealTime;
		m_RotationX += InputFHManager.Controls.CameraFree.FreeCamRotate.ReadValue<Vector2>().x * m_CameraSensitivity * num;
		m_RotationY += InputFHManager.Controls.CameraFree.FreeCamRotate.ReadValue<Vector2>().y * m_CameraSensitivity * num;
		if (m_RotationX > 360f)
		{
			m_RotationX -= 360f;
		}
		if (m_RotationY > 360f)
		{
			m_RotationY -= 360f;
		}
		m_Camera.transform.localRotation = Quaternion.AngleAxis(m_RotationX, Vector3.up);
		m_Camera.transform.localRotation *= Quaternion.AngleAxis(m_RotationY, Vector3.left);
		Vector2 vector = InputFHManager.Controls.CameraFree.FreeCamMovement.ReadValue<Vector2>();
		if (vector != Vector2.zero)
		{
			m_Camera.transform.position += m_Camera.transform.right * num * m_CameraMoveSpeed * vector.x;
			m_Camera.transform.position += m_Camera.transform.forward * num * m_CameraMoveSpeed * vector.y;
		}
		m_Camera.transform.position += m_Camera.transform.up * m_ClimbSpeed * num * InputFHManager.Controls.CameraFree.FreeCamMoveVertical.ReadValue<float>();
		if (InputFHManager.GetKeyDown(Key.End))
		{
			Cursor.lockState = ((Cursor.lockState == CursorLockMode.None) ? CursorLockMode.Locked : CursorLockMode.None);
		}
		CheckDevModeKeys();
		m_LastRealTime = Time.realtimeSinceStartup;
	}

	private void LateUpdate()
	{
		if (!isInitialised || m_Camera == null)
		{
			return;
		}
		if (!Main.s_DevMode && !Main.s_InternalRelease)
		{
			if (InputFHManager.GetKey(Key.F2) && !InputFHManager.GetKeyDown(Key.LeftShift))
			{
				if (GloomUtility.IsGUIVisible())
				{
					GloomUtility.ToggleGUI();
				}
			}
			else if (!GloomUtility.IsGUIVisible())
			{
				GloomUtility.ToggleGUI();
			}
		}
		if (SceneController.Instance.GlobalErrorMessage.ShowingMessage || m_IsCameraCodeControlDisabled || (overriddenBehavior != null && overriddenBehavior.Update(this)))
		{
			return;
		}
		if (isRotatingLeft || isRotatingRight)
		{
			RotateAroundController();
			return;
		}
		bool isTextFieldFocused = InputFHManager.IsTextFieldFocused;
		if (m_IsFreeCamEnabled)
		{
			if (!InputFHManager.Controls.CameraFree.enabled)
			{
				InputFHManager.Controls.Disable(this, InputFHManager.Controls.Camera);
				InputFHManager.Controls.Enable(this, InputFHManager.Controls.CameraFree);
			}
			FreeCamController();
			return;
		}
		if (InputFHManager.Controls.CameraFree.enabled)
		{
			InputFHManager.Controls.Enable(this, InputFHManager.Controls.Camera);
			InputFHManager.Controls.Disable(this, InputFHManager.Controls.CameraFree);
		}
		if (!isTextFieldFocused && (Main.s_DevMode || Main.s_InternalRelease))
		{
			CheckDevModeKeys();
			CheckPerfTestKeys();
		}
		if (Main.s_Pause3DWorld)
		{
			return;
		}
		if (!isTextFieldFocused && !CameraInputDisabled.HasFlag(ECameraInput.Rotate))
		{
			if (!smoothRotating)
			{
				if (InputFHManager.Controls.Camera.DragRotate.IsPressed() && Mouse.current != null)
				{
					m_DeltaAngle = (0f - Mouse.current.delta.ReadValue().x) * 2.4f;
					setRotationalVariables();
				}
				else if (m_RotationStarted)
				{
					m_RotationStarted = false;
					s_Rotated = false;
				}
				if (InputFHManager.Controls.Camera.Rotate.ReadValue<float>() < -0.5f || m_ForceRotate < 0)
				{
					float num = 45f - m_CameraGameHorizontalAngle % 45f;
					newSmoothRotateAngle = ((m_CameraGameHorizontalAngle > 360f) ? (m_CameraGameHorizontalAngle - 360f + num) : (m_CameraGameHorizontalAngle + num));
					smoothRotating = true;
					smoothRotatingRight = true;
					m_ForceRotate = 0;
				}
				if (InputFHManager.Controls.Camera.Rotate.ReadValue<float>() > 0.5f || m_ForceRotate > 0)
				{
					float num2 = m_CameraGameHorizontalAngle % 45f;
					if (num2 < 0.0001f)
					{
						num2 = 45f;
					}
					newSmoothRotateAngle = ((m_CameraGameHorizontalAngle < 0f) ? (m_CameraGameHorizontalAngle + 360f - num2) : (m_CameraGameHorizontalAngle - num2));
					smoothRotating = true;
					smoothRotatingRight = false;
					m_ForceRotate = 0;
				}
			}
			else
			{
				SmoothRotate();
			}
		}
		if (Mathf.Abs(m_DeltaAngle) > 0f)
		{
			float num3 = m_DeltaAngle * Timekeeper.instance.m_GlobalClock.unscaledDeltaTime * 3f;
			m_DeltaAngle = ((Mathf.Abs(m_DeltaAngle) < 0.01f) ? 0f : (m_DeltaAngle - num3));
			m_Camera.transform.position += (Vector3)m_Camera.transform.worldToLocalMatrix.GetRow(0) * num3 * m_OldLength * 0.25f;
			m_CameraGameHorizontalAngle = Quaternion.FromToRotation(m_CameraLeftVector, m_Camera.transform.forward).eulerAngles.y;
			UpdateCameraToFocalTargetDiff();
		}
		if (!isTextFieldFocused && !CameraInputDisabled.HasFlag(ECameraInput.Move) && InputFHManager.Controls.Camera.DragMove.IsPressed())
		{
			if (!m_DragStartCursorPosition.HasValue)
			{
				m_DragStartCursorPosition = InputFHManager.CursorPosition;
			}
			Ray ray = m_Camera.ScreenPointToRay(InputFHManager.CursorPosition);
			if (s_GroundPlane.Raycast(ray, out var enter))
			{
				Vector3 point = ray.GetPoint(enter);
				float num4 = Vector2.Distance(m_DragStartCursorPosition.Value, InputFHManager.CursorPosition);
				if (m_DragStarted || (num4 != 0f && num4 > m_DragDeadZone && !UIManager.IsPointerOverUI))
				{
					m_DragStarted = true;
					Singleton<ActorStatPanel>.Instance?.DettachEvents();
					CanvasGroup canvasGroup = m_WorldUICanvasGroup ?? (m_WorldUICanvasGroup = Choreographer.s_Choreographer.m_WorldSpaceCanvas.GetComponent<CanvasGroup>());
					canvasGroup.interactable = false;
					canvasGroup.blocksRaycasts = false;
					if (!m_TranslationStarted)
					{
						m_TranslationStarted = true;
						m_TranslationStartTargetFocalPoint = m_FocalPoint;
						m_TranslationStartMousePosOnPlane = point;
						SetFocalPointGameObject(null);
						s_Translated = false;
					}
					m_TargetFocalPoint = m_TranslationStartTargetFocalPoint + (m_TranslationStartMousePosOnPlane - point);
					SetOverriddenBehavior(new MoveAndZoomCameraBehavior("cameraDragMove", m_TargetZoom, m_TargetFocalPoint, 0f, Ease.Unset, null, () =>
					{
						s_CameraController.ClearOverriddenBehavior();
					}));
					if (!s_Translated)
					{
						s_Translated = !((m_FocalPoint - m_TargetFocalPoint).sqrMagnitude < 0.001f);
					}
				}
			}
		}
		else
		{
			m_DragStartCursorPosition = null;
			if (m_DragStarted)
			{
				m_DragStarted = false;
				Singleton<ActorStatPanel>.Instance?.AttachEvents();
				CanvasGroup canvasGroup2 = m_WorldUICanvasGroup ?? (m_WorldUICanvasGroup = Choreographer.s_Choreographer.m_WorldSpaceCanvas.GetComponent<CanvasGroup>());
				canvasGroup2.interactable = true;
				canvasGroup2.blocksRaycasts = true;
				s_CameraController.ClearOverriddenBehavior();
			}
			if (m_TranslationStarted)
			{
				m_TranslationStarted = false;
			}
		}
		m_oldTargetFocalPoint = m_TargetFocalPoint;
		if (!isTextFieldFocused && !CameraInputDisabled.HasFlag(ECameraInput.Move) && !m_EnableCameraShake)
		{
			Vector2 vector = InputFHManager.Controls.Camera.Movement.ReadValue<Vector2>();
			if (!vector.x.IsZero())
			{
				m_TargetFocalPointInteractable = null;
				m_TargetFocalPoint += m_Camera.transform.right * Timekeeper.instance.m_GlobalClock.unscaledDeltaTime * m_CameraMoveSpeed * vector.x;
				m_TranslationKeyPressed = true;
			}
			if (!vector.y.IsZero())
			{
				m_TargetFocalPointInteractable = null;
				Vector3 forward = m_Camera.transform.forward;
				forward.y = 0f;
				forward = forward.normalized;
				m_TargetFocalPoint += forward * Timekeeper.instance.m_GlobalClock.unscaledDeltaTime * m_CameraMoveSpeed * vector.y;
				m_TranslationKeyPressed = true;
			}
			m_TargetFocalPoint = new Vector3(Mathf.Max(m_FocalBounds.min.x, Mathf.Min(m_FocalBounds.max.x, m_TargetFocalPoint.x)), m_TargetFocalPoint.y, Mathf.Max(m_FocalBounds.min.z, Mathf.Min(m_FocalBounds.max.z, m_TargetFocalPoint.z)));
		}
		if (m_oldTargetFocalPoint != m_TargetFocalPoint)
		{
			m_TargetFocalPoint = new Vector3(m_TargetFocalPoint.x, 0f, m_TargetFocalPoint.z);
			SetFocalPointGameObject(null);
			UpdateCameraToFocalTargetDiff();
		}
		if ((bool)m_TargetFocalPointInteractable)
		{
			m_TargetFocalPoint = m_TargetFocalPointInteractable.transform.position;
		}
		else if ((bool)m_TargetFocalPointGameObject)
		{
			m_TargetFocalPoint = m_TargetFocalPointGameObject.transform.position;
		}
		Vector3 vector2 = (m_TargetFocalPoint - m_FocalPoint) * Mathf.Min(Timekeeper.instance.m_GlobalClock.deltaTime * 5f, 1f);
		if (m_TranslationKeyPressed)
		{
			m_TranslationKeyPressed = (m_TargetFocalPoint - m_FocalPoint).sqrMagnitude > 0.01f;
		}
		else
		{
			vector2 *= m_FocusSpeed;
		}
		if (!m_DragStarted)
		{
			m_FocalPoint += vector2;
		}
		if (vector2.sqrMagnitude < 0.01f)
		{
			onArrived?.Invoke(m_TargetFocalPoint);
			onArrived = null;
			if (m_PauseDuringTransition)
			{
				TimeManager.UnpauseTime();
				m_PauseDuringTransition = false;
			}
		}
		m_TranslationStartTargetFocalPoint += vector2;
		if (!isTextFieldFocused && !CameraInputDisabled.HasFlag(ECameraInput.Zoom))
		{
			bool flag = InputFHManager.Controls.Camera.Zoom.activeControl?.device is Mouse;
			float num5 = InputFHManager.Controls.Camera.Zoom.ReadValue<float>() * m_ZoomWheelScalar * (flag ? 0.016f : Timekeeper.instance.m_GlobalClock.deltaTime);
			if (flag && EventSystem.current.IsPointerOverGameObject() && UIManager.GameObjectUnderPointer()?.GetComponentInParent<IgnoreMouseScroll>() != null)
			{
				num5 = 0f;
			}
			m_TargetZoom -= num5;
		}
		m_TargetZoom = Mathf.Clamp(m_TargetZoom, TotalMinimumFOV, m_DefaultFOV);
		m_Camera.fieldOfView += (m_TargetZoom - m_Camera.fieldOfView) * Mathf.Min(Timekeeper.instance.m_GlobalClock.unscaledDeltaTime * 10f, 1f);
		RefreshFocusPosition();
		if (m_EnableCameraShake)
		{
			m_Camera.transform.position += m_CameraShakePosition;
			m_Camera.transform.rotation *= m_CameraShakeRotation;
		}
		void setRotationalVariables()
		{
			m_OldLength = 1f;
			if (!m_RotationStarted)
			{
				m_RotationStarted = true;
				m_CameraStartAngle = m_Camera.transform.eulerAngles.y;
			}
			s_Rotated = Mathf.Abs(m_CameraStartAngle - m_Camera.transform.eulerAngles.y) > 1f;
		}
	}

	private void RefreshFocusPosition(float? y = null)
	{
		Vector3 position = m_FocalPoint + m_CameraToFocalTargetDiff;
		if (!y.HasValue)
		{
			position.y = CalculateYForZoomFactor(m_ZoomFactor);
		}
		else
		{
			position.y = y.Value;
		}
		if (!float.IsNaN(position.x) && !float.IsNaN(position.y) && !float.IsNaN(position.z))
		{
			m_Camera.transform.position = position;
			m_Camera.transform.LookAt(m_FocalPoint + Vector3.up * m_FocusPointHeight);
		}
	}

	public void ResetPositionToFocusOnTargetPoint()
	{
		ClearOverriddenBehavior(ECameraBehaviorType.Move);
		RefreshPositionToFocusOnTargetPoint();
	}

	public void RefreshPositionToFocusOnTargetPoint(float? y = null)
	{
		Vector3 vector = m_TargetFocalPoint - m_FocalPoint;
		m_FocalPoint += vector;
		m_TranslationStartTargetFocalPoint += vector;
		RefreshFocusPosition(y);
	}

	public void ResetZoomTo(float zoom)
	{
		CancelZoom();
		SetZoom(zoom);
		Log.InfoFormat("<color=#A3DA77FF>[CAMERA]</color> {0}", "Reset Zoom to " + m_TargetZoom);
	}

	public void SetZoom(float zoom)
	{
		m_TargetZoom = Mathf.Clamp(zoom, TotalMinimumFOV, m_DefaultFOV);
		m_Camera.fieldOfView = m_TargetZoom;
	}

	public bool IsMoving()
	{
		if (!m_DragStarted && !m_RotationStarted)
		{
			return m_TranslationStarted;
		}
		return true;
	}
}
