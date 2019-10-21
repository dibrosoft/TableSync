using System;
using System.Collections.ObjectModel;

namespace TableSync
{
    public class ColumnInfo
    {
        public ColumnInfo(string columnName, string columnType, bool isRequired)
        {
            this.ColumnName = columnName;
            this.ColumnType = columnType;
            this.IsRequired = isRequired;
            this.IsPrimary = false;
        }


        public string ColumnName { get; set; }

        public string ColumnType { get; set; }

        public bool IsRequired { get; set; }

        public bool IsPrimary { get; set; }

        public override string ToString()
        {
            return $"{ColumnName} [Type = {ColumnType}, IsRequired = {IsRequired}, IsPrimary = {IsPrimary}]";
        }
    }

    public class ColumnInfos : KeyedCollection<string, ColumnInfo>
    {
        public ColumnInfos() : base(StringComparer.InvariantCultureIgnoreCase) { }

        protected override string GetKeyForItem(ColumnInfo item)
        {
            return item.ColumnName;
        }
    }
}

