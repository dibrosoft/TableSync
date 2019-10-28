using System;
using System.Collections.ObjectModel;

namespace TableSync
{
    public class Columns : KeyedCollection<string, Column>
    {
        public Columns() : base(StringComparer.InvariantCultureIgnoreCase) { }

        protected override string GetKeyForItem(Column item)
        {
            return item.Name;
        }

        public Columns Clone()
        {
            var result = new Columns();

            foreach (var item in this)
                result.Add(item.Clone());

            return result;
        }
    }

}
