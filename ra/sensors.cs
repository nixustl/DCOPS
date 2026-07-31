using System;

public class sensors
{
    public class DigitalSensor
    {
        public int id { get; set; }
        public int type { get; set; }
        public bool connected { get; set; }
        public string label { get; set; }
        public bool alarmEnabled { get; set; }
        public bool alarmState { get; set; }
        public double temperature { get; set; }
        public double temperatureHigh { get; set; }
        public double temperatureLow { get; set; }
        public double humidity { get; set; }
        public double humidityHigh { get; set; }
        public double humidityLow { get; set; }
        public double heatIndex { get; set; }
        public double heatIndexHigh { get; set; }
        public double heatIndexLow { get; set; }
        public double dewPoint { get; set; }
        public double dewPointHigh { get; set; }
        public double dewPointLow { get; set; }
    }

    public class Result
    {
        public int id { get; set; }
        public string value { get; set; }
        public string info { get; set; }
    }

}
