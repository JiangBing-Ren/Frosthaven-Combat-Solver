using UnityEngine;

public class RFX4_ShaderFloatCurve : MonoBehaviour
{
	public RFX4_ShaderProperties ShaderFloatProperty = RFX4_ShaderProperties._Cutout;

	public AnimationCurve FloatCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float GraphTimeMultiplier = 1f;

	public float GraphIntensityMultiplier = 1f;

	public bool IsLoop;

	private bool canUpdate;

	private float startTime;

	private int propertyID;

	private string shaderProperty;

	private MaterialPropertyBlock props;

	private Renderer rend;

	private void Awake()
	{
		if (props == null)
		{
			props = new MaterialPropertyBlock();
		}
		if (rend == null)
		{
			rend = GetComponent<Renderer>();
		}
		if (!rend)
		{
			Debug.LogError($"Visual Effect {name} in {transform.parent} is mising a Renderer component.");
		}
		shaderProperty = ShaderFloatProperty.ToString();
		propertyID = Shader.PropertyToID(shaderProperty);
	}

	private void OnEnable()
	{
		if ((bool)rend)
		{
			startTime = Time.time;
			canUpdate = true;
			rend.GetPropertyBlock(props);
			float value = FloatCurve.Evaluate(0f) * GraphIntensityMultiplier;
			props.SetFloat(propertyID, value);
			rend.SetPropertyBlock(props);
		}
	}

	private void Update()
	{
		if (!rend)
		{
			return;
		}
		rend.GetPropertyBlock(props);
		float num = Time.time - startTime;
		if (canUpdate)
		{
			float value = FloatCurve.Evaluate(num / GraphTimeMultiplier) * GraphIntensityMultiplier;
			props.SetFloat(propertyID, value);
		}
		if (num >= GraphTimeMultiplier)
		{
			if (IsLoop)
			{
				startTime = Time.time;
			}
			else
			{
				canUpdate = false;
			}
		}
		rend.SetPropertyBlock(props);
	}
}
