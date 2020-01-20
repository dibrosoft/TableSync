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
    }
}