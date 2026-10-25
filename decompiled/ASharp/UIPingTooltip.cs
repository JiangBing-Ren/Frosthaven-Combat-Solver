using TMPro;
using UnityEngine;

public class UIPingTooltip : MonoBehaviour
{
	public enum ScaleType
	{
		Scale,
		Size
	}

	public float m_GUIScreenSpaceOffsetX;

	public float m_GUIScreenSpaceOffsetY;

	public Vector3 m_WorldspaceOffset;

	public bool DontScaleUI;

	public ScaleType m_ScaleType;

	public bool m_ScalingWidth = true;

	public bool m_ScalingHeight = true;

	public RectTransform m_ScaleRectTransform;

	public AnimationCurve m_ScalingCurve;

	[SerializeField]
	private GUIAnimator showAnimator;

	[SerializeField]
	private string audioItemShow;

	[SerializeField]
	private GUIAnimator hideAnimator;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	protected GameObject m_ObjectToTrack;

	private RectTransform m_CanvasRect;

	private void Awake()
	{
		hideAnimator.OnAnimationFinished.AddListener(() =>
		{
			gameObject.SetActive(value: false);
		});
	}

	public void Show(string description, GameObject objectToTrack, RectTransform canvasRect)
	{
		m_ObjectToTrack = objectToTrack;
		m_CanvasRect = canvasRect;
		descriptionText.text = description;
		hideAnimator.Stop();
		showAnimator.Stop();
		gameObject.SetActive(value: true);
		showAnimator.Play();
		AudioControllerUtils.PlaySound(audioItemShow);
	}

	public void Hide(bool instant = false)
	{
		if (gameObject.activeSelf)
		{
			showAnimator.Stop();
			if (instant)
			{
				hideAnimator.Stop();
				gameObject.SetActive(value: false);
			}
			else
			{
				hideAnimator.Play();
			}
		}
	}

	private void OnDestroy()
	{
		hideAnimator.Stop();
		showAnimator.Stop();
	}

	private void LateUpdate()
	{
		if (!(m_ObjectToTrack != null))
		{
			return;
		}
		TrackPosition();
		if (!DontScaleUI)
		{
			float num = m_ScalingCurve.Evaluate(1f - (CameraController.s_CameraController.Zoom - CameraController.s_CameraController.m_MinimumFOV) / (75f - CameraController.s_CameraController.m_MinimumFOV));
			Vector2 vector = new Vector2(m_ScalingWidth ? num : 1f, m_ScalingHeight ? num : 1f);
			switch (m_ScaleType)
			{
			case ScaleType.Scale:
				m_ScaleRectTransform.localScale = vector;
				break;
			case ScaleType.Size:
				m_ScaleRectTransform.sizeDelta = vector;
				break;
			}
		}
	}

	private void TrackPosition()
	{
		if (!(WorldspaceUITools.Instance.WorldspaceCamera == null))
		{
			Vector3 position = m_ObjectToTrack.transform.position;
			position += m_WorldspaceOffset;
			Vector2 screenPoint = WorldspaceUITools.Instance.WorldspaceCamera.WorldToScreenPoint(position);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_CanvasRect, screenPoint, WorldspaceUITools.Instance.WorldspaceCanvas.worldCamera, out var localPoint);
			transform.localPosition = localPoint + new Vector2(m_GUIScreenSpaceOffsetX, m_GUIScreenSpaceOffsetY);
		}
	}
}
