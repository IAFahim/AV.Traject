using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using AV.Traject.Integration;
using AV.Traject.Runtime.Core;
using AV.Traject.Runtime.Extensions;
using AV.Traject.Runtime.Integration;

namespace AV.Traject.Editor
{
    /// <summary>
    /// Handles Scene View visualization for TrajectAuthoring.
    /// Uses Unity's [DrawGizmo] attribute to decouple drawing from the Component.
    /// </summary>
    public static class TrajectVisualizer
    {
        private static readonly Color PathColor = new Color(0f, 1f, 1f, 0.5f); // Cyan
        private static readonly Color GhostColor = new Color(1f, 0.5f, 0f, 0.8f); // Orange
        private const int SampleResolution = 60;

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawGizmos(TrajectAuthoring authoring, GizmoType gizmoType)
        {
            // Only draw full details if selected, otherwise just a subtle line
            bool isSelected = (gizmoType & GizmoType.Selected) != 0;
            if (!isSelected) return;

            var asset = authoring.Asset;
            if (asset == null) return;

            // 1. Get Context
            TrajectBasis basis = authoring.GetEditorBasis();
            float range = authoring.GetRange();

            // 2. Draw Path (The Shape)
            DrawPath(asset, basis, range);

            // 3. Draw Ghost (Current Time Position)
            // We read the state directly from the component
            var state = authoring.GetState();
            float progress = state.GetNormalizedProgress();

            // If playing in editor (via preview) or runtime
            if (Application.isPlaying || state.IsPlaying())
            {
                // Handled by runtime object moving, but we can draw a ghost if we want
            }
            else
            {
                // Draw 'Ghost' at current scrubbed time
                asset.Evaluate(in basis, range, progress, out float3 ghostPos);
                Gizmos.color = GhostColor;
                Gizmos.DrawWireSphere(ghostPos, 0.2f);
            }
        }

        private static void DrawPath(TrajectAsset asset, TrajectBasis basis, float range)
        {
            Handles.color = PathColor;
            float3? prevPos = null;

            for (int i = 0; i <= SampleResolution; i++)
            {
                float t = (float)i / SampleResolution;
                asset.Evaluate(in basis, range, t, out float3 pos);

                if (prevPos.HasValue)
                {
                    Handles.DrawLine(prevPos.Value, pos);
                }
                prevPos = pos;
            }

            // Draw Range Endpoint indicator
            asset.Evaluate(in basis, range, 1f, out float3 endPos);
            Handles.DrawWireDisc(endPos, basis.ForwardDirection, 0.1f);
        }
    }
}
