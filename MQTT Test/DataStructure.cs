using System;

namespace MqttTestApp
{
    public class DataStructure
    {
        public class Status
        {
            public bool bstatus { get; set; }
            public DateTime Timestamp { get; set; }
        }
        public class SVPV<T> where T : struct, IConvertible
        {
            public T SV { get; set; }
            public T PV { get; set; }
            public SVPV()
            {
                SV = default(T);
                PV = default(T);
            }
        }
        public class Power
        {
            public SVPV<int> ICPRF = new SVPV<int>();
            public SVPV<int> BiasRF = new SVPV<int>();
        }

        public class MFC
        {
            public SVPV<int> N2 { get; set; } = new SVPV<int>();
            public SVPV<int> O2 { get; set; } = new SVPV<int>();
            public SVPV<int> Ar { get; set; } = new SVPV<int>();
            public SVPV<int> CF4 { get; set; } = new SVPV<int>();
        }

        public class MV
        {
            public Power Power { get; set; } = new Power();
            public SVPV<double> Pressure { get; set; } = new SVPV<double>();
            public MFC MFC { get; set; } = new MFC();
        }
        public class matching
        {
            public double Reflect { get; set; } = 0;
            public double TunePos { get; set; } = 0;
            public double LoadPos { get; set; } = 0;
        }
        public class Matching
        {
            public matching ICPRF { get; set; } = new matching();
            public matching BiasRF { get; set; } = new matching();
        }
        public class Valve
        {
            public double Position { get; set; } = 0;
            public double ConvPM { get; set; } = 0;
            public double ConvLine { get; set; } = 0;
        }
        public class IV
        {
            public Matching Matching { get; set; } = new Matching();
            public Valve Valve { get; set; } = new Valve();
            public double Temp { get; set; } = 0;
        }
        public class Log
        {
            public bool IsDataReceived { get; set; } = false;
            public MV MV { get; set; } = new MV();
            public IV IV { get; set; } = new IV();
            public DateTime Timestamp { get; set; }
        }
        public class Control
        {
            public MV MV { get; set; } = new MV();
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        }
    }
}
