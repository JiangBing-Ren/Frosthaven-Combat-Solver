using System;
using Unity.Jobs;

namespace FH.LevelComponents.ProcGen;

public interface IOcclusionCheckJob<TResult> : IJobFor, IDisposable where TResult : struct
{
	TResult[] GetResults();

	int GetStartIndex();
}
