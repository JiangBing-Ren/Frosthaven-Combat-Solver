using System.Collections.Generic;
using SETUtil.Common.Extend;
using UnityEngine;
using UnityEngine.UI;

public class RandomBackgroundPicker : MonoBehaviour
{
	public List<Sprite> backgrounds;

	public Image image;

	private void OnEnable()
	{
		image.sprite = backgrounds.SelectRandom();
	}
}
