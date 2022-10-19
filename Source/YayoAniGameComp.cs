using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace yayoAni
{
    public class YayoAniGameComp : GameComponent
    {
        public override void StartedNewGame() => ResetData();
        public override void FinalizeInit() => ResetData();

        public YayoAniGameComp([UsedImplicitly] Game _)
        { }

        public static void ResetData() => dataUtility.reset();

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
                dataUtility.GC();
        }
    }
}