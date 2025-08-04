using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttTestApp
{
    public class Key
    {
        public bool AppCts { get; set; } = true;
        public bool GenCts { get; set; } = false;

        private Task LogCheckTask = Task.CompletedTask;
        public async Task KeyInput(MQTT mqttUnit)
        {
            while (AppCts)
            {
                var k = Console.ReadKey(true);
                if (k.Key == ConsoleKey.P)
                {
                    if (GenCts)
                    {
                        GenCts = false;
                        await LogCheckTask;
                        Console.WriteLine("⏸ 로그 체크 중지");
                    }

                    else
                    {
                        await mqttUnit.Connect();
                        GenCts = true;
                        LogCheckTask = Task.Run(() => mqttUnit.LogCheck());
                        Console.WriteLine("▶ 로그 체크 시작");
                    }
                }

                if (k.Key == ConsoleKey.X)
                {
                    Console.WriteLine("❌ 프로그램 종료");
                    AppCts = !AppCts;
                }

            }
        }
    }
}
