using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;



namespace MqttTestApp
{
    public class RandomGen
    {
        public Log log { get; set; }
        public Key _key { get; set; }

        public RandomGen(Key key)
        {
            _key = key;
        }

        public async Task GenerateAsync()
        {
            Random random = new Random();

            while (_key.AppCts)
            {
                if (log == null)
                {
                    log = new Log
                    {
                        LogEntries = new Dictionary<string, int>
                        {
                            { "ICPRF_SV", 0 },
                            { "ICPRF_Forward", 0 },
                            { "ICPRF_Reflect", 0 },
                            { "ICPRF_TunePos", 0 },
                            { "ICPRF_LoadPos", 0 },
                            { "BiasRF_SV", 0 },
                            { "BiasRFF_Forward", 0 },
                            { "BiasRF_Reflect", 0 },
                            { "BiasRF_TunePos", 0 },
                            { "BiasRF_LoadPos", 0 },
                            { "BiasVolt", 0 },
                            { "PressurePV", 0 },
                            { "PositionPV", 0 },
                            { "MFCN2_SV", 0 },
                            { "MFCN2_PV", 0 },
                            { "MFCO2_SV", 0 },
                            { "MFCO2_PV", 0 },
                            { "MFCAr_SV", 0 },
                            { "MFCAr_PV", 0 },
                            { "MFCCF4_SV", 0 },
                            { "MFCCF4_PV", 0 },
                            { "ConvPM_Pressure", 0 },
                            { "ConvLine_Pressure", 0 },
                            { "Pedestal_Temp", 0 },
                            { "Chiller_Temp", 0 }
                        },
                        Timestamp = DateTime.UtcNow
                    };
                }

                if (_key.GenCts)
                {
                    foreach (var logKey in log.LogEntries.Keys.ToList())
                    {
                        log.LogEntries[logKey] = random.Next(1, 301);
                    }
                    log.Timestamp = DateTime.UtcNow;

                }
                try
                {
                    await Task.Delay(100);
                }
                catch (TaskCanceledException)
                {
                    break; // 앱이 종료되면 루프 중단
                }
            }
        }


    }


}
