using Assimp;

namespace Blockslice
{
    internal class InfPlane
    {
        public Vector3D Point { get; }
        public Vector3D Normal { get; }

        public InfPlane(Vector3D point, Vector3D normal)
        {
            Point = point;
            Normal = normal;
        }
    }
}