using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using yayoAni.Compat;

namespace yayoAni
{
    [HotSwappable]
    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipment))]
    public class Patch_DrawEquipment
    {
        [HarmonyPriority(0)]
        [HarmonyPrefix]
        private static bool Prefix(PawnRenderer __instance, Vector3 rootLoc)
        {
            if (!Core.settings.combatEnabled)
                return true;

            if (Find.CameraDriver.CurrentZoom > Core.settings.maximumZoomLevel)
                return true;

            Pawn pawn = __instance.pawn;
            if (pawn.Dead || !pawn.Spawned)
                return false;

            if (pawn.Faction != Faction.OfPlayer && Core.settings.onlyPlayerPawns)
                return true;

            if (pawn.RaceProps.IsMechanoid && !Core.settings.mechanoidCombatEnabled)
                return true;

            if (pawn.RaceProps.Animal && !Core.settings.animalCombatEnabled)
                return true;

            if (pawn.equipment?.Primary == null)
                return false;

            if (pawn.CurJob != null && pawn.CurJob.def.neverShowWeapon)
                return false;

            // duelWeld
            ThingWithComps offHandEquip = null;
            if (Core.usingDualWield && pawn.equipment.TryGetOffHandEquipment(out offHandEquip))
            {
            }
            else if (Core.usingOversizedWeapons && pawn.equipment.IsOversizedDualWield()) 
                offHandEquip = pawn.equipment.Primary;

            // 주무기
            Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
            PawnRenderer_Override.AnimateEquip(__instance, pawn, rootLoc, pawn.equipment.Primary, stance_Busy, new Vector3(0f, 0f, 0.0005f));

            // 보조무기
            if (offHandEquip != null)
            {
                Stance_Busy offHandStance = null;
                if (offHandEquip == pawn.equipment.Primary)
                    offHandStance = stance_Busy;
                else if (pawn.GetStancesOffHand() != null) 
                    offHandStance = pawn.GetStancesOffHand().curStance as Stance_Busy;

                PawnRenderer_Override.AnimateEquip(__instance, pawn, rootLoc, offHandEquip, offHandStance, new Vector3(0.1f, 0.1f, 0f), true);
            }

            return false;
        }
    }


    [HotSwappable]
    public static class PawnRenderer_Override
    {
        public static void AnimateEquip(PawnRenderer __instance, Pawn pawn, Vector3 rootLoc, ThingWithComps thing, Stance_Busy stanceBusy, Vector3 offset, bool isSub = false)
        {
            Vector3 rootLoc2 = rootLoc;

            bool isMechanoid = pawn.RaceProps.IsMechanoid;

            offset.z += (pawn.Rotation == Rot4.North) ? (-0.00289575267f) : 0.03474903f;

            // 설정과 무기 무게에 따른 회전 애니메이션 사용 여부
            bool useTwirl = 
                Core.settings.combatTwirlEnabled &&
                !isMechanoid &&
                (!Core.settings.combatTwirlMaxMassEnabled || thing.def.BaseMass <= Core.settings.combatTwirlMaxMass) &&
                (!Core.settings.combatTwirlMaxSizeEnabled || (thing.Graphic.drawSize is var graphic && 
                                                              graphic.x <= Core.settings.combatTwirlMaxSize &&
                                                              graphic.y <= Core.settings.combatTwirlMaxSize));

            if (stanceBusy != null && !stanceBusy.neverAimWeapon && stanceBusy.focusTarg.IsValid)
            {
                if (thing.def.IsRangedWeapon && !stanceBusy.verb.IsMeleeAttack)
                {
                    // 원거리용

                    //Log.Message((pawn.LastAttackTargetTick + thing.thingIDNumber).ToString());
                    int ticksToNextBurstShot = stanceBusy.verb.ticksToNextBurstShot;
                    int atkType = (pawn.LastAttackTargetTick + thing.thingIDNumber) % 10000 % 1000 % 100 % 5; // 랜덤 공격타입 결정
                    // Stance_Cooldown Stance_Cooldown = pawn.stances.curStance as Stance_Cooldown;
                    Stance_Warmup Stance_Warmup = pawn.stances.curStance as Stance_Warmup;

                    if (ticksToNextBurstShot > 10)
                    {
                        ticksToNextBurstShot = 10;
                    }

                    //atkType = 2; // 공격타입 테스트

                    float ani_burst = ticksToNextBurstShot;
                    float ani_cool = stanceBusy.ticksLeft;

                    float ani = 0f;
                    if (!isMechanoid)
                        ani = Mathf.Max(ani_cool, 25f) * 0.001f;

                    if (ticksToNextBurstShot > 0)
                        ani = ani_burst * 0.02f;

                    float addAngle = 0f;
                    float addX = offset.x;
                    float addY = offset.y;


                    // 준비동작 애니메이션
                    if (!isMechanoid)
                    {
                        float wiggleSlow;
                        if (!isSub)
                            wiggleSlow = Mathf.Sin(ani_cool * 0.035f) * 0.05f;
                        else
                            wiggleSlow = Mathf.Sin(ani_cool * 0.035f + 0.5f) * 0.05f;

                        switch (atkType)
                        {
                            case 0:
                                // 회전
                                if (useTwirl)
                                {
                                    /*
                                    if (stance_Busy.ticksLeft < 35 && stance_Busy.ticksLeft > 10 && ticksToNextBurstShot == 0 && Stance_Warmup == null)
                                    {
                                        addAngle += ani_cool * 50f + 180f;
                                    }
                                    else if (stance_Busy.ticksLeft > 1)
                                    {
                                        addY += wiggle_slow;
                                    }
                                    */
                                }
                                else
                                {
                                    if (stanceBusy.ticksLeft > 1)
                                    {
                                        addY += wiggleSlow;
                                    }
                                }

                                break;
                            case 1:
                                // 재장전
                                if (ticksToNextBurstShot == 0)
                                {
                                    switch (stanceBusy.ticksLeft)
                                    {
                                        case > 78:
                                            break;
                                        case > 48 when Stance_Warmup == null:
                                        {
                                            float wiggle = Mathf.Sin(ani_cool * 0.1f) * 0.05f;
                                            addX += wiggle - 0.2f;
                                            addY += wiggle + 0.2f;
                                            addAngle += wiggle + 30f + ani_cool * 0.5f;
                                            break;
                                        }
                                        case > 40 when Stance_Warmup == null:
                                        {
                                            float wiggle = Mathf.Sin(ani_cool * 0.1f) * 0.05f;
                                            float wiggle_fast = Mathf.Sin(ani_cool) * 0.05f;
                                            addX += wiggle_fast + 0.05f;
                                            addY += wiggle - 0.05f;
                                            addAngle += wiggle_fast * 100f - 15f;
                                            break;
                                        }
                                        case > 1:
                                            addY += wiggleSlow;
                                            break;
                                    }
                                }

                                break;
                            default:
                                if (stanceBusy.ticksLeft > 1)
                                {
                                    addY += wiggleSlow;
                                }

                                break;
                        }
                    }

                    Vector3 a = stanceBusy.focusTarg.Thing?.DrawPos ?? stanceBusy.focusTarg.Cell.ToVector3Shifted();
                    float num = 0f;
                    if ((a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                        num = (a - pawn.DrawPos).AngleFlat();

                    Vector3 drawLoc;
                    if (pawn.Rotation == Rot4.West)
                        drawLoc = rootLoc2 + new Vector3(addY, offset.z, 0.4f + addX - ani).RotatedBy(num);
                    else if (pawn.Rotation != Rot4.Invalid)
                        drawLoc = rootLoc2 + new Vector3(-addY, offset.z, 0.4f + addX - ani).RotatedBy(num);
                    else
                        drawLoc = Vector3.zero;


                    //drawLoc.y += 0.03787879f;

                    // 반동 계수
                    const float reboundFactor = 70f;

                    if (pawn.Rotation == Rot4.West)
                        __instance.DrawEquipmentAiming(thing, drawLoc, num + ani * reboundFactor + addAngle);
                    else if (pawn.Rotation != Rot4.Invalid)
                        __instance.DrawEquipmentAiming(thing, drawLoc, num - ani * reboundFactor - addAngle);

                    return;
                }
                else
                {
                    // 근접용

                    //Log.Message("A");
                    int atkType = (pawn.LastAttackTargetTick + thing.thingIDNumber) % 10000 % 1000 % 100 % 3; // 랜덤 공격타입 결정

                    //Log.Message("B");
                    //atkType = 1; // 공격 타입 테스트

                    // 공격 타입에 따른 각도
                    var addAngle = atkType switch
                    {
                        1 =>
                            // 내려찍기
                            25f,
                        2 =>
                            // 머리찌르기
                            -25f,
                        _ => 0f
                    };
                    //Log.Message("C");
                    // 원거리 무기일경우 각도보정
                    if (thing.def.IsRangedWeapon)
                        addAngle -= 35f;

                    //Log.Message("D");

                    const float readyZ = 0.2f;


                    //Log.Message("E");
                    if (stanceBusy.ticksLeft > 15)
                    {
                        //Log.Message("F");
                        // 애니메이션
                        Vector3 a = stanceBusy.focusTarg.Thing?.DrawPos ?? stanceBusy.focusTarg.Cell.ToVector3Shifted();

                        float num = 0f;
                        if ((a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                        {
                            num = (a - pawn.DrawPos).AngleFlat();
                        }

                        float ani = Mathf.Min(stanceBusy.ticksLeft, 60f);
                        float ani2 = ani * 0.0075f; // 0.45f -> 0f
                        float addZ = offset.x;
                        float addX = offset.y;

                        switch (atkType)
                        {
                            default:
                                // 평범한 공격
                                addZ += readyZ + 0.05f + ani2; // 높을 수록 무기를 적쪽으로 내밀음
                                addX += 0.45f - 0.5f - ani2 * 0.1f; // 높을수록 무기를 아래까지 내려침
                                break;
                            case 1:
                                // 내려찍기
                                addZ += readyZ + 0.05f + ani2; // 높을 수록 무기를 적쪽으로 내밀음
                                addX += 0.45f - 0.35f + ani2 * 0.5f; // 높을수록 무기를 아래까지 내려침, 애니메이션 반대방향
                                ani = 30f + ani * 0.5f; // 각도 고정값 + 각도 변화량
                                break;
                            case 2:
                                // 머리찌르기
                                addZ += readyZ + 0.05f + ani2; // 높을 수록 무기를 적쪽으로 내밀음
                                addX += 0.45f - 0.35f - ani2; // 높을수록 무기를 아래까지 내려침
                                break;
                        }

                        // 회전 애니메이션
                        // if (useTwirl && pawn.LastAttackTargetTick % 5 == 0 && stanceBusy.ticksLeft <= 25)
                        // {
                        //     //addAngle += ani2 * 5000f;
                        // }

                        // 캐릭터 방향에 따라 적용
                        if (pawn.Rotation == Rot4.West)
                        {
                            Vector3 drawLoc = rootLoc2 + new Vector3(-addX, offset.z, addZ).RotatedBy(num);
                            //drawLoc.y += 0.03787879f;
                            num -= addAngle;

                            __instance.DrawEquipmentAiming(thing, drawLoc, num - ani);
                        }
                        else if (pawn.Rotation == Rot4.East)
                        {
                            Vector3 drawLoc = rootLoc2 + new Vector3(addX, offset.z, addZ).RotatedBy(num);
                            //drawLoc.y += 0.03787879f;
                            num += addAngle;

                            __instance.DrawEquipmentAiming(thing, drawLoc, num + ani);
                        }
                        else if (pawn.Rotation != Rot4.Invalid)
                        {
                            Vector3 drawLoc = rootLoc2 + new Vector3(-addX, offset.z, addZ).RotatedBy(num);
                            //drawLoc.y += 0.03787879f;
                            num += addAngle;

                            __instance.DrawEquipmentAiming(thing, drawLoc, num + ani);
                        }
                    }
                    else
                    {
                        Vector3 a = stanceBusy.focusTarg.Thing?.DrawPos ?? stanceBusy.focusTarg.Cell.ToVector3Shifted();

                        float num = 0f;
                        if ((a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                        {
                            num = (a - pawn.DrawPos).AngleFlat();
                        }

                        Vector3 drawLoc = rootLoc2 + new Vector3(0f, offset.z, readyZ).RotatedBy(num);
                        //drawLoc.y += 0.03787879f;

                        __instance.DrawEquipmentAiming(thing, drawLoc, num);
                    }

                    return;
                }
            }

            //Log.Message("11");
            // 대기
            if ((pawn.carryTracker?.CarriedThing == null) &&
                (pawn.Drafted || (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon) || (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon)))
            {
                int tick = Mathf.Abs(pawn.HashOffsetTicks() % 1000000000);
                tick %= 100000000;
                tick %= 10000000;
                tick %= 1000000;

                tick %= 100000;
                tick %= 10000;
                tick %= 1000;
                float wiggle;
                if (!isSub)
                    wiggle = Mathf.Sin(tick * 0.05f);
                else
                    wiggle = Mathf.Sin(tick * 0.05f + 0.5f);

                float aniAngle = -5f;
                float addAngle = 0f;

                if (useTwirl)
                {
                    if (!isSub)
                    {
                        if (tick is < 80 and >= 40)
                        {
                            addAngle += tick * 36f;
                            rootLoc2 += new Vector3(-0.2f, 0f, 0.1f);
                        }
                    }
                    else
                    {
                        if (tick < 40)
                        {
                            addAngle += (tick - 40) * -36f;
                            rootLoc2 += new Vector3(0.2f, 0f, 0.1f);
                        }
                    }
                }

                if (pawn.Rotation == Rot4.South)
                {
                    Vector3 drawLoc;
                    float angle;
                    if (!isSub)
                    {
                        drawLoc = rootLoc2 + new Vector3(0f, offset.z, -0.22f + wiggle * 0.05f);
                        angle = 143f;
                    }
                    else
                    {
                        drawLoc = rootLoc2 + new Vector3(0f, offset.z, -0.22f + wiggle * 0.05f);
                        angle = 350f - 143f;
                        aniAngle *= -1f;
                    }

                    //drawLoc2.y += 0.03787879f;
                    __instance.DrawEquipmentAiming(thing, drawLoc, addAngle + angle + wiggle * aniAngle);
                    return;
                }

                if (pawn.Rotation == Rot4.North)
                {
                    Vector3 drawLoc;
                    float angle;
                    if (!isSub)
                    {
                        drawLoc = rootLoc2 + new Vector3(0f, offset.z, -0.11f + wiggle * 0.05f);
                        angle = 143f;
                    }
                    else
                    {
                        drawLoc = rootLoc2 + new Vector3(0f, offset.z, -0.11f + wiggle * 0.05f);
                        angle = 350f - 143f;
                        aniAngle *= -1f;
                    }

                    //drawLoc3.y += 0f;
                    __instance.DrawEquipmentAiming(thing, drawLoc, addAngle + angle + wiggle * aniAngle);
                    return;
                }

                if (pawn.Rotation == Rot4.East)
                {
                    Vector3 drawLoc;
                    float angle;
                    if (!isSub)
                    {
                        drawLoc = rootLoc2 + new Vector3(0.2f, offset.z, -0.22f + wiggle * 0.05f);
                        angle = 143f;
                    }
                    else
                    {
                        drawLoc = rootLoc2 + new Vector3(0.2f, offset.z, -0.22f + wiggle * 0.05f);
                        angle = 350f - 143f;
                        aniAngle *= -1f;
                    }

                    //drawLoc4.y += 0.03787879f;
                    __instance.DrawEquipmentAiming(thing, drawLoc, addAngle + angle + wiggle * aniAngle);
                    return;
                }

                if (pawn.Rotation == Rot4.West)
                {
                    Vector3 drawLoc;
                    float angle;
                    if (!isSub)
                    {
                        drawLoc = rootLoc2 + new Vector3(-0.2f, offset.z, -0.22f + wiggle * 0.05f);
                        angle = 217f;
                    }
                    else
                    {
                        drawLoc = rootLoc2 + new Vector3(-0.2f, offset.z, -0.22f + wiggle * 0.05f);
                        angle = 350f - 217f;
                        aniAngle *= -1f;
                    }

                    //drawLoc5.y += 0.03787879f;
                    __instance.DrawEquipmentAiming(thing, drawLoc, addAngle + angle + wiggle * aniAngle);
                    return;
                }
            }

            return;
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming))]
    internal static class patch_DrawEquipmentAiming
    {
        [HarmonyPriority(9999)]
        [HarmonyPrefix]
        private static bool Prefix(PawnRenderer __instance, Thing eq, Vector3 drawLoc, float aimAngle)
        {
            if (!Core.settings.combatEnabled)
                return true;

            if (Find.CameraDriver.CurrentZoom > Core.settings.maximumZoomLevel)
                return true;

            var pawn = __instance.pawn;

            if (pawn.Faction != Faction.OfPlayer && Core.settings.onlyPlayerPawns)
                return true;

            if (pawn.RaceProps.IsMechanoid && !Core.settings.mechanoidCombatEnabled)
                return true;

            if (pawn.RaceProps.Animal && !Core.settings.animalCombatEnabled)
                return true;

            CompEquippable compEquippable = null;
            ThingComp compOversized = null;

            if (eq is ThingWithComps eqComps)
            {
                foreach (var comp in eqComps.AllComps)
                {
                    if (comp is CompEquippable equippable)
                        compEquippable = equippable;
                    else if (Core.usingOversizedWeapons && comp.IsOversizedComp())
                        compOversized = comp;
                    else if (Core.usingDeflector && comp.IsDeflectorAndAnimatingNow())
                        return false;
                }
            }

            var pawnRotation = pawn.Rotation;

            var currentAngle = aimAngle - 90f;
            Mesh mesh;

            var isMeleeAtk = false;
            var flip = false;

            var stance_Busy = pawn.stances.curStance as Stance_Busy;

            var flag = !(pawn.CurJob != null && pawn.CurJob.def.neverShowWeapon);

            if (flag && stance_Busy is { neverAimWeapon: false, focusTarg: { IsValid: true } })
            {
                if (pawnRotation == Rot4.West)
                {
                    flip = true;
                }

                if (!pawn.equipment.Primary.def.IsRangedWeapon || stance_Busy.verb.IsMeleeAttack)
                {
                    // 근접공격
                    isMeleeAtk = true;
                }
            }

            if (isMeleeAtk)
            {
                if (flip)
                {
                    mesh = MeshPool.plane10Flip;
                    currentAngle -= 180f;
                    currentAngle -= eq.def.equippedAngleOffset;
                }
                else
                {
                    mesh = MeshPool.plane10;
                    currentAngle += eq.def.equippedAngleOffset;
                }
            }
            else
            {
                if (aimAngle is > 20f and < 160f)
                {
                    mesh = MeshPool.plane10;
                    currentAngle += eq.def.equippedAngleOffset;
                }
                //else if ((aimAngle > 200f && aimAngle < 340f) || ignore)
                else if (aimAngle is > 200f and < 340f || flip)
                {
                    flip = true;
                    mesh = MeshPool.plane10Flip;
                    currentAngle -= 180f;
                    currentAngle -= eq.def.equippedAngleOffset;
                }
                else
                {
                    mesh = MeshPool.plane10;
                    currentAngle += eq.def.equippedAngleOffset;
                }
            }

            // Equipment offset - added by Enable Oversized Weapons, by default handles all weapons. Let's do the same (even if it's not active).
            if (eq.StyleDef?.graphicData != null)
                drawLoc += eq.StyleDef.graphicData.DrawOffsetForRot(pawnRotation);
            else
                drawLoc += eq.def.graphicData.DrawOffsetForRot(pawnRotation);

            if (compEquippable != null)
            {
                EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
                drawLoc += drawOffset;
                currentAngle += angleOffset;
            }
            if (Core.settings.applyOversizedChanges)
                compOversized?.HandleOversizedDrawing(ref drawLoc, ref currentAngle, pawn, flip);

            currentAngle %= 360f;

            // Material matSingle;
            //if (graphic_StackCount != null)
            //{
            //    matSingle = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle;
            //}
            //else
            //{
            //    matSingle = eq.Graphic.MatSingle;
            //}
            //Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0);

            Vector3 size = new(eq.Graphic.drawSize.x, 1f, eq.Graphic.drawSize.y);
            var mat = eq.Graphic is Graphic_StackCount graphicStackCount
                ? graphicStackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq)
                : eq.Graphic.MatSingleFor(eq);

            var matrix = Matrix4x4.TRS(drawLoc, Quaternion.AngleAxis(currentAngle, Vector3.up), size);

            Graphics.DrawMesh(
                mesh: mesh,
                matrix: matrix,
                material: mat,
                layer: 0);

            return false;
        }
    }
}
