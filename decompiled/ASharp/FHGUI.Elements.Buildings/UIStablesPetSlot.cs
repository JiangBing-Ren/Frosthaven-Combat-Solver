using FHGUI.DTO;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements.Buildings;

public class UIStablesPetSlot : MonoBehaviour
{
	public struct Params
	{
		public PetDTO Pet;

		public bool CanUsePet;

		public uint RequiredStablesLevel;

		public bool Locked;
	}

	[Header("Components")]
	[SerializeField]
	private Image Background;

	[SerializeField]
	private Image Icon;

	[SerializeField]
	private Image Empty;

	[SerializeField]
	private UIBuildingLockRequirement Lock;

	[SerializeField]
	private UITextTooltipTarget Tooltip;

	[Header("Materials")]
	[SerializeField]
	private Material DesaturateMaterial;

	public void Init(Params @params, uint currentLevel)
	{
		bool flag = @params.Pet.Def == null;
		if (!flag)
		{
			Icon.sprite = @params.Pet.Icon;
		}
		Icon.enabled = !flag;
		Empty.enabled = flag;
		Background.enabled = !@params.Locked;
		if (@params.Locked)
		{
			Lock.Show(@params.RequiredStablesLevel, currentLevel);
		}
		else
		{
			Lock.Hide();
		}
		Icon.material = ((!flag && !@params.CanUsePet) ? DesaturateMaterial : null);
		Tooltip.SetText((!flag) ? FHUILocFormatting.FormatSimpleTooltip(@params.Pet.NameLocKey, @params.Pet.EffectLocKey) : string.Empty);
	}
}
