using UnityEngine;

namespace Chronos;

[AddComponentMenu("Time/Area Clock 2D")]
[DisallowMultipleComponent]
[HelpURL("http://ludiq.io/chronos/documentation#AreaClock")]
public class AreaClock2D : AreaClock<Collider2D, Vector2>
{
	protected virtual void OnTriggerEnter2D(Collider2D other)
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

	protected virtual void OnTriggerExit2D(Collider2D collider)
	{
		Timeline component = collider.GetComponent<Timeline>();
		if (component != null)
		{
			Release(component);
		}
	}

	protected override float PointToEdgeTimeScale(Vector3 position)
	{
		Vector2 vector = transform.TransformPoint(center);
		Vector2 vector2 = (Vector2)position - vector;
		Vector2 normalized = vector2.normalized;
		float magnitude = vector2.magnitude;
		float a = ((innerBlend == ClockBlend.Multiplicative) ? 1 : 0);
		if (normalized == Vector2.zero)
		{
			return Mathf.Lerp(a, timeScale, curve.Evaluate(0f));
		}
		CircleCollider2D circleCollider2D = _collider as CircleCollider2D;
		if (circleCollider2D != null && circleCollider2D.offset == center)
		{
			Vector3 lossyScale = transform.lossyScale;
			float num = Mathf.Max(lossyScale.x, lossyScale.y);
			float num2 = circleCollider2D.radius * num;
			Vector2 vector3 = vector + num2 / 2f * normalized;
			if (Timekeeper.instance.debug)
			{
				Debug.DrawLine(vector, vector3, Color.cyan);
				Debug.DrawLine(vector, position, Color.magenta);
			}
			return Mathf.Lerp(a, timeScale, curve.Evaluate(magnitude / num2));
		}
		float magnitude2 = (_collider.bounds.max - _collider.bounds.min).magnitude;
		Vector2 origin = vector + magnitude2 * normalized;
		normalized = -normalized;
		RaycastHit2D[] array = Physics2D.RaycastAll(origin, normalized, magnitude2, 1 << gameObject.layer);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit2D raycastHit2D = array[i];
			if (raycastHit2D.collider == _collider)
			{
				Vector2 point = raycastHit2D.point;
				float magnitude3 = (point - vector).magnitude;
				if (Timekeeper.instance.debug)
				{
					Debug.DrawLine(vector, point, Color.cyan);
					Debug.DrawLine(vector, position, Color.magenta);
				}
				return Mathf.Lerp(a, timeScale, curve.Evaluate(magnitude / magnitude3));
			}
		}
		Debug.LogWarning("Area clock cannot find its collider's edge. Make sure the center is inside and the collider is convex.");
		return timeScale;
	}

	public override bool ContainsPoint(Vector3 point)
	{
		Bounds bounds = _collider.bounds;
		Vector3 size = bounds.size;
		bounds.size = new Vector3(size.x, size.y, 999f);
		return bounds.Contains(point);
	}

	public override void CacheComponents()
	{
		_collider = GetComponent<Collider2D>();
		if (_collider == null)
		{
			throw new ChronosException($"Missing collider for area clock.");
		}
	}
}
