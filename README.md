# SX1268Library

This library simplifies control of [SX1268](https://www.waveshare.com/wiki/SX1268_433M_LoRa_HAT) LoRa HAT module in C#. The library is designed and configured to run on a Raspberry Pi 4.  

## Usage

> [!NOTE]  
> All samples below assume that LoRa HAT is exposed on `/dev/serial0` on a Raspberry Pi 4. Please visit [SX1268 Wiki](https://www.waveshare.com/wiki/SX1268_433M_LoRa_HAT) for details about configuring the module for Raspberry Pi.

- Below is a short snippet to send data over LoRa using Raspberry Pi 4, without modifying module configuration.
  ```c#
  var module = SX1268.CreateRaspberryPi("/dev/serial0");
  var bytes = Encoding.ASCII.GetBytes("Hello World!");
  module.SendData(bytes);
  ```
  - Similarly, one can create an instance for Jetson by calling `SX1268.CreateJetsonOrinNano("/dev/serial0")` or `SX1268.CreateJetsonAlternative("/dev/serial0")`, depending on how Jetson is setup.

- It is possible to modify various properties of the module on initialization, by passing a `SX1268Configuration` instance to the constructor. This method requires one to explicitly state all GPIO pins to be used (for M0 and M1 pins of SX126x module)
  ```c#
  var config = new SX1268Configuration()
  {
      AirSpeed = RATE_9K6,
      BufferSize = SIZE_128B,
      Power = POWER_22DBM
  };

  var module = new SX1268("/dev/serial0", 22, 27, config);
  var bytes = Encoding.ASCII.GetBytes("Hello World!");
  module.SendData(bytes);
  ```

- Changing module properties can also be done after the class is initialized.
  ```c#
  var config = new SX1268Configuration()
  {
      AirSpeed = RATE_9K6,
      BufferSize = SIZE_128B,
      Power = POWER_22DBM
  };

  var module = SX1268.CreateRaspberryPi("/dev/serial0");
  var bytes = Encoding.ASCII.GetBytes("Old configuration");
  
  // Send with the old configuration.
  module.SendData(bytes);
  
  // Modify some of the properties.
  module.SetConfiguration(config);

  // Send with the new configuration.
  bytes = Encoding.ASCII.GetBytes("New configuration");
  module.SendData(bytes);
  ```

- To receive data from the module, one can use the `DataReceived` event.
  ```c#
  var config = new SX1268Configuration()
  {
      AirSpeed = RATE_9K6,
      BufferSize = SIZE_128B,
      Power = POWER_22DBM
  };

  var module = SX1268.CreateRaspberryPi("/dev/serial0");
  module.DataAvailable += (sender, args) => 
  {
      Console.WriteLine($"Received {args.Payload.Length} bytes: {Encoding.ASCII.GetString(args.Payload)}");
  }
  ```

## Credits
This project is solely based on [SX126x binaries & source code package](https://files.waveshare.com/upload/1/18/SX126X_LoRa_HAT_CODE.zip) provided in [SX1268 Wiki](https://www.waveshare.com/wiki/SX1268_433M_LoRa_HAT) page.