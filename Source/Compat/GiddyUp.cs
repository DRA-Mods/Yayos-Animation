using System.Runtime.CompilerServices;
using Verse;
#if BIOTECH_PLUS
using GiddyUp;
using GiddyUp.Jobs;
#else
using GiddyUpCore;
using GiddyUpCore.Jobs;
using GiddyUpCore.Utilities;
#endif

namespace yayoAni.Compat;

public static class GiddyUp
{
#if BIOTECH_PLUS
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool HasMount(this Pawn pawn) => ExtendedDataStorage.isMounted.Contains(pawn.thingIDNumber);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Pawn MountingPawn(this Pawn animal)
    {
        if (animal.CurJob == null || animal.CurJob.def != ResourceBank.JobDefOf.Mounted || animal.jobs.curDriver is not JobDriver_Mounted driver)
            return null;

        var rider = driver.rider;
        if (!ExtendedDataStorage.isMounted.Contains(rider.thingIDNumber))
            return null;

        return rider;
    }
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool HasMount(this Pawn pawn) => pawn.CurMount() != null;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Pawn MountingPawn(this Pawn animal)
    {
        if (animal.CurJob == null || animal.CurJob.def != GUC_JobDefOf.Mounted || animal.jobs.curDriver is not JobDriver_Mounted driver)
            return null;

        var rider = driver.Rider;
        if (Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(rider).mount == null)
            return null;

        return rider;
    }
#endif
}