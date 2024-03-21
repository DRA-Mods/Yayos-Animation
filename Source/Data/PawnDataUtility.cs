using System.Collections.Concurrent;
using System.Linq;
using Prepatcher;
using Verse;

namespace YayoAnimation.Data;

public static class PawnDataUtility
{
    private static readonly ConcurrentDictionary<Pawn, PawnDrawData> DrawDataDictionary = new();

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
        var empty = DrawDataDictionary.Where(a => a.Key == null || a.Key.mapIndexOrState < 0 || a.Key.mapIndexOrState >= Find.Maps.Count).ToList();
        foreach (var value in empty)
            DrawDataDictionary.TryRemove(value.Key, out _);
        Log.Message($"[{Core.ModName}] GC : animation data count [{prev} -> {DrawDataDictionary.Keys.Count}]");
    }

    // Should never be called with Prepatcher active, will be no-op anyway
    public static void Reset()
    {
        var prev = DrawDataDictionary.Keys.Count;
        DrawDataDictionary.Clear();
        Log.Message($"[{Core.ModName}] GC : animation data count [{prev} -> {DrawDataDictionary.Keys.Count}]");
    }
}