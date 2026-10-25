using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class InteractabilityHighlightRect : MonoBehaviour
{
	public RectTransform RectToStretch;

	public CanvasGroup DisplayGroup;

	public UIFX_Image_Get_RectFormat FxRectAdjuster;

	private RectTransform m_ParentCanvasRect;

	public RectTransform m_RectTransformToFollow;

	private IEnumerator m_RectFollowRoutine;

	private IEnumerator m_CanvasFadeRoutine;

	private Image m_Image;

	private InteractabilityIsolatedUIControl m_InteractabilityControl;

	private Vector3[] m_ScreenSpaceCorners = new Vector3[4];

	private Vector4 m_Padding;

	private CanvasRenderer m_RectToFollowRenderer;

	private void Awake()
	{
		m_Image = GetComponent<Image>();
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}

	public void SetInetractabilityControl(InteractabilityIsolatedUIControl control)
	{
		m_InteractabilityControl = control;
	}

	public void SetRectToFollow(RectTransform rectToFollow, RectTransform parentCanvasRect, Vector4 padding = default(Vector4))
	{
		m_ParentCanvasRect = parentCanvasRect;
		m_RectTransformToFollow = rectToFollow;
		m_RectToFollowRenderer = rectToFollow?.GetComponentInChildren<CanvasRenderer>();
		m_Padding = padding;
		if (m_RectFollowRoutine != null)
		{
			StopCoroutine(m_RectFollowRoutine);
			m_RectFollowRoutine = null;
		}
		m_RectFollowRoutine = ContinuouslyRefitToFollowRect();
		StartCoroutine(m_RectFollowRoutine);
		SetShowing(shouldShow: true);
	}

	public void SetPulse(bool instant)
	{
		m_Image.material.SetFloat("_PulseSpeed", (!instant) ? 3 : 0);
		m_Image.material.SetFloat("_AngularHighlightIntensity", instant ? 0f : 1.5f);
	}

	public void SetShowing(bool shouldShow)
	{
		if (m_CanvasFadeRoutine != null)
		{
			StopCoroutine(m_CanvasFadeRoutine);
			m_CanvasFadeRoutine = null;
		}
		if (!gameObject.activeInHierarchy)
		{
			Object.Destroy(gameObject);
			return;
		}
		m_CanvasFadeRoutine = GloomUtility.FadeCanvasGroup(DisplayGroup, 0f, 0.3f, shouldShow ? 1f : 0f, AnimationCurve.Linear(0f, 0f, 1f, 1f), null, () =>
		{
			if (!shouldShow)
			{
				Object.Destroy(gameObject);
			}
		});
		StartCoroutine(m_CanvasFadeRoutine);
	}

	private IEnumerator ContinuouslyRefitToFollowRect()
	{
		while (true)
		{
			bool visible = m_RectTransformToFollow != null && m_RectTransformToFollow.gameObject.activeInHierarchy;
			if (m_RectToFollowRenderer != null && m_RectToFollowRenderer.cull)
			{
				visible = false;
			}
			if (visible && m_InteractabilityControl?.ValidateHighlight != null)
			{
				visible &= m_InteractabilityControl.ValidateHighlight();
			}
			if (visible)
			{
				Rect rect = RectTransformToScreenSpace(m_RectTransformToFollow);
				m_ScreenSpaceCorners[0] = m_ParentCanvasRect.InverseTransformPoint(rect.center + new Vector2((0f - rect.width) / 2f, (0f - rect.height) / 2f));
				m_ScreenSpaceCorners[0] += new Vector3(m_Padding.x, m_Padding.w, 0f);
				m_ScreenSpaceCorners[1] = m_ParentCanvasRect.InverseTransformPoint(rect.center + new Vector2((0f - rect.width) / 2f, rect.height / 2f));
				m_ScreenSpaceCorners[1] += new Vector3(m_Padding.x, 0f - m_Padding.z, 0f);
				m_ScreenSpaceCorners[2] = m_ParentCanvasRect.InverseTransformPoint(rect.center + new Vector2(rect.width / 2f, rect.height / 2f));
				m_ScreenSpaceCorners[2] += new Vector3(0f - m_Padding.y, 0f - m_Padding.z, 0f);
				m_ScreenSpaceCorners[3] = m_ParentCanvasRect.InverseTransformPoint(rect.center + new Vector2(rect.width / 2f, (0f - rect.height) / 2f));
				m_ScreenSpaceCorners[3] += new Vector3(0f - m_Padding.y, m_Padding.w, 0f);
				RectToStretch.anchoredPosition = m_ScreenSpaceCorners[1];
				float x = m_ScreenSpaceCorners[2].x - m_ScreenSpaceCorners[1].x;
				float y = m_ScreenSpaceCorners[1].y - m_ScreenSpaceCorners[0].y;
				RectToStretch.sizeDelta = new Vector2(x, y);
				FxRectAdjuster.doAdjust();
			}
			if (m_Image != null && m_Image.enabled != visible)
			{
				yield return default(TimeUpdate.WaitForLastPresentationAndUpdateTime);
			}
			if (m_Image != null)
			{
				m_Image.enabled = visible;
			}
			yield return null;
		}
	}

	public Rect RectTransformToScreenSpace(RectTransform rectTransform)
	{
		Vector2 vector = Vector2.Scale(rectTransform.rect.size, rectTransform.lossyScale);
		return new Rect((Vector2)rectTransform.position - vector * rectTransform.pivot, vector);
	}
}
