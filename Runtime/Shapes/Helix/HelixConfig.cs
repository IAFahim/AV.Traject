using System;
using System.Runtime.InteropServices;

namespace AV.Traject.Runtime.Shapes.Helix
{
    /// <summary>
    /// Configuration data for helix (spiral) trajectory movement.
    /// Pure data struct - Burst compatible.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct HelixConfig
    {
        /// <summary>Radius of the spiral.</summary>
        public float Radius;

        /// <summary>Number of full rotations along the path.</summary>
        public float Frequency;

        /// <summary>Initial rotation phase in degrees.</summary>
        public float Phase;

        /// <summary>
        /// How the radius changes over time.
        /// None = Constant radius (cylinder)
        /// Parabolic = Starts at 0, expands, returns to 0 (rugby ball shape)
        /// </summary>
        public EnvelopeType EnvelopeType;

        /// <summary>Creates a new helix configuration.</summary>
        public HelixConfig(float radius = 1f, float frequency = 2f, float phase = 0f, EnvelopeType envelopeType = EnvelopeType.None)
        {
            Radius = radius;
            Frequency = frequency;
            Phase = phase;
            EnvelopeType = envelopeType;
        }

        /// <summary>Default configuration (1 unit radius, 2 rotations, cylindrical).</summary>
        public static HelixConfig Default => new HelixConfig(1f, 2f, 0f, EnvelopeType.None);

        /// <summary>Cone shape (expands from 0 to full radius).</summary>
        public static HelixConfig Cone => new HelixConfig(1f, 2f, 0f, EnvelopeType.EaseOut);

        /// <summary>Rugby ball/American football shape (0 -> max -> 0).</summary>
        public static HelixConfig Football => new HelixConfig(1f, 2f, 0f, EnvelopeType.Parabolic);
    }
}
