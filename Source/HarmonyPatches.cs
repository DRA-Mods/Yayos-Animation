using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace yayoAni
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches : Mod
    {
        public static Harmony harmony;

        public HarmonyPatches(ModContentPack content) : base(content)
        {
            harmony = new Harmony("com.yayo.yayoAni");
        }
    }


    [HarmonyPatch(typeof(PawnRenderer))]
    [HarmonyPatch("RenderPawnAt")]
    public static class Patch_RenderPawnAt2
    {
        public static bool equal(this CodeInstruction ci, OpCode oc)
        {
            return ci.opcode == oc;
        }

        public static bool oploc(this CodeInstruction ci, OpCode oc, int i)
        {
            if (ci.opcode == oc && ci.operand is LocalBuilder localBuilder)
                return localBuilder.LocalIndex == i;

            return false;
        }

        public enum findPointType
        {
            start,
            after
        }

        public static bool findPoint(this List<CodeInstruction> ar, List<OpCode> target, out int point, findPointType findType)
        {
            point = -1;
            for (int i = 0; i < ar.Count - target.Count; i++)
            {
                for (int j = 0; j < target.Count; j++)
                {
                    if (ar[i + j].equal(target[j]))
                    {
                        if (j == target.Count - 1)
                        {
                            switch (findType)
                            {
                                case findPointType.start:
                                    point = i;
                                    break;
                                case findPointType.after:
                                    point = i + j + 1;
                                    break;
                            }

                            return true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return false;
        }


        [UsedImplicitly]
        static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> ar_ci = instructions.ToList();
            List<CodeInstruction> ar_insert;
            List<OpCode> ar_find;


            // --------------------

            #region GenDraw.DrawMeshNowOrLater(GetBlitMeshUpdatedFrame(frameSet, rot, PawnDrawMode.BodyAndHead), drawLoc, Quaternion.AngleAxis(0f, Vector3.up), original, drawNow: false);

            ar_find = new List<OpCode>()
            {
                OpCodes.Ldarg_0,
                OpCodes.Ldloc_S,
                OpCodes.Ldloc_0,
                OpCodes.Ldc_I4_0,
                OpCodes.Call,
                OpCodes.Ldarg_1,
                OpCodes.Ldc_R4,
                OpCodes.Call,
                OpCodes.Call,
                OpCodes.Ldloc_S,
                OpCodes.Ldc_I4_0,
                OpCodes.Call
            };

            if (ar_ci.findPoint(ar_find, out var point, findPointType.start))
            {
                //for (int i = point; i < point + ar_find.Count; i++)
                //{
                //    Log.Message($"{i} : {ar_ci[i].opcode}");
                //}
                //Log.Message($"--------------change-----------------");

                var tmp_point = point + 11;
                ar_ci.RemoveRange(tmp_point, 1);
                ar_insert = new List<CodeInstruction>()
                {
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn")),
                    new(OpCodes.Call, AccessTools.Method(typeof(yayo), "DrawMeshNowOrLater"))
                };
                ar_ci.InsertRange(tmp_point, ar_insert);


                tmp_point = point + 4;
                ar_ci.RemoveRange(tmp_point, 1);
                ar_insert = new List<CodeInstruction>()
                {
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn")),
                    new(OpCodes.Call, AccessTools.Method(typeof(yayo), "GetBlitMeshUpdatedFrame"))
                };
                ar_ci.InsertRange(tmp_point, ar_insert);


                tmp_point = point + 0;
                ar_ci.RemoveRange(tmp_point, 1);


                //for (int i = point; i < point + ar_find.Count; i++)
                //{
                //	Log.Message($"{i} : {ar_ci[i].opcode}");

                //}
            }

            #endregion

            #region RenderPawnInternal(drawLoc, 0f, renderBody: true, rot, curRotDrawMode, pawnRenderFlags);

            //ar_find = new List<OpCode>()
            //{
            //	//IL_0106: br.s IL_0118
            //	OpCodes.Br_S,
            //	//IL_0108: ldarg.0
            //	OpCodes.Ldarg_0,
            //	//IL_0109: ldarg.1
            //	OpCodes.Ldarg_1,
            //	//IL_010a: ldc.r4 0.0
            //	OpCodes.Ldc_R4,
            //	//IL_010f: ldc.i4.1
            //	OpCodes.Ldc_I4_1,
            //	//IL_0110: ldloc.0
            //	OpCodes.Ldloc_0,
            //	//IL_0111: ldloc.2
            //	OpCodes.Ldloc_2,
            //	//IL_0112: ldloc.1
            //	OpCodes.Ldloc_1,
            //	//IL_0113: call instance void Verse.PawnRenderer::RenderPawnInternal(valuetype[UnityEngine.CoreModule]UnityEngine.Vector3, float32, bool, valuetype Verse.Rot4, valuetype Verse.RotDrawMode, valuetype Verse.PawnRenderFlags)
            //	OpCodes.Call
            //};

            //if (ar_ci.findPoint(ar_find, out point, findPointType.start))
            //{
            //             for (int i = point - 5; i < point + ar_find.Count + 5; i++)
            //             {
            //                 Log.Message($"{i} : {ar_ci[i].opcode} - {ar_ci[i].operand}");
            //             }
            //             Log.Message($"--------------change-----------------");

            //             tmp_point = point + 8;
            //	ar_ci.RemoveRange(tmp_point, 1);
            //	ar_insert = new List<CodeInstruction>()
            //	{
            //                 //new CodeInstruction(OpCodes.Ldarg_2),
            //		//new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn")),
            //		new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(yayo), "RenderPawnInternal"))

            //	};
            //	ar_ci.InsertRange(tmp_point, ar_insert);


            //             //ar_ci.RemoveRange(point + 0, 2);
            //             //ar_ci.Insert(point + 3, new CodeInstruction(OpCodes.Ldarg_2));


            //             for (int i = point - 5; i < point + ar_find.Count + 5; i++)
            //             {
            //                 Log.Message($"{i} : {ar_ci[i].opcode} - {ar_ci[i].operand}");

            //             }

            //         }

            #endregion


            // result out
            for (int i = 0; i < ar_ci.Count; i++)
            {
                yield return ar_ci[i];
            }
        }
    }


    public static class yayo
    {
        public static Mesh GetBlitMeshUpdatedFrame(PawnTextureAtlasFrameSet frameSet, Rot4 rotation, PawnDrawMode drawMode, Pawn p)
        {
            var pdd = dataUtility.GetData(p);
            rotation = pdd.fixed_rot ?? rotation;
            return p.Drawer.renderer.GetBlitMeshUpdatedFrame(frameSet, rotation, drawMode);
        }


        public static void DrawMeshNowOrLater(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Pawn p)
        {
            var pdd = dataUtility.GetData(p);
            quat = Quaternion.AngleAxis(pdd.offset_angle, Vector3.up);
            GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
        }


        //public static void RenderPawnInternal(Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
        //{
        //	//pdd = dataUtility.GetData(p);
        //	//angle += pdd.offset_angle;
        // //         bodyFacing = pdd.fixed_rot ?? bodyFacing;
        // //         AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal").Invoke(p.Drawer.renderer, new object[] { rootLoc, angle, renderBody, bodyFacing, bodyDrawType, flags });
        //}


        public static void checkAni(Pawn pawn, ref Vector3 pos, Rot4 rot)

        {
            if (pawn.Dead) return;
            if (pawn.GetPosture() == PawnPosture.Standing)
            {
                ani0(pawn, ref pos, rot);
            }
            else
            {
                ani1(pawn, ref pos, rot);
            }
        }

        public enum aniType
        {
            none,
            doSomeThing,
            social,
            smash,
            idle,
            gameCeremony,
            crowd,
            solemn
        }

        public static void ani0(Pawn pawn, ref Vector3 pos, Rot4 rot)
        {
            bool changed = false;
            float oa = 0f;
            Vector3 op = Vector3.zero;
            pawnDrawData pdd = dataUtility.GetData(pawn);

            if (pawn.pather != null && pawn.pather.MovingNow)
            {
                if (core.settings.val_walk)
                {
                    changed = true;
                    int IdTick = pawn.thingIDNumber * 20;

                    float wiggle = Mathf.Sin((Find.TickManager.TicksGame + IdTick) * 7f * core.settings.val_walkSpeed / pawn.pather.nextCellCostTotal);
                    oa = wiggle * 9f * core.settings.val_walkAngle;
                    op = new Vector3(wiggle * 0.025f, 0f, 0f);
                }
            }
            else if (core.settings.val_anyJob && pawn.CurJob != null)
            {
                changed = true;
                int IdTick = pawn.thingIDNumber * 20;

                if (core.settings.val_debug)
                    if (pawn.IsColonist)
                        Log.Message($"{pawn.NameShortColored} : {pawn.CurJob.def.defName}");

                //if (pawn.IsColonist) Log.Message($"{pawn.NameShortColored} : {pawn.CurJob.def.defName} / id {pawn.thingIDNumber}");
                //float wiggle = Mathf.Sin((Find.TickManager.TicksGame + IdTick) * 7f / pawn.pather.nextCellCostTotal);
                int t = 0;
                int t2;
                int total;
                aniType aniType = aniType.none;
                float f;
                Rot4 r;
                Rot4 tr;


                switch (pawn.CurJob.def.defName)
                {
                    // do something
                    case "UseArtifact":
                        aniType = aniType.doSomeThing;
                        break;
                    case "UseNeurotrainer":
                        aniType = aniType.doSomeThing;
                        break;
                    case "UseStylingStation":
                        aniType = aniType.doSomeThing;
                        break;
                    case "UseStylingStationAutomatic":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Wear":
                        aniType = aniType.doSomeThing;
                        break;
                    case "SmoothWall":
                        aniType = aniType.doSomeThing;
                        break;
                    case "UnloadYourInventory":
                        aniType = aniType.doSomeThing;
                        break;
                    case "UnloadInventory":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Uninstall":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Train":
                        aniType = aniType.doSomeThing;
                        break;
                    case "TendPatient":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Tame":
                        aniType = aniType.doSomeThing;
                        break;
                    case "TakeBeerOutOfFermentingBarrel":
                        aniType = aniType.doSomeThing;
                        break;
                    case "StudyThing":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Strip":
                        aniType = aniType.doSomeThing;
                        break;
                    case "SmoothFloor":
                        aniType = aniType.doSomeThing;
                        break;
                    case "SlaveSuppress":
                        aniType = aniType.doSomeThing;
                        break;
                    case "SlaveExecution":
                        aniType = aniType.doSomeThing;
                        break;
                    case "DoBill": // 제작, 조리
                        aniType = aniType.doSomeThing;
                        break;
                    case "Deconstruct":
                        aniType = aniType.doSomeThing;
                        break;
                    case "FinishFrame": // 건설
                        aniType = aniType.doSomeThing;
                        break;
                    case "Equip":
                        aniType = aniType.doSomeThing;
                        break;
                    case "ExtractRelic":
                        aniType = aniType.doSomeThing;
                        break;
                    case "ExtractSkull":
                        aniType = aniType.doSomeThing;
                        break;
                    case "ExtractTree":
                        aniType = aniType.doSomeThing;
                        break;
                    case "GiveSpeech":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Hack":
                        aniType = aniType.doSomeThing;
                        break;
                    case "InstallRelic":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Insult":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Milk":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Open":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Play_MusicalInstrument":
                        aniType = aniType.doSomeThing;
                        break;
                    case "PruneGauranlenTree":
                        aniType = aniType.doSomeThing;
                        break;
                    case "RearmTurret":
                        aniType = aniType.doSomeThing;
                        break;
                    case "RearmTurretAtomic":
                        aniType = aniType.doSomeThing;
                        break;
                    case "RecolorApparel":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Refuel":
                        aniType = aniType.doSomeThing;
                        break;
                    case "RefuelAtomic":
                        aniType = aniType.doSomeThing;
                        break;

                    case "Reload":
                        aniType = aniType.doSomeThing;
                        break;
                    case "RemoveApparel":
                        aniType = aniType.doSomeThing;
                        break;
                    case "RemoveFloor":
                        aniType = aniType.doSomeThing;
                        break;
                    case "RemoveRoof":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Repair":
                        aniType = aniType.doSomeThing;
                        break;

                    case "Research":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Resurrect":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Sacrifice":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Scarify":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Shear":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Slaughter":
                        aniType = aniType.doSomeThing;
                        break;
                    case "Ignite":
                        aniType = aniType.doSomeThing;
                        break;
                    case "ManTurret":
                        aniType = aniType.doSomeThing;
                        break;


                    // social
                    case "GotoAndBeSociallyActive":
                        aniType = aniType.social;
                        break;
                    case "StandAndBeSociallyActive":
                        aniType = aniType.social;
                        break;
                    case "VisitSickPawn":
                        aniType = aniType.social;
                        break;
                    case "SocialRelax":
                        aniType = aniType.social;
                        break;


                    case "Wait_Combat":
                        aniType = aniType.idle;
                        break;
                    case "Wait":
                        aniType = aniType.idle;
                        break;


                    case "Vomit":
                        t = (Find.TickManager.TicksGame + IdTick) % 200;
                        if (!core.Ani(ref t, 25, ref oa, 15f, 35f, -1f, ref op, rot))
                            if (!core.Ani(ref t, 25, ref oa, 35f, 25f, -1f, ref op, rot))
                                if (!core.Ani(ref t, 25, ref oa, 25f, 35f, -1f, ref op, rot))
                                    if (!core.Ani(ref t, 25, ref oa, 35f, 25f, -1f, ref op, rot))
                                        if (!core.Ani(ref t, 25, ref oa, 25f, 35f, -1f, ref op, rot))
                                            if (!core.Ani(ref t, 25, ref oa, 35f, 25f, -1f, ref op, rot))
                                                if (!core.Ani(ref t, 25, ref oa, 25f, 35f, -1f, ref op, rot))
                                                    core.Ani(ref t, 25, ref oa, 35f, 15f, -1f, ref op, rot);

                        break;


                    case "Clean":
                        aniType = aniType.doSomeThing;
                        break;


                    case "Mate":
                        break;


                    case "MarryAdjacentPawn":
                        t = (Find.TickManager.TicksGame) % 310;

                        if (!core.Ani(ref t, 150))
                        {
                            if (!core.Ani(ref t, 20, ref oa, 0f, 5f, -1f, ref op, Vector3.zero, new Vector3(0.05f, 0f, 0f), rot))
                                if (!core.Ani(ref t, 50, ref oa, 5f, 10f, -1f, ref op, new Vector3(0.05f, 0f, 0f), new Vector3(0.05f, 0f, 0f), rot))
                                    if (!core.Ani(ref t, 50, ref oa, 10, 10f, -1f, ref op, new Vector3(0.05f, 0f, 0f), new Vector3(0.05f, 0f, 0f), rot))
                                        core.Ani(ref t, 40, ref oa, 10f, 0f, -1f, ref op, new Vector3(0.05f, 0f, 0f), Vector3.zero, rot);
                        }

                        break;
                    case "SpectateCeremony": // 각종 행사, 의식 (결혼식, 장례식, 이념행사)
                        LordJob_Ritual ritualJob = core.GetPawnRitual(pawn);
                        if (ritualJob == null) // 기본
                        {
                            aniType = aniType.crowd;
                        }
                        else if (ritualJob.Ritual == null)
                        {
                            // 로얄티 수여식 관중
                            aniType = aniType.solemn;
                        }
                        else
                        {
                            switch (ritualJob.Ritual.def.defName)
                            {
                                default:
                                    aniType = aniType.crowd;
                                    break;

                                case "Funeral": // 장례식
                                    aniType = aniType.solemn;
                                    break;
                            }
                        }

                        break;
                    case "BestowingCeremony": // 로얄티 수여식 받는 대상
                        aniType = aniType.solemn;
                        break;


                    case "Dance":
                        break;


                    // joy


                    case "Play_Hoopstone":
                        t = (Find.TickManager.TicksGame + IdTick) % 60;
                        if (!core.Ani(ref t, 30, ref oa, 10f, -20f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot))
                        {
                            core.Ani(ref t, 30, ref oa, -20f, 10f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot);
                        }

                        break;
                    case "Play_Horseshoes":
                        t = (Find.TickManager.TicksGame + IdTick) % 60;
                        if (!core.Ani(ref t, 30, ref oa, 10f, -20f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot))
                        {
                            core.Ani(ref t, 30, ref oa, -20f, 10f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot);
                        }

                        break;


                    case "Play_GameOfUr":
                        t = (Find.TickManager.TicksGame + IdTick * 27) % 900;
                        if (t <= 159)
                        {
                            aniType = aniType.gameCeremony;
                        }
                        else
                        {
                            aniType = aniType.doSomeThing;
                        }

                        break;

                    case "Play_Poker":
                        t = (Find.TickManager.TicksGame + IdTick * 27) % 900;
                        if (t <= 159)
                        {
                            aniType = aniType.gameCeremony;
                        }
                        else
                        {
                            aniType = aniType.doSomeThing;
                        }

                        break;

                    case "Play_Billiards":
                        t = (Find.TickManager.TicksGame + IdTick * 27) % 900;
                        if (t <= 159)
                        {
                            aniType = aniType.gameCeremony;
                        }
                        else
                        {
                            aniType = aniType.doSomeThing;
                        }

                        break;

                    case "Play_Chess":
                        t = (Find.TickManager.TicksGame + IdTick * 27) % 900;
                        if (t <= 159)
                        {
                            aniType = aniType.gameCeremony;
                        }
                        else
                        {
                            aniType = aniType.doSomeThing;
                        }

                        break;


                    case "ExtinguishSelf": // 스스로 불 끄기

                        int tg = 10; // 틱 갭
                        t = (Find.TickManager.TicksGame + IdTick) % (12 * tg);
                        r = Rot4.East;

                        float cx = -0.5f;
                        float cy = 0f; // 중심점

                        float gx = -0.25f; // 이동 갭
                        float gy = 0.25f; // 이동 갭

                        int step = 0; // 이동 단계

                        float a = 45f;

                        if (!core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step), new Vector3(cx + gx * (step + 1), 0f, cy + gy * (step + 1)), r,
                                core.tweenType.line))
                        {
                            rot = core.Rot90(rot);
                            step++;
                            if (!core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step), new Vector3(cx + gx * (step + 1), 0f, cy + gy * (step + 1)), r,
                                    core.tweenType.line))
                            {
                                rot = core.Rot90(rot);
                                step++;
                                if (!core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step), new Vector3(cx + gx * (step + 1), 0f, cy + gy * (step + 1)), r,
                                        core.tweenType.line))
                                {
                                    rot = core.Rot90(rot);
                                    step++;


                                    // reverse
                                    if (!core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step), new Vector3(cx + gx * (step - 1), 0f, cy + gy * (step - 1)), r,
                                            core.tweenType.line))
                                    {
                                        rot = core.Rot90b(rot);
                                        step--;
                                        if (!core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                new Vector3(cx + gx * (step - 1), 0f, cy + gy * (step - 1)), r, core.tweenType.line))
                                        {
                                            rot = core.Rot90b(rot);
                                            step--;
                                            if (!core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                    new Vector3(cx + gx * (step - 1), 0f, cy + gy * (step - 1)), r, core.tweenType.line))
                                            {
                                                rot = core.Rot90b(rot);
                                                step--;
                                                if (!core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                        new Vector3(cx + gx * (step - 1), 0f, cy + gy * (step - 1)), r, core.tweenType.line))
                                                {
                                                    rot = core.Rot90b(rot);
                                                    step--;
                                                    if (!core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                            new Vector3(cx + gx * (step - 1), 0f, cy + gy * (step - 1)), r, core.tweenType.line))
                                                    {
                                                        rot = core.Rot90b(rot);
                                                        step--;
                                                        if (!core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                                new Vector3(cx + gx * (step - 1), 0f, cy + gy * (step - 1)), r, core.tweenType.line))
                                                        {
                                                            rot = core.Rot90b(rot);
                                                            step--;

                                                            // reverse
                                                            if (!core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                                    new Vector3(cx + gx * (step + 1), 0f, cy + gy * (step + 1)), r, core.tweenType.line))
                                                            {
                                                                rot = core.Rot90(rot);
                                                                step++;
                                                                if (!core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                                        new Vector3(cx + gx * (step + 1), 0f, cy + gy * (step + 1)), r, core.tweenType.line))
                                                                {
                                                                    rot = core.Rot90(rot);
                                                                    step++;
                                                                    if (!core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                                            new Vector3(cx + gx * (step + 1), 0f, cy + gy * (step + 1)), r, core.tweenType.line))
                                                                    {
                                                                        rot = core.Rot90(rot);
                                                                        step++;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        try
                        {
                            pawn.CurJob.targetA.Thing.DrawAt(pos + op + new Vector3(0f, 0.1f, 0f));
                        }
                        catch
                        {
                            // ignored
                        }


                        break;


                    case "Sow": // 씨뿌리기
                        t = (Find.TickManager.TicksGame + IdTick) % 50;

                        if (!core.Ani(ref t, 35))
                            if (!core.Ani(ref t, 5, ref oa, 0f, 10f, -1f, ref op, rot))
                                core.Ani(ref t, 10, ref oa, 10f, 0f, -1f, ref op, rot);

                        break;


                    case "CutPlant": // 식물 베기
                        if (pawn.CurJob.targetA.Thing?.def.plant?.IsTree != null && pawn.CurJob.targetA.Thing.def.plant.IsTree)
                        {
                            aniType = aniType.smash;
                        }
                        else
                        {
                            aniType = aniType.doSomeThing;
                        }

                        break;
                    case "Harvest": // 자동 수확
                        if (pawn.CurJob.targetA.Thing?.def.plant?.IsTree != null && pawn.CurJob.targetA.Thing.def.plant.IsTree)
                        {
                            aniType = aniType.smash;
                        }
                        else
                        {
                            aniType = aniType.doSomeThing;
                        }

                        break;

                    case "HarvestDesignated": // 수동 수확
                        if (pawn.CurJob.targetA.Thing?.def.plant?.IsTree != null && pawn.CurJob.targetA.Thing.def.plant.IsTree)
                        {
                            aniType = aniType.smash;
                        }
                        else
                        {
                            aniType = aniType.doSomeThing;
                        }

                        break;

                    case "Mine": // 채굴
                        aniType = aniType.smash;
                        break;

                    case "Ingest": // 밥먹기
                        t = (Find.TickManager.TicksGame + IdTick) % 150;
                        f = 0.03f;
                        if (!core.Ani(ref t, 10, ref oa, 0f, 15f, -1f, ref op, Vector3.zero, new Vector3(0f, 0f, 0f), rot))
                            if (!core.Ani(ref t, 10, ref oa, 15f, 0f, -1f, ref op, Vector3.zero, new Vector3(0f, 0f, 0f), rot))
                                if (!core.Ani(ref t, 10, ref oa, 0f, 0f, -1f, ref op, Vector3.zero, new Vector3(0f, 0f, f), rot))
                                    if (!core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, f), new Vector3(0f, 0f, -f), rot))
                                        if (!core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, -f), new Vector3(0f, 0f, f), rot))
                                            if (!core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, f), new Vector3(0f, 0f, -f), rot))
                                                if (!core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, -f), new Vector3(0f, 0f, f), rot))
                                                    if (!core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, f), new Vector3(0f, 0f, -f), rot))
                                                        core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, -f), new Vector3(0f, 0f, f), rot);

                        break;
                }


                switch (aniType)
                {
                    case aniType.solemn:
                        t = (Find.TickManager.TicksGame + (IdTick % 25)) % 660;

                        if (!core.Ani(ref t, 300))
                        {
                            if (!core.Ani(ref t, 30, ref oa, 0f, 15f, -1f, ref op, Vector3.zero, Vector3.zero, rot))
                                if (!core.Ani(ref t, 300, ref oa, 15f, 15f, -1f, ref op, Vector3.zero, Vector3.zero, rot))
                                    core.Ani(ref t, 30, ref oa, 15f, 0f, -1f, ref op, Vector3.zero, Vector3.zero, rot);
                        }

                        break;

                    case aniType.crowd:
                        total = 143;
                        t2 = (Find.TickManager.TicksGame + IdTick) % (total * 2);
                        t = t2 % total;
                        r = core.Rot90(rot);
                        tr = rot;
                        if (!core.Ani(ref t, 20))
                        {
                            if (!core.Ani(ref t, 5, ref oa, 0f, 10f, -1f, ref op, r))
                                if (!core.Ani(ref t, 20, ref oa, 10f, 10f, -1f, ref op, r))
                                    if (!core.Ani(ref t, 5, ref oa, 10f, -10f, -1f, ref op, r))
                                        if (!core.Ani(ref t, 20, ref oa, -10f, -10f, -1f, ref op, r))
                                        {
                                            if (!core.Ani(ref t, 5, ref oa, -10f, 0f, -1f, ref op, r))
                                            {
                                                tr = t2 >= total ? core.Rot90(rot) : core.Rot90b(rot);
                                                if (!core.Ani(ref t, 15, ref oa, 0f, 0f, -1f, ref op, rot)) // 85
                                                {
                                                    tr = rot;
                                                    if (!core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, rot)) // 105


                                                        if (t2 >= total)
                                                        {
                                                            if (!core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.60f), rot))
                                                                core.Ani(ref t, 13, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0.60f), new Vector3(0f, 0f, 0f), rot, core.tweenType.line);
                                                        }
                                                        else
                                                        {
                                                            core.Ani(ref t, 33);
                                                        }
                                                }
                                            }
                                        }
                        }

                        rot = tr;
                        break;

                    case aniType.gameCeremony:

                        // need 159 tick

                        r = core.Rot90(rot);
                        tr = rot;

                        if (!core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.60f), rot))
                        {
                            if (!core.Ani(ref t, 13, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0.60f), new Vector3(0f, 0f, 0f), rot, core.tweenType.line))

                                if (!core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.60f), rot))
                                    if (!core.Ani(ref t, 13, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0.60f), new Vector3(0f, 0f, 0f), rot, core.tweenType.line))
                                    {
                                        rot = core.Rot90b(rot);
                                        if (!core.Ani(ref t, 10, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot))
                                        {
                                            rot = core.Rot90b(rot);
                                            if (!core.Ani(ref t, 10, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot))

                                                if (!core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.60f), rot))
                                                    if (!core.Ani(ref t, 13, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0.60f), new Vector3(0f, 0f, 0f), rot, core.tweenType.line))
                                                        if (!core.Ani(ref t, 10, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot))
                                                        {
                                                            rot = core.Rot90b(rot);
                                                            if (!core.Ani(ref t, 10, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot))
                                                            {
                                                                rot = core.Rot90b(rot);
                                                                core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot);
                                                            }
                                                        }
                                        }
                                    }
                        }


                        break;

                    case aniType.idle:
                        t = (Find.TickManager.TicksGame + IdTick * 13) % 800;
                        f = 4.5f;
                        r = core.Rot90(rot);
                        if (!core.Ani(ref t, 500, ref oa, 0f, 0f, -1f, ref op, r))
                            if (!core.Ani(ref t, 25, ref oa, 0f, f, -1f, ref op, r))
                                if (!core.Ani(ref t, 50, ref oa, f, -f, -1f, ref op, r))
                                    if (!core.Ani(ref t, 50, ref oa, -f, f, -1f, ref op, r))
                                        if (!core.Ani(ref t, 50, ref oa, f, -f, -1f, ref op, r))
                                            if (!core.Ani(ref t, 50, ref oa, -f, f, -1f, ref op, r))
                                                if (!core.Ani(ref t, 50, ref oa, f, -f, -1f, ref op, r))
                                                    core.Ani(ref t, 25, ref oa, -f, 0f, -1f, ref op, r);
                        break;

                    case aniType.smash:
                        t = (Find.TickManager.TicksGame + IdTick) % 133;

                        if (!core.Ani(ref t, 70, ref oa, 0f, -20f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot))
                        {
                            if (!core.Ani(ref t, 3, ref oa, -20f, 10f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot, core.tweenType.line))
                                if (!core.Ani(ref t, 20, ref oa, 10f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot))
                                    core.Ani(ref t, 40, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot);
                        }

                        break;

                    case aniType.doSomeThing:
                        total = 121;
                        t2 = (Find.TickManager.TicksGame + IdTick) % (total * 2);
                        t = t2 % total;
                        r = core.Rot90(rot);
                        tr = rot;
                        if (!core.Ani(ref t, 20))
                            if (!core.Ani(ref t, 5, ref oa, 0f, 10f, -1f, ref op, r))
                                if (!core.Ani(ref t, 20, ref oa, 10f, 10f, -1f, ref op, r))
                                    if (!core.Ani(ref t, 5, ref oa, 10f, -10f, -1f, ref op, r))
                                        if (!core.Ani(ref t, 20, ref oa, -10f, -10f, -1f, ref op, r))
                                        {
                                            if (!core.Ani(ref t, 5, ref oa, -10f, 0f, -1f, ref op, r))
                                            {
                                                //tr = t2 >= total ? core.Rot90(rot) : core.Rot90b(rot);
                                                if (!core.Ani(ref t, 15, ref oa, 0f, 0f, -1f, ref op, rot)) // 85
                                                {
                                                    //tr = rot;
                                                    if (!core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, rot)) // 105
                                                        if (!core.Ani(ref t, 5, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.05f), rot))
                                                            core.Ani(ref t, 6, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0.05f), new Vector3(0f, 0f, 0f), rot);
                                                }
                                            }
                                        }

                        rot = tr;
                        break;


                    case aniType.social:
                        total = 221;
                        t2 = (Find.TickManager.TicksGame + IdTick) % (total * 2);
                        t = t2 % total;
                        r = core.Rot90(rot);
                        tr = rot;
                        if (!core.Ani(ref t, 20))
                            if (!core.Ani(ref t, 5, ref oa, 0f, 10f, -1f, ref op, r))
                                if (!core.Ani(ref t, 20, ref oa, 10f, 10f, -1f, ref op, r))
                                    if (!core.Ani(ref t, 5, ref oa, 10f, -10f, -1f, ref op, r))
                                        if (!core.Ani(ref t, 20, ref oa, -10f, -10f, -1f, ref op, r))
                                        {
                                            if (!core.Ani(ref t, 5, ref oa, -10f, 0f, -1f, ref op, r))
                                            {
                                                tr = t2 >= total ? core.Rot90(rot) : core.Rot90b(rot);
                                                if (!core.Ani(ref t, 15, ref oa, 0f, 0f, -1f, ref op, rot)) // 85
                                                {
                                                    tr = rot;
                                                    if (!core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, rot)) // 105
                                                        if (!core.Ani(ref t, 5, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.05f), rot))
                                                            if (!core.Ani(ref t, 6, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0.05f), new Vector3(0f, 0f, 0f), rot))

                                                                if (!core.Ani(ref t, 35, ref oa, 0f, 0f, -1f, ref op, rot))
                                                                    if (!core.Ani(ref t, 10, ref oa, 0f, 10f, -1f, ref op, rot))
                                                                        if (!core.Ani(ref t, 10, ref oa, 10f, 0f, -1f, ref op, rot))
                                                                            if (!core.Ani(ref t, 10, ref oa, 0f, 10f, -1f, ref op, rot))
                                                                                if (!core.Ani(ref t, 10, ref oa, 10f, 0f, -1f, ref op, rot))
                                                                                    core.Ani(ref t, 25, ref oa, 0f, 0f, -1f, ref op, rot);
                                                }
                                            }
                                        }

                        rot = tr;
                        break;
                }
            }

            if (changed)
            {
                pdd.offset_angle = oa;
                pdd.fixed_rot = rot;
                op = new Vector3(op.x, 0f, op.z);
                pdd.offset_pos = op;
                pos += op;
            }
            else
            {
                pdd.reset();
            }
        }


        public static void ani1(Pawn pawn, ref Vector3 pos, Rot4 rot)
        {
            try
            {
                pawnDrawData pdd = dataUtility.GetData(pawn);
                float oa = 0f;
                Vector3 op = Vector3.zero;
                bool changed = false;


                if (pawn.CurJob != null)
                {
                    changed = true;

                    if (core.settings.val_debug)
                        if (pawn.IsColonist)
                            Log.Message($"{pawn.NameShortColored} : {pawn.CurJob.def.defName}");


                    int IdTick = pawn.thingIDNumber * 20;
                    pdd.forcedShowBody = false;

                    switch (pawn.CurJob.def.defName)
                    {
                        case "Lovin": // 사랑나누기
                            if (!core.settings.val_lovin) return;
                            Building_Bed building_Bed = pawn.CurrentBed();
                            if (building_Bed == null) return;
                            var t = (Find.TickManager.TicksGame + IdTick % 30) % 360;
                            var f = 0.03f;
                            if (pawn.RaceProps.Humanlike)
                            {
                                rot = core.getRot(pawn.CurJob.targetA.Pawn.DrawPos - pawn.DrawPos, building_Bed.Rotation);

                                if (t <= 160)
                                {
                                    if (!core.Ani(ref t, 20, ref oa, 0f, 5f, -1f, ref op, Vector3.zero, new Vector3(0.05f, 0f, 0.05f), rot, core.tweenType.sin, building_Bed.Rotation))
                                        if (!core.Ani(ref t, 50, ref oa, 5f, 10f, -1f, ref op, new Vector3(0.05f, 0f, 0.05f), new Vector3(0.05f, 0f, 0.1f), rot, core.tweenType.sin,
                                                building_Bed.Rotation))
                                            if (!core.Ani(ref t, 50, ref oa, 10, 10f, -1f, ref op, new Vector3(0.05f, 0f, 0.1f), new Vector3(0.05f, 0f, 0.1f), rot, core.tweenType.sin,
                                                    building_Bed.Rotation))
                                                core.Ani(ref t, 40, ref oa, 10f, 0f, -1f, ref op, new Vector3(0.05f, 0f, 0.1f), Vector3.zero, rot, core.tweenType.sin, building_Bed.Rotation);
                                }
                                else
                                {
                                    t = (Find.TickManager.TicksGame + IdTick) % 40;
                                    if (!core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, f), new Vector3(0f, 0f, -f), rot, core.tweenType.sin, building_Bed.Rotation))
                                        core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, -f), new Vector3(0f, 0f, f), rot, core.tweenType.sin, building_Bed.Rotation);
                                }
                            }


                            break;

                        case "LayDown": // 잠자기
                            if (!core.settings.val_sleep) return;
                            if (!(pawn.jobs?.curDriver?.asleep ?? false)) return;

                            int seed = ((Find.TickManager.TicksGame + IdTick * 5) / 2500 + IdTick * 5);
                            rot = Rand.RangeSeeded(0, 4, seed) switch
                            {
                                0 => Rot4.East,
                                1 => Rot4.West,
                                2 => Rot4.South,
                                3 => Rot4.North,
                                _ => rot
                            };

                            switch (Rand.RangeSeeded(0, 3, seed + 100))
                            {
                                case 0:
                                    op = new Vector3(Rand.RangeSeeded(-0.1f, 0.1f, seed + 50), 0f, Rand.RangeSeeded(-0.1f, 0.1f, seed + 100));
                                    break;
                                case 1:
                                    op = new Vector3(Rand.RangeSeeded(-0.2f, 0.2f, seed + 150), 0f, Rand.RangeSeeded(-0.1f, 0.1f, seed + 200));
                                    break;
                                case 2:
                                    op = new Vector3(Rand.RangeSeeded(-0.3f, 0.3f, seed + 250), 0f, Rand.RangeSeeded(-0.2f, 0.2f, seed + 300));
                                    pdd.forcedShowBody = true;
                                    break;
                            }

                            oa = Rand.RangeSeeded(0, 3, seed + 200) switch
                            {
                                0 => Rand.RangeSeeded(-15f, 15f, seed),
                                1 => Rand.RangeSeeded(-15f, 15f, seed),
                                2 => Rand.RangeSeeded(-45f, 45f, seed),
                                _ => oa
                            };

                            break;
                    }
                }

                if (changed)
                {
                    pdd.offset_angle = oa;
                    pdd.fixed_rot = rot;
                    op = new Vector3(op.x, 0f, op.z);
                    pdd.offset_pos = op;
                    pos += op;
                }
                else
                {
                    pdd.reset();
                }
            }
            catch
            {
                dataUtility.GetData(pawn).reset();
            }
        }
    }


    // ---------------------------------------------------
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnAt")]
    public class Patch_PawnRenderer_RenderPawnAt
    {
        [HarmonyPriority(0)]
        [UsedImplicitly]
        public static void Prefix(PawnRenderer __instance, Pawn ___pawn, ref Vector3 drawLoc, Rot4? rotOverride = null, bool neverAimWeapon = false)
        {
            dataUtility.GetData(___pawn);
            yayo.checkAni(___pawn, ref drawLoc, rotOverride ?? ___pawn.Rotation);
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "DrawDynamicParts")]
    public class Patch_PawnRenderer_DrawDynamicParts
    {
        [HarmonyPriority(0)]
        [UsedImplicitly]
        public static void Prefix(PawnRenderer __instance, ref Vector3 rootLoc, ref float angle, ref Rot4 pawnRotation, PawnRenderFlags flags, Pawn ___pawn)
        {
            if (___pawn.GetPosture() == PawnPosture.Standing)
            {
                var pdd = dataUtility.GetData(___pawn);
                angle += pdd.offset_angle;
                pawnRotation = pdd.fixed_rot ?? pawnRotation;
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal")]
    public class Patch_PawnRenderer_RenderPawnInternal
    {
        public static bool skipPatch = false;

        [HarmonyPriority(0)]
        [UsedImplicitly]
        public static void Prefix(PawnRenderer __instance, ref Vector3 rootLoc, ref float angle, bool renderBody, ref Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, Pawn ___pawn)
        {
            if (skipPatch)
            {
                skipPatch = false;
                return;
            }

            if (___pawn.GetPosture() == PawnPosture.Standing)
            {
                var pdd = dataUtility.GetData(___pawn);
                angle += pdd.offset_angle;
                bodyFacing = pdd.fixed_rot ?? bodyFacing;
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "RenderCache")]
    public class Patch_PawnRenderer_RenderCache
    {
        [HarmonyPriority(0)]
        [UsedImplicitly]
        public static bool Prefix(PawnRenderer __instance, Pawn ___pawn, Dictionary<Apparel, (Color, bool)> ___tmpOriginalColors, Rot4 rotation, ref float angle, Vector3 positionOffset,
            bool renderHead, bool renderBody, bool portrait, bool renderHeadgear, bool renderClothes, Dictionary<Apparel, Color> overrideApparelColor = null, Color? overrideHairColor = null,
            bool stylingStation = false)
        {
            Vector3 zero = Vector3.zero;
            //PawnRenderFlags pawnRenderFlags = GetDefaultRenderFlags(pawn);
            PawnRenderFlags pawnRenderFlags = __instance.GetDefaultRenderFlags(___pawn);
            if (portrait)
            {
                pawnRenderFlags |= PawnRenderFlags.Portrait;
            }

            pawnRenderFlags |= PawnRenderFlags.Cache;
            pawnRenderFlags |= PawnRenderFlags.DrawNow;
            if (!renderHead)
            {
                pawnRenderFlags |= PawnRenderFlags.HeadStump;
            }

            if (renderHeadgear)
            {
                pawnRenderFlags |= PawnRenderFlags.Headgear;
            }

            if (renderClothes)
            {
                pawnRenderFlags |= PawnRenderFlags.Clothes;
            }

            if (stylingStation)
            {
                pawnRenderFlags |= PawnRenderFlags.StylingStation;
            }

            ___tmpOriginalColors.Clear();
            try
            {
                if (overrideApparelColor != null)
                {
                    foreach (var (key, value) in overrideApparelColor)
                    {
                        CompColorable compColorable = key.TryGetComp<CompColorable>();
                        if (compColorable != null)
                        {
                            ___tmpOriginalColors.Add(key, (compColorable.Color, compColorable.Active));
                            key.SetColor(value);
                        }
                    }
                }

                Color hairColor = Color.white;
                if (___pawn.story != null)
                {
                    hairColor = ___pawn.story.hairColor;
                    if (overrideHairColor.HasValue)
                    {
                        ___pawn.story.hairColor = overrideHairColor.Value;
                        ___pawn.Drawer.renderer.graphics.CalculateHairMats();
                    }
                }

                //RenderPawnInternal(zero + positionOffset, angle, renderBody, rotation, ___CurRotDrawMode, pawnRenderFlags);
                Patch_PawnRenderer_RenderPawnInternal.skipPatch = true;
                RotDrawMode CurRotDrawMode = __instance.CurRotDrawMode;
                __instance.RenderPawnInternal(zero + positionOffset, angle, renderBody, rotation, CurRotDrawMode, pawnRenderFlags);
                foreach (KeyValuePair<Apparel, (Color, bool)> tmpOriginalColor in ___tmpOriginalColors)
                {
                    if (!tmpOriginalColor.Value.Item2)
                    {
                        tmpOriginalColor.Key.TryGetComp<CompColorable>().Disable();
                    }
                    else
                    {
                        tmpOriginalColor.Key.SetColor(tmpOriginalColor.Value.Item1);
                    }
                }

                if (___pawn.story != null && overrideHairColor.HasValue)
                {
                    ___pawn.story.hairColor = hairColor;
                    ___pawn.Drawer.renderer.graphics.CalculateHairMats();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error rendering pawn portrait: " + ex.Message);
            }
            finally
            {
                ___tmpOriginalColors.Clear();
            }

            return false;
        }
    }


    [HarmonyPatch(typeof(PawnRenderer), "GetBodyPos")]
    internal class Patch_PawnRenderer_GetBodyPos
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void Postfix(PawnRenderer __instance, ref Vector3 __result, Vector3 drawLoc, ref bool showBody, Pawn ___pawn)
        {
            var pdd = dataUtility.GetData(___pawn);
            __result += pdd.offset_pos;
            if (pdd.forcedShowBody) showBody = true;
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "BodyAngle")]
    internal class Patch_PawnRenderer_BodyAngle
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void Postfix(PawnRenderer __instance, ref float __result, Pawn ___pawn)
        {
            var pdd = dataUtility.GetData(___pawn);
            __result += pdd.offset_angle;
        }
    }


    [HarmonyPatch(typeof(PawnRenderer), "LayingFacing")]
    internal class Patch_PawnRenderer_LayingFacing
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void Postfix(PawnRenderer __instance, ref Rot4 __result, Pawn ___pawn)
        {
            var pdd = dataUtility.GetData(___pawn);
            __result = pdd.fixed_rot ?? __result;
        }
    }


    /*
	// ---------------------------------------------------

	[HarmonyPatch(typeof(PawnRenderer), "RenderPawnAt")]
    public class Patch_PawnRenderer_RenderPawnAt
    {
        static Pawn pawn;

        [HarmonyPriority(0)]
        public static bool Prefix(PawnRenderer __instance, Vector3 drawLoc, Rot4? rotOverride = null, bool neverAimWeapon = false)
        {

            pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            if (!__instance.graphics.AllResolved)
            {
                __instance.graphics.ResolveAllGraphics();
            }

            Rot4 rot = rotOverride ?? pawn.Rotation;
            PawnRenderFlags pawnRenderFlags = (PawnRenderFlags)AccessTools.Method(typeof(PawnRenderer), "GetDefaultRenderFlags").Invoke(__instance, new object[] { pawn });
            if (neverAimWeapon)
            {
                pawnRenderFlags |= PawnRenderFlags.NeverAimWeapon;
            }

            RotDrawMode curRotDrawMode = Traverse.Create(__instance).Property("CurRotDrawMode").GetValue<RotDrawMode>();
            bool flag = pawn.RaceProps.Humanlike && curRotDrawMode != RotDrawMode.Dessicated && !pawn.IsInvisible();
            PawnTextureAtlasFrameSet frameSet = null;
            if (flag && !GlobalTextureAtlasManager.TryGetPawnFrameSet(pawn, out frameSet, out var _))
            {
                flag = false;
            }

            int IdTick = pawn.thingIDNumber * 100;

            if (pawn.GetPosture() == PawnPosture.Standing)
            {
                // yayo
                float angle = 0f;
                yayo.ani0(pawn, ref drawLoc, ref rot, out angle);


                if (flag)
                {
                    Material original = MaterialPool.MatFrom(new MaterialRequest(frameSet.atlas, ShaderDatabase.Cutout));
                    original = (Material)AccessTools.Method(typeof(PawnRenderer), "OverrideMaterialIfNeeded").Invoke(__instance, new object[] { original, pawn, false });

					// need fix
					GenDraw.DrawMeshNowOrLater((Mesh)AccessTools.Method(typeof(PawnRenderer), "GetBlitMeshUpdatedFrame").Invoke(__instance, new object[] { frameSet, rot, PawnDrawMode.BodyAndHead }), drawLoc, Quaternion.AngleAxis(angle, Vector3.up), original, drawNow: false);

					// need fix
					AccessTools.Method(typeof(PawnRenderer), "DrawDynamicParts").Invoke(__instance, new object[] { drawLoc, angle, rot, pawnRenderFlags });
                }
                else
                {
					// need fix
					AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal").Invoke(__instance, new object[] { drawLoc, angle, true, rot, curRotDrawMode, pawnRenderFlags });
                }
				AccessTools.Method(typeof(PawnRenderer), "DrawCarriedThing").Invoke(__instance, new object[] { drawLoc });
                if (!pawnRenderFlags.FlagSet(PawnRenderFlags.Invisible))
                {
					AccessTools.Method(typeof(PawnRenderer), "DrawInvisibleShadow").Invoke(__instance, new object[] { drawLoc });
                }
            }
            else
            {
                bool showBody = true;
                Vector3 bodyPos = (Vector3)AccessTools.Method(typeof(PawnRenderer), "GetBodyPos").Invoke(__instance, new object[] { drawLoc, showBody });
                float angle = __instance.BodyAngle();
                Rot4 rot2 = __instance.LayingFacing();

				// yayo
				yayo.ani1(pawn, ref bodyPos, ref rot2, ref angle, ref showBody);


                if (flag)
                {
                    Material original2 = MaterialPool.MatFrom(new MaterialRequest(frameSet.atlas, ShaderDatabase.Cutout));
                    original2 = (Material)AccessTools.Method(typeof(PawnRenderer), "OverrideMaterialIfNeeded").Invoke(__instance, new object[] { original2, pawn, false });

					// need fix
					GenDraw.DrawMeshNowOrLater((Mesh)AccessTools.Method(typeof(PawnRenderer), "GetBlitMeshUpdatedFrame").Invoke(__instance, new object[] { frameSet, rot2, (!showBody) ? PawnDrawMode.HeadOnly : PawnDrawMode.BodyAndHead }), bodyPos, Quaternion.AngleAxis(angle, Vector3.up), original2, drawNow: false);
					// need fix
					AccessTools.Method(typeof(PawnRenderer), "DrawDynamicParts").Invoke(__instance, new object[] { bodyPos, angle, rot, pawnRenderFlags });
                }
                else
                {
					// need fix
					AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal").Invoke(__instance, new object[] { bodyPos, angle, showBody, rot2, curRotDrawMode, pawnRenderFlags });
                }
            }
            if (pawn.Spawned && !pawn.Dead)
            {
                pawn.stances.StanceTrackerDraw();
                pawn.pather.PatherDraw();
                pawn.roping.RopingDraw();
            }

            AccessTools.Method(typeof(PawnRenderer), "DrawDebug").Invoke(__instance, new object[] { });
            return false;
        }
    }
	*/
}