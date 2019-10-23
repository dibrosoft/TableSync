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

        public Workbook Open(string fileName, bool testExistence = true)
        {
            if (testExistence && !File.Exists(fileName))
                throw new FileNotFoundException(fileName);

            if (string.Compare(Path.GetExtension(fileName), ".xlsx", true) != 0)
                throw new IllegalFileExtensionException(fileName, ".xlsx");

            return new Workbook(connections, fileName);
        }

        public string Info(string connectionStringOrName, string tableName)
        {
            var sb = new StringBuilder();

            if (string.IsNullOrEmpty(connectionStringOrName))
            {
                sb.AppendLine("Available connections:");
                foreach (var connection in connections)
                    sb.AppendLine($"  {connection.ToString()}");
            }
            else
            {
                var connectionString = connections != null ? connections.GetConnectionString(connectionStringOrName) : connectionStringOrName;
                var databaseInfo = new DatabaseInfo(connectionString);

                if (string.IsNullOrEmpty(tableName))
                {
                    sb.AppendLine("Available tables:");
                    foreach (var tableinfo in databaseInfo.TableInfos)
                        sb.AppendLine($"  {tableinfo.ToString()}");
                }
                else
                {
                    TableInfo tableInfo;
                    if (databaseInfo.TableInfos.Contains(tableName))
                        tableInfo = databaseInfo.TableInfos[tableName];
                    else
                        tableInfo = databaseInfo.TableInfos.Where(item => string.Compare(item.TableName, tableName, true) == 0).SingleOrDefault();

                    if (tableInfo == null)
                        throw new MissingTableException(tableName);

                    sb.AppendLine("Available columns:");
                    foreach (var columnInfo in tableInfo.ColumnInfos)
                        sb.AppendLine($"  {columnInfo.ToString()}");
                }
            }

            return sb.ToString();
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
