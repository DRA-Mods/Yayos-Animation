// using System.Collections.Generic;
// using System.Reflection;
// using System.Reflection.Emit;
// using HarmonyLib;
// using UnityEngine;
// using Verse;
// using YayoAnimation.Data;
//
// namespace YayoAnimation.HarmonyPatches;
//
// [HotSwappable]
// [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt))]
// public static class RenderPawnAtPatch
// {
//     public static void Prefix(PawnRenderer __instance, Pawn ___pawn, ref Vector3 drawLoc, Rot4? rotOverride = null, bool neverAimWeapon = false)
//     {
//         if (___pawn == null)
//             return;
//
//         var pdd = ___pawn.GetData();
//         Yayo.CheckAni(___pawn, ref drawLoc, rotOverride ?? ___pawn.Rotation, pdd);
//     }
//
//     public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
//     {
//         var pawnField = AccessTools.DeclaredField(typeof(PawnRenderer), nameof(PawnRenderer.pawn));
//         var replacements = new Dictionary<MethodInfo, MethodInfo>()
//         {
//             {
//                 AccessTools.DeclaredMethod(typeof(GenDraw), nameof(GenDraw.DrawMeshNowOrLater),
//                     [typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool)]),
//                 MethodUtil.MethodOf(Yayo.DrawMeshNowOrLater)
//             },
//             {
//                 AccessTools.DeclaredMethod(typeof(PawnRenderer), nameof(PawnRenderer.GetBlitMeshUpdatedFrame)),
//                 MethodUtil.MethodOf(Yayo.GetBlitMeshUpdatedFrame)
//             },
//         };
//         var replacedCount = 0;
//
//         foreach (var ci in instr)
//         {
//             if ((ci.opcode == OpCodes.Call || ci.opcode == OpCodes.Callvirt) &&
//                 ci.operand is MethodInfo method && replacements.TryGetValue(method, out var replacement))
//             {
//                 ci.opcode = OpCodes.Call;
//                 ci.operand = replacement;
//
//                 // Add 'this.pawn' call as another argument to the method
//                 yield return new CodeInstruction(OpCodes.Ldarg_0);
//                 yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
//
//                 replacedCount++;
//             }
//
//             yield return ci;
//         }
//
//         const int expected = 2;
//         if (replacedCount != expected)
//         {
//             var name = (baseMethod.DeclaringType?.Namespace).NullOrEmpty() ? baseMethod.Name : $"{baseMethod.DeclaringType!.Name}:{baseMethod.Name}";
//             Log.Warning($"Patched incorrect number of calls (patched {replacedCount}, expected {expected}) for method {name}");
//         }
//     }
// }