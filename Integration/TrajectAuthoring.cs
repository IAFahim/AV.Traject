using AV.Direction.Runtime.Attributes;
using AV.Traject.Runtime.Core;
using AV.Traject.Runtime.Extensions;
using AV.Traject.Runtime.Integration;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace AV.Traject.Integration
{
    /// <summary>
    /// A thin "Driver" component that bridges the gap between Unity GameObjects
    /// and the stateless Trajectory system.
    /// </summary>
    [AddComponentMenu("AV/Traject/Traject Authoring")]
    public class TrajectAuthoring : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("The trajectory shape asset.")]
        [SerializeField] protected TrajectAsset asset;

        [Tooltip("The range/distance of the trajectory in world units.")]
        [SerializeField] [LineRange(0, 0, 1)] protected float range = 10f;

        [Tooltip("The core runtime state. Edits here affect behavior directly.")]
        [SerializeField] protected TrajectState state = TrajectState.Default;

        [Header("Events")]
        public UnityEvent OnComplete;
        public UnityEvent OnLoop;

        // --- Internal State ---
        // We cache the starting point (Basis) so moving the object doesn't 
        // dragging the origin with it (the "Fly Away" fix).
        private TrajectBasis _startBasis;
        private bool _isInitialized = false;

        // --- Public Accessors ---
        public TrajectAsset Asset => asset;
        public float NormalizedTime => state.GetNormalizedProgress();
        public bool IsPlaying => state.IsPlaying();
        public float TimeScale { get => state.PlaybackSpeedMultiplier; set => state.PlaybackSpeedMultiplier = value; }

        private void OnEnable()
        {
            // Capture the "Start Point" whenever this object is activated.
            // This allows Pooling to work correctly.
            ResetOrigin();
        }

        private void OnValidate()
        {
            if (state.PlaybackTimer.Duration < 0f) state.PlaybackTimer.Duration = 0f;
        }

        private void Update()
        {
            if (asset == null) return;

            // 1. Tick the State
            bool boundaryHit = state.Tick(UnityEngine.Time.deltaTime, out float t);

            // 2. Event Handling
            if (boundaryHit)
            {
                if (state.IsLooping()) OnLoop?.Invoke();
                else OnComplete?.Invoke();
            }

            // 3. Application
            if (state.IsPlaying())
            {
                ApplyPosition(t);
            }
        }

        /// <summary>
        /// Resets the Trajectory origin to the object's CURRENT Transform position/rotation.
        /// Call this if you respawn/pool the object.
        /// </summary>
        public void ResetOrigin()
        {
            _startBasis = new TrajectBasis(transform.position, transform.forward, transform.right, transform.up);
            _isInitialized = true;
        }

        private void ApplyPosition(float t)
        {
            if (asset == null) return;

            // Get the correct basis (Runtime = Cached Start, Editor/Preview = Live Transform)
            TrajectBasis basis = GetBasis();
            
            asset.Evaluate(in basis, range, t, out float3 pos);
            transform.position = pos;
        }

        /// <summary>
        /// Logic to determine the reference frame.
        /// Runtime: Uses the cached start point to prevent feedback loops.
        /// Editor (Not Playing): Uses live transform for easy visualization.
        /// </summary>
        private TrajectBasis GetBasis()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return new TrajectBasis(transform.position, transform.forward, transform.right, transform.up);
            }
#endif
            // Safety: If somehow called before OnEnable, init to current.
            if (!_isInitialized) ResetOrigin();
            
            return _startBasis;
        }

        // --- API ---
        public void Play()
        {
            // Optional: Uncomment next line if Play() should always restart from the object's current location
            // ResetOrigin(); 
            state.Play();
        } 
        
        public void Pause() => state.Pause();
        
        public void Stop() 
        { 
            state.Stop(); 
            ApplyPosition(0f); 
        }
        
        public void Rewind() => state.Rewind();

        /// <summary>
        /// Returns the evaluated position at current time without applying it.
        /// </summary>
        public float3 GetCurrentPosition()
        {
            if (asset == null) return transform.position;

            float t = state.GetNormalizedProgress();
            TrajectBasis basis = GetBasis();

            asset.Evaluate(in basis, range, t, out float3 pos);
            return pos;
        }

#if UNITY_EDITOR
        // API for Editor Visualizers
        public TrajectState GetState() => state;
        public float GetRange() => range;
        public TrajectBasis GetEditorBasis() => GetBasis();
#endif
    }
}