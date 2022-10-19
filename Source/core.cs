using System.Collections.Generic;
using System.Linq;
using HugsLib;
using HugsLib.Settings;
using RimWorld;
using UnityEngine;
using Verse;

namespace yayoAni
{
    public class core : ModBase
    {
        public override string ModIdentifier => "yayoAni";


        private SettingHandle<bool> val_walk_s;
        public static bool val_walk;

        private SettingHandle<float> val_walkSpeed_s;
        public static float val_walkSpeed;

        private SettingHandle<float> val_walkAngle_s;
        public static float val_walkAngle;


        private SettingHandle<bool> val_combat_s;
        public static bool val_combat;

        private SettingHandle<bool> val_combatTwirl_s;
        public static bool val_combatTwirl;


        private SettingHandle<bool> val_anyJob_s;
        public static bool val_anyJob;

        private SettingHandle<bool> val_sleep_s;
        public static bool val_sleep;
        private SettingHandle<bool> val_lovin_s;
        public static bool val_lovin;


        private SettingHandle<bool> val_debug_s;
        public static bool val_debug;


        public static bool using_dualWeld = false;

        static core()
        {
            if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.PackageId.ToLower().Contains("DualWield".ToLower())))
            {
                using_dualWeld = true;
                Log.Message($"# DualWield detected");
            }
        }

        public override void DefsLoaded()
        {
            val_walk_s = Settings.GetHandle<bool>("val_walk", "walk".Translate(), "", true);
            val_walkSpeed_s = Settings.GetHandle<float>("val_walkSpeed", "walk_ani_speed".Translate(), "", 0.8f);
            val_walkAngle_s = Settings.GetHandle<float>("val_walkAngle", "walk_ani_angle".Translate(), "", 0.6f);

            val_combat_s = Settings.GetHandle<bool>("val_combat", "combat".Translate(), "", true);
            val_combatTwirl_s = Settings.GetHandle<bool>("val_combatTwirl", "twirl_weapon".Translate(), "", true);

            val_anyJob_s = Settings.GetHandle<bool>("val_anyJob", "all_job".Translate(), "", true);

            val_sleep_s = Settings.GetHandle<bool>("val_sleep", "sleep".Translate(), "", true);
            val_lovin_s = Settings.GetHandle<bool>("val_lovin", "lovin_bed".Translate(), "", true);

            val_debug_s = Settings.GetHandle<bool>("val_debug", "debug_mode".Translate(), "", false);

            SettingsChanged();
        }

        public override void SettingsChanged()
        {
            val_walk = val_walk_s.Value;
            val_walkSpeed = Mathf.Clamp(val_walkSpeed_s.Value, 0.1f, 10f);
            val_walkAngle = Mathf.Clamp(val_walkAngle_s.Value, 0.1f, 10f);

            val_combat = val_combat_s.Value;
            val_combatTwirl = val_combatTwirl_s.Value;

            val_anyJob = val_anyJob_s.Value;

            val_sleep = val_sleep_s.Value;
            val_lovin = val_lovin_s.Value;

            val_debug = val_debug_s.Value;
        }

        public override void WorldLoaded()
        {
            dataUtility.reset();
        }

        public override void Tick(int currentTick)
        {
            if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
                dataUtility.GC();
        }

        public static Rot4 getRot(Vector3 vel, Rot4 curRot)
        {
            Rot4 r = Rot4.South;
            if (curRot == Rot4.North || curRot == Rot4.East)
            {
                if (Mathf.Abs(vel.x) > Mathf.Abs(vel.z))
                {
                    r = vel.x >= 0 ? Rot4.West : Rot4.East;
                }
                else
                {
                    r = vel.z > 0 ? Rot4.East : Rot4.West;
                }
            }
            else if (curRot == Rot4.South || curRot == Rot4.West)
            {
                if (Mathf.Abs(vel.x) > Mathf.Abs(vel.z))
                {
                    r = vel.x >= 0 ? Rot4.East : Rot4.West;
                }
                else
                {
                    r = vel.z > 0 ? Rot4.West : Rot4.East;
                }
            }

            return r;
        }

        public static bool checkAniTick(ref int tick, int duration)
        {
            if (tick >= duration)
            {
                tick -= duration;
                return false;
            }

            return true;
        }

        const float piHalf = Mathf.PI / 2f;
        const float angleReduce = 0.5f;
        const float angleToPos = 0.01f;

        public enum tweenType
        {
            line,
            sin
        }

        public static bool Ani(ref int tick, int duration, ref float angle, float s_angle, float t_angle, float centerY, ref Vector3 pos, Vector3 s_pos, Vector3 t_pos, Rot4? rot = null,
            tweenType tween = tweenType.sin, Rot4? axis = null)
        {
            if (tick >= duration)
            {
                tick -= duration;
                return false;
            }

            bool needCenterCheck = true;
            if (axis != null)
            {
                if (rot != null)
                {
                    if (rot == Rot4.West)
                    {
                        s_angle = -s_angle;
                        t_angle = -t_angle;
                        s_pos = new Vector3(-s_pos.x, 0f, s_pos.z);
                        t_pos = new Vector3(-t_pos.x, 0f, t_pos.z);
                    }
                }

                if (axis != Rot4.South)
                {
                    needCenterCheck = false;
                }

                if (axis == Rot4.North)
                {
                    //s_angle = -s_angle;
                    //t_angle = -t_angle;
                    s_pos = new Vector3(-s_pos.x, 0f, -s_pos.z);
                    t_pos = new Vector3(-t_pos.x, 0f, -t_pos.z);
                    if (centerY != 0f)
                    {
                        s_pos += new Vector3(s_angle * 0.01f * centerY, 0f, 0f);
                        t_pos += new Vector3(t_angle * 0.01f * centerY, 0f, 0f);
                    }
                }
                else if (axis == Rot4.West)
                {
                    s_pos = new Vector3(s_pos.z, 0f, -s_pos.x);
                    t_pos = new Vector3(t_pos.z, 0f, -t_pos.x);
                    if (centerY != 0f)
                    {
                        s_pos += new Vector3(0f, 0f, s_angle * 0.01f * centerY);
                        t_pos += new Vector3(0f, 0f, t_angle * 0.01f * centerY);
                    }
                }
                else if (axis == Rot4.East)
                {
                    s_pos = new Vector3(-s_pos.z, 0f, s_pos.x);
                    t_pos = new Vector3(-t_pos.z, 0f, t_pos.x);
                    if (centerY != 0f)
                    {
                        s_pos += new Vector3(0f, 0f, -s_angle * 0.01f * centerY);
                        t_pos += new Vector3(0f, 0f, -t_angle * 0.01f * centerY);
                    }
                }
            }
            else if (rot != null)
            {
                if (rot == Rot4.West)
                {
                    s_angle = -s_angle;
                    t_angle = -t_angle;
                    s_pos = new Vector3(-s_pos.x, 0f, s_pos.z);
                    t_pos = new Vector3(-t_pos.x, 0f, t_pos.z);
                }
                else if ((Rot4)rot == Rot4.South)
                {
                    s_angle *= angleReduce;
                    t_angle *= angleReduce;
                    s_pos = new Vector3(0f, 0f, s_pos.z - s_pos.x - s_angle * angleToPos);
                    t_pos = new Vector3(0f, 0f, t_pos.z - t_pos.x - t_angle * angleToPos);
                }
                else if ((Rot4)rot == Rot4.North)
                {
                    s_angle *= -angleReduce;
                    t_angle *= -angleReduce;
                    s_pos = new Vector3(0f, 0f, s_pos.z + s_pos.x - s_angle * angleToPos);
                    t_pos = new Vector3(0f, 0f, t_pos.z + t_pos.x - t_angle * angleToPos);
                }
            }

            if (needCenterCheck && centerY != 0f)
            {
                s_pos += new Vector3(s_angle * -0.01f * centerY, 0f, 0f);
                t_pos += new Vector3(t_angle * -0.01f * centerY, 0f, 0f);
            }

            float tickPer = tween switch
            {
                tweenType.sin => Mathf.Sin(piHalf * (tick / (float)duration)),
                _ => (tick / (float)duration)
            };

            angle += s_angle + (t_angle - s_angle) * tickPer;
            pos += s_pos + (t_pos - s_pos) * tickPer;
            return true;
        }

        public static bool Ani(ref int tick, int duration, ref float angle, float s_angle, float t_angle, float centerY, ref Vector3 pos, Rot4? rot = null, tweenType tween = tweenType.sin)
        {
            return Ani(ref tick, duration, ref angle, s_angle, t_angle, centerY, ref pos, Vector3.zero, Vector3.zero, rot, tween);
        }

        public static bool Ani(ref int tick, int duration, ref float angle, ref Vector3 pos, Vector3 s_pos, Vector3 t_pos, Rot4? rot = null, tweenType tween = tweenType.sin)
        {
            return Ani(ref tick, duration, ref angle, 0f, 0f, 0f, ref pos, s_pos, t_pos, rot, tween);
        }

        public static bool Ani(ref int tick, int duration)
        {
            if (tick >= duration)
            {
                tick -= duration;
                return false;
            }

            return true;
        }

        public static Rot4 Rot90(Rot4 rot) => new(rot.AsInt + 1);
        public static Rot4 Rot90b(Rot4 rot) => new(rot.AsInt - 1);

        static List<LordJob_Ritual> ar_lordJob_ritual = new();

        public static LordJob_Ritual GetPawnRitual(Pawn p)
        {
            ar_lordJob_ritual = Find.IdeoManager.GetActiveRituals(p.Map);
            if (ar_lordJob_ritual == null) return null;
            foreach (LordJob_Ritual l in ar_lordJob_ritual)
            {
                if (l.PawnsToCountTowardsPresence.Contains(p)) return l;
            }

            return null;
        }
    }
}