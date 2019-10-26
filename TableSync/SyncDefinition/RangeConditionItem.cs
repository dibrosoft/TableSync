using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TableSync
{
    public class RangeConditionItem
    {
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public RangeConditionOperator Operator { get; set; }


        public string OperatorTemplate { get; set; }

        public object Value { get; set; }

        public RangeConditionItem Clone()
        {
            return new RangeConditionItem()
            {
                Name = Name,
                Value = Value,
                Operator = Operator,
                OperatorTemplate = OperatorTemplate
            };
        }
    }

}
