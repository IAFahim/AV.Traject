using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace AV.Traject.Runtime.Core
{
    [BurstCompile]
    public static class TrajectStateLogic
    {
        /// <summary>
        /// Calculates the next time step based on delta time and speed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CalculateNextTime(
            in float currentElapsedTime,
            in float deltaTimeSeconds,
            in float speedMultiplier,
            out float nextElapsedTime)
        {
            nextElapsedTime = currentElapsedTime + (deltaTimeSeconds * speedMultiplier);
        }

        /// <summary>
        /// Handles the looping logic (wrapping time).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WrapTime(
            in float inputTime,
            in float duration,
            out float wrappedTime,
            out bool didLoop)
        {
            if (duration <= 0.0001f)
            {
                wrappedTime = 0f;
                didLoop = false;
                return;
            }

            wrappedTime = math.fmod(inputTime, duration);

            // Handle negative wrapping for reverse playback
            if (wrappedTime < 0f)
            {
                wrappedTime += duration;
            }

            // Heuristic: If the input was outside [0, duration], we looped
            didLoop = inputTime >= duration || inputTime < 0f;
        }

        /// <summary>
        /// Clamps time to duration and determines if completion occurred.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClampTime(
            in float inputTime,
            in float duration,
            out float clampedTime,
            out bool isFinished)
        {
            if (inputTime >= duration)
            {
                clampedTime = duration;
                isFinished = true;
                return;
            }

            if (inputTime <= 0f)
            {
                clampedTime = 0f;
                // If we hit 0 while reversing, we are technically "finished" with the playback
                isFinished = true;
                return;
            }

            clampedTime = inputTime;
            isFinished = false;
        }

        /// <summary>
        /// Calculates the normalized progress [0-1].
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CalculateNormalizedProgress(
            in float elapsedTime,
            in float duration,
            out float normalizedProgress)
        {
            if (duration <= 0.0001f)
            {
                normalizedProgress = 1f;
                return;
            }
            normalizedProgress = math.clamp(elapsedTime / duration, 0f, 1f);
        }

        /// <summary>
        /// Applies PingPong effect to normalized progress (0->1->0).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplyPingPong(
            in float inputProgress,
            out float pingPongProgress)
        {
            // Maps 0->1->0 (triangle wave)
            // Math: 1 - abs(2t - 1)
            pingPongProgress = 1f - math.abs(2f * inputProgress - 1f);
        }

        // --- Flag Helpers ---

        /// <summary>Checks if the IsPlaying flag is set.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static bool IsPlaying(in TrajectStatusFlags flags)
        {
            return (flags & TrajectStatusFlags.IsPlaying) != 0;
        }

        /// <summary>Checks if the IsLooping flag is set.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static bool IsLooping(in TrajectStatusFlags flags)
        {
            return (flags & TrajectStatusFlags.IsLooping) != 0;
        }

        /// <summary>Checks if the IsPingPong flag is set.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static bool IsPingPong(in TrajectStatusFlags flags)
        {
            return (flags & TrajectStatusFlags.IsPingPong) != 0;
        }

        /// <summary>Checks if the HasCompleted flag is set.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static bool IsCompleted(in TrajectStatusFlags flags)
        {
            return (flags & TrajectStatusFlags.HasCompleted) != 0;
        }

        /// <summary>Sets the Playing flag and clears Completed.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static void SetPlaying(in TrajectStatusFlags flags, out TrajectStatusFlags newFlags)
        {
            newFlags = flags | TrajectStatusFlags.IsPlaying;
            newFlags &= ~TrajectStatusFlags.HasCompleted;
        }

        /// <summary>Clears the Playing flag.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static void ClearPlaying(in TrajectStatusFlags flags, out TrajectStatusFlags newFlags)
        {
            newFlags = flags & ~TrajectStatusFlags.IsPlaying;
        }

        /// <summary>Resets time to zero.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static void ResetTime(out float newTime)
        {
            newTime = 0f;
        }

        /// <summary>Checks if normalized time is approximately at the end.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static bool IsNearEnd(in float currentTime, in float duration)
        {
            if (duration <= 0f) return true;
            float t = currentTime / duration;
            return t >= 0.999f;
        }

        /// <summary>Checks if time is approximately at the start.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static bool IsNearStart(in float currentTime)
        {
            return currentTime <= 0.001f;
        }
    }
}
