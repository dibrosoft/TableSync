using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TableSync
{
    public class Range
    {
        public Range() { }

        public Range(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public string Schema { get; set; }
        public string TableName { get; set; }

        [JsonIgnore()]
        public string FullTableName
        {
            get
            {
                var usedSchema = Schema;
                var usedTableName = TableName;

                if (string.IsNullOrEmpty(usedSchema) && string.IsNullOrEmpty(usedTableName))
                {
                    var parts = Name.Split('_');

                    switch (parts.Length)
                    {
                        case 1:
                            usedSchema = "dbo";
                            usedTableName = parts[0];
                            break;
                        case 2:
                            if (Name.Contains("_"))
                            {
                                usedSchema = Name.Split('_')[0];
                                usedTableName = Name.Substring(usedSchema.Length + 1);

                                if (string.IsNullOrEmpty(usedSchema) || string.IsNullOrEmpty(usedTableName))
                                    throw new IllegalRangeNameException();
                            }
                            break;
                        default:
                            throw new IllegalRangeNameException();
                    }
                }

                return TableInfo.CreateSqlTableName(usedSchema, usedTableName);
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public RangeOrientation Orientation { get; set; }

        public RangeColumns Columns { get; set; }
        public RangeOrder Order { get; set; }
        public RangeCondition Condition { get; set; }

        [JsonIgnore()]
        public bool HasColumns { get { return Columns != null && Columns.Count > 0; } }

        [JsonIgnore()]
        public bool HasOrder { get { return Order != null && Order.Count > 0; } }

        [JsonIgnore()]
        public bool HasCondition { get { return Condition != null && Condition.Count > 0; } }

        public Range Clone()
        {
            return new Range()
            {
                Name = Name,
                Schema = Schema,
                TableName = TableName,
                Orientation = Orientation,
                Columns = Columns?.Clone(),
                Order = Order?.Clone(),
                Condition = Condition?.Clone()
            };
        }
    }
}
