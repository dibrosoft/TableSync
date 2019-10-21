using System;
using System.Collections.ObjectModel;

namespace TableSync
{
    public class Settings : KeyedCollection<string, Setting>
    {
        public Settings() : base(StringComparer.InvariantCultureIgnoreCase) { }

        protected override string GetKeyForItem(Setting item)
        {
            return item.Name;
        }

        public Settings Clone()
        {
            var result = new Settings();

            foreach (var setting in this)
                result.Add(setting.Clone());

            return result;
        }
    }

}
