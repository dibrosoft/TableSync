using System.Collections.Generic;

namespace TableSync
{
    public static class TableInfoExtensions
    {
        public static ColumnInfo PrimaryColumnInfo(this TableInfo tableInfo)
        {
            foreach (ColumnInfo columnInfo in tableInfo.ColumnInfos)
            {
                if (columnInfo.IsPrimary)
                    return columnInfo;
            }
            return null;
        }

        public static TableInfo Create(string schema, string tableName, IEnumerable<ColumnInfo> ColumnSources)
        {
            TableInfo Result = new TableInfo(schema, tableName);

            var columnInfos = new ColumnInfos();
            Result.ColumnInfos = columnInfos;

            bool FirstDataColumn = true;
            foreach (ColumnInfo ColumnSource in ColumnSources)
            {
                if (FirstDataColumn)
                {
                    ColumnSource.IsPrimary = true;
                    FirstDataColumn = false;
                }

                columnInfos.Add(ColumnSource);
            }

            return Result;
        }
    }
}