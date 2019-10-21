using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TableSync
{
    public class RangeConditionItem
    {
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public RangeConditionOperator Operator { get; set; }

        public string SettingName { get; set; }

        public RangeConditionItem Clone()
        {
            return new RangeConditionItem()
            {
                Name = Name,
                SettingName = SettingName,
                Operator = Operator
            };
        }
    }

}
