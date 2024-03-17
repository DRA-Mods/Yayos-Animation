// using HarmonyLib;
// using RimWorld;
// using UnityEngine;
// using Verse;
// using YayoAnimation.Data;
//
// namespace YayoAnimation.HarmonyPatches;
//
// [HotSwappable]
// [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnInternal))]
// [HarmonyBefore("rimworld.Nals.FacialAnimation")]
// public static class RenderPawnInternalPatch
// {
//     public static bool skipPatch = false;
//
//     // public static void Prefix(PawnRenderer __instance, ref Vector3 rootLoc, ref float angle, bool renderBody, ref Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, Pawn ___pawn)
//     public static void Prefix(ref PawnDrawParms parms)
//     {
//         if (skipPatch)
//         {
//             skipPatch = false;
//             return;
//         }
//
//         if (parms.pawn.GetPosture() == PawnPosture.Standing)
//         {
//             var pdd = parms.pawn.GetData();
//             angle += pdd.angleOffset;
//             parms.facing = pdd.fixedRot ?? parms.facing;
//         }
//     }
// }