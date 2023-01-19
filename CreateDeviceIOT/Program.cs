using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;
using System.Text;
using Newtonsoft.Json;


namespace CreateDeviceIOT
{
    class Program
    {
        

        //telemetarty message 
        private static DeviceClient s_deviceClient;
        private readonly static string s_connectionString01 = "HostName=iotdevicehubltimind.azure-devices.net;DeviceId=device0;SharedAccessKey=V+jGow37cnkVtgwfMVcGvKm+NhVtiAxwVxfBxGj8X3s=";



        //create , read, update and delete device code commented below
        
        static RegistryManager registryManager;
        static string connectionstring = "HostName=iotdevicehubltimind.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ZwZK8Xs5Vu7CojeJIgiud1yLZi5zN4+zAYgxy1KcSIU=";

        static void Main(string[] args)
        {
            //object loop1 = null;
            //loop1;
            registryManager = RegistryManager.CreateFromConnectionString(connectionstring);

            Console.WriteLine("1- Device create, 2-Update device , 3- Send Telemetry message , 4- Detele device");

            // Menu 
            //Console.WriteLine("Press 1 to create the device");

            // Create a string variable and get user input from the keyboard and store it in the variable
            string Menu = Console.ReadLine();

            if (Menu == "1")
            {

                // commented below as tested for adding device successfully
                string deviceId = "device";

                for (int i = 0; i < 1; i++)
                {
                    string newDeviceId = deviceId + i;

                    AddDeviceAsync(newDeviceId).Wait();

                }

                Console.WriteLine("Device created");

            }
            else if (Menu == "2")
            {
                var iotdevices = registryManager.GetDeviceAsync("device0").Result;

                if (!string.IsNullOrEmpty(iotdevices.Id.ToString()))
                {
                    //Update the device in IOT HUB ... update status to disabled

                    try
                    {
                        UpdateDeviceAsync().Wait();
                        Console.WriteLine("Device Updated");
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Device Updated Fail");
                    }                   
                   
                }
            }
            else if (Menu == "3")
            {
                var iotdevices = registryManager.GetDeviceAsync("device0").Result;

                if (!string.IsNullOrEmpty(iotdevices.Id.ToString()))
                {
                    //

                    try
                    {
                        // telemetary message
                        s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString01, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
                        SendDeviceToCloudMessagesAsync(s_deviceClient);
                        // end
                        //Console.WriteLine("Device Updated");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Message Fail");
                    }

                }
            }
            else if (Menu == "4")
            {
                var iotdevices = registryManager.GetDeviceAsync("device0").Result;

                if (!string.IsNullOrEmpty(iotdevices.Id.ToString()))
                {
                    //Delete device

                    try
                    {
                        // commented below , it is for removing the device from IOT HUB
                        RemoveDeviceAsync().Wait();
                        Console.WriteLine("Device Removed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Device Removed Fail");
                    }

                }
            }

            //goto loop1;

            Console.Read();

        }
        

        /// <summary>
        /// Telemetry message
        /// </summary>
        /// <param name="s_deviceClient"></param>
        private static async void SendDeviceToCloudMessagesAsync(DeviceClient s_deviceClient)
        {
            try
            {
                double minTemperature = 20;
                double minHumidity = 60;
                Random rand = new Random();

                while (true)
                {
                    double currentTemperature = minTemperature + rand.NextDouble() * 15;
                    double currentHumidity = minHumidity + rand.NextDouble() * 20;

                    // Create JSON message  

                    var telemetryDataPoint = new
                    {

                        temperature = currentTemperature,
                        humidity = currentHumidity
                    };

                    string messageString = "";



                    messageString = JsonConvert.SerializeObject(telemetryDataPoint);

                    var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageString));

                    // Add a custom application property to the message.  
                    // An IoT hub can filter on these properties without access to the message body.  
                    //message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");  

                    // Send the telemetry message  
                    await s_deviceClient.SendEventAsync(message);
                    Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
                    await Task.Delay(1000 * 10);

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }



        /// <summary>
        /// Update the properties for IOT hub
        /// </summary>
        /// <returns></returns>
        private static async Task UpdateDeviceAsync()
        {
            try
            {
                Console.WriteLine("Update device");

                var d = await registryManager.GetDeviceAsync("device0");

                if (d != null)
                {
                    d.Status = DeviceStatus.Disabled;
                    d.StatusReason = "Disabled for test";

                    var dd = await registryManager.UpdateDeviceAsync(d);
                }
                else
                {
                    Console.WriteLine("Device not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex.Message}");
            }
        }

        /// <summary>
        /// Remove device for IOT hub
        /// </summary>
        /// <returns></returns>
        private static async Task RemoveDeviceAsync()
        {
            try
            {
                Console.WriteLine("Remove device");

                await registryManager.RemoveDeviceAsync("device0");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex.Message}");
            }
        }

        /// <summary>
        /// Adding device to IOT HUB  , for demo added two device device1demo and device2demo
        /// </summary>
        /// <returns></returns>
        private static async Task AddDeviceAsync(string newDeviceId)
        {

            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(newDeviceId));

            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(newDeviceId);
            }
            Console.WriteLine("Generated device key:{0}", device.Authentication.SymmetricKey.PrimaryKey);
        }

    }
}
