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
        public DataStructure.Log log { get; set; } = new DataStructure.Log();
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
                    log = new DataStructure.Log();
                    log.Timestamp = MonoClock.NowNs();
                }

                if (_key.GenCts)
                {
                    log.MV.Power.ICPRF.SV = random.Next(1,301);
                    log.MV.Power.ICPRF.PV = random.Next(1, 301);
                    log.MV.Power.BiasRF.SV = random.Next(1, 301);
                    log.MV.Power.BiasRF.PV = random.Next(1, 301);
                    log.MV.Pressure.SV = random.Next(1, 301);
                    log.MV.Pressure.PV = random.Next(1, 301);
                    log.MV.MFC.N2.SV = random.Next(1, 301);
                    log.MV.MFC.N2.PV = random.Next(1, 301);
                    log.MV.MFC.O2.SV = random.Next(1, 301);
                    log.MV.MFC.O2.PV = random.Next(1, 301);
                    log.MV.MFC.Ar.SV = random.Next(1, 301);
                    log.MV.MFC.Ar.PV = random.Next(1, 301);
                    log.MV.MFC.CF4.SV = random.Next(1, 301);
                    log.MV.MFC.CF4.PV = random.Next(1, 301);
                    log.IV.Matching.ICPRF.Reflect = random.Next(1, 301);
                    log.IV.Matching.ICPRF.TunePos = random.Next(1, 301);
                    log.IV.Matching.ICPRF.LoadPos = random.Next(1, 301);
                    log.IV.Matching.BiasRF.Reflect = random.Next(1, 301);
                    log.IV.Matching.BiasRF.TunePos = random.Next(1, 301);
                    log.IV.Matching.BiasRF.LoadPos = random.Next(1, 301);
                    log.IV.Valve.Position = random.Next(1, 301);
                    log.IV.Valve.ConvPM = random.Next(1, 301);
                    log.IV.Valve.ConvLine = random.Next(1, 301);
                    log.IV.Temp = random.Next(1, 301);
                    log.Timestamp = 0;
                    log.MonoTimeNs = MonoClock.NowNs();
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
