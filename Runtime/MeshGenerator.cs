using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace TezDecals.Runtime
{
	public class MeshGenerator
	{
		private readonly List<int> _indices = new List<int>();
		private readonly List<Vector3> _vertices = new List<Vector3>();

		public void AddFace(Vector3[] face)
		{
			var i1 = AddVertex(face[0]);

			for (var i = 1; i < face.Length - 1; i++)
			{
				var i2 = AddVertex(face[i]);
				var i3 = AddVertex(face[i + 1]);

				_indices.Add(i1);
				_indices.Add(i2);
				_indices.Add(i3);
			}

			int AddVertex(Vector3 vertex)
			{
				const float EPSILON = 0.01f;

				var index = _vertices.FindIndex(i => Vector3.Distance(i, vertex) < EPSILON);

				if (index == -1)
				{
					_vertices.Add(vertex);

					return _vertices.Count - 1;
				}
				else
				{
					return index;
				}
			}
		}

		public Mesh GenerateMesh(Mesh existingMesh, Rect uv, float offset)
		{
			if (existingMesh == null)
				existingMesh = new Mesh();

			existingMesh.Clear(true);

			if (_indices.Count == 0) 
				return existingMesh;

			var vertices = _vertices.ToArray();
			var indices = _indices.ToArray();
			var normals = GetNormals();
			var uvs = vertices.Select(v => GetUVs(v)).ToArray();

			for (var i = 0; i < vertices.Length; i++)
			{
				vertices[i] += normals[i] * offset;
			}

			existingMesh.vertices = vertices;
			existingMesh.normals = normals;
			existingMesh.uv = uvs;
			existingMesh.triangles = indices;

			return existingMesh;

			Vector2 GetUVs(Vector3 vertex)
			{
				var u = Mathf.Lerp(uv.xMin, uv.xMax, vertex.x + 0.5f);
				var v = Mathf.Lerp(uv.yMin, uv.yMax, vertex.y + 0.5f);

				return new Vector2(u, v);
			}

			Vector3[] GetNormals()
			{
				var normals = new Vector3[vertices.Length];

				for (var i = 0; i < indices.Length; i += 3)
				{
					var ind1 = indices[i];
					var ind2 = indices[i + 1];
					var ind3 = indices[i + 2];

					var v1 = vertices[ind1];
					var v2 = vertices[ind2];
					var v3 = vertices[ind3];

					var normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

					normals[ind1] += normal;
					normals[ind2] += normal;
					normals[ind3] += normal;
				}

				for (var i = 0; i < normals.Length; i++)
				{
					normals[i].Normalize();
				}

				return normals;
			}
		}

		public void Clear()
		{
			_vertices.Clear();
			_indices.Clear();
		}
	}
}
