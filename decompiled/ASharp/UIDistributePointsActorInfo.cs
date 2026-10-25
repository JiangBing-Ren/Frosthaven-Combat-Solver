using UnityEngine;

public abstract class UIDistributePointsActorInfo : MonoBehaviour
{
	public abstract void Display(IDistributePointsActor action);

	public abstract void RefreshAssignedPoints(int currentPoints, int points);
}
