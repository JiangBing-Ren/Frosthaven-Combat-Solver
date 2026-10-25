using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DevLocker.GFrame.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace FHGUI;

public class FHMapCameraController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private BoxCollider _bounds;

	[SerializeField]
	private bool _useBoundsCenterAsDefaultFocusPosition;

	[SerializeField]
	private float _mapHeight;

	[Header("Movement")]
	[SerializeField]
	private bool _allowMoveInput = true;

	[SerializeField]
	private float _movementSpeed;

	[SerializeField]
	private float _acceleration;

	[SerializeField]
	private float _maxSpeed;

	[Header("Drag")]
	[SerializeField]
	private bool _allowDragInput = true;

	[SerializeField]
	private float _dragSpeed;

	[Header("Zoom")]
	[SerializeField]
	private bool _allowZoomInput = true;

	[SerializeField]
	private float _zoomSpeed;

	[SerializeField]
	private float _minZoomDistance;

	[SerializeField]
	private float _maxZoomDistance;

	[SerializeField]
	[Range(0f, 1f)]
	private float _zoomMovementSpeedModifier;

	[Header("Edge Scroll")]
	[SerializeField]
	private bool _allowEdgeScroll = true;

	[SerializeField]
	private float _edgeScrollSpeed;

	[SerializeField]
	private bool _allowPreciseScrollDirection = true;

	[SerializeField]
	[Range(0f, 1f)]
	private float _edgeTriggerSize = 0.2f;

	[Header("Automated Movement")]
	[SerializeField]
	private float _autoMovementDuration = 0.4f;

	[SerializeField]
	private Ease _autoMovementEase = Ease.InOutCubic;

	private FHControls Controls;

	private InputAction MoveAction;

	private InputAction DragAction;

	private InputAction ZoomAction;

	private Plane _mapPlane;

	private Vector3 _mapRight;

	private Vector3 _mapUp;

	private bool _isAutomated;

	private bool _hasMoveInput;

	private bool _isDragging;

	private bool _clampToBounds;

	private float _currentSpeed;

	private Vector3 _initialFocusPosition;

	private Vector3 _targetFocusPosition;

	private Vector3 _targetCameraPosition;

	private Vector3 _smoothedCameraPosition;

	private bool _hasZoomInput;

	private float _currentZoom;

	private float _initialCameraDistance;

	private float _targetCameraDistance;

	private Tween _runningAutomation;

	private Transform _lockedOnTarget;

	private const float REACH_TARGET_TOLERANCE = 0.1f;

	private bool _pointerIsOverUI;

	public Action<float> OnZoom;

	public Camera Camera => _camera;

	private void Awake()
	{
		_mapPlane = new Plane(Vector3.up, Vector3.up * _mapHeight);
		_clampToBounds = _bounds != null;
		_initialFocusPosition = FindCurrentlyFocusedPosition(out _initialCameraDistance);
		if (_clampToBounds && _useBoundsCenterAsDefaultFocusPosition)
		{
			Vector3 vector = _bounds.bounds.center - _initialFocusPosition;
			vector.y = 0f;
			_initialFocusPosition += vector;
		}
		_targetFocusPosition = _initialFocusPosition;
		_targetCameraDistance = _initialCameraDistance;
		_mapRight = _camera.transform.right;
		_mapRight.y = 0f;
		_mapRight = _mapRight.normalized;
		_mapUp = _camera.transform.forward;
		_mapUp.y = 0f;
		_mapUp = _mapUp.normalized;
	}

	private void OnEnable()
	{
		_targetFocusPosition = _initialFocusPosition;
		_targetCameraDistance = _initialCameraDistance;
		_targetCameraPosition = _camera.transform.position;
		_smoothedCameraPosition = _targetCameraPosition;
		if (_allowMoveInput || _allowDragInput || _allowZoomInput)
		{
			FHControls.CameraActions camera = InputFHManager.Controls.Camera;
			InputFHManager.Controls.Enable(this, camera);
			if (_allowMoveInput)
			{
				MoveAction = camera.Movement;
				MoveAction.started += OnMoveStart;
				MoveAction.canceled += OnMoveStop;
			}
			if (_allowDragInput)
			{
				DragAction = camera.DragMove;
				DragAction.started += OnDragMoveStart;
				DragAction.canceled += OnMoveStop;
			}
			if (_allowZoomInput)
			{
				ZoomAction = camera.Zoom;
				ZoomAction.performed += OnZoomStart;
				ZoomAction.canceled += OnZoomEnd;
			}
		}
		OffsetFocusPosition(0f, 0f);
		SetCameraDistance(_initialCameraDistance);
	}

	private void OnDisable()
	{
		if (MoveAction != null)
		{
			MoveAction.started -= OnMoveStart;
			MoveAction.canceled -= OnMoveStop;
		}
		if (DragAction != null)
		{
			DragAction.started -= OnMoveStart;
			DragAction.canceled -= OnMoveStop;
		}
		if (ZoomAction != null)
		{
			ZoomAction.started -= OnZoomStart;
			ZoomAction.canceled -= OnZoomEnd;
		}
		if (InputFHManager.Controls != null)
		{
			InputFHManager.Controls.Enable(this, InputFHManager.Controls.Camera);
		}
	}

	private void OnMoveStart(InputAction.CallbackContext ctx)
	{
		_hasMoveInput = true;
		_isDragging = false;
	}

	private void OnDragMoveStart(InputAction.CallbackContext ctx)
	{
		if (!_pointerIsOverUI)
		{
			_hasMoveInput = true;
			_isDragging = true;
		}
	}

	private void OnMoveStop(InputAction.CallbackContext ctx)
	{
		_hasMoveInput = false;
		_isDragging = false;
	}

	private void OnZoomStart(InputAction.CallbackContext ctx)
	{
		_hasZoomInput = true;
	}

	private void OnZoomEnd(InputAction.CallbackContext ctx)
	{
		_hasZoomInput = false;
	}

	private void ReadInput(out Vector2 moveInput, out float zoomInput)
	{
		moveInput = Vector2.zero;
		zoomInput = 0f;
		if (_hasMoveInput)
		{
			if (_isDragging)
			{
				InputControl activeControl = DragAction.activeControl;
				if (activeControl != null && activeControl.device is Pointer pointer)
				{
					Vector2 value = pointer.delta.value;
					moveInput = -value * _dragSpeed;
					goto IL_0256;
				}
			}
			moveInput = MoveAction.ReadValue<Vector2>().normalized * _movementSpeed * Time.deltaTime;
		}
		else if (_allowEdgeScroll && InputFHManager.Controls.Camera.enabled)
		{
			Vector2 cursorPosition = InputFHManager.CursorPosition;
			Vector2 vector = new Vector2(Screen.width, Screen.height);
			Vector2 vector2 = vector * _edgeTriggerSize * 0.5f;
			if (_allowPreciseScrollDirection)
			{
				if (cursorPosition.x < vector2.x || cursorPosition.x > (float)Screen.width - vector2.x || cursorPosition.y < vector2.y || cursorPosition.y > (float)Screen.height - vector2.y)
				{
					moveInput = (cursorPosition - vector * 0.5f).normalized * _edgeScrollSpeed * Time.deltaTime;
				}
			}
			else
			{
				Vector2 zero = Vector2.zero;
				if (cursorPosition.x < vector2.x)
				{
					zero += Vector2.left;
				}
				else if (cursorPosition.x > (float)Screen.width - vector2.x)
				{
					zero += Vector2.right;
				}
				if (cursorPosition.y < vector2.y)
				{
					zero += Vector2.down;
				}
				else if (cursorPosition.y > (float)Screen.height - vector2.y)
				{
					zero += Vector2.up;
				}
				if (zero.sqrMagnitude > 0f)
				{
					zero = zero.normalized;
					moveInput = zero * _edgeScrollSpeed * Time.deltaTime;
				}
			}
		}
		goto IL_0256;
		IL_0256:
		if (_hasZoomInput && !_pointerIsOverUI)
		{
			zoomInput = ZoomAction.ReadValue<float>() * _zoomSpeed * Time.deltaTime;
		}
	}

	private void Update()
	{
		_pointerIsOverUI = EventSystem.current.IsPointerOverGameObject();
		if (!_isAutomated)
		{
			ReadInput(out var moveInput, out var zoomInput);
			if (zoomInput != 0f)
			{
				SetZoom(_currentZoom + zoomInput);
			}
			moveInput *= _zoomMovementSpeedModifier + (1f - _currentZoom) * (1f - _zoomMovementSpeedModifier);
			bool flag = (_smoothedCameraPosition - _targetCameraPosition).sqrMagnitude > 0.1f;
			_currentSpeed = Mathf.Lerp(_currentSpeed, flag ? 1f : 0f, _acceleration * Time.deltaTime);
			OffsetFocusPosition(moveInput.x, moveInput.y);
		}
	}

	private void LateUpdate()
	{
		if (_isAutomated)
		{
			if (_lockedOnTarget == null)
			{
				return;
			}
			SetFocusPosition(_lockedOnTarget.transform.position);
			_currentSpeed = 1f;
		}
		_smoothedCameraPosition = Vector3.Lerp(_smoothedCameraPosition, _targetCameraPosition, _currentSpeed * _maxSpeed * Time.deltaTime);
		_camera.transform.position = _smoothedCameraPosition;
	}

	private Vector3 FindCurrentlyFocusedPosition(out float distance)
	{
		return CastPositionOntoMapPlane(_camera.transform.position, out distance);
	}

	private Vector3 CastPositionOntoMapPlane(Vector3 position, out float distance)
	{
		Vector3 forward = _camera.transform.forward;
		_mapPlane.Raycast(new Ray(position, forward), out distance);
		return position + forward * distance;
	}

	private Vector3 CalculateCameraPositionForFocus(Vector3 focusPosition)
	{
		return focusPosition - _camera.transform.forward * _targetCameraDistance;
	}

	private void OffsetFocusPosition(float x, float y)
	{
		Vector3 vector = x * _mapRight + y * _mapUp;
		Vector3 focusPosition = _targetFocusPosition + vector;
		SetFocusPosition(focusPosition);
	}

	private void SetZoom(float zoom)
	{
		_currentZoom = Mathf.Clamp01(zoom);
		_targetCameraDistance = _minZoomDistance + (1f - _currentZoom) * (_maxZoomDistance - _minZoomDistance);
		OnZoom?.Invoke(_currentZoom);
	}

	private void SetCameraDistance(float cameraDistance)
	{
		_targetCameraDistance = cameraDistance;
		_currentZoom = (_targetCameraDistance - _minZoomDistance) / (_maxZoomDistance - _minZoomDistance);
	}

	private void SetFocusPosition(Vector3 focusPosition)
	{
		focusPosition = CastPositionOntoMapPlane(focusPosition, out var _);
		if (_clampToBounds && !_bounds.bounds.Contains(focusPosition))
		{
			focusPosition = _bounds.bounds.ClosestPoint(focusPosition);
		}
		_targetFocusPosition = focusPosition;
		_targetCameraPosition = CalculateCameraPositionForFocus(focusPosition);
	}

	private Tween GenerateFocusTween(Vector3 focusPosition, float duration, Ease ease)
	{
		SetFocusPosition(focusPosition);
		_smoothedCameraPosition = _targetCameraPosition;
		TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = _camera.transform.DOMove(_targetCameraPosition, duration).SetEase(ease).SetUpdate(UpdateType.Late)
			.SetRecyclable(recyclable: false);
		((Tween)tweenerCore).OnStart((TweenCallback)(() =>
		{
			_isAutomated = true;
		}));
		((Tween)tweenerCore).OnComplete((TweenCallback)(() =>
		{
			_isAutomated = false;
		}));
		return tweenerCore;
	}

	public void FocusOnTarget(Vector3 target, Ease ease, float duration)
	{
		StopRunningAutomation();
		_runningAutomation = GenerateFocusTween(target, duration, ease);
	}

	public void FocusOnTarget(Vector3 target)
	{
		FocusOnTarget(target, _autoMovementEase, _autoMovementDuration);
	}

	public void FocusOnTarget(Vector3 target, float zoom)
	{
		SetZoom(zoom);
		FocusOnTarget(target);
	}

	public void FocusOnTarget(Vector3 target, float zoom, Ease ease, float duration)
	{
		SetZoom(zoom);
		FocusOnTarget(target, ease, duration);
	}

	public void FocusOnTargetImmediately(Vector3 target)
	{
		StopRunningAutomation();
		SetFocusPosition(target);
		_smoothedCameraPosition = _targetCameraPosition;
		_camera.transform.position = _targetCameraPosition;
	}

	public void FocusAndLockOntoTarget(Transform target, float zoom)
	{
		StopRunningAutomation();
		_isAutomated = true;
		SetZoom(zoom);
		_runningAutomation = GenerateFocusTween(target.position, _autoMovementDuration, _autoMovementEase);
		_runningAutomation.OnComplete(() =>
		{
			LockOntoTarget(target);
		});
	}

	public void LockOntoTarget(Transform target)
	{
		StopRunningAutomation();
		_lockedOnTarget = target;
		_isAutomated = true;
	}

	public void LockOntoTarget(Transform target, float zoom)
	{
		SetZoom(zoom);
		LockOntoTarget(target);
	}

	public void StopRunningAutomation()
	{
		_runningAutomation?.Kill();
		_lockedOnTarget = null;
		_isAutomated = false;
	}

	[ContextMenu("Reset Focus")]
	public void ResetFocus()
	{
		SetCameraDistance(_initialCameraDistance);
		FocusOnTarget(_initialFocusPosition);
	}

	public void ResetFocusImmediately()
	{
		StopRunningAutomation();
		SetCameraDistance(_initialCameraDistance);
		SetFocusPosition(_initialFocusPosition);
		_smoothedCameraPosition = _targetCameraPosition;
		_camera.transform.position = _targetCameraPosition;
	}
}
