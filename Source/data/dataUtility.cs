using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld.Planet;

namespace yayoAni
{


    public static class dataUtility
    {
        public static readonly Dictionary<Pawn, pawnDrawData> dic_pdd = new();

        public static pawnDrawData GetData(Pawn key)
        {
            if (!dic_pdd.ContainsKey(key)) dic_pdd[key] = new pawnDrawData();
            return dic_pdd[key];
        }

        public static void Remove(Pawn key)
        {
            dic_pdd.Remove(key);
        }

        public static void GC()
        {
            int prev = dic_pdd.Keys.Count;
            dic_pdd.RemoveAll(a => a.Key == null || a.Key.Map == null);
            Log.Message($"GC : animation data count [{prev} -> {dic_pdd.Keys.Count}]");
        }

        public static void reset()
        {
            int prev = dic_pdd.Keys.Count;
            dic_pdd.Clear();
            Log.Message($"GC : animation data count [{prev} -> {dic_pdd.Keys.Count}]");
        }


    }


}
