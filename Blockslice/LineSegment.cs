using Assimp;

namespace Blockslice
{
    internal class LineSegment
    {
        public Face OriginatingFace { get; }
        public Vector3D Start { get; }
        public Vector3D End { get; }

        public LineSegment(Vector3D start, Vector3D end, Face originatingFace)
        {
            Start = start;
            End = end;
            OriginatingFace = originatingFace;
        }
    }
}