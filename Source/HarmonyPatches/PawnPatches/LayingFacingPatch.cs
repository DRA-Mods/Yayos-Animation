using HarmonyLib;
using Verse;
using YayoAnimation.Data;

namespace YayoAnimation.HarmonyPatches.PawnPatches;

[HotSwappable]
[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.LayingFacing))]
public static class LayingFacingPatch
{
    public static void Postfix(PawnRenderer __instance, ref Rot4 __result, Pawn ___pawn)
    {
        __result = ___pawn.GetData().fixedRot ?? __result;
    }
}