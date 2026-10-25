using ScenarioRuleLibrary;
using UnityEngine;

public class PropFacingRotator : MonoBehaviour
{
	private CObjectProp prop;

	public float rotationSpeed = 1f;

	private void Update()
	{
		if (prop == null)
		{
			UnityGameEditorObject componentInParent = GetComponentInParent<UnityGameEditorObject>();
			if (componentInParent != null)
			{
				prop = componentInParent.PropObject;
			}
			if (prop == null)
			{
				return;
			}
		}
		CObjectProp cObjectProp = prop;
		if (cObjectProp == null)
		{
			return;
		}
		_ = cObjectProp.VisualsForceLookAt;
		if (1 == 0)
		{
			return;
		}
		GameObject gameObject = base.gameObject;
		CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[prop.VisualsForceLookAt.X, prop.VisualsForceLookAt.Y];
		if (gameObject != null && cClientTile != null && cClientTile.m_GameObject != null)
		{
			Vector3 forward = cClientTile.m_GameObject.transform.position - transform.position;
			forward.y = 0f;
			if (!(forward.sqrMagnitude < 0.0001f))
			{
				Quaternion b = Quaternion.LookRotation(forward);
				transform.rotation = Quaternion.Slerp(transform.rotation, b, rotationSpeed * Time.deltaTime);
			}
		}
	}
}
