using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace AV.Traject.Runtime.Core
{
    /// <summary>
    /// Defines the local coordinate space for trajectory calculations.
    /// Marked readonly for Burst optimization and safety with in parameters.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct TrajectBasis
    {
        public readonly float3 OriginPosition;
        public readonly float3 ForwardDirection;
        public readonly float3 RightDirection;
        public readonly float3 UpDirection;

        /// <summary>Constructor for explicit basis creation.</summary>
        public TrajectBasis(float3 originPosition, float3 forwardDirection, float3 rightDirection, float3 upDirection)
        {
            OriginPosition = originPosition;
            ForwardDirection = forwardDirection;
            RightDirection = rightDirection;
            UpDirection = upDirection;
        }

        // Implicit operators are allowed for convenience if they don't contain complex logic
        public static implicit operator TrajectBasis(quaternion rotation)
        {
            return new TrajectBasis(
                float3.zero,
                math.mul(rotation, new float3(0, 0, 1)),
                math.mul(rotation, new float3(1, 0, 0)),
                math.mul(rotation, new float3(0, 1, 0))
            );
        }

        public static TrajectBasis Identity => new TrajectBasis(
            float3.zero,
            new float3(0, 0, 1),
            new float3(1, 0, 0),
            new float3(0, 1, 0)
        );
    }
}
