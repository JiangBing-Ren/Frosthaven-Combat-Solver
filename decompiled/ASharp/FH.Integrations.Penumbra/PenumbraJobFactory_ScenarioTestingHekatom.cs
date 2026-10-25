using Libs.GameConsole;
using Libs.Penumbra;
using UnityEngine.Scripting;

namespace FH.Integrations.Penumbra;

[Preserve]
public class PenumbraJobFactory_ScenarioTestingHekatom : PenumbraJobFactory_ScenarioTestingSubsetBase
{
	public override string JobFilterRef => "Hekatom";

	protected override string[] _scenarioNames => new string[9] { "FS_001", "FS_002", "FS_003", "FS_004", "FS_006", "FS_012", "FS_066", "FH-66", "FS_098" };

	[ConsoleCommand(Command = "RunPenumbra_Hekatom", Description = "Run sub set of scenes")]
	public static void StartFromConsole(IConsole console)
	{
		PenumbraAgent.Instance.StartPenumbra(0u, "CustomSet," + new PenumbraJobFactory_ScenarioTestingHekatom().JobFilterRef);
	}
}
