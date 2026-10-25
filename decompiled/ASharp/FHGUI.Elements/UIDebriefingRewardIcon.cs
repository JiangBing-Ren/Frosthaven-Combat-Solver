using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIDebriefingRewardIcon : MonoBehaviour
{
	public struct Params
	{
		public Sprite Icon;

		public string Value;
	}

	[Header("Components")]
	[SerializeField]
	public Image Icon;

	[SerializeField]
	public TMP_Text Value;

	public void Init(Params @params)
	{
		if ((bool)Icon)
		{
			Icon.sprite = @params.Icon;
		}
		if ((bool)Value)
		{
			Value.text = @params.Value;
		}
	}
}
