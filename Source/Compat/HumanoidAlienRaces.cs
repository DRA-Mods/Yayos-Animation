#if IDEOLOGY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace yayoAni.Compat;

public static class HumanoidAlienRaces
{
    public static bool harPatchActive = false;

    public static void SetHarPatch(bool state)
    {
        if (!Core.usingHar || harPatchActive == state)
            return;

        var method = AccessTools.Method("AlienRace.HarmonyPatches:DrawAddons");
        var patch = AccessTools.Method(
            typeof(AlienRace_DrawAddon_Transpiler),
            nameof(AlienRace_DrawAddon_Transpiler.Transpiler));

        try
        {
            if (state)
                Core.harmony.Patch(method, transpiler: new HarmonyMethod(patch, 0));
            else
                Core.harmony.Unpatch(method, patch);

            harPatchActive = state;
        }
        catch (Exception e)
        {
            Log.ErrorOnce($"[Yayo's Animation] - Encountered error applying HAR patch. Exception caught:\n{e}", -1266925295);
        }
    }
    
    
    public static class AlienRace_DrawAddon_Transpiler
    {
        private static float DotReplacement(Quaternion identity, Quaternion b) => b.eulerAngles.y / (2f * Mathf.Rad2Deg);

        private static float SimpleDotReplacement(Quaternion identity, Quaternion b) => b.eulerAngles.y;

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var acosTarget = AccessTools.Method(typeof(Mathf), nameof(Mathf.Acos));
            var dotTarget = AccessTools.Method(typeof(Quaternion), nameof(Quaternion.Dot));

            CodeInstruction targetInstruction = null;
            var removedMult = false;
            var instr = instructions.ToArray();

            for (var index = 0; index < instr.Length; index++)
            {
                var ci = instr[index];

                if (ci.opcode == OpCodes.Call && ci.operand is MethodInfo method)
                {
                    if (method == acosTarget)
                        continue;
                    if (method == dotTarget)
                        targetInstruction = ci;
                }
                else if (ci.opcode == OpCodes.Ldc_R4 && 
                         index + 4 < instr.Length && 
                         ci.operand is 2f &&
                         instr[index + 1].opcode == OpCodes.Mul &&
                         instr[index + 2].opcode == OpCodes.Ldc_R4 &&
                         instr[index + 2].operand is Mathf.Rad2Deg &&
                         instr[index + 3].opcode == OpCodes.Mul)
                {
                    index += 3;
                    removedMult = true;
                    continue;
                }

                yield return ci;
            }

            if (targetInstruction == null)
                throw new Exception($"[Yayo's Animation] - Failed patching HAR, could not find {nameof(Quaternion)}.{nameof(Quaternion.Dot)} method.");

            targetInstruction.operand = AccessTools.Method(
                typeof(AlienRace_DrawAddon_Transpiler),
                removedMult
                    ? nameof(SimpleDotReplacement)
                    : nameof(DotReplacement));
        }
    }
}
#endif