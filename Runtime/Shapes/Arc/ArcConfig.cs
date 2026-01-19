using System;
using System.Runtime.InteropServices;
using AV.Eases.Runtime;
using Unity.Mathematics;

namespace AV.Traject.Runtime.Shapes.Arc
{
    /// <summary>
    /// Configuration data for arc (parabolic) trajectory movement.
    /// Pure data struct - Burst compatible.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct ArcConfig
    {
        /// <summary>Peak height of the arc relative to the straight line.</summary>
        public float Height;

        /// <summary>
        /// Shifts the peak forward or backward along the path.
        /// 0.5 = center, 0.25 = earlier peak, 0.75 = later peak.
        /// </summary>
        public float PeakBias;

        /// <summary>Easing function for forward motion along the path.</summary>
        public EEase ForwardEase;

        /// <summary>Creates a new arc configuration.</summary>
        public ArcConfig(float height = 2f, float peakBias = 0.5f, EEase forwardEase = EEase.Linear)
        {
            Height = height;
            PeakBias = math.clamp(peakBias, 0f, 1f);
            ForwardEase = forwardEase;
        }

        /// <summary>Default configuration (2 unit height, centered peak).</summary>
        public static ArcConfig Default => new ArcConfig(2f, 0.5f, EEase.Linear);
    }
}
