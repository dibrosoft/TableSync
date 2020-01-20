using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace TableSync
{
    public class DatabaseInfo
    {
        public TableInfos TableInfos { get; set; } = new TableInfos();

        private ConnectionInfo connectionInfo;

        public DatabaseInfo(ConnectionInfo connectionInfo, HashSet<string> tablesOfInterest = null)
        {
            this.connectionInfo = connectionInfo;

            using (var connection = new SqlConnection(connectionInfo.ConnectionString))
            {
                connection.Open();

                GenerateTableInfos(connection, tablesOfInterest);

                AddPrimaryInformation(connection);

                AddDependsOn(connection);

                CheckForTablesOfInterest(tablesOfInterest);
            }
        }

        public TableInfos SearchTableInfos(IEnumerable<string> tableNames)
        {
            if (tableNames == null || tableNames.Count() == 0)
                return TableInfos;

            var result = new TableInfos();

            foreach (var tableName in tableNames)
            {
                var tableInfo =  
                    TableInfos.Where(item => string.Compare(item.TableName, tableName, true) == 0 ||
                                             string.Compare(item.FullTableName, tableName, true) == 0 ||
                                             string.Compare(item.RangeTableName, tableName, true) == 0).SingleOrDefault();
                
                if (tableInfo == null)
                    throw new MissingTableException(tableName);

                result.Add(tableInfo);
            }

            return result;
        }

        private void GenerateTableInfos(SqlConnection connection, HashSet<string> tablesOfInterest)
        {
            var dataTypeMap = GenerateDataTypeMap(connection);

            var columns = new DataView(connection.GetSchema("Columns"));
            columns.Sort = "TABLE_SCHEMA, TABLE_NAME, ORDINAL_POSITION";

            foreach (DataRowView column in columns)
            {
                var tableSchema = column["TABLE_SCHEMA"].ToString();
                var tableName = column["TABLE_NAME"].ToString();

                if (!connectionInfo.IsReservedTableName(tableName))
                {
                    var fullTableName = TableInfo.CreateSqlTableName(tableSchema, tableName);
                    if (tablesOfInterest == null || tablesOfInterest.Contains(fullTableName))
                    {
                        var columnName = column["COLUMN_NAME"].ToString();
                        var dataType = column["DATA_TYPE"].ToString();
                        var columnType = dataTypeMap.ContainsKey(dataType) ? dataTypeMap[dataType] : "System.Byte[]";
                        var isRequired = string.Compare(column["IS_NULLABLE"].ToString(), "NO", true) == 0;
                        var columnInfo = new ColumnInfo(columnName, columnType, dataType, isRequired);

                        if (!TableInfos.Contains(fullTableName))
                            TableInfos.Add(new TableInfo(tableSchema, tableName));

                        var tableInfo = TableInfos[fullTableName];
                        tableInfo.ColumnInfos.Add(columnInfo);
                    }
                }
            }
        }

        private Dictionary<string, string> GenerateDataTypeMap(SqlConnection connection)
        {
            var result = new Dictionary<string, string>();

            foreach (DataRow row in connection.GetSchema("DataTypes").Rows)
                result.Add(row["TypeName"].ToString(), row["DataType"].ToString());

            return result;
        }

        private void AddDependsOn(SqlConnection connection)
        {
            using (var command = new SqlCommand())
            {
                command.CommandText = "SELECT OBJECT_SCHEMA_NAME(parent_object_id) AS SchemaName, OBJECT_NAME(parent_object_id) AS Name, OBJECT_SCHEMA_NAME(referenced_object_id) AS DependsOnSchemaName, OBJECT_NAME(referenced_object_id) AS DependsOnName FROM sys.foreign_keys";
                command.Connection = connection;
                using (var dataAdapter = new SqlDataAdapter())
                {
                    dataAdapter.SelectCommand = command;

                    var dataTable = new DataTable();
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
        private void AddPrimaryInformation(SqlConnection connection)
        {
            const string primaryQuery = @"
SELECT tc.TABLE_SCHEMA, tc.TABLE_NAME, COLUMN_NAME 
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc 
JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu ON tc.CONSTRAINT_NAME = ccu.Constraint_name 
WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'";

            using (var command = new SqlCommand() { Connection = connection, CommandText = primaryQuery })
            using (var dataAdapter = new SqlDataAdapter() { SelectCommand = command })
            using (var dataTable = new DataTable())
            {
                dataAdapter.Fill(dataTable);
                foreach (DataRow row in dataTable.Rows)
                {
                    var tableSchema = row["TABLE_SCHEMA"].ToString();
                    var tableName = row["TABLE_NAME"].ToString();
                    var columnName = row["COLUMN_NAME"].ToString();

                    var fullName = TableInfo.CreateSqlTableName(tableSchema, tableName);
                    if (TableInfos.Contains(fullName))
                    {
                        var tableInfo = TableInfos[fullName];
                        if (tableInfo.ColumnInfos.Contains(columnName))
                            tableInfo.ColumnInfos[columnName].IsPrimary = true;
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
    }
}
