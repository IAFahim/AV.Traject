using System;
using System.Runtime.InteropServices;
using Variable.Timer;

namespace AV.Traject.Runtime.Core
{
    [Serializable]
    [Flags]
    public enum TrajectStatusFlags : byte
    {
        None = 0,
        IsPlaying = 1 << 0,
        IsLooping = 1 << 1,
        IsPingPong = 1 << 2,
        HasCompleted = 1 << 3
    }

    /// <summary>
    /// Pure runtime state for trajectory playback.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct TrajectState
    {
        /// <summary>The playback timer tracking elapsed time.</summary>
        public Timer PlaybackTimer;

        /// <summary>Playback speed multiplier (1.0 = Normal, -1.0 = Reverse).</summary>
        public float PlaybackSpeedMultiplier;

        /// <summary>Current status flags.</summary>
        public TrajectStatusFlags StatusFlags;

        public static TrajectState Default => new TrajectState
        {
            PlaybackTimer = new Timer(1f, 0f),
            PlaybackSpeedMultiplier = 1f,
            StatusFlags = TrajectStatusFlags.IsPlaying
        };
    }
}
