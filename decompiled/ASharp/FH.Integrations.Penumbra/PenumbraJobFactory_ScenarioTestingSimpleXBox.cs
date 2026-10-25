using Libs.GameConsole;
using Libs.Penumbra;
using UnityEngine.Scripting;

namespace FH.Integrations.Penumbra;

[Preserve]
public class PenumbraJobFactory_ScenarioTestingSimpleXBox : PenumbraJobFactory_ScenarioTestingSubsetBase
{
	public override string JobFilterRef => "SimpleXBox";

	protected override string[] _scenarioNames => new string[3] { "FS_001", "FS_004", "FS_098" };

	[ConsoleCommand(Command = "RunPenumbra_XbSimp", Description = "Run sub set of scenes")]
	public static void StartFromConsole(IConsole console)
	{
		PenumbraAgent.Instance.StartPenumbra(0u, "CustomSet," + new PenumbraJobFactory_ScenarioTestingSimpleXBox().JobFilterRef);
	}
}
