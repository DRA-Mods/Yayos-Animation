using System.Collections.Generic;
using Prepatcher;
using Verse;

namespace YayoAnimation.Data;

public static class PawnDataUtility
{
    private static readonly Dictionary<Pawn, PawnDrawData> DrawDataDictionary = new();

    private static PawnDrawData MakeBlankDrawData() => new();

    [PrepatcherField]
    [ValueInitializer(nameof(MakeBlankDrawData))]
    public static PawnDrawData GetData(this Pawn key)
    {
        if (DrawDataDictionary.TryGetValue(key, out var data))
            return data;

        data = MakeBlankDrawData();
        DrawDataDictionary[key] = data;
        return data;
    }

    // Should never be called with Prepatcher active, will be no-op anyway
    public static void GC()
    {
        var prev = DrawDataDictionary.Keys.Count;
        DrawDataDictionary.RemoveAll(a => a.Key == null || a.Key.mapIndexOrState < 0 || a.Key.mapIndexOrState >= Find.Maps.Count);
        Log.Message($"[Yayo's Animation] GC : animation data count [{prev} -> {DrawDataDictionary.Keys.Count}]");
    }

    // Should never be called with Prepatcher active, will be no-op anyway
    public static void Reset()
    {
        var prev = DrawDataDictionary.Keys.Count;
        DrawDataDictionary.Clear();
        Log.Message($"[Yayo's Animation] GC : animation data count [{prev} -> {DrawDataDictionary.Keys.Count}]");
    }
}