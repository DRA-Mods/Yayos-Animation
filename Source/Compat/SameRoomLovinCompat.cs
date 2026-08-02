using Verse;

namespace YayoAnimation.Compat;

public static class SameRoomLovinCompat
{
    private const string GroupSecondaryJobName = "SRL_Lovin_Group_Secondary";

    public static Pawn GetPartner(Pawn pawn, string jobDefName)
    {
        var directPartner = pawn.CurJob?.targetA.Pawn;
        if (directPartner != null && directPartner != pawn)
            return directPartner;

        if (jobDefName != "SRL_Lovin_Group_Primary" || pawn.Map == null)
            return null;

        Pawn closestPartner = null;
        var closestDistance = int.MaxValue;
        foreach (var candidate in pawn.Map.mapPawns.AllPawnsSpawned)
        {
            var candidateJob = candidate.CurJob;
            if (candidate == pawn ||
                candidateJob?.def?.defName != GroupSecondaryJobName ||
                candidateJob.targetA.Pawn != pawn)
                continue;

            var distance = candidate.Position.DistanceToSquared(pawn.Position);
            if (distance < closestDistance ||
                distance == closestDistance && (closestPartner == null || candidate.thingIDNumber < closestPartner.thingIDNumber))
            {
                closestPartner = candidate;
                closestDistance = distance;
            }
        }

        return closestPartner;
    }
}
