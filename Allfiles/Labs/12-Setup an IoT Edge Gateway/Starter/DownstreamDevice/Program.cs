// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace CaveDevice
{
    class Program
    {
        // Contains methods that a device can use to send messages to and receive from an IoT Hub.
        private static DeviceClient deviceClient;

        // The device connection string to authenticate the device with your IoT hub.
        // Note: in real-world applications you would not "hard-code" the connection string
        // It could be stored within an environment variable, passed in via the command-line or
        // store securely within a TPM module.
        private readonly static string connectionString = "{Your device connection string here}";

        private static void Main(string[] args)
        {
            Console.WriteLine("IoT Hub C# Simulated Cave Device. Ctrl-C to exit.\n");

            InstallCACert();

            // Connect to the IoT hub using the MQTT protocol
            deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt);
            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        /// <summary>
        /// Add certificate in local cert store for use by downstream device
        /// client for secure connection to IoT Edge runtime.
        ///
        ///    Note: On Windows machines, if you have not run this from an Administrator prompt,
        ///    a prompt will likely come up to confirm the installation of the certificate.
        ///    This usually happens the first time a certificate will be installed.
        /// </summary>
        static void InstallCACert()
        {
            string trustedCACertPath = "azure-iot-test-only.root.ca.cert.pem";
            if (!string.IsNullOrWhiteSpace(trustedCACertPath))
            {
                Console.WriteLine("User configured CA certificate path: {0}", trustedCACertPath);
                if (!File.Exists(trustedCACertPath))
                {
                    // cannot proceed further without a proper cert file
                    Console.WriteLine("Certificate file not found: {0}", trustedCACertPath);
                    throw new InvalidOperationException("Invalid certificate file.");
                }
                else
                {
                    Console.WriteLine("Attempting to install CA certificate: {0}", trustedCACertPath);
                    X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(new X509Certificate2(X509Certificate.CreateFromCertFile(trustedCACertPath)));
                    Console.WriteLine("Successfully added certificate: {0}", trustedCACertPath);
                    store.Close();
                }
            }
            else
            {
                Console.WriteLine("trustedCACertPath was not set or null, not installing any CA certificate");
            }
        }


        // Async method to send simulated telemetry
        private static async void SendDeviceToCloudMessagesAsync()
        {
            // Create an instance of our sensor 
            var sensor = new EnvironmentSensor();

            while (true)
            {
                // read data from the sensor
                var currentTemperature = sensor.ReadTemperature();
                var currentHumidity = sensor.ReadHumidity();

                var messageString = CreateMessageString(currentTemperature, currentHumidity);
                using (var message = new Message(Encoding.ASCII.GetBytes(messageString)))
                {
                    // Add a custom application property to the message.
                    // An IoT hub can filter on these properties without access to the message body.
                    message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                    // Send the telemetry message
                    await deviceClient.SendEventAsync(message);
                    Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
                }
                await Task.Delay(1000);
            }
        }

        private static string CreateMessageString(double temperature, double humidity)
        {
            // Create an anonymous object that matches the data structure we wish to send
            var telemetryDataPoint = new
            {
                temperature = temperature,
                humidity = humidity
            };

            // Create a JSON string from the anonymous object
            return JsonConvert.SerializeObject(telemetryDataPoint);
        }
    }

    /// <summary>
    /// This class represents a sensor 
    /// real-world sensors would contain code to initialize
    /// the device or devices and maintain internal state
    /// a real-world example can be found here: https://bit.ly/IoT-BME280
    /// </summary>
    internal class EnvironmentSensor
    {
        // Initial telemetry values
        double minTemperature = 20;
        double minHumidity = 60;
        Random rand = new Random();

        internal EnvironmentSensor()
        {
            // device initialization could occur here
        }

        internal double ReadTemperature()
        {
            return minTemperature + rand.NextDouble() * 15;
        }

        internal double ReadHumidity()
        {
            return minHumidity + rand.NextDouble() * 20;
        }
    }
}
