using FHGUI.DTO;

namespace FHGUI.Elements;

public interface IBuildInfoFunctionality<in T> : IBuildInfoFunctionality where T : IBuildingInfoParams
{
	void Init(BuildingStateDTO building, T data);
}
public interface IBuildInfoFunctionality
{
	void Hide();
}
