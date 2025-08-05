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
        public class SVPV
        {
            public int SV { get; set; } = 0;
            public int PV { get; set; } = 0;
        }
        public class Power
        {
            public SVPV ICPRF = new SVPV();
            public SVPV BiasRF = new SVPV();
        }

        public class MFC
        {
            public SVPV N2 { get; set; } = new SVPV();
            public SVPV O2 { get; set; } = new SVPV();
            public SVPV Ar { get; set; } = new SVPV();
            public SVPV CF4 { get; set; } = new SVPV();
        }

        public class MV
        {
            public Power Power { get; set; } = new Power();
            public SVPV Pressure { get; set; } = new SVPV();
            public MFC MFC { get; set; } = new MFC();
        }
        public class matching
        {
            public int Reflect { get; set; } = 0;
            public int TunePos { get; set; } = 0;
            public int LoadPos { get; set; } = 0;
        }
        public class Matching
        {
            public matching ICPRF { get; set; } = new matching();
            public matching BiasRF { get; set; } = new matching();
        }
        public class Valve
        {
            public int Position { get; set; } = 0;
            public int ConvPM { get; set; } = 0;
            public int ConvLine { get; set; } = 0;
        }
        public class IV
        {
            public Matching Matching { get; set; } = new Matching();
            public Valve Valve { get; set; } = new Valve();
            public int Temp { get; set; } = 0;
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
            public DateTime Timestampe { get; set; } = DateTime.UtcNow;
        }
    }
}
