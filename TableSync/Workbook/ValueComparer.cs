using System;

namespace TableSync
{
    public static class ValueComparer
    {
        public static bool AreEqual(object value1, object value2, string columnType)
        {
            if (value1 == null)
                throw new ArgumentException("value1");

            if (value2 == null)
                throw new ArgumentException("value2");

            if (value1.GetType().FullName != value2.GetType().FullName)
                return false;

            return (dynamic)value1 == (dynamic)value2;
        }
    }
}
