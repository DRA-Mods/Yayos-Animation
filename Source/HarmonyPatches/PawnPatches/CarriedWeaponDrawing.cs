using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
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

        HashSet<OpCode> ldLoc =
        [
            OpCodes.Ldloc_0,
            OpCodes.Ldloc_1,
            OpCodes.Ldloc_2,
            OpCodes.Ldloc_3,
            OpCodes.Ldloc_S,
            OpCodes.Ldloc
        ];

        var drawEquipmentAiming = MethodUtil.MethodOf(PawnRenderUtility.DrawEquipmentAiming);

        var addWiggleToCarryPos = MethodUtil.MethodOf(AimingPatch.AddWiggleToCarryPos);
        var addWiggleToCarryPosCounter = MethodUtil.MethodOf(AimingPatch.AddWiggleToCarryPosCounter);
        var addWiggleToAngle = MethodUtil.MethodOf(AimingPatch.AddWiggleToAngle);

        var patchedAimingCalls = 0;
        var patchedFields = 0;

        // Check if Dual Wield is loaded, and if it's later in the mod list, if it's already applied the patch.
        var dualWieldPatchField = ModsConfig.IsActive("MemeGoddess.DualWield") 
            ? AccessTools.TypeByName("DualWield.Harmony.PawnRenderUtility_DrawCarriedWeapon")?.Field("PatchApplied") 
            : null;
        var dualWieldLoaded = dualWieldPatchField != null && dualWieldPatchField.GetValue(null) is true;
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
            if (!dualWieldLoaded && ci.opcode == OpCodes.Ldsfld && ci.operand is FieldInfo field && locFields.Contains(field))
            {
                yield return new CodeInstruction(OpCodes.Call, addWiggleToCarryPos);

                patchedFields++;
            }
            else if (dualWieldLoaded && ldLoc.Contains(ci.opcode) && ci.operand is LocalBuilder localBuilder && localBuilder.LocalType == typeof(Vector3))
            {
                yield return new CodeInstruction(OpCodes.Call, patchedFields % 2 == 0 ? addWiggleToCarryPos : addWiggleToCarryPosCounter);
                patchedFields++;
            }
        }

        int expectedPatchedAimingCalls = dualWieldLoaded ? 2 : 1;
        int expectedPatchedFields = dualWieldLoaded ? 2 : 4;

        if (patchedAimingCalls != expectedPatchedAimingCalls)
            Log.Error($"[{Core.ModName}] - patched incorrect number of calls to PawnRenderUtility.DrawEquipmentAiming (expected: {expectedPatchedAimingCalls}, patched: {patchedAimingCalls}) for method {baseMethod.GetNameWithNamespace()}");
        if (patchedFields != expectedPatchedFields)
            Log.Error($"[{Core.ModName}] - patched incorrect number of calls to PawnRenderUtility directional rotation fields (expected: {expectedPatchedFields}, patched: {patchedFields}) for method {baseMethod.GetNameWithNamespace()}");
    }
}