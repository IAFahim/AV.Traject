using System;
using System.Runtime.InteropServices;
using AV.Eases.Runtime;

namespace AV.Traject.Runtime.Shapes.Linear
{
    /// <summary>
    /// Configuration data for linear trajectory movement.
    /// Pure data struct - Burst compatible.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct LinearConfig
    {
        /// <summary>Easing function for forward movement.</summary>
        public EEase Ease;

        /// <summary>Creates a new linear configuration.</summary>
        public LinearConfig(EEase ease = EEase.Linear)
        {
            Ease = ease;
        }

        /// <summary>Default configuration (linear easing).</summary>
        public static LinearConfig Default => new LinearConfig(EEase.Linear);
    }
}
