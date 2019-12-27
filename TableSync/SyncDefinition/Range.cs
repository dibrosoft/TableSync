using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TableSync
{
    public class Range
    {
        public Range() { }

        public Range(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new IllegalRangeNameException();

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
                            usedSchema = parts[0];
                            usedTableName = parts[1];
                            break;
                    }

                    if (string.IsNullOrEmpty(usedSchema) || string.IsNullOrEmpty(usedTableName))
                        throw new IllegalRangeNameException();
                }

                return TableInfo.CreateSqlTableName(usedSchema, usedTableName);
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public Orientation Orientation { get; set; }

        public Columns Columns { get; set; }
        public Order Order { get; set; }
        public Condition Condition { get; set; }

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
