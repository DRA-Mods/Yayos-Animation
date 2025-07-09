using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace YayoAnimation.HarmonyPatches.PawnPatches;

[HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawCarriedWeapon))]
public static class CarriedWeaponDrawing
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
    {
        var locFields = new HashSet<FieldInfo>
        {
            AccessTools.DeclaredField(typeof(PawnRenderUtility), nameof(PawnRenderUtility.EqLocNorth)),
            AccessTools.DeclaredField(typeof(PawnRenderUtility), nameof(PawnRenderUtility.EqLocEast)),
            AccessTools.DeclaredField(typeof(PawnRenderUtility), nameof(PawnRenderUtility.EqLocSouth)),
            AccessTools.DeclaredField(typeof(PawnRenderUtility), nameof(PawnRenderUtility.EqLocWest)),
        };
        var drawEquipmentAiming = MethodUtil.MethodOf(PawnRenderUtility.DrawEquipmentAiming);

        var addWiggleToCarryPos = MethodUtil.MethodOf(AimingPatch.AddWiggleToCarryPos);
        var addWiggleToAngle = MethodUtil.MethodOf(AimingPatch.AddWiggleToAngle);

        var patchedAimingCalls = 0;
        var patchedFields = 0;

        foreach (var ci in instr)
        {
            // If DrawEquipmentAiming, then the last value on stack is float (angle) - replace it with ours
            if (ci.Calls(drawEquipmentAiming))
            {
                yield return new CodeInstruction(OpCodes.Call, addWiggleToAngle);

                patchedAimingCalls++;
            }

            yield return ci;

            // If any of the 4 static fields was loaded, call our method to add wiggle to it
            if (ci.opcode == OpCodes.Ldsfld && ci.operand is FieldInfo field && locFields.Contains(field))
            {
                yield return new CodeInstruction(OpCodes.Call, addWiggleToCarryPos);

                patchedFields++;
            }
        }

        const int expectedPatchedAimingCalls = 1;
        const int expectedPatchedFields = 4;

        if (patchedAimingCalls != expectedPatchedAimingCalls)
            Log.Error($"[{Core.ModName}] - patched incorrect number of calls to PawnRenderUtility.DrawEquipmentAiming (expected: {expectedPatchedAimingCalls}, patched: {patchedAimingCalls}) for method {baseMethod.GetNameWithNamespace()}");
        if (patchedFields != expectedPatchedFields)
            Log.Error($"[{Core.ModName}] - patched incorrect number of calls to PawnRenderUtility directional rotation fields (expected: {expectedPatchedFields}, patched: {patchedFields}) for method {baseMethod.GetNameWithNamespace()}");
    }
}