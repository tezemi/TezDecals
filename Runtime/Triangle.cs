using UnityEngine;

namespace TezDecals.Runtime
{
	internal struct Triangle
	{
		public Vector3 Vector1;
		public Vector3 Vector2;
		public Vector3 Vector3;

		public Triangle(Vector3 vector1, Vector3 vector2, Vector3 vector3)
		{
			Vector1 = vector1;
			Vector2 = vector2;
			Vector3 = vector3;
		}
	}
}
