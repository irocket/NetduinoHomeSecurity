using System;
using Microsoft.SPOT;
using MQTT;
using Device.Core;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System.Threading;

namespace HomeSecurity.Device.DoorBell
{
	public class DoorBellController
	{
        private readonly IMqtt _mqttService;
        private readonly ILogger _logger;
        private string _locationCode;
        private string _houseCode;
        private static Timer _pingResponseTimer = null;
        private OutputPort _pingResponseOutput = new OutputPort(Pins.ONBOARD_LED, false);
        private OutputPort _doorbellFrontOutput = new OutputPort(Pins.GPIO_PIN_D0, false);
        private OutputPort _doorbellBackOutput = new OutputPort(Pins.GPIO_PIN_D1, false);
        private OutputPort _doorbellSideOutput = new OutputPort(Pins.GPIO_PIN_D2, false);
        private const string _frontDoorbellTopic = "/externaldoor/front/doorbell";
        private const string _backDoorbellTopic = "/externaldoor/back/doorbell";
        private const string _sideDoorbellTopic = "/externaldoor/side/doorbell";

        #region ctor

        public DoorBellController(IMqtt mqttService, ILogger logger, string houseCode, string locationCode)
        {
            _logger = logger;
            _mqttService = mqttService;
			_houseCode = houseCode;
            _locationCode = locationCode;

            // Setup the timer to wait forever
            _pingResponseTimer = new Timer(new TimerCallback(OnPingResponseTimer), this._pingResponseOutput, Timeout.Infinite, Timeout.Infinite);

        }

		#endregion

		#region Public Properties

		public string Topic
		{
			get
			{
				return "/" + _houseCode + "/doorbell/" + _locationCode + "/";
			}
		}

		#endregion

		#region Public Methods
		public void Start()
        {
			if (ConnectToBroker())
            {
				if (Subscribe())
                {
                    // Send out a ping topic with Hello World as the message and it should come back to this device as a pingresp
                    _mqttService.Publish(new MqttParcel(Topic + "ping", "Hello world", QoS.BestEfforts, false));
                }
				else
					_logger.Error("Unable to subscribe to the Broker");
			}
			else
				_logger.Error("Unable to connect to the Broker");
        }
		#endregion

		#region Private methods
		private bool ConnectToBroker()
        {
            bool success = false;
            try
            {
                _mqttService.PublishArrived -= new PublishArrivedDelegate(PublishArrived);
                _mqttService.ConnectionLost -= new ConnectionDelegate(ConnectionLost);
                _mqttService.Connect();
                success = true;
                _mqttService.PublishArrived += new PublishArrivedDelegate(PublishArrived);
                _mqttService.ConnectionLost += new ConnectionDelegate(ConnectionLost);

            }
            catch (Exception ex)
            {
                _logger.Debug("Unable to connect " + ex.Message);
            }

            return success;
        }

        private bool Subscribe()
        {
            bool success = false;
            int messageId = 0;

            try
            {
                Subscription subscription = null;
                subscription = new Subscription(Topic + "pingresp", QoS.BestEfforts);
                messageId = _mqttService.Subscribe(subscription);

                // Subscribe to any doorbell messages
                string topic = "/" + _houseCode + "/externaldoor/+/doorbell";
                subscription = new Subscription(topic, QoS.BestEfforts);
                messageId = _mqttService.Subscribe(subscription);

				success = true;
            }
            catch (Exception ex)
            {
                _logger.Error("Exception during subscription", ex);
            }
            return success;
        }

        private void ConnectionLost(object sender, EventArgs e)
        {
            _logger.Info("Connection Lost");
        }

        private bool PublishArrived(object sender, PublishArrivedArgs e)
        {
			_logger.Info("Msg Recvd: " + e.Topic + " " + e.Payload.ToString());

            if (e.Topic.Equals(Topic + "pingresp"))
            {
                _pingResponseOutput.Write(true);
                _pingResponseTimer.Change(3000, 3000);
                return true;
            }

            return true;
		}

        public void CheckForDoorbellMessages(PublishArrivedArgs e)
        {
            if (e.Topic.Equals("/" + _houseCode + _frontDoorbellTopic))
            {
                // TODO set the doorbell indicator on for 3 seconds
            }

            if (e.Topic.Equals("/" + _houseCode + _backDoorbellTopic))
            {
                // TODO set the doorbell indicator on for 3 seconds
            }

            if (e.Topic.Equals("/" + _houseCode + _sideDoorbellTopic))
            {
                // TODO set the doorbell indicator on for 3 seconds
            }
        }

        private static void OnPingResponseTimer(object state)
        {
            _pingResponseTimer.Change(Timeout.Infinite, Timeout.Infinite);
            OutputPort output = (OutputPort)state;
            bool isOn = output.Read();
            output.Write(!isOn);
        }

        #endregion
	}
}
