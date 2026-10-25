using System.Collections.Generic;
using UnityEngine;

public class InteractabilityChecker : Singleton<InteractabilityChecker>
{
	private HashSet<Component> activeRequests = new HashSet<Component>();

	public virtual bool IsActive => activeRequests.Count > 0;

	public void RequestActive(Component request, bool active)
	{
		if (active)
		{
			activeRequests.Add(request);
		}
		else
		{
			activeRequests.Remove(request);
		}
	}

	public void ClearActiveRequests()
	{
		activeRequests.Clear();
	}
}
