using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
    }

}
