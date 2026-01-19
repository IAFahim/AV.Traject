using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace AV.Traject.Runtime.Core
{
    /// <summary>
    /// High-performance mathematical utilities for trajectory calculations.
    /// All methods are Burst-compatible and use strict in/out patterns.
    /// </summary>
    [BurstCompile]
    public static class TrajectMath
    {
        /// <summary>
        /// The height multiplier for parabolic calculations.
        /// A parabola with formula 4*t*(1-t) reaches height 1 at t=0.5.
        /// </summary>
        public const float ParabolaHeightFactor = 4f;

        /// <summary>
        /// Resolves a position in world space based on the trajectory basis and local offsets.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResolvePositionInBasis(
            in TrajectBasis basis,
            in float forwardOffset,
            in float rightOffset,
            in float upOffset,
            out float3 resultPosition)
        {
            resultPosition = basis.OriginPosition
                           + (basis.ForwardDirection * forwardOffset)
                           + (basis.RightDirection * rightOffset)
                           + (basis.UpDirection * upOffset);
        }

        /// <summary>
        /// Calculates a parabolic arc value (0 at start, 1 at peak, 0 at end).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CalculateParabola(in float normalizedTime, out float resultHeight)
        {
            // Parabola formula: heightFactor * t * (1 - t)
            resultHeight = ParabolaHeightFactor * normalizedTime * (1f - normalizedTime);
        }

        /// <summary>
        /// Calculates both sine and cosine efficiently.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SinCos(in float angle, out float sine, out float cosine)
        {
            math.sincos(angle, out sine, out cosine);
        }

        /// <summary>
        /// Safe arc-sine with clamping to avoid NaN results.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeAsin(in float value, out float result)
        {
            result = math.asin(math.clamp(value, -1f, 1f));
        }
    }
}
