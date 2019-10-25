using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TableSync
{

    public class Ranges : KeyedCollection<string, Range>
    {
        public Ranges() : base(StringComparer.InvariantCultureIgnoreCase) { }

        public Ranges(IEnumerable<Range> newItems) : base(StringComparer.InvariantCultureIgnoreCase)
        {
            foreach (var item in newItems)
                this.Add(item);
        }

        protected override string GetKeyForItem(Range item)
        {
            return item.Name;
        }

        public Range SearchByFullTableName(string fullTableName)
        {
            return this.Where(item => string.Compare(item.FullTableName, fullTableName, true) == 0).Single();
        }
    }

}
