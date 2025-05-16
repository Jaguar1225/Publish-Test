using System;
using System.Threading;
using System.Threading.Tasks;



namespace MqttTestApp
{
    class App
    {

        static async Task Main(string[] args)
        { 

            Key key = new Key();

            RandomGen randomlog = new RandomGen(key);
            Task KeyCheck = Task.CompletedTask;
            MQTT _mqttUnit = new MQTT(key, randomlog);

            Console.WriteLine("KeyCheck 실행");
            KeyCheck = Task.Run(() => key.KeyInput(_mqttUnit));
            Console.WriteLine("randomlog 객체 생성");
            await randomlog.GenerateAsync();

        }
    }


}