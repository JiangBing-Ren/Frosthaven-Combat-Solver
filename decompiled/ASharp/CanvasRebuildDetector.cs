using UnityEngine;
using UnityEngine.UI;

public class CanvasRebuildDetector : Image, ICanvasElement
{
	public override void Rebuild(CanvasUpdate executing)
	{
		Debug.Log("Rebuild! - " + gameObject.name);
	}

	public override void LayoutComplete()
	{
		Debug.Log("LayoutComplete! - " + gameObject.name);
	}

	public override void GraphicUpdateComplete()
	{
		Debug.Log("GraphicUpdateComplete! - " + gameObject.name);
	}
}
