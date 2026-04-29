using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Helpers.Math
{
	public static class MathUtil
	{
		public static float ClampMagnitude(this float n, float max)
		{
			return Mathf.Sign(n) * Mathf.Min(Mathf.Abs(n), max);
		}

		public static float Remap(this float value, float srcMin, float srcMax, float destMin, float destMax, bool clamp = false)
		{
			if (clamp) value = Mathf.Clamp(value, srcMin, srcMax);

			return (value - srcMin) / (srcMax - srcMin) * (destMax - destMin) + destMin;
		}

		public static float Damp(float a, float b, float lambda, float dt = -1)
		{
			if (dt == -1) dt = Time.deltaTime;
			return Mathf.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
		}

		public static float DampDegrees(float a, float b, float lambda, float dt = -1)
		{
			if (dt == -1) dt = Time.deltaTime;

			a.Repeat(-180, 180);
			b.Repeat(-180, 180);

			float v;
			if (Mathf.Abs(b - a) < 180)
				v = Damp(a, b, lambda, dt);
			else
				v = Damp(Mathf.Max(a, b) - 360, Mathf.Min(a, b), lambda, dt);
			
			return v;
		}

		public static Vector3 Damp(Vector3 a, Vector3 b, float lambda, float dt = -1)
		{
			return new Vector3(
				DampDegrees(a.x, b.x, lambda, dt),
				DampDegrees(a.y, b.y, lambda, dt),
				DampDegrees(a.z, b.z, lambda, dt));
		}

		public static float Repeat(this float n, float min, float max)
		{
			return min + (n - min) % (max - min);
		}
	}
}