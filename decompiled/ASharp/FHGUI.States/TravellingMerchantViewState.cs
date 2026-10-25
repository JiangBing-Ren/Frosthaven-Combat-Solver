using FHGUI.DTO;
using FHGUI.Modules;

namespace FHGUI.States;

public class TravellingMerchantViewState : ShopViewState
{
	public const string GUI_TRAVELLING_MERCHANT_ICON = "FH_UI_MENU_QuickAccessBar_Outpost_ICO_TravelingMerc_Selected";

	protected override MainViewSubCategories SubCategory => MainViewSubCategories.TravellingMerchant;

	protected override ShopViewModule.Params GetParams()
	{
		ShopViewModule.Params result = base.GetParams();
		result.CaptionKey = "GUI_TRAVELLING_MERCHANT";
		result.CategorySprite = UIInfoTools.Instance.GetGenericIcon("FH_UI_MENU_QuickAccessBar_Outpost_ICO_TravelingMerc_Selected");
		result.DontShowCharges = true;
		result.LimitDescriptionKey = "GUI_TRAVELLING_MERCHANT_ITEM_LIMIT";
		return result;
	}
}
