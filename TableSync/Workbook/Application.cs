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
            if (string.IsNullOrEmpty(connectionStringOrName))
                return MyJsonConvert.SerializeObject(connections.Select(item => item.Name).ToList());

            var connectionString = connections != null ? connections.GetConnectionString(connectionStringOrName) : connectionStringOrName;
            var databaseInfo = new DatabaseInfo(connectionString);

            if (string.IsNullOrEmpty(tableName))
                return MyJsonConvert.SerializeObject(databaseInfo);
            else
            {
                TableInfo tableInfo;
                if (databaseInfo.TableInfos.Contains(tableName))
                    tableInfo = databaseInfo.TableInfos[tableName];
                else
                    tableInfo = databaseInfo.TableInfos.Where(item => string.Compare(item.TableName, tableName, true) == 0).SingleOrDefault();

                if (tableInfo == null)
                    throw new MissingTableException(tableName);

                return MyJsonConvert.SerializeObject(tableInfo);
            }
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
