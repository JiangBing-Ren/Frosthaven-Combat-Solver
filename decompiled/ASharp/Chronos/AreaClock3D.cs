using UnityEngine;

namespace Chronos;

[AddComponentMenu("Time/Area Clock 3D")]
[DisallowMultipleComponent]
[HelpURL("http://ludiq.io/chronos/documentation#AreaClock")]
public class AreaClock3D : AreaClock<Collider, Vector3>
{
	protected virtual void OnTriggerEnter(Collider other)
	{
		if (enabled)
		{
			Timeline component = other.GetComponent<Timeline>();
			if (component != null)
			{
				Vector3 entry = transform.InverseTransformPoint(other.transform.position);
				Capture(component, entry);
			}
		}
	}

	protected virtual void OnTriggerExit(Collider collider)
	{
		Timeline component = collider.GetComponent<Timeline>();
		if (component != null)
		{
			Release(component);
		}
	}

	protected override float PointToEdgeTimeScale(Vector3 position)
	{
		Vector3 vector = transform.TransformPoint(center);
		Vector3 vector2 = position - vector;
		Vector3 normalized = vector2.normalized;
		float magnitude = vector2.magnitude;
		float a = ((innerBlend == ClockBlend.Multiplicative) ? 1 : 0);
		if (normalized == Vector3.zero)
		{
			return Mathf.Lerp(a, timeScale, curve.Evaluate(0f));
		}
		SphereCollider sphereCollider = _collider as SphereCollider;
		if (sphereCollider != null && sphereCollider.center == center)
		{
			Vector3 lossyScale = transform.lossyScale;
			float num = Mathf.Max(lossyScale.x, lossyScale.y, lossyScale.z);
			float num2 = sphereCollider.radius * num;
			Vector3 end = vector + normalized * num2;
			if (Timekeeper.instance.debug)
			{
				Debug.DrawLine(vector, end, Color.cyan);
				Debug.DrawLine(vector, position, Color.magenta);
			}
			return Mathf.Lerp(a, timeScale, curve.Evaluate(magnitude / num2));
		}
		float magnitude2 = (_collider.bounds.max - _collider.bounds.min).magnitude;
		Ray ray = new Ray(vector, normalized);
		ray.origin = ray.GetPoint(magnitude2);
		ray.direction = -ray.direction;
		if (_collider.Raycast(ray, out var hitInfo, magnitude2))
		{
			Vector3 point = hitInfo.point;
			float magnitude3 = (point - vector).magnitude;
			if (Timekeeper.instance.debug)
			{
				Debug.DrawLine(vector, point, Color.cyan);
				Debug.DrawLine(vector, position, Color.magenta);
			}
			return Mathf.Lerp(a, timeScale, curve.Evaluate(magnitude / magnitude3));
		}
		Debug.LogWarning("Area clock cannot find its collider's edge. Make sure the center is inside and the collider is convex.");
		return timeScale;
	}

	public override bool ContainsPoint(Vector3 point)
	{
		return _collider.bounds.Contains(point);
	}

	public override void CacheComponents()
	{
		_collider = GetComponent<Collider>();
		if (_collider == null)
		{
			throw new ChronosException($"Missing collider for area clock.");
		}
	}
}
