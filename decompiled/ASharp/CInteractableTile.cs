using UnityEngine;

public class CInteractableTile : CInteractable
{
	private TileBehaviour m_TileBehaviour;

	public override void ShowNormalInterface(bool disabled, EInputType inputType)
	{
		if (m_TileBehaviour == null)
		{
			TileBehaviour component = GetComponent<TileBehaviour>();
			if (component.m_ClientTile == null)
			{
				Object.Destroy(this);
				return;
			}
			m_TileBehaviour = component;
		}
		if (TileBehaviour.s_Callback != null && m_TileBehaviour != null)
		{
			TileBehaviour.s_Callback(m_TileBehaviour.m_ClientTile, null, networkActionIfOnline: true, isUserClick: true, SaveData.Instance.Global.EnableSecondClickHexToConfirm, inputType);
		}
	}
}
