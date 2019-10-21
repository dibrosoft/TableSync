using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;

namespace TableSync
{
    public class DatabaseInfo
    {
        public TableInfos TableInfos { get; set; } = new TableInfos();

        public DatabaseInfo(string connectionString, HashSet<string> tablesOfInterest = null)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                GenerateTableInfos(connection, tablesOfInterest);

                AddDependsOn(connection);

                CheckForTablesOfInterest(tablesOfInterest);
            }
        }

        private void GenerateTableInfos(SqlConnection connection, HashSet<string> tablesOfInterest)
        {
            var dataTypeMap = GenerateDataTypeMap(connection);

            DataView columns = new DataView(connection.GetSchema("Columns"));
            columns.Sort = "TABLE_SCHEMA, TABLE_NAME, ORDINAL_POSITION";

            string schema = null;
            string tableName = null;
            List<ColumnInfo> columnSources = new List<ColumnInfo>();

            foreach (DataRowView column in columns)
            {
                var newSchema = column["TABLE_SCHEMA"].ToString();
                var newTableName = column["TABLE_NAME"].ToString();
                var newFullTableName = TableInfo.CreateSqlTableName(newSchema, newTableName);
                if (tablesOfInterest == null || tablesOfInterest.Contains(newFullTableName))
                {
                    var columnName = column["COLUMN_NAME"].ToString();
                    var dataType = column["DATA_TYPE"].ToString();
                    var columnType = dataTypeMap.ContainsKey(dataType) ? dataTypeMap[dataType] : "System.Byte[]";
                    var isRequired = string.Compare(column["IS_NULLABLE"].ToString(), "NO", true) == 0;
                    ColumnInfo NewColumnSource = new ColumnInfo(columnName, columnType, isRequired);

                    if (tableName == null)
                    {
                        schema = newSchema;
                        tableName = newTableName;
                    }

                    if (string.Compare(tableName, newTableName, true) == 0 && string.Compare(schema, newSchema, true) == 0)
                        columnSources.Add(NewColumnSource);
                    else
                    {
                        Refresh(schema, tableName, columnSources);

                        schema = newSchema;
                        tableName = newTableName;
                        columnSources = new List<ColumnInfo>();
                        columnSources.Add(NewColumnSource);
                    }
                }
            }

            if (columnSources.Count > 0)
                Refresh(schema, tableName, columnSources);
        }

        private Dictionary<string, string> GenerateDataTypeMap(SqlConnection connection)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (DataRow row in connection.GetSchema("DataTypes").Rows)
                result.Add(row["TypeName"].ToString(), row["DataType"].ToString());

            return result;
        }

        private void AddDependsOn(SqlConnection connection)
        {
            using (var command = DbProviderFactories.GetFactory(connection).CreateCommand())
            {
                command.CommandText = "SELECT OBJECT_SCHEMA_NAME(parent_object_id) AS SchemaName, OBJECT_NAME(parent_object_id) AS Name, OBJECT_SCHEMA_NAME(referenced_object_id) AS DependsOnSchemaName, OBJECT_NAME(referenced_object_id) AS DependsOnName FROM sys.foreign_keys";
                command.Connection = connection;
                using (var dataAdapter = DbProviderFactories.GetFactory(connection).CreateDataAdapter())
                {
                    dataAdapter.SelectCommand = command;

                    DataTable dataTable = new DataTable();
                    dataTable.Locale = CultureInfo.InvariantCulture;

                    dataAdapter.Fill(dataTable);
                    foreach (DataRow dependency in dataTable.Rows)
                    {
                        var name = dependency["Name"].ToString();
                        var schemaName = dependency["SchemaName"].ToString();
                        var fullName = TableInfo.CreateSqlTableName(schemaName, name);

                        var dependsOnSchemaName = dependency["DependsOnSchemaName"].ToString();
                        var dependsOnName = dependency["DependsOnName"].ToString();
                        var dependsOnFullName = TableInfo.CreateSqlTableName(dependsOnSchemaName, dependsOnName);

                        if (TableInfos.Contains(fullName))
                        {
                            var tableInfo = TableInfos[fullName];
                            if (!tableInfo.DependsOn.Contains(dependsOnFullName))
                                tableInfo.DependsOn.Add(dependsOnFullName);
                        }
                    }
                }
            }
        }

        private void CheckForTablesOfInterest(HashSet<string> tablesOfInterest)
        {
            if (tablesOfInterest == null)
                return;

            foreach (var tableOfInterest in tablesOfInterest)
                if (!TableInfos.Contains(tableOfInterest))
                    throw new MissingTableException(tableOfInterest);
        }

        private void Refresh(string schema, string tableName, IEnumerable<ColumnInfo> columnSources)
        {
            var fullTableName = TableInfo.CreateSqlTableName(schema, tableName);

            if (!TableInfos.Contains(fullTableName))
            {
                TableInfo tableInfo = TableInfoExtensions.Create(schema, tableName, columnSources);
                if (tableInfo != null)
                    TableInfos.Add(tableInfo);
            }
        }
    }
}
