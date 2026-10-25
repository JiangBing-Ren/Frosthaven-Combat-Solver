using FHRL;

namespace FHGUI.Services;

public interface IReadyCheckProcessor
{
	bool IsApplicableReadyCheckCommand(string readyCheckID);

	bool ProcessReadyCheckResult(MultiplayerGameController.Result readyCheckResult);
}
