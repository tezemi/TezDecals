using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TezDecals.Runtime
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Decal : MonoBehaviour
    {
        public bool FixedAspect = true;
        public int MaxAngle = 120;
        public float Offset = 0.009f;
		public LayerMask LayerMask = -1;
		public Material Material;
        public Sprite Sprite;
		public const int MinMaxAngle = 0;
		public const int MaxMaxAngle = 180;
		public const float MinOffset = 0.005f;
		public const float MaxOffset = 0.05f;
        private Vector3 _oldScale;
		private MeshFilter _meshFilter;
		private MeshRenderer _meshRenderer;
		private readonly Color _noSpriteGizmoColor = new Color(0f, 1f, 1f, 0.35f);
		private readonly MeshGenerator _meshGenerator = new MeshGenerator();
		#if UNITY_EDITOR
		private static readonly List<Decal> _selectedDecals = new List<Decal>();
		#endif		

		public MeshFilter MeshFilter
        { 
            get
            {
                if (_meshFilter == null)
                {
                    _meshFilter = GetComponent<MeshFilter>();
                }

                return _meshFilter;
            }
            private set
            {
                _meshFilter = value;
            }
        }

		public MeshRenderer MeshRenderer
		{
			get
			{
				if (_meshRenderer == null)
				{
					_meshRenderer = GetComponent<MeshRenderer>();
				}

				return _meshRenderer;
			}
			private set
			{
				_meshRenderer = value;
			}
		}

		public Bounds Bounds
		{
			get
			{
				var size = transform.lossyScale;
				var min = -size / 2f;
				var max = size / 2f;

				var points = new Vector3[]
				{
					new Vector3(min.x, min.y, min.z),
					new Vector3(max.x, min.y, min.z),
					new Vector3(min.x, max.y, min.z),
					new Vector3(max.x, max.y, min.z),
					new Vector3(min.x, min.y, max.z),
					new Vector3(max.x, min.y, max.z),
					new Vector3(min.x, max.y, max.z),
					new Vector3(max.x, max.y, max.z),
				};

				points = points.Select(transform.TransformDirection).ToArray();
				min = points.Aggregate(Vector3.Min);
				max = points.Aggregate(Vector3.Max);

				return new Bounds(transform.position, max - min);
			}
		}

		#if UNITY_EDITOR
		static Decal()
		{
			// This prevents duplicated decals from using the same mesh
			UnityEditor.Selection.selectionChanged += () =>
			{
				foreach (var decal in _selectedDecals)
				{
					if (decal == null)
						continue;

					foreach (var obj in UnityEditor.Selection.gameObjects)
					{
						if (obj == null)
							continue;

						if (obj.TryGetComponent<Decal>(out var otherDecal) &&
						decal != otherDecal &&
						decal.MeshFilter.sharedMesh == otherDecal.MeshFilter.sharedMesh)
						{
							otherDecal.MeshFilter.sharedMesh = null;
						}
					}
				}

				_selectedDecals.Clear();

				foreach (var obj in UnityEditor.Selection.gameObjects)
				{
					if (obj == null)
						continue;

					if (obj.TryGetComponent<Decal>(out var decal))
					{
						_selectedDecals.Add(decal);
					}
				}
			};
		}
		#endif

		public virtual void OnValidate()
        {
            #if UNITY_EDITOR
            if (UnityEditor.BuildPipeline.isBuildingPlayer)
				return;
			#endif

			MaxAngle = Mathf.Clamp(MaxAngle, MinMaxAngle, MaxMaxAngle);
			Offset = Mathf.Clamp(Offset, MinOffset, MaxOffset);

			if (Material == null)
            {
                Sprite = null;
            }

            if (Sprite != null && Material.mainTexture != Sprite.texture)
            {
                Sprite = null;
            }
        }

		protected virtual void OnDrawGizmos()
		{
			if (Sprite == null)
			{
				DrawBoundingBox();

				Gizmos.matrix = transform.localToWorldMatrix;
				Gizmos.color = _noSpriteGizmoColor;
				Gizmos.DrawCube(Vector3.zero, Vector3.one);
			}
		}

		protected virtual void OnDrawGizmosSelected()
		{
			DrawBoundingBox();

			var forward = -transform.forward;
			DrawArrow(transform.position, forward * 1.5f, Color.cyan);
		}

		protected virtual void Awake()
        {
            #if UNITY_EDITOR
			if (UnityEditor.BuildPipeline.isBuildingPlayer)
				return;
			#endif

			MeshFilter = GetComponent<MeshFilter>();
			MeshRenderer = GetComponent<MeshRenderer>();
		}

		protected virtual void Update()
		{
			#if UNITY_EDITOR
			if (UnityEditor.BuildPipeline.isBuildingPlayer)
				return;
			#endif

			if (Application.isPlaying)
				return;

			if (transform.hasChanged)
			{
				transform.hasChanged = false;
				Generate();
			}
		}

		public void Generate()
		{
			if (Sprite != null && FixedAspect)
			{
				var rect = Sprite.rect;
				var scale = transform.localScale;
				var ratio = rect.width / rect.height;

				if (!Mathf.Approximately(_oldScale.x, scale.x))
				{
					scale.y = scale.x / ratio;
				}
				else if (!Mathf.Approximately(_oldScale.y, scale.y))
				{
					scale.x = scale.y * ratio;
				}
				else if (!Mathf.Approximately(scale.x / scale.y, ratio))
				{
					scale.x = scale.y * ratio;
				}

				_oldScale = scale;

				var hasChanged = transform.hasChanged;
				transform.localScale = scale;
				transform.hasChanged = hasChanged;
			}

			if (Material != null && Sprite != null)
			{
				_meshGenerator.Clear();

				var bounds = Bounds;
				var intersectingMeshes = FindObjectsOfType<MeshRenderer>()
				.Where(mr => bounds.Intersects(mr.bounds) &&
							(LayerMask & (1 << mr.gameObject.layer)) != 0 &&
							(mr.gameObject.isStatic || !gameObject.isStatic) &&
							!mr.TryGetComponent<Decal>(out var _))					
				.Select(mf => mf.GetComponent<MeshFilter>())
					.Where(mf => mf.sharedMesh != null)
				.ToArray();
				
				foreach (var triangle in GetTriangles(intersectingMeshes))
				{
					var face = TrimFace(triangle.Vector1, triangle.Vector2, triangle.Vector3);

					if (face.Length > 0)
						_meshGenerator.AddFace(face);
				}

				MeshFilter.sharedMesh = _meshGenerator.GenerateMesh(MeshFilter.sharedMesh, GetSpriteUV(), Offset);
				MeshRenderer.sharedMaterial = Material;
			}
			else
			{
				DestroyImmediate(MeshFilter.sharedMesh);

				MeshFilter.sharedMesh = null;
				MeshRenderer.sharedMaterial = null;
			}
		}

		public void GenerateAndSetDirty()
		{
			Generate();
			SetDirty();
		}
	
		public static Decal CreateDecal(Vector3 position, Quaternion rotation, Material material, Sprite sprite, int maxAngle = 90, float offset = 0.009f)
		{
			return CreateDecal(position, Vector3.one, rotation, material, sprite, maxAngle, offset);
		}

		public static Decal CreateDecal(Vector3 position, Vector3 scale, Quaternion rotation, Material material, Sprite sprite, int maxAngle = 90, float offset = 0.009f)
		{
			var decalGameObject = new GameObject($"Decal ({sprite.name})", typeof(Decal));
			var decal = decalGameObject.GetComponent<Decal>();

			decal.Material = material;
			decal.Sprite = sprite;
			decal.MaxAngle = maxAngle;
			decal.Offset = offset;

			var tr = decal.transform;
			tr.position = position;
			tr.localScale = scale;
			tr.rotation = rotation;

			decal.GenerateAndSetDirty();

			return decal;
		}

		public static Decal CreateDecal(Vector3 position, Quaternion rotation, Material material, Sprite sprite, LayerMask layerMask, int maxAngle = 90, float offset = 0.009f)
		{
			return CreateDecal(position, Vector3.one, rotation, material, sprite, layerMask, maxAngle, offset);
		}

		public static Decal CreateDecal(Vector3 position, Vector3 scale, Quaternion rotation, Material material, Sprite sprite, LayerMask layerMask, int maxAngle = 90, float offset = 0.009f)
		{
			var decalGameObject = new GameObject($"Decal ({sprite.name})", typeof(Decal));
			var decal = decalGameObject.GetComponent<Decal>();

			decal.Material = material;
			decal.Sprite = sprite;
			decal.MaxAngle = maxAngle;
			decal.Offset = offset;
			decal.LayerMask = layerMask;

			var tr = decal.transform;
			tr.position = position;
			tr.localScale = scale;
			tr.rotation = rotation;

			decal.GenerateAndSetDirty();

			return decal;
		}

		private void DrawBoundingBox()
		{
			Gizmos.color = Color.cyan;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

			var bounds = Sprite != null ? Bounds : new Bounds(transform.position, Vector3.one);
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(bounds.center, bounds.size + Vector3.one * 0.01f);
		}

		private void DrawArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
		{
			Gizmos.color = color;
			Gizmos.DrawRay(pos, direction);

			var right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
			var left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);

			Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
			Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
		}

		private void SetDirty()
		{
			#if UNITY_EDITOR
			if (gameObject.scene.IsValid() && !UnityEditor.EditorApplication.isPlaying)
			{
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
			}
			else
			{
				UnityEditor.EditorUtility.SetDirty(gameObject);
			}
			#endif
		}

		private Rect GetSpriteUV()
		{
			var texture = Sprite.texture;
			var textureRect = Sprite.textureRect;

			textureRect.x /= texture.width;
			textureRect.y /= texture.height;
			textureRect.width /= texture.width;
			textureRect.height /= texture.height;

			return textureRect;
		}

		private IEnumerable<Triangle> GetTriangles(MeshFilter[] meshFilters)
		{
			foreach (var meshFilter in meshFilters)
			{
				var meshFilterIntersectionMatrix = transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;
				
				var mesh = meshFilter.sharedMesh;

				var vertices = mesh.vertices;
				var triangles = mesh.triangles;

				for (var i = 0; i < triangles.Length; i += 3)
				{
					var i1 = triangles[i];
					var i2 = triangles[i + 1];
					var i3 = triangles[i + 2];

					var v1 = vertices[i1];
					var v2 = vertices[i2];
					var v3 = vertices[i3];

					var triangle = new Triangle(v1, v2, v3);
					var transformedTriangle = Transform(triangle, meshFilterIntersectionMatrix);

					var normal = GetNormal(transformedTriangle);
					var angle = Vector3.Angle(Vector3.back, normal);

					if (angle <= MaxAngle)
						yield return transformedTriangle;
				}
			}

			Triangle Transform(Triangle triangle, Matrix4x4 matrix)
			{
				var v1 = matrix.MultiplyPoint(triangle.Vector1);
				var v2 = matrix.MultiplyPoint(triangle.Vector2);
				var v3 = matrix.MultiplyPoint(triangle.Vector3);

				return new Triangle(v1, v2, v3);
			}

			Vector3 GetNormal(Triangle triangle)
			{
				return Vector3.Cross
				(
					triangle.Vector2 - triangle.Vector1,
					triangle.Vector3 - triangle.Vector1
				).normalized;
			}
		}

		private Vector3[] TrimFace(params Vector3[] face)
		{
			var front = new Plane(Vector3.forward, 0.5f);
			var back = new Plane(Vector3.back, 0.5f);
			var top = new Plane(Vector3.up, 0.5f);
			var bottom = new Plane(Vector3.down, 0.5f);
			var right = new Plane(Vector3.right, 0.5f);
			var left = new Plane(Vector3.left, 0.5f);

			face = Restrain(front).ToArray();
			face = Restrain(back).ToArray();
			face = Restrain(top).ToArray();
			face = Restrain(bottom).ToArray();
			face = Restrain(right).ToArray();
			face = Restrain(left).ToArray();

			return face;

			IEnumerable<Vector3> Restrain(Plane plane)
			{
				for (var i = 0; i < face.Length; i++)
				{
					var nextIndex = (i + 1) % face.Length;

					var v1 = face[i];
					var v2 = face[nextIndex];

					if (plane.GetSide(v1))
					{
						yield return v1;
					}

					if (plane.GetSide(v1) != plane.GetSide(v2))
					{
						yield return PlaneLineCast(plane, v1, v2);
					}
				}

				Vector3 PlaneLineCast(Plane plane, Vector3 a, Vector3 b)
				{
					var ray = new Ray(a, b - a);

					plane.Raycast(ray, out var enter);

					return ray.GetPoint(enter);
				}
			}
		}		
	}
}
