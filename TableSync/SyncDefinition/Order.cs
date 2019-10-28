using System;
using System.Collections.ObjectModel;

namespace TableSync
{
    public class Order : KeyedCollection<string, OrderItem>
    {
        public Order() : base(StringComparer.InvariantCultureIgnoreCase) { }

        protected override string GetKeyForItem(OrderItem item)
        {
            return item.Name;
        }

        internal Order Clone()
        {
            var result = new Order();

            foreach (var item in this)
                result.Add(item.Clone());

            return result;
        }
    }

}
