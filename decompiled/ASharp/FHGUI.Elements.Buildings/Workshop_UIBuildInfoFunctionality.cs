using FHGUI.DTO;
using GLOOM;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements.Buildings;

public class Workshop_UIBuildInfoFunctionality : MonoBehaviour, IBuildInfoFunctionality<WorkshopBuildingParams>, IBuildInfoFunctionality
{
	[Header("Texts")]
	[SerializeField]
	private TextLocalizedListener _description;

	[Header("Elements")]
	[SerializeField]
	private Image _sled;

	[SerializeField]
	private Image _boat;

	[SerializeField]
	private Image _climbingGear;

	[Header("Materials")]
	[SerializeField]
	private Material GrayscaleMaterial;

	public void Init(BuildingStateDTO building, WorkshopBuildingParams data)
	{
		gameObject.SetActive(value: true);
		string text = LocalizationManager.GetTranslation(BuildInfoUtils.GetDescription(building));
		if (building.IsWrecked)
		{
			text = $"<style=\"Wrecked\">{text}</style>";
		}
		_description.Text.text = text;
		_sled.material = (data.HasSled ? null : GrayscaleMaterial);
		_boat.material = (data.HasBoat ? null : GrayscaleMaterial);
		_climbingGear.material = (data.HasClimbingGear ? null : GrayscaleMaterial);
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}
