using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using YayoAnimation.Data;

namespace YayoAnimation.HarmonyPatches.PawnPatches;

[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.ParallelGetPreRenderResults))]
public static class ParallelGetPreRenderResultsPatch
{
    public static void Prefix(PawnRenderer __instance, Pawn ___pawn, ref Vector3 drawLoc, ref Rot4? rotOverride)
    {
        var data = ___pawn.GetData();

        Yayo.CheckAni(
            ___pawn,
            ref drawLoc,
            rotOverride ?? (___pawn.GetPosture() == PawnPosture.Standing || ___pawn.Crawling ? ___pawn.Rotation : __instance.LayingFacing()),
            data);

        // drawLoc += data.posOffset;
        rotOverride = data.fixedRot ?? rotOverride;
    }

    private static float GetPawnRotationOffset(PawnRenderer instance) => instance.pawn.GetData().angleOffset;

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // We need to turn
        // float num = ((posture == PawnPosture.Standing) ? 0f : this.BodyAngle(pawnRenderFlags));
        // into
        // float num = ((posture == PawnPosture.Standing) ? pawn.GetData().posOffset : this.BodyAngle(pawnRenderFlags));

        foreach (var ci in instructions)
        {
            yield return ci;

            if (ci.opcode == OpCodes.Ldc_R4 && ci.operand is 0.0f)
            {
                // Replace the instruction with 'this'
                ci.opcode = OpCodes.Ldarg_0;
                // Call our method with 'this' as the argument
                yield return new CodeInstruction(OpCodes.Call, MethodUtil.MethodOf(GetPawnRotationOffset));
            }
        }
    }
}