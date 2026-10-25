using Apparance.Net;
using UnityEngine;

public class ProceduralLabel : MonoBehaviour, IPlacementParameters
{
	public string text = "Hello World";

	public Color colour = Color.yellow;

	public void ApplyParameters(ParameterCollection parameters)
	{
		if (parameters.Count >= 2)
		{
			Parameter parameterAt = parameters.GetParameterAt(1);
			if (parameterAt != null && parameterAt.Type == '$')
			{
				text = parameterAt.Value as string;
			}
			Parameter parameterAt2 = parameters.GetParameterAt(2);
			if (parameterAt2 != null && parameterAt2.Type == 'C')
			{
				Colour c = (Colour)parameterAt2.Value;
				colour = Conversion.UCfromAC(c);
			}
		}
		gameObject.hideFlags &= ~HideFlags.HideInHierarchy;
	}
}
