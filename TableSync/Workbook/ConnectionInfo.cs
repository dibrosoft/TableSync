namespace TableSync
{
    public class ConnectionInfo
    {
        public string ConnectionString { get; set; }
        public HiddenTableNames HiddenTableNames { private get; set; }
        public bool IsReservedTableName(string tableName) => HiddenTableNames != null && HiddenTableNames.Contains(tableName);
    }
}
