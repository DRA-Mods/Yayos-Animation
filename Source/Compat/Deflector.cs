using System;
using System.Runtime.CompilerServices;
using Verse;

namespace yayoAni.Compat;

public static class Deflector
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool IsDeflectorAndAnimatingNow(this ThingComp comp)
        => comp is CompDeflector.CompDeflector { IsAnimatingNow: true };

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void CheckDeflectorActive()
    {
        try
        {
            // Basically a check to see if deflector is active.
            bool Temp()
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                return typeof(CompDeflector.CompDeflector) != null &&
                       // ReSharper disable once ConstantConditionalAccessQualifier
                       new CompDeflector.CompDeflector
                       {
                           AnimationDeflectionTicks = 100,
                       }?.IsAnimatingNow != null;
            }

            Core.usingDeflector = Temp();
            if (Core.usingSheathYourSword)
                Log.Message("[Yayo's Animation] - CompDeflector detected");
        }
        catch (Exception e)
        {
            Core.usingDeflector = false;
            Log.Message(e is not TypeLoadException or TypeInitializationException or MissingFieldException
                ? $"[Yayo's Animation] - No CompDeflector detected. Unexpected exception caught: {e.GetType()}"
                : "[Yayo's Animation] - No CompDeflector detected.");
        }
    }
}