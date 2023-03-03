#if BIOTECH_PLUS
using System.Runtime.CompilerServices;
using Tacticowl;
using UnityEngine;
using Verse;

namespace yayoAni.Compat;

public static class TacticowlComp
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool TryGetOffHandEquipmentOwl(this Pawn pawn, out ThingWithComps result)
        => pawn.GetOffHander(out result);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Pawn_StanceTracker GetStancesOffHandOwl(this Pawn pawn)
        => pawn.GetOffHandStanceTracker();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void GetOffsets(ThingWithComps offHandEquip, Pawn pawn, out Vector3 offsetMainHand, out Vector3 offsetOffHand, Stance_Busy primaryStance, Stance_Busy offhandStance)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsMelee(Thing thing) => thing.def.IsMeleeWeapon;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsAiming(Stance_Busy stance) => stance is { neverAimWeapon: false, focusTarg.IsValid: true };

        switch (pawn.Rotation.AsByte)
        {
            case Core.RotEast:
            {
                offsetMainHand = Vector3.zero;
                offsetOffHand = new Vector3(0f, -1f, 0.1f);
                return;
            }
            case Core.RotWest:
            {
                offsetMainHand = new Vector3(0f, -1f, 0f);
                offsetOffHand = new Vector3(0f, 0f, -0.1f);
                return;
            }
            case Core.RotNorth:
            {
                var mainHandAiming = IsAiming(primaryStance);
                var offHandAiming = IsAiming(offhandStance);

                if (!mainHandAiming && !offHandAiming)
                {
                    var primaryMelee = IsMelee(pawn.equipment.Primary);
                    var secondaryMelee = IsMelee(offHandEquip);

                    offsetMainHand = new Vector3(primaryMelee ? ModSettings_Tacticowl.meleeXOffset : ModSettings_Tacticowl.rangedXOffset, 0f, primaryMelee ? ModSettings_Tacticowl.meleeZOffset : ModSettings_Tacticowl.rangedZOffset);
                    offsetOffHand = new Vector3(secondaryMelee ? (-ModSettings_Tacticowl.meleeXOffset) : (-ModSettings_Tacticowl.rangedXOffset), 0f, secondaryMelee ? (-ModSettings_Tacticowl.meleeZOffset) : (-ModSettings_Tacticowl.rangedZOffset));
                }
                else
                {
                    offsetMainHand = Vector3.zero;
                    offsetOffHand = new Vector3(-0.1f, 0f, 0f);
                }
                return;
            }
            case Core.RotSouth:
            {
                var mainHandAiming = IsAiming(primaryStance);
                var offHandAiming = IsAiming(offhandStance);

                if (!mainHandAiming && !offHandAiming)
                {
                    var isMeleeWeapon2 = IsMelee(pawn.equipment.Primary);
                    var isMeleeWeapon = IsMelee(offHandEquip);

                    offsetMainHand = new Vector3(isMeleeWeapon2 ? (-ModSettings_Tacticowl.meleeXOffset) : (-ModSettings_Tacticowl.rangedXOffset), 1f, isMeleeWeapon2 ? (-ModSettings_Tacticowl.meleeZOffset) : (-ModSettings_Tacticowl.rangedZOffset));
                    offsetOffHand = new Vector3(isMeleeWeapon ? ModSettings_Tacticowl.meleeXOffset : ModSettings_Tacticowl.rangedXOffset, 0f, isMeleeWeapon ? ModSettings_Tacticowl.meleeZOffset : ModSettings_Tacticowl.rangedZOffset);
                }
                else
                {
                    offsetMainHand = Vector3.zero;
                    offsetOffHand = new Vector3(0.1f, 0f, 0f);
                }
                return;
            }
            default:
            {
                offsetMainHand = Vector3.zero;
                offsetOffHand = Vector3.zero;
                return;
            }
        }
    }
}
#endif