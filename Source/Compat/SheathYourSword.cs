using System;
using System.Runtime.CompilerServices;
using SYS;
using UnityEngine;
using Verse;

namespace yayoAni.Compat;

[HotSwappable]
public static class SheathYourSword
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool SheathExtensionDrawLittleLower(this ThingWithComps thing)
        => thing.GetComp<CompWeaponExtention>()?.Props.littleDown == true;
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void DrawFullSheath(this ThingWithComps thing, Pawn pawn, in Vector3 rootLoc)
    {
        var comp = thing.GetComp<CompSheath>();
        if (comp != null)
            DrawEquipment_WeaponBackPatch.DrawSheath(comp, pawn, rootLoc, comp.FullGraphic);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void DrawEmptySheath(this ThingWithComps thing, Pawn pawn, in Vector3 rootLoc)
    {
        var comp = thing.GetComp<CompSheath>();
        if (comp != null)
            DrawEquipment_WeaponBackPatch.DrawSheath(comp, pawn, rootLoc, comp.SheathOnlyGraphic);
    }

    // [MethodImpl(MethodImplOptions.NoInlining)]
    // public static bool IsSheathWeaponExtension(this ThingComp comp)
    //     => comp is CompWeaponExtention;
    //
    // [MethodImpl(MethodImplOptions.NoInlining)]
    // public static void HandleSheathExtensionDrawing(this ThingComp comp, ref Vector3 drawLoc, ref float aimAngle, Pawn pawn)
    // {
    //     if (comp.props is not CompProperties_WeaponExtention props)
    //         return;
    //
    //     var pawnRotation = pawn.Rotation;
    //
    //     switch (pawnRotation.AsInt)
    //     {
    //         case Core.RotNorth:
    //             // Log.Error(props.northOffset.position.ToString());
    //             // drawLoc += props.northOffset.position;
    //             aimAngle += props.northOffset.angle;
    //             break;
    //         case Core.RotEast:
    //             // Log.Error(props.eastOffset.position.ToString());
    //             // drawLoc += props.eastOffset.position;
    //             aimAngle += props.eastOffset.angle;
    //             break;
    //         case Core.RotWest:
    //             // Log.Error(props.westOffset.position.ToString());
    //             // drawLoc += props.westOffset.position;
    //             aimAngle += props.westOffset.angle;
    //             break;
    //         default:
    //             // Log.Error(props.southOffset.position.ToString());
    //             drawLoc += props.southOffset.position;
    //             aimAngle += props.southOffset.angle - 90f;
    //             break;
    //     }
    // }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void CheckSheathYourSwordActive()
    {
        try
        {
            // Basically a check to see if oversized weapons are active, and aren't an outdated version
            // which (despite me checking the code with decompiler) are causing errors when accessing fields in props.
            // Need to put it into a method other than this, as otherwise the static constructor will error.
            bool Temp()
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                return typeof(CompSheath) != null &&
                       typeof(CompProperties_Sheath) != null &&
                       typeof(CompWeaponExtention) != null &&
                       typeof(CompProperties_WeaponExtention) != null &&
                       // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                       new CompProperties_Sheath
                       {
                           northOffset = new Offset
                           {
                               position = Vector3.zero,
                               angle = 1531,
                           },
                           eastOffset = new Offset
                           {
                               position = Vector3.zero,
                               angle = 1531,
                           },
                           southOffset = new Offset
                           {
                               position = Vector3.zero,
                               angle = 1531,
                           },
                           westOffset = new Offset
                           {
                               position = Vector3.zero,
                               angle = 1531,
                           },
                       } != null &&
                       new CompProperties_WeaponExtention
                       {
                           northOffset = new Offset
                           {
                               position = Vector3.zero,
                               angle = 1531,
                           },
                           eastOffset = new Offset
                           {
                               position = Vector3.zero,
                               angle = 1531,
                           },
                           southOffset = new Offset
                           {
                               position = Vector3.zero,
                               angle = 1531,
                           },
                           westOffset = new Offset
                           {
                               position = Vector3.zero,
                               angle = 1531,
                           },
                           littleDown = true,
                       } != null;
            }

            Core.usingSheathYourSword = Temp();
            if (Core.usingSheathYourSword)
                Log.Message("[Yayo's Animation] - SheathYourSword detected");
        }
        catch (Exception e)
        {
            Core.usingSheathYourSword = false;
            Log.Message(e is not TypeLoadException or TypeInitializationException or MissingFieldException
                ? $"[Yayo's Animation] - No SheathYourSword detected. Unexpected exception caught: {e.GetType()}"
                : "[Yayo's Animation] - No SheathYourSword detected.");
        }
    }
}