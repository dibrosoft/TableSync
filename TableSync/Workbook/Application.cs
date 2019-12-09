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

        public string Info(string connectionStringOrName, IEnumerable<string> tableNames, string workbookFileName, bool jsonFormat)
        {
            if (jsonFormat)
                return InfoAsJson(connectionStringOrName, tableNames, workbookFileName);

            return InfoAsText(connectionStringOrName, tableNames, workbookFileName);
        }

        private string InfoAsText(string connectionStringOrName, IEnumerable<string> tableNames, string workbookFileName)
        {
            var result = new StringBuilder();

            if (!string.IsNullOrEmpty(workbookFileName))
            {
                using (var wb = Open(workbookFileName))
                {
                    var syncDefinition = wb.GetDefinition();
                    result.AppendLine($"Workbook {workbookFileName}:");
                    foreach (var range in syncDefinition.Ranges)
                    {
                        result.Append("  ");
                        result.AppendLine(range.Name);
                    }
                }
                return result.ToString();
            }

            if (connections != null && string.IsNullOrEmpty(connectionStringOrName))
            {
                result.AppendLine("Connections:");
                foreach (var connection in connections)
                {
                    result.Append("  ");
                    result.AppendLine(connection.Name);
                }
                result.AppendLine();
                result.AppendLine("Path of the connection config file:");
                result.Append("  ");
                result.AppendLine(ConnectionsProvider.GetDefaultConnectionsPath());
                return result.ToString();
            }

            if (!string.IsNullOrEmpty(connectionStringOrName))
            {
                var connectionInfo = connections.GetConnectionInfo(connectionStringOrName);
                var databaseInfo = new DatabaseInfo(connectionInfo);
                var tableInfos = databaseInfo.SearchTableInfos(tableNames);

                result.AppendLine("Tables:");
                result.AppendLine();

                foreach (var tableInfo in tableInfos)
                {
                    result.AppendLine(tableInfo.RangeTableName);
                    foreach (var columnInfo in tableInfo.ColumnInfos)
                    {
                        result.Append("  ");
                        result.AppendLine(columnInfo.ColumnName);
                    }

                    result.AppendLine();
                }
            }

            return result.ToString();
        }

        private string InfoAsJson(string connectionStringOrName, IEnumerable<string> tableNames, string workbookFileName)
        {
            if (!string.IsNullOrEmpty(workbookFileName))
                using (var wb = Open(workbookFileName))
                    return MyJsonConvert.SerializeObject(wb.GetDefinition());

            if (string.IsNullOrEmpty(connectionStringOrName))
                return MyJsonConvert.SerializeObject(connections.ToList());

            var connectionInfo = connections.GetConnectionInfo(connectionStringOrName);
            var databaseInfo = new DatabaseInfo(connectionInfo);
            var tableInfos = databaseInfo.SearchTableInfos(tableNames);

            return MyJsonConvert.SerializeObject(tableInfos);
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
