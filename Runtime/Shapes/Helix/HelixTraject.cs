using AV.Traject.Runtime.Core;
using AV.Traject.Runtime.Integration;
using Unity.Mathematics;
using UnityEngine;

namespace AV.Traject.Runtime.Shapes.Helix
{
    /// <summary>
    /// Helix (spiral) trajectory asset.
    /// Creates corkscrew/magic missile movements using Right and Up axes.
    /// </summary>
    [CreateAssetMenu(menuName = "AV/Traject/Helix", fileName = "HelixTraject")]
    public class HelixTraject : TrajectAsset
    {
        [Tooltip("Helix trajectory configuration.")]
        [SerializeField] public HelixConfig config = HelixConfig.Default;

        /// <summary>
        /// Evaluates the helix trajectory at normalized time t.
        /// </summary>
        public override void Evaluate(in TrajectBasis basis, float range, float t, out float3 pos)
        {
            HelixLogic.Evaluate(in basis, in range, in config, in t, out pos);
        }
    }
}
