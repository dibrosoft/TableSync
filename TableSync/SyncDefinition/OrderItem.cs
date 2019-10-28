using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TableSync
{
    public class OrderItem
    {
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public OrderDirection Direction { get; set; }

        public OrderItem Clone()
        {
            return new OrderItem()
            {
                Name = Name,
                Direction = Direction
            };
        }
    }

}
