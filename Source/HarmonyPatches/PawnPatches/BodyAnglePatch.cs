using HarmonyLib;
using Verse;
using YayoAnimation.Data;

namespace YayoAnimation.HarmonyPatches.PawnPatches;

[HotSwappable]
[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.BodyAngle))]
public static class BodyAnglePatch
{
    // Can be threaded

    public static void Postfix(PawnRenderer __instance, ref float __result, Pawn ___pawn)
    {
        var pdd = ___pawn.GetData();
        __result += pdd.angleOffset;
    }
}