using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ImageMaterialSwapper : MonoBehaviour
{
	public Material TargetMaterial;

	[Tooltip("If enabled it'll grab the child images of this game object on awake/trigger")]
	public bool UseChildImages;

	public Image[] Images;

	private bool _gotChildren;

	private readonly Dictionary<Image, Material> _imageMaterialPreSwap = new Dictionary<Image, Material>();

	public void SwapToTargetMaterial()
	{
		if (UseChildImages && !_gotChildren)
		{
			Images = (from i in GetComponentsInChildren<Image>()
				where !i.TryGetComponent<Mask>(out var _)
				select i).ToArray();
			_gotChildren = true;
		}
		Image[] images = Images;
		foreach (Image image in images)
		{
			_imageMaterialPreSwap.TryAdd(image, image.material);
			image.material = TargetMaterial;
		}
	}

	public void ResetToPreSwap()
	{
		foreach (Image key in _imageMaterialPreSwap.Keys)
		{
			key.material = _imageMaterialPreSwap[key];
		}
		_imageMaterialPreSwap.Clear();
	}
}
