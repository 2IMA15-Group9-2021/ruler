﻿using System;
using System.Collections.Generic;
using System.Linq;
using DotsAndPolygons;
using General.Model;
using UnityEngine;
using UnityEngine.UI;
using Util.Geometry;
using Random = UnityEngine.Random;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoProperty

namespace DotsAndPolygons
{
    using static HelperFunctions;

    public class DotsController3 : DotsController
    {
        private bool _showTrapDecomLines = false;

        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                _showTrapDecomLines = !_showTrapDecomLines;
                if (_showTrapDecomLines)
                    ShowTrapDecomLines();
                else
                    RemoveTrapDecomLines();
            }

            // User clicked a point and is drawing line from starting point
            if (FirstPoint == null) return;
            // User is holding mouse button
            if (Input.GetMouseButton(0))
            {
                // update edge endpont
                Camera mainCamera = Camera.main;
                if (mainCamera == null) return;
                Vector3 pos = mainCamera.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);

                SetDrawingLinePosition(1, pos);
            }
            else // User let go of mouse button
            {
                if (SecondPoint == null)
                {
                    print("SecondPoint was null");
                }
                else if (FirstPoint == SecondPoint)
                {
                    print("FirstPoint was same as SecondPoint");
                }
                // use isInside method to see of middle of line lies in a face
                else if (Faces.Where(it => it?.OuterComponentHalfEdges != null).Any(face =>
                    IsInside(
                        face.OuterComponentVertices.Select(it => it.Coordinates).ToList(),
                        new LineSegment(FirstPoint.Coordinates, SecondPoint.Coordinates).Midpoint
                    )
                ))
                {
                    print($"Line between {FirstPoint} and {SecondPoint} lies inside face");
                }
                else if (EdgeAlreadyExists(Edges, FirstPoint, SecondPoint))
                {
                    print("edge between first and second point already exists");
                }
                else if (InterSEGtsAny(
                    new LineSegment(FirstPoint.Coordinates, SecondPoint.Coordinates),
                    Edges.Select(edge => edge.Segment)
                ))
                {
                    print(
                        $"Edge between first and second point intersects something ({FirstPoint.Coordinates.x}, {FirstPoint.Coordinates.y}), ({SecondPoint.Coordinates.x}, {SecondPoint.Coordinates.y})");
                }
                else
                {
                    AddVisualEdge(FirstPoint, SecondPoint);

                    bool faceCreated = AddEdge(FirstPoint, SecondPoint, CurrentPlayer, HalfEdges, Vertices,
                        GameMode.GameMode2, this, root);

                    RemoveTrapDecomLines();
                    ShowTrapDecomLines();

                    if (!faceCreated)
                    {
                        CurrentPlayer = CurrentPlayer == 1 ? 2 : 1;
                        currentPlayerText.text = $"Go Player {CurrentPlayer}!";
                        currentPlayerText.gameObject.GetComponentInParent<Image>().color =
                            CurrentPlayer == 1 ? Color.blue : Color.red;
                    }

                    CheckSolution();
                }

                FirstPoint = null;
                SecondPoint = null;
                p1Line.enabled = false;
                p2Line.enabled = false;
            }
        }


        private void RemoveTrapDecomLines()
        {
            foreach (GameObject line in lines)
            {
                Destroy(line);
            }

            lines.Clear();
        }

        private void ShowTrapDecomLines()
        {
            if (!_showTrapDecomLines) return;
            faces = extract_faces(root.LeftChild, new List<TrapFace>(), 0);

            foreach (TrapFace face in faces)
            {
                GameObject upper = UnityTrapDecomLine.CreateUnityTrapDecomLine(face.Upper.Segment, this);
                if (upper != null)
                    lines.Add(upper);

                GameObject downer = UnityTrapDecomLine.CreateUnityTrapDecomLine(face.Downer.Segment, this);
                if (downer != null)
                    lines.Add(downer);

                GameObject left = UnityTrapDecomLine.CreateUnityTrapDecomLine(face.Left, this);
                if (left != null)
                    lines.Add(left);

                GameObject right = UnityTrapDecomLine.CreateUnityTrapDecomLine(face.Right, this);
                if (right != null)
                    lines.Add(right);
            }
        }

        public bool CheckArea() => Math.Abs((TotalAreaP1 + TotalAreaP2) - HullArea) < .001f;

        public override void CheckSolution()
        {
            if (CheckHull() && CheckArea())
            {
                FinishLevel();
            }
        }

        public override void InitLevel()
        {
            base.InitLevel();

            IEnumerable<IDotsVertex> dots = GetVerticesInConvexPosition(numberOfDots, false, radius: 3f);

            foreach (IDotsVertex dotsVertex in dots)
            {
                GameObject dot = Instantiate(
                    dotPrefab,
                    new Vector3(dotsVertex.Coordinates.x, dotsVertex.Coordinates.y, 0),
                    Quaternion.identity
                );
                dot.transform.parent = transform;
                InstantObjects.Add(dot);
            }


            faces.Add(frame);
            LineSegment left = new LineSegment(new Vector2(-6, 3), new Vector2(-6, -3));
            LineSegment upper = new LineSegment(new Vector2(-6, 3), new Vector2(6, 3));
            LineSegment right = new LineSegment(new Vector2(6, 3), new Vector2(6, -3));
            LineSegment lower = new LineSegment(new Vector2(6, -3), new Vector2(-6, -3));
            root = new TrapDecomRoot(frame);
        }
    }
}