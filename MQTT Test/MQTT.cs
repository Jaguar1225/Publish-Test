using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace MqttTestApp
{
    public class Status
    {
        public bool bstatus { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class Power
    {
        public Dictionary<string, int> SV { get; set; } = new Dictionary<string, int>
            {
                { "ICPRF_SV", 0 },
                { "BiasRF_SV", 0 },
            };
        public Dictionary<string, int> PV { get; set; } = new Dictionary<string, int>
            {
                { "ICPRF_Forward", 0 },
                { "BiasRFF_Forward", 0 },
            };
    }

    public class Pressure
    {
        public Dictionary<string, int> SV { get; set; } = new Dictionary<string, int>
            {
                { "Pressure_SV", 0 },
            };
        public Dictionary<string, int> PV { get; set; } = new Dictionary<string, int>
            {
                { "PressurePV", 0 },
            };
    }
    public class MFC
    {
        public Dictionary<string, int> SV { get; set; } = new Dictionary<string, int>
            {
                { "MFCN2_SV", 0 },
                { "MFCO2_SV", 0 },
                { "MFCAr_SV", 0 },
                { "MFCCF4_SV", 0 }
            };
        public Dictionary<string, int> PV { get; set; } = new Dictionary<string, int>
            {
                { "MFCN2_PV", 0 },
                { "MFCO2_PV", 0 },
                { "MFCAr_PV", 0 },
                { "MFCCF4_PV", 0 }
            };
    }
    public class MV
    {
        public Power Power { get; set; } = new Power();
        public Pressure Pressure { get; set; } = new Pressure();
        public MFC MFC { get; set; } = new MFC();
    }
    public class Log
    {
        public MV MV { get; set; } = new MV();
        public Dictionary<string, int> IV { get; set; } = new Dictionary<string, int>
        {
            { "ICPRF_Reflect", 0 },
            { "ICPRF_TunePos", 0 },
            { "ICPRF_LoadPos", 0 },
            { "BiasRF_Reflect", 0 },
            { "BiasRF_TunePos", 0 },
            { "BiasRF_LoadPos", 0 },
            { "PositionPV", 0},
            { "ConvPM_Pressure", 0 },
            { "ConvLine_Pressure", 0 },
            { "Chiller_Temp", 0 }
        };
        public DateTime Timestamp { get; set; }
    }

    public class MQTT
    {
        public string Host { get; set; } = "192.168.127.2";
        public short? Port { get; set; } = 9101;
        public Key _key { get; set; }
        private IMqttClient mqttClient;
        private MqttClientOptions options;
        private MqttClientConnectResult response;
        public bool disconnect { get; set; } = true;
        public string TopicStatus { private get; set; } = "Status";
        public string TopicLog { private get; set; } = "Logs";
        public string TopicControl { private get; set; } = "Control";

        public RandomGen _randomgen;
        public Status status { get; private set; }
        public Dictionary<string, int> Log { get; set; }

        private Task StatusCheckTask = Task.CompletedTask;

        public MQTT(Key key, RandomGen randomgen)
        {
            _key = key;
            _randomgen = randomgen;
        }

        public async Task Connect()
        {
            if (mqttClient == null)
            {
                mqttClient = new MqttFactory().CreateMqttClient();
                mqttClient.ApplicationMessageReceivedAsync += async eventArgs => 
                {
                    string message = Encoding.UTF8.GetString(eventArgs.ApplicationMessage.PayloadSegment.ToArray());
                    status = JsonConvert.DeserializeObject<Status>(message);
                };
                status = new Status
                {
                    bstatus = _key.GenCts,
                    Timestamp = DateTime.UtcNow
                };
                options = new MqttClientOptionsBuilder()
                    .WithTcpServer(Host, Port)
                    .Build();
            }

            while (!mqttClient.IsConnected)
            {
                try
                {
                    if (!mqttClient.IsConnected)
                    {
                        response = await mqttClient.ConnectAsync(options);
                    }
                    disconnect = false;
                }
                catch (Exception exc)
                {
                    Task.Delay(200);
                }
            }
            if ((response.ResultCode == MqttClientConnectResultCode.Success)
                && StatusCheckTask.IsCompleted)
            {
                StatusCheckTask = Task.Run(() => StatusCheck());
            }
        }

        public async Task StatusCheck()
        {
            while (_key.AppCts)
            {
                try
                {
                    status.bstatus = _key.GenCts;
                    status.Timestamp = DateTime.UtcNow;
                    string message = JsonConvert.SerializeObject(status);
                    var response = await Publish(mqttClient, TopicStatus, message);
                }

                catch (Exception Exc)
                {
                    continue;
                }
                await Task.Delay(100);
            }
        }

        public async Task LogCheck()
        {
            while (_key.GenCts)
            {
                try
                {
                    string message = JsonConvert.SerializeObject(_randomgen.log);
                    var response = await Publish(mqttClient, TopicLog, message);
                    Console.WriteLine(message);
                }
                catch (Exception Exc)
                {
                    continue;
                }
                await Task.Delay(500);
            }
        }

        public async Task Clean_Disconnect()
        {
            _key.GenCts = false;

            if (mqttClient != null)
            {
                await mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder()
                    .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());
                mqttClient.Dispose();
                mqttClient = null;
            }

            disconnect = true;
        }


        public async Task<MqttClientPublishResult> Publish(IMqttClient mqttClient, string topic, string message)
        {
            var encoded_message = Encoding.UTF8.GetBytes(message);
            var applicationMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                        .WithPayload(message)
                        .Build();
            return await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
        }

        public async Task<MqttClientSubscribeResult> Subscribe(IMqttClient mqttClient, string topic)
        {
            var mqttClientSubscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(topic)
                .Build();
            return await mqttClient.SubscribeAsync(mqttClientSubscribeOptions, CancellationToken.None);
        }
    }
}
