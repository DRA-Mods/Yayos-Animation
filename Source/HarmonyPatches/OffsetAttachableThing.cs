using HarmonyLib;
using UnityEngine;
using Verse;
using YayoAnimation.Data;

namespace YayoAnimation.HarmonyPatches;

[HarmonyPatch(typeof(AttachableThing), nameof(AttachableThing.DrawPos), MethodType.Getter)]
public static class OffsetAttachableThing
{
    public static void Postfix(ref Vector3 __result, Thing ___parent)
    {
        if (___parent is Pawn pawn)
            __result += pawn.GetData().posOffset;
    }
}