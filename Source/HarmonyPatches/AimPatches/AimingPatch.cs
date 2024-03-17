using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace YayoAnimation.HarmonyPatches.AimPatches;

[HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAndApparelExtras))]
// [HarmonyPatchCategory(HarmonyCategories.Combat)]
public static class AimingPatch
{
    private static readonly Vector3 Offset = new(-0.2f, 0f, 0.1f);
    // private static readonly Vector3 OffsetSub = new(0.2f, 0f, 0.1f);

    private static bool IsAimingAnimation = true;
    private static Pawn CurrentPawn;
    private static int CurrentPawnTick;
    private static float Wiggle;
    private static bool IsTwirling;

    private static void Prefix(Pawn pawn)
    {
        // Cache the pawn for the remaining methods
        CurrentPawn = pawn;
    }

    // TODO: Re-add dual wielding support in the future
    private static bool UseTwirl()
        => Core.settings.combatTwirlEnabled &&
           !CurrentPawn.RaceProps.IsMechanoid &&
           (!Core.settings.combatTwirlMaxMassEnabled || CurrentPawn.equipment.Primary.def.BaseMass <= Core.settings.combatTwirlMaxMass);
    
    private static void SetCurrentPawnTick() => CurrentPawnTick = CurrentPawn.HashOffsetTicks() & 0b0011_1111_1111;

    // private static Vector3 AddWiggleToAimingRotation(Vector3 vec, float angle)
    // {
    //     IsAimingAnimation = false;
    //     IsTwirling = false;
    //     SetCurrentPawnTick();
    //
    //     
    // }

    private static Vector3 AddWiggleToRotation(Vector3 vec)
    {
        IsAimingAnimation = false;
        IsTwirling = false;
        SetCurrentPawnTick();

        // TODO: Re-add dual wielding support in the future
        Wiggle = Mathf.Sin(CurrentPawnTick / 0.05f);
        // var wiggle = !isSub
        //     ? Mathf.Sin(tick * 0.05f)
        //     : Mathf.Sin(tick * 0.05f + 0.5f);

        if (UseTwirl())
        {
            // TODO: Re-add dual wielding support in the future
            // if (!isSub)
            {
                if (CurrentPawnTick is < 80 and >= 40)
                {
                    IsTwirling = true;
                    return new Vector3(vec.x + Offset.x, vec.y, vec.z + Offset.z + Wiggle * 0.05f);
                }
            }
            // else
            // {
            //     if (tick < 40)
            //     {
            //         addAngle += (tick - 40) * -36f;
            //         rootLoc += OffsetSub;
            //     }
            // }
        }

        return new Vector3(vec.x, vec.y, vec.z + Wiggle * 0.05f);
    }

    private static void DrawEquipmentAimingReplacement(Thing weapon, Vector3 drawPos, float angle)
    {
        if (IsAimingAnimation)
        {
            PawnRenderUtility.DrawEquipmentAiming(weapon, drawPos, angle);
            return;
        }

        if (IsTwirling)
        {
            // TODO: Re-add dual wielding support in the future
            angle += CurrentPawnTick * 36f;
        }

        PawnRenderUtility.DrawEquipmentAiming(weapon, drawPos, angle + Wiggle * -5f);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var locFields = new HashSet<FieldInfo>
        {
            AccessTools.DeclaredField(typeof(PawnRenderUtility), nameof(PawnRenderUtility.EqLocNorth)),
            AccessTools.DeclaredField(typeof(PawnRenderUtility), nameof(PawnRenderUtility.EqLocEast)),
            AccessTools.DeclaredField(typeof(PawnRenderUtility), nameof(PawnRenderUtility.EqLocSouth)),
            AccessTools.DeclaredField(typeof(PawnRenderUtility), nameof(PawnRenderUtility.EqLocWest)),
        };
        var drawEquipmentAiming = MethodUtil.MethodOf(PawnRenderUtility.DrawEquipmentAiming);
        var drawEquipmentAimingReplacement = MethodUtil.MethodOf(DrawEquipmentAimingReplacement);

        foreach (var ci in instr)
        {
            if (ci.opcode == OpCodes.Ldsfld && ci.operand is FieldInfo field && locFields.Contains(field))
            {
                yield return new CodeInstruction(OpCodes.Call, MethodUtil.MethodOf(AddWiggleToRotation));
            }
            else if (ci.Calls(drawEquipmentAiming))
            {
                ci.opcode = OpCodes.Call;
                ci.operand = drawEquipmentAimingReplacement;
            }

            yield return ci;
        }
    }
}