using System;
using System.Runtime.CompilerServices;

namespace YayoAnimation;

/// <summary>
/// A fast random number generator for .NET
/// Colin Green, January 2005
/// 
/// September 4th 2005
///	 Added NextBytesUnsafe() - commented out by default.
///	 Fixed bug in Reinitialise() - y,z and w variables were not being reset.
/// 
/// Key points:
///  1) Based on a simple and fast xor-shift pseudo random number generator (RNG) specified in: 
///  Marsaglia, George. (2003). Xorshift RNGs.
///  http://www.jstatsoft.org/v08/i14/xorshift.pdf
///  
///  This particular implementation of xorshift has a period of 2^128-1. See the above paper to see
///  how this can be easily extened if you need a longer period. At the time of writing I could find no 
///  information on the period of System.Random for comparison.
/// 
///  2) Faster than System.Random. Up to 8x faster, depending on which methods are called.
/// 
///  3) Direct replacement for System.Random. This class implements all of the methods that System.Random 
///  does plus some additional methods. The like named methods are functionally equivalent.
///  
///  4) Allows fast re-initialisation with a seed, unlike System.Random which accepts a seed at construction
///  time which then executes a relatively expensive initialisation routine. This provides a vast speed improvement
///  if you need to reset the pseudo-random number sequence many times, e.g. if you want to re-generate the same
///  sequence many times. An alternative might be to cache random numbers in an array, but that approach is limited
///  by memory capacity and the fact that you may also want a large number of different sequences cached. Each sequence
///  can each be represented by a single seed value (int) when using FastRandom.
///  
///  Notes.
///  A further performance improvement can be obtained by declaring local variables as static, thus avoiding 
///  re-allocation of variables on each call. However care should be taken if multiple instances of
///  FastRandom are in use or if being used in a multi-threaded environment.
/// 
/// </summary>
public class FastRandom
{
    // The +1 ensures NextDouble doesn't generate 1.0
    private const double RealUnitInt = 1.0 / ((double)int.MaxValue + 1.0);
    private const double RealUnitUint = 1.0 / ((double)uint.MaxValue + 1.0);
    private const uint Y = 842502087, Z = 3579807591, W = 273326509;

    private uint x, y, z, w;

    #region Reinitialisation

    /// <summary>
    /// Reinitialises using an int value as a seed.
    /// </summary>
    public void Reinitialise(int seed)
    {
        // The only stipulation stated for the xorshift RNG is that at least one of
        // the seeds x,y,z,w is non-zero. We fulfill that requirement by only allowing
        // resetting of the x seed.

        // The first random sample will be very closely related to the value of _x we set here. 
        // Thus setting _x = seed will result in a close correlation between the bit patterns of the seed and
        // the first random sample, therefore if the seed has a pattern (e.g. 1,2,3) then there will also be 
        // a recognisable pattern across the first random samples.
        //
        // Such a strong correlation between the seed and the first random sample is an undesirable
        // characteristic of a RNG, therefore we significantly weaken any correlation by hashing the seed's bits. 
        // This is achieved by multiplying the seed with four large primes each with bits distributed over the
        // full length of a 32bit value, finally adding the results to give _x.
        x = (uint)((seed * 1431655781)
                   + (seed * 1183186591)
                   + (seed * 622729787)
                   + (seed * 338294347));

        y = Y;
        z = Z;
        w = W;
    }

    #endregion

    #region RNG methods

    /// <summary>
    /// Generates a random int over the range 0 to upperBound-1, and not including upperBound.
    /// </summary>
    public int Next(int upperBound, int seed)
    {
        if (upperBound < 0)
            throw new ArgumentOutOfRangeException(nameof(upperBound), upperBound, $"{nameof(upperBound)} must be >=0");

        Reinitialise(seed);

        var t = x ^ (x << 11);
        x = y;
        y = z;
        z = w;

        // ENHANCEMENT: Can we do this without converting to a double and back again?
        // The explicit int cast before the first multiplication gives better performance.
        // See comments in NextDouble.
        return (int)((RealUnitInt * (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8))))) * upperBound);
    }

    /// <summary>
    /// Generates a random int over the range lowerBound to upperBound-1, and not including upperBound.
    /// upperBound must be >= lowerBound. lowerBound may be negative.
    /// </summary>
    public int Next(int lowerBound, int upperBound, int seed)
    {
        if (lowerBound > upperBound)
            throw new ArgumentOutOfRangeException(nameof(upperBound), upperBound, $"{nameof(upperBound)} must be >={nameof(lowerBound)}");

        Reinitialise(seed);

        var t = x ^ (x << 11);
        x = y;
        y = z;
        z = w;

        // The explicit int cast before the first multiplication gives better performance.
        // See comments in NextDouble.
        var range = upperBound - lowerBound;
        if (range < 0)
        {
            // If range is <0 then an overflow has occured and must resort to using long integer arithmetic instead (slower).
            // We also must use all 32 bits of precision, instead of the normal 31, which again is slower.  
            return lowerBound + (int)((RealUnitUint * (double)(w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))) * (double)((long)upperBound - (long)lowerBound));
        }

        // 31 bits of precision will suffice if range<=int.MaxValue. This allows us to cast to an int and gain
        // a little more performance.
        return lowerBound + (int)((RealUnitInt * (double)(int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8))))) * (double)range);
    }

    /// <summary>
    /// Generates a random double. Values returned are over the range [0, 1). That is, inclusive of 0.0 and exclusive of 1.0.
    /// </summary>
    public float Next(float upperBound, int seed)
    {
        if (upperBound < 0)
            throw new ArgumentOutOfRangeException(nameof(upperBound), upperBound, $"{nameof(upperBound)} must be >=0");

        Reinitialise(seed);

        var t = x ^ (x << 11);
        x = y;
        y = z;
        z = w;

        // Here we can gain a 2x speed improvement by generating a value that can be cast to 
        // an int instead of the more easily available uint. If we then explicitly cast to an 
        // int the compiler will then cast the int to a double to perform the multiplication, 
        // this final cast is a lot faster than casting from a uint to a double. The extra cast
        // to an int is very fast (the allocated bits remain the same) and so the overall effect 
        // of the extra cast is a significant performance improvement.
        //
        // Also note that the loss of one bit of precision is equivalent to what occurs within 
        // System.Random.
        return (float)(RealUnitInt * (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))) * upperBound);
    }

    /// <summary>
    /// Generates a random double. Values returned are over the range [0, 1). That is, inclusive of 0.0 and exclusive of 1.0.
    /// </summary>
    public float Next(float lowerBound, float upperBound, int seed)
    {
        if (lowerBound > upperBound)
            throw new ArgumentOutOfRangeException(nameof(upperBound), upperBound, $"{nameof(upperBound)} must be >={nameof(lowerBound)}");

        Reinitialise(seed);

        var t = x ^ (x << 11);
        x = y;
        y = z;
        z = w;

        // Here we can gain a 2x speed improvement by generating a value that can be cast to 
        // an int instead of the more easily available uint. If we then explicitly cast to an 
        // int the compiler will then cast the int to a double to perform the multiplication, 
        // this final cast is a lot faster than casting from a uint to a double. The extra cast
        // to an int is very fast (the allocated bits remain the same) and so the overall effect 
        // of the extra cast is a significant performance improvement.
        //
        // Also note that the loss of one bit of precision is equivalent to what occurs within 
        // System.Random.
        return (float)(RealUnitInt * (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))) * (upperBound - lowerBound) + lowerBound);
    }

    /// <summary>
    /// Generates a random double. Values returned are over the range [0, 1). That is, inclusive of 0.0 and exclusive of 1.0.
    /// </summary>
    public double Double(int seed)
    {
        Reinitialise(seed);

        var t = x ^ (x << 11);
        x = y;
        y = z;
        z = w;

        // Here we can gain a 2x speed improvement by generating a value that can be cast to 
        // an int instead of the more easily available uint. If we then explicitly cast to an 
        // int the compiler will then cast the int to a double to perform the multiplication, 
        // this final cast is a lot faster than casting from a uint to a double. The extra cast
        // to an int is very fast (the allocated bits remain the same) and so the overall effect 
        // of the extra cast is a significant performance improvement.
        //
        // Also note that the loss of one bit of precision is equivalent to what occurs within 
        // System.Random.
        return RealUnitInt * (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8))));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Bool(int seed) => Double(seed) < 0.5;

    #endregion

    #region Static helper methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NewNext(int upperBound, int seed)
        => new FastRandom().Next(upperBound, seed);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NewNext(int lowerBound, int upperBound, int seed)
        => new FastRandom().Next(lowerBound, upperBound, seed);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float NewNext(float upperBound, int seed)
        => new FastRandom().Next(upperBound, seed);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float NewNext(float lowerBound, float upperBound, int seed)
        => new FastRandom().Next(lowerBound, upperBound, seed);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NewBool(int seed)
        => new FastRandom().Double(seed) < 0.5;

    #endregion
}