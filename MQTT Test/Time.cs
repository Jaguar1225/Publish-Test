using System;
using System.Diagnostics;

public static class MonoClock
{
    public static long NowNs()
    {
        long ts = Stopwatch.GetTimestamp();
        double f = Stopwatch.Frequency;             // ticks per second
        return (long)(ts * (1_000_000_000.0 / f)); // Convert to nanoseconds
    }
    public static double NsToMs(long ns) => ns / 1e6;
    public static double NsToSec(long ns) => ns / 1e9;
}