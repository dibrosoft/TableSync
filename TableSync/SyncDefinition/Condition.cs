using System.Collections.ObjectModel;

namespace TableSync
{
    public class Condition : Collection<ConditionItem>
    {
        internal Condition Clone()
        {
            var result = new Condition();

            foreach (var item in this)
                result.Add(item.Clone());

            return result;
        }
    }

}
