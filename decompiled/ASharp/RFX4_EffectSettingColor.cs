using UnityEngine;

public class RFX4_EffectSettingColor : MonoBehaviour
{
	public Color Color = Color.red;

	private Color previousColor;

	private void OnEnable()
	{
		UpdateColor();
	}

	private void Update()
	{
		if (previousColor != Color)
		{
			UpdateColor();
		}
	}

	private void UpdateColor()
	{
		RFX4_TransformMotion componentInChildren = GetComponentInChildren<RFX4_TransformMotion>(includeInactive: true);
		RFX4_RaycastCollision componentInChildren2 = GetComponentInChildren<RFX4_RaycastCollision>(includeInactive: true);
		float h = RFX4_ColorHelper.ColorToHSV(Color).H;
		RFX4_ColorHelper.ChangeObjectColorByHUE(gameObject, h);
		if (componentInChildren != null)
		{
			componentInChildren.HUE = h;
		}
		if (componentInChildren2 != null)
		{
			componentInChildren2.HUE = h;
		}
		previousColor = Color;
	}
}
