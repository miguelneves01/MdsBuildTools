using System;
using System.Collections.Generic;
using HarmonyLib;
using MdsBuildTools.Core;
using UnityEngine;

namespace MdsBuildTools.Patches
{
    /// <summary>
    /// Hooks into Placing.PlacingObjects (the game's furniture placement controller).
    ///
    /// Game flow (decompiled): LateUpdate → MoveObject → RotateObject → TryPlaceObject.
    /// MoveObject raycasts the mouse and sets _hoveringObject.transform.position = hit.point.
    /// We post-process that position (grid → alignment → floor), so the game's own collision
    /// check, placing command (undo/redo) and save data all see our corrected position.
    /// </summary>
    internal static class PlacementPatches
    {
        private const float AlignSearchRadius = 2f;
        private const float FloorProbeHeight = 5f;

        private static readonly List<Alignment.Span> SpansX = new List<Alignment.Span>();
        private static readonly List<Alignment.Span> SpansZ = new List<Alignment.Span>();

        public static void Apply(Harmony harmony)
        {
            harmony.Patch(GameRefs.MoveObject, postfix: new HarmonyMethod(typeof(PlacementPatches), nameof(MoveObjectPostfix)));
            harmony.Patch(GameRefs.RotateNormalObject, prefix: new HarmonyMethod(typeof(PlacementPatches), nameof(RotateNormalObjectPrefix)));
        }

        // ---- position --------------------------------------------------------------------------

        private static void MoveObjectPostfix(object __instance)
        {
            try
            {
                var obj = GameRefs.HoveringObject(__instance);
                if (obj == null) return;

                var cfg = Plugin.Cfg;
                var onWall = GameRefs.HoveringLayer(__instance) == LayerMask.NameToLayer("WallSurface");

                if (cfg.SnapActive)
                    SnapToGrid(obj.transform, GameRefs.DefaultParent(__instance), onWall);

                if (cfg.AlignActive && !onWall)
                    AlignToNeighbours(obj, GameRefs.SpawnedFurniture(__instance));

                if (cfg.Floor.Enabled.Value)
                    KeepAboveFloor(obj, GameRefs.FloorMask(__instance));
            }
            catch (Exception e)
            {
                Plugin.LogOnce("MoveObjectPostfix", e);
            }
        }

        /// <summary>
        /// Snaps in the room's local space (the room parent can be rotated). On floors/surfaces we
        /// snap X/Z and keep Y (surface height); on walls we snap the two axes lying in the wall plane.
        /// </summary>
        private static void SnapToGrid(Transform t, Transform? room, bool onWall)
        {
            var step = Plugin.Cfg.GridStepMetres;
            var local = room != null ? room.InverseTransformPoint(t.position) : t.position;

            if (!onWall)
            {
                local.x = SnapMath.Snap(local.x, step);
                local.z = SnapMath.Snap(local.z, step);
            }
            else
            {
                // Wall objects face away from the wall: their forward is the wall normal.
                var normal = room != null ? room.InverseTransformDirection(t.forward) : t.forward;
                local.y = SnapMath.Snap(local.y, step);
                if (Mathf.Abs(normal.x) > Mathf.Abs(normal.z)) local.z = SnapMath.Snap(local.z, step);
                else local.x = SnapMath.Snap(local.x, step);
            }

            t.position = room != null ? room.TransformPoint(local) : local;
        }

        /// <summary>Lines up the hovering object's edges/centre with nearby placed furniture (world X/Z).</summary>
        private static void AlignToNeighbours(GameObject obj, List<GameObject> placed)
        {
            if (!TryGetBounds(obj, out var self)) return;

            SpansX.Clear();
            SpansZ.Clear();
            foreach (var other in placed)
            {
                if (other == null || other == obj || !other.activeInHierarchy) continue;
                if (!TryGetBounds(other, out var b)) continue;
                if ((b.center - self.center).sqrMagnitude > AlignSearchRadius * AlignSearchRadius) continue;
                SpansX.Add(new Alignment.Span(b.min.x, b.max.x));
                SpansZ.Add(new Alignment.Span(b.min.z, b.max.z));
            }
            if (SpansX.Count == 0) return;

            var threshold = Plugin.Cfg.AlignThresholdMetres;
            var dx = Alignment.FindShift(new Alignment.Span(self.min.x, self.max.x), SpansX, threshold) ?? 0f;
            var dz = Alignment.FindShift(new Alignment.Span(self.min.z, self.max.z), SpansZ, threshold) ?? 0f;
            if (dx != 0f || dz != 0f)
                obj.transform.position += new Vector3(dx, 0f, dz);
        }

        /// <summary>
        /// Fix for objects whose mesh pivot isn't at their base: the game puts the pivot on the hit
        /// point, so part of the mesh ends up under the floor. Raise it until it rests on the floor.
        /// </summary>
        private static void KeepAboveFloor(GameObject obj, LayerMask floorMask)
        {
            if (!TryGetBounds(obj, out var b)) return;

            var origin = new Vector3(b.center.x, b.max.y + FloorProbeHeight, b.center.z);
            var floorY = Physics.Raycast(origin, Vector3.down, out var hit, float.MaxValue, floorMask) ? hit.point.y : 0f;

            var dy = FloorClamp.Correction(b.min.y, floorY);
            if (dy > 0f)
                obj.transform.position += new Vector3(0f, dy, 0f);
        }

        private static bool TryGetBounds(GameObject obj, out Bounds bounds)
        {
            var rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                bounds = rend.bounds;
                return true;
            }
            var col = obj.GetComponent<Collider>();
            bounds = col != null ? col.bounds : default;
            return col != null;
        }

        // ---- rotation --------------------------------------------------------------------------

        /// <summary>
        /// The game snaps scroll rotation to a hard-coded 45° (Shift = free 2° steps). When grid snapping
        /// is on we replace the 45° with the configured step; Shift-precise rotation is left untouched.
        /// </summary>
        private static bool RotateNormalObjectPrefix(object __instance, bool preciseRotation, float delta)
        {
            if (preciseRotation || !Plugin.Cfg.SnapActive) return true;

            try
            {
                var obj = GameRefs.HoveringObject(__instance);
                if (obj == null) return true;

                var parentYaw = GameRefs.DefaultParent(__instance)?.eulerAngles.y ?? 0f;
                var step = Plugin.Cfg.AngleStepDeg.Value;
                var relative = obj.transform.eulerAngles.y - parentYaw + Mathf.Sign(delta) * step;
                obj.transform.rotation = Quaternion.Euler(0f, SnapMath.SnapAngle(relative, step) + parentYaw, 0f);
                return false;
            }
            catch (Exception e)
            {
                Plugin.LogOnce("RotateNormalObjectPrefix", e);
                return true; // fall back to the game's behaviour
            }
        }
    }
}
