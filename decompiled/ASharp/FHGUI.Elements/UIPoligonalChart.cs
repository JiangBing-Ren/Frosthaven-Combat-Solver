using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIPoligonalChart : MaskableGraphic
{
	public class PolygonalData
	{
		public float Value;

		public float MaxValue;

		public string Text;

		public Vector2 LabelOffset;

		public float GetNormalized(float max, uint zeroOffset = 1u)
		{
			if (max == 0f)
			{
				return 0f;
			}
			Value = Mathf.Clamp(Value, 0f, max);
			return (Value + (float)zeroOffset) / max;
		}
	}

	[Header("Base Elements")]
	[SerializeField]
	private Texture m_Texture;

	[SerializeField]
	private TMP_Text m_LabelPrototype;

	[SerializeField]
	private RectTransform m_LabelContainer;

	[SerializeField]
	private GameObject m_ValueMarkPrototype;

	[SerializeField]
	private RectTransform m_ValueMarkContainer;

	[Header("Settings")]
	[SerializeField]
	private float m_Radius = 100f;

	[SerializeField]
	private float m_StartAngleDegree;

	[SerializeField]
	private float m_LabelOffset;

	private List<PolygonalData> m_Points;

	private List<TMP_Text> m_Labels = new List<TMP_Text>();

	private List<GameObject> m_ValueMarks = new List<GameObject>();

	private uint m_ZeroOffset;

	private bool m_Initialized;

	private bool m_InvalidateLabels;

	public override Texture mainTexture
	{
		get
		{
			if (m_Texture != null)
			{
				return m_Texture;
			}
			if (material != null && material.mainTexture != null)
			{
				return material.mainTexture;
			}
			return Graphic.s_WhiteTexture;
		}
	}

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_Labels.Clear();
			if ((object)m_LabelPrototype != null)
			{
				m_LabelPrototype.gameObject.SetActive(value: false);
			}
			if ((object)m_ValueMarkPrototype != null)
			{
				m_ValueMarkPrototype.gameObject.SetActive(value: false);
			}
			useLegacyMeshGeneration = false;
		}
	}

	public void Init(List<PolygonalData> points, uint offsetZeroBy = 1u)
	{
		InitCache();
		m_Points = points;
		m_ZeroOffset = offsetZeroBy;
		DrawMarks();
		InvalidateLabels();
		SetVerticesDirty();
	}

	private float GetMaxValue()
	{
		float num = 0f;
		for (int i = 0; i < m_Points.Count; i++)
		{
			float num2 = m_Points[i].MaxValue + (float)m_ZeroOffset;
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		if (m_Points == null || m_Points.Count < 1)
		{
			return;
		}
		float maxValue = GetMaxValue();
		int count = m_Points.Count;
		float num = (90f - m_StartAngleDegree) / 180f * MathF.PI;
		float num2 = MathF.PI * -2f / (float)count;
		Vector3[] array = new Vector3[count + 1];
		Vector2[] array2 = new Vector2[array.Length];
		int[] array3 = new int[3 * count];
		array[0] = new Vector3(0f, 0f, 0f);
		array2[0] = new Vector2(0f, 0f);
		for (int i = 0; i < count; i++)
		{
			float f = num + (float)i * num2;
			float num3 = m_Radius * m_Points[i].GetNormalized(maxValue, m_ZeroOffset);
			array[i + 1] = new Vector3(Mathf.Cos(f) * num3, Mathf.Sin(f) * num3, 0f);
			bool flag = i >= count - 1;
			if (!flag)
			{
				f = num + (float)(i + 1) * num2;
				num3 = m_Radius * m_Points[i + 1].GetNormalized(maxValue, m_ZeroOffset);
				array[i + 2] = new Vector3(Mathf.Cos(f) * num3, Mathf.Sin(f) * num3, 0f);
			}
			array3[i * 3] = 0;
			array3[i * 3 + 1] = i + 1;
			array3[i * 3 + 2] = (flag ? 1 : (i + 2));
			array2[i + 1] = new Vector2(1f, 1f);
		}
		for (int j = 0; j < array.Length; j++)
		{
			vh.AddVert(array[j], color, array2[j]);
		}
		for (int k = 0; k < count; k++)
		{
			int num4 = k * 3;
			vh.AddTriangle(array3[num4], array3[num4 + 1], array3[num4 + 2]);
		}
	}

	private void DrawMarks()
	{
		if (m_ValueMarkPrototype == null || m_ValueMarkContainer == null || m_Points.Count == 0)
		{
			return;
		}
		float num = (90f - m_StartAngleDegree) / 180f * MathF.PI;
		float num2 = MathF.PI * -2f / (float)m_Points.Count;
		float maxValue = GetMaxValue();
		HelperTools.NormalizePool(ref m_ValueMarks, m_ValueMarkPrototype, m_ValueMarkContainer, m_Points.Count);
		for (int i = 0; i < m_ValueMarks.Count; i++)
		{
			GameObject gameObject = ((m_ValueMarks.Count > i) ? m_ValueMarks[i] : null);
			GameObject gameObject2 = m_ValueMarks[i];
			gameObject2.gameObject.SetActive(gameObject != null);
			if (!(gameObject == null))
			{
				float f = num + (float)i * num2;
				float num3 = m_Radius * m_Points[i].GetNormalized(maxValue);
				Vector3 localPosition = new Vector3(Mathf.Cos(f) * num3, Mathf.Sin(f) * num3, 0f);
				gameObject2.transform.localPosition = localPosition;
			}
		}
	}

	private void InvalidateLabels()
	{
		m_InvalidateLabels = true;
	}

	private void LateUpdate()
	{
		if (m_InvalidateLabels)
		{
			m_InvalidateLabels = false;
			UpdateLabelsPosition();
		}
	}

	private void UpdateLabelsPosition()
	{
		if (m_Points == null || m_Points.Count == 0)
		{
			return;
		}
		float num = (90f - m_StartAngleDegree) / 180f * MathF.PI;
		float num2 = MathF.PI * -2f / (float)m_Points.Count;
		HelperTools.NormalizePool(ref m_Labels, m_LabelPrototype.gameObject, m_LabelContainer, m_Points.Count());
		for (int i = 0; i < m_Labels.Count; i++)
		{
			PolygonalData polygonalData = ((m_Points.Count > i) ? m_Points[i] : null);
			TMP_Text tMP_Text = m_Labels[i];
			tMP_Text.gameObject.SetActive(polygonalData != null);
			if (polygonalData != null)
			{
				tMP_Text.text = polygonalData.Text;
				tMP_Text.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
				tMP_Text.rectTransform.sizeDelta = tMP_Text.GetRenderedValues(onlyVisibleCharacters: true);
				float f = num + (float)i * num2;
				Vector3 vector = new Vector3(Mathf.Cos(f) * (m_Radius + m_LabelOffset), Mathf.Sin(f) * (m_Radius + m_LabelOffset), 0f);
				vector.x += m_Points[i].LabelOffset.x;
				vector.y += m_Points[i].LabelOffset.y;
				((RectTransform)tMP_Text.transform).pivot = GetLabelAlignmentPivot(vector);
				tMP_Text.transform.localPosition = vector;
			}
		}
	}

	private Vector2 GetLabelAlignmentPivot(Vector3 point, float offsetNormalizedX = 0.15f, float offsetNormalizedY = 0.3f)
	{
		if (IsEqual(point.x, 0f, 0.01f) && point.y > 0f)
		{
			return new Vector2(0.5f, 0f - offsetNormalizedY);
		}
		if (IsEqual(point.x, 0f, 0.01f) && point.y < 0f)
		{
			return new Vector2(0.5f, 1f + offsetNormalizedY);
		}
		if (point.x > 0f && point.y != 0f)
		{
			return new Vector2(0f - offsetNormalizedX, 0.5f);
		}
		if (point.x < 0f && point.y != 0f)
		{
			return new Vector2(1f + offsetNormalizedX, 0.5f);
		}
		return new Vector2(0.5f, 0.5f);
	}

	private bool IsEqual(float a, float b, float precision)
	{
		if (a >= b - precision)
		{
			return a <= b + precision;
		}
		return false;
	}
}
