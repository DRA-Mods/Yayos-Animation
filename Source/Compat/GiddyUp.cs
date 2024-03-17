using System.Runtime.CompilerServices;
using GiddyUp;
using GiddyUp.Jobs;
using Verse;

namespace YayoAnimation.Compat;

public static class GiddyUp
{
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
}