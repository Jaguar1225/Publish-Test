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
        public Log log { get; set; } = new Log();
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
                    log.MV.Power.SV["ICPRF_SV"] = 0;
                    log.MV.Power.SV["BiasRF_SV"] = 0;
                    log.MV.Power.PV["ICPRF_Forward"] = 0;
                    log.MV.Power.PV["BiasRF_Forward"] = 0;
                    log.MV.Pressure.SV["Pressure_SV"] = 0;
                    log.MV.Pressure.PV["Pressure_PV"] = 0;
                    log.MV.MFC.SV["MFCN2_SV"] = 0;
                    log.MV.MFC.SV["MFCO2_SV"] = 0;
                    log.MV.MFC.SV["MFCAr_SV"] = 0;
                    log.MV.MFC.SV["MFCCF4_SV"] = 0;
                    log.MV.MFC.PV["MFCN2_PV"] = 0;
                    log.MV.MFC.PV["MFCO2_PV"] = 0;
                    log.MV.MFC.PV["MFCAr_PV"] = 0;
                    log.MV.MFC.PV["MFCCF4_PV"] = 0;
                    log.IV["ICPRF_Reflect"] = 0;
                    log.IV["ICPRF_TunePos"] = 0;
                    log.IV["ICPRF_LoadPos"] = 0;
                    log.IV["BiasRF_Reflect"] = 0;
                    log.IV["BiasRF_TunePos"] = 0;
                    log.IV["BiasRF_LoadPos"] = 0;
                    log.IV["PositionPV"] = 0;
                    log.IV["ConvPM_Pressure"] = 0;
                    log.IV["ConvLine_Pressure"] = 0;
                    log.IV["Chiller_Temp"] = 0;
                    log.Timestamp = DateTime.UtcNow;
                }

                if (_key.GenCts)
                {
                    log.MV.Power.SV["ICPRF_SV"] = random.Next(1,301);
                    log.MV.Power.SV["BiasRF_SV"] = random.Next(1, 301);
                    log.MV.Power.PV["ICPRF_Forward"] = random.Next(1, 301);
                    log.MV.Power.PV["BiasRF_Forward"] = random.Next(1, 301);
                    log.MV.Pressure.SV["Pressure_SV"] = random.Next(1, 301);
                    log.MV.Pressure.PV["Pressure_PV"] = random.Next(1, 301);
                    log.MV.MFC.SV["MFCN2_SV"] = random.Next(1, 301);
                    log.MV.MFC.SV["MFCO2_SV"] = random.Next(1, 301);
                    log.MV.MFC.SV["MFCAr_SV"] = random.Next(1, 301);
                    log.MV.MFC.SV["MFCCF4_SV"] = random.Next(1, 301);
                    log.MV.MFC.PV["MFCN2_PV"] = random.Next(1, 301);
                    log.MV.MFC.PV["MFCO2_PV"] = random.Next(1, 301);
                    log.MV.MFC.PV["MFCAr_PV"] = random.Next(1, 301);
                    log.MV.MFC.PV["MFCCF4_PV"] = random.Next(1, 301);
                    log.IV["ICPRF_Reflect"] = random.Next(1, 301);
                    log.IV["ICPRF_TunePos"] = random.Next(1, 301);
                    log.IV["ICPRF_LoadPos"] = random.Next(1, 301);
                    log.IV["BiasRF_Reflect"] = random.Next(1, 301);
                    log.IV["BiasRF_TunePos"] = random.Next(1, 301);
                    log.IV["BiasRF_LoadPos"] = random.Next(1, 301);
                    log.IV["PositionPV"] = random.Next(1, 301);
                    log.IV["ConvPM_Pressure"] = random.Next(1, 301);
                    log.IV["ConvLine_Pressure"] = random.Next(1, 301);
                    log.IV["Chiller_Temp"] = random.Next(1, 301);
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
