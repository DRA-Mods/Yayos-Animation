using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using YayoAnimation.Data;

namespace YayoAnimation.HarmonyPatches;

// GameComponent would cause (one time) errors if removed from game
public static class GameComponentUtilityPatch
{
    // TODO: Replace with check for prepatcher once supporting prepatcher
    private static bool IsPatchActive() => true;

    [HarmonyPatch(typeof(GameComponentUtility), nameof(GameComponentUtility.StartedNewGame))]
    [HarmonyPatch(typeof(GameComponentUtility), nameof(GameComponentUtility.LoadedGame))]
    public static class ResetOnStartedOrLoaded
    {
        public static bool Prepare(MethodBase method) => ModsConfig.IsActive("");

        public static void Postfix() => PawnDataUtility.Reset();
    }

    [HarmonyPatch(typeof(GameComponentUtility), nameof(GameComponentUtility.GameComponentTick))]
    public static class DoTicking
    {
        public static bool Prepare(MethodBase method) => IsPatchActive();

        public static void Postfix()
        {
            if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
                PawnDataUtility.GC();
        }
    }
}