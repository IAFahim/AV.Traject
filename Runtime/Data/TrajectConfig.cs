using System;
using System.Runtime.InteropServices;

namespace AV.Traject.Runtime.Data
{
    /// <summary>
    /// Base configuration data for all trajectories.
    /// Pure data struct - Burst compatible and serializable.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct TrajectConfig
    {
        /// <summary>Default duration in seconds.</summary>
        public float Duration;

        /// <summary>Creates a new trajectory configuration.</summary>
        public TrajectConfig(float duration = 1f)
        {
            Duration = duration;
        }

        /// <summary>Default configuration (1 second duration).</summary>
        public static TrajectConfig Default => new TrajectConfig(1f);
    }
}
