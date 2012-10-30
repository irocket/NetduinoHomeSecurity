﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using Device.Core;
using MQTT;

namespace HomeSecurity.Device.ExternalDoor
{
	public class Program
	{
		// BEGIN******* YOU MUST EDIT THE FOLLOWING
		// Change the following line to be your IP of your Netduino Device
		private static string _deviceGateway = "192.168.0.1";
		// Change the following line to set your Unique ID for the MQTT Broker (use your initials)
		private static string _mqttDeviceId = "mjl";
		// Change the IP of your device (this would be provided to you at the event)
		private static string _deviceIP = "192.168.0.2";
		// END******* 

		// Networking
		private static string _deviceSubnet = "255.255.255.0";

		// MQTT Message Broker endpoint
		private static string _mqttConnection = "tcp://168.62.49.240:1883";

		private static ILogger _logger;

		public static void Main()
		{
			// Setup the logger
			_logger = new ConsoleLogger();
			_logger.CurrentLogLevel = LogLevel.Debug;

			// Begin Initializing network
			Network.InitStaticNetwork(_deviceIP, _deviceSubnet, _deviceGateway);

			// Begin Creating MQTT client
			IMqtt client = MqttClientFactory.CreateClient(_mqttConnection, _mqttDeviceId, _logger);

			// Begin doing some sucurty related stuff
			ExternalDoorController controller = new ExternalDoorController(client, _logger,"house1","front");
			controller.Start();

			Thread.Sleep(Timeout.Infinite);

		}
	}
}