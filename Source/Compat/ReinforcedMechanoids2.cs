#if BIOTECH_PLUS
using System.Runtime.CompilerServices;
using HarmonyLib;
using ReinforcedMechanoids;
using Verse;

namespace yayoAni.Compat;

public static class ReinforcedMechanoids2
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void UnpatchReinforcedMechanoidsDrawingPatch()
    {
        var original = AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming));

        Core.harmony.Unpatch(original, HarmonyPatchType.Prefix, HarmonyPatches.harmony.Id);
        Core.harmony.Unpatch(original, HarmonyPatchType.Postfix, HarmonyPatches.harmony.Id);
    }

    public static void GetAngleOffsetForPawn(this Pawn pawn, ref float aimAngle)
    {
        var extension = pawn.def.GetModExtension<EquipmentDrawPositionOffsetExtension>();
        if (extension == null)
            return;

        switch (pawn.Rotation.AsInt)
        {
            case 0:
                if (extension.northEquippedAngleOffset != null)
                    aimAngle = extension.northEquippedAngleOffset.Value;
                break;
            case 1:
                if (extension.eastEquippedAngleOffset != null)
                    aimAngle = extension.eastEquippedAngleOffset.Value;
                break;
            case 2:
                if (extension.southEquippedAngleOffset != null)
                    aimAngle = extension.southEquippedAngleOffset.Value;
                break;
            case 3:
                if (extension.westEquippedAngleOffset != null)
                    aimAngle = extension.westEquippedAngleOffset.Value;
                break;
        }
    }
}
#endif
