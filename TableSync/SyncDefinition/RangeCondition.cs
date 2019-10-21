using System.Collections.ObjectModel;

namespace TableSync
{
    public class RangeCondition : Collection<RangeConditionItem>
    {
        internal RangeCondition Clone()
        {
            var result = new RangeCondition();

            foreach (var item in this)
                result.Add(item.Clone());

            return result;
        }
    }

}
