namespace SX1268Library;

public enum ModuleType : byte
{
    MODULE_E22400T22S,
    MODULE_E22400T30S,
    MODULE_E22230T22S,
    MODULE_E22230T30S,
    MODULE_900900T22S,
    MODULE_900900T30S,
    MODULE_E90DTU400SL37,
    MODULE_E90DTU230SL37,
    Unknown = 0xFF
}

public enum ModuleFrequency : byte
{
    FREQ_170MHZ,
    FREQ_230MHZ,
    FREQ_315MHZ,
    FREQ_433MHZ,
    FREQ_470MHZ,
    FREQ_780MHZ,
    FREQ_868MHZ,
    FREQ_915MHZ,
    FREQ_2_4GHZ,
    FREQ_400MHZ,
    FREQ_900MHZ
}

public enum ModuleUartRate : byte
{
    RATE_1200BPS,
    RATE_2400BPS,
    RATE_4800BPS,
    RATE_9600BPS,
    RATE_19200BPS,
    RATE_38400BPS,
    RATE_57600BPS,
    RATE_115200BPS
}

public enum ModuleUartParity : byte
{
    PARITY_8N1,
    PARITY_8O1,
    PARITY_8E1
}

public enum ModuleAirRate : byte
{
    RATE_0K3,
    RATE_1K2,
    RATE_2K4,
    RATE_4K8,
    RATE_9K6,
    RATE_19K2,
    RATE_38K4,
    RATE_62K5
}

public enum ModuleWakeOnRadioRate : byte
{
    RATE_500MS,
    RATE_1000MS,
    RATE_1500MS,
    RATE_2000MS,
    RATE_2500MS,
    RATE_3000MS,
    RATE_3500MS,
    RATE_4000MS
}

public enum ModulePacketSize : byte
{
    SIZE_240B,
    SIZE_128B,
    SIZE_64B,
    SIZE_32B
}

public enum ModulePower : byte
{
    POWER_22DBM,
    POWER_17DBM,
    POWER_13DBM,
    POWER_10DBM
}

public enum ModuleTranslateMode : byte
{
    TRANSLATE_NORMAL,
    TRANSLATE_FIXED
}

public enum ModuleWorMode : byte
{
    WOR_TRANSLATE,
    WOR_RECEIVE
}

public class Constants
{
    public static string[] ModuleTypeNames = 
    [
        "E22-400T22S",
        "E22-400T30S",
        "E22-230T22S",
        "E22-230T30S",
        "900-900T22S",
        "900-900T30S",
        "E90-DTU(400SL37)",
        "E90-DTU(230SL37)"
    ];

    public static string[] FrequencyNames =
    [
        "170MHz",
        "230MHz",
        "315MHz",
        "433MHz",
        "470MHz",
        "780MHz",
        "868MHz",
        "915MHz",
        "2.4GHz",
        "400MHz",
        "900MHz"
    ];

    public static string[] UartRateNames = 
    [
        "1200bps",
        "2400bps",
        "4800bps",
        "9600bps",
        "19200bps",
        "38400bps",
        "57600bps",
        "115200bps"
    ];

    public static string[] AirRateNames = 
    [
        "0.3Kbps",
        "1.2Kbps",
        "2.4Kbps",
        "4.8Kbps",
        "9.6Kbps",
        "19.2Kbps",
        "38.4Kbps",
        "62.5Kbps"
    ];

    public static string[] WakeOnRadioCycleNames =
    [
        "500ms",
        "1000ms",
        "1500ms",
        "2000ms",
        "2500ms",
        "3000ms",
        "3500ms",
        "4000ms"
    ];

    public static string[] PacketLengthNames =
    [
        "240 Bytes",
        "128 Bytes",
        "64 Bytes",
        "32 Bytes"
    ];

    public static string[] PowerNames =
    [
        "22dBm",
        "17dBm",
        "13dBm",
        "10dBm"
    ];

    public static string[] UartParityNames =
    [
        "8N1",
        "8O1",
        "8E1",
        "8N1"
    ];

    public static string[] TranslateModeNames =
    [
        "Normal",
        "Fixed"
    ];

    public static string[] WorModeNames =
    [
        "Translate",
        "Receive"
    ];

    public static double[] DeviceFrequencies =
    [
        170,
        230,
        315,
        433,
        470,
        780,
        868,
        915,
        2400,
        400,
        900
    ];

    public static ushort[] BufferSizes =
    [
        240,
        128,
        64,
        32
    ];
}