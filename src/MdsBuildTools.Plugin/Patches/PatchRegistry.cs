using HarmonyLib;

namespace MdsBuildTools.Patches
{
    /// <summary>Resolves game members and applies Harmony patches. Missing members disable the hooks instead of crashing.</summary>
    internal static class PatchRegistry
    {
        public static void ApplyAll(Harmony harmony)
        {
            var missing = GameRefs.Resolve();
            if (missing.Count > 0)
            {
                Plugin.Log.LogError("Game code changed, placement hooks disabled. Missing: " + string.Join(", ", missing));
                return;
            }

            PlacementPatches.Apply(harmony);
            Plugin.Log.LogInfo("Placement hooks applied (grid, alignment, floor, rotation).");
        }
    }
}
