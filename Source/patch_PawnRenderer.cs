using RimWorld;
using UnityEngine;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace yayoAni
{

    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
    internal class patch_DrawEquipment
    {
        [HarmonyPriority(0)]
        [HarmonyPrefix]
        [UsedImplicitly]
        static bool Prefix(PawnRenderer __instance, Vector3 rootLoc)
        {
            if (!core.val_combat)
            {
                return true;
            }
            Pawn pawn = __instance.pawn;



            if (pawn.Dead || !pawn.Spawned)
            {
                return false;
            }
            if (pawn.equipment == null || pawn.equipment.Primary == null)
            {
                return false;
            }
            if (pawn.CurJob != null && pawn.CurJob.def.neverShowWeapon)
            {
                return false;
            }
            


            // duelWeld
            ThingWithComps offHandEquip = null;
            if (core.using_dualWeld)
            {
                if (pawn.equipment.TryGetOffHandEquipment(out ThingWithComps result))
                {
                    offHandEquip = result;
                }
            }


            


            // 주무기
            Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
            PawnRenderer_override.animateEquip(__instance, pawn, rootLoc, pawn.equipment.Primary, stance_Busy, new Vector3(0f, 0f, 0.0005f));

            // 보조무기
            if (offHandEquip != null)
            {
                Stance_Busy offHandStance = null;
                if (pawn.GetStancesOffHand() != null)
                {
                    offHandStance = pawn.GetStancesOffHand().curStance as Stance_Busy;
                }
                PawnRenderer_override.animateEquip(__instance, pawn, rootLoc, offHandEquip, offHandStance, new Vector3(0.1f, 0.1f, 0f), true);
            }

            return false;
        }
    }



    public static class PawnRenderer_override
    {
        public static void animateEquip(PawnRenderer __instance, Pawn pawn, Vector3 rootLoc, ThingWithComps thing, Stance_Busy stance_Busy, Vector3 offset, bool isSub = false)
        {


            Vector3 rootLoc2 = rootLoc;

            bool isMechanoid = pawn.RaceProps.IsMechanoid;

            offset.z += (pawn.Rotation == Rot4.North) ? (-0.00289575267f) : 0.03474903f;

            // 설정과 무기 무게에 따른 회전 애니메이션 사용 여부
            bool useTwirl = core.val_combatTwirl && !pawn.RaceProps.IsMechanoid && thing.def.BaseMass < 5f;

            if (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
            {
                if (thing.def.IsRangedWeapon && !stance_Busy.verb.IsMeleeAttack)
                {

                    // 원거리용

                    //Log.Message((pawn.LastAttackTargetTick + thing.thingIDNumber).ToString());
                    int ticksToNextBurstShot = stance_Busy.verb.ticksToNextBurstShot;
                    int atkType = (pawn.LastAttackTargetTick + thing.thingIDNumber) % 10000 % 1000 % 100 % 5; // 랜덤 공격타입 결정
                    // Stance_Cooldown Stance_Cooldown = pawn.stances.curStance as Stance_Cooldown;
                    Stance_Warmup Stance_Warmup = pawn.stances.curStance as Stance_Warmup;

                    if (ticksToNextBurstShot > 10)
                    {
                        ticksToNextBurstShot = 10;
                    }

                    //atkType = 2; // 공격타입 테스트




                    float ani_burst = (float)ticksToNextBurstShot;
                    float ani_cool = (float)stance_Busy.ticksLeft;

                    float ani = 0f;
                    if (!isMechanoid)
                    {
                        ani = Mathf.Max(ani_cool, 25f) * 0.001f;
                    }

                    if (ticksToNextBurstShot > 0)
                    {
                        ani = ani_burst * 0.02f;
                    }

                    float addAngle = 0f;
                    float addX = offset.x;
                    float addY = offset.y;


                    // 준비동작 애니메이션
                    if (!isMechanoid)
                    {
                        float wiggle_slow = 0f;
                        if (!isSub)
                        {
                            wiggle_slow = Mathf.Sin(ani_cool * 0.035f) * 0.05f;
                        }
                        else
                        {
                            wiggle_slow = Mathf.Sin(ani_cool * 0.035f + 0.5f) * 0.05f;
                        }
                        
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
                                    if (stance_Busy.ticksLeft > 1)
                                    {
                                        addY += wiggle_slow;
                                    }
                                }

                                break;
                            case 1:
                                // 재장전
                                if (ticksToNextBurstShot == 0)
                                {
                                    if (stance_Busy.ticksLeft > 78)
                                    {

                                    }
                                    else if (stance_Busy.ticksLeft > 48 && Stance_Warmup == null)
                                    {
                                        float wiggle = Mathf.Sin(ani_cool * 0.1f) * 0.05f;
                                        addX += wiggle - 0.2f;
                                        addY += wiggle + 0.2f;
                                        addAngle += wiggle + 30f + ani_cool * 0.5f;
                                    }
                                    else if (stance_Busy.ticksLeft > 40 && Stance_Warmup == null)
                                    {
                                        float wiggle = Mathf.Sin(ani_cool * 0.1f) * 0.05f;
                                        float wiggle_fast = Mathf.Sin(ani_cool) * 0.05f;
                                        addX += wiggle_fast + 0.05f;
                                        addY += wiggle - 0.05f;
                                        addAngle += wiggle_fast * 100f - 15f;

                                    }
                                    else if (stance_Busy.ticksLeft > 1)
                                    {
                                        addY += wiggle_slow;
                                    }

                                }
                                break;
                            default:
                                if (stance_Busy.ticksLeft > 1)
                                {
                                    addY += wiggle_slow;
                                }
                                break;
                        }
                    }




                    Vector3 a;
                    if (stance_Busy.focusTarg.HasThing)
                    {
                        a = stance_Busy.focusTarg.Thing.DrawPos;
                    }
                    else
                    {
                        a = stance_Busy.focusTarg.Cell.ToVector3Shifted();
                    }
                    float num = 0f;
                    if ((a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                    {
                        num = (a - pawn.DrawPos).AngleFlat();
                    }
                    Vector3 drawLoc = Vector3.zero;

                    

                    if (pawn.Rotation == Rot4.South)
                    {
                        drawLoc = rootLoc2 + new Vector3(-addY, offset.z, 0.4f + addX - ani).RotatedBy(num);
                    }
                    if (pawn.Rotation == Rot4.North)
                    {
                        drawLoc = rootLoc2 + new Vector3(-addY, offset.z, 0.4f + addX - ani).RotatedBy(num);
                    }
                    if (pawn.Rotation == Rot4.East)
                    {
                        drawLoc = rootLoc2 + new Vector3(-addY, offset.z, 0.4f + addX - ani).RotatedBy(num);
                    }
                    if (pawn.Rotation == Rot4.West)
                    {
                        drawLoc = rootLoc2 + new Vector3(addY, offset.z, 0.4f + addX - ani).RotatedBy(num);
                    }


                    //drawLoc.y += 0.03787879f;

                    // 반동 계수
                    float reboundFactor = 70f;

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
                    float addAngle = 0f;
                    int atkType = (pawn.LastAttackTargetTick + thing.thingIDNumber) % 10000 % 1000 % 100 % 3; // 랜덤 공격타입 결정

                    //Log.Message("B");
                    //atkType = 1; // 공격 타입 테스트

                    // 공격 타입에 따른 각도
                    switch (atkType)
                    {
                        // 낮을수록 위로, 높을수록 아래로 휘두름
                        default:
                            // 평범
                            addAngle = 0f;
                            break;
                        case 1:
                            // 내려찍기
                            addAngle = 25f;
                            break;
                        case 2:
                            // 머리찌르기
                            addAngle = -25f;
                            break;
                    }
                    //Log.Message("C");
                    // 원거리 무기일경우 각도보정
                    if (thing.def.IsRangedWeapon)
                    {
                        addAngle -= 35f;
                    }

                    //Log.Message("D");

                    float readyZ = 0.2f;



                    //Log.Message("E");
                    if (stance_Busy.ticksLeft > 15)
                    {
                        //Log.Message("F");
                        // 애니메이션
                        Vector3 a;
                        if (stance_Busy.focusTarg.HasThing)
                        {
                            a = stance_Busy.focusTarg.Thing.DrawPos;
                        }
                        else
                        {
                            a = stance_Busy.focusTarg.Cell.ToVector3Shifted();
                        }

                        float num = 0f;
                        if ((a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                        {
                            num = (a - pawn.DrawPos).AngleFlat();
                        }


                        float ani = Mathf.Min((float)stance_Busy.ticksLeft, 60f);
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
                        if (useTwirl && pawn.LastAttackTargetTick % 5 == 0 && stance_Busy.ticksLeft <= 25)
                        {
                            //addAngle += ani2 * 5000f;
                        }

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
                        Vector3 a;
                        if (stance_Busy.focusTarg.HasThing)
                        {
                            a = stance_Busy.focusTarg.Thing.DrawPos;
                        }
                        else
                        {
                            a = stance_Busy.focusTarg.Cell.ToVector3Shifted();
                        }

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
            if ((pawn.carryTracker == null || pawn.carryTracker.CarriedThing == null) && (pawn.Drafted || (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon) || (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon)))
            {
                int tick = Mathf.Abs(pawn.HashOffsetTicks() % 1000000000);
                tick = tick % 100000000;
                tick = tick % 10000000;
                tick = tick % 1000000;

                tick = tick % 100000;
                tick = tick % 10000;
                tick = tick % 1000;
                float wiggle = 0f;
                if (!isSub)
                {
                    wiggle = Mathf.Sin((float)tick * 0.05f);
                }
                else
                {
                    wiggle = Mathf.Sin((float)tick * 0.05f + 0.5f);
                }
                float aniAngle = -5f;

                float addAngle = 0f;

                if (useTwirl)
                {
                    if (!isSub)
                    {
                        if (tick < 80 && tick >= 40)
                        {
                            addAngle += (float)tick * 36f;
                            rootLoc2 += new Vector3(-0.2f, 0f, 0.1f);
                        }
                    }
                    else
                    {
                        if (tick < 40)
                        {
                            addAngle += (float)(tick - 40) * -36f;
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




        
    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
    internal class patch_DrawEquipmentAiming
    {
        [HarmonyPriority(9999)]
        [HarmonyPrefix]
        [UsedImplicitly]
        static bool Prefix(PawnRenderer __instance, Thing eq, Vector3 drawLoc, float aimAngle)
        {
            if (!core.val_combat)
            {
                return true;
            }
            Pawn pawn = __instance.pawn;

            float num = aimAngle - 90f;
            Mesh mesh;



            bool isMeleeAtk = false;
            bool flip = false;

            
            Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;

            bool flag = true;
            if (pawn.CurJob != null && pawn.CurJob.def.neverShowWeapon) flag = false;

            if (flag && stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
            {
                if (pawn.Rotation == Rot4.West)
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
                    num -= 180f;
                    num -= eq.def.equippedAngleOffset;
                }
                else
                {
                    mesh = MeshPool.plane10;
                    num += eq.def.equippedAngleOffset;
                }
            }
            else
            {
                if (aimAngle > 20f && aimAngle < 160f)
                {
                    mesh = MeshPool.plane10;
                    num += eq.def.equippedAngleOffset;
                }
                //else if ((aimAngle > 200f && aimAngle < 340f) || ignore)
                else if ((aimAngle > 200f && aimAngle < 340f) || flip)
                {
                    mesh = MeshPool.plane10Flip;
                    num -= 180f;
                    num -= eq.def.equippedAngleOffset;
                }
                else
                {
                    mesh = MeshPool.plane10;
                    num += eq.def.equippedAngleOffset;
                }
            }



            num %= 360f;

            CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
            if (compEquippable != null)
            {
                EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
                drawLoc += drawOffset;
                num += angleOffset;
            }

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
            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
            Graphics.DrawMesh(material: (graphic_StackCount == null) ? eq.Graphic.MatSingleFor(eq) : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq), mesh: mesh, position: drawLoc, rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0);



            return false;

        }








        





    }


    





}

















