using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace YayoAnimation.HarmonyPatches.PawnPatches;

[HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAndApparelExtras))]
// [HarmonyPatchCategory(HarmonyCategories.Combat)]
public static class AimingPatch
{
    // Shouldn't be threaded

    #region Setup

    private static float OffsetX = -0.2f;
    // private static float OffsetXSub = 0.2f;
    private static float OffsetZ = 0.1f;

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

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
    {
        var locFields = new HashSet<FieldInfo>
        {
            AccessTools.DeclaredField(typeof(PawnRenderUtility), nameof(PawnRenderUtility.EqLocNorth)),
            AccessTools.DeclaredField(typeof(PawnRenderUtility), nameof(PawnRenderUtility.EqLocEast)),
            AccessTools.DeclaredField(typeof(PawnRenderUtility), nameof(PawnRenderUtility.EqLocSouth)),
            AccessTools.DeclaredField(typeof(PawnRenderUtility), nameof(PawnRenderUtility.EqLocWest)),
        };

        var addWiggleToRotation = MethodUtil.MethodOf(AddWiggleToRotation);

        var drawEquipmentAiming = MethodUtil.MethodOf(PawnRenderUtility.DrawEquipmentAiming);
        var addWiggleToAngle = MethodUtil.MethodOf(AddWiggleToAngle);

        var rotatedBy = AccessTools.DeclaredMethod(typeof(Vector3Utility), nameof(Vector3Utility.RotatedBy),
            [typeof(Vector3), typeof(float)]);
        var addWiggleToAimingRotation = MethodUtil.MethodOf(AddWiggleToAimingRotation);

        var patchedFields = 0;
        var patchedAimingCalls = 0;
        var patchedRotatedByCalls = 0;

        foreach (var ci in instr)
        {
            // If DrawEquipmentAiming, then the last value on stack is float (angle) - replace it with ours
            if (ci.Calls(drawEquipmentAiming))
            {
                yield return new CodeInstruction(OpCodes.Call, addWiggleToAngle);

                patchedAimingCalls++;
            }

            yield return ci;

            // If RotatedBy called, apply our operations and then rotate
            if (ci.Calls(rotatedBy))
            {
                ci.opcode = OpCodes.Call;
                ci.operand = addWiggleToAimingRotation;

                patchedRotatedByCalls++;
            }
            // If any of the 4 static fields was loaded, call our method to add wiggle to it
            else if (ci.opcode == OpCodes.Ldsfld && ci.operand is FieldInfo field && locFields.Contains(field))
            {
                yield return new CodeInstruction(OpCodes.Call, addWiggleToRotation);

                patchedFields++;
            }
        }

        const int expectedPatchedFields = 4;
        const int expectedPatchedAimingCalls = 2;
        const int expectedRotatedByCalls = 1;

        if (patchedFields != expectedPatchedFields)
            Log.Error($"[{Core.ModName}] - patched incorrect number of calls to PawnRenderUtility directional rotation fields (expected: {expectedPatchedFields}, patched: {patchedFields}) for method {baseMethod.GetNameWithNamespace()}");
        if (patchedAimingCalls != expectedPatchedAimingCalls)
            Log.Error($"[{Core.ModName}] - patched incorrect number of calls to PawnRenderUtility.DrawEquipmentAiming (expected: {expectedPatchedAimingCalls}, patched: {patchedAimingCalls}) for method {baseMethod.GetNameWithNamespace()}");
        if (patchedRotatedByCalls != expectedRotatedByCalls)
            Log.Error($"[{Core.ModName}] - patched incorrect number of calls to Vector3Utility.RotatedBy (expected: {expectedRotatedByCalls}, patched: {patchedRotatedByCalls}) for method {baseMethod.GetNameWithNamespace()}");
    }

    #endregion

    #region Utilities

    // TODO: Re-add dual wielding support in the future
    private static bool UseTwirl()
        => Core.settings.combatTwirlEnabled &&
           !CurrentPawn.RaceProps.IsMechanoid &&
           (!Core.settings.combatTwirlMaxMassEnabled || CurrentPawn.equipment.Primary.def.BaseMass <= Core.settings.combatTwirlMaxMass);

    private static void SetCurrentPawnTick() => CurrentPawnTick = CurrentPawn.HashOffsetTicks() & 0b0011_1111_1111;

    #endregion

    #region Shared

    private static float AddWiggleToAngle(float angle)
    {
        // Aiming angle handled earlier.
        if (IsAimingAnimation)
            return Wiggle;

        // TODO: Re-add dual wielding support in the future
        if (IsTwirling)
            angle += CurrentPawnTick * 36f;

        return angle + Wiggle * -5f;
    }

    #endregion

    #region Weapon aiming

    private static Vector3 AddWiggleToAimingRotation(Vector3 vec, float angle)
    {
        IsAimingAnimation = true;
        IsTwirling = false;
        SetCurrentPawnTick();
        // Since we always override the angle with wiggle, set it up to angle as fallback
        Wiggle = angle;

        // Could probably pass as argument, however this should be safer.
        // Guaranteed non-null, focusTarg is valid, neverAimWeapon is false
        var stance = (CurrentPawn.stances.curStance as Stance_Busy)!;
        // Guaranteed non-null.
        var weapon = CurrentPawn.equipment.Primary;

        if (weapon.def.IsRangedWeapon && !stance.verb.IsMeleeAttack)
        {
            if (!CurrentPawn.RaceProps.IsMechanoid)
            {
                var ticksToNextBurstShot = stance.verb.ticksToNextBurstShot;
                var addX = 0f;
                var addZ = 0f;
                var addAngle = 0f;

                if (ticksToNextBurstShot > 10)
                    ticksToNextBurstShot = 10;

                var ani = Mathf.Max(stance.ticksLeft, 25f) * 0.001f;
                if (ticksToNextBurstShot > 0)
                    ani = ticksToNextBurstShot * 0.02f;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                float WiggleSlow() => Mathf.Sin(stance.ticksLeft * 0.035f) * 0.05f;
                // TODO: Re-add dual wielding support in the future
                // [MethodImpl(MethodImplOptions.AggressiveInlining)]
                // float WiggleSlow() => !isSub 
                //     ? Mathf.Sin(ani_cool * 0.035f) * 0.05f
                //     : Mathf.Sin(ani_cool * 0.035f + 0.5f) * 0.05f;

                switch ((CurrentPawn.LastAttackTargetTick ^ weapon.thingIDNumber) % 0b111)
                {
                    // Twirl, unused/not finished
                    // case 0:
                    // case 1:
                    //     if (UseTwirl())
                    //     {
                    //         
                    //     }
                    //     else
                    //     {
                    //         if (stanceBusy.ticksLeft > 1)
                    //         {
                    //             addY += wiggleSlow;
                    //         }
                    //     }
                    //     break;
                    // Reload animation
                    case 2:
                    case 3:
                        if (ticksToNextBurstShot == 0)
                        {
                            switch (stance.ticksLeft)
                            {
                                case > 78:
                                    break;
                                case > 48 when stance is not Stance_Warmup:
                                {
                                    var wiggle = Mathf.Sin(stance.ticksLeft * 0.1f) * 0.05f;
                                    addX += wiggle - 0.2f;
                                    addZ += wiggle + 0.2f;
                                    addAngle += wiggle + 30f + stance.ticksLeft * 0.5f;
                                    break;
                                }
                                case > 40 when stance is not Stance_Warmup:
                                {
                                    var wiggle = Mathf.Sin(stance.ticksLeft * 0.1f) * 0.05f;
                                    var wiggleFast = Mathf.Sin(stance.ticksLeft) * 0.05f;
                                    addX += wiggleFast + 0.05f;
                                    addZ += wiggle - 0.05f;
                                    addAngle += wiggleFast * 100f - 15f;
                                    break;
                                }
                                case > 1:
                                    addZ += WiggleSlow();
                                    break;
                            }
                        }

                        break;
                    default:
                        if (stance.ticksLeft > 1)
                        {
                            addZ += WiggleSlow();
                        }

                        break;
                }

                const float reboundFactor = 70f;

                if (CurrentPawn.Rotation == Rot4.West)
                {
                    vec += new Vector3(addZ, vec.y, 0.4f + addX + ani);
                    Wiggle = angle + ani * reboundFactor + addAngle;
                }
                else
                {
                    vec += new Vector3(-addZ, vec.y, 0.4f + addX - ani);
                    Wiggle = angle - ani * reboundFactor - addAngle;
                }
            }
            // else is mechanoid, don't do anything
        }
        else
        {
            var atkType = (CurrentPawn.LastAttackTargetTick ^ weapon.thingIDNumber) & 0b0011;

            var addAngle = atkType switch
            {
                1 => 25f, // Takedown
                2 => -25f, // Head stab
                _ => 0f
            };

            // Correct angles for ranged weapons
            if (weapon.def.IsRangedWeapon)
                addAngle -= 35f;

            const float readyZ = 0.2f;

            if (stance.ticksLeft > 15)
            {
                var ani = Mathf.Min(stance.ticksLeft, 60f);
                var ani2 = ani * 0.0075f;
                float addZ;
                float addX;

                switch (atkType)
                {
                    default:
                        // Ordinary attack
                        addZ = readyZ + 0.05f + ani2; // Higher -> closer to enemy
                        addX = 0.45f - 0.5f - ani2 * 0.1f; // Higher -> weapon is lower
                        break;
                    case 1:
                        // Takedown
                        addZ = readyZ + 0.05f + ani2; // Higher -> closer to enemy
                        addX = 0.45f - 0.35f + ani2 * 0.5f;  // Higher -> weapon is lower, animate in opposite direction
                        ani = 30f + ani * 0.5f; // Fixed angle value + angle change amount
                        break;
                    case 2:
                        // Head stab
                        addZ = readyZ + 0.05f + ani2; // Higher -> closer to enemy
                        addX = 0.45f - 0.35f - ani2;  // Higher -> weapon is lower
                        break;
                }

                switch (CurrentPawn.Rotation.AsInt)
                {
                    case Core.RotWest:
                    {
                        vec = new Vector3(vec.x - addX, vec.y, vec.z + addZ);
                        Wiggle = angle - addAngle - ani;
                        break;
                    }
                    case Core.RotEast:
                    {
                        vec = new Vector3(vec.x + addX, vec.y, vec.z + addZ);
                        Wiggle = angle + addAngle + ani;
                        break;
                    }
                    default:
                    {
                        vec = new Vector3(vec.x - addX, vec.y, vec.z + addZ);
                        Wiggle = angle + addAngle + ani;
                        break;
                    }
                }
            }
            else
            {
                vec = new Vector3(vec.x, vec.y, vec.z + readyZ);
            }
        }

        return vec.RotatedBy(angle);
    }

    #endregion

    #region Weapon carry

    private static Vector3 AddWiggleToRotation(Vector3 vec)
    {
        IsAimingAnimation = false;
        IsTwirling = false;
        SetCurrentPawnTick();

        // TODO: Re-add dual wielding support in the future
        Wiggle = Mathf.Sin(CurrentPawnTick * 0.05f);
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
                    return new Vector3(vec.x + OffsetX, vec.y, vec.z + OffsetZ + Wiggle * 0.05f);
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

    #endregion
}