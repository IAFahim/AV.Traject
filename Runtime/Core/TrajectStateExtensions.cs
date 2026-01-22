using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using Variable.Timer;

namespace AV.Traject.Runtime.Core
{
    /// <summary>
    /// Timer-specific extension methods for <see cref="TrajectState"/>.
    /// These provide direct access to Timer functionality without conflicting with TrajectExtensions.
    /// </summary>
    public static class TrajectStateExtensions
    {
        // --- Time Management ---

        /// <summary>
        /// Advances the trajectory playback timer by delta time with playback speed.
        /// </summary>
        /// <param name="self">The trajectory state.</param>
        /// <param name="deltaTime">The time elapsed since the last frame.</param>
        /// <returns>True if the trajectory COMPLETED (reached duration); false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TickAndCheckComplete(ref this TrajectState self, float deltaTime)
        {
            // Apply playback speed multiplier
            float adjustedDeltaTime = deltaTime * self.PlaybackSpeedMultiplier;

            // Handle reverse playback by negating delta time
            if (adjustedDeltaTime < 0f)
            {
                // For reverse playback, manually decrement and check if we reached 0
                self.PlaybackTimer.Current = math.max(0f, self.PlaybackTimer.Current + adjustedDeltaTime);
                return TrajectStateLogic.IsNearStart(self.PlaybackTimer.Current);
            }

            // Normal forward playback - use Timer tick
            return self.PlaybackTimer.TickAndCheckComplete(adjustedDeltaTime);
        }

        /// <summary>
        /// Advances the trajectory timer and handles looping behavior.
        /// </summary>
        /// <param name="self">The trajectory state.</param>
        /// <param name="deltaTime">The time elapsed since the last frame.</param>
        /// <returns>True if a loop occurred (time wrapped around); false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TickAndHandleLoop(ref this TrajectState self, float deltaTime)
        {
            TrajectStateLogic.CalculateNextTime(
                in self.PlaybackTimer.Current,
                in deltaTime,
                in self.PlaybackSpeedMultiplier,
                out float nextTime);

            TrajectStateLogic.WrapTime(
                in nextTime,
                in self.PlaybackTimer.Duration,
                out float wrappedTime,
                out bool didLoop);

            self.PlaybackTimer.Current = wrappedTime;
            return didLoop;
        }

        /// <summary>
        /// Resets the trajectory playback timer to the beginning.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetPlayback(ref this TrajectState self)
        {
            self.PlaybackTimer.Reset();
        }

        /// <summary>
        /// Resets the trajectory playback timer to the beginning and sets a new duration.
        /// </summary>
        /// <param name="self">The trajectory state.</param>
        /// <param name="newDuration">The new duration in seconds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetWithDuration(ref this TrajectState self, float newDuration)
        {
            self.PlaybackTimer = new Timer(newDuration, 0f);
        }

        /// <summary>
        /// Seeks the playback timer to a specific time.
        /// </summary>
        /// <param name="self">The trajectory state.</param>
        /// <param name="time">The time to seek to (clamped between 0 and duration).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SeekTo(ref this TrajectState self, float time)
        {
            TimerLogic.Clamp(time, 0f, self.PlaybackTimer.Duration, out self.PlaybackTimer.Current);
        }

        // --- Query Methods ---

        /// <summary>
        /// Gets the current elapsed time in seconds.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetElapsedTime(this in TrajectState self)
        {
            return self.PlaybackTimer.Current;
        }

        /// <summary>
        /// Gets the total duration in seconds.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetTotalDuration(this in TrajectState self)
        {
            return self.PlaybackTimer.Duration;
        }

        /// <summary>
        /// Gets the normalized progress with PingPong effect applied [0-1-0].
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetPingPongProgress(this in TrajectState self)
        {
            TrajectStateLogic.CalculateNormalizedProgress(
                in self.PlaybackTimer.Current,
                in self.PlaybackTimer.Duration,
                out float progress);
            TrajectStateLogic.ApplyPingPong(in progress, out float pingPongProgress);
            return pingPongProgress;
        }

        /// <summary>
        /// Checks if the trajectory playback is at or near the end.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearEnd(this in TrajectState self)
        {
            return TrajectStateLogic.IsNearEnd(self.PlaybackTimer.Current, self.PlaybackTimer.Duration);
        }

        /// <summary>
        /// Checks if the trajectory playback is at or near the start.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearStart(this in TrajectState self)
        {
            return TrajectStateLogic.IsNearStart(self.PlaybackTimer.Current);
        }

        /// <summary>
        /// Checks if the trajectory playback timer is complete (reached duration).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsComplete(this in TrajectState self)
        {
            return self.PlaybackTimer.IsFull();
        }

        /// <summary>
        /// Gets the ratio of completion [0-1] using the Timer's built-in ratio.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetRatio(this in TrajectState self)
        {
            return self.PlaybackTimer.GetRatio();
        }
    }
}
