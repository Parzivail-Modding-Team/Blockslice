using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;

namespace Blockslice
{
    class Slicer
    {
        public static List<LineSegment> Slice(Mesh mesh, InfPlane plane)
        {
            var segments = new List<LineSegment>();
            foreach (var face in mesh.Faces)
            {
                if (!TryIntersect(mesh, face, plane, out var intersection)) continue;
                segments.Add(intersection);
            }

            return segments;
        }

        private static bool TryIntersect(Mesh mesh, Face face, InfPlane plane, out LineSegment intersection)
        {
            intersection = null;

            if (face.IndexCount != 3)
                throw new Exception("Face not triangular!");

            var ia = face.Indices[0];
            var ib = face.Indices[1];
            var ic = face.Indices[2];

            var a = mesh.Vertices[ia];
            var b = mesh.Vertices[ib];
            var c = mesh.Vertices[ic];

            var ab = CanIntersect(plane, a, b);
            var bc = CanIntersect(plane, b, c);
            var ca = CanIntersect(plane, c, a);

            var planeNormal = plane.Normal;
            var planePoint = plane.Point;

            if (ab && bc)
            {
                var start = IntersectPoint(a, b, planeNormal, planePoint);
                var end = IntersectPoint(b, c, planeNormal, planePoint);
                intersection = new LineSegment(start, end);
            }
            else if (bc && ca)
            {
                var start = IntersectPoint(b, c, planeNormal, planePoint);
                var end = IntersectPoint(c, a, planeNormal, planePoint);
                intersection = new LineSegment(start, end);
            }
            else if (ca && ab)
            {
                var start = IntersectPoint(c, a, planeNormal, planePoint);
                var end = IntersectPoint(a, b, planeNormal, planePoint);
                intersection = new LineSegment(start, end);
            }
            else
                return false;

            return true;
        }

        private static bool CanIntersect(InfPlane plane, Vector3D lineStart, Vector3D lineEnd)
        {
            var startAbove = IsPointAbovePlane(lineStart, plane);
            var endAbove = IsPointAbovePlane(lineEnd, plane);

            return (startAbove && !endAbove) || (!startAbove && endAbove);
        }

        private static bool IsPointAbovePlane(Vector3D point, InfPlane plane)
        {
            return plane.Normal.Dot(point - plane.Point) > 0;
        }

        private static Vector3D IntersectPoint(Vector3D lineStart, Vector3D lineEnd, Vector3D planeNormal,
            Vector3D planePoint)
        {
            var lineDirection = lineEnd - lineStart;
            lineDirection.Normalize();
            var diff = lineStart - planePoint;
            var prod1 = diff.Dot(planeNormal);
            var prod2 = lineDirection.Dot(planeNormal);
            var prod3 = prod1 / prod2;
            return lineStart - lineDirection.Scale(prod3);
        }
    }
}
