﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using LudeonTK;
using Verse;

namespace YayoAnimation;

public static class DebugTools
{
    private static HashSet<string> TempSet;

    [DebugAction("Yayo's Animation", "Log unsupported jobs", allowedGameStates = AllowedGameStates.Entry)]
    private static void LogAllMissingJobs()
    {
        try
        {
            Core.harmony.Patch(AccessTools.Method(typeof(AnimationCore), nameof(AnimationCore.AniStanding)),
                transpiler: new HarmonyMethod(typeof(DebugTools), nameof(Transpiler)));
        }
        catch (Exception)
        {
            // ignored
        }

        var normalJobs = TempSet;

        try
        {
            Core.harmony.Patch(AccessTools.Method(typeof(AnimationCore), nameof(AnimationCore.AniLaying)),
                transpiler: new HarmonyMethod(typeof(DebugTools), nameof(Transpiler)));
        }
        catch (Exception)
        {
            // ignored
        }

        var layingDownJobs = TempSet;
        TempSet = null;

        var defNames = DefDatabase<JobDef>.AllDefs.Select(def => def.defName).Where(name => name != null).ToList();
        var defNamesSet = defNames.ToHashSet();

        if (!normalJobs.EnumerableNullOrEmpty())
            defNames.RemoveAll(job => normalJobs.Contains(job));
        if (!layingDownJobs.EnumerableNullOrEmpty())
            defNames.RemoveAll(job => layingDownJobs.Contains(job));

        Log.Message($"[{Core.ModName}] - Jobs potentially missing animations:\n{defNames.ToStringSafeEnumerable()}");
        if (!normalJobs.EnumerableNullOrEmpty())
            Log.Message($"[{Core.ModName}] - Normal animations for jobs that may not exist (from inactive mods?):\n{normalJobs.Where(job => !defNamesSet.Contains(job)).ToStringSafeEnumerable()}");
        if (!layingDownJobs.EnumerableNullOrEmpty())
            Log.Message($"[{Core.ModName}] - Laying-down animations for jobs that may not exist (from inactive mods?):\n{layingDownJobs.Where(job => !defNamesSet.Contains(job)).ToStringSafeEnumerable()}");
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        TempSet = new HashSet<string>();

        foreach (var ci in instr)
        {
            if (ci.opcode == OpCodes.Ldstr && ci.operand is string s && s.IndexOf(' ') == -1)
                TempSet.Add(s);
        }

        throw new Exception();
    }
}