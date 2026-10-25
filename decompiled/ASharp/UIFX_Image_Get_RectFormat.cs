using SETUtil;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIFX_Image_Get_RectFormat : MonoBehaviour
{
	public string shaderProperty = "_RectFormat";

	private Vector2 rectFormat;

	public bool adjust;

	private RectTransform myRect;

	[Header("UIElements")]
	public Image ThisImage;

	private Material imageMaterialCopy;

	private void Awake()
	{
		imageMaterialCopy = new Material(ThisImage.material);
		ThisImage.material = imageMaterialCopy;
	}

	private void OnDestroy()
	{
		SceneUtil.SmartDestroy(imageMaterialCopy);
	}

	[ContextMenu("Manually adjust")]
	public void doAdjust()
	{
		rectFormat = ThisImage.rectTransform.sizeDelta;
		imageMaterialCopy.SetVector(shaderProperty, rectFormat);
	}
}
