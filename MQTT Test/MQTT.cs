using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;

namespace MqttTestApp
{
    public class Status
    {
        public bool bstatus { get;set; }
        public DateTime Timestamp { get; set; }
    }

    public class Log
    {
        public Dictionary<string, int> LogEntries { get; set; } = new Dictionary<string, int>
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
        };
        public DateTime Timestamp { get; set; }
    }
    public class MQTT
    {
        public string Host { get; set; } = "192.168.127.2";
        public short? PortStatus { get; set; } = 9001;
        public short? PortLog { get; set; } = 9002;
        public Key _key { get; set; }
        private IMqttClient mqttClientLog, mqttClientStatus;
        private MqttClientOptions optionsStatus,optionsLog;
        private MqttClientConnectResult responseLogs, responseStatus;
        public bool disconnect { get; set; } = true;
        public string TopicStatus { private get; set; } = "Status";
        public string TopicLog { private get; set; } = "Logs";

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
            if (mqttClientStatus == null)
            {
                mqttClientStatus = new MqttFactory().CreateMqttClient();
                mqttClientStatus.ApplicationMessageReceivedAsync += async eventArgs =>
                {
                    string message = Encoding.UTF8.GetString(eventArgs.ApplicationMessage.PayloadSegment.ToArray());
                    status = JsonConvert.DeserializeObject<Status>(message);
                };
                status = new Status
                {
                    bstatus = _key.GenCts,
                    Timestamp = DateTime.UtcNow
                };
                optionsStatus = new MqttClientOptionsBuilder()
                    .WithTcpServer(Host, PortStatus)
                    .Build();
            }
            if (mqttClientLog == null)
            {
                mqttClientLog = new MqttFactory().CreateMqttClient();
                optionsLog = new MqttClientOptionsBuilder()
                    .WithTcpServer(Host, PortLog)
                    .Build();
            }
            disconnect = !mqttClientStatus.IsConnected || !mqttClientLog.IsConnected;
            while (disconnect)
            {
                try
                {
                    if (!mqttClientStatus.IsConnected)
                    {
                        responseStatus = await mqttClientStatus.ConnectAsync(optionsStatus);
                    }
                    if (!mqttClientLog.IsConnected)
                    {
                        responseLogs = await mqttClientLog.ConnectAsync(optionsLog);
                    }
                    disconnect = false;
                }
                catch (Exception exc)
                {
                    Task.Delay(200);
                }
            }
            if ((responseStatus.ResultCode == MqttClientConnectResultCode.Success)
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
                    var response = await Publish(mqttClientStatus, TopicStatus, message);
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
                    var response = await Publish(mqttClientLog, TopicLog, message);
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

            if (mqttClientLog != null)
            {
                await mqttClientLog.DisconnectAsync(new MqttClientDisconnectOptionsBuilder()
                    .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());
                mqttClientLog.Dispose();
                mqttClientLog = null;

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
