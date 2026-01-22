using AV.Eases.Runtime;
using AV.Traject.Runtime.Core;
using AV.Traject.Runtime.Integration;
using Unity.Mathematics;
using UnityEngine;

namespace AV.Traject.Runtime.Shapes.Arc
{
    /// <summary>
    /// Arc (parabolic) trajectory asset.
    /// Creates curved jump/lob trajectories using the Up axis.
    /// </summary>
    [CreateAssetMenu(menuName = "AV/Traject/Arc", fileName = "ArcTraject")]
    public class ArcTraject : TrajectAsset
    {
        [Tooltip("Peak height of the arc in world units.")]
        [SerializeField] public float ArcHeightUnits = 2f;

        [Range(0f, 1f)]
        [Tooltip("Shifts the peak position: 0.5 = center, < 0.5 = earlier, > 0.5 = later.")]
        [SerializeField] public float PeakBias = 0.5f;

        [Tooltip("Easing function for forward motion along the path.")]
        [SerializeField] public EEase ForwardEase = EEase.Linear;

        /// <summary>
        /// Evaluates the arc trajectory at normalized time t.
        /// </summary>
        public override void Evaluate(in TrajectBasis basis, float range, float t, out float3 pos)
        {
            // Create EaseConfig from the EEase enum (leading3Bit = 0 for basic easing)
            var easeConfig = EaseConfig.New(ForwardEase, 0);
            ArcLogic.Evaluate(in basis, in range, in ArcHeightUnits, in PeakBias, in easeConfig, in t, out pos);
        }
    }
}
