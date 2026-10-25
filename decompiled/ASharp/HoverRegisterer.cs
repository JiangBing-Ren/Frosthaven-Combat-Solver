using System.Collections.Generic;
using UnityEngine;

public class HoverRegisterer : MonoBehaviour
{
	[SerializeField]
	private LayerMask targetLayer;

	private Camera m_Camera;

	private List<IHoverable> m_CursorTargets;

	private List<IHoverable> m_CurrentRaycastTargets;

	private void Awake()
	{
		m_Camera = Camera.main;
		m_CursorTargets = new List<IHoverable>();
		m_CurrentRaycastTargets = new List<IHoverable>();
	}

	private void FixedUpdate()
	{
		if (m_Camera == null)
		{
			return;
		}
		if (!m_Camera.gameObject.activeSelf)
		{
			m_Camera = Camera.main;
		}
		Vector2 cursorPosition = InputFHManager.CursorPosition;
		if ((Singleton<InputFHManager>.InstanceFast == null || !Singleton<InputFHManager>.InstanceFast.UsingGamepad) && (cursorPosition.x < 0f || cursorPosition.y < 0f || cursorPosition.x > (float)Screen.width || cursorPosition.y > (float)Screen.height))
		{
			return;
		}
		if (!UIManager.IsPointerOverUI)
		{
			Ray ray = m_Camera.ScreenPointToRay(cursorPosition);
			RaycastHit hitInfo = default;
			if (Physics.Raycast(ray, out hitInfo, 1000f, targetLayer))
			{
				m_CurrentRaycastTargets.Clear();
				m_CurrentRaycastTargets.AddRange(hitInfo.transform.gameObject.GetComponents<IHoverable>());
				for (int num = m_CursorTargets.Count - 1; num >= 0; num--)
				{
					IHoverable hoverable = m_CursorTargets[num];
					if (!m_CurrentRaycastTargets.Contains(hoverable))
					{
						hoverable?.OnCursorExit();
						m_CursorTargets.Remove(hoverable);
					}
				}
				if (m_CurrentRaycastTargets.Count <= 0)
				{
					return;
				}
				{
					foreach (IHoverable currentRaycastTarget in m_CurrentRaycastTargets)
					{
						if (currentRaycastTarget != null && !m_CursorTargets.Contains(currentRaycastTarget))
						{
							m_CursorTargets.Add(currentRaycastTarget);
							currentRaycastTarget.OnCursorEnter();
						}
					}
					return;
				}
			}
			if (m_CursorTargets.Count <= 0)
			{
				return;
			}
			foreach (IHoverable cursorTarget in m_CursorTargets)
			{
				if (cursorTarget != null)
				{
					cursorTarget?.OnCursorExit();
				}
			}
			m_CursorTargets.Clear();
		}
		else
		{
			if (m_CursorTargets.Count <= 0)
			{
				return;
			}
			foreach (IHoverable cursorTarget2 in m_CursorTargets)
			{
				if (cursorTarget2 != null)
				{
					cursorTarget2?.OnCursorExit();
				}
			}
			m_CursorTargets.Clear();
		}
	}
}
