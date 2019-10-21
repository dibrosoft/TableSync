using System;
using System.Collections.ObjectModel;

namespace TableSync
{
    public class RangeColumns : KeyedCollection<string, RangeColumn>
    {
        public RangeColumns() : base(StringComparer.InvariantCultureIgnoreCase) { }

        protected override string GetKeyForItem(RangeColumn item)
        {
            return item.Name;
        }

        public RangeColumns Clone()
        {
            var result = new RangeColumns();

            foreach (var item in this)
                result.Add(item.Clone());

            return result;
        }
    }

}
