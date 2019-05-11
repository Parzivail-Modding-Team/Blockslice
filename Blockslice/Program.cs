using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using Assimp.Unmanaged;

namespace Blockslice
{
    class Program
    {
        static int Main(string[] args)
        {
            var importer = new AssimpContext();
            var scene = importer.ImportFile(args[0]);

            if (scene == null || !scene.HasMeshes)
                return -1;

            var mesh = scene.Meshes[0];

            var maxPoint = new Vector3D(float.MinValue, float.MinValue, float.MinValue);
            var minPoint = new Vector3D(float.MaxValue, float.MaxValue, float.MaxValue);

            foreach (var vertex in mesh.Vertices)
            {
                if (vertex.X > maxPoint.X)
                    maxPoint.X = vertex.X;
                if (vertex.Y > maxPoint.Y)
                    maxPoint.Y = vertex.Y;
                if (vertex.Z > maxPoint.Z)
                    maxPoint.Z = vertex.Z;

                if (vertex.X < minPoint.X)
                    minPoint.X = vertex.X;
                if (vertex.Y < minPoint.Y)
                    minPoint.Y = vertex.Y;
                if (vertex.Z < minPoint.Z)
                    minPoint.Z = vertex.Z;
            }

            Directory.CreateDirectory("slices");

            var slice = 0;
            for (var planeY = minPoint.Z + 0.1f; planeY < maxPoint.Z; planeY += 0.1f)
            {
                var segments = new List<LineSegment>();

                foreach (var face in mesh.Faces)
                {
                    if (!TryIntersect(mesh, face, planeY, out var intersection)) continue;
                    segments.Add(intersection);
                }

                var r = (maxPoint.X - minPoint.X) / (maxPoint.Y - minPoint.Y);
                var width = 512;
                var height = (int)(width / r);
                var bmp = new Bitmap(width, height);
                using (var gfx = Graphics.FromImage(bmp))
                {
                    gfx.Clear(Color.White);

                    foreach (var segment in segments)
                    {
                        var normalizedStart = MapPoint(segment.Start, minPoint, maxPoint);
                        var normalizedEnd = MapPoint(segment.End, minPoint, maxPoint);

                        gfx.DrawLine(Pens.Black, normalizedStart.X * width, normalizedStart.Y * height, normalizedEnd.X * width,
                            normalizedEnd.Y * height);
                    }

                    bmp.Save($"slices/slice-{slice:D5}.png");
                    slice++;
                }
            }

            return 0;
        }

        private static Vector3D MapPoint(Vector3D segmentStart, Vector3D minPoint, Vector3D maxPoint)
        {
            return new Vector3D(Map(segmentStart.X, minPoint.X, maxPoint.X), Map(segmentStart.Y, minPoint.Y, maxPoint.Y), Map(segmentStart.Z, minPoint.Z, maxPoint.Z));
        }

        private static float Map(float a, float min, float max)
        {
            return (a - min) / (max - min);
        }

        private static bool TryIntersect(Mesh mesh, Face face, float planeY, out LineSegment intersection)
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

            var ab = CanIntersect(planeY, a, b);
            var bc = CanIntersect(planeY, b, c);
            var ca = CanIntersect(planeY, c, a);

            var planeNormal = new Vector3D(0, 0, 1);
            var planePoint = new Vector3D(0, 0, planeY);

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

        private static bool CanIntersect(float planeY, Vector3D lineStart, Vector3D lineEnd)
        {
            return (lineStart.Z > planeY && lineEnd.Z < planeY) || (lineEnd.Z > planeY && lineStart.Z < planeY);
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
