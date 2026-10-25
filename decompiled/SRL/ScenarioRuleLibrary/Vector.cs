using System;

namespace ScenarioRuleLibrary;

public struct Vector(float x, float y)
{
	public float X = x;

	public float Y = y;

	public static Vector operator -(Vector v, Vector w)
	{
		return new Vector(v.X - w.X, v.Y - w.Y);
	}

	public static Vector operator +(Vector v, Vector w)
	{
		return new Vector(v.X + w.X, v.Y + w.Y);
	}

	public static float operator *(Vector v, Vector w)
	{
		return v.X * w.X + v.Y * w.Y;
	}

	public static Vector operator *(Vector v, float mult)
	{
		return new Vector(v.X * mult, v.Y * mult);
	}

	public static Vector operator *(float mult, Vector v)
	{
		return new Vector(v.X * mult, v.Y * mult);
	}

	public float Cross(Vector v)
	{
		return X * v.Y - Y * v.X;
	}

	public Vector Normalise()
	{
		float num = (float)Math.Sqrt(this * this);
		return new Vector(X / num, Y / num);
	}

	public float Magnitude()
	{
		return (float)Math.Sqrt(this * this);
	}

	public bool IsAllZero()
	{
		if (MF.IsZero(X))
		{
			return MF.IsZero(Y);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		Vector vector = (Vector)obj;
		if (MF.IsZero(X - vector.X))
		{
			return MF.IsZero(Y - vector.Y);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
