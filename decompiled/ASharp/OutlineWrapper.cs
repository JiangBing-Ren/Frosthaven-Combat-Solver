using EPOOutline;
using ScenarioRuleLibrary;
using UnityEngine;

[RequireComponent(typeof(Outlinable))]
public class OutlineWrapper : MonoBehaviour
{
	public bool IsObject = true;

	private CActor m_ActorAttachedToObject;

	[ConditionalField("IsObject", null, true)]
	public ScenarioManager.ObjectImportType ObjectImportType = ScenarioManager.ObjectImportType.None;

	private Outlinable m_Outlinable;

	private bool m_LastRevealState = true;

	private void Awake()
	{
		Outlinable[] components = GetComponents<Outlinable>();
		for (int i = 0; i < components.Length; i++)
		{
			if (!OutlinableIsObstacle(components[i]))
			{
				m_Outlinable = components[i];
				break;
			}
		}
	}

	private void OnEnable()
	{
		if (m_Outlinable == null || !WorldspaceUITools.Instance)
		{
			return;
		}
		if (IsObject)
		{
			if (m_ActorAttachedToObject == null)
			{
				m_ActorAttachedToObject = GetComponentInParent<UnityGameEditorObject>()?.PropObject?.RuntimeAttachedActor;
			}
			if (m_ActorAttachedToObject == null)
			{
				m_Outlinable.OutlineParameters.Color = UIInfoTools.GetPropOutlineColor(ObjectImportType);
				WorldspaceUITools.Instance.AddOutlinableToList(m_Outlinable);
				WorldspaceUITools.Instance.SetRevealState(m_Outlinable, m_LastRevealState);
			}
			else
			{
				m_Outlinable.OutlineParameters.Color = UIInfoTools.GetCharacterOutlineColor(m_ActorAttachedToObject.OriginalType);
				WorldspaceUITools.Instance.AddOutlinableToList(m_Outlinable, m_ActorAttachedToObject);
				WorldspaceUITools.Instance.SetRevealState(m_Outlinable, m_LastRevealState);
			}
		}
		else
		{
			WorldspaceUITools.Instance.AddOutlinableToList(m_Outlinable);
			WorldspaceUITools.Instance.SetRevealState(m_Outlinable, m_LastRevealState);
		}
	}

	private void OnDisable()
	{
		if (m_Outlinable != null && (bool)WorldspaceUITools.Instance)
		{
			WorldspaceUITools.Instance.RemoveOutlinableFromList(m_Outlinable);
		}
	}

	private void OnDestroy()
	{
		if ((object)m_Outlinable != null && (bool)WorldspaceUITools.Instance)
		{
			WorldspaceUITools.Instance.RemoveOutlinableFromList(m_Outlinable);
		}
	}

	public void Refresh()
	{
		OnDisable();
		OnEnable();
	}

	public void SetRevealState(bool isRevealed)
	{
		m_LastRevealState = isRevealed;
		WorldspaceUITools.Instance?.SetRevealState(m_Outlinable, isRevealed);
	}

	public static bool OutlinableIsObstacle(Outlinable outlinable)
	{
		return outlinable.OutlineLayer == 1;
	}
}
