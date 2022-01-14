using System;
using System.Collections.Generic;
using System.Linq;
using Stealth.Objects;
using UnityEngine;
using Util.Geometry;
using Util.Geometry.Polygon;

namespace Stealth.Utils
{
    /// <summary>
    /// This class computes the polygon representing the area observed by a <see cref="GalleryCamera"/>.
    /// </summary>
    public class CameraVision
    {
        /// <summary>
        /// The camera we're calculating the visibility of.
        /// </summary>
        private GalleryCamera camera;

        /// <summary>
        /// The level polygon.
        /// </summary>
        private Polygon2DWithHoles level;
        
        /// <summary>
        /// The current state of the sweep line.
        /// </summary>
        private Line sweepLine;

        /// <summary>
        /// The final polygon to be reported.
        /// </summary>
        private Polygon2D result;

        /// <summary>
        /// The event queue.
        /// </summary>
        private Queue<Endpoint> eventQueue;

        /// <summary>
        /// Structure storing the segments currently being intersected.
        /// </summary>
        private StatusStructure intersectedSegments;

        /// <summary>
        /// Small value we use to avoid rounding-errors when computing the events.
        /// </summary>
        private float EPSILON = 0.2f;

        /// <summary>
        /// This class compares two <see cref="LineSegment"/>s by their distance
        /// to the camera along the sweep line.
        /// </summary>
        private class SegmentComparer : IComparer<LineSegment>
        {
            private Vector2 viewpoint;

            public SegmentComparer(Vector2 viewpoint)
            {
                this.viewpoint = viewpoint;
            }

            public int Compare(LineSegment a, LineSegment b)
            {
                if (a.Equals(b))
                {
                    return 0;
                }

                LineSegment aOriginal = a;
                LineSegment bOriginal = b;

                // We shorten the line segments, to avoid intersections
                // between shared endpoints
                a = a.Shorten(0.01f);
                b = b.Shorten(0.01f);

                // We assume the line segments do NOT intersect

                bool b1RightOfA = a.IsRightOf(b.Point1);
                bool b2RightOfA = a.IsRightOf(b.Point2);
                bool a1RightOfB = b.IsRightOf(a.Point1);
                bool a2RightOfB = b.IsRightOf(a.Point2);
                bool vRightOfA = a.IsRightOf(viewpoint);
                bool vRightOfB = b.IsRightOf(viewpoint);

                if (a1RightOfB != a2RightOfB && b1RightOfA != b2RightOfA)
                {
                    throw new GeomException($"{aOriginal} intersects {bOriginal}.");
                }

                // If B is sandwiched between the viewpoint and A
                // then A is behind B from the perspective of the viewpoint
                if (a1RightOfB == a2RightOfB && a1RightOfB != vRightOfB)
                {
                    return 1;
                }
                // If A is sandwiched between the viewpoint and B
                // then B is behind A from the perspective of the viewpoint
                if (b1RightOfA == b2RightOfA && b1RightOfA != vRightOfA)
                {
                    return -1;
                }

                return 1;
            }
        }

        /// <summary>
        /// Custom storage for intersected line segments using a linked list.
        /// Should ideally be replaced with a BST.
        /// </summary>
        private class StatusStructure
        {
            private LinkedList<LineSegment> list;
            private SegmentComparer comparer;

            public StatusStructure(SegmentComparer comparer)
            {
                this.list = new LinkedList<LineSegment>();
                this.comparer = comparer;
            }

            public LineSegment FindMin()
            {
                if (list.Count == 0)
                {
                    return null;
                }
                return list.First.Value;
            }
            public LineSegment FindMax()
            {
                if (list.Count == 0)
                {
                    return null;
                }
                return list.Last.Value;
            }

            public void Insert(LineSegment segment)
            {
                var node = list.First;
                while (node != null && comparer.Compare(segment, node.Value) > 0)
                {
                    node = node.Next;
                }
                if (node == null)
                {
                    list.AddLast(segment);
                }
                else
                {
                    list.AddBefore(node, segment);
                }
            }

            public bool Delete(LineSegment segment)
            {
                bool result = list.Remove(segment);
                return result;
            }
        }

        /// <summary>
        /// Creates a new <see cref="CameraVision"/> object.
        /// </summary>
        /// <param name="camera">The camera to compute the vision for.</param>
        /// <param name="level">The level the camera is placed in.</param>
        public CameraVision(GalleryCamera camera, LevelPolygon level)
        {
            this.camera = camera;
            this.level = level.TotalPolygon;

            intersectedSegments = new StatusStructure(
                new SegmentComparer(camera.transform.position));
        }

        private float GetCounterClockwiseAngle(Vector2 viewpoint, Line reference, Vector2 vertex, bool shortest = false)
        {
            Vector2 boundary = (reference.Point2 - reference.Point1);

            Vector2 viewpointToVertex = vertex - viewpoint;

            float angle = Vector2.SignedAngle(boundary, viewpointToVertex);
            // If the angle is only slightly negative we treat it as a rounding error
            if (-EPSILON < angle && angle < 0) angle = 0;
            return shortest || angle >= 0 ? angle : 360 + angle;
        }

        private List<Endpoint> CreateEndpoints(Polygon2DWithHoles polygon, GalleryCamera camera)
        {
            List<Endpoint> endpoints = new List<Endpoint>();

            Vector2 viewpoint = camera.transform.position;
            Line boundaryLine = camera.GetRightBoundary();

            // Add endpoints for all segments
            foreach (LineSegment segment in polygon.Segments)
            {
                Endpoint endpoint1 = new Endpoint
                {
                    Vertex = segment.Point1,
                    AngleToViewpoint = GetCounterClockwiseAngle(viewpoint, boundaryLine, segment.Point1),
                    Segment = segment,
                };
                Endpoint endpoint2 = new Endpoint
                {
                    Vertex = segment.Point2,
                    AngleToViewpoint = GetCounterClockwiseAngle(viewpoint, boundaryLine, segment.Point2),
                    Segment = segment,
                };

                // Determine which is the start endpoint and which is the end endpoint
                // We want to follow the segment in the direction of the sweep line
                // so we can determine the order by their angle to the viewpoint
                float angleDiff = GetCounterClockwiseAngle(viewpoint, boundaryLine, segment.Point2, true) - GetCounterClockwiseAngle(viewpoint, boundaryLine, segment.Point1, true);
                endpoint1.IsBegin = angleDiff > 0;
                endpoint2.IsBegin = !endpoint1.IsBegin;

                // Add endpoints to list
                endpoints.Add(endpoint1);
                endpoints.Add(endpoint2);
            }

            // Sort endpoints by angle
            endpoints.Sort();

            // Filter points that fall outside the vision cone
            //endpoints = endpoints.Where(e => e.AngleToViewpoint <= camera.FieldOfViewDegrees).ToList();

            return endpoints;
        }

        /// <summary>
        /// Finds all intersections between line segments of <paramref name="level"/> with sweep line.
        /// </summary>
        /// <remarks>
        /// The intersections are sorted by their distance to the viewpoint.
        /// </remarks>
        /// <param name="line">The line to intersect with.</param>
        /// <param name="level">The polygon to test intersections of.</param>
        /// <param name="transform">The transform of the viewpoint.</param>
        /// <returns>All intersections between line segments of <paramref name="level"/> with sweep line.</returns>
        private List<Tuple<LineSegment, Vector2>> FindIntersections(Line line, Polygon2DWithHoles level, Transform transform)
        {
            Vector2 pos = transform.position;
            Vector2 right = transform.right;

            List<Tuple<LineSegment, Vector2>> intersections = new List<Tuple<LineSegment, Vector2>>();
            foreach (LineSegment segments in level.Segments)
            {
                Vector2? intersection = segments.Intersect(line);
                if (intersection.HasValue)
                {
                    // The sweep line extends from the viewpoint to infinity
                    // We don't want intersections that are behind the viewpoint
                    bool correctDirection = Vector2.Dot(right, intersection.Value - pos) > 0;
                    if (correctDirection)
                    {
                        intersections.Add(Tuple.Create(segments, intersection.Value));
                    }
                }
            }

            // Sort intersections by distance to viewpoint
            intersections.Sort((a, b) =>
            {
                Vector2 intersectionA = a.Item2;
                Vector2 intersectionB = b.Item2;

                float distA = Vector2.Distance(pos, intersectionA);
                float distB = Vector2.Distance(pos, intersectionB);
                return (int)Mathf.Sign(distA - distB);
            });

            return intersections;
        }

        /// <summary>
        /// Computes the vision polygon.
        /// </summary>
        /// <param name="inLocalSpace">True, if the polygon should be returned in local space of the camera.</param>
        /// <returns>The vision polygon.</returns>
        public Polygon2D Compute(bool inLocalSpace = true)
        {
            result = new Polygon2D();
            result.AddVertex(camera.transform.position);

            // Initialize the event queue
            List<Endpoint> endpoints = CreateEndpoints(level, camera);
            // Filter endpoints that fall outside vision cone
            endpoints = endpoints.Where(e => e.AngleToViewpoint <= camera.FieldOfViewDegrees + EPSILON).ToList();
            // Create queue from list
            eventQueue = new Queue<Endpoint>(endpoints);

            // Initialize status
            // Start sweeping from the right boundary, counter-clockwise
            sweepLine = camera.GetRightBoundary();

            List<Tuple<LineSegment, Vector2>> intersections = FindIntersections(sweepLine, level, camera.transform);
            bool insertedSegment = false;
            for (int i = 0; i < intersections.Count; i++)
            {
                LineSegment segment = intersections[i].Item1;
                Vector2 point = intersections[i].Item2;

                // Ignore intersections with endpoints
                if (segment.IsEndpoint(point))
                {
                    continue;
                }

                if (!insertedSegment)
                {
                    // Insert the first intersection that isn't an endpoint into the polygon
                    result.AddVertex(point);
                    insertedSegment = true;
                }

                // Add segment into status
                intersectedSegments.Insert(segment);
            }

            // Handle all events
            while (eventQueue.Count > 0)
            {
                Endpoint e = eventQueue.Dequeue();
                HandleEvent(e);
            }

            // Find closest intersection and add vertex for it, if it is not an endpoint
            sweepLine = camera.GetLeftBoundary();
            intersections = FindIntersections(sweepLine, level, camera.transform);
            if (intersections.Count > 0)
            {
                if (!intersections[0].Item1.IsEndpoint(intersections[0].Item2))
                {
                    result.AddVertex(intersections[0].Item2);
                }
            }

            // Return polygon in local space of camera transform, if necessary
            return inLocalSpace ? result.ToLocalSpace(camera.transform) : result;
        }

        private void HandleEvent(Endpoint e)
        {
            // Update sweep line
            sweepLine = new Line(camera.transform.position, e.Vertex);

            // Find the segment which is currently in front
            LineSegment oldFront = intersectedSegments.FindMin();

            if (e.IsBegin) // The endpoint starts a segment
            {
                // Insert segment into status
                intersectedSegments.Insert(e.Segment);

                // If the inserted segment is in front of the old
                // insert a vertex at the intersection with the old
                LineSegment newFront = intersectedSegments.FindMin();
                // We can assume there are always segments in the status
                // as the camera lies in a bounded polygon
                // (i.e. oldFront should never be null)
                if (oldFront == null)
                {
                    throw new GeomException($"{nameof(intersectedSegments)} is empty. This should not happen.");
                }
                if (oldFront != newFront)
                {
                    Vector2? intersection = oldFront.Intersect(sweepLine);
                    if (!intersection.HasValue)
                    {
                        throw new GeomException($"Sweep line doesn't intersect {oldFront}.");
                    }
                    if (intersection.Value != e.Vertex && intersection.Value != result.Vertices.Last())
                    {
                        result.AddVertex(intersection.Value);
                    }
                    if (e.Vertex != result.Vertices.Last())
                    {
                        result.AddVertex(e.Vertex);
                    }
                }
            }
            else // The endpoint ends a segment
            {
                intersectedSegments.Delete(e.Segment);
                LineSegment newFront = intersectedSegments.FindMin();
                if (oldFront != newFront)
                {
                    if (e.Vertex != result.Vertices.Last())
                    {
                        result.AddVertex(e.Vertex);
                    }
                    Vector2? intersection = newFront.Intersect(sweepLine);
                    if (!intersection.HasValue)
                    {
                        throw new GeomException($"Sweep line doesn't intersect {newFront}.");
                    }
                    if (intersection.Value != e.Vertex && intersection.Value != result.Vertices.Last())
                    {
                        result.AddVertex(intersection.Value);
                    }
                }
            }
        }
    }
}
