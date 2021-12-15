using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stealth
{
    /// <summary>
    /// A camera in the game world.
    /// </summary>
    public class GalleryCamera : MonoBehaviour
    {
        [SerializeField]
        private Color gizmoColor;

        [SerializeField]
        private float gizmoLength = 1f;

        private MeshFilter mf;
        private Mesh tri;

        /// <summary>
        /// Backing field for <see cref="FieldOfViewDegrees"/>.
        /// </summary>
        [SerializeField]
        [Tooltip("The field of view of the camera, expressed in degrees.")]
        private float _fieldOfViewDegrees;

        /// <summary>
        /// The field of view of the camera, expressed in degrees.
        /// </summary>
        public float FieldOfViewDegrees
        {
            get => _fieldOfViewDegrees;
            private set
            {
                _fieldOfViewDegrees = value;
                FieldOfView = Mathf.Deg2Rad * value;
            } 
        }

        /// <summary>
        /// Backing field for <see cref="FieldOfView"/>.
        /// </summary>
        private float _fieldOfView;
        /// <summary>
        /// The field of view of the camera, expressed in radians.
        /// </summary>
        public float FieldOfView
        {
            get => _fieldOfView;
            private set
            {
                _fieldOfView = value;
                FieldOfViewDegrees = Mathf.Rad2Deg * value;
            }
        }

        void Start()
        {
            mf = GetComponent<MeshFilter>();
            tri = mf.mesh;
            tri.vertices = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero };
            tri.triangles = new int[] { 0, 1, 2 };
            mf.mesh = tri;
        }


        private Mesh generateTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            tri = mf.mesh;
            tri.Clear();

            tri.vertices = new Vector3[] { v1, v2, v3 };
            //mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
            tri.triangles = new int[] { 0, 1, 2 };

            mf.mesh = tri;

            return tri;
        }

        void OnValidate()
        {
            if (mf == null)
            {
                mf = GetComponent<MeshFilter>();
            }

            Quaternion rot1 = Quaternion.Euler(0, 0, 0.5f * FieldOfViewDegrees);
            Quaternion rot2 = Quaternion.Euler(0, 0, -0.5f * FieldOfViewDegrees);
            Mesh mesh = generateTriangle(Vector3.zero,
                rot1 * new Vector3( 1, 0, 0 ) * gizmoLength,
                rot2 * new Vector3( 1, 0, 0 ) * gizmoLength);
            Graphics.DrawMeshNow(mesh, transform.position, Quaternion.Euler(0, 0, 0));
        }

        /// <summary>
        /// Draws camera field of view in the editor.
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Quaternion rot1 = Quaternion.Euler(0, 0, -0.5f * FieldOfViewDegrees);
            Quaternion rot2 = Quaternion.Euler(0, 0, 0.5f * FieldOfViewDegrees);
            Gizmos.DrawRay(transform.position, rot1 * transform.right * gizmoLength);
            Gizmos.DrawRay(transform.position, rot2 * transform.right * gizmoLength);
        }
    }
}
