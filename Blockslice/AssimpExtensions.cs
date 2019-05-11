using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;

namespace Blockslice
{
    static class AssimpExtensions
    {
        public static float Dot(this Vector3D left, Vector3D right)
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }

        public static Vector3D Scale(this Vector3D left, float right)
        {
            return new Vector3D(left.X * right, left.Y * right, left.Z * right);
        }
    }
}
