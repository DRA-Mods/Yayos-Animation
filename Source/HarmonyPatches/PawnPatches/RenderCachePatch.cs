// using HarmonyLib;
// using Verse;
//
// namespace YayoAnimation.HarmonyPatches;
//
// [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderCache))]
// public static class RenderCachePatch
// {
//     // The old patch was just a prefix that replaced the whole method.
//     // All it did was set `skipPatch` to true, the rest was copy of vanilla code.
//
//     private static void Prefix() => RenderPawnInternalPatch.skipPatch = true;
//
//     private static void Finalizer() => RenderPawnInternalPatch.skipPatch = false;
// }