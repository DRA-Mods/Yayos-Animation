// using HarmonyLib;
// using RimWorld;
// using UnityEngine;
// using Verse;
// using YayoAnimation.Data;
//
// namespace YayoAnimation.HarmonyPatches;
//
// [HotSwappable]
// [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAndApparelExtras))]
// public static class DrawEquipmentAndApparelExtrasPatch
// {
//     public static void Prefix(Pawn pawn, ref Vector3 drawPos,
//         // ref float angle,
//         ref Rot4 facing, PawnRenderFlags flags)
//     {
//         if (pawn.GetPosture() == PawnPosture.Standing)
//         {
//             var pdd = pawn.GetData();
//             // angle += pdd.angleOffset;
//             facing = pdd.fixedRot ?? facing;
//         }
//     }
// }