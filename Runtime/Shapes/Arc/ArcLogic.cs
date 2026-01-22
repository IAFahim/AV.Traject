using System.Runtime.CompilerServices;
using AV.Eases.Runtime;
using AV.Traject.Runtime.Core;
using Unity.Burst;
using Unity.Mathematics;

namespace AV.Traject.Runtime.Shapes.Arc
{
    /// <summary>
    /// Core logic for arc (parabolic) trajectory evaluation.
    /// Burst-compiled static methods with strict in/out patterns.
    /// </summary>
    [BurstCompile]
    public static class ArcLogic
    {
        /// <summary>
        /// Evaluates an arc trajectory at normalized time t.
        /// Creates a parabolic jump/lob using the Up axis of the basis.
        /// </summary>
        /// <param name="basis">The coordinate basis for evaluation.</param>
        /// <param name="range">The distance along the forward direction.</param>
        /// <param name="height">Peak height of the arc.</param>
        /// <param name="peakBias">Shifts the peak forward or backward (0.5 = center).</param>
        /// <param name="easeConfig">Easing configuration for forward motion.</param>
        /// <param name="t">Normalized time [0, 1].</param>
        /// <param name="position">Output: World space position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Evaluate(
            in TrajectBasis basis,
            in float range,
            in float height,
            in float peakBias,
            in EaseConfig easeConfig,
            in float t,
            out float3 position)
        {
            // 1. Forward motion with easing
            easeConfig.Evaluate(t, out float easedT);
            float forwardDist = range * easedT;

            // 2. Vertical arc (parabolic)
            // Apply peak bias by remapping t before calculating parabola
            float biasedT = ApplyPeakBias(in t, in peakBias);

            // Calculate parabolic curve: 4 * t * (1 - t) = 0 -> 1 -> 0
            TrajectMath.CalculateParabola(in biasedT, out float parabola);

            // Scale by configured height
            float upOffset = parabola * height;

            // 3. Synthesize position (Forward distance + Up arc)
            TrajectMath.ResolvePositionInBasis(in basis, in forwardDist, 0f, in upOffset, out position);
        }

        /// <summary>
        /// Applies peak bias to normalized time.
        /// Shifts the peak of the parabola along the timeline.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ApplyPeakBias(in float t, in float peakBias)
        {
            // If peak is centered, no bias needed
            if (math.abs(peakBias - 0.5f) < 0.001f)
            {
                return t;
            }

            // Remap t to shift the peak
            // This creates a skewed parabola where the peak occurs at peakBias
            float biasOffset = (peakBias - 0.5f) * 2f; // Range: [-1, 1]
            float biasedT = t + biasOffset * (t - t * t); // Nonlinear remapping

            return math.clamp(biasedT, 0f, 1f);
        }
    }
}
