using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
            for (var planeY = minPoint.Z + 0.1f; planeY < maxPoint.Z; planeY += 1f)
            {
                var segments = Slicer.Slice(mesh, new InfPlane(new Vector3D(0, 0, planeY), new Vector3D(0, 0, 1)));

                var r = (maxPoint.X - minPoint.X) / (maxPoint.Y - minPoint.Y);
                var width = 512;
                var height = (int)(width / r);
                var bmp = new Bitmap(width, height);
                using (var gfx = Graphics.FromImage(bmp))
                {
                    gfx.SmoothingMode = SmoothingMode.HighQuality;

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
    }
}
