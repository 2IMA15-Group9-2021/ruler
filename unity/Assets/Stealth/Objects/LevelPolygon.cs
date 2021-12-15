using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Util.Algorithms.Triangulation;
using Util.Geometry.Polygon;
using Util.Geometry.Triangulation;

namespace Stealth
{
    /// <summary>
    /// Represents the level polygon.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class LevelPolygon : MonoBehaviour
    {   
        [SerializeField]
        private Vector2[] outsideVertices;

        public Vector2[] OutsideVertices
        {
            get { return outsideVertices; }
            set { outsideVertices = value; }
        }

        [SerializeField]
        private HoleBoundary[] holesBoundary;

        public HoleBoundary[] HolesBoundary
        {
            get { return holesBoundary; }
            set { holesBoundary = value; }
        }

        [System.Serializable]
        public class HoleBoundary
        {
            public Vector2[] Vertices;
        }

        [SerializeField]
        private LevelPolygonHole holePrefab;

        [SerializeField]
        private DebugSettings debugSettings = new DebugSettings()
        {
            OutsideColor = Color.white,
            HolesColor = Color.gray,
            VertexGizmoRadius = 0.1f
        };

        [System.Serializable]
        private class DebugSettings
        {
            public Color OutsideColor;
            public Color HolesColor;
            public float VertexGizmoRadius;
        }

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        private Mesh outsideMesh;
        private Mesh[] holesMesh;
        private Polygon2DWithHoles levelPolygon;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();

            meshRenderer.enabled = true;
        }

        private void Start()
        {
            UpdateMesh();
        }

        [ContextMenu("Update mesh")]
        public void UpdateMesh()
        {
            if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();

            // Create polygons for outside boundary and holes
            Polygon2D outsidePolygon = new Polygon2D(outsideVertices);
            Polygon2D[] holesPolygon = holesBoundary.Select(hole => new Polygon2D(hole.Vertices)).ToArray();

            // Create polygon for level
            levelPolygon = new Polygon2DWithHoles(outsidePolygon, holesPolygon);

            // Create meshes for outside and holes
            outsideMesh = Triangulator.Triangulate(levelPolygon.Outside, false).CreateMesh();
            outsideMesh.RecalculateNormals();
            holesMesh = holesPolygon.Select(hole => Triangulator.Triangulate(hole, false).CreateMesh()).ToArray();
            foreach (Mesh hole in holesMesh)
            {
                hole.RecalculateNormals();
            }

            // Update mesh filters if we're in play mode
            if (Application.isPlaying)
            {
                // Assign mesh to mesh filter
                meshFilter.mesh = outsideMesh;

                // Destroy all current hole objects
                foreach (Transform child in transform)
                {
                    if (child.gameObject == holePrefab.gameObject) continue;
                    // Assume all children are hole objects
                    Destroy(child.gameObject);
                }
                // Create enough hole objects
                for (int i = 0; i < holesMesh.Length; i++)
                {
                    // Instantiate as child of self
                    LevelPolygonHole instance = Instantiate(holePrefab, transform);
                    instance.gameObject.SetActive(true);
                    instance.UpdateMesh(holesMesh[i]);
                }
            }
            else
            {
                // If we're in edit mode, disable the mesh renderer
                // we'll draw the meshes using gizmos
                meshRenderer.enabled = false;
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                // Check that no two consecutive vertices have the same coordinates
                for (int i = 0; i < outsideVertices.Length; i++)
                {
                    if (i == outsideVertices.Length - 1) break;

                    if (Vector2.Distance(outsideVertices[i], outsideVertices[i + 1]) < Mathf.Epsilon)
                    {
                        // Move the second vertex slightly
                        Vector2 vertex = outsideVertices[i + 1];
                        vertex.x += 1;
                        outsideVertices[i + 1] = vertex;
                    }
                }
                UpdateMesh();
            }
        }

        private void OnDrawGizmos()
        {
            // Draw vertices of outside boundary
            Gizmos.color = debugSettings.OutsideColor;
            Gizmos.DrawMesh(outsideMesh, transform.position);

            // Draw edges of holes
            Gizmos.color = debugSettings.HolesColor;
            foreach (Mesh hole in holesMesh)
            {
                if (hole.vertices.Length < 3) continue;
                Gizmos.DrawMesh(hole, transform.position);
            }
        }
    }
}