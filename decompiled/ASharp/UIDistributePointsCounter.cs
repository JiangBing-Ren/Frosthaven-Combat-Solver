using UnityEngine;

public abstract class UIDistributePointsCounter : MonoBehaviour
{
	public virtual void Setup(int maxPoints, int currentPoints)
	{
		SetCurrentPoints(currentPoints);
		SetExtendedPoints(0);
	}

	public abstract void SetCurrentPoints(int currentPoints);

	public abstract void SetExtendedPoints(int extendedPoints);
}
