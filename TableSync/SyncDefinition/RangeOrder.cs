using System;
using System.Collections.ObjectModel;

namespace TableSync
{
    public class RangeOrder : KeyedCollection<string, RangeOrderItem>
    {
        public RangeOrder() : base(StringComparer.InvariantCultureIgnoreCase) { }

        protected override string GetKeyForItem(RangeOrderItem item)
        {
            return item.Name;
        }

        internal RangeOrder Clone()
        {
            var result = new RangeOrder();

            foreach (var item in this)
                result.Add(item.Clone());

            return result;
        }
    }

}
