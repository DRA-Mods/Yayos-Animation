using System.Collections.Generic;
#if BIOTECH_PLUS
using System.Runtime.CompilerServices;
using Prepatcher;
#endif
using Verse;

namespace yayoAni.Data;

public static class DataUtility
{
#if BIOTECH_PLUS
    public static readonly Dictionary<Pawn, StrongBox<PawnDrawData>> DrawDataDictionary = new();

    public static PawnDrawData MakeBlankDrawData() => new();

    [PrepatcherField]
    [ValueInitializer(nameof(MakeBlankDrawData))]
    public static ref PawnDrawData GetData(this Pawn key)
    {
        if (DrawDataDictionary.TryGetValue(key, out var data))
            return ref data.Value;

        data = new StrongBox<PawnDrawData>(MakeBlankDrawData());
        DrawDataDictionary[key] = data;
        return ref data.Value;
    }
#else
    public static readonly Dictionary<Pawn, PawnDrawData> DrawDataDictionary = new();

    public static PawnDrawData GetData(Pawn key)
    {
        if (DrawDataDictionary.TryGetValue(key, out var data))
            return data;

        data = new PawnDrawData();
        DrawDataDictionary[key] = data;
        return data;
    }
#endif

    public static void Remove(Pawn key)
    {
        DrawDataDictionary.Remove(key);
    }

    public static void GC()
    {
        int prev = DrawDataDictionary.Keys.Count;
        DrawDataDictionary.RemoveAll(a => a.Key == null || a.Key.mapIndexOrState < 0 || a.Key.mapIndexOrState >= Find.Maps.Count);
        Log.Message($"[Yayo's Animation] GC : animation data count [{prev} -> {DrawDataDictionary.Keys.Count}]");
    }

    public static void Reset()
    {
        int prev = DrawDataDictionary.Keys.Count;
        DrawDataDictionary.Clear();
        Log.Message($"[Yayo's Animation] GC : animation data count [{prev} -> {DrawDataDictionary.Keys.Count}]");
    }
}