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
