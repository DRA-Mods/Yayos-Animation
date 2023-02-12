using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;

namespace yayoAni.Compat;

public static class OversizedWeapon
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static ThingComp GetOversizedComp(this ThingWithComps thing)
        => thing.GetComp<CompOversizedWeapon.CompOversizedWeapon>();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool IsOversizedComp(this ThingComp comp)
        => comp is CompOversizedWeapon.CompOversizedWeapon;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool IsOversizedDualWield(this Pawn_EquipmentTracker instance)
    {
        var comp = instance.Primary.GetComp<CompOversizedWeapon.CompOversizedWeapon>();
        var props = comp?.Props;
        return props is { isDualWeapon: true };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void HandleOversizedDrawing(this ThingComp comp, ref Vector3 drawLoc, ref float aimAngle, Pawn pawn, bool flip)
    {
        var props = (comp as CompOversizedWeapon.CompOversizedWeapon)?.Props;
        if (props == null)
            return;

        var pawnRotation = pawn.Rotation;

        if (pawnRotation == Rot4.North) drawLoc += props.northOffset;
        else if (pawnRotation == Rot4.East) drawLoc += props.eastOffset;
        else if (pawnRotation == Rot4.West) drawLoc += props.westOffset;
        else drawLoc += props.southOffset;

        if (!pawn.IsFighting())
        {
            if (flip && props.verticalFlipOutsideCombat)
                aimAngle += 180f;
            if (props.verticalFlipNorth && pawnRotation == Rot4.North)
                aimAngle += 180f;

            if (pawnRotation == Rot4.North) aimAngle += props.angleAdjustmentNorth;
            else if (pawnRotation == Rot4.East) aimAngle += props.angleAdjustmentEast;
            else if (pawnRotation == Rot4.West) aimAngle += props.angleAdjustmentWest;
            else aimAngle += props.angleAdjustmentSouth;
        }
    }
}