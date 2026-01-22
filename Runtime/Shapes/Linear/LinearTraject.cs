using AV.Eases.Runtime;
using AV.Traject.Runtime.Core;
using AV.Traject.Runtime.Integration;
using Unity.Mathematics;
using UnityEngine;

namespace AV.Traject.Runtime.Shapes.Linear
{
    /// <summary>
    /// Linear trajectory asset.
    /// Creates straight-line movement with optional easing.
    /// </summary>
    [CreateAssetMenu(menuName = "AV/Traject/Linear", fileName = "LinearTraject")]
    public class LinearTraject : TrajectAsset
    {
        [Tooltip("Easing function for the movement.")]
        [SerializeField] public EEase MovementEase = EEase.Linear;

        /// <summary>
        /// Evaluates the linear trajectory at normalized time t.
        /// </summary>
        public override void Evaluate(in TrajectBasis basis, float range, float t, out float3 pos)
        {
            // Create EaseConfig from the EEase enum (leading3Bit = 0 for basic easing)
            var easeConfig = EaseConfig.New(MovementEase, 0);
            LinearLogic.Evaluate(in basis, in range, in easeConfig, in t, out pos);
        }
    }
}
