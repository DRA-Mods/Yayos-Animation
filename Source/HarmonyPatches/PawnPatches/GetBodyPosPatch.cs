using HarmonyLib;
using UnityEngine;
using Verse;
using YayoAnimation.Data;

namespace YayoAnimation.HarmonyPatches.PawnPatches;

[HotSwappable]
[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.GetBodyPos))]
public static class GetBodyPosPatch
{
    // Can be threaded

    public static void Postfix(PawnRenderer __instance, ref Vector3 __result, Vector3 drawLoc, ref bool showBody, Pawn ___pawn)
    {
        var pdd = ___pawn.GetData();
        __result += pdd.posOffset;
        if (pdd.forcedShowBody) showBody = true;
    }
}