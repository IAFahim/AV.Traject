using System.Runtime.CompilerServices;
using AV.Traject.Runtime.Core;
using Unity.Burst;
using Unity.Mathematics;

namespace AV.Traject.Runtime.Shapes.Helix
{
    /// <summary>
    /// Core logic for helix (spiral) trajectory evaluation.
    /// Burst-compiled static methods with strict in/out patterns.
    /// </summary>
    [BurstCompile]
    public static class HelixLogic
    {
        /// <summary>
        /// Evaluates a helix trajectory at normalized time t.
        /// Creates a spiral/corkscrew using the Right and Up axes of the basis.
        /// </summary>
        /// <param name="basis">The coordinate basis for evaluation.</param>
        /// <param name="range">The distance along the forward direction.</param>
        /// <param name="config">Configuration parameters (radius, frequency, phase, envelope).</param>
        /// <param name="t">Normalized time [0, 1].</param>
        /// <param name="position">Output: World space position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Evaluate(
            in TrajectBasis basis,
            in float range,
            in HelixConfig config,
            in float t,
            out float3 position)
        {
            // 1. Calculate radius with envelope modulation
            // Envelope allows the spiral to expand/contract along the path
            float envelope = CalculateEnvelope(in t, in config.EnvelopeType);
            float radius = config.Radius * envelope;

            // 2. Calculate angle (rotates over time)
            // t * Frequency * 2PI = total rotation
            // Phase allows offsetting the starting angle
            float angle = (t * config.Frequency * math.PI * 2f) + math.radians(config.Phase);

            // 3. Calculate sine and cosine efficiently
            TrajectMath.SinCos(in angle, out float sine, out float cosine);

            // 4. Calculate deviations in Right/Up plane
            // Right axis gets Cosine, Up axis gets Sine
            // This creates a circle in the Right-Up plane
            float rightOffset = cosine * radius;
            float upOffset = sine * radius;

            // 5. Linear forward progression
            float forwardDist = range * t;

            // 6. Synthesize position (Forward + Circular deviation in Right/Up)
            TrajectMath.ResolvePositionInBasis(in basis, in forwardDist, in rightOffset, in upOffset, out position);
        }

        /// <summary>
        /// Calculates envelope modulation for helix radius.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalculateEnvelope(in float t, in EnvelopeType envelopeType)
        {
            switch (envelopeType)
            {
                case EnvelopeType.None:
                    return 1f;

                case EnvelopeType.Linear:
                    return t;

                case EnvelopeType.EaseIn:
                    return t * t;

                case EnvelopeType.EaseOut:
                    float tt1 = 1f - t;
                    return 1f - (tt1 * tt1);

                case EnvelopeType.EaseInOut:
                    return t < 0.5f ? 2f * t * t : 1f - math.pow(-2f * t + 2f, 2f) * 0.5f;

                case EnvelopeType.Parabolic:
                    // 4 * t * (1 - t) = 0 -> 1 -> 0
                    return 4f * t * (1f - t);

                case EnvelopeType.HalfParabolic:
                    // 2 * t - t² = 0 -> 1
                    return 2f * t - (t * t);

                default:
                    return 1f;
            }
        }
    }

    /// <summary>
    /// Envelope function types for trajectory modulation.
    /// </summary>
    public enum EnvelopeType
    {
        None,           // No modulation (constant)
        Linear,         // Linear ramp from 0 to 1
        EaseIn,         // Smooth start
        EaseOut,        // Smooth end
        EaseInOut,      // Smooth start and end
        Parabolic,      // Arc: 0->1->0
        HalfParabolic   // Arc: 0->1
    }
}
