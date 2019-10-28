using System;
using System.Collections.ObjectModel;
using System.Text;

namespace TableSync
{
    public class TableInfos : KeyedCollection<string, TableInfo>
    {
        public TableInfos() : base(StringComparer.InvariantCultureIgnoreCase) { }

        protected override string GetKeyForItem(TableInfo item)
        {
            return item.FullTableName;
        }
    }

    public class TableInfo
    {
        public TableInfo(string schema, string tableName)
        {
            this.Schema = schema;
            this.TableName = tableName;
        }

        public string Schema { get; set; }
        public string TableName { get; set; }

        public string FullTableName { get { return CreateSqlTableName(Schema, TableName); } }
        public string RangeTableName { get { return CreateRangeTableName(Schema, TableName); } }

        public ColumnInfos ColumnInfos { get; set; } = new ColumnInfos();
        public DependsOn DependsOn { get; set; } = new DependsOn();

        public static string CreateSqlTableName(string schema, string tableName)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(schema) && string.Compare(schema, "dbo", true) != 0)
                sb.Append($"[{schema}].");

            sb.Append($"[{tableName}]");

            return sb.ToString();
        }
        public static string CreateRangeTableName(string schema, string tableName)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(schema) && string.Compare(schema, "dbo", true) != 0)
                sb.Append($"{schema}_");

            sb.Append(tableName);

            return sb.ToString();
        }

        public override string ToString()
        {
            return $"{FullTableName} [Schema = {Schema}, TableName = {TableName}, DependsOn = {DependsOn}]";
        }
    }

    public class DependsOn : KeyedCollection<string, string>
    {
        public DependsOn() : base(StringComparer.InvariantCultureIgnoreCase) { }

        protected override string GetKeyForItem(string item)
        {
            return item;
        }

        public override string ToString()
        {
            return string.Join(",", Items);
        }
    }
}