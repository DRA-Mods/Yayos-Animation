using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using yayoAni.Data;

namespace yayoAni;

// GameComponent would cause (one time) errors if removed from game
public static class Patch_GameComponentUtility
{
#if BIOTECH_PLUS
    private static bool IsPatchActive() => !ModsConfig.IsActive("zetrith.prepatcher");
#endif

    [HarmonyPatch(typeof(GameComponentUtility), nameof(GameComponentUtility.StartedNewGame))]
    [HarmonyPatch(typeof(GameComponentUtility), nameof(GameComponentUtility.LoadedGame))]
    public static class ResetOnStartedOrLoaded
    {
#if BIOTECH_PLUS
        public static bool Prepare(MethodBase method) => IsPatchActive();
#endif
        
        public static void Postfix() => DataUtility.Reset();
    }

    [HarmonyPatch(typeof(GameComponentUtility), nameof(GameComponentUtility.GameComponentTick))]
    public static class DoTicking
    {
#if BIOTECH_PLUS
        public static bool Prepare(MethodBase method) => IsPatchActive();
#endif

        public static void Postfix()
        {
            if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
                DataUtility.GC();
        }
    }
}