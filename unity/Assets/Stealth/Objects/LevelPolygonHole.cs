using System.Linq;
using UnityEngine;

namespace Stealth.Objects
{
    public class LevelPolygonHole : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshCollider = GetComponent<MeshCollider>();
        }

        public void UpdateMesh(Mesh mesh)
        {
            if (meshFilter == null) GetComponent<MeshFilter>();

            meshFilter.mesh = mesh;
        }

        private void InvertMesh()
        {
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            mesh.triangles = mesh.triangles.Reverse().ToArray();
            meshFilter.mesh = mesh;
        }
    }
}
