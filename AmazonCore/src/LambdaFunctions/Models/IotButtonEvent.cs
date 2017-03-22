using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LambdaFunctions.Models
{
    public class IotButtonEvent
    {
        public string SerialNumber { get; set; }
        public string BatteryVoltage { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ClickType ClickType { get; set; }
    }
}
