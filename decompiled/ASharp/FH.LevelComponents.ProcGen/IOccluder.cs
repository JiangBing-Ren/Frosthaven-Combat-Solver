using System;

namespace FH.LevelComponents.ProcGen;

public interface IOccluder<in TOcclusionCheckResult>
{
	void SetOnDestroyCallback(Action callback);

	void OnUpdatedOcclusionCheck(TOcclusionCheckResult occlusionResult);
}
