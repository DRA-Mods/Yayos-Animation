using System.Runtime.CompilerServices;
using Verse;

namespace yayoAni.Compat;

public static class Deflector
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool IsDeflectorAndAnimatingNow(this ThingComp comp)
        => comp is CompDeflector.CompDeflector { IsAnimatingNow: true };
}