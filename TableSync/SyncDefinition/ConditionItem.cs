using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TableSync
{
    public class ConditionItem
    {
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ConditionOperator Operator { get; set; }


        public string OperatorTemplate { get; set; }

        public object Value { get; set; }

        public ConditionItem Clone()
        {
            return new ConditionItem()
            {
                Name = Name,
                Value = Value,
                Operator = Operator,
                OperatorTemplate = OperatorTemplate
            };
        }
    }

}
