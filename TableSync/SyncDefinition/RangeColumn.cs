using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TableSync
{
    public class RangeColumn
    {
        public string Name { get; set; }

        public string Title { get; set; }

        [JsonIgnore()]
        public string DisplayTitle { get { return string.IsNullOrEmpty(Title) ? Name : Title; } }

        [JsonConverter(typeof(StringEnumConverter))]
        public NumberFormat NumberFormat { get; set; }

        public string CustomNumberFormat { get; set; }

        [JsonIgnore()]
        public string DisplayNumberFormat { get { return NumberFormat == NumberFormat.Custom ? CustomNumberFormat : NumberFormat.FormatString(); } }

        public RangeColumn Clone()
        {
            return new RangeColumn()
            {
                Name = Name,
                Title = Title,
                NumberFormat = NumberFormat,
                CustomNumberFormat = CustomNumberFormat
            };
        }
    }

}
