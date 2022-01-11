using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Stealth.Objects
{
    public class LevelPolygonHole : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private PolygonCollider2D collider;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            collider = GetComponent<PolygonCollider2D>();
        }

        public void UpdateMesh(Mesh mesh)
        {
            if (meshFilter == null) GetComponent<MeshFilter>();

            meshFilter.mesh = mesh;
            Vector3[] verts = mesh.vertices;
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < verts.Length; i++)
            {
                points.Add(new Vector2(verts[i].x, verts[i].y) );
            }

            collider.points = points.ToArray();
        }

        private void InvertMesh()
        {
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            mesh.triangles = mesh.triangles.Reverse().ToArray();
            meshFilter.mesh = mesh;
        }
    }
}
