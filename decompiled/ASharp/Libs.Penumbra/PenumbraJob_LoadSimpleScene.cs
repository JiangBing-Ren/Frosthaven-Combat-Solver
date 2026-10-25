using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace Libs.Penumbra;

[Preserve]
public sealed class PenumbraJob_LoadSimpleScene : PenumbraJob
{
	public string target;

	public override IEnumerator LoadCrt(ILogger logger, PenumbraJobResults outJobResults, [CanBeNull] PenumbraStopwatch stopwatch)
	{
		logger.Log("loading scene " + target);
		SceneManager.LoadScene(target);
		stopwatch?.Time("Loaded");
		yield break;
	}

	public override IEnumerator UnloadCrt(ILogger logger)
	{
		yield break;
	}
}
