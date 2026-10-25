using System.Text;
using Assets.Script.GUI.MainMenu.Modding;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIModSlot : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI nameText;

	[SerializeField]
	protected RawImage thumbnailImage;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	private const string RATING_FORMAT = "<sprite name=\"Star\" color=#{0}><sprite name=\"Star\" color=#{1}><sprite name=\"Star\" color=#{2}><sprite name=\"Star\" color=#{3}><sprite name=\"Star\" color=#{4}>";

	protected IMod modData;

	public virtual void SetMod(IMod modData)
	{
		this.modData = modData;
		nameText.text = modData.Name;
		RefreshRanking();
	}

	private string BuildRating(int rating)
	{
		string text = UIInfoTools.DefInstance.greyedOutTextColor.ToHex();
		string text2 = UIInfoTools.Instance.goldColor.ToHex();
		return $"<sprite name=\"Star\" color=#{((rating >= 1) ? text2 : text)}><sprite name=\"Star\" color=#{((rating >= 2) ? text2 : text)}><sprite name=\"Star\" color=#{((rating >= 3) ? text2 : text)}><sprite name=\"Star\" color=#{((rating >= 4) ? text2 : text)}><sprite name=\"Star\" color=#{((rating >= 5) ? text2 : text)}>";
	}

	public void RefreshRanking()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("<color=#{0}>{1}</color>\n", UIInfoTools.DefInstance.basicTextColor.ToHex(), modData.Name);
		if (modData.Rating >= 0)
		{
			stringBuilder.AppendLine(BuildRating(modData.Rating));
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine(LocalizationManager.GetTranslation(modData.IsCustomMod ? "GUI_MODDING_CUSTOM_MOD" : "GUI_MODDING_STEAM_MOD"));
		stringBuilder.Append(modData.Description);
		tooltip.SetText(stringBuilder.ToString());
	}
}
