using System.Runtime.CompilerServices;
using DualWield;
using HarmonyLib;
using UnityEngine;
using Verse;
using YayoAnimation.HarmonyPatches.PawnPatches;

namespace YayoAnimation.Compat;

public static class DualWieldCompat
{
    private static bool IsPatchActive() => ModLister.AnyModActiveNoSuffix(["MemeGoddess.DualWield"]);

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ThingWithComps GetOffhandWeapons(Pawn pawn)
    {
        pawn.equipment.TryGetOffHandEquipment(out var equipment);
        return equipment;
    }

    [HarmonyPatch("DualWield.Harmony.PawnRenderUtility_DrawEquipmentAndApparelExtras", "PrepareMainHand")]
    public static class PatchMainHandDraw
    {
        private static bool Prepare() => IsPatchActive();

        private static void Prefix(Pawn pawn, ref Vector3 drawLoc, ref float aimAngle)
        {
            if (!AimingPatch.HasPawn || pawn.stances.curStance is not Stance_Busy stance || stance.neverAimWeapon || !stance.focusTarg.IsValid)
                return;

            drawLoc += AimingPatch.AddWiggleToAimingRotation(new Vector3(0f, 0f, 0.4f + pawn.equipment.Primary.def.equippedDistanceOffset), aimAngle, false)
                       * pawn.ageTracker.CurLifeStage.equipmentDrawDistanceFactor;

            aimAngle = AimingPatch.AddWiggleToAngle(aimAngle);
        }
    }

    [HarmonyPatch("DualWield.Harmony.PawnRenderUtility_DrawEquipmentAndApparelExtras", "PrepareOffhandDraw")]
    public static class PatchOffhandDraw
    {
        private static bool Prepare() => IsPatchActive();

        private static void Prefix(Pawn pawn, ref Vector3 drawLoc, ref float aimAngle)
        {
            if (!AimingPatch.HasPawn || pawn.stances.curStance is not Stance_Busy stance || stance.neverAimWeapon || !stance.focusTarg.IsValid)
                return;

            drawLoc += AimingPatch.AddWiggleToAimingRotation(new Vector3(0f, 0f, 0.4f + pawn.equipment.Primary.def.equippedDistanceOffset), aimAngle, true)
                       * pawn.ageTracker.CurLifeStage.equipmentDrawDistanceFactor;

            aimAngle = AimingPatch.AddWiggleToAngle(aimAngle);
        }
    }
}