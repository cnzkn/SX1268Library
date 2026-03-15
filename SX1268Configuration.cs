using System.Text;

namespace SX1268Library;

public class SX1268Configuration
{
    public ModuleType? ModuleType { get; private set; }
    public ushort? DeviceName { get; set; }
    public byte? DeviceVersion { get; private set; }
    public byte? DevicePower { get; private set; }
    public ModuleFrequency? DeviceFrequency { get; private set; }

    public byte? Channel { get; set; } // Frequency offset
    public short? Address { get; set; }
    public ModulePower? Power { get; set; }
    public bool? PacketRssiEnabled { get; set; }
    public bool? ChannelRssiEnabled { get; set; }
    public ModuleAirRate? AirSpeed { get; set; }
    public byte? NetId { get; set; }
    public ModulePacketSize? BufferSize { get; set; }
    public ModuleUartRate? UartRate { get; set; }
    public ModuleUartParity? UartParity { get; set; }
    public short? EncryptionKey { get; set; }
    public bool? Relay { get; set; }
    public bool? LBT { get; set; }
    public ModuleTranslateMode? TranslateMode { get; set; }

    public ModuleWorMode? WorMode { get; set; }
    public ModuleWakeOnRadioRate? WorRate { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Module Type: {ModuleType}");
        sb.AppendLine($"Device Name: {DeviceName}");
        sb.AppendLine($"Device Version: {DeviceVersion}");
        sb.AppendLine($"Device Power: {DevicePower}");
        sb.AppendLine($"Device Frequency: {DeviceFrequency}");
        sb.AppendLine($"Channel: {Channel}");
        sb.AppendLine($"Address: {Address}");
        sb.AppendLine($"Power: {Power}");
        sb.AppendLine($"Packet RSSI Enabled: {PacketRssiEnabled}");
        sb.AppendLine($"Channel RSSI Enabled: {ChannelRssiEnabled}");
        sb.AppendLine($"Air Speed: {AirSpeed}");
        sb.AppendLine($"Net ID: {NetId}");
        sb.AppendLine($"Buffer Size: {BufferSize}");
        sb.AppendLine($"UART Rate: {UartRate}");
        sb.AppendLine($"UART Parity: {UartParity}");
        sb.AppendLine($"Encryption Key: {EncryptionKey}");
        sb.AppendLine($"Relay: {Relay}");
        sb.AppendLine($"LBT: {LBT}");
        sb.AppendLine($"Translate Mode: {TranslateMode}");
        sb.AppendLine($"Wake-on-Radio Mode: {WorMode}");
        sb.AppendLine($"Wake-on-Radio Rate: {WorRate}");

        return sb.ToString();
    }

    public void FromByteArray(byte[] reg)
    {
        Address = (short)((reg[3] << 8) | reg[4]);
        NetId = reg[5];
        UartRate = (ModuleUartRate)((reg[6] >> 5) & 0x07);
        UartParity = (ModuleUartParity)((reg[6] >> 3) & 0x03);
        AirSpeed = (ModuleAirRate)(reg[6] & 0x07);
        BufferSize = (ModulePacketSize)((reg[7] >> 6) & 0x03);
        ChannelRssiEnabled = ((reg[7] >> 5) & 0x01) == 1;
        Power = (ModulePower)(reg[7] & 0x03);
        Channel = reg[8];
        PacketRssiEnabled = ((reg[9] >> 7) & 0x01) == 1;
        TranslateMode = (ModuleTranslateMode)((reg[9] >> 6) & 0x01);
        Relay = ((reg[9] >> 5) & 1) == 0x01;
        LBT = ((reg[9] >> 4) & 1) == 0x01;
        WorMode = (ModuleWorMode)((reg[9] >> 3) & 0x01);
        WorRate = (ModuleWakeOnRadioRate)(reg[9] & 0x07);
        EncryptionKey = (short)((reg[10] << 8) | reg[11]);
    }

    public byte[] ToByteArray()
    {
        if (!Channel.HasValue) throw new ArgumentNullException(nameof(Channel));
        if (!Address.HasValue) throw new ArgumentNullException(nameof(Address));
        if (!Power.HasValue) throw new ArgumentNullException(nameof(Power));
        if (!PacketRssiEnabled.HasValue) throw new ArgumentNullException(nameof(PacketRssiEnabled));
        if (!ChannelRssiEnabled.HasValue) throw new ArgumentNullException(nameof(ChannelRssiEnabled));
        if (!AirSpeed.HasValue) throw new ArgumentNullException(nameof(AirSpeed));
        if (!NetId.HasValue) throw new ArgumentNullException(nameof(NetId));
        if (!BufferSize.HasValue) throw new ArgumentNullException(nameof(BufferSize));
        if (!UartRate.HasValue) throw new ArgumentNullException(nameof(UartRate));
        if (!UartParity.HasValue) throw new ArgumentNullException(nameof(UartParity));
        if (!EncryptionKey.HasValue) throw new ArgumentNullException(nameof(EncryptionKey));
        if (!Relay.HasValue) throw new ArgumentNullException(nameof(Relay));
        if (!LBT.HasValue) throw new ArgumentNullException(nameof(LBT));
        if (!TranslateMode.HasValue) throw new ArgumentNullException(nameof(TranslateMode));
        if (!WorMode.HasValue) throw new ArgumentNullException(nameof(WorMode));
        if (!WorRate.HasValue) throw new ArgumentNullException(nameof(WorRate));
        
        var reg = new byte[12];
        byte addrLo = (byte)(Address.Value & 0xFF), addrHi = (byte)((Address.Value >> 8) & 0xFF);
        byte keyLo = (byte)(EncryptionKey.Value & 0xFF), keyHi = (byte)((EncryptionKey.Value >> 8) & 0xFF);

        reg[0] = 0xC0;
        reg[1] = 0x00;
        reg[2] = 0x09;
        reg[3] = addrHi;
        reg[4] = addrLo;
        reg[5] = NetId.Value;
        reg[6] = (byte)(((byte)UartRate.Value << 5) | ((byte)UartParity.Value << 3) | (byte)AirSpeed.Value);
        reg[7] = (byte)(((byte)BufferSize.Value << 6) | ((byte)(ChannelRssiEnabled.Value ? 1 : 0) << 5) | (byte)Power.Value);
        reg[8] = Channel.Value;
        reg[9] = (byte)(((byte)(PacketRssiEnabled.Value ? 1 : 0) << 7) | ((byte)TranslateMode.Value << 6) | ((byte)(Relay.Value ? 1 : 0) << 5) | ((byte)(LBT.Value ? 1 : 0) << 4) | ((byte)WorMode.Value << 3) | (byte)WorRate.Value);
        reg[10] = keyHi;
        reg[11] = keyLo;

        return reg;
    }

    public void ApplyChanges(SX1268Configuration other)
    {
        if (other.Channel.HasValue) Channel = other.Channel.Value;
        if (other.Address.HasValue) Address = other.Address.Value;
        if (other.Power.HasValue) Power = other.Power.Value;
        if (other.PacketRssiEnabled.HasValue) PacketRssiEnabled = other.PacketRssiEnabled.Value;
        if (other.ChannelRssiEnabled.HasValue) ChannelRssiEnabled = other.ChannelRssiEnabled.Value;
        if (other.AirSpeed.HasValue) AirSpeed = other.AirSpeed.Value;
        if (other.NetId.HasValue) NetId = other.NetId.Value;
        if (other.BufferSize.HasValue) BufferSize = other.BufferSize.Value;
        if (other.UartRate.HasValue) UartRate = other.UartRate.Value;
        if (other.UartParity.HasValue) UartParity = other.UartParity.Value;
        if (other.EncryptionKey.HasValue) EncryptionKey = other.EncryptionKey.Value;
        if (other.Relay.HasValue) Relay = other.Relay.Value;
        if (other.LBT.HasValue) LBT = other.LBT.Value;
        if (other.TranslateMode.HasValue) TranslateMode = other.TranslateMode.Value;
        if (other.WorMode.HasValue) WorMode = other.WorMode.Value;
        if (other.WorRate.HasValue) WorRate = other.WorRate.Value;

        ClampChannel();
    }

    internal void DetermineModuleType(ushort name, byte version, byte frequency, byte power)
    {
        DeviceName = name;
        DeviceVersion = version;
        DeviceFrequency = (ModuleFrequency)(frequency - 1);
        DevicePower = power;

        ModuleType = DeviceName switch
        {
            0x22 => // E22
                DeviceFrequency switch
                {
                    ModuleFrequency.FREQ_400MHZ or ModuleFrequency.FREQ_433MHZ => power switch
                    {
                        22 => SX1268Library.ModuleType.MODULE_E22400T22S,
                        30 => SX1268Library.ModuleType.MODULE_E22400T30S,
                        _ => SX1268Library.ModuleType.Unknown
                    },
                    ModuleFrequency.FREQ_230MHZ => power switch
                    {
                        22 => SX1268Library.ModuleType.MODULE_E22230T22S,
                        30 => SX1268Library.ModuleType.MODULE_E22230T30S,
                        _ => SX1268Library.ModuleType.Unknown
                    },
                    ModuleFrequency.FREQ_900MHZ => power switch
                    {
                        22 => SX1268Library.ModuleType.MODULE_900900T22S,
                        30 => SX1268Library.ModuleType.MODULE_900900T30S,
                        _ => SX1268Library.ModuleType.Unknown
                    },
                    _ => ModuleType
                },

            0x90 => // E90
                DeviceFrequency switch
                {
                    ModuleFrequency.FREQ_400MHZ or ModuleFrequency.FREQ_433MHZ => SX1268Library.ModuleType
                        .MODULE_E90DTU400SL37,
                    ModuleFrequency.FREQ_230MHZ => SX1268Library.ModuleType.MODULE_E90DTU230SL37,
                    _ => SX1268Library.ModuleType.Unknown
                },

            _ => SX1268Library.ModuleType.Unknown
        };
    }

    private void ClampChannel()
    {
        if (!Channel.HasValue) return;

        Channel = ModuleType switch
        {
            SX1268Library.ModuleType.Unknown or
            SX1268Library.ModuleType.MODULE_E22400T22S or
            SX1268Library.ModuleType.MODULE_E22400T30S or
            SX1268Library.ModuleType.MODULE_E90DTU400SL37 => 
                Math.Clamp(Channel.Value, (byte)0, (byte)84),

            SX1268Library.ModuleType.MODULE_E22230T22S or 
            SX1268Library.ModuleType.MODULE_E22230T30S or
            SX1268Library.ModuleType.MODULE_E90DTU230SL37 => 
                Math.Clamp(Channel.Value, (byte)0, (byte)64),

            SX1268Library.ModuleType.MODULE_900900T22S or
            SX1268Library.ModuleType.MODULE_900900T30S =>
                Math.Clamp(Channel.Value, (byte)0, (byte)61),

            _ => Channel
        };
    }
}