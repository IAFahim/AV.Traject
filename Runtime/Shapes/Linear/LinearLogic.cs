using System.Runtime.CompilerServices;
using AV.Eases.Runtime;
using AV.Traject.Runtime.Core;
using Unity.Burst;
using Unity.Mathematics;

namespace AV.Traject.Runtime.Shapes.Linear
{
    /// <summary>
    /// Core logic for linear trajectory evaluation.
    /// Burst-compiled static methods with strict in/out patterns.
    /// </summary>
    [BurstCompile]
    public static class LinearLogic
    {
        /// <summary>
        /// Evaluates a linear trajectory at normalized time t.
        /// Movement is along the Forward axis of the basis with optional easing.
        /// </summary>
        /// <param name="basis">The coordinate basis for evaluation.</param>
        /// <param name="range">The distance along the forward direction.</param>
        /// <param name="easeConfig">Configuration parameters (easing type).</param>
        /// <param name="t">Normalized time [0, 1].</param>
        /// <param name="position">Output: World space position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Evaluate(
            in TrajectBasis basis,
            in float range,
            in EaseConfig easeConfig,
            in float t,
            out float3 position)
        {
            // 1. Apply easing to time (controls acceleration along the line)
            easeConfig.Evaluate(t, out float easedT);

            // 2. Calculate forward distance
            float forwardDist = range * easedT;

            // 3. Synthesize position (Origin + Forward * distance)
            // Note: Linear has 0 deviation in Right/Up axes
            TrajectMath.ResolvePositionInBasis(in basis, in forwardDist, 0f, 0f, out position);
        }
    }
}
