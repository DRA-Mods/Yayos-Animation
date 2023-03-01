using System;
using System.Runtime.CompilerServices;
using CompOversizedWeapon;
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
        if (comp.props is not CompProperties_OversizedWeapon props)
            return;

        var pawnRotation = pawn.Rotation;

        drawLoc += pawnRotation.AsInt switch
        {
            Core.RotNorth => props.northOffset,
            Core.RotEast => props.eastOffset,
            Core.RotWest => props.westOffset,
            _ => props.southOffset
        };

        if (pawn.IsFighting())
            return;
        if (flip && props.verticalFlipOutsideCombat)
            aimAngle += 180f;
        if (props.verticalFlipNorth && pawnRotation == Rot4.North)
            aimAngle += 180f;

        aimAngle += pawnRotation.AsInt switch
        {
            Core.RotNorth => props.angleAdjustmentNorth,
            Core.RotEast => props.angleAdjustmentEast,
            Core.RotWest => props.angleAdjustmentWest,
            _ => props.angleAdjustmentSouth
        };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void CheckOversizedActive()
    {
        try
        {
            // Basically a check to see if oversized weapons are active, and aren't an outdated version
            // which (despite me checking the code with decompiler) are causing errors when accessing fields in props.
            // Need to put it into a method other than this, as otherwise the static constructor will error.
            bool Temp()
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                return typeof(CompOversizedWeapon.CompOversizedWeapon) != null &&
                       typeof(CompProperties_OversizedWeapon) != null &&
                       // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                       new CompProperties_OversizedWeapon
                       {
                           northOffset = Vector3.zero,
                           eastOffset = Vector3.zero,
                           southOffset = Vector3.zero,
                           westOffset = Vector3.zero,
                           verticalFlipOutsideCombat = true,
                           verticalFlipNorth = true,
                           isDualWeapon = true,
                           angleAdjustmentEast = 1,
                           angleAdjustmentWest = 1,
                           angleAdjustmentNorth = 1,
                           angleAdjustmentSouth = 1,
                       } != null;
            }

            Core.usingOversizedWeapons = Temp();
            if (Core.usingOversizedWeapons)
                Log.Message("[Yayo's Animation] - CompOversizedWeapons detected");
        }
        catch (Exception e)
        {
            Core.usingOversizedWeapons = false;
            Log.Message(e is not TypeLoadException or TypeInitializationException or MissingFieldException
                ? $"[Yayo's Animation] - No CompOversizedWeapons detected. Unexpected exception caught: {e.GetType()}"
                : "[Yayo's Animation] - No CompOversizedWeapons detected.");
        }
    }
}