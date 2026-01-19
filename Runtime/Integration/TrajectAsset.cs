using System.Runtime.CompilerServices;
using AV.Traject.Runtime.Core;
using Unity.Mathematics;
using UnityEngine;

namespace AV.Traject.Runtime.Integration
{
    /// <summary>
    /// Base ScriptableObject for trajectory assets.
    /// Acts as a container for trajectory configuration data.
    /// Specific trajectory types inherit and provide their config.
    /// </summary>
    public abstract class TrajectAsset : ScriptableObject
    {
        /// <summary>
        /// Evaluates the trajectory at normalized time t [0, 1].
        /// Must be implemented by derived types.
        /// </summary>
        /// <param name="basis">The coordinate basis for evaluation.</param>
        /// <param name="range">The range/distance of the trajectory.</param>
        /// <param name="t">Normalized time [0, 1].</param>
        /// <param name="pos">Output position in world space.</param>
        public abstract void Evaluate(in TrajectBasis basis, float range, float t, out float3 pos);

        /// <summary>
        /// Samples multiple points along the trajectory for visualization.
        /// Default implementation uses uniform sampling.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Sample(in TrajectBasis basis, float range, int segmentCount, ref float3[] positions)
        {
            // Validate segment count
            if (segmentCount <= 0)
            {
                positions = System.Array.Empty<float3>();
                return;
            }

            // Allocate or resize array
            if (positions == null || positions.Length != segmentCount + 1)
            {
                positions = new float3[segmentCount + 1];
            }

            // Sample points
            for (int i = 0; i <= segmentCount; i++)
            {
                float t = (float)i / segmentCount;
                Evaluate(in basis, range, t, out positions[i]);
            }
        }

        /// <summary>
        /// Gets the starting position of the trajectory.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void GetStart(in TrajectBasis basis, float range, out float3 pos)
        {
            Evaluate(in basis, range, 0f, out pos);
        }

        /// <summary>
        /// Gets the ending position of the trajectory.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void GetEnd(in TrajectBasis basis, float range, out float3 pos)
        {
            Evaluate(in basis, range, 1f, out pos);
        }
    }
}
