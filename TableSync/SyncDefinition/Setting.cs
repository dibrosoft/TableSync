using System;

namespace TableSync
{
    public class Setting
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public Setting Clone()
        {
            return new Setting() { Name = Name, Value = Value };
        }
    }

}
