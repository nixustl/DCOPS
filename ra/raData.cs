namespace ra
{
    public class raData
    {
    }
}

public class Rootobject
{
    public General general { get; set; }
    public Debug debug { get; set; }
    public Digitalsensor[] digitalSensors { get; set; }
    public Switchsensor[] switchSensors { get; set; }
    public Analogsensor[] analogSensors { get; set; }
    public Externalrelay[] externalRelays { get; set; }
    public Internalrelay[] internalRelays { get; set; }
    public Powersensor powerSensor { get; set; }
}

public class General
{
    public string schemaVersion { get; set; }
    public string environment { get; set; }
    public string deviceName { get; set; }
    public int deviceTimestamp { get; set; }
    public string temperatureScale { get; set; }
    public string unitOfSpeed { get; set; }
    public string macAddress { get; set; }
    public string uniqueId { get; set; }
    public string chipType { get; set; }
    public int deviceType { get; set; }
    public int interfaceRefreshRate { get; set; }
    public string firmwareVersion { get; set; }
    public int httpPort { get; set; }
    public bool https { get; set; }
    public int httpsCertExpiration { get; set; }
    public string ipv4Address { get; set; }
    public string serialNumber { get; set; }
    public int raaPushInterval { get; set; }
    public int raaPushTimeout { get; set; }
    public bool raaPushEnabled { get; set; }
    public int customPushInterval { get; set; }
    public int customPushTimeout { get; set; }
    public bool customPushEnabled { get; set; }
    public int bid { get; set; }
}

public class Debug
{
    public int sensorPicVersion { get; set; }
    public bool sensorPicActive { get; set; }
    public bool resetButtonState { get; set; }
    public int uptime { get; set; }
    public Timeconfiguration timeConfiguration { get; set; }
    public Ethernetconfiguration ethernetConfiguration { get; set; }
    public int lastRaaTransmission { get; set; }
    public int lastCustomTransmission { get; set; }
}

public class Timeconfiguration
{
    public int timezone { get; set; }
    public bool displayAmPm { get; set; }
    public bool displayMonthFirst { get; set; }
    public bool daylightSavingsEnabled { get; set; }
}

public class Ethernetconfiguration
{
    public bool autoNegotiateEnabled { get; set; }
}

public class Powersensor
{
    public bool connected { get; set; }
    public string label { get; set; }
    public bool state { get; set; }
    public bool alarmEnabled { get; set; }
    public bool alarmState { get; set; }
}

public class Digitalsensor
{
    public int id { get; set; }
    public int type { get; set; }
    public bool connected { get; set; }
    public string label { get; set; }
    public bool alarmEnabled { get; set; }
    public bool alarmState { get; set; }
    public float temperature { get; set; }
    public float temperatureHigh { get; set; }
    public float temperatureLow { get; set; }
    public float humidity { get; set; }
    public float humidityHigh { get; set; }
    public float humidityLow { get; set; }
    public float heatIndex { get; set; }
    public float heatIndexHigh { get; set; }
    public float heatIndexLow { get; set; }
    public float dewPoint { get; set; }
    public float dewPointHigh { get; set; }
    public float dewPointLow { get; set; }
}

public class Switchsensor
{
    public int id { get; set; }
    public string label { get; set; }
    public bool enabled { get; set; }
    public bool alarmState { get; set; }
    public bool state { get; set; }
}

public class Analogsensor
{
    public string label { get; set; }
    public int id { get; set; }
    public int type { get; set; }
    public bool enabled { get; set; }
}

public class Externalrelay
{
    public int id { get; set; }
    public bool connected { get; set; }
    public int type { get; set; }
    public string label { get; set; }
    public Relay[] relays { get; set; }
}

public class Relay
{
    public int id { get; set; }
    public bool state { get; set; }
}

public class Internalrelay
{
    public int id { get; set; }
    public string label { get; set; }
    public Relay1 relay { get; set; }
}

public class Relay1
{
    public int id { get; set; }
    public bool state { get; set; }
}

public class DCOPS_Message
{
    public bool isError { get; set; }
    public int errorNumber { get; set; } = 0;
    public string? msg { get; set; }
    public const int SuccessCode = 0;
}
