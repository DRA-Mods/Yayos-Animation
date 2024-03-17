// using HarmonyLib;
// using RimWorld;
// using UnityEngine;
// using Verse;
// using YayoAnimation.Compat;
//
// namespace YayoAnimation.HarmonyPatches.AimPatches;
//
// [HotSwappable]
// [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming))]
// public static class DrawEquipmentAimingPatch
// {
//     private static bool Prefix(Thing eq, Vector3 drawLoc, float aimAngle)
//     {
//         if (!Core.settings.combatEnabled)
//             return true;
//
//         if (Find.CameraDriver.CurrentZoom > Core.settings.maximumZoomLevel)
//             return true;
//
//         if (eq == null)
//             return true;
//
//         var pawn = __instance.pawn;
//
//         if (pawn == null || pawn.Dead || pawn.Downed || !pawn.Spawned)
//             return true;
//
//         if (pawn.Faction != Faction.OfPlayer && Core.settings.onlyPlayerPawns)
//             return true;
//
//         if (pawn.RaceProps.IsMechanoid && !Core.settings.mechanoidCombatEnabled)
//             return true;
//
//         if (pawn.RaceProps.Animal && !Core.settings.animalCombatEnabled)
//             return true;
//
//         CompEquippable compEquippable = null;
//         ThingComp compOversized = null;
//         // ThingComp compSheathWeaponExtension = null;
//
//         if (eq is ThingWithComps eqComps)
//         {
//             foreach (var comp in eqComps.AllComps)
//             {
//                 if (comp is CompEquippable equippable)
//                     compEquippable = equippable;
//                 else if (Core.usingOversizedWeapons && comp.IsOversizedComp())
//                     compOversized = comp;
//                 else if (Core.usingDeflector && comp.IsDeflectorAndAnimatingNow())
//                     return false;
//                 // else if (Core.usingSheathYourSword && comp.IsSheathWeaponExtension())
//                 //     compSheathWeaponExtension = comp;
//             }
//         }
//
//         var pawnRotation = pawn.Rotation;
//
//         var currentAngle = aimAngle - 90f;
//         Mesh mesh;
//
//         var isMeleeAtk = false;
//         var flip = false;
//         var offset = eq.def.equippedAngleOffset;
//         // if (Core.usingReinforcedMechanoids)
//         //     pawn.GetAngleOffsetForPawn(ref offset);
//
//         var flag = !(pawn.CurJob != null && pawn.CurJob.def.neverShowWeapon);
//
//         if (flag && pawn.stances.curStance is Stance_Busy { neverAimWeapon: false, focusTarg: { IsValid: true } } stance_Busy)
//         {
//             if (pawnRotation == Rot4.West)
//             {
//                 flip = true;
//             }
//
//             if (!pawn.equipment.Primary.def.IsRangedWeapon || stance_Busy.verb.IsMeleeAttack)
//             {
//                 // 근접공격
//                 isMeleeAtk = true;
//             }
//         }
//
//         if (isMeleeAtk)
//         {
//             if (flip)
//             {
//                 mesh = MeshPool.plane10Flip;
//                 currentAngle -= 180f;
//                 currentAngle -= offset;
//             }
//             else
//             {
//                 mesh = MeshPool.plane10;
//                 currentAngle += offset;
//             }
//         }
//         else
//         {
//             if (aimAngle is > 20f and < 160f)
//             {
//                 mesh = MeshPool.plane10;
//                 currentAngle += offset;
//             }
//             //else if ((aimAngle > 200f && aimAngle < 340f) || ignore)
//             else if (aimAngle is > 200f and < 340f || flip)
//             {
//                 flip = true;
//                 mesh = MeshPool.plane10Flip;
//                 currentAngle -= 180f;
//                 currentAngle -= offset;
//             }
//             else
//             {
//                 mesh = MeshPool.plane10;
//                 currentAngle += offset;
//             }
//         }
//
//         // Equipment offset - added by Enable Oversized Weapons, by default handles all weapons. Let's do the same (even if it's not active).
//         if (eq.StyleDef?.graphicData != null)
//             drawLoc += eq.StyleDef.graphicData.DrawOffsetForRot(pawnRotation);
//         else
//             drawLoc += eq.def.graphicData.DrawOffsetForRot(pawnRotation);
//
//         if (compEquippable != null)
//         {
//             EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
//             drawLoc += drawOffset;
//             currentAngle += angleOffset;
//         }
//
//         if (Core.settings.applyOversizedChanges)
//             compOversized?.HandleOversizedDrawing(ref drawLoc, ref currentAngle, pawn, flip);
//         // compSheathWeaponExtension?.HandleSheathExtensionDrawing(ref drawLoc, ref currentAngle, pawn);
//
//         currentAngle %= 360f;
//
//         // Material matSingle;
//         //if (graphic_StackCount != null)
//         //{
//         //    matSingle = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle;
//         //}
//         //else
//         //{
//         //    matSingle = eq.Graphic.MatSingle;
//         //}
//         //Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0);
//
//         Vector3 size = new(eq.Graphic.drawSize.x, 1f, eq.Graphic.drawSize.y);
//         var mat = eq.Graphic is Graphic_StackCount graphicStackCount
//             ? graphicStackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq)
//             : eq.Graphic.MatSingleFor(eq);
//
//         var matrix = Matrix4x4.TRS(drawLoc, Quaternion.AngleAxis(currentAngle, Vector3.up), size);
//
//         Graphics.DrawMesh(
//             mesh: mesh,
//             matrix: matrix,
//             material: mat,
//             layer: 0);
//
//         return false;
//     }
// }