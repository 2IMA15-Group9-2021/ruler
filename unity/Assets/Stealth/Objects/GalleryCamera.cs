using Stealth.Controller;
using Stealth.Utils;
using UnityEngine;
using Util.Algorithms.Triangulation;
using Util.Geometry;
using Util.Geometry.Polygon;

namespace Stealth.Objects
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

        /// <summary>
        /// Backing field for <see cref="FieldOfViewDegrees"/>.
        /// </summary>
        [SerializeField]
        [Tooltip("The field of view of the camera, expressed in degrees.")]
        private float _fieldOfViewDegrees;

        [SerializeField]
        private MeshFilter visionMeshFilter;

        private Mesh visionMesh;
        public Polygon2D visionArea;
        private LevelIsland level;

        //private float oldFieldOfViewDegrees;

        [SerializeField]
        public bool disabled = false;

        private StealthController stealthController;
        private CameraVision vision;

        /// <summary>
        /// The field of view of the camera, expressed in degrees.
        /// </summary>
        public float FieldOfViewDegrees
        {
            get => _fieldOfViewDegrees;
            private set
            {
                _fieldOfViewDegrees = value;
            } 
        }

        /// <summary>
        /// The field of view of the camera, expressed in radians.
        /// </summary>
        public float FieldOfView
        {
            get
            {
                return FieldOfViewDegrees * Mathf.Deg2Rad;
            }
            private set
            {
                FieldOfViewDegrees = Mathf.Rad2Deg * value;
            }
        }

        private void Awake()
        {
            level = FindObjectOfType<LevelIsland>();
            visionMesh = new Mesh();
            visionMeshFilter = GetComponentInChildren<MeshFilter>();
            stealthController = FindObjectOfType<StealthController>();
        }

        public bool IsPointVisible(Vector2 point)
        {
            return visionArea == null ? false : visionArea.ContainsInside(point);
        }

        /// <summary>
        /// Returns a <see cref="Line"/> along the right vision boundary in world space.
        /// </summary>
        /// <returns>A <see cref="Line"/> along the right vision boundary in world space.</returns>
        public Line GetRightBoundary()
        {
            float rot = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            return new Line(transform.position, rot - 0.5f * FieldOfView);
        }

        /// <summary>
        /// Returns a <see cref="Line"/> along the left vision boundary in world space.
        /// </summary>
        /// <returns>A <see cref="Line"/> along the left vision boundary in world space.</returns>
        public Line GetLeftBoundary()
        {
            float rot = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            return new Line(transform.position, rot + 0.5f * FieldOfView);
        }

        /// <summary>
        /// Computes the vision area of this camera
        /// </summary>
        [ContextMenu("Compute vision area")]
        public void ComputeVisionArea()
        {
            if (level == null) level = FindObjectOfType<LevelIsland>();

            vision = new CameraVision(this, level);
            visionArea = vision.Compute(inLocalSpace: false);



            visionMesh = Triangulator.Triangulate(visionArea.ToLocalSpace(transform)).CreateMesh();
            visionMesh.RecalculateNormals();
            if (Application.isPlaying)
            {
                visionMeshFilter.mesh = visionMesh;
            }
        }

        //private void Start()
        //{
        //    oldFieldOfViewDegrees = FieldOfViewDegrees;
        //}

        //private void Update()
        //{
        //    if (transform.hasChanged || FieldOfViewDegrees!=oldFieldOfViewDegrees)
        //    {
        //        CalculateVisionPolygon();
        //        transform.hasChanged = false;
        //        oldFieldOfViewDegrees = FieldOfViewDegrees;
        //    }
        //}

        ///// <summary>
        ///// Toggles this camera from enabled to disabled
        ///// </summary>
        //private void OnMouseDown()
        //{
        //    visionMeshFilter.gameObject.SetActive(disabled);
        //    disabled = !disabled;
        //    stealthController.cameraVisionChanged = true;
        //}


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

            if (visionMesh != null)
            {
                Gizmos.DrawMesh(visionMesh, transform.position, transform.rotation);
            }
        }
    }
}
