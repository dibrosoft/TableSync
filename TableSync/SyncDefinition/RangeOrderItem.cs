using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TableSync
{
    public class RangeOrderItem
    {
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public RangeOrderDirection Direction { get; set; }

        public RangeOrderItem Clone()
        {
            return new RangeOrderItem()
            {
                Name = Name,
                Direction = Direction
            };
        }
    }

}
