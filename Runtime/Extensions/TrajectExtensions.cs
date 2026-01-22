using System.Runtime.CompilerServices;
using AV.Traject.Runtime.Core;
using AV.Traject.Runtime.Integration;
using Unity.Mathematics;
using Variable.Timer;

namespace AV.Traject.Runtime.Extensions
{
    /// <summary>
    /// Extension methods providing convenient adapters for trajectory data structures.
    /// These methods decompose structs into primitives and delegate to Logic layer.
    /// </summary>
    public static class TrajectExtensions
    {
        #region TrajectBasis Extensions

        /// <summary>
        /// Creates a position in basis space from forward/right/up components.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetPosition(
            in this TrajectBasis basis,
            float forwardOffset,
            float rightOffset,
            float upOffset)
        {
            TrajectMath.ResolvePositionInBasis(
                in basis,
                in forwardOffset,
                in rightOffset,
                in upOffset,
                out float3 result);
            return result;
        }

        /// <summary>
        /// Gets the position at a specific distance along the forward direction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetPositionAtDistance(in this TrajectBasis basis, float distance)
        {
            return basis.OriginPosition + (basis.ForwardDirection * distance);
        }

        /// <summary>
        /// Rotates a direction from local basis space to world space.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 LocalToWorld(in this TrajectBasis basis, float3 localDirection)
        {
            return basis.OriginPosition
                   + (basis.ForwardDirection * localDirection.z)
                   + (basis.RightDirection * localDirection.x)
                   + (basis.UpDirection * localDirection.y);
        }

        /// <summary>
        /// Gets the distance from this basis's origin to a world point.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceTo(in this TrajectBasis basis, float3 worldPoint)
        {
            return math.distance(basis.OriginPosition, worldPoint);
        }

        /// <summary>
        /// Creates a new basis offset from this one.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TrajectBasis OffsetBy(in this TrajectBasis basis, float3 worldOffset)
        {
            return new TrajectBasis(
                basis.OriginPosition + worldOffset,
                basis.ForwardDirection,
                basis.RightDirection,
                basis.UpDirection
            );
        }

        /// <summary>
        /// Creates a new basis at a specific distance along the forward direction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TrajectBasis AtDistance(in this TrajectBasis basis, float distance)
        {
            float3 newOrigin = basis.GetPositionAtDistance(distance);
            return new TrajectBasis(
                newOrigin,
                basis.ForwardDirection,
                basis.RightDirection,
                basis.UpDirection
            );
        }

        #endregion

        #region TrajectState Extensions

        // --- Properties (Hiding Bitwise Logic) ---

        /// <summary>Checks if the trajectory is currently playing.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPlaying(in this TrajectState s)
        {
            return TrajectStateLogic.IsPlaying(in s.StatusFlags);
        }

        /// <summary>Checks if the trajectory is set to loop.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLooping(in this TrajectState s)
        {
            return TrajectStateLogic.IsLooping(in s.StatusFlags);
        }

        /// <summary>Checks if the trajectory is ping-ponging.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPingPong(in this TrajectState s)
        {
            return TrajectStateLogic.IsPingPong(in s.StatusFlags);
        }

        /// <summary>Checks if the trajectory completed this frame.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasCompleted(in this TrajectState s)
        {
            return TrajectStateLogic.IsCompleted(in s.StatusFlags);
        }

        /// <summary>Gets the current playback time scale.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetTimeScale(in this TrajectState s)
        {
            return s.PlaybackSpeedMultiplier;
        }

        // --- Mutation Methods (Using ref) ---

        /// <summary>Starts playback from the current time.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Play(ref this TrajectState s)
        {
            TrajectStateLogic.SetPlaying(in s.StatusFlags, out s.StatusFlags);
        }

        /// <summary>Pauses playback at the current time.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pause(ref this TrajectState s)
        {
            TrajectStateLogic.ClearPlaying(in s.StatusFlags, out s.StatusFlags);
        }

        /// <summary>Stops playback and resets to the beginning.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Stop(ref this TrajectState s)
        {
            s.PlaybackTimer.Reset();
            TrajectStateLogic.ClearPlaying(in s.StatusFlags, out s.StatusFlags);
        }

        /// <summary>Resets time to zero without changing playback state.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Rewind(ref this TrajectState s)
        {
            s.PlaybackTimer.Reset();
        }

        /// <summary>Sets the time scale multiplier.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTimeScale(ref this TrajectState s, float timeScale)
        {
            s.PlaybackSpeedMultiplier = timeScale;
        }

        /// <summary>Enables slow-motion (0.5x speed).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SlowMo(ref this TrajectState s)
        {
            s.PlaybackSpeedMultiplier = 0.5f;
        }

        /// <summary>Enables fast-forward (2x speed).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FastForward(ref this TrajectState s)
        {
            s.PlaybackSpeedMultiplier = 2f;
        }

        /// <summary>Enables reverse playback.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reverse(ref this TrajectState s)
        {
            s.PlaybackSpeedMultiplier = -math.abs(s.PlaybackSpeedMultiplier);
        }

        /// <summary>Resumes forward playback at normal speed.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NormalSpeed(ref this TrajectState s)
        {
            s.PlaybackSpeedMultiplier = 1f;
        }

        /// <summary>
        /// Main update method. Advances the trajectory state.
        /// Returns TRUE if the trajectory just completed or looped this frame.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Tick(ref this TrajectState s, float deltaTime, out float normalizedTime)
        {
            // 1. Validation / Early Exit
            if (!s.IsPlaying())
            {
                // Calculate normalized time for rendering anyway
                TrajectStateLogic.CalculateNormalizedProgress(
                    in s.PlaybackTimer.Current,
                    in s.PlaybackTimer.Duration,
                    out normalizedTime);
                return false;
            }

            // 2. Calculate Next Time
            TrajectStateLogic.CalculateNextTime(
                in s.PlaybackTimer.Current,
                in deltaTime,
                in s.PlaybackSpeedMultiplier,
                out float nextTime);

            bool eventTriggered = false;
            bool isLooping = s.IsLooping();

            // 3. Handle Boundary Logic
            if (isLooping)
            {
                TrajectStateLogic.WrapTime(
                    in nextTime,
                    in s.PlaybackTimer.Duration,
                    out s.PlaybackTimer.Current,
                    out eventTriggered);

                // Reset completed flag (will be set if boundary reached this frame)
                s.StatusFlags &= ~TrajectStatusFlags.HasCompleted;
                if (eventTriggered)
                {
                    s.StatusFlags |= TrajectStatusFlags.HasCompleted;
                }
            }
            else
            {
                TrajectStateLogic.ClampTime(
                    in nextTime,
                    in s.PlaybackTimer.Duration,
                    out s.PlaybackTimer.Current,
                    out bool isFinished);

                if (isFinished)
                {
                    // Stop playing
                    s.StatusFlags &= ~TrajectStatusFlags.IsPlaying;
                    s.StatusFlags |= TrajectStatusFlags.HasCompleted;
                    eventTriggered = true;
                }
            }

            // 4. Calculate Normalized Progress
            TrajectStateLogic.CalculateNormalizedProgress(
                in s.PlaybackTimer.Current,
                in s.PlaybackTimer.Duration,
                out normalizedTime);

            // 5. Apply PingPong if enabled
            if (s.IsPingPong())
            {
                TrajectStateLogic.ApplyPingPong(in normalizedTime, out normalizedTime);
            }

            return eventTriggered;
        }

        /// <summary>
        /// Gets the normalized progress [0-1], accounting for PingPong if enabled.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetNormalizedProgress(in this TrajectState state)
        {
            TrajectStateLogic.CalculateNormalizedProgress(
                in state.PlaybackTimer.Current,
                in state.PlaybackTimer.Duration,
                out float progress);

            if (state.IsPingPong())
            {
                TrajectStateLogic.ApplyPingPong(in progress, out progress);
            }

            return progress;
        }

        /// <summary>
        /// Checks if state is at the start (with tolerance).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAtStart(in this TrajectState state)
        {
            return TrajectStateLogic.IsNearStart(in state.PlaybackTimer.Current);
        }

        /// <summary>
        /// Checks if state is at the end (with tolerance).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAtEnd(in this TrajectState state)
        {
            return TrajectStateLogic.IsNearEnd(in state.PlaybackTimer.Current, in state.PlaybackTimer.Duration);
        }

        /// <summary>
        /// Gets the current progress as a percentage (0-100).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetProgress(in this TrajectState state)
        {
            return state.GetNormalizedProgress() * 100f;
        }

        /// <summary>
        /// Sets elapsed time in seconds (clamped to [0, TotalDuration]).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTime(ref this TrajectState state, float timeInSeconds)
        {
            state.PlaybackTimer.Current = math.clamp(timeInSeconds, 0f, state.PlaybackTimer.Duration);
        }

        #endregion

        #region Float3 Extensions (Trajectory Helpers)

        /// <summary>
        /// Creates a direction vector from two points.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 DirectionTo(in this float3 from, float3 to)
        {
            return math.normalizesafe(to - from);
        }

        /// <summary>
        /// Gets a point at a specific distance along a direction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetPointAt(in this float3 origin, float3 direction, float distance)
        {
            return origin + (math.normalizesafe(direction) * distance);
        }

        /// <summary>
        /// Interpolates between two points.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 LerpTo(in this float3 from, float3 to, float t)
        {
            return math.lerp(from, to, t);
        }

        #endregion

        #region Trajectory Evaluation Helpers

        /// <summary>
        /// Evaluates a trajectory asset with a basis and range.
        /// Extension method adapter for convenient calling.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Evaluate(
            this TrajectAsset asset,
            in TrajectBasis basis,
            float range,
            float normalizedTime,
            out float3 resultPosition)
        {
            asset.Evaluate(in basis, range, normalizedTime, out resultPosition);
        }

        /// <summary>
        /// Evaluates a trajectory at a specific world time (seconds).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EvaluateAtTime(
            this TrajectAsset asset,
            in TrajectBasis basis,
            float range,
            float timeInSeconds,
            float duration,
            out float3 pos)
        {
            float t = timeInSeconds / math.max(duration, 0.001f);
            asset.Evaluate(in basis, range, t, out pos);
        }

        #endregion
    }
}
