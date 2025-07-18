using RimWorld;
using UnityEngine;
using Verse;
using YayoAnimation.Compat;
using YayoAnimation.Data;

namespace YayoAnimation;

[HotSwappable]
public static class AnimationCore
{
    private static readonly Vector3 XOffset005 = new(0.05f, 0f, 0f);
    private static readonly Vector3 ZOffset005 = new(0f, 0f, 0.05f);
    private static readonly Vector3 ZOffset05 = new(0, 0f, 0.5f);
    private static readonly Vector3 ZOffset06 = new(0f, 0f, 0.60f);
    private static readonly Vector3 XZOffset005 = new(0.05f, 0f, 0.05f);
    private static readonly Vector3 XOffset005ZOffset01 = new(0.05f, 0f, 0.1f);
    private static readonly Vector3 YOffset01 = new(0f, 0.1f, 0f);
    private static readonly Vector3 ZOffset003 = new(0f, 0f, 0.03f);
    private static readonly Vector3 ZOffsetM003 = new(0f, 0f, -0.03f);

    // private const float Cx = -0.5f;
    // private const float Cy = 0f; // 중심점
    // private const float Gx = -0.25f; // 이동 갭
    // private const float Gy = 0.25f; // 이동 갭
    // private static readonly Vector3 PosStepX = new(Cx + Gx * step, 0f, Cy + Gy * step);
    private static readonly Vector3 PosStepM3 = new(-0.5f + -0.25f * -3, 0f, 0f + 0.25f * -3);
    private static readonly Vector3 PosStepM2 = new(-0.5f + -0.25f * -2, 0f, 0f + 0.25f * -2);
    private static readonly Vector3 PosStepM1 = new(-0.5f + -0.25f * -1, 0f, 0f + 0.25f * -1);
    private static readonly Vector3 PosStep0 = new(-0.5f, 0f, 0f);
    private static readonly Vector3 PosStep1 = new(-0.5f + -0.25f * 1, 0f, 0f + 0.25f * 1);
    private static readonly Vector3 PosStep2 = new(-0.5f + -0.25f * 2, 0f, 0f + 0.25f * 2);
    private static readonly Vector3 PosStep3 = new(-0.5f + -0.25f * 3, 0f, 0f + 0.25f * 3);

    public static void CheckAni(Pawn pawn, Rot4 rot, PawnDrawData pdd)
    {
        if (!pawn.Spawned || pawn.Dead || pawn.Drawer.renderer.HasAnimation)
        {
            pdd.Reset();
            return;
        }

        if (Core.settings.onlyPlayerPawns && pawn.Faction != Faction.OfPlayer || Find.CameraDriver.CurrentZoom > Core.settings.maximumZoomLevel)
        {
            pdd.Reset();
            return;
        }

        var defName = pawn.CurJob?.def?.defName;
        if (pdd.jobName != null && // Make sure we've cached some job before cancelling
            pdd.jobName == defName && // Check if the current pawn's job is the same as cached
            Find.TickManager.TicksGame < pdd.nextUpdateTick && // Check if it's the proper tick to update
            (pawn.pather == null || pawn.pather.MovingNow == false) && // Make sure pawn isn't moving
            (pawn.stances?.curStance is not Stance_Busy busy || busy.neverAimWeapon || !busy.focusTarg.IsValid)) // Make sure the pawn isn't aiming at something
        {
            // pos += pdd.posOffset;
            return;
        }

        try
        {
            if (!AniMovement(pawn, ref rot, pdd, defName))
            {
                if (Core.settings.anyJobEnabled &&
                    defName != null &&
                    (Core.settings.mechanoidJobEnabled || !pawn.RaceProps.IsMechanoid) &&
                    (Core.settings.animalJobEnabled || !pawn.RaceProps.Animal))
                {
                    if (pawn.GetPosture() == PawnPosture.Standing)
                    {
                        AniStanding(pawn, ref rot, pdd, defName);
                    }
                    else
                    {
                        AniLaying(pawn, pdd, defName);
                    }
                }
                else pdd.Reset();
            }
        }
        catch
        {
            pdd.Reset();
        }
    }

    public enum AniType
    {
        none,
        doSomeThing,
        social,
        smash,
        idle,
        wiggleGentle,
        gameCeremony,
        crowd,
        solemn
    }

    public static void AniStanding(Pawn pawn, ref Rot4 rot, PawnDrawData pdd, string defName)
    {
        var oa = 0f;
        var op = Vector3.zero;
        int? nextUpdate = null;

        pdd.jobName = defName;
        var idTick = pawn.thingIDNumber * 20;

        if (Core.settings.debugMode)
            if (pawn.IsColonist)
                Log.Message($"[{Core.ModName}] - {pawn.NameShortColored} : {defName}");

        //if (pawn.IsColonist) Log.Message($"{pawn.NameShortColored} : {defName} / id {pawn.thingIDNumber}");
        //float wiggle = Mathf.Sin((Find.TickManager.TicksGame + IdTick) * 7f / pawn.pather.nextCellCostTotal);
        var t = 0;
        int t2;
        int total;
        var aniType = AniType.none;
        Rot4 r;
        Rot4 tr;

        switch (defName)
        {
            // do something
            case "UseArtifact":
            case "UseNeurotrainer":
            case "LinkPsylinkable":
            case "UseStylingStation":
            case "UseStylingStationAutomatic":
            case "DyeHair":
            case "Wear":
            case "SmoothWall":
            case "UnloadYourInventory":
            case "UnloadInventory":
            case "Uninstall":
            case "Train":
            case "TendPatient":
            case "TendEntity":
            case "ExtractBioferrite":
            case "Tame":
            case "FillFermentingBarrel":
            case "TakeBeerOutOfFermentingBarrel":
            case "TakeBioferriteOutOfHarvester":
            case "StudyItem":
            case "StudyInteract":
            case "AnalyzeItem":
            case "Strip":
            case "SmoothFloor":
            case "SlaveSuppress":
            case "PrisonerAttemptRecruit":
            case "PrisonerEnslave":
            case "PrisonerConvert":
            case "DoBill": // 제작, 조리
            case "Deconstruct":
            case "FinishFrame": // 건설
            case "BuildCubeSculpture":
            case "FillIn":
            case "Equip":
            case "ExtractRelic":
            case "ExtractToInventory":
            case "ExtractSkull":
            case "ExtractTree":
            case "Replant":
            case "GiveSpeech":
            case "AcceptRole":
            case "Hack":
            case "InstallRelic":
            case "Insult":
            case "Milk":
            case "Open":
            case "Play_MusicalInstrument":
            case "PlantSeed":
            case "PruneGauranlenTree":
            case "RearmTurret":
            case "RearmTurretAtomic":
            case "RecolorApparel":
            case "Refuel":
            case "RefuelAtomic":
            case "Reload":
            case "RemoveApparel":
            case "RemoveFloor":
            case "RemoveRoof":
            case "BuildRoof":
            case "Repair":
            case "FixBrokenDownBuilding":
            case "Research":
            case "ApplyTechprint":
            case "InvestigateMonolith":
            case "ActivateMonolith":
            case "OperateDeepDrill":
            case "OperateScanner":
            case "Resurrect":
            case "Sacrifice":
            case "Scarify":
            case "Shear":
            case "Slaughter":
            case "Ignite":
            case "ManTurret":
            case "Clean":
            case "ClearSnow":
            case "BuildSnowman":
            case "HaulToContainer": // Bury pawn
            case "PrepareSkylantern":
            case "PaintBuilding":
            case "PaintFloor":
            case "RemovePaintBuilding":
            case "RemovePaintFloor":
            case "InstallMechlink":
            case "RemoveMechlink":
            case "DisassembleMech":
            case "CreateXenogerm":
            case "ReadDatacore":
            case "ClearPollution":
            case "RepairMech":
            case "Floordrawing":
            // Dubs Paint Shop
            case "PaintThings":
            case "PaintCells":
            // Dubs Bad Hygiene
            case "TriggerFireSprinkler":
            case "emptySeptictank":
            case "emptyLatrine":
            case "LoadWashing":
            case "UnloadWashing":
            case "LoadComposter":
            case "UnloadComposter":
            case "RemoveSewage":
            case "DrainWaterTankJob":
            case "PlaceFertilizer":
            case "DBHPackWaterBottle":
            case "DBHStockpileWaterBottles":
            case "DBHAdministerFluids":
            case "useWashBucket":
            case "washAtCell":
            case "takeShower":
            case "washHands":
            case "washPatient":
            case "RefillTub":
            case "RefillWater:":
            case "cleanBedpan":
            case "clearBlockage":
            // Dubs Rimatomics
            case "RimatomicsResearch":
            case "SuperviseConstruction":
            case "SuperviseResearch":
            case "UpgradeBuilding":
            case "LoadRailgunMagazine":
            case "LoadSilo":
            case "UseReactorConsole":
            case "RemoveFuelModule":
            case "LoadSpentFuel":
            case "UnloadPlutonium":
            // Rimefeller
            case "CleanOil":
            case "SuperviseDrilling":
            case "EmptyAutoclave":
            case "FillAutoclave":
            case "OperateResourceConsole":
            // Vanilla Furniture Core
            case "Play_Arcade":
            case "Play_Piano":
            case "Play_ComputerIndustrial":
            case "Play_ComputerModern":
            // Vanilla Furniture Security
            case "VFES_RearmTrap":
            // Vanilla Factions Ancients
            case "VFEA_Play_AncientEducator":
            // Vanilla Factions Insectoid
            case "VFEI_InsertFirstGenomeJob":
            case "VFEI_InsertSecondGenomeJob":
            case "VFEI_InsertThirdGenomeJob":
            case "VFEM_RefuelSilo":
            // Vanilla Factions Pirates
            case "VFEP_DoWelding":
            // Vanilla Factions Settlers
            case "Play_FiveFingerFillet":
            // Vanilla Factions Vikings
            case "VFEV_TakeHoneyOutOfApiary":
            case "VFEV_TendToApiary":
            // Vanilla Factions Medieval
            case "VFEM_DigTerrain":
            case "VFEM_FillTerrain":
            // Vanilla Genetics Expanded
            case "GR_UseGenomeExcavator":
            case "GR_HumanoidHybridRecruit":
            case "GR_AnimalDeconstructJob":
            case "GR_AnimalHarvestJob":
            case "GR_InsertIngredients":
            case "GR_InsertGrowthCell":
            case "GR_InsertArchotechGrowthCell":
            // Vanilla Genetics Expanded - More Lab Stuff
            case "GR_UseGenetrainer":
            // Vanilla Hair Expanded
            case "VHE_ChangeHairstyle":
            // Vanilla Ideology - Memes and Structures
            case "VME_DeconstructBuilding":
            case "VME_MaintainInsectNest":
            // Research Reinvented
            case "RR_AnalyseInPlace":
            case "RR_Analyse":
            case "RR_AnalyseTerrain":
            case "RR_Research":
                aniType = AniType.doSomeThing;
                break;


            // social
            case "GotoAndBeSociallyActive":
            case "StandAndBeSociallyActive":
            case "VisitSickPawn":
            case "SocialRelax":
            case "WatchTelevision":
            case "Radiotalking":
            case "Workwatching":
            case "Lessontaking":
            case "Lessongiving":
            case "PlayStatic":
            case "PlayToys":
            case "GoldenCubePlay":
            // Dubs Bad Hygiene
            case "WatchWashingMachine":
            case "DBHGoSwimming":
            case "DBHUseSauna":
            // Vanilla Expanded Classical
            case "VFEC_Stage_Performance":
            case "VFEC_Stage_WatchPerformance":
            // Vanilla Genetics Expanded
            case "GR_HumanoidHybridTalk":
            // Vanilla Social Interactions
            case "VSIE_VentToFriend":
            case "VSIE_TalkToSecondPawn":
            case "VSIE_WatchTelevisionTogether":
            // More social variant of those 2 interactions
            case "VSIE_ViewArtTogether":
            case "VSIE_BuildSnowmanTogether":
                aniType = AniType.social;
                break;

            // Vanilla Expanded Classical
            case "VFEC_Relax_Thermaebath":
                var idTickMult = idTick * 3;
                var idTickDiv = (Find.TickManager.TicksGame + idTickMult) / 2500;
                var seed = idTickDiv + idTickMult;
                nextUpdate = (idTickDiv + 1) * 2500 - idTickMult;
                rot = FastRandom.NewNext(0, 4, seed) switch
                {
                    0 => Rot4.East,
                    1 => Rot4.West,
                    2 => Rot4.South,
                    3 => Rot4.North,
                    _ => rot
                };
                aniType = AniType.social;
                break;


            case "Wait_Combat":
            case "Wait":
            case "Wait_SafeTemperature":
            case "Wait_Wander":
            case "ViewArt":
            case "Meditate":
            case "Pray":
            case "MeditatePray": // Meditation for ideology with a deity
            case "Reign": // Meditate royally
            // Dubs Bad Hygiene
            case "haveWildPoo":
            case "UseToilet":
            // Vanilla Fishing Expanded
            case "VCEF_FishJob":
            // Vanilla Books Expanded
            case "VBE_ReadBook":
            // Vanilla Social Interactions
            case "VSIE_StandAndHearVenting": // Don't do any movements that could be interpreted as excitement, etc.
                aniType = AniType.idle;
                break;

            case "BottleFeedBaby":
            case "Breastfeed":
                aniType = AniType.wiggleGentle;
                break;


            case "Vomit":
                t = (Find.TickManager.TicksGame + idTick) % 200;
                if (!Core.Ani(ref t, 25, ref oa, 15f, 35f, -1f, ref op, rot) &&
                    !Core.Ani(ref t, 25, ref oa, 35f, 25f, -1f, ref op, rot) &&
                    !Core.Ani(ref t, 25, ref oa, 25f, 35f, -1f, ref op, rot) &&
                    !Core.Ani(ref t, 25, ref oa, 35f, 25f, -1f, ref op, rot) &&
                    !Core.Ani(ref t, 25, ref oa, 25f, 35f, -1f, ref op, rot) &&
                    !Core.Ani(ref t, 25, ref oa, 35f, 25f, -1f, ref op, rot) &&
                    !Core.Ani(ref t, 25, ref oa, 25f, 35f, -1f, ref op, rot))
                    Core.Ani(ref t, 25, ref oa, 35f, 15f, -1f, ref op, rot);

                break;


            // case "Mate":
            //     break;


            case "MarryAdjacentPawn":
                t = (Find.TickManager.TicksGame) % 310;

                if (!Core.Ani(ref t, 150, ref nextUpdate) &&
                    !Core.Ani(ref t, 20, ref oa, 0f, 5f, -1f, ref op, Vector3.zero, XOffset005, rot) &&
                    !Core.Ani(ref t, 50, ref oa, 5f, 10f, -1f, ref op, XOffset005, XOffset005, rot) &&
                    !Core.Ani(ref t, 50, ref nextUpdate, ref oa, 10f, -1f, ref op, XOffset005, rot))
                    Core.Ani(ref t, 40, ref oa, 10f, 0f, -1f, ref op, XOffset005, Vector3.zero, rot);

                break;
            case "SpectateCeremony": // 각종 행사, 의식 (결혼식, 장례식, 이념행사)
                var ritualJob = Core.GetPawnRitual(pawn);
                if (ritualJob == null) // 기본
                {
                    aniType = AniType.crowd;
                }
                else if (ritualJob.Ritual == null)
                {
                    // 로얄티 수여식 관중
                    aniType = AniType.solemn;
                }
                else
                {
                    aniType = ritualJob.Ritual.def.defName switch
                    {
                        // 장례식
                        "Funeral" => AniType.solemn,
                        _ => AniType.crowd
                    };
                }

                break;
            case "BestowingCeremony": // 로얄티 수여식 받는 대상
            case "VisitGrave":
            case "UseTelescope":
            // Vanilla Social Interactions
            case "VSIE_HonorPawn":
                aniType = AniType.solemn;
                break;


            // case "Dance":
            //     break;


            // joy


            case "Play_Hoopstone":
            case "Play_Horseshoes":
            // Vanilla Furniture Core
            case "Play_DartsBoard":
                t = (Find.TickManager.TicksGame + idTick) % 60;
                if (!Core.Ani(ref t, 30, ref oa, 10f, -20f, -1f, ref op, Vector3.zero, Vector3.zero, rot))
                    Core.Ani(ref t, 30, ref oa, -20f, 10f, -1f, ref op, Vector3.zero, Vector3.zero, rot);

                break;


            case "Play_GameOfUr":
            case "Play_Poker":
            case "Play_Billiards":
            case "Play_Chess":
            // Vanilla Furniture Core
            case "Play_Roulette":
            // Vanilla Factions Ancients
            case "VFEA_Play_AncientFoosballTable":
            // Vanilla Factions Settlers
            case "Play_Faro":
            // Vanilla Factions Vikings
            case "Play_Hnefatafl":
                t = (Find.TickManager.TicksGame + idTick * 27) % 900;
                if (t <= 159)
                    aniType = AniType.gameCeremony;
                else
                    aniType = AniType.doSomeThing;

                break;


            case "ExtinguishSelf": // 스스로 불 끄기

                var tg = 10; // 틱 갭
                t = (Find.TickManager.TicksGame + idTick) % (12 * tg);
                r = Rot4.East;

                const float extinguish = 45f;

                if (!Core.Ani(ref t, tg, ref oa, extinguish, extinguish, -1f, ref op, PosStep0, PosStep1, r, Core.tweenType.line))
                {
                    rot.Rotate(RotationDirection.Clockwise);
                    if (!Core.Ani(ref t, tg, ref oa, extinguish, extinguish, -1f, ref op, PosStep1, PosStep2, r, Core.tweenType.line))
                    {
                        rot.Rotate(RotationDirection.Clockwise);
                        if (!Core.Ani(ref t, tg, ref oa, extinguish, extinguish, -1f, ref op, PosStep2, PosStep3, r, Core.tweenType.line))
                        {
                            rot.Rotate(RotationDirection.Clockwise);
                            // reverse
                            if (!Core.Ani(ref t, tg, ref oa, extinguish, extinguish, -1f, ref op, PosStep3, PosStep2, r, Core.tweenType.line))
                            {
                                rot.Rotate(RotationDirection.Counterclockwise);
                                if (!Core.Ani(ref t, tg, ref oa, extinguish, extinguish, -1f, ref op, PosStep2, PosStep1, r, Core.tweenType.line))
                                {
                                    rot.Rotate(RotationDirection.Counterclockwise);
                                    if (!Core.Ani(ref t, tg, ref oa, extinguish, extinguish, -1f, ref op, PosStep1, PosStep0, r, Core.tweenType.line))
                                    {
                                        rot.Rotate(RotationDirection.Counterclockwise);
                                        if (!Core.Ani(ref t, tg, ref oa, extinguish, extinguish, -1f, ref op, PosStep0, PosStepM1, r, Core.tweenType.line))
                                        {
                                            rot.Rotate(RotationDirection.Counterclockwise);
                                            if (!Core.Ani(ref t, tg, ref oa, extinguish, extinguish, -1f, ref op, PosStepM1, PosStepM2, r, Core.tweenType.line))
                                            {
                                                rot.Rotate(RotationDirection.Counterclockwise);
                                                if (!Core.Ani(ref t, tg, ref oa, extinguish, extinguish, -1f, ref op, PosStepM2, PosStepM3, r, Core.tweenType.line))
                                                {
                                                    rot.Rotate(RotationDirection.Counterclockwise);
                                                    // reverse
                                                    if (!Core.Ani(ref t, tg, ref oa, extinguish, extinguish, -1f, ref op, PosStepM3, PosStepM2, r, Core.tweenType.line))
                                                    {
                                                        rot.Rotate(RotationDirection.Clockwise);
                                                        if (!Core.Ani(ref t, tg, ref oa, extinguish, extinguish, -1f, ref op, PosStepM2, PosStepM1, r, Core.tweenType.line))
                                                        {
                                                            rot.Rotate(RotationDirection.Clockwise);
                                                            Core.Ani(ref t, tg, ref oa, extinguish, extinguish, -1f, ref op, PosStepM1, PosStep0, r, Core.tweenType.line);
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

                break;


            case "Sow": // 씨뿌리기
                t = (Find.TickManager.TicksGame + idTick) % 50;

                if (!Core.Ani(ref t, 35, ref nextUpdate) &&
                    !Core.Ani(ref t, 5, ref oa, 0f, 10f, -1f, ref op, rot))
                    Core.Ani(ref t, 10, ref oa, 10f, 0f, -1f, ref op, rot);

                break;


            case "CutPlant": // 식물 베기
            case "CutPlantDesignated":
            case "Harvest": // 자동 수확
            case "HarvestDesignated": // 수동 수확
                if (pawn.CurJob.targetA.Thing?.def.plant?.IsTree != null && pawn.CurJob.targetA.Thing.def.plant.IsTree)
                    aniType = AniType.smash;
                else
                    aniType = AniType.doSomeThing;

                break;

            case "Mine": // 채굴
            // Dubs Bad Hygiene
            case "TipOverSewage":
            // Vanilla Furniture Core
            case "Play_PunchingBag":
                aniType = AniType.smash;
                break;

            case "Ingest": // 밥먹기
            case "EatAtCannibalPlatter":
            // Dubs Bad Hygiene
            case "DBHDrinkFromGround":
            case "DBHDrinkFromBasin":
            // Vanilla Social Interactions
            case "VSIE_HaveMealTogether":
                t = (Find.TickManager.TicksGame + idTick) % 150;
                if (!Core.Ani(ref t, 10, ref oa, 0f, 15f, -1f, ref op, Vector3.zero, Vector3.zero, rot) &&
                    !Core.Ani(ref t, 10, ref oa, 15f, 0f, -1f, ref op, Vector3.zero, Vector3.zero, rot) &&
                    !Core.Ani(ref t, 10, ref oa, 0f, 0f, -1f, ref op, Vector3.zero, ZOffset003, rot) &&
                    !Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, ZOffset003, ZOffsetM003, rot) &&
                    !Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, ZOffsetM003, ZOffset003, rot) &&
                    !Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, ZOffset003, ZOffsetM003, rot) &&
                    !Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, ZOffsetM003, ZOffset003, rot) &&
                    !Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, ZOffset003, ZOffsetM003, rot))
                    Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, ZOffsetM003, ZOffset003, rot);

                break;

            default:
                nextUpdate = int.MaxValue; // Update on a new job
                break;
        }


        switch (aniType)
        {
            case AniType.solemn:
                t = (Find.TickManager.TicksGame + (idTick % 25)) % 660;

                if (!Core.Ani(ref t, 300, ref nextUpdate) &&
                    !Core.Ani(ref t, 30, ref oa, 0f, 15f, -1f, ref op, Vector3.zero, Vector3.zero, rot) &&
                    !Core.Ani(ref t, 300, ref nextUpdate, ref oa, 15f, -1f, ref op, Vector3.zero, rot))
                    Core.Ani(ref t, 30, ref oa, 15f, 0f, -1f, ref op, Vector3.zero, Vector3.zero, rot);

                break;

            case AniType.crowd:
                total = 143;
                t2 = (Find.TickManager.TicksGame + idTick) % (total * 2);
                t = t2 % total;
                r = rot.Rotated(RotationDirection.Clockwise);
                tr = rot;
                if (!Core.Ani(ref t, 20, ref nextUpdate) &&
                    !Core.Ani(ref t, 5, ref oa, 0f, 10f, -1f, ref op, r) &&
                    !Core.Ani(ref t, 20, ref nextUpdate, ref oa, 10f, -1f, ref op, r) &&
                    !Core.Ani(ref t, 5, ref oa, 10f, -10f, -1f, ref op, r) &&
                    !Core.Ani(ref t, 20, ref nextUpdate, ref oa, -10f, -1f, ref op, r) &&
                    !Core.Ani(ref t, 5, ref oa, -10f, 0f, -1f, ref op, r))
                {
                    tr = t2 >= total
                        ? rot.Rotated(RotationDirection.Clockwise)
                        : rot.Rotated(RotationDirection.Counterclockwise);
                    if (!Core.Ani(ref t, 15, ref nextUpdate, ref oa, 0f, -1f, ref op, rot)) // 85
                    {
                        tr = rot;
                        if (!Core.Ani(ref t, 20, ref nextUpdate, ref oa, 0f, -1f, ref op, rot)) // 105
                            if (t2 >= total)
                            {
                                if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, Vector3.zero, ZOffset06, rot))
                                    Core.Ani(ref t, 13, ref oa, 0f, 0f, -1f, ref op, ZOffset06, Vector3.zero, rot, Core.tweenType.line);
                            }
                            else
                            {
                                Core.Ani(ref t, 33, ref nextUpdate);
                            }
                    }
                }

                rot = tr;
                break;

            case AniType.gameCeremony:

                // need 159 tick

                // r = Core.Rot90(rot);
                // tr = rot;

                if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, Vector3.zero, ZOffset06, rot) &&
                    !Core.Ani(ref t, 13, ref oa, 0f, 0f, -1f, ref op, ZOffset06, Vector3.zero, rot, Core.tweenType.line) &&
                    !Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, Vector3.zero, ZOffset06, rot) &&
                    !Core.Ani(ref t, 13, ref oa, 0f, 0f, -1f, ref op, ZOffset06, Vector3.zero, rot, Core.tweenType.line))
                {
                    rot.Rotate(RotationDirection.Counterclockwise);
                    if (!Core.Ani(ref t, 10, ref nextUpdate, ref oa, 0f, -1f, ref op, Vector3.zero, rot))
                    {
                        rot.Rotate(RotationDirection.Counterclockwise);
                        if (!Core.Ani(ref t, 10, ref nextUpdate, ref oa, 0f, -1f, ref op, Vector3.zero, rot) &&
                            !Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, Vector3.zero, ZOffset06, rot) &&
                            !Core.Ani(ref t, 13, ref oa, 0f, 0f, -1f, ref op, ZOffset06, Vector3.zero, rot, Core.tweenType.line) &&
                            !Core.Ani(ref t, 10, ref nextUpdate, ref oa, 0f, -1f, ref op, Vector3.zero, rot))
                        {
                            rot.Rotate(RotationDirection.Counterclockwise);
                            if (!Core.Ani(ref t, 10, ref nextUpdate, ref oa, 0f, -1f, ref op, Vector3.zero, rot))
                            {
                                rot.Rotate(RotationDirection.Counterclockwise);
                                Core.Ani(ref t, 20, ref nextUpdate, ref oa, 0f, -1f, ref op, Vector3.zero, rot);
                            }
                        }
                    }
                }


                break;

            case AniType.idle:
                t = (Find.TickManager.TicksGame + idTick * 13) % 800;
                const float idleAngle = 4.5f;
                r = rot.Rotated(RotationDirection.Clockwise);
                if (!Core.Ani(ref t, 500, ref nextUpdate, ref oa, 0f, -1f, ref op, r) &&
                    !Core.Ani(ref t, 25, ref oa, 0f, idleAngle, -1f, ref op, r) &&
                    !Core.Ani(ref t, 50, ref oa, idleAngle, -idleAngle, -1f, ref op, r) &&
                    !Core.Ani(ref t, 50, ref oa, -idleAngle, idleAngle, -1f, ref op, r) &&
                    !Core.Ani(ref t, 50, ref oa, idleAngle, -idleAngle, -1f, ref op, r) &&
                    !Core.Ani(ref t, 50, ref oa, -idleAngle, idleAngle, -1f, ref op, r) &&
                    !Core.Ani(ref t, 50, ref oa, idleAngle, -idleAngle, -1f, ref op, r))
                    Core.Ani(ref t, 25, ref oa, -idleAngle, 0f, -1f, ref op, r);
                break;

            case AniType.wiggleGentle:
                t = (Find.TickManager.TicksGame + idTick * 13) % 200;
                const float wiggleGentleAngle = 2.5f;
                r = rot.Rotated(RotationDirection.Clockwise);
                if (!Core.Ani(ref t, 100, ref oa, -wiggleGentleAngle, wiggleGentleAngle, -1f, ref op, r))
                    Core.Ani(ref t, 100, ref oa, wiggleGentleAngle, -wiggleGentleAngle, -1f, ref op, r);
                break;

            case AniType.smash:
                t = (Find.TickManager.TicksGame + idTick) % 133;

                if (!Core.Ani(ref t, 70, ref oa, 0f, -20f, -1f, ref op, Vector3.zero, Vector3.zero, rot) &&
                    !Core.Ani(ref t, 3, ref oa, -20f, 10f, -1f, ref op, Vector3.zero, Vector3.zero, rot, Core.tweenType.line) &&
                    !Core.Ani(ref t, 20, ref oa, 10f, 0f, -1f, ref op, Vector3.zero, Vector3.zero, rot))
                    Core.Ani(ref t, 40, ref nextUpdate, ref oa, 0f, -1f, ref op, Vector3.zero, rot);

                break;

            case AniType.doSomeThing:
                total = 121;
                t2 = (Find.TickManager.TicksGame + idTick) % (total * 2);
                t = t2 % total;
                r = rot.Rotated(RotationDirection.Clockwise);
                tr = rot;
                if (!Core.Ani(ref t, 20, ref nextUpdate) &&
                    !Core.Ani(ref t, 5, ref oa, 0f, 10f, -1f, ref op, r) &&
                    !Core.Ani(ref t, 20, ref nextUpdate, ref oa, 10f, -1f, ref op, r) &&
                    !Core.Ani(ref t, 5, ref oa, 10f, -10f, -1f, ref op, r) &&
                    !Core.Ani(ref t, 20, ref nextUpdate, ref oa, -10f, -1f, ref op, r) &&
                    !Core.Ani(ref t, 5, ref oa, -10f, 0f, -1f, ref op, r) &&
                    //tr = t2 >= total ? core.Rot90(rot) : core.Rot90b(rot);
                    // if (!Core.Ani(ref t, 15, ref nextUpdate, ref oa, 0f, -1f, ref op, rot)) // 85
                    !Core.Ani(ref t, 35, ref nextUpdate, ref oa, 0f, -1f, ref op, rot) &&
                    // 105
                    //tr = rot;
                    // if (!Core.Ani(ref t, 20, ref nextUpdate, ref oa, 0f, -1f, ref op, rot)) // 105
                    !Core.Ani(ref t, 5, ref oa, 0f, 0f, -1f, ref op, Vector3.zero, ZOffset005, rot))
                    Core.Ani(ref t, 6, ref oa, 0f, 0f, -1f, ref op, ZOffset005, Vector3.zero, rot);

                rot = tr;
                break;


            case AniType.social:
                total = 221;
                t2 = (Find.TickManager.TicksGame + idTick) % (total * 2);
                t = t2 % total;
                r = rot.Rotated(RotationDirection.Clockwise);
                tr = rot;
                if (!Core.Ani(ref t, 20, ref nextUpdate) &&
                    !Core.Ani(ref t, 5, ref oa, 0f, 10f, -1f, ref op, r) &&
                    !Core.Ani(ref t, 20, ref nextUpdate, ref oa, 10f, -1f, ref op, r) &&
                    !Core.Ani(ref t, 5, ref oa, 10f, -10f, -1f, ref op, r) &&
                    !Core.Ani(ref t, 20, ref nextUpdate, ref oa, -10f, -1f, ref op, r) &&
                    !Core.Ani(ref t, 5, ref oa, -10f, 0f, -1f, ref op, r))
                {
                    tr = t2 >= total
                        ? rot.Rotated(RotationDirection.Clockwise)
                        : rot.Rotated(RotationDirection.Counterclockwise);
                    if (!Core.Ani(ref t, 15, ref nextUpdate, ref oa, 0f, -1f, ref op, rot)) // 85
                    {
                        tr = rot;
                        if (!Core.Ani(ref t, 20, ref nextUpdate, ref oa, 0f, -1f, ref op, rot) && // 105
                            !Core.Ani(ref t, 5, ref oa, 0f, 0f, -1f, ref op, Vector3.zero, ZOffset005, rot) &&
                            !Core.Ani(ref t, 6, ref oa, 0f, 0f, -1f, ref op, ZOffset005, Vector3.zero, rot) &&
                            !Core.Ani(ref t, 35, ref nextUpdate, ref oa, 0f, -1f, ref op, rot) &&
                            !Core.Ani(ref t, 10, ref oa, 0f, 10f, -1f, ref op, rot) &&
                            !Core.Ani(ref t, 10, ref oa, 10f, 0f, -1f, ref op, rot) &&
                            !Core.Ani(ref t, 10, ref oa, 0f, 10f, -1f, ref op, rot) &&
                            !Core.Ani(ref t, 10, ref oa, 10f, 0f, -1f, ref op, rot))
                            Core.Ani(ref t, 25, ref nextUpdate, ref oa, 0f, -1f, ref op, rot);
                    }
                }

                rot = tr;
                break;
        }

        pdd.angleOffset = oa;
        pdd.fixedRot = rot.IsValid ? rot : null;
        op = new Vector3(op.x, 0f, op.z);
        pdd.posOffset = op;
        // pos += op;
        pdd.nextUpdateTick = nextUpdate ?? Find.TickManager.TicksGame + Core.settings.updateFrequencyTicks;
    }

    public static void AniLaying(Pawn pawn, PawnDrawData pdd, string defName)
    {
        var oa = 0f;
        var op = Vector3.zero;
        var rot = Rot4.Invalid;
        int? nextUpdate = null;

        pdd.jobName = defName;

        if (Core.settings.debugMode)
            if (pawn.IsColonist)
                Log.Message($"[{Core.ModName}] - {pawn.NameShortColored} : {defName}");

        var idTick = pawn.thingIDNumber * 20;
        // pdd.forcedShowBody = false;

        int seed;
        int idTickMult;
        int idTickDiv;
        FastRandom rand;

        switch (defName)
        {
            case "Lovin": // 사랑나누기
            case "VSIE_OneStandLovin":
                if (!Core.settings.lovinEnabled) break;
                var bed = pawn.CurrentBed();
                if (bed == null) break;
                var t = (Find.TickManager.TicksGame + idTick % 30) % 360;
                if (pawn.RaceProps.Humanlike)
                {
                    rot = Core.getRot(pawn.CurJob.targetA.Pawn.DrawPos - pawn.DrawPos, bed.Rotation);

                    if (t <= 160)
                    {
                        if (!Core.Ani(ref t, 20, ref oa, 0f, 5f, -1f, ref op, Vector3.zero, XZOffset005, rot, Core.tweenType.sin, bed.Rotation) &&
                            !Core.Ani(ref t, 50, ref oa, 5f, 10f, -1f, ref op, XZOffset005, XOffset005ZOffset01, rot, Core.tweenType.sin, bed.Rotation) &&
                            !Core.Ani(ref t, 50, ref nextUpdate, ref oa, 10f, -1f, ref op, XOffset005ZOffset01, rot, Core.tweenType.sin, bed.Rotation))
                            Core.Ani(ref t, 40, ref oa, 10f, 0f, -1f, ref op, XOffset005ZOffset01, Vector3.zero, rot, Core.tweenType.sin, bed.Rotation);
                    }
                    else
                    {
                        t = (Find.TickManager.TicksGame + idTick) % 40;
                        if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, ZOffset003, ZOffsetM003, rot, Core.tweenType.sin, bed.Rotation))
                            Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, ZOffsetM003, ZOffset003, rot, Core.tweenType.sin, bed.Rotation);
                    }
                }


                break;

            case "LayDown": // 잠자기
                if (!Core.settings.sleepEnabled) break;
                if (!(pawn.jobs?.curDriver?.asleep ?? false)) break;
                if (pawn.DevelopmentalStage.Newborn() || pawn.DevelopmentalStage.Baby()) break;

                idTickMult = idTick * 5;
                idTickDiv = (Find.TickManager.TicksGame + idTickMult) / 2500;
                seed = idTickDiv + idTickMult;
                nextUpdate = (idTickDiv + 1) * 2500 - idTickMult;

                rand = new FastRandom();
                rot = rand.Next(4, seed) switch
                {
                    0 => Rot4.East,
                    1 => Rot4.West,
                    2 => Rot4.South,
                    _ => Rot4.North,
                };

                switch (rand.Next(3, seed + 100))
                {
                    case 0:
                        op = new Vector3(rand.Next(-0.1f, 0.1f, seed + 50), 0f, rand.Next(-0.1f, 0.1f, seed + 100));
                        break;
                    case 1:
                        op = new Vector3(rand.Next(-0.2f, 0.2f, seed + 150), 0f, rand.Next(-0.1f, 0.1f, seed + 200));
                        break;
                    case 2:
                        op = new Vector3(rand.Next(-0.3f, 0.3f, seed + 250), 0f, rand.Next(-0.2f, 0.2f, seed + 300));
                        // pdd.forcedShowBody = true;
                        break;
                }

                oa = rand.Next(3, seed + 200) switch
                {
                    2 => rand.Next(-45f, 45f, seed),
                    _ => rand.Next(-15f, 15f, seed),
                };

                break;

            case "Skygaze":
            case "Skydreaming":
            case "VSIE_Skygaze":
                seed = pawn.CurJob.loadID + idTick * 5;

                nextUpdate = int.MaxValue;
                rand = new FastRandom();
                op = rand.Next(3, seed + 100) switch
                {
                    0 => new Vector3(rand.Next(-0.1f, 0.1f, seed + 50), 0f, rand.Next(-0.1f, 0.1f, seed + 100)),
                    1 => new Vector3(rand.Next(-0.2f, 0.2f, seed + 150), 0f, rand.Next(-0.1f, 0.1f, seed + 200)),
                    _ => new Vector3(rand.Next(-0.3f, 0.3f, seed + 250), 0f, rand.Next(-0.2f, 0.2f, seed + 300)),
                };

                oa = rand.Next(360f, seed + 200);

                break;

            // // Dubs Bad Hygiene
            // case "takeBath":
            //     rot = Rot4.Invalid;
            //     break;
            case "UseHotTub":
                nextUpdate = int.MaxValue;
                if (FastRandom.NewBool(pawn.CurJob.loadID + idTick))
                {
                    op = ZOffset05;
                    oa = 180f;
                }

                break;
            case "VFEV_HypothermiaResponse":
                if (!Core.settings.sleepEnabled) break;

                idTickMult = idTick * 5;
                idTickDiv = (Find.TickManager.TicksGame + idTickMult) / 2500;
                seed = idTickDiv + idTickMult;
                nextUpdate = (idTickDiv + 1) * 2500 - idTickMult;

                rand = new FastRandom();
                rot = rand.Next(4, seed) switch
                {
                    0 => Rot4.East,
                    1 => Rot4.West,
                    2 => Rot4.South,
                    3 => Rot4.North,
                    _ => rot
                };

                if (rand.Bool(seed + 100))
                    op = new Vector3(rand.Next(-0.1f, 0.1f, seed + 50), 0f, rand.Next(-0.1f, 0.1f, seed + 100));
                else
                    op = new Vector3(rand.Next(-0.2f, 0.2f, seed + 150), 0f, rand.Next(-0.1f, 0.1f, seed + 200));

                break;

            default:
                nextUpdate = int.MaxValue; // Update on a new job
                break;
        }

        pdd.angleOffset = oa;
        pdd.fixedRot = rot.IsValid ? rot : null;
        op = new Vector3(op.x, 0f, op.z);
        pdd.posOffset = op;
        // pos += op;
        pdd.nextUpdateTick = nextUpdate ?? Find.TickManager.TicksGame + Core.settings.updateFrequencyTicks;
    }

    public static bool AniMovement(Pawn pawn, ref Rot4 rot, PawnDrawData pdd, string defName)
    {
        if (pawn.Faction != Faction.OfPlayer && Core.settings.onlyPlayerPawns ||
            Find.CameraDriver.CurrentZoom > Core.settings.maximumZoomLevel ||
            Core.usingGiddyUp && pawn.HasMount())
        {
            pdd.Reset();
            return true;
        }

        if ((pawn.pather.lastMovedTick >= Find.TickManager.TicksGame - 1 && pawn.pather is { MovingNow: true }) || (Core.usingGiddyUp && defName is "Mounted"))
        {
            if (Core.settings.walkEnabled)
            {
                var targetPawn = pawn;
                if (Core.usingGiddyUp && defName is "Mounted" && Core.settings.animalWalkEnabled)
                {
                    var rider = pawn.MountingPawn();
                    if (rider is { pather.MovingNow: true } && rider.pather.lastMovedTick >= Find.TickManager.TicksGame - 1)
                        targetPawn = rider;
                    else
                        targetPawn = null;
                }

                if (targetPawn != null &&
                    (!targetPawn.RaceProps.IsMechanoid || Core.settings.mechanoidWalkEnabled) &&
                    (!targetPawn.RaceProps.Animal || Core.settings.animalWalkEnabled))
                {
                    var idTick = targetPawn.thingIDNumber * 20;
                    var walkSpeed = Core.settings.walkSpeed;
                    if (defName is "Hunt" or "GR_AnimalHuntJob" || (Core.usingGiddyUp && defName is "Mounted"))
                        walkSpeed *= 0.6f;

                    var nextCellCost = targetPawn.pather.nextCellCostTotal;
                    if (nextCellCost <= 0)
                        nextCellCost = Mathf.Epsilon;

                    var wiggle = Mathf.Sin((Find.TickManager.TicksGame + idTick) * 7f * walkSpeed / nextCellCost);
                    // oa = wiggle * 9f * Core.settings.walkAngle;
                    // op = new Vector3(wiggle * 0.025f, 0f, 0f);

                    pdd.angleOffset = wiggle * 9f * Core.settings.walkAngle;
                    pdd.fixedRot = rot.IsValid ? rot : null;
                    // op = new Vector3(op.x, 0f, op.z);
                    pdd.posOffset = new Vector3(wiggle * 0.025f, 0f, 0f);
                    // pos += pdd.posOffset;
                    pdd.nextUpdateTick = Find.TickManager.TicksGame + Core.settings.updateFrequencyTicks;

                    return true;
                }
            }

            pdd.Reset();
            return true;
        }

        return false;
    }
}