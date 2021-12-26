﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Util.Algorithms.Triangulation;
using Util.Geometry;
using Util.Geometry.Polygon;
using static Stealth.CameraVision;

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

        /// <summary>
        /// Backing field for <see cref="FieldOfViewDegrees"/>.
        /// </summary>
        [SerializeField]
        [Tooltip("The field of view of the camera, expressed in degrees.")]
        private float _fieldOfViewDegrees;

        private MeshFilter meshFilter;
        private Mesh visionMesh;
        private LevelPolygon level;

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
            level = FindObjectOfType<LevelPolygon>();
            meshFilter = GetComponent<MeshFilter>();
            visionMesh = new Mesh();
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

        [ContextMenu("Calculate vision polygon")]
        private void CalculateVisionPolygon()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("This method needs to be written differently to be used outside play mode.");
                return;
            }

            var cameraVision = new CameraVision(this, level);
            var polygon = cameraVision.Compute();

            visionMesh = Triangulator.Triangulate(polygon).CreateMesh();
            visionMesh.RecalculateNormals();
            meshFilter.mesh = visionMesh;
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
