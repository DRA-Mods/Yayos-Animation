using Verse;

namespace yayoAni.Compat;

public static class Deflector
{
    public static bool IsDeflectorAndAnimatingNow(this ThingComp comp)
        => comp is CompDeflector.CompDeflector { IsAnimatingNow: true };
}