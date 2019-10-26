using System;
using Microsoft.SqlServer.Types;

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
                switch (expectedType.FullName)
                {
                    case "Microsoft.SqlServer.Types.SqlGeography":
                        return SqlGeography.Parse(new System.Data.SqlTypes.SqlString(workbookValue.ToString()));
                    case "Microsoft.SqlServer.Types.SqlGeometry":
                        return SqlGeometry.Parse(new System.Data.SqlTypes.SqlString(workbookValue.ToString()));
                    case "Microsoft.SqlServer.Types.SqlHierachyId":
                        return SqlHierarchyId.Parse(new System.Data.SqlTypes.SqlString(workbookValue.ToString()));
                    default:
                        if (workbookValue is double)
                        {
                            var dateTimeExpected = expectedType == typeof(DateTime);
                            if (dateTimeExpected)
                                return DateTime.FromOADate((double)workbookValue);
                        }
                        break;
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
