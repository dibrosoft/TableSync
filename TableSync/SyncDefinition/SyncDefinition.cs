using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TableSync
{
    public class SyncDefinition
    {
        public SyncDefinition() {  }

        public SyncDefinition(IEnumerable<string> names)
        {
            foreach (var name in names)
                Ranges.Add(new Range(name));
        }

        public Ranges Ranges { get; set; } = new Ranges();
        public Settings Settings { get; set; }

        [JsonIgnore()]
        public bool HasSettings { get { return Settings != null && Settings.Count > 0; } }


        [JsonIgnore()]
        public HashSet<string> TablesOfInterest
        {
            get
            {
                return new HashSet<string>(Ranges.Select(item => item.FullTableName).Distinct(), StringComparer.InvariantCultureIgnoreCase);
            }
        }
    }

}
