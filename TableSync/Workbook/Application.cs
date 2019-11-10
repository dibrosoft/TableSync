using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TableSync
{
    public class Application
    {
        private readonly Connections connections;

        public Application(Connections connections = null)
        {
            this.connections = connections;
        }

        public Workbook CreateOrOpen(string fileName)
        {
            if (string.Compare(Path.GetExtension(fileName), ".xlsx", true) != 0)
                throw new IllegalFileExtensionException(fileName, ".xlsx");

            return new Workbook(connections, fileName);
        }

        public Workbook Open(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException(fileName);

            return new Workbook(connections, fileName);
        }

        public string Info(string connectionStringOrName, string tableName, string workbookFileName, bool jsonFormat)
        {
            if (jsonFormat)
                    return InfoAsJson(connectionStringOrName, tableName, workbookFileName);

            return InfoAsText(connectionStringOrName, tableName, workbookFileName);
        }

        private string InfoAsText(string connectionStringOrName, string tableName, string workbookFileName)
        {
            var result = new StringBuilder();

            if (!string.IsNullOrEmpty(workbookFileName))
            {
                using (var wb = Open(workbookFileName))
                {
                    var syncDefinition = wb.GetDefinition();
                    result.AppendLine($"Ranges of workbook: {workbookFileName}");
                    foreach (var range in syncDefinition.Ranges)
                    {
                        result.Append("  ");
                        result.AppendLine(range.Name);
                    }
                }
            }
            else if (string.IsNullOrEmpty(connectionStringOrName))
            {
                result.AppendLine("Connection names:");
                foreach (var connection in connections)
                {
                    result.Append("  ");
                    result.AppendLine(connection.Name);
                }
            }
            else
            {
                var connectionString = connections != null ? connections.GetConnectionString(connectionStringOrName) : connectionStringOrName;
                var databaseInfo = new DatabaseInfo(connectionString);

                if (string.IsNullOrEmpty(tableName))
                {
                    result.Append("Tables");
                    var connectionStringName = connections.GetProofedConnectionStringName(connectionStringOrName);
                    if (!string.IsNullOrEmpty(connectionStringName))
                        result.Append($" of connection {connectionStringName}");
                    result.AppendLine(":");

                    foreach (var tableInfo in databaseInfo.TableInfos)
                    {
                        result.Append("  ");
                        result.AppendLine(tableInfo.RangeTableName);
                    }
                }
                else
                {
                    var tableInfo = databaseInfo.SearchTableInfo(tableName);
                    if (tableInfo == null)
                        throw new MissingTableException(tableName);

                    result.AppendLine($"Columns of table {tableInfo.RangeTableName}");
                    foreach (var columnInfo in tableInfo.ColumnInfos)
                    {
                        result.Append("  ");
                        result.AppendLine(columnInfo.ColumnName);
                    }
                }
            }

            return result.ToString();
        }

        private string InfoAsJson(string connectionStringOrName, string tableName, string workbookFileName)
        {
            if (!string.IsNullOrEmpty(workbookFileName))
                using (var wb = Open(workbookFileName))
                    return MyJsonConvert.SerializeObject(wb.GetDefinition());

            if (string.IsNullOrEmpty(connectionStringOrName))
                return MyJsonConvert.SerializeObject(connections.Select(item => item.Name).ToList());

            var connectionString = connections != null ? connections.GetConnectionString(connectionStringOrName) : connectionStringOrName;
            var databaseInfo = new DatabaseInfo(connectionString);

            if (string.IsNullOrEmpty(tableName))
                return MyJsonConvert.SerializeObject(databaseInfo);

            var tableInfo = databaseInfo.SearchTableInfo(tableName);
            if (tableInfo == null)
                throw new MissingTableException(tableName);

            return MyJsonConvert.SerializeObject(tableInfo);
        }
        public static SyncDefinition GetDefinitionOrDefault(IEnumerable<string> tableNames, string definitionFileName)
        {
            if (tableNames.Count() > 0)
                return new SyncDefinition(tableNames);

            if (!string.IsNullOrEmpty(definitionFileName))
                return MyJsonConvert.DeserializeObjectFromFile<SyncDefinition>(definitionFileName);

            return null;
        }

        public static Settings GetSettingsOrDefault(string settingsFileName)
        {
            if (string.IsNullOrEmpty(settingsFileName))
                return null;

            if (!File.Exists(settingsFileName))
                throw new FileNotFoundException(settingsFileName);

            if (string.Compare(Path.GetExtension(settingsFileName), ".json", true) != 0)
                throw new IllegalFileExtensionException(settingsFileName, ".json");

            return MyJsonConvert.DeserializeObjectFromFile<Settings>(settingsFileName);
        }
    }
}
