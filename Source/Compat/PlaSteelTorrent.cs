using System;
using Verse;
using PLA.Vehicle;

namespace yayoAni.Compat;

public static class PlaSteelTorrent
{
    public static Func<Pawn, bool> IsVehicle { get; private set; } = DefaultHandler;

    public static void Init() => IsVehicle = SteelTorrentHandler;

    private static bool DefaultHandler(Pawn _) => false;

    private static bool SteelTorrentHandler(Pawn pawn) => 
        pawn.GetType() == typeof(DroneVehicle);
}