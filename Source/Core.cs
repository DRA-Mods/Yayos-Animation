using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using yayoAni.Compat;

namespace yayoAni;

public class Core : Mod
{
    public static YayoAniSettings settings;
    public static Harmony harmony;

#if BIOTECH_PLUS
    public const byte RotNorth = Rot4.NorthInt;
    public const byte RotEast = Rot4.EastInt;
    public const byte RotSouth = Rot4.SouthInt;
    public const byte RotWest = Rot4.WestInt;
#else
    public const byte RotNorth = 0;
    public const byte RotEast = 1;
    public const byte RotSouth = 2;
    public const byte RotWest = 3;
#endif

    public Core(ModContentPack content) : base(content)
    {
        settings = GetSettings<YayoAniSettings>();
        harmony = new Harmony("com.yayo.yayoAni");
        harmony.PatchAll();

#if IDEOLOGY
        if (usingHar && settings.applyHarPatch)
            LongEventHandler.ExecuteWhenFinished(() => HumanoidAlienRaces.SetHarPatch(true));
#endif
#if BIOTECH_PLUS
        if (usingReinforcedMechanoids)
            LongEventHandler.ExecuteWhenFinished(() => ReinforcedMechanoids2.SetReinforcedMechanoidsPatch(settings.combatEnabled));
#endif
    }

    public override string SettingsCategory() => "Yayo's Animation";

    public override void DoSettingsWindowContents(Rect inRect) => settings.DoSettingsWindowContents(inRect);

    public static bool usingDualWield = false;
#if IDEOLOGY
    public static bool usingHar = false;
#endif
    public static bool usingOversizedWeapons = false;
    public static bool usingDeflector = false;
    public static bool usingGiddyUp = false;
    public static bool usingSheathYourSword = false;
#if BIOTECH_PLUS
    // public static bool usingVfeCore = false;
    public static bool usingReinforcedMechanoids = false;
    public static bool usingTacticowl = false;
#endif

    static Core()
    {
        foreach (var mod in ModsConfig.ActiveModsInLoadOrder)
        {
            switch (mod.PackageId.ToLower())
            {
                case "roolo.dualwield":
                    usingDualWield = true;
                    Compat.DualWield.Init();
                    Log.Message("[Yayo's Animation] - DualWield detected");
                    break;
#if IDEOLOGY
                case "erdelf.humanoidalienraces":
                    usingHar = true;
                    Log.Message("[Yayo's Animation] - HumanoidAlienRaces detected");
                    break;
#endif
#if BIOTECH_PLUS
                case "owlchemist.giddyup":
#else
                case "roolo.giddyupcore":
#endif
                    usingGiddyUp = true;
                    Log.Message("[Yayo's Animation] - Giddy-up! detected");
                    break;
#if BIOTECH_PLUS
                case "hlx.reinforcedmechanoids2":
                    usingReinforcedMechanoids = true;
                    Log.Message("[Yayo's Animation] - Reinforced Mechanoids 2 detected");
                    break;
                case "owlchemist.tacticowl":
                    usingTacticowl = true;
                    Log.Message("[Yayo's Animation] - Tacticowl detected");
                    break;
#endif
            }
        }

        OversizedWeapon.CheckOversizedActive();
        Deflector.CheckDeflectorActive();
        SheathYourSword.CheckSheathYourSwordActive();
    }

    public static Rot4 getRot(in Vector3 vel, in Rot4 curRot)
    {
        var r = Rot4.South;
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

    private const float piHalf = Mathf.PI / 2f;
    private const float angleReduce = 0.5f;
    private const float angleToPos = 0.01f;

    public enum tweenType
    {
        line,
        sin
    }

    public static bool Ani(ref int tick, int duration, ref float angle, float s_angle, float t_angle, float centerY, ref Vector3 pos, Vector3 s_pos, Vector3 t_pos, Rot4? rot = null, tweenType tween = tweenType.sin, Rot4? axis = null)
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
            switch (rot.Value.AsByte)
            {
                case RotWest:
                    s_angle = -s_angle;
                    t_angle = -t_angle;
                    s_pos = new Vector3(-s_pos.x, 0f, s_pos.z);
                    t_pos = new Vector3(-t_pos.x, 0f, t_pos.z);
                    break;
                case RotSouth:
                    s_angle *= angleReduce;
                    t_angle *= angleReduce;
                    s_pos = new Vector3(0f, 0f, s_pos.z - s_pos.x - s_angle * angleToPos);
                    t_pos = new Vector3(0f, 0f, t_pos.z - t_pos.x - t_angle * angleToPos);
                    break;
                case RotNorth:
                    s_angle *= -angleReduce;
                    t_angle *= -angleReduce;
                    s_pos = new Vector3(0f, 0f, s_pos.z + s_pos.x - s_angle * angleToPos);
                    t_pos = new Vector3(0f, 0f, t_pos.z + t_pos.x - t_angle * angleToPos);
                    break;
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

    public static bool Ani(ref int tick, int duration, ref int? nextUpdateTick, ref float angle, float t_angle, float centerY, ref Vector3 pos, Vector3 t_pos, Rot4? rot = null, tweenType tween = tweenType.sin, Rot4? axis = null)
    {
        if (!Ani(ref tick, duration, ref angle, t_angle, t_angle, centerY, ref pos, t_pos, t_pos, rot, tween, axis))
            return false;

        nextUpdateTick = Find.TickManager.TicksGame + (tick - duration);
        return true;
    }

    public static bool Ani(ref int tick, int duration, ref float angle, float s_angle, float t_angle, float centerY, ref Vector3 pos, Rot4? rot = null, tweenType tween = tweenType.sin)
    {
        return Ani(ref tick, duration, ref angle, s_angle, t_angle, centerY, ref pos, Vector3.zero, Vector3.zero, rot, tween);
    }

    public static bool Ani(ref int tick, int duration, ref int? nextUpdateTick, ref float angle, float t_angle, float centerY, ref Vector3 pos, Rot4? rot = null, tweenType tween = tweenType.sin)
    {
        if (!Ani(ref tick, duration, ref angle, t_angle, t_angle, centerY, ref pos, Vector3.zero, Vector3.zero, rot, tween))
            return false;

        nextUpdateTick = Find.TickManager.TicksGame + (tick - duration);
        return true;
    }

    public static bool Ani(ref int tick, int duration, ref float angle, ref Vector3 pos, Vector3 s_pos, Vector3 t_pos, Rot4? rot = null, tweenType tween = tweenType.sin)
    {
        return Ani(ref tick, duration, ref angle, 0f, 0f, 0f, ref pos, s_pos, t_pos, rot, tween);
    }

    public static bool Ani(ref int tick, int duration, ref int? nextUpdateTick)
    {
        if (tick >= duration)
        {
            tick -= duration;
            return false;
        }

        nextUpdateTick = Find.TickManager.TicksGame + (tick - duration);
        return true;
    }

    public static LordJob_Ritual GetPawnRitual(Pawn p)
    {
        var ar_lordJob_ritual = Find.IdeoManager.GetActiveRituals(p.Map);
        if (ar_lordJob_ritual == null) return null;
        foreach (var l in ar_lordJob_ritual)
        {
            if (l.PawnsToCountTowardsPresence.Contains(p))
                return l;
        }

        return null;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class HotSwappableAttribute : Attribute
{
}