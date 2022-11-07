using GiddyUpCore;
using GiddyUpCore.Jobs;
using GiddyUpCore.Utilities;
using Verse;

namespace yayoAni.Compat;

public static class GiddyUp
{
    public static bool HasMount(this Pawn pawn) => pawn.CurMount() != null;

    public static Pawn MountingPawn(this Pawn animal)
    {
        if (animal.CurJob == null || animal.CurJob.def != GUC_JobDefOf.Mounted || animal.jobs.curDriver is not JobDriver_Mounted driver)
            return null;

        var rider = driver.Rider;
        if (Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(rider).mount == null)
            return null;

        return rider;
    }
}