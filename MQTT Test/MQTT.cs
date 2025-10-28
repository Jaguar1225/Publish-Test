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
    public class MQTT
    {
        public string Host { get; set; } = "127.0.0.1";
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
        public DataStructure.Status status { get; private set; }
        public DataStructure.Control? control { get; set; }
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
                mqttClient.ApplicationMessageReceivedAsync += HandleMessage;
                status = new DataStructure.Status
                {
                    bstatus = _key.GenCts,
                    Timestamp = MonoClock.NowNs()
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
                        await mqttClient.SubscribeAsync(
                            new MqttTopicFilterBuilder()
                            .WithTopic(TopicControl)
                            .Build());
                        await mqttClient.SubscribeAsync(
                            new MqttTopicFilterBuilder()
                            .WithTopic("Time/req")
                            .Build());
                    }
                    disconnect = false;
                }
                catch (Exception exc)
                {
                   Console.WriteLine($"Connection failed: {exc.Message}");
                   await Task.Delay(200);
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
                    status.Timestamp = MonoClock.NowNs();
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

        public async Task HandleMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {

                var topic = e.ApplicationMessage.Topic;
                var msg = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment.ToArray());
                switch (topic)
                {
                    case var t when t == "Time/req":
                        try
                        {
                            long t3 = MonoClock.NowNs();
                            var settings = new JsonSerializerSettings
                            {
                                MissingMemberHandling = MissingMemberHandling.Ignore,
                                NullValueHandling = NullValueHandling.Ignore,
                                Error = (sender, args) =>
                                {
                                    Console.WriteLine($"Error deserializing message: {args.ErrorContext.Error.Message}");
                                    args.ErrorContext.Handled = true;
                                }
                            };

                            var newTime = JsonConvert.DeserializeObject<DataStructure.Timestamp>(msg, settings);
                            if (newTime != null)
                            {
                                long t0 = newTime.t0, t1 = newTime.t1, t2 = newTime.t2;
                                double offset = ((t1 - t0) + (t2 - t3)) / 2.0;
                                string payload = JsonConvert.SerializeObject(offset);
                                await Publish(mqttClient, "Time/res", payload);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing time message: {ex.Message}");
                        }
                        break;
                    case var t when t == TopicControl:
                        try
                        {
                            var settings = new JsonSerializerSettings
                            {
                                MissingMemberHandling = MissingMemberHandling.Ignore,
                                NullValueHandling = NullValueHandling.Ignore,
                                Error = (sender, args) =>
                                {
                                    Console.WriteLine($"Error deserializing message: {args.ErrorContext.Error.Message}");
                                    args.ErrorContext.Handled = true;
                                }
                            };
                            var control = JsonConvert.DeserializeObject<DataStructure.Control>(msg);
                            Console.WriteLine($"Received message: {control} on topic: {e.ApplicationMessage.Topic}");

                        }
                        catch
                        {
                            break;
                        }
                        break;
                }

            }
            catch (Exception Exc)
            {
                return;
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
            }
            mqttClient = null;
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
