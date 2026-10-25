using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

namespace FHGUI.Scenario;

public class FHScenarioViewModule : MonoBehaviour, IUIViewModule
{
	public TMP_Text Text;

	public GameObject GameObject => gameObject;

	public void SetStateInfo(CPhase.PhaseType phase, FHScenarioPhaseViewState phaseViewState, FHScenarioInfoViewState infoViewState, FHScenarioReactionViewState reactionViewState)
	{
		Text.text = $"Phase: {phase}\nPhaseViewState: {phaseViewState?.GetType().Name}\nInfoViewState:{infoViewState?.GetType().Name}\nReactionViewState:{reactionViewState?.GetType().Name}";
	}
}
