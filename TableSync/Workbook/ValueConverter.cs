using System;
using System.Data.SqlTypes;
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
                        return SqlGeography.Parse(new SqlString(workbookValue.ToString()));
                    case "Microsoft.SqlServer.Types.SqlGeometry":
                        return SqlGeometry.Parse(new SqlString(workbookValue.ToString()));
                    case "Microsoft.SqlServer.Types.SqlHierachyId":
                        return SqlHierarchyId.Parse(new SqlString(workbookValue.ToString()));
                    case "System.DateTime":
                        if (workbookValue is double)
                            return DateTime.FromOADate((double)workbookValue);

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
