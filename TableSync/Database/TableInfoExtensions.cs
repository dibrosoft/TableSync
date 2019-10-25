using System.Collections.Generic;
using System.Linq;

namespace TableSync
{
    public static class TableInfoExtensions
    {
        public static IEnumerable<ColumnInfo> PrimaryColumnInfos(this TableInfo tableInfo)
        {
            return tableInfo.ColumnInfos.Where(item => item.IsPrimary).ToList();
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