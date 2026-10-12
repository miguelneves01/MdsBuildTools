using HarmonyLib;
using MdsBuildTools.Core;
using UnityEngine;

namespace MdsBuildTools.Patches
{
    /// <summary>
    /// Central place where game hooks are wired. The target classes/methods in Assembly-CSharp are still
    /// unknown (need to decompile the game), so each feature exposes a game-agnostic Apply* helper and
    /// the Harmony patches get added here once targets are identified.
    /// </summary>
    internal static class PatchRegistry
    {
        public static void ApplyAll(Harmony harmony)
        {
#if HAS_GAME
            // TODO(game-dll): harmony.PatchAll(typeof(PlacementPatches));
            // TODO(game-dll): harmony.PatchAll(typeof(WallPatches));
#endif
            Plugin.Log.LogInfo("No game hooks wired yet (skeleton build).");
        }
    }

    /// <summary>Game-agnostic transforms the placement patch will call on the preview object each frame.</summary>
    internal static class PlacementLogic
    {
        public static Vector3 SnapPosition(Vector3 pos)
        {
            if (!Plugin.Cfg.SnapActive) return pos;
            var step = Plugin.Cfg.GridStepMetres;
            // Y is left to the game (floor/surface height); only snap on the floor plane.
            return new Vector3(SnapMath.Snap(pos.x, step), pos.y, SnapMath.Snap(pos.z, step));
        }

        public static Quaternion SnapRotation(Quaternion rot)
        {
            if (!Plugin.Cfg.SnapActive) return rot;
            var e = rot.eulerAngles;
            return Quaternion.Euler(e.x, SnapMath.SnapAngle(e.y, Plugin.Cfg.AngleStepDeg.Value), e.z);
        }

        /// <summary>Raises <paramref name="obj"/> so its lowest renderer point rests on <paramref name="floorY"/>.</summary>
        public static void ClampAboveFloor(GameObject obj, float floorY)
        {
            if (!Plugin.Cfg.Floor.Enabled.Value) return;

            var renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            var dy = FloorClamp.Correction(bounds.min.y, floorY);
            if (dy > 0f) obj.transform.position += new Vector3(0f, dy, 0f);
        }
    }
}
