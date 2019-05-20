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

            var rH = (maxPoint.X - minPoint.X) / (maxPoint.Y - minPoint.Y);
            var rZ = (maxPoint.X - minPoint.X) / (maxPoint.Z - minPoint.Z);
            var width = 80;
            var height = (int)Math.Ceiling(width / rH);
            var length = (int)Math.Ceiling(width / rZ);

            var minProjectedPoint = new Vector3D(0, 0, 0);
            var maxProjectedPoint = new Vector3D(width, height, length);

            var sliceSize = (maxPoint.Z - minPoint.Z) / length;

            var planeNormal = new Vector3D(0, 0, 1);

            for (var planeY = minPoint.Z + 0.1f; planeY < maxPoint.Z + 0.1f; planeY += sliceSize)
            {
                var segments = Slicer.Slice(mesh, new InfPlane(new Vector3D(0, 0, planeY), planeNormal));

                var bmp = new Bitmap(width, height);
                using (var gfx = Graphics.FromImage(bmp))
                {
                    gfx.Clear(Color.White);

                    foreach (var segment in segments)
                    {
                        var projectedStart = Map(segment.Start, minPoint, maxPoint, minProjectedPoint, maxProjectedPoint);
                        var projectedEnd = Map(segment.End, minPoint, maxPoint, minProjectedPoint, maxProjectedPoint);

                        gfx.DrawLine(Pens.Black, projectedStart.X, projectedStart.Y, projectedEnd.X, projectedEnd.Y);
                    }

                    bmp.Save($"slices/slice-{slice:D5}.png");
                    slice++;
                }
                bmp.Dispose();
            }

            return 0;
        }

        private static Vector3D Map(Vector3D x, Vector3D fromMin, Vector3D fromMax, Vector3D toMin, Vector3D toMax)
        {
            return new Vector3D(Map(x.X, fromMin.X, fromMax.X, toMin.X, toMax.X), Map(x.Y, fromMin.Y, fromMax.Y, toMin.Y, toMax.Y), Map(x.Z, fromMin.Z, fromMax.Z, toMin.Z, toMax.Z));
        }

        private static float Map(float x, float fromMin, float fromMax, float toMin, float toMax)
        {
            return (x - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }
    }
}
