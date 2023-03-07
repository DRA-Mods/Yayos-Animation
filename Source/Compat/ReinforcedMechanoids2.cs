#if BIOTECH_PLUS
using System.Runtime.CompilerServices;
using HarmonyLib;
using ReinforcedMechanoids;
using Verse;

namespace yayoAni.Compat;

public static class ReinforcedMechanoids2
{
    public static bool rm2PatchActive = false;

    public static void SetReinforcedMechanoidsPatch(bool state)
    {
        if (!Core.usingReinforcedMechanoids || rm2PatchActive == state)
            return;

        if (state)
            LongEventHandler.ExecuteWhenFinished(UnpatchReinforcedMechanoidsDrawingPatch);
        else
            LongEventHandler.ExecuteWhenFinished(RepatchReinforcedMechanoidsDrawingPatch);

        rm2PatchActive = state;
    }

    private static void UnpatchReinforcedMechanoidsDrawingPatch()
    {
        var original = AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming));

        Core.harmony.Unpatch(original, AccessTools.DeclaredMethod(typeof(PawnRenderer_DrawEquipmentAiming_Patch), nameof(PawnRenderer_DrawEquipmentAiming_Patch.Prefix)));
        Core.harmony.Unpatch(original, AccessTools.DeclaredMethod(typeof(PawnRenderer_DrawEquipmentAiming_Patch), nameof(PawnRenderer_DrawEquipmentAiming_Patch.Postfix)));
    }

    private static void RepatchReinforcedMechanoidsDrawingPatch()
    {
        var original = AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming));

        Core.harmony.Patch(original,
            prefix: new HarmonyMethod(typeof(PawnRenderer_DrawEquipmentAiming_Patch), nameof(PawnRenderer_DrawEquipmentAiming_Patch.Prefix)),
            postfix: new HarmonyMethod(typeof(PawnRenderer_DrawEquipmentAiming_Patch), nameof(PawnRenderer_DrawEquipmentAiming_Patch.Postfix)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
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
