namespace TableSync
{
    public class ConnectionInfo
    {
        public string ConnectionString { get; set; }
        public HiddenTableNames ReservedTableNames { private get; set; }
        public bool IsReservedTableName(string tableName) => ReservedTableNames != null && ReservedTableNames.Contains(tableName);
    }
}
