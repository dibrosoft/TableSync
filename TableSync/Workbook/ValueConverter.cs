using System;
using System.Collections.Generic;
using System.Text;

namespace TableSync
{
    public static class ValueConverter
    {
        public static object WorkbookToDatabase(object workbookValue, Type expectedType)
        {
            if (workbookValue == null)
                return DBNull.Value;

            if (workbookValue is string && string.IsNullOrEmpty(workbookValue.ToString()))
                return DBNull.Value;

            if (expectedType != null)
            {
                if (workbookValue is double)
                {
                    var dateTimeExpected = expectedType == typeof(DateTime);
                    if (dateTimeExpected)
                        workbookValue = DateTime.FromOADate((double)workbookValue);
                }
            }

            return workbookValue;
        }

        public static object DatabaseToWorkbook(object databaseValue)
        {
            if (databaseValue == DBNull.Value || (databaseValue is string && string.IsNullOrEmpty(databaseValue.ToString())))
                return null;
            else if (databaseValue is DateTime)
                return ((DateTime)databaseValue).ToOADate();
            else
                return databaseValue;
        }
    }
}
