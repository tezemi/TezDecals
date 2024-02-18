using UnityEngine;
using TezDecals.Runtime;
using NUnit.Framework;

namespace TezDecals.Tests
{
    // These suck but will be fine for now
    public class MeshGeneratorTests
    {
        private readonly MeshGenerator _meshGenerator = new MeshGenerator();

        [Test]
        public void MeshGeneratorGenerateMeshTest()
        {
            _meshGenerator.AddFace(new[] { new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 1f) });

            var mesh = new Mesh();
            mesh = _meshGenerator.GenerateMesh(mesh, new Rect(0f, 0f, 1f, 1f), 0.001f);

            Assert.AreEqual(3, mesh.vertexCount);
        }

		[Test]
		public void MeshGeneratorClearTest()
		{
			_meshGenerator.AddFace(new[] { new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 1f) });

			_meshGenerator.Clear();

			var mesh = new Mesh();
			mesh = _meshGenerator.GenerateMesh(mesh, new Rect(0f, 0f, 1f, 1f), 0.001f);

			Assert.AreEqual(0, mesh.vertexCount);
		}
	}
}
