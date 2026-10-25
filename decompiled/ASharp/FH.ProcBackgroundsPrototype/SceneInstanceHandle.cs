using UnityEngine;

namespace FH.ProcBackgroundsPrototype;

internal sealed class SceneInstanceHandle
{
	public readonly AsyncOperation handle;

	public readonly string sceneName;

	public GameBackgrounds.WorldContext context;

	public SceneInstanceHandle(AsyncOperation handle, string sceneName)
	{
		this.handle = handle;
		this.sceneName = sceneName;
	}
}
